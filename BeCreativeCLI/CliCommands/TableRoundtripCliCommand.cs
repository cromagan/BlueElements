// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Globalization;
using System.IO;
using System.IO.Compression;
using BlueTable.ColumnFormats;

namespace BeCreativeCLI.CliCommands;

/// <summary>
/// Intern: Roundtrip-Tests für TableFileNew (.btb): anlegen, speichern, laden,
/// erneut speichern — die Bytes müssen identisch sein, ausgenommen des
/// Zip-Eintrag-Datums. Dazu Kopie über eine frische Instanz (bestimmter
/// Inhalt) und Ändern → Speichern → Laden → Wertprüfung.
/// </summary>
public class TableRoundtripCliCommand : CliCommand {

    #region Properties

    public override string Command => "table-roundtrip";

    public override List<string> Flags => ["dev", "keep"];

    public override bool Hidden => true;

    public override List<string> Options => ["rows"];

    public override string Syntax => "bcr table-roundtrip [zielordner] [--rows <anzahl>] [--keep]";

    public override string? HelpDetails =>
        "Legt eine Testtabelle im Temp-Ordner an und prüft: " +
        "Erneutes Speichern ohne Änderung lässt die Datenbestände unverändert (Zip-Eintrag-Datum normalisiert), " +
        "eine Kopie über eine frische Instanz liefert denselben Inhalt (Erzeuger-Stempel ausgenommen) und " +
        "Ändern → Speichern → Laden erhält die geänderten Werte. --keep behält den Temp-Ordner.";

    /// <summary>
    /// Feste Referenz für das Zip-Eintrag-Datum: Zwei Speicherungen desselben Inhalts
    /// unterscheiden sich ausschließlich in diesem Datum.
    /// </summary>
    private static DateTimeOffset FixedZipDate => new(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);

    #endregion

    #region Methods

    public override int DoIt(CliArgs args) {
        if (args.PositionalCount > 1) {
            return UsageError($"Erwartet wird höchstens 1 Positionsargument (<zielordner>), erhalten: {args.PositionalCount}.");
        }

        if (!int.TryParse(args.Option("rows"), out var rows) || rows is < 1 or > 100) { rows = 5; }

        var rootDir = args[0] ?? string.Empty;

        if (rootDir is { Length: > 0 } && !rootDir.IsValidFilepathAndName()) {
            try {
                rootDir = Path.GetFullPath(rootDir);
            } catch {
                Console.Error.WriteLine("Zielordner ungültig: " + rootDir);
                return 2;
            }
        }

        if (rootDir is not { Length: > 0 }) { rootDir = Path.GetTempPath(); }

        var testDir = Path.Combine(rootDir, "bcr_roundtrip_" + DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture));

        if (CreateDirectory(testDir).IsFailed) {
            Console.Error.WriteLine("Temp-Ordner konnte nicht erstellt werden: " + testDir);
            return 1;
        }

        var failures = new List<string>();
        var keep = args.Flag("keep");

        try {
            CheckLayout(failures, testDir, rows);
        } finally {
            if (keep) {
                Out(string.Empty);
                Out("Temp-Ordner behalten: " + testDir);
            } else {
                DeleteDir(testDir, false);
            }
        }

        Out(string.Empty);
        Out("=== Ergebnis ===");

        if (failures.Count == 0) {
            Out("*** ALLE TESTS ERFOLGREICH ***");
            return 0;
        }

        Out(failures.Count.ToString1() + " FEHLER:");
        foreach (var f in failures) {
            Out("  - " + f);
        }

        return 1;
    }

    /// <summary>
    /// Führt alle Prüfungen des Layouts aus.
    /// </summary>
    private static void CheckLayout(List<string> failures, string testDir, int rows) {
        Out(string.Empty);
        Out("=== Layout: " + nameof(SaveableFile) + " ===");

        var dir = Path.Combine(testDir, nameof(SaveableFile).ToLowerInvariant());
        _ = CreateDirectory(dir);

        var head = Path.Combine(dir, "rta.btb");
        var headCopy = Path.Combine(dir, "rtc.btb");

        // Phase 1: Anlegen, füllen, speichern.
        if (!CreateFilled(failures, head, rows)) { return; }

        var rawBefore = ReadDataRaw(failures, head, true);
        var inventoryBefore = InventoryOf(dir, "rta");

        // Phase 2: Laden, ohne Änderung erneut speichern — Bytes müssen gleich sein (bis auf Zip-Datum).
        CheckResaveIdentity(failures, head, rawBefore, inventoryBefore);

        // Phase 3: Kopie über eine frische Instanz — Inhalt muss bestimmt sein (Erzeuger-Stempel ausgenommen).
        CheckCopyInstance(failures, head, headCopy, rawBefore);

        // Phase 4: Ändern, speichern, erneut laden — Werte müssen erhalten sein.
        CheckModify(failures, head);
    }

    /// <summary>
    /// Legt eine Testtabelle an, füllt Spalten und Zeilen und speichert.
    /// </summary>
    private static bool CreateFilled(List<string> failures, string head, int rows) {
        var saveable = new SaveableFile(head);

        TableFileNew? tbl = null;

        try {
            tbl = new TableFileNew(head, null, saveable);
            saveable.WriteHeadFile();

            if (!FillTable(failures, tbl, rows)) { return false; }

            RowCollection.InvalidatedRowsManager.DoAllInvalidatedRows(null, true, null);

            var opr = tbl.Save();

            if (opr.IsFailed) {
                Fail(failures, "Erstes Speichern", opr.FailedReason ?? "Unbekannter Fehler");
                return false;
            }

            if (saveable.IsFreezed) {
                Fail(failures, "Erstes Speichern", "eingefroren: " + saveable.FrozenReason);
                return false;
            }

            Out("  [OK] Angelegt und gespeichert (" + rows.ToString1() + " Zeilen)");
            return true;
        } catch (Exception ex) {
            Fail(failures, "Anlegen", ex.Message);
            return false;
        } finally {
            tbl?.Dispose();

            if (tbl is null) { saveable.Dispose(); }
        }
    }

    /// <summary>
    /// Lädt eine bestehende .btb-Datei über eine frische Verwaltung (gleicher Pfad
    /// wie Table.CreateInstance).
    /// </summary>
    private static TableFileNew? LoadExisting(List<string> failures, string head) {
        var saveable = new SaveableFile(head);

        TableFileNew? tbl = null;

        try {
            tbl = new TableFileNew(head.FileNameWithoutSuffix(), saveable);
            tbl.LoadFromFile(head, string.Empty);

            if (saveable.IsFreezed) {
                Fail(failures, "Laden", "eingefroren: " + saveable.FrozenReason);
                tbl.Dispose();
                return null;
            }

            // Daten-Chunks nachladen: Datenbereiche mit Chunk-Wert stehen nur in
            // den Chunk-Dateien und werden nicht automatisch beim Start geladen.
            _ = tbl.LoadTableRows(false, -1);

            return tbl;
        } catch (Exception ex) {
            Fail(failures, "Laden", ex.Message);
            tbl?.Dispose();

            if (tbl is null) { saveable.Dispose(); }

            return null;
        }
    }

    /// <summary>
    /// Legt eine Testtabelle an, füllt Spalten und Zeilen und speichert. Eine
    /// Chunk-Spalte sorgt dafür, dass die Zeilen je Chunk-Wert auf Daten-Chunks
    /// verteilt werden.
    /// </summary>
    private static bool FillTable(List<string> failures,TableFileNew tbl, int rows) {
        var format = ColumnFormat.AllFormats.Instances.FirstOrDefault(f => f.KeyName.Equals("TextOneLine", StringComparison.OrdinalIgnoreCase));

        if (format is null) {
            Fail(failures, "Spalten anlegen", "Format 'TextOneLine' nicht gefunden.");
            return false;
        }

        var c1 = tbl.Column.GenerateAndAdd("name", "Name", format, string.Empty);
        var c2 = tbl.Column.GenerateAndAdd("menge", "Menge", format, string.Empty);
        var c3 = tbl.Column.GenerateAndAdd("notiz", "Notiz", format, string.Empty);

        if (c1 is null || c2 is null || c3 is null) {
            Fail(failures, "Spalten anlegen", "Spalten konnten nicht angelegt werden.");
            return false;
        }

        var chunkSpalte = tbl.Column.GenerateAndAdd("chunk", "Chunk", format, string.Empty);

        if (chunkSpalte is { IsDisposed: false } cvc) {
            cvc.Value_for_Chunk = ChunkType.ByName;
            tbl.Column.GetSystems();
        } else {
            Fail(failures, "Spalten anlegen", "Chunk-Spalte konnte nicht angelegt werden.");
            return false;
        }

        for (var i = 1; i <= rows; i++) {
            List<FilterItem> filters = [new(c1, FilterType.Istgleich, c1.AutoCorrect("wert" + i.ToString1(), true))];

            // Muss-Spalte: die Chunk-Spalte je Zeile auf drei Werte verteilen; der
            // Filterwert wird beim Anlegen als Initialwert der Zelle übernommen.
            if (chunkSpalte is { IsDisposed: false } cvcZeile) { filters.Add(new FilterItem(cvcZeile, FilterType.Istgleich, "cb" + (i % 3).ToString1())); }

            var opr = tbl.Row.GenerateAndAdd([.. filters], "bcr table-roundtrip");

            if (opr.IsFailed || opr.Value is not RowItem row) {
                Fail(failures, "Zeile anlegen", "Zeile " + i.ToString1() + ": " + opr.FailedReason);
                return false;
            }

            var e2 = row.CellSet(c2, (i * 10).ToString1(), "bcr table-roundtrip");
            var e3 = row.CellSet(c3, "ÄÖÜäöüß " + i.ToString1(), "bcr table-roundtrip");

            if (e2 is { Length: > 0 }) {
                Fail(failures, "Zelle setzen", "menge, Zeile " + i.ToString1() + ": " + e2);
                return false;
            }

            if (e3 is { Length: > 0 }) {
                Fail(failures, "Zelle setzen", "notiz, Zeile " + i.ToString1() + ": " + e3);
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Phase 2: Laden und ohne Änderung erneut speichern. Es darf nichts
    /// geschrieben werden (kein offener Writer, unveränderte Chunks); die
    /// bestehenden Dateien bleiben unverändert.
    /// </summary>
    private static void CheckResaveIdentity(List<string> failures, string head, Dictionary<string, byte[]> rawBefore, Dictionary<string, byte[]> inventoryBefore) {
        var tbl = LoadExisting(failures, head);

        if (tbl is not { Saveable: { } saveable }) { return; }

        try {
            saveable.MarkChanged();

            var opr = tbl.Save();

            if (opr.IsFailed) {
                Fail(failures, "Zweit-Speicherung", opr.FailedReason ?? "Unbekannter Fehler");
                return;
            }

            CheckResaveChunk(failures, head, rawBefore, inventoryBefore);
        } finally {
            tbl.Dispose();
        }
    }

    private static string BytesToHex(byte[] bytes) {
        var sb = new StringBuilder(bytes.Length * 3);

        foreach (var b in bytes) {
            if (sb.Length > 0) { sb.Append(' '); }
            sb.Append(b.ToString("X2", CultureInfo.InvariantCulture));
        }

        return sb.ToString();
    }

    /// <summary>
    /// Wandelt Bytes in Win1252-Text um und escaped Steuerzeichen, damit die
    /// Ausgabe verlustfrei kopierbar ist.
    /// </summary>
    private static string BytesToPrintable(byte[] bytes) {
        try {
            return Win1252.GetString(bytes).EncodeControlChars();
        } catch {
            return "<binär nicht darstellbar>";
        }
    }


    /// <summary>
    /// Gibt eine Diff-Analyse zweier Byte-Folgen aus: Längendifferenz, erste
    /// Abweichung mit Kontext (Text und Hex) und Anzahl unterschiedlicher Bytes.
    /// </summary>
    public static void GenerateDiffReport(byte[] original, byte[] roundtrip) {
        Out(string.Empty);
        Out("--- Diff-Analyse ---");
        Out("Längendifferenz (Roundtrip - Original): " + (roundtrip.Length - original.Length) + " Bytes");

        var minLen = Math.Min(original.Length, roundtrip.Length);
        var firstDiff = -1;

        for (var i = 0; i < minLen; i++) {
            if (original[i] != roundtrip[i]) {
                firstDiff = i;
                break;
            }
        }

        if (firstDiff < 0) {
            // Alle gemeinsamen Bytes sind identisch - der Unterschied liegt in der Länge
            if (original.Length < roundtrip.Length) {
                var extra = roundtrip[original.Length..];
                Out("Inhalt ist im gemeinsamen Bereich (0.." + (original.Length - 1) + ") identisch.");
                Out("Roundtrip hat zusätzlichen Inhalt ab Position " + original.Length + " (" + extra.Length + " Bytes):");
                Out(BytesToPrintable(extra));
            } else {
                var missing = original[roundtrip.Length..];
                Out("Inhalt ist im gemeinsamen Bereich (0.." + (roundtrip.Length - 1) + ") identisch.");
                Out("Roundtrip fehlt Inhalt ab Position " + roundtrip.Length + " (" + missing.Length + " Bytes):");
                Out(BytesToPrintable(missing));
            }
            return;
        }

        Out("Erste Abweichung an Byte-Position: " + firstDiff);

        // Kontext um die erste Abweichung: als Text (escaped) und als Hex-Dump
        var contextStart = Math.Max(0, firstDiff - 40);
        var contextEnd = Math.Min(minLen, firstDiff + 40);

        Out("Kontext Original (Bytes " + contextStart + ".." + (contextEnd - 1) + "):");
        Out("Text: " + BytesToPrintable(original[contextStart..contextEnd]));
        Out("Hex : " + BytesToHex(original[contextStart..contextEnd]));
        Out("Kontext Roundtrip:");
        Out("Text: " + BytesToPrintable(roundtrip[contextStart..contextEnd]));
        Out("Hex : " + BytesToHex(roundtrip[contextStart..contextEnd]));

        var diffCount = 0;

        for (var i = 0; i < minLen; i++) {
            if (original[i] != roundtrip[i]) { diffCount++; }
        }

        Out("Anzahl unterschiedlicher Bytes im gemeinsamen Bereich (0.." + (minLen - 1) + "): " + diffCount);
        Out("Größenunterschied: " + Math.Abs(original.Length - roundtrip.Length) + " Bytes");
    }
    /// <summary>
    /// Zweit-Speicherung: Chunks werden write-once nur bei echten Änderungen neu
    /// abgelegt; die jüngsten Nutzdaten müssen dem ersten Stand entsprechen.
    /// </summary>
    private static void CheckResaveChunk(List<string> failures, string head, Dictionary<string, byte[]> rawBefore, Dictionary<string, byte[]> inventoryBefore) {
        var rawAfter = ReadDataRaw(failures, head, true);

        foreach (var kvp in rawBefore) {
            if (!rawAfter.TryGetValue(kvp.Key, out var after)) {
                Fail(failures, "Zweit-Speicherung", "Chunk fehlt nach Zweit-Speicherung: " + kvp.Key);
                continue;
            }

            if (!WithoutHeadRecords(PayloadOf(kvp.Value)).SequenceEqual(WithoutHeadRecords(PayloadOf(after)))) {
                Fail(failures, "Zweit-Speicherung", "Chunk-Inhalt geändert: " + kvp.Key);
                GenerateDiffReport(WithoutHeadRecords(PayloadOf(kvp.Value)), WithoutHeadRecords(PayloadOf(after)));
            }
        }

        Out("  [OK] Chunk-Inhalte identisch (" + rawBefore.Count.ToString1() + " Chunks)");

        // Write-once: Alle ersten Dateien müssen noch vorhanden sein; neue
        // Zeitstempel-Dateien sind beim erneuten Schreiben legitim.
        CheckInventoryUnchanged(failures, "Zweit-Speicherung", inventoryBefore, InventoryOf(head.FilePath(), "rta"), [], true);
    }

    /// <summary>
    /// Phase 3: Kopie über eine frische Verwaltung — der serialisierte Inhalt muss
    /// bestimmt sein; nur der Erzeuger-Stempel (CreateUser/CreateTimeUtc) darf
    /// abweichen.
    /// </summary>
    private static void CheckCopyInstance(List<string> failures, string head, string headCopy, Dictionary<string, byte[]> rawBefore) {
        var source = LoadExisting(failures, head);

        if (source is null) { return; }

        var copySaveable = new SaveableFile(headCopy);

        TableFileNew? copy = null;

        try {
            copy = new TableFileNew(headCopy, source, copySaveable);
            copySaveable.WriteHeadFile();

            var opr = copy.Save();

            if (opr.IsFailed) {
                Fail(failures, "Kopie", opr.FailedReason ?? "Unbekannter Fehler");
                return;
            }

            var rawCopy = ReadDataRaw(failures, headCopy, true);

            if (rawBefore.Count != rawCopy.Count) {
                Fail(failures, "Kopie", "Datenbestand weicht ab: " + rawBefore.Count.ToString1() + " vs " + rawCopy.Count.ToString1() + " Bereiche.");
                return;
            }

            foreach (var kvp in rawBefore) {
                if (!rawCopy.TryGetValue(kvp.Key, out var copyRaw)) {
                    Fail(failures, "Kopie", "Bereich fehlt in der Kopie: " + kvp.Key);
                    continue;
                }

                var original = WithoutHeadRecords(PayloadOf(kvp.Value));
                var copyPayload = WithoutHeadRecords(PayloadOf(copyRaw));

                if (!original.SequenceEqual(copyPayload)) {
                    Fail(failures, "Kopie", "Bereich unterscheidet sich (System-Kopf ausgenommen): " + kvp.Key);
                    GenerateDiffReport(original, copyPayload);
                }
            }

            Out("  [OK] Kopie inhaltlich identisch (" + rawBefore.Count.ToString1() + " Bereiche)");
        } catch (Exception ex) {
            Fail(failures, "Kopie", ex.Message);
        } finally {
            copy?.Dispose();

            if (copy is null) { copySaveable.Dispose(); }

            source.Dispose();
        }
    }

    /// <summary>
    /// Suchfilter für die Zeile 'wert1': Bei Tabellen mit Chunk-Spalte ist der
    /// Chunk-Filter zwingend (CalculateFilteredRows lädt nur den gefilterten Chunk).
    /// Die Testzeilen tragen die Chunk-Werte cb0–cb2; wert1 gehört zu cb1.
    /// </summary>
    private static FilterItem[] ZeilenFilter(TableFileNew tbl, ColumnItem c1) {
        List<FilterItem> filters = [new(c1, FilterType.Istgleich, c1.AutoCorrect("wert1", true))];

        if (tbl.Column.ChunkValueColumn is { IsDisposed: false } cvc) { filters.Add(new(cvc, FilterType.Istgleich, "cb1")); }

        return [.. filters];
    }

    /// <summary>
    /// Phase 4: Zelle ändern, speichern, erneut laden — der neue Wert muss
    /// vorhanden sein, ein Nachbarwert unverändert.
    /// </summary>
    private static void CheckModify(List<string> failures, string head) {
        var tbl = LoadExisting(failures, head);

        if (tbl is null) { return; }

        var rowKey = string.Empty;
        var notizBefore = string.Empty;

        try {
            var c1 = tbl.Column["name"];
            var c2 = tbl.Column["menge"];
            var c3 = tbl.Column["notiz"];

            if (c1 is null || c2 is null || c3 is null) {
                Fail(failures, "Ändern", "Spalten nach Laden nicht gefunden.");
                return;
            }

            var row = tbl.Row[ZeilenFilter(tbl, c1)];

            if (row is not { IsDisposed: false }) {
                Fail(failures, "Ändern", "Zeile 'wert1' nicht gefunden.");
                return;
            }

            rowKey = row.KeyName;
            notizBefore = row.CellGetStringCore(c3);

            var failed = row.CellSet(c2, "4711", "bcr table-roundtrip");

            if (failed is { Length: > 0 }) {
                Fail(failures, "Ändern", "menge: " + failed);
                return;
            }

            RowCollection.InvalidatedRowsManager.DoAllInvalidatedRows(null, true, null);

            var opr = tbl.Save();

            if (opr.IsFailed) {
                Fail(failures, "Ändern/Speichern", opr.FailedReason ?? "Unbekannter Fehler");
                return;
            }

            Out("  [OK] Änderung gespeichert");
        } catch (Exception ex) {
            Fail(failures, "Ändern", ex.Message);
        } finally {
            tbl.Dispose();
        }

        if (rowKey is not { Length: > 0 }) { return; }

        VerifyModifyReload(failures, head, rowKey, notizBefore);
    }

    /// <summary>
    /// Lädt die Tabelle nach der Änderung erneut und prüft die Werte.
    /// </summary>
    private static void VerifyModifyReload(List<string> failures, string head, string rowKey, string notizBefore) {
        var tbl = LoadExisting(failures, head);

        if (tbl is null) { return; }

        try {
            var c1 = tbl.Column["name"];
            var c2 = tbl.Column["menge"];
            var c3 = tbl.Column["notiz"];

            if (c1 is null || c2 is null || c3 is null) {
                Fail(failures, "Ändern/Nachladen", "Spalten nach Nachladen nicht gefunden.");
                return;
            }

            var row = tbl.Row[ZeilenFilter(tbl, c1)];

            if (row is not { IsDisposed: false }) {
                Fail(failures, "Ändern/Nachladen", "Zeile nach Nachladen nicht gefunden.");
                return;
            }

            if (!string.Equals(row.KeyName, rowKey, StringComparison.Ordinal)) {
                Fail(failures, "Ändern/Nachladen", "Zeilen-Identität geändert: " + row.KeyName + " (vorher " + rowKey + ")");
            }

            var mengeExpected = c2.AutoCorrect("4711", true);
            var mengeStored = row.CellGetStringCore(c2);

            if (!string.Equals(mengeStored, mengeExpected, StringComparison.Ordinal)) {
                Fail(failures, "Ändern/Nachladen", "menge='" + mengeStored + "', erwartet '" + mengeExpected + "'");
            } else {
                Out("  [OK] Änderung nach Nachladen vorhanden");
            }

            var notizStored = row.CellGetStringCore(c3);

            if (!string.Equals(notizStored, notizBefore, StringComparison.Ordinal)) {
                Fail(failures, "Ändern/Nachladen", "notiz geändert ohne Schreibzugriff: '" + notizStored + "' (vorher '" + notizBefore + "')");
            } else {
                Out("  [OK] Nachbarzelle unverändert");
            }
        } finally {
            tbl.Dispose();
        }
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Liest die Roh-Bytes der Datendateien: der jüngste Kopf-Snapshot plus die
    /// jüngste Datei je Chunk-Ordner. Key ist der Bereichsname ('snapshot', Chunk-Id).
    /// </summary>
    private static Dictionary<string, byte[]> ReadDataRaw(List<string> failures, string head, bool ignoreMaster) {
        var result = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
        var folder = head.FilePath() + head.FileNameWithoutSuffix() + "\\";

        var snapshot = NewestFileOf(failures, folder, "*.tbls");
        if (snapshot is not null) {
            var snapBytes = ReadRaw(failures, snapshot);
            if (snapBytes is { Length: > 0 }) { result.Add("snapshot", snapBytes); }
        }

        if (DirectoryExists(folder)) {
            foreach (var sub in GetDirectories(folder)) {
                var chunkId = sub.TrimEnd('\\').FileNameWithSuffix().ToLowerInvariant();

                if (ignoreMaster && string.Equals(chunkId, TableChunk.Chunk_Master, StringComparison.OrdinalIgnoreCase)) { continue; }

                var chunkFile = NewestFileOf(failures, sub, "*.tblc");
                if (chunkFile is null) { continue; }

                var chunkBytes = ReadRaw(failures, chunkFile);
                if (chunkBytes is { Length: > 0 }) { result.Add(chunkId, chunkBytes); }
            }
        }

        return result;
    }

    /// <summary>
    /// Liest eine Datei als Roh-Bytes.
    /// </summary>
    private static byte[]? ReadRaw(List<string> failures, string file) {
        if (ReadAllBytes(file, 5).Value is not byte[] raw) { return null; }

        if (raw is not { Length: > 0 }) {
            Fail(failures, "Lesen", "Datei nicht lesbar: " + file);
            return null;
        }

        return raw;
    }

    /// <summary>
    /// Liefert den Nutzdaten-Inhalt: Zip-Dateien werden entpackt, alles andere
    /// unverändert geliefert.
    /// </summary>
    private static byte[] PayloadOf(byte[] raw) {
        if (!raw.IsZipped()) { return raw; }

        return raw.UnzipIt() ?? [];
    }

    /// <summary>
    /// Liefert die jüngste Datei eines Ordners zum Suchmuster. Die Dateinamen
    /// tragen vorne einen UTC-Zeitstempel, deshalb entspricht die lexikografische
    /// Ordnung der zeitlichen.
    /// </summary>
    private static string? NewestFileOf(List<string> failures, string folder, string searchPattern) {
        if (!DirectoryExists(folder)) {
            Fail(failures, "Lesen", "Ordner fehlt: " + folder);
            return null;
        }

        var files = GetFiles(folder, searchPattern, SearchOption.TopDirectoryOnly);

        return files.Length == 0 ? null : files.OrderBy(f => f).Last();
    }

    /// <summary>
    /// Datei-Bestand einer Tabelle: Kopf-Datei plus aller Daten-Ordner-Inhalte,
    /// Bytes mit normalisiertem Zip-Datum.
    /// </summary>
    private static Dictionary<string, byte[]> InventoryOf(string dir, string key) {
        var result = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
        var head = Path.Combine(dir, key + ".btb");

        if (FileExists(head) && ReadAllBytes(head, 5).Value is byte[] headBytes and { Length: > 0 }) {
            result.Add("<head>", NormalizeZipDate(headBytes) ?? []);
        }

        var folder = Path.Combine(dir, key);

        if (DirectoryExists(folder)) {
            foreach (var f in GetFiles(folder, "*", SearchOption.AllDirectories)) {
                var rel = f[(folder.Length + 1)..].ToLowerInvariant();
                var raw = ReadAllBytes(f, 5).Value;
                result.Add(rel, raw is byte[] b and { Length: > 0 } ? NormalizeZipDate(b) ?? [] : []);
            }
        }

        return result;
    }

    /// <summary>
    /// Vergleicht zwei Datei-Bestände: fehlende, zusätzliche und veränderte
    /// Dateien sind Fehler; zusätzlich erlaubte neue Dateien oder komplett neue
    /// Dateien (Write-once-Layouts) werden ignoriert.
    /// </summary>
    private static void CheckInventoryUnchanged(List<string> failures, string checkName, Dictionary<string, byte[]> expected, Dictionary<string, byte[]> actual, HashSet<string> allowedExtra, bool allowAnyNewFiles) {
        var problems = 0;

        foreach (var kvp in expected) {
            if (!actual.TryGetValue(kvp.Key, out var bytes)) {
                Fail(failures, checkName, "Datei verschwunden: " + kvp.Key);
                problems++;
                continue;
            }

            if (!kvp.Value.SequenceEqual(bytes)) {
                Fail(failures, checkName, "Datei verändert: " + kvp.Key);
                problems++;
            }
        }

        if (!allowAnyNewFiles) {
            foreach (var key in actual.Keys) {
                if (!expected.ContainsKey(key) && !allowedExtra.Contains(key)) {
                    Fail(failures, checkName, "Unerwartete neue Datei: " + key);
                    problems++;
                }
            }
        }

        if (problems == 0) {
            Out("  [OK] Datei-Bestand unverändert (" + expected.Count.ToString1() + " Dateien)");
        }
    }

    /// <summary>
    /// Entfernt den System-Kopf (Erzeuger- und Speicher-Sätze) vom Anfang
    /// eines serialisierten Inhalts: Diese Sätze tragen den Zustand des jeweiligen
    /// Speichervorgangs (CreateUser/CreateTimeUtc, SaveUser/Machine/SaveTimeUtc)
    /// und sind wie das Zip-Datum nicht inhaltsbestimmend.
    /// </summary>
    private static byte[] WithoutHeadRecords(byte[] payload) {
        using var ms = new MemoryStream(payload);
        var start = ms.Position;

        while (FragmentRecord.TryReadNext(ms) is { } record) {
            if (!record.IsFileTag) { break; }

            start = ms.Position;
        }

        return payload[(int)start..];
    }

    /// <summary>
    /// Setzt das Datum aller Zip-Einträge auf einen festen Wert, damit zwei
    /// Speicherungen desselben Inhalts byte-identisch verglichen werden können.
    /// </summary>
    private static byte[]? NormalizeZipDate(byte[]? zipped) {
        if (zipped is not { Length: > 0 }) { return null; }
        if (!zipped.IsZipped()) { return zipped; }

        using var ms = new MemoryStream();

        try {
            ms.Write(zipped, 0, zipped.Length);
            ms.Position = 0;

            using var zip = new ZipArchive(ms, ZipArchiveMode.Update, true);
            var entry = zip.GetEntry("Main.bin");

            entry?.LastWriteTime = FixedZipDate;
        } catch {
            return null;
        }

        return ms.ToArray();
    }

    private static void Fail(List<string> failures, string check, string detail) {
        failures.Add(check + ": " + detail);
        Out("  [FAIL] " + check + " — " + detail);
    }

    #endregion
}

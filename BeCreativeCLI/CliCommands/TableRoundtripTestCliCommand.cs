// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Globalization;
using System.IO;
using static BeCreativeCLI.CliCommands.TableFbdbTestCliCommand;

namespace BeCreativeCLI.CliCommands;

/// <summary>
/// Intern: Bytegenauer Roundtrip-Test der TableFile-Formate (.bdb klassisch, .nbdb binär).
/// Speichert die Quelle mehrfach per "Speichern unter" abwechselnd als .bdb und .nbdb,
/// prüft nach jedem Hop den Datenstand gegen die Quelle und vergleicht am Ende
/// alle Stände je Format bytegenau (gezippt und entpackt) mit dem ersten Stand.
/// </summary>
public class TableRoundtripTestCliCommand : CliCommand {

    #region Properties

    public override string Command => "table-roundtriptest";

    public override List<string> Flags => ["keep", "dev"];

    public override bool Hidden => true;

    public override List<string> Options => ["dir"];

    public override string Syntax => "bcr table-roundtriptest <tabelle> [--dir <ordner>] [--keep]";

    public override string? HelpDetails =>
        "Führt den Byte-Roundtrip-Test (.bdb <-> .nbdb) im Arbeitsordner aus (Standard: Temp). " +
        "Exit 0 bei allen Tests; der Ordner wird bei Erfolg gelöscht, --keep erhält ihn zur Untersuchung.";

    #endregion

    #region Methods

    public override int DoIt(CliArgs args) {
        if (args.PositionalCount != 1) {
            return UsageError($"Erwartet wird genau 1 Positionsargument (<tabelle>), erhalten: {args.PositionalCount}.");
        }

        var quelle = args[0] ?? string.Empty;

        if (!quelle.IsValidFilepathAndName()) {
            try {
                quelle = Path.GetFullPath(quelle);
            } catch {
                Console.Error.WriteLine("Tabelle nicht gefunden: " + args[0]);
                return 1;
            }
        }

        if (!FileExists(quelle)) {
            Console.Error.WriteLine("Tabelle nicht gefunden: " + quelle);
            return 1;
        }

        var workdir = (args.Option("dir") ?? Path.GetTempPath() + "roundtriptest_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture) + "\\").NormalizeFile();

        if (Directory.Exists(workdir)) {
            Console.Error.WriteLine("Arbeitsordner existiert bereits: " + workdir);
            return 1;
        }

        _ = CreateDirectory(workdir);

        var srcFile = workdir + "Quelle." + quelle.FileSuffix().ToLowerInvariant();

        if (!FileCopy(quelle, srcFile, false)) {
            Console.Error.WriteLine("Quelltabelle konnte nicht kopiert werden nach: " + srcFile);
            return 1;
        }

        var failures = new List<string>();
        var oldUserGroup = UserGroup;
        UserGroup = Administrator; // Tabellen-Administratorrechte für den Test
        var oldDiagFlag = DiagFlag;
        DiagFlag = true; // Diagnose-Ausgaben der Load-Pfade aktivieren

        try {
            RunAllTests(workdir, srcFile, failures);

            Out(string.Empty);

            if (failures.Count == 0) {
                Out("*** ALLE ROUNDTRIP-TESTS ERFOLGREICH ***");

                if (!args.Flag("keep")) {
                    ReleaseAll();

                    try {
                        Directory.Delete(workdir, true);
                        Out("Arbeitsordner gelöscht: " + workdir);
                    } catch (Exception ex) {
                        Out("Arbeitsordner konnte nicht gelöscht werden (" + ex.Message + "): " + workdir);
                    }

                    return 0;
                }

                Out("Arbeitsordner behalten (--keep): " + workdir);
            } else {
                Out($"*** {failures.Count} FEHLER ***");
                foreach (var f in failures) { Out("FEHLER: " + f); }
                Out("Arbeitsordner zur Untersuchung behalten: " + workdir);
            }
        } finally {
            ReleaseAll();
            DiagFlag = oldDiagFlag;
            UserGroup = oldUserGroup;
        }

        return failures.Count == 0 ? 0 : 1;
    }

    #endregion

    #region Tests

    /// <summary>
    /// Führt alle Teststufen aus und sammelt Fehlermeldungen.
    /// </summary>
    private static void RunAllTests(string workdir, string srcFile, List<string> failures) {
        const int hops = 6;

        #region T1: Baseline aus der Quelle

        Out("=== T1: Quelle laden und Vergleichsstand erzeugen ===");

        var tblSrc = Load(srcFile);

        if (tblSrc is null) {
            failures.Add("T1: Quelltabelle konnte nicht geladen werden: " + srcFile);
            return;
        }

        var baseline = SnapshotOf(tblSrc);
        ReleaseAll();

        #endregion

        #region T2: Kette (Speichern unter, abwechselnd bdb/nbdb)

        Out(string.Empty);
        Out("=== T2: Konvertierungskette (Speichern unter, abwechselnd .bdb/.nbdb) ===");

        List<string> files = [];
        var chain = srcFile;

        for (var i = 0; i <= hops; i++) {
            var suffix = i % 2 == 0 ? "bdb" : "nbdb";
            var target = workdir + "Round" + i.ToString1() + "." + suffix;

            // Vor dem Öffnen Rohinhalt merken: Ein reines Laden darf die Datei nicht ändern.
            byte[]? before = null;

            if (i > 0) { before = RawContentOf(chain); }

            var tblHop = Load(chain);

            if (tblHop is null) {
                failures.Add("T2: Kettentabelle konnte nicht geladen werden: " + chain);
                return;
            }

            Out("  Vor Hop " + i.ToString1() + ": " + SaveOrderPreview(tblHop));

            var opr = TableFile.Create(target, tblHop);
            ReleaseAll();

            if (before is not null) {
                var after = RawContentOf(chain);
                var d0 = after is null ? "Datei nach dem Öffnen nicht mehr lesbar" : DiffOf(before, after);

                if (!Check(failures, d0 is null, $"T2 Hop {i}: Öffnen ändert die Datei nicht: {chain.FileNameWithSuffix()}")) {
                    Out("  Öffnen-Diff: " + d0);
                }
            }

            if (!Check(failures, opr.IsSuccessful, $"T2 Hop {i}: {chain.FileNameWithSuffix()} -> {target.FileNameWithSuffix()}" + (opr.IsSuccessful ? string.Empty : " — " + opr.FailedReason))) { return; }

            if (i == 1) {
                Out("  Arrangements-Dump von " + target.FileNameWithSuffix() + ":");
                DumpArrangementsRegion(RawContentOf(target) ?? []);
            }

            var tblCheck = Load(target);

            if (tblCheck is null) {
                failures.Add("T2: Zieldatei konnte nicht geladen werden: " + target);
                return;
            }

            CompareSnapshots(failures, $"T2 Hop {i}: Daten identisch zur Quelle", baseline, SnapshotOf(tblCheck));
            Out("  " + SaveOrderPreview(tblCheck));
            ReleaseAll();

            files.Add(target);
            chain = target;
        }

        #endregion

        #region T3: Bytegenaue Vergleiche erster Stand vs. folgende Stände

        Out(string.Empty);
        Out("=== T3: Bytegenauer Vergleich erster Stand vs. letzte Stände ===");

        List<string> bdbFiles = [];
        List<string> nbdbFiles = [];

        foreach (var f in files) {
            if (f.FileSuffix().Equals("bdb", StringComparison.OrdinalIgnoreCase)) {
                bdbFiles.Add(f);
            } else {
                nbdbFiles.Add(f);
            }
        }

        CheckByteIdentical(failures, "T3 .bdb", bdbFiles);
        CheckByteIdentical(failures, "T3 .nbdb", nbdbFiles);

        #endregion
    }

    /// <summary>
    /// Vergleicht jede Datei der Kette mit dem ersten Stand: Datei-Bytes bytegenau,
    /// dazu der entpackte Inhalt (trennt ZIP-Header- von Daten-Problemen).
    /// </summary>
    private static void CheckByteIdentical(List<string> failures, string caption, List<string> files) {
        if (files.Count < 2) {
            _ = Check(failures, false, caption + ": Kette zu kurz (weniger als 2 Stände)");
            return;
        }

        var reference = files[0];
        var referenceBytes = ReadAllBytes(reference, 10).Value as byte[];
        var referenceRaw = RawContentOf(reference);

        if (referenceBytes is not { Length: > 0 } refBytes || referenceRaw is not { } refRaw) {
            _ = Check(failures, false, caption + ": Referenzstand lesbar: " + reference.FileNameWithSuffix());
            return;
        }

        for (var i = 1; i < files.Count; i++) {
            var f = files[i];
            var bytes = ReadAllBytes(f, 10).Value as byte[];
            var raw = RawContentOf(f);

            if (bytes is not { Length: > 0 } cmpBytes || raw is not { } cmpRaw) {
                _ = Check(failures, false, caption + ": Stand lesbar: " + f.FileNameWithSuffix());
                continue;
            }

            if (!Check(failures, cmpBytes.SequenceEqual(refBytes), caption + " — Datei bytegenau identisch: " + f.FileNameWithSuffix() + $" ({cmpBytes.Length} Bytes)")) {
                Out("  Datei-Bytes unterscheiden sich: " + f.FileNameWithSuffix());
            }

            var d = DiffOf(refRaw, cmpRaw);

            if (!Check(failures, d is null, caption + " — entpackter Inhalt identisch: " + f.FileNameWithSuffix())) { Out("  Rohinhalt-Diff: " + d); }
        }

        Out($"  {caption}: {files.Count} Stände geprüft, Referenz {reference.FileNameWithSuffix()} ({refBytes.Length} Bytes)");
    }

    /// <summary>
    /// Liefert den entpackten Datei-Inhalt (null, wenn nicht lesbar oder nicht entpackbar).
    /// </summary>
    private static byte[]? RawContentOf(string file) {
        if (ReadAllBytes(file, 10).Value is not byte[] bytes) { return null; }
        return bytes.IsZipped() ? bytes.UnzipIt() : bytes;
    }

    /// <summary>
    /// Diagnose: Dumpiert die Satzstruktur der Arrangements-Sektion einer binären
    /// Tabellendatei (Tiefe, Typ-Bytes, Content-Anfang) zur Format-Fehlersuche.
    /// </summary>
    private static void DumpArrangementsRegion(byte[] data) {
        var pos = 0;
        var inRegion = false;

        while (pos < data.Length) {
            var start = pos;
            var tabs = 0;

            while (pos < data.Length && data[pos] == 9) { tabs++; pos++; }

            if (pos >= data.Length) { break; }

            pos++; // SpeicherTyp

            if (pos + 4 > data.Length) { break; }

            var len1 = BitConverter.ToInt32(data, pos);
            pos += 4;

            if (len1 < 1 || pos + len1 > data.Length) {
                Out($"  Dump: ungültiger Satz bei {start} (len1={len1})");
                break;
            }

            var leafStart = pos;
            var leafType = data[leafStart];
            var text = string.Empty;

            if (len1 >= 7 && data[leafStart] == 0x20) {
                var len2 = BitConverter.ToInt32(data, leafStart + 3);
                var contentStart = leafStart + 7;

                if (len2 >= 0 && contentStart + len2 <= pos + len1) {
                    text = Encoding.UTF8.GetString(data, contentStart, Math.Min(len2, 24)).EncodeControlChars();
                }
            }

            var leafInfo = tabs <= 2 ? $" leaf=0x{leafType:X2}" : string.Empty;

            if (tabs == 1 && leafType == 0x5F) { inRegion = true; }
            else if (tabs == 1 && leafType == 0x5E) {
                Out("  Dump-Ende (VariableCollection erreicht)");
                return;
            }

            if (inRegion && tabs <= 2) {
                Out($"  Dump t{tabs} @{start} len1={len1}{leafInfo}{(text.Length > 0 ? " [" + text + "]" : string.Empty)}");
            }

            pos = start + tabs + 1 + 4 + len1;
        }
    }

    /// <summary>
    /// Diagnose: Anzahl Ansichten, Name der Ansicht 1 und die ersten Spalten ihrer Liste.
    /// </summary>
    private static string SaveOrderPreview(Table tbl) {
        var arr = tbl.ColumnArrangements;
        var sb = new StringBuilder();
        _ = sb.Append("Ansichten=").Append(arr.Count);

        foreach (var v in arr) {
            if (v is null) { _ = sb.Append(" <null>"); continue; }

            var piText = v.ParseableItems().FinishParseable();
            var columnCount = piText.Split(["Column="], StringSplitOptions.None).Length - 1;
            _ = sb.Append(" [").Append(v.KeyName).Append(v.IsDisposed ? "/DISP" : string.Empty).Append(":PI-Col=").Append(columnCount).Append(']');
        }

        if (arr.Count > 1) {
            List<string> names = [];

            foreach (var c in arr[1].ListOfUsedColumn()) {
                if (c is { IsDisposed: false }) { names.Add(c.KeyName); }
            }

            _ = sb.Append(" Ansicht1-Liste: ").Append(string.Join(",", names.Take(12)));
        }

        // Rechte-Diagnose der Spalte BEISPIEL
        if (tbl.Column["BEISPIEL"] is { IsDisposed: false } bsp) {
            _ = sb.Append(" | BEISPIEL-PG: ").Append(string.Join(",", bsp.PermissionGroupsChangeCell));
        }

        return sb.ToString();
    }

    #endregion
}

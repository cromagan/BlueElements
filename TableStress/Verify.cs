// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;

namespace TableStress;

/// <summary>
/// Prüf-Lauf für eine Tabelle im Fragment-Format: Verzeichnis-, Chunk-, Undo-,
/// Speicher-/Lade-Rundtrip- und Dump-Prüfung mit Bericht als Datei im Ordner der Tabelle.
/// ExitCodes: 0 = keine FEHLER, 1 = FEHLER gefunden, 2 = falsche Argumente, 3 = abgebrochen.
/// </summary>
public static class Verify {

    #region Fields

    private static readonly UTF8Encoding Utf8Bom = new(true);

    private static StreamWriter? _report;

    private static int _fehler;

    private static int _warnungen;

    private static int _meldungCount;

    /// <summary>
    /// Suffixe der Kopf-Snapshots: neues Format (.snp) und Legacy (.tbls).
    /// </summary>
    private static readonly string[] SnapshotSuffixe = ["snp", "tbls"];

    /// <summary>
    /// Suffixe der Kopf-Journale: neues Format (.frg) und Legacy (.tblf).
    /// </summary>
    private static readonly string[] JournalSuffixe = ["frg", "tblf"];

    /// <summary>
    /// Suffixe der Chunk-Dateien: neues Format (.chk) und Legacy (.tblc).
    /// </summary>
    private static readonly string[] ChunkSuffixe = ["chk", "tblc"];

    #endregion

    #region Methods

    private static void DumpSchreiben(Table tbl, string dumpPath) {
        var columns = tbl.Column.Where(c => c is { IsDisposed: false }).ToList();

        using var w = new StreamWriter(dumpPath, false, Utf8Bom) { AutoFlush = true };
        w.WriteLine($"#TABELLE|{tbl.KeyName}");
        w.WriteLine($"#ZEIT|{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
        w.WriteLine($"#ZEILEN|{tbl.Row.Count}");

        foreach (var row in tbl.Row) {
            foreach (var c in columns) {
                var v = row.CellGetString(c).Replace('|', '/');
                w.WriteLine($"{row.KeyName}|{row.ChunkValue.Replace('|', '/')}|{c.KeyName}|{v}");
            }
        }

        Write("DUMP", dumpPath);
    }

    private static void Fehler(string detail) {
        _fehler++;
        Write("FEHLER", detail);
    }

    /// <summary>
    /// Fingerabdruck eines Meta-Satzes; Basis des Vergleichs Speicher ↔ Festplatte.
    /// </summary>
    private static string Fingerprint(FragmentRecord r) =>
        $"{r.DateTimeUtc.Ticks}|{r.User}|{r.Comment}|{r.ValueId}|{Convert.ToBase64String(r.ByteContent)}";

    private static List<string> KnownChunkIds(TableFileNew tfn) => tfn.Saveable.KnownChunkIds();

    private static List<FragmentRecord> MetaSaetzeVonDatei(string file) {
        List<FragmentRecord> result = [];

        if (IO.ReadAllBytes(file, 5).Value is not byte[] raw || raw.Length == 0) { return result; }

        var content = raw.IsZipped() ? raw.UnzipIt() : raw;
        if (content is not { Length: > 0 }) { return result; }

        using var ms = new MemoryStream(content);

        while (FragmentRecord.TryReadNext(ms) is { } r) {
            if (r.HasMeta) { result.Add(r); }
        }

        return result;
    }

    /// <summary>
    /// Develop-Meldungen ab Warning mit in den Bericht aufnehmen.
    /// </summary>
    private static void OnDevelopMessage(ErrorType type, object? reference, string category, ImageCode symbol, string message, int indent) {
        _ = reference;
        _ = symbol;
        _ = indent;
        if (type > ErrorType.Warning) { return; }

        _meldungCount++;
        Write(type == ErrorType.Error ? "FEHLER" : "WARNUNG", $"MELDUNG {category}: {OneLine(message)}");
        if (type == ErrorType.Error) { _fehler++; } else { _warnungen++; }
    }

    private static string OneLine(string text) => text.Replace("\r\n", " / ").Replace('\r', ' ').Replace('\n', ' ');

    private static DateTime? ParseChunkFileDate(string filePath) {
        var n = filePath.FileNameWithoutSuffix();
        if (n.Length < 23) { return null; }

        return DateTime.TryParseExact(n[..23], "yyyy-MM-dd-HH-mm-ss-fff", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)
            ? DateTime.SpecifyKind(d, DateTimeKind.Utc)
            : null;
    }

    /// <summary>
    /// Prüft die Änderungs-Historie gegen die Festplatte: Jeder Eintrag muss im
    /// Kopf-Snapshot oder -Journal vorliegen — im Legacy-Layout alternativ in den
    /// Chunk-Dateien des Ziel-Chunks.
    /// </summary>
    private static void PruefeUndos(Table tbl, string phase) {
        if (tbl is not TableFileNew tfn) { return; }

        var saveable = tfn.Saveable;

        var datei = tfn.Filename;
        var basis = datei.FilePath() + datei.FileNameWithoutSuffix() + "\\";

        var entries = UndoEintraege(saveable.Undos);
        var disk = UndoFingerprintsVonDisk(basis);

        // Die Historie kann im Kopf (Kopf-Snapshot/Journal) oder — Legacy-Layout —
        // je Chunk-Ordner liegen; der Eintrag muss irgendwo auf der Platte sein.
        var alle = new HashSet<string>(StringComparer.Ordinal);
        foreach (var set in disk.Values) { alle.UnionWith(set); }

        var ok = 0;
        var fehlt = 0;
        var gesamtSaetze = saveable.Undos.Count;

        foreach (var entry in entries) {
            var meta = entry.LastOrDefault(r => r.HasMeta);
            if (meta is null) { continue; }

            var fp = Fingerprint(meta);

            if (alle.Contains(fp)) {
                ok++;
            } else {
                fehlt++;
                if (fehlt <= 10) { Write("UNDO-FEHLT", $"Phase={phase} Ziel=(Kopf) User={meta.User} Zeit={meta.DateTimeUtc:HH:mm:ss.fff} Kommentar={meta.Comment}"); }
            }
        }

        if (fehlt > 10) { Fehler($"Phase={phase}: weitere {fehlt - 10} Undo-Einträge fehlen auf der Festplatte (Details unterdrückt)"); }
        if (fehlt > 0) { Fehler($"Phase={phase}: {fehlt} Undo-Einträge fehlen auf der Festplatte"); }

        Write("UNDO", $"Phase={phase} Einträge={entries.Count} Sätze={gesamtSaetze} ImKopf={ok} Fehlt={fehlt}");
    }

    /// <summary>
    /// Prüft Kopf-Datei, Arbeitsverzeichnis, Ordner-Namen und alle Fragment-Dateien
    /// auf gültige Namen, Zeitstempel und sauberen Abschluss (EOF-Marker).
    /// </summary>
    private static void PruefeVerzeichnis(Table tbl) {
        if (tbl is not TableFileNew tfn) { Warnung("Kein TableFileNew — Verzeichnis-Prüfung übersprungen"); return; }

        var datei = tfn.Filename;
        if (datei is not { Length: > 0 }) { Fehler("Kein Dateiname"); return; }

        if (!IO.FileExists(datei)) {
            Fehler("Kopf-Datei fehlt: " + datei);
        } else if (!SaveableFile.TryReadHeadClassId(datei, out var layout)) {
            Fehler("Kopf-Datei ohne gültigen Layout-Kopf: " + datei);
        } else {
            Write("LAYOUT", layout);
        }

        var basis = datei.FilePath() + datei.FileNameWithoutSuffix() + "\\";
        if (!Directory.Exists(basis)) { Fehler("Arbeitsverzeichnis fehlt: " + basis); return; }

        var systemOrdner = 0;
        var datenOrdner = 0;
        var dateiGesamt = 0;
        List<string> leereDatenOrdner = [];

        foreach (var dir in Directory.GetDirectories(basis)) {            var name = dir.TrimEnd(Path.DirectorySeparatorChar).FileNameWithSuffix();
            var files = Directory.GetFiles(dir, "*", SearchOption.TopDirectoryOnly);

            if (name.Length > 0 && name[0] == '_') {
                systemOrdner++;
            } else {
                datenOrdner++;

                if (!name.Equals(name.ToLowerInvariant(), StringComparison.Ordinal)) { Fehler($"Chunk-Ordner nicht lowercase: {name}"); }
                if (name.Length > 12) { Fehler($"Chunk-Ordner länger als 12 Zeichen: {name}"); }
            }

            // Nur Daten-Ordner und der Haupt-Chunk müssen Inhalt tragen; andere
            // System-Ordner (z. B. _master vor dem ersten Master) dürfen leer sein.
            // Leere Daten-Ordner entstehen, wenn ein Chunk-Wert nur angesprochen
            // wurde — sie räumt DeleteOrphanedChunkFolders später weg.
            if (files.Length == 0 && name.Equals("_maindata", StringComparison.OrdinalIgnoreCase)) {
                Fehler($"Haupt-Chunk-Ordner leer: {dir}");
            } else if (files.Length == 0 && name[0] != '_') {
                leereDatenOrdner.Add(name);
            }

            foreach (var f in files) {
                dateiGesamt++;

                if (ParseChunkFileDate(f) is null) { Fehler($"Dateiname ohne gültigen Zeitstempel: {f}"); }

                if (IO.ReadAllBytes(f, 5).Value is not byte[] raw || raw.Length == 0) {
                    Fehler($"Datei nicht lesbar: {f}");
                    continue;
                }

                var content = raw.IsZipped() ? raw.UnzipIt() : raw;
                if (content is not { Length: > 0 } || !FragmentRecord.HasValidEofMarker(content)) {
                    Fehler($"Inhalt unvollständig (kein EOF-Marker): {f}");
                }
            }
        }

        // Leere Daten-Ordner entstehen gezielt als „Chunk bekannt, leer“-Marker beim
        // Nachladen; DeleteOrphanedChunkFolders räumt verwaiste später weg. Diese
        // Sammelzeile hält den Bericht kompakt.
        if (leereDatenOrdner.Count > 0) {
            var beispiele = string.Join(",", [.. leereDatenOrdner.OrderBy(n => n, StringComparer.Ordinal).Take(5)]);
            Warnung($"Leere Daten-Ordner={leereDatenOrdner.Count} (Marker/Nachläufer, Aufräumarbeit durch DeleteOrphanedChunkFolders) Beispiele={beispiele}");
        }

        // Kopf-Dateien (Snapshots und Journale, neues Format und Legacy) liegen im
        // Arbeitsverzeichnis und werden ebenso auf Name und Abschluss geprüft.
        // Das eigene, während des Laufs offene Journal bleibt außen vor.
        var eigenesJournalMuster = datei.FileNameWithoutSuffix().ToUpperInvariant() + "-" + Generic.UserName + "-";
        var loseDateien = Directory.GetFiles(basis, "*", SearchOption.TopDirectoryOnly);
        var kopfNeu = 0;
        var kopfLegacy = 0;

        foreach (var f in loseDateien) {
            if (f.FileNameWithoutSuffix().StartsWith(eigenesJournalMuster, StringComparison.OrdinalIgnoreCase)) { continue; }

            var suffix = f.FileSuffix().ToLowerInvariant();

            if (SnapshotSuffixe.Contains(suffix) || JournalSuffixe.Contains(suffix)) {
                var istSnapshot = SnapshotSuffixe.Contains(suffix);
                var istNeu = suffix is "snp" or "frg";

                if (istNeu) { kopfNeu++; } else { kopfLegacy++; }

                // Snapshots tragen den Zeitstempel; Journal-Dateien sind
                // benutzerbezogen benannt und werden nur auf Abschluss geprüft.
                if (istSnapshot && ParseChunkFileDate(f) is null) { Fehler($"Snapshot-Name ohne gültigen Zeitstempel: {f}"); }

                if (IO.ReadAllBytes(f, 5).Value is not byte[] rawF || rawF.Length == 0) {
                    Fehler($"Datei nicht lesbar: {f}");
                    continue;
                }

                var contentF = rawF.IsZipped() ? rawF.UnzipIt() : rawF;
                if (contentF is not { Length: > 0 } || !FragmentRecord.HasValidEofMarker(contentF)) {
                    // Ein Journal ohne EOF ist der normale Zustand nach einem hart
                    // beendeten Prozess (Kill/Absturz): Der Inhalt bleibt lesbar und
                    // wird eingebunden. Ein unvollständiger Snapshot dagegen ist ein
                    // echter Fehler.
                    if (istSnapshot) {
                        Fehler($"Snapshot unvollständig (kein EOF-Marker): {f}");
                    } else {
                        Warnung($"Journal ohne EOF-Marker (Sitzung nicht sauber beendet): {f}");
                    }
                }

                continue;
            }

            Warnung("Unerwartete Datei im Arbeitsverzeichnis: " + f);
        }

        Write("VERZEICHNIS", $"SystemOrdner={systemOrdner} DatenOrdner={datenOrdner} LeereDatenOrdner={leereDatenOrdner.Count} Dateien={dateiGesamt + loseDateien.Length} KopfNeu={kopfNeu} KopfLegacy={kopfLegacy} Basis={basis}");
    }

    /// <summary>
    /// Zahl der Chunk-Dateien (neues Format und Legacy) im Ordner; -1, wenn der Ordner fehlt.
    /// </summary>
    private static int ChunkDateiZahl(string ordner) => Directory.Exists(ordner)
        ? Directory.GetFiles(ordner, "*", SearchOption.TopDirectoryOnly).Count(f => ChunkSuffixe.Contains(f.FileSuffix().ToLowerInvariant()))
        : -1;

    /// <summary>
    /// Prüft je Zeile mit Chunk-Wert, dass ihr Ziel-Chunk-Ordner auf der Festplatte
    /// existiert und Daten enthält. Zeilen ohne Chunk-Wert laufen als Kopf-Fragment
    /// und brauchen keinen Chunk-Ordner.
    /// </summary>
    private static void PruefeZeilenZuChunks(Table tbl) {
        if (tbl is not TableFileNew tfn) { return; }

        var datei = tfn.Filename;
        var basis = datei.FilePath() + datei.FileNameWithoutSuffix() + "\\";

        var chunkSpalte = tbl.Column.ChunkValueColumn;

        if (chunkSpalte is { IsDisposed: false } cvc) {
            Write("CHUNK-INFO", $"ChunkValueColumn={cvc.KeyName} SaveContent={cvc.SaveContent} ValueForChunk={cvc.Value_for_Chunk} SaveOrder={(tbl.ColumnsInSaveOrder().Exists(c => c.KeyName.Equals(cvc.KeyName, StringComparison.OrdinalIgnoreCase)) ? "enthalten" : "FEHLT")}");
        }

        var zeilen = tbl.Row.ToList();
        foreach (var row in zeilen.Take(5)) {
            Write("ZEILE", $"Key={row.KeyName} ChunkValue='{row.ChunkValue}' → {row.ChunkId()}");
        }

        var bekannteIds = string.Join(",", KnownChunkIds(tfn));
        Write("CHUNK-IDS", $"Bekannt={bekannteIds}");

        var ohneOrdner = 0;
        var kopfZeilen = 0;

        foreach (var row in tbl.Row) {
            if (row.ChunkValue is not { Length: > 0 }) { kopfZeilen++; continue; }

            var id = row.ChunkId();
            var folder = basis + id + "\\";

            if (ChunkDateiZahl(folder) == 0) {
                ohneOrdner++;
                if (ohneOrdner <= 20) { Fehler($"Zeile {row.KeyName} (Chunk '{row.ChunkValue}' → '{id}') hat keine Chunk-Dateien"); }
            }
        }

        if (ohneOrdner > 20) { Fehler($"Weitere {ohneOrdner - 20} Zeilen ohne Chunk-Dateien (Details unterdrückt)"); }

        Write("CHUNKS", $"Zeilen={tbl.Row.Count} KopfZeilen={kopfZeilen} OhneChunkDateien={ohneOrdner}");

        // Verteilungsnachweis: je Chunk-Id die Zeilen-Zahl und die Datei-Zahl des
        // Ordners auf der Festplatte (-1 = Ordner fehlt).
        List<string> verteilung = [];

        foreach (var gruppe in tbl.Row.GroupBy(r => r.ChunkId(), StringComparer.OrdinalIgnoreCase).OrderBy(g => g.Key, StringComparer.Ordinal)) {
            var ordner = basis + gruppe.Key + "\\";
            var dateien = ChunkDateiZahl(ordner);
            verteilung.Add($"{gruppe.Key}={gruppe.Count()}/{dateien}");
        }

        Write("CHUNK-VERTEILUNG", string.Join(" ", verteilung));
    }

    /// <summary>
    /// Führt die Prüfung durch: TableStress --verify "pfad-zur-tabelle" [dump-datei]
    /// </summary>
    public static int Run(string tablePath, string? dumpPath) {
        Develop.StartService();
        Generic.UserName = "VERIFY";
        RowCollection.AllowProcessingAborts = false;
        tablePath = tablePath.Trim();

        // Relative Pfade auflösen: Table.Get arbeitet mit absoluten Pfaden.
        if (!Path.IsPathRooted(tablePath)) { tablePath = Path.GetFullPath(tablePath); }

        _fehler = 0;
        _warnungen = 0;
        _meldungCount = 0;

        Develop.MessageDG += OnDevelopMessage;

        try {
            // Bericht erst im Try öffnen: Ein ungültiger Tabellen-Pfad darf nicht
            // als unbehandelte Exception enden, sondern als FEHLER-Zeile.
            var dir = Path.GetDirectoryName(Path.GetFullPath(tablePath)) ?? AppContext.BaseDirectory;
            var reportPath = Path.Combine(dir, $"VERIFY-{DateTime.Now:HH-mm-ss}.txt");
            using var report = new StreamWriter(reportPath, false, Utf8Bom) { AutoFlush = true };
            _report = report;

            Write("START", $"Tabelle={tablePath} PID={Environment.ProcessId}");

            var tbl = Table.Get(tablePath);

            if (tbl is not { IsDisposed: false }) {
                Fehler($"Tabelle nicht ladbar: {tablePath}");
                return 1;
            }

            if (!tbl.BeSureToBeUpToDate(false)) { Fehler($"Tabelle {tbl.KeyName} konnte nicht auf den aktuellen Stand gebracht werden"); }

            var okLoad = tbl.LoadTableRows(false, -1);
            Write("DATENLADEN", okLoad ? $"ok Zeilen={tbl.Row.Count} Spalten={tbl.Column.Count}" : "fehlgeschlagen");
            if (!okLoad) { Fehler("LoadTableRows fehlgeschlagen"); }

            if (tbl.Column.Count == 0) { Fehler("Tabelle hat keine Spalten"); }

            PruefeVerzeichnis(tbl);
            PruefeZeilenZuChunks(tbl);

            // Ohne geladene Spalten gibt es keinen prüfbaren Stand: Ein Save würde
            // einen leeren Snapshot schreiben und den Inhalt auf der Platte überdecken.
            if (tbl.Column.Count == 0) {
                tbl.Dispose();
                Write("ENDE", $"FEHLER={_fehler} WARNUNGEN={_warnungen} MELDUNGEN={_meldungCount} Bericht={reportPath}");
                return _fehler == 0 ? 0 : 1;
            }

            PruefeUndos(tbl, "LADEN");

            // Speichern-Laden-Rundtrip: Stand sichern, freigeben, neu laden, vergleichen.
            var snap1 = Snapshot(tbl);
            var opr = tbl is TableFileNew tfn1 ? tfn1.Save() : (tbl as TableFile)?.Save() ?? OperationResult.Success;
            Write("SPEICHERN", opr.IsFailed ? "FEHLER: " + opr.FailedReason : "ok");
            if (opr.IsFailed) { Fehler("Save fehlgeschlagen: " + opr.FailedReason); }

            PruefeUndos(tbl, "NACH-SPEICHERN");

            tbl.Dispose();

            Thread.Sleep(1500);

            tbl = Table.Get(tablePath);
            if (tbl is not { IsDisposed: false }) {
                Fehler("Tabelle nach dem Neuladen nicht ladbar");
                return 1;
            }

            _ = tbl.BeSureToBeUpToDate(false);
            _ = tbl.LoadTableRows(false, -1);

            var snap2 = Snapshot(tbl);
            VergleicheSnapshots(snap1, snap2);
            PruefeUndos(tbl, "RELOAD");

            if (dumpPath is { Length: > 0 }) { DumpSchreiben(tbl, dumpPath); }

            tbl.Dispose();

            Write("ENDE", $"FEHLER={_fehler} WARNUNGEN={_warnungen} MELDUNGEN={_meldungCount} Bericht={reportPath}");
            return _fehler == 0 ? 0 : 1;
        } catch (Exception ex) {
            Fehler($"Fatal Typ={ex.GetType().Name} Meldung={ex.Message} Aufrufstack={Develop.DiagStack()}");
            return 3;
        } finally {
            Develop.MessageDG -= OnDevelopMessage;
            _report = null;
        }
    }

    /// <summary>
    /// Liest den kompletten Datenstand: je Zeile Key, Chunk-Wert und alle Zellwerte.
    /// </summary>
    private static List<ZeilenStand> Snapshot(Table tbl) {
        var columns = tbl.Column.Where(c => c is { IsDisposed: false }).ToList();
        List<ZeilenStand> result = [];

        foreach (var row in tbl.Row) {
            List<ZellWert> zellen = [];

            foreach (var c in columns) {
                var v = row.CellGetString(c).Replace('|', '/');
                zellen.Add(new ZellWert(c.KeyName, v));
            }

            result.Add(new ZeilenStand(row.KeyName, row.ChunkValue, zellen));
        }

        return [.. result.OrderBy(r => r.Key, StringComparer.OrdinalIgnoreCase)];
    }

    /// <summary>
    /// Gruppiert die Änderungs-Historie in Einträge (Sätze ab Tiefe 0 bis zum nächsten Tiefe-0-Satz).
    /// </summary>
    private static List<List<FragmentRecord>> UndoEintraege(IReadOnlyList<FragmentRecord> undos) {
        List<List<FragmentRecord>> entries = [];
        List<FragmentRecord>? cur = null;

        foreach (var r in undos) {
            if (r.Depth == 0 && cur is { Count: > 0 }) { entries.Add(cur); cur = null; }

            cur ??= [];
            cur.Add(r);
        }

        if (cur is { Count: > 0 }) { entries.Add(cur); }

        return entries;
    }

    /// <summary>
    /// Liest alle Meta-Fingerabdrücke der Fragment-Dateien: je Chunk-Ordner die
    /// Union aller Chunk-Dateien (der Ordner hält je Benutzer eine write-once Datei),
    /// zusätzlich die Kopf-Dateien (Snapshots/Journale) unter "*".
    /// </summary>
    private static Dictionary<string, HashSet<string>> UndoFingerprintsVonDisk(string basis) {
        var result = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var dir in Directory.GetDirectories(basis)) {
            var id = dir.TrimEnd(Path.DirectorySeparatorChar).FileNameWithSuffix().ToLowerInvariant();
            var set = new HashSet<string>(StringComparer.Ordinal);

            foreach (var file in Directory.GetFiles(dir, "*", SearchOption.TopDirectoryOnly)) {
                if (!ChunkSuffixe.Contains(file.FileSuffix().ToLowerInvariant())) { continue; }

                foreach (var r in MetaSaetzeVonDatei(file)) { set.Add(Fingerprint(r)); }
            }

            if (set.Count > 0) { result[id] = set; }
        }

        // Kopf-Fragmente: Snapshots und Journale im Wurzelverzeichnis.
        var head = new HashSet<string>(StringComparer.Ordinal);

        foreach (var f in Directory.GetFiles(basis, "*", SearchOption.TopDirectoryOnly)) {
            var suffix = f.FileSuffix().ToLowerInvariant();
            if (!SnapshotSuffixe.Contains(suffix) && !JournalSuffixe.Contains(suffix)) { continue; }

            foreach (var r in MetaSaetzeVonDatei(f)) { head.Add(Fingerprint(r)); }
        }

        result["*"] = head;

        return result;
    }

    private static void VergleicheSnapshots(List<ZeilenStand> snap1, List<ZeilenStand> snap2) {
        var keys1 = snap1.Select(r => r.Key).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var keys2 = snap2.Select(r => r.Key).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var verloren = keys1.Except(keys2).ToList();
        var geister = keys2.Except(keys1).ToList();

        foreach (var k in verloren.Take(10)) { Fehler($"Zeile nach Reload verloren: {k}"); }
        foreach (var k in geister.Take(10)) { Fehler($"Zeile nach Reload neu/Geist: {k}"); }

        var zellDifferenzen = 0;
        var chunkDifferenzen = 0;

        var byKey2 = snap2.ToDictionary(r => r.Key, StringComparer.OrdinalIgnoreCase);

        foreach (var r1 in snap1) {
            if (!byKey2.TryGetValue(r1.Key, out var r2)) { continue; }

            if (!string.Equals(r1.ChunkValue, r2.ChunkValue, StringComparison.Ordinal)) {
                chunkDifferenzen++;
                if (chunkDifferenzen <= 10) { Fehler($"Chunk-Wert geändert nach Reload: {r1.Key} '{r1.ChunkValue}' → '{r2.ChunkValue}'"); }
            }

            var z2 = r2.Zellen.ToDictionary(z => z.Spalte, StringComparer.OrdinalIgnoreCase);

            foreach (var z1 in r1.Zellen) {
                if (!z2.TryGetValue(z1.Spalte, out var w2)) { continue; }

                if (!string.Equals(z1.Wert, w2.Wert, StringComparison.Ordinal)) {
                    zellDifferenzen++;
                    if (zellDifferenzen <= 10) { Fehler($"Zellwert geändert nach Reload: {r1.Key} / {z1.Spalte}: '{z1.Wert}' → '{w2.Wert}'"); }
                }
            }
        }

        if (zellDifferenzen > 10) { Fehler($"Weitere {zellDifferenzen - 10} Zellwert-Differenzen nach Reload (Details unterdrückt)"); }

        Write("RELOAD", $"Zeilen Vorher={snap1.Count} Nachher={snap2.Count} Verloren={verloren.Count} Geister={geister.Count} ChunkDiff={chunkDifferenzen} ZellDiff={zellDifferenzen}");
    }

    private static void Warnung(string detail) {
        _warnungen++;
        Write("WARNUNG", detail);
    }

    private static void Write(string aktion, string detail) {
        var line = $"VERIFY|{DateTime.Now:HH:mm:ss.fff}|{aktion}|{OneLine(detail)}";

        if (_report is { } r) {
            lock (r) { r.WriteLine(line); }
        }

        Console.Out.WriteLine(line);
    }

    #endregion

    private sealed record ZellWert(string Spalte, string Wert);

    private sealed record ZeilenStand(string Key, string ChunkValue, List<ZellWert> Zellen);
}

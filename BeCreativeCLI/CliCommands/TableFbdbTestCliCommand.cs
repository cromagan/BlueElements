// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Globalization;
using System.IO;

namespace BeCreativeCLI.CliCommands;

/// <summary>
/// Intern: Roundtrip-Test des binären Fragment-Formats (.fbdb, TableFragmentsNew).
/// Legt eine Kopie der Quelltabelle an, konvertiert mehrfach (fbdb ↔ mbdb), schreibt
/// Änderungen über Fragmente, konvertiert auch den geänderten Stand und löst
/// Komplettierungen aus. Zusätzlich legt eine neue Tabelle "TabNew" als Kopie
/// der Quelle an (.nbdb/TableFile binär und .fbdb) und prüft deren Roundtrip,
/// Konvertierungskette und Komplettierung. Prüft Parser-Robustheit sowie
/// Writer-/Reader-Regeln von FragmentRecord (BinaryFormat.md §8).
/// </summary>
public class TableFbdbTestCliCommand : CliCommand {

    #region Fields

    /// <summary>
    /// Das Trenn-Byte 0x1F (Unit Separator) des Fragment-Formats, als Zeichen für Content-Tests.
    /// </summary>
    private const char UnitSeparatorChar = '\u001F';

    /// <summary>
    /// Blatt-Typ für die Prüfsätze des FBDB-Tests: Zellinhalt als UTF-8-Text
    /// (Spiegel von CellFragmentType.UTF8Value_withoutSizeData). Nur diesem Test bekannt.
    /// </summary>
    private enum TestFragmentType : byte {

        /// <summary>
        /// Zellinhalt als UTF-8-Text (Blatt-Typ einer Zelle).
        /// </summary>
        CellValue = 202
    }

    #endregion

    #region Properties

    public override string Command => "table-fbdbtest";

    public override List<string> Flags => ["keep", "dev"];

    public override bool Hidden => true;

    public override List<string> Options => ["dir"];

    public override string Syntax => "bcr table-fbdbtest <tabelle> [--dir <ordner>] [--keep]";

    public override string? HelpDetails =>
        "Führt den fbdb-Roundtrip-Test im Arbeitsordner aus (Standard: Temp). Exit 0 bei allen Tests; " +
        "der Ordner wird bei Erfolg gelöscht, --keep erhält ihn zur Untersuchung.";

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

        var workdir = (args.Option("dir") ?? Path.GetTempPath() + "fbdbtest_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture) + "\\").NormalizeFile();

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
        UserGroup = Administrator; // Master-Setzen und Komplettierung erlauben

        try {
            RunAllTests(workdir, srcFile, failures);

            Out(string.Empty);

            if (failures.Count == 0) {
                Out("*** ALLE FBDB-TESTS ERFOLGREICH ***");

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
        var fbdbFile = workdir + "TabelleNew.fbdb";

        #region T1: Baseline und fbdb-Kopie

        Out("=== T1: Kopie anlegen (.mbdb -> .fbdb) und Datenvergleich ===");

        var tblSrc = Load(srcFile);

        if (tblSrc is null) {
            failures.Add("T1: Quelltabelle konnte nicht geladen werden: " + srcFile);
            return;
        }

        var baseline = SnapshotOf(tblSrc);
        var opr = TableFile.Create(fbdbFile, tblSrc);
        ReleaseAll();

        if (!Check(failures, opr.IsSuccessful, "T1: Kopie TabelleNew.fbdb angelegt" + (opr.IsSuccessful ? string.Empty : " — " + opr.FailedReason))) { return; }

        var tblFbdb = Load(fbdbFile);

        if (tblFbdb is null) {
            failures.Add("T1: TabelleNew.fbdb konnte nicht geladen werden.");
            return;
        }

        CompareSnapshots(failures, "T1: fbdb-Kopie vs. Quelle", baseline, SnapshotOf(tblFbdb));
        ReleaseAll();

        #endregion

        #region T1b: Neue Tabelle TabNew (CopyTo-Pfad) als .nbdb und .fbdb

        Out(string.Empty);
        Out("=== T1b: Neue Tabelle 'TabNew' (Kopie von Table) als .nbdb und .fbdb ===");

        var nbdbFile = workdir + "TabNew.nbdb";
        var tblSrcNb = Load(srcFile);

        if (tblSrcNb is null) {
            failures.Add("T1b: Quelltabelle konnte nicht geladen werden: " + srcFile);
            return;
        }

        var oprNbdb = TableFile.Create(nbdbFile, tblSrcNb);
        ReleaseAll();

        if (!Check(failures, oprNbdb.IsSuccessful, "T1b: TabNew.nbdb angelegt (CopyTo)" + (oprNbdb.IsSuccessful ? string.Empty : " — " + oprNbdb.FailedReason))) { return; }

        var tblNbdb = Load(nbdbFile);

        if (tblNbdb is null) {
            failures.Add("T1b: TabNew.nbdb konnte nicht geladen werden.");
            return;
        }

        CompareSnapshots(failures, "T1b: TabNew.nbdb vs. Quelle", baseline, SnapshotOf(tblNbdb));
        ReleaseAll();

        // Die neue Tabelle ins fbdb-Format konvertieren und vergleichen.
        // Anderer Basisname, weil Create gleiche Namen mit anderer Endung ablehnt.
        var tblNbdb2 = Load(nbdbFile);

        if (tblNbdb2 is null) {
            failures.Add("T1b: TabNew.nbdb (2. Laden) konnte nicht geladen werden.");
            return;
        }

        var nbdbFbdbFile = workdir + "TabNew2.fbdb";
        var oprNbdbFbdb = TableFile.Create(nbdbFbdbFile, tblNbdb2);
        ReleaseAll();

        if (!Check(failures, oprNbdbFbdb.IsSuccessful, "T1b: TabNew.nbdb -> TabNew.fbdb" + (oprNbdbFbdb.IsSuccessful ? string.Empty : " — " + oprNbdbFbdb.FailedReason))) { return; }

        var tblNbdbFbdb = Load(nbdbFbdbFile);

        if (tblNbdbFbdb is null) {
            failures.Add("T1b: TabNew.fbdb konnte nicht geladen werden.");
            return;
        }

        CompareSnapshots(failures, "T1b: TabNew.fbdb (aus .nbdb) vs. Quelle", baseline, SnapshotOf(tblNbdbFbdb));
        ReleaseAll();

        #endregion

        #region T2: Konvertierungskette fbdb <-> mbdb

        Out(string.Empty);
        Out("=== T2: Konvertierungskette (mehrfach) ===");

        var chainFile = fbdbFile;

        for (var i = 1; i <= 4; i++) {
            var suffix = i % 2 == 1 ? "mbdb" : "fbdb";
            var target = workdir + "Hop" + i.ToString1() + "." + suffix;
            var tblHop = Load(chainFile);

            if (tblHop is null) {
                failures.Add("T2: Kettentabelle konnte nicht geladen werden: " + chainFile);
                return;
            }

            var hopOpr = TableFile.Create(target, tblHop);
            ReleaseAll();

            if (!Check(failures, hopOpr.IsSuccessful, $"T2 Hop {i}: {chainFile.FileNameWithSuffix()} -> {target.FileNameWithSuffix()}" + (hopOpr.IsSuccessful ? string.Empty : " — " + hopOpr.FailedReason))) { return; }

            var tblCheck = Load(target);

            if (tblCheck is null) {
                failures.Add("T2: Kettentabelle konnte nicht geladen werden: " + target);
                return;
            }

            CompareSnapshots(failures, $"T2 Hop {i}: Daten identisch zur Quelle", baseline, SnapshotOf(tblCheck));
            ReleaseAll();
            chainFile = target;
        }

        #endregion

        #region T2b: TabNew — Konvertierungskette nbdb <-> fbdb <-> mbdb

        Out(string.Empty);
        Out("=== T2b: TabNew — Konvertierungskette (nbdb, fbdb, mbdb) ===");

        var newChainFile = nbdbFbdbFile;

        for (var i = 1; i <= 4; i++) {
            var suffix = i % 2 == 1 ? "mbdb" : "fbdb";
            var target = workdir + "NewHop" + i.ToString1() + "." + suffix;
            var tblNewHop = Load(newChainFile);

            if (tblNewHop is null) {
                failures.Add("T2b: Kettentabelle konnte nicht geladen werden: " + newChainFile);
                return;
            }

            var hopOpr = TableFile.Create(target, tblNewHop);
            ReleaseAll();

            if (!Check(failures, hopOpr.IsSuccessful, $"T2b Hop {i}: {newChainFile.FileNameWithSuffix()} -> {target.FileNameWithSuffix()}" + (hopOpr.IsSuccessful ? string.Empty : " — " + hopOpr.FailedReason))) { return; }

            var tblNewCheck = Load(target);

            if (tblNewCheck is null) {
                failures.Add("T2b: Kettentabelle konnte nicht geladen werden: " + target);
                return;
            }

            CompareSnapshots(failures, $"T2b Hop {i}: Daten identisch zur Quelle", baseline, SnapshotOf(tblNewCheck));
            ReleaseAll();
            newChainFile = target;
        }

        #endregion

        #region T4b: TabNew — Komplettierung

        Out(string.Empty);
        Out("=== T4b: TabNew — Komplettierung (Vorgänger-Session, Nachfolger schreibt Hauptdatei neu) ===");

        // Vorgänger-Instanz: erzeugt eine Fragment-Session mit einer echten,
        // rückgängig gemachten Kopf-Änderung (wichtig genug, um beim Schließen
        // nicht gelöscht zu werden) und schließt die Datei sauber mit 255 ab.
        var tblPredNew = Load(newChainFile);

        if (tblPredNew is null) {
            failures.Add("T4b: Laden für Vorgänger-Session fehlgeschlagen: " + newChainFile);
            return;
        }

        var quickInfoNew = tblPredNew.RowQuickInfo;
        var fehlerPNHin = tblPredNew.ChangeData(TableDataType.RowQuickInfo, null, quickInfoNew, quickInfoNew + " #fbdbtest");
        var fehlerPNZurueck = tblPredNew.ChangeData(TableDataType.RowQuickInfo, null, quickInfoNew + " #fbdbtest", quickInfoNew);

        if (!Check(failures, fehlerPNHin is { Length: 0 } && fehlerPNZurueck is { Length: 0 }, "T4b: Vorgänger-Session angelegt (RowQuickInfo hin und zurück)")) {
            Out("  RowQuickInfo-Fehler: " + fehlerPNHin + " / " + fehlerPNZurueck);
            ReleaseAll();
            return;
        }

        ReleaseAll(); // Session-Datei sauber mit EOF abschließen

        // Nachfolge-Instanz: nimmt die Vorgänger-Session als Fremd-Fragment auf
        // und stößt als Master die Komplettierung an.
        var tblCompleteNew = Load(newChainFile);

        if (tblCompleteNew is null) {
            failures.Add("T4b: Laden nach Session-Schluss fehlgeschlagen: " + newChainFile);
            return;
        }

        tblCompleteNew.MasterMe();
        tblCompleteNew.TemporaryTableMasterTimeUtc = DateTime.UtcNow.AddMinutes(-8).ToString5();

        KomplettierungPruefen("T4b", tblCompleteNew, newChainFile, failures, baseline);

        #endregion

        #region T3: Änderungen schreiben und über Fragmente nachladen

        Out(string.Empty);
        Out("=== T3: Änderungen über Fragment-Dateien (Schreiben + Nachladen) ===");

        var tblEdit = Load(chainFile);

        if (tblEdit is null) {
            failures.Add("T3: Änderungstabelle konnte nicht geladen werden: " + chainFile);
            return;
        }

        var editInfo = ApplyChanges(tblEdit);

        // Hintergrundprozesse abwarten (Zeilenprüfung setzt SYS_ROWSTATE zeitgesteuert)
        // und den Tabellenzustand reparieren, wie es ein Neuladen täte — erst dann
        // entspricht der Live-Stand dem, was Fragmente beim Nachladen ergeben.
        RowCollection.InvalidatedRowsManager.DoAllInvalidatedRows(null, true, null);
        System.Threading.Thread.Sleep(1500);
        tblEdit.RepairAfterParse();

        var expected = SnapshotOf(tblEdit);
        ReleaseAll();

        if (!Check(failures, editInfo.Ok, "T3: Änderungen ausgeführt (" + editInfo.Message + ")")) { return; }

        var tblReplay = Load(chainFile);

        if (tblReplay is null) {
            failures.Add("T3: Nachladen der Änderungstabelle fehlgeschlagen: " + chainFile);
            return;
        }

        CompareSnapshots(failures, "T3: Fragment-Nachladen identisch zu erwartetem Stand", expected, SnapshotOf(tblReplay));
        ReleaseAll();

        #endregion

        #region T4: Komplettierung

        Out(string.Empty);
        Out("=== T4: Komplettierung (Master altert, Nachladen schreibt Hauptdatei neu) ===");

        var tblMaster = Load(chainFile);

        if (tblMaster is null) {
            failures.Add("T4: Laden für Komplettierung fehlgeschlagen: " + chainFile);
            return;
        }

        // Master setzen und den Eintrag über MasterTry (7 min) hinaus altern lassen.
        tblMaster.MasterMe();
        tblMaster.TemporaryTableMasterTimeUtc = DateTime.UtcNow.AddMinutes(-8).ToString5();

        // Master-Sätze gelten als "unwichtig" — eine Session mit nur Master-Sätzen würde
        // ihre Fragment-Datei beim Schließen löschen. Eine wertgleiche, aber wichtige
        // Kopf-Änderung hält die Datei am Leben (wie echte Benutzereingaben in der Session).
        var fehlerMaster = tblMaster.ChangeData(TableDataType.RowQuickInfo, null, tblMaster.RowQuickInfo, tblMaster.RowQuickInfo);

        if (!Check(failures, fehlerMaster is { Length: 0 }, "T4: Master gealtert, Fragment-Session stabil")) {
            Out("  RowQuickInfo-Fehler: " + fehlerMaster);
            ReleaseAll();
            return;
        }

        // Hauptdatei älter als DoComplete (60 min) machen, Session-Fragment merken
        // (jüngste .frg): Sie muss nach dem Freigeben sauber mit 255 abgeschlossen sein.
        var sessionFile = GetFiles(chainFile.FilePath() + "Frgm\\", "*.frg", SearchOption.TopDirectoryOnly)
            .Select(f => GetFileInfo(f))
            .Where(fi => fi is not null && fi.LastWriteTimeUtc > DateTime.UtcNow.AddSeconds(-10))
            .OrderBy(fi => fi!.LastWriteTimeUtc)
            .LastOrDefault()?.FullName;

        // Ein Nachlade-Zyklus (wie der periodische TableUpdater einer offenen Anwendung)
        // liest die Fragment-Dateien anderer Sessions und stößt die Komplettierung an.
        KomplettierungPruefen("T4", tblMaster, chainFile, failures, expected);

        if (sessionFile is not null) {
            using (var stream = new FileStream(sessionFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                Check(failures, FragmentRecord.EndsOnEof(stream), "T4: Session-Fragment endet auf 255 (EOF): " + sessionFile.FileNameWithSuffix());
            }

            if (ReadAllBytes(sessionFile, 10).Value is byte[] sessionBytes) {
                var versionOk = FragmentRecord.TryParse(sessionBytes, 0, out var first, out _) && first is { TagType: FragmentTagType.SerializerVersion };
                Check(failures, versionOk, "T4: Session-Fragment beginnt mit SerializerVersion-Satz (232): " + sessionFile.FileNameWithSuffix());
            }
        }

        #endregion

        #region T5: Stabilität + Fragment-Datei-Prüfungen + Müll-Fragment

        Out(string.Empty);
        Out("=== T5: Stabilität nach Komplettierung, Fragment-Dateien, Robustheit ===");

        var tblStable = Load(chainFile);

        if (tblStable is null) {
            failures.Add("T5: Erneutes Laden nach Komplettierung fehlgeschlagen: " + chainFile);
            return;
        }

        CompareSnapshots(failures, "T5: Erneutes Laden identisch zu erwartetem Stand", expected, SnapshotOf(tblStable));
        WriteRobustnessFragment(tblStable, editInfo.MarkerRowKey, failures);
        ReleaseAll();

        var tblRobust = Load(chainFile);

        if (tblRobust is null) {
            failures.Add("T5: Laden mit Müll-Fragment fehlgeschlagen: " + chainFile);
            return;
        }

        var robustOk = editInfo.MarkerRowKey is { Length: > 0 } markerKey &&
                       tblRobust.Column["KATEGORIE"] is { IsDisposed: false } robCol &&
                       tblRobust.Row.GetByKey(markerKey) is { IsDisposed: false } robRow &&
                       robRow.CellGetString(robCol) == "RobustheitsMarkerXYZ";

        Check(failures, robustOk, "T5: Marker-Wert aus gültigem Satz neben Müllsätzen übernommen");

        // Der Marker-Satz ist jetzt fester Bestandteil des Tabellenstands — der
        // erwartete Stand für weitere Tests umfasst ihn.
        expected = SnapshotOf(tblRobust);
        ReleaseAll();

        #endregion

        #region T6: Konvertierungskette nach Änderungen + zweite Komplettierung

        Out(string.Empty);
        Out("=== T6: Geänderte Tabelle konvertieren (fbdb <-> mbdb) und erneut komplettieren ===");

        var editChainFile = chainFile;

        for (var i = 1; i <= 4; i++) {
            var suffix = i % 2 == 1 ? "mbdb" : "fbdb";
            var target = workdir + "EditHop" + i.ToString1() + "." + suffix;
            var tblEditHop = Load(editChainFile);

            if (tblEditHop is null) {
                failures.Add("T6: Geänderte Kettentabelle konnte nicht geladen werden: " + editChainFile);
                return;
            }

            var hopOpr = TableFile.Create(target, tblEditHop);
            ReleaseAll();

            if (!Check(failures, hopOpr.IsSuccessful, $"T6 Hop {i}: {editChainFile.FileNameWithSuffix()} -> {target.FileNameWithSuffix()}" + (hopOpr.IsSuccessful ? string.Empty : " — " + hopOpr.FailedReason))) { return; }

            var tblEditCheck = Load(target);

            if (tblEditCheck is null) {
                failures.Add("T6: Geänderte Kettentabelle konnte nicht geladen werden: " + target);
                return;
            }

            CompareSnapshots(failures, $"T6 Hop {i}: Geänderter Stand unverändert", expected, SnapshotOf(tblEditCheck));
            ReleaseAll();
            editChainFile = target;
        }

        // Zweite Komplettierung auf dem zuletzt geschriebenen fbdb-Stand.
        // Ablauf wie in T4: Eine Vorgänger-Instanz erzeugt die Fragment-Session
        // (echte, rückgängig gemachte Kopf-Änderung — wichtig genug, um beim Schließen
        // nicht gelöscht zu werden), eine Nachfolge-Instanz bleibt als Master im
        // Speicher und stößt beim Nachladen die Komplettierung an.
        var tblMaster2 = Load(editChainFile);

        if (tblMaster2 is null) {
            failures.Add("T6: Laden für zweite Komplettierung fehlgeschlagen: " + editChainFile);
            return;
        }

        var quickInfoOriginal = tblMaster2.RowQuickInfo;
        var fehlerM2Hin = tblMaster2.ChangeData(TableDataType.RowQuickInfo, null, quickInfoOriginal, quickInfoOriginal + " #fbdbtest");
        var fehlerM2Zurueck = tblMaster2.ChangeData(TableDataType.RowQuickInfo, null, quickInfoOriginal + " #fbdbtest", quickInfoOriginal);

        if (!Check(failures, fehlerM2Hin is { Length: 0 } && fehlerM2Zurueck is { Length: 0 }, "T6: Master-Session angelegt (RowQuickInfo hin und zurück)")) {
            Out("  RowQuickInfo-Fehler: " + fehlerM2Hin + " / " + fehlerM2Zurueck);
            ReleaseAll();
            return;
        }

        ReleaseAll(); // Session-Datei sauber mit EOF abschließen

        // Nachfolge-Instanz: nimmt die Vorgänger-Session als Fremd-Fragment auf und
        // wird selbst Master (der Replay endet auf "Unset" — ein neuer Master darf
        // sofort übernehmen).
        var tblComplete2 = Load(editChainFile);

        if (tblComplete2 is null) {
            failures.Add("T6: Laden nach Session-Schluss fehlgeschlagen: " + editChainFile);
            return;
        }

        tblComplete2.MasterMe();
        tblComplete2.TemporaryTableMasterTimeUtc = DateTime.UtcNow.AddMinutes(-8).ToString5();

        // Ein Nachlade-Zyklus liest die Fragment-Datei und stößt die Komplettierung an.
        KomplettierungPruefen("T6", tblComplete2, editChainFile, failures, expected);

        #endregion

        #region T7: FragmentRecord-Regeln (Writer/Reader)

        Out(string.Empty);
        Out("=== T7: FragmentRecord — Writer-/Reader-Regeln ===");

        RunRecordTests(failures);

        #endregion
    }

    /// <summary>
    /// Führt die Änderungen für T3 aus: Zellen (inkl. Umbrüche, US im Content, langer Text),
    /// Zeile anlegen/löschen, Spalte anlegen/löschen, Tags. Liefert den Key der
    /// nicht gesperrten ersten Zeile (Marker-Ziel für den Robustheitstest) oder leer.
    /// </summary>
    private static (bool Ok, string Message, string MarkerRowKey) ApplyChanges(Table tbl) {
        var colKategorie = tbl.Column["KATEGORIE"];
        var colUeberschrift = tbl.Column["UEBERSCHRIFT"];
        var colAnleitung = tbl.Column["ANLEITUNG"];
        var colBeispiel = tbl.Column["BEISPIEL"];

        if (colKategorie is null || colUeberschrift is null || colAnleitung is null || colBeispiel is null) {
            return (false, "erwartete Spalten fehlen (KATEGORIE/UEBERSCHRIFT/ANLEITUNG/BEISPIEL)", string.Empty);
        }

        // Zwei nicht gesperrte Zeilen suchen.
        RowItem? rowA = null;
        RowItem? rowB = null;

        foreach (var r in tbl.Row) {
            if (IsLocked(null, r)) { continue; }

            if (rowA is null) {
                rowA = r;
                continue;
            }

            rowB = r;
            break;
        }

        if (rowA is null || rowB is null) { return (false, "weniger als 2 nicht gesperrte Zeilen", string.Empty); }

        // Zell-Änderungen mit Sonderfällen: Umlaute, Umbruch, US (0x1F) im Content, langer Text, Tab.
        var fehlerA = rowA.CellSet(colKategorie, "fbdb-Roundtrip äöüß ;|<>\r\n" + UnitSeparatorChar + "US-im-Content", "fbdbtest Zelle A");
        var fehlerLang = rowB.CellSet(colAnleitung, new string('L', 2000) + "-Ende", "fbdbtest langer Wert");
        var fehlerB = rowB.CellSet(colBeispiel, "Wert mit\r\nZeilenumbruch und\tTab", "fbdbtest Zelle B");

        if (fehlerA is { Length: > 0 } || fehlerLang is { Length: > 0 } || fehlerB is { Length: > 0 }) {
            return (false, $"Zell-Änderungen abgelehnt: KATEGORIE='{fehlerA}' ANLEITUNG='{fehlerLang}' BEISPIEL='{fehlerB}'", string.Empty);
        }

        // Zeilen anlegen und eine davon wieder löschen.
        var rowNew = tbl.Row.GenerateAndAdd("fbdb Testzeile ÄÖÜ", "fbdbtest addrow");
        var rowDel = tbl.Row.GenerateAndAdd("fbdb Loeschzeile", "fbdbtest addrow temp");

        if (rowNew is null || rowDel is null) { return (false, "Zeilen konnten nicht angelegt werden", string.Empty); }

        var delOpr = RowCollection.Remove(rowDel, "fbdbtest delrow");

        if (delOpr.IsFailed) { return (false, "Löschzeile konnte nicht entfernt werden: " + delOpr.FailedReason, string.Empty); }

        // Spalten anlegen, beschreiben; zweite Spalte wieder löschen.
        var colNew = tbl.Column.GenerateAndAdd("FBDBTESTSPALTE", "FBDB-Testspalte", null, "Roundtrip-Prüfung");
        var colTemp = tbl.Column.GenerateAndAdd("FBDBTEMPSPALTE", "Temp", null, string.Empty);

        if (colNew is null || colTemp is null) { return (false, "Spalten konnten nicht angelegt werden", string.Empty); }

        var fehlerNeu = rowNew.CellSet(colNew, "Spaltenwert äöü", "fbdbtest neue Spalte");

        if (fehlerNeu is { Length: > 0 }) { return (false, "Wert in neuer Spalte abgelehnt: " + fehlerNeu, string.Empty); }

        if (!tbl.Column.Remove(colTemp, "fbdbtest delcolumn")) { return (false, "Temp-Spalte konnte nicht gelöscht werden", string.Empty); }

        // Kopf-Wert ändern.
        tbl.Tags = ["FBDBTEST", "Zweiter Tag"];

        return (true, "3 Zellen, +2/-1 Zeilen, +2/-1 Spalten, 1 Zelle in neuer Spalte, Tags", rowA.KeyName);
    }

    /// <summary>
    /// Altert die Hauptdatei über die Komplettierungs-Schwelle (DoComplete), stößt
    /// per Nachlade-Zyklus die Komplettierung an und prüft, dass die Hauptdatei
    /// neu geschrieben wurde und die Daten unverändert bleiben.
    /// </summary>
    private static void KomplettierungPruefen(string testId, Table tbl, string mainFile, List<string> failures, (byte[] Main, byte[] HeadVars, byte[] Rows) expected) {
        // Hauptdatei älter als DoComplete (60 min) machen; Chunk-Cache invalideren,
        // damit LastSaveMainFileUtcDate das neue Dateidatum liest.
        File.SetLastWriteTimeUtc(mainFile, DateTime.UtcNow.AddMinutes(-70));

        if (Chunk.Get(mainFile) is { } masterChunk) { masterChunk.Invalidate(); }

        var before = File.GetLastWriteTimeUtc(mainFile);

        // Ein Nachlade-Zyklus (wie der periodische TableUpdater einer offenen Anwendung)
        // liest die Fragment-Dateien anderer Sessions und stößt die Komplettierung an.
        var reloadOk = tbl.BeSureToBeUpToDate(false);

        var after = File.GetLastWriteTimeUtc(mainFile);

        Check(failures, reloadOk && after.Subtract(before).TotalMinutes > 30, $"{testId}: Hauptdatei neu geschrieben (vorher {before.ToString9()}, nachher {after.ToString9()})");
        CompareSnapshots(failures, $"{testId}: Daten nach Komplettierung identisch zu erwartetem Stand", expected, SnapshotOf(tbl));
        ReleaseAll();
    }

    /// <summary>
    /// T6: Writer-/Reader-Regeln von FragmentRecord im Direktvergleich (BinaryFormat.md §4/§8).
    /// </summary>
    private static void RunRecordTests(List<string> failures) {
        // Steuer-Satz schreiben und lesen.
        var verStream = new MemoryStream();
        var verWritten = FragmentRecord.Append(verStream, 1, FragmentTagType.SerializerVersion, "4.3");
        var verBytes = verStream.ToArray();
        var verOk = FragmentRecord.TryParse(verBytes, 0, out var verRec, out var verNext);

        Check(failures, verWritten, "Append Steuer-Satz SerializerVersion");
        Check(failures, verOk && verRec is { TagType: FragmentTagType.SerializerVersion, StringContent: "4.3", Depth: 1 } && verNext == verBytes.Length, "Steuer-Satz Roundtrip (SerializerVersion, Content, Tiefe 1, Satzende)");

        // Daten-Satz mit Tiefe und Meta; Content mit Umbrüchen, Tab und US.
        var utc = new DateTime(2026, 11, 6, 10, 12, 33, DateTimeKind.Utc);
        var contentMitSonderzeichen = "Zeile1\r\nZeile2\tTab" + UnitSeparatorChar + "US-im-Content Ende";

        var dataStream = new MemoryStream();
        var dataWritten = FragmentRecord.Append(dataStream, 3, (byte)TestFragmentType.CellValue, contentMitSonderzeichen, "User ÄÖÜ", utc, "Kommentar mit ; und |");
        var dataBytes = dataStream.ToArray();
        var dataOk = FragmentRecord.TryParse(dataBytes, 0, out var dataRec, out _);

        Check(failures, dataWritten && dataOk, "Daten-Satz mit Sonderzeichen geschrieben und geparst");
        Check(failures, dataRec is { HasMeta: true } r
                && r.Depth == 3
                && r.LeafType == (byte)TestFragmentType.CellValue
                && r.StringContent == contentMitSonderzeichen
                && r.User == "User ÄÖÜ"
                && r.Comment == "Kommentar mit ; und |"
                && Math.Abs(r.DateTimeUtc.Subtract(utc).TotalSeconds) < 1,
            "Daten-Satz Roundtrip: Tiefe, Content, Meta korrekt");

        // Verbotene Sätze: Tiefe 0, Empfänger-Typ > 230, US in User und Comment,
        // Steuer-Satz ohne Steuer-Wert.
        var rejectStream = new MemoryStream();

        Check(failures, !FragmentRecord.Append(rejectStream, 0, (byte)TestFragmentType.CellValue, "x"), "Writer lehnt Tiefe 0 ab");
        Check(failures, !FragmentRecord.Append(rejectStream, 2, 231, "x"), "Writer lehnt Empfänger-Typ > 230 ab");
        Check(failures, !FragmentRecord.Append(rejectStream, 1, (byte)TestFragmentType.CellValue, "x", "u" + UnitSeparatorChar, utc, "k"), "Writer lehnt US im User ab");
        Check(failures, !FragmentRecord.Append(rejectStream, 1, (byte)TestFragmentType.CellValue, "x", "u", utc, "k" + UnitSeparatorChar), "Writer lehnt US im Comment ab");
        Check(failures, !FragmentRecord.Append(rejectStream, 1, FragmentTagType.None, "x"), "Writer lehnt Steuer-Satz ohne Steuer-Wert ab");

        // Großer Content: die 4-Byte-Längen (256^4) erlauben weit mehr als 65535 Bytes.
        var bigStream = new MemoryStream();
        var bigWritten = FragmentRecord.Append(bigStream, 1, (byte)TestFragmentType.CellValue, new string('x', 70000), "u", utc, "k");
        var bigOk = FragmentRecord.TryParse(bigStream.ToArray(), 0, out var bigRec, out _);

        Check(failures, bigWritten && bigOk && bigRec is { StringContent.Length: 70000 }, "Großer Content (70000 Bytes) rundtrippt (4-Byte-Längen)");

        // Tab-Validierung: Steuer-Satz ohne Tab -> Satz ungültig, Position rückt auf Satzende.
        var wrongTabs = new byte[] { 0x01, 0x07, 0x00, 0x00, 0x00, 0x20, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x0D };
        var wrongOk = FragmentRecord.TryParse(wrongTabs, 0, out _, out var wrongNext);

        Check(failures, !wrongOk && wrongNext == wrongTabs.Length, "Steuer-Satz ohne Tab -> Satz ungültig, Position auf Satzende");

        // Steuer-Satz mit 3 Tabs: gültig, Tiefe 3 (Kommentar-Trenner an beliebiger Tiefe).
        var deepTabs = new byte[] { 0x09, 0x09, 0x09, 0x01, 0x07, 0x00, 0x00, 0x00, 0x20, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x0D };
        var deepOk = FragmentRecord.TryParse(deepTabs, 0, out var deepRec, out var deepNext);

        Check(failures, deepOk && deepRec is { TagType: FragmentTagType.Eof, Depth: 3 } && deepNext == deepTabs.Length, "Steuer-Satz mit 3 Tabs -> gültig (Tiefe 3)");

        // Zustandserkennung: 255 am Ende = abgeschlossen, 254 = in Bearbeitung, Altformat-Zeilen.
        var eofStream = new MemoryStream();
        _ = FragmentRecord.Append(eofStream, 1, FragmentTagType.Eof, string.Empty);

        using (var s = new MemoryStream(eofStream.ToArray())) { Check(failures, FragmentRecord.EndsOnEof(s), "EndsOnEof: 255-Satz = abgeschlossen"); }

        var contStream = new MemoryStream();
        _ = FragmentRecord.Append(contStream, 1, FragmentTagType.ContinueEof, string.Empty);

        using (var s = new MemoryStream(contStream.ToArray())) { Check(failures, !FragmentRecord.EndsOnEof(s), "EndsOnEof: 254-Satz = in Bearbeitung"); }

        using (var s = new MemoryStream(Encoding.UTF8.GetBytes("- User cp33\r\n- EOF\r\n"))) { Check(failures, FragmentRecord.EndsOnEof(s), "EndsOnEof: Altformat '- EOF' = abgeschlossen"); }

        using (var s = new MemoryStream()) { Check(failures, !FragmentRecord.EndsOnEof(s), "EndsOnEof: leere Datei = nicht abgeschlossen"); }

        using (var s = new MemoryStream(Encoding.UTF8.GetBytes("- User abc\r\n- EOF\r\n"))) { Check(failures, FragmentRecord.HeadUser(s) == "abc", "HeadUser: Altformat '- User abc'"); }

        var userStream = new MemoryStream();
        _ = FragmentRecord.Append(userStream, 1, FragmentTagType.User, "cp33");

        using (var s = new MemoryStream(userStream.ToArray())) { Check(failures, FragmentRecord.HeadUser(s) == "cp33", "HeadUser: 233-Satz"); }
    }

    /// <summary>
    /// Schreibt eine Fragment-Datei mit einem gültigen Zell-Satz, gefolgt von Müllsätzen
    /// (unbekannter SpeicherTyp, falsche Tab-Zahl, US im Pfad, Pfad-Typ > 230).
    /// </summary>
    private static void WriteRobustnessFragment(Table tbl, string markerRowKey, List<string> failures) {
        if (markerRowKey is not { Length: > 0 }) {
            Check(failures, false, "T5: Robustheitstest übersprungen (keine Marker-Zeile)");
            return;
        }

        if (tbl is not TableFragmentsNew tfn) {
            Check(failures, false, "T5: Robustheitstest übersprungen (kein TableFragmentsNew)");
            return;
        }

        var path = tfn.FragmengtsPath() + tbl.KeyName.ToUpperInvariant() + "-ROBUST-" + DateTime.UtcNow.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture) + "." + TableFragmentsNew.SuffixOfFragments;

        using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);

        // Gültiger Block: Zell-Änderung mit Meta (RowKey- + SubColumn-Satz + Blatt).
        var ok = FragmentChange.AppendFragment(fs, new FragmentChange(tbl.KeyName, TableDataType.UTF8Value_withoutSizeData, "KATEGORIE", markerRowKey, "RobustheitsMarkerXYZ", UserName, DateTime.UtcNow, "fbdbtest Robustheit", "fbdbtest"));

        if (!Check(failures, ok, "T5: gültiger Marker-Block geschrieben: " + path.FileNameWithSuffix())) { return; }

        // Müll 1: unbekannter SpeicherTyp, korrektes len1, Steuer-Satz-Bauform -> wird übersprungen.
        fs.Write(new byte[] { 0x09, 0x63, 0x08, 0x00, 0x00, 0x00, 0x20, 0xF8, 0x01, 0x00, 0x00, 0x00, 0x5A, 0x0D });

        // Müll 2: falsche Tab-Zahl (3 statt 1) -> Satz ungültig, Reader springt per len1 weiter.
        fs.Write(new byte[] { 0x09, 0x09, 0x09, 0x01, 0x07, 0x00, 0x00, 0x00, 0x20, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x0D });

        // Müll 3: Empfänger-Typ ohne 0x20-Trenner -> Satz nicht lesbar -> verworfen.
        fs.Write(new byte[] { 0x09, 0x01, 0x0F, 0x00, 0x00, 0x00, 0xD2, 0x1F, 0x61, 0x1F, 0x62, 0x1F, 0x63, 0x20, 0x02, 0x01, 0x00, 0x00, 0x00, 0x58, 0x0D });

        // Müll 4: Empfänger-Typ > 230 (reservierter Bereich) -> Satz verworfen.
        fs.Write(new byte[] { 0x09, 0x01, 0x09, 0x00, 0x00, 0x00, 0xFE, 0x20, 0x02, 0x01, 0x00, 0x00, 0x00, 0x58, 0x0D });

        fs.Flush();

        Check(failures, true, "T5: Müllsätze angehängt (unbekannter SpeicherTyp, falsche Tabs, Trenner fehlt, reservierter Empfänger-Typ)");
    }

    #endregion

    #region Load, Snapshot, Prüfungen

    /// <summary>
    /// Lädt eine Tabelle frisch von der Datei (Instanzen müssen vorher freigegeben sein).
    /// </summary>
    private static Table? Load(string filename) {
        var tbl = Table.Get(filename);

        if (tbl is not { IsDisposed: false }) {
            Console.Error.WriteLine("Tabelle nicht ladbar: " + filename);
            return null;
        }

        if (!tbl.Unlocked) {
            Console.Error.WriteLine("Tabelle passwortgeschützt: " + tbl.KeyName);
            tbl.Dispose();
            return null;
        }

        if (!tbl.BeSureToBeUpToDate(false)) {
            Console.Error.WriteLine("Tabelle nicht auf aktuellem Stand bringbar: " + tbl.KeyName);
            tbl.Dispose();
            return null;
        }

        if (tbl.IsFreezed) {
            Console.Error.WriteLine("Tabelle eingefroren: " + tbl.KeyName + " — " + tbl.FreezedReason);
            tbl.Dispose();
            return null;
        }

        return tbl;
    }

    /// <summary>
    /// Vergleicht drei Snapshot-Teile und verbucht Abweichungen mit Diff-Details.
    /// Zeilen werden semantisch verglichen, ohne das zeitgesteuerte SYS_ROWSTATE.
    /// </summary>
    private static void CompareSnapshots(List<string> failures, string caption, (byte[] Main, byte[] HeadVars, byte[] Rows) expected, (byte[] Main, byte[] HeadVars, byte[] Rows) actual) {
        var d1 = DiffOf(expected.Main, actual.Main);

        if (!Check(failures, d1 is null, caption + " — Head+Spalten (MainChunk)")) { Out("  MainChunk-Diff: " + d1); }

        var d2 = DiffOf(expected.HeadVars, actual.HeadVars);

        if (!Check(failures, d2 is null, caption + " — Variablen (HeadVariableChunks)")) { Out("  HeadVars-Diff: " + d2); }

        var (ok3, detail3) = RowsDiff(expected.Rows, actual.Rows);

        if (!Check(failures, ok3, caption + " — Zeilen (RowChunk)")) { Out("  Rows-Diff: " + detail3); }
    }

    /// <summary>
    /// Vergleicht zwei RowChunks über die geparsten Datensätze, ohne SYS_ROWSTATE
    /// (wird von der Zeilenprüfung je Prüfzeitpunkt neu gesetzt).
    /// </summary>
    private static (bool Ok, string Detail) RowsDiff(byte[] expectedRows, byte[] actualRows) {
        var e = RowChunkRecords(expectedRows);
        var a = RowChunkRecords(actualRows);

        if (e.Count != a.Count) { return (false, $"Anzahl Datensätze expect={e.Count}, actual={a.Count}"); }

        // Reihenfolge-insensitiv vergleichen: Die Zell-Speicherreihenfolge einer Zeile
        // ist historieabhängig (Einfüge-Reihenfolge), Werte und Anzahl müssen gleich sein.
        e.Sort(StringComparer.Ordinal);
        a.Sort(StringComparer.Ordinal);

        for (var i = 0; i < e.Count; i++) {
            if (e[i] != a[i]) { return (false, $"Datensatz {i} unterscheidet sich:\r\n  expect: {e[i]}\r\n  actual: {a[i]}"); }
        }

        return (true, string.Empty);
    }

    /// <summary>
    /// Zerlegt einen RowChunk in Datensatz-Zeilen (Typ|Spalte|Zeile|Wert), ohne SYS_ROWSTATE.
    /// </summary>
    private static List<string> RowChunkRecords(byte[] rows) {
        List<string> result = [];
        var pointer = 0;

        while (pointer < rows.Length) {
            var (newPointer, type, value, colName, rowKey) = Table.Parse(rows, pointer);

            if (newPointer <= pointer) { break; }

            pointer = newPointer;

            if (colName == "SYS_ROWSTATE") { continue; }

            result.Add(((int)type).ToString1() + "|" + colName + "|" + rowKey + "|" + value);
        }

        return result;
    }

    /// <summary>
    /// Vergleicht zwei Byte-Arrays und meldet bis zu drei Abweichungsregionen mit Kontext.
    /// </summary>
    private static string? DiffOf(byte[] expected, byte[] actual) {
        if (expected.SequenceEqual(actual)) { return null; }

        var minLen = Math.Min(expected.Length, actual.Length);
        List<int> diffs = [];

        for (var i = 0; i < minLen; i++) {
            if (expected[i] != actual[i]) {
                diffs.Add(i);
                if (diffs.Count >= 3) { break; }
            }
        }

        if (diffs.Count == 0) {
            return $"Länge expect={expected.Length}, actual={actual.Length}, gemeinsamer Bereich identisch";
        }

        var sb = new StringBuilder($"Länge expect={expected.Length}, actual={actual.Length}; erste Abweichung bei {diffs[0]}".ToString(CultureInfo.InvariantCulture));

        foreach (var d in diffs) {
            var s = Math.Max(0, d - 20);
            sb.Append(CultureInfo.InvariantCulture, $"\r\n  Abweichung bei {d}\r\n    expect: ").Append(Printable(expected, s, 140)).Append("\r\n    actual: ").Append(Printable(actual, s, 140));
        }

        return sb.ToString();
    }

    /// <summary>
    /// Wandelt Bytes in escapted Text (Steuerzeichen als \xNN), für Diff-Ausgaben.
    /// </summary>
    private static string Printable(byte[] bytes, int start, int count) {
        var sb = new StringBuilder();
        var end = Math.Min(bytes.Length, start + count);

        for (var i = start; i < end; i++) {
            var b = bytes[i];

            if (b is >= 0x20 and < 0x7F) {
                sb.Append((char)b);
            } else {
                sb.Append("\\x" + b.ToString("X2", CultureInfo.InvariantCulture));
            }
        }

        return sb.ToString();
    }

    private static void Out(string text) => Console.Out.WriteLine(text);

    /// <summary>
    /// Prüft eine Bedingung, gibt das Ergebnis aus und verbucht Fehler. Liefert die Bedingung zurück.
    /// </summary>
    private static bool Check(List<string> failures, bool ok, string text) {
        Out((ok ? "[OK]  " : "[FEHLER] ") + text);

        if (!ok) { failures.Add(text); }

        return ok;
    }

    private static void ReleaseAll() {
        RowCollection.InvalidatedRowsManager.DoAllInvalidatedRows(null, true, null);

        foreach (var tbl in Table.AllInstances().ToList()) {
            if (tbl is { IsDisposed: false }) { tbl.Dispose(); }
        }
    }

    /// <summary>
    /// Erzeugt den Vergleichsstand einer Tabelle: MainChunk (Kopf + Spalten),
    /// HeadVariableChunks (Variablen) und RowChunk (alle Zeilen).
    /// </summary>
    private static (byte[] Main, byte[] HeadVars, byte[] Rows) SnapshotOf(Table tbl) {
        var tbf = (TableFile)tbl;
        return ([.. TableChunk.GenerateMainChunk(tbf)], [.. TableChunk.GenerateHeadVariableChunks(tbf)], [.. TableChunk.GenerateRowChunk(tbf, true, string.Empty)]);
    }

    #endregion
}

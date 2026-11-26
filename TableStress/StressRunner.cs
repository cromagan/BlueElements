// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Diagnostics;
using System.IO;
using System.Threading;

namespace TableStress;

/// <summary>
/// Führt den Stresstest aus: alle 2 Sekunden eine Zufalls-Aktion auf der Tabelle, 10 Minuten lang.
/// </summary>
public sealed class StressRunner(Table tbl, StressProtocol prot, string user) {

    #region Fields

    private readonly Random random = new();

    private readonly TimeSpan runTime = TimeSpan.FromMinutes(10);

    /// <summary>
    /// Werte mit kritischen Zeichen (Umbruch, Pipe, Anführungszeichen, Unicode, Tabs,
    /// Leerzeichen, Länge): üben die Serialisierung von Zellinhalten.
    /// </summary>
    private static readonly string[] SonderWerte = [
        string.Empty,
        "a\r\nb",
        "x\ny",
        "a|b|c",
        "Er sagte \"Hallo\"",
        "a;b,c",
        "a\tb",
        "  gepolstert  ",
        "Zelle ✓ #12 🚀 äöüß",
        new string('L', 5000),
    ];

    /// <summary>
    /// Fester Pool von Chunk-Werten: hält die Zahl der Chunk-Ordner überschaubar,
    /// sodass sich Zeilen nachweisbar zwischen den Ordnern verteilen und wandern.
    /// </summary>
    private static readonly string[] ChunkWerte = ["Alpha", "Beta", "Gamma", "Delta", "Epsilon", "Zeta"];

    /// <summary>
    /// Master setzen und Komplettierung anstoßen sind nur in dieser Anfangszeit erlaubt; danach ist der Tick Idle.
    /// </summary>
    private readonly TimeSpan masterTime = TimeSpan.FromMinutes(2);

    private readonly AutoResetEvent tickSignal = new(false);

    private DateTime startUtc;

    private bool freezeReported;

    /// <summary>
    /// True, sobald ein NEU-LADEN die Tabelle nicht wiederherstellen konnte;
    /// der Restlauf läuft dann ohne Aktionen weiter, um keine Flut zu erzeugen.
    /// </summary>
    private bool tabellenVerlust;

    private int scriptCount;

    private int spaltenCount;

    private System.Threading.Timer? timer;

    private int tickCount;

    #endregion

    #region Methods

    private void ActionAlleDatenLaden() {
        var ok = tbl.LoadTableRows(false, -1);
        prot.Write("DATENLADEN", ok ? $"Alle Daten geladen, Zeilen={tbl.Row.Count}" : "Nachladen fehlgeschlagen");
    }

    private void ActionCsvExportPruefen() {
        var csv = CsvHelper.ExportCSV(tbl, ';', true);

        if (csv.Length == 0 && tbl.Row.Count > 0) { prot.Write("FEHLER", "CSV-Export leer, obwohl Zeilen vorhanden"); }

        prot.Write("CSV", $"Zeichen={csv.Length} Zeilen={csv.SplitBy("\r\n").Count(l => l.Length > 0)} TabellenZeilen={tbl.Row.Count}");
    }

    /// <summary>
    /// Setzt die CHUNK-Zelle einer Zufallszeile auf einen anderen Pool-Wert:
    /// Die Zeile wandert beim Speichern in einen anderen Chunk-Ordner, inklusive
    /// der zugehörigen Undo-Historie.
    /// </summary>
    private void ActionChunkWechsel() {
        WartenAufSpeichern();
        var spalte = ChunkSpalte();

        if (spalte is null) { prot.Write("CHUNK-WECHSEL", "Keine Chunk-Spalte vorhanden"); return; }

        var rows = tbl.Row.ToArray();

        if (rows.Length == 0) { prot.Write("CHUNK-WECHSEL", "Keine Zeile vorhanden"); return; }

        var row = rows[random.Next(rows.Length)];
        var alt = row.CellGetString(spalte);
        var neu = ZufallsChunk(alt);
        var failed = row.CellSet(spalte, neu, $"StressTest {user} Chunk wechseln");

        prot.Write("CHUNK-WECHSEL", failed is { Length: > 0 }
            ? $"Key={row.KeyName} FEHLER: {failed}"
            : $"Key={row.KeyName} Wert='{Logtext(alt)}'→'{neu}' Id={IHasChunkValueExtension.ChunkId(alt)}→{IHasChunkValueExtension.ChunkId(neu)} ok");
    }

    private void ActionDuplikatePruefen() {
        var columns = tbl.Column.Where(c => c is { IsDisposed: false }).ToList();

        if (columns.Count == 0) { prot.Write("DUPLIKATE", "Keine Spalte vorhanden"); return; }

        var spalte = columns[random.Next(columns.Count)];
        var gruppen = RowCollection.DuplicateGroups(tbl, [spalte], null, false).ToList();

        prot.Write("DUPLIKATE", $"Spalte={spalte.KeyName} Gruppen={gruppen.Count} Zeilen={gruppen.Sum(g => g.Count)}");
    }

    /// <summary>
    /// Filter-Engine-Stichprobe: Sucht per Istgleich nach dem Wert einer Zufallszelle
    /// (mit Chunk-Filter, wenn eine Chunk-Spalte vorhanden ist). Die Quellzeile muss
    /// im Ergebnis sein und kein Treffer darf den Filter verletzen.
    /// </summary>
    private void ActionFilterPruefung() {
        var rows = tbl.Row.ToArray();

        if (rows.Length == 0) { prot.Write("FILTER", "Keine Zeile vorhanden"); return; }

        var chunkSpalte = ChunkSpalte();
        var columns = tbl.Column.Where(c => c is { IsDisposed: false } && !c.KeyName.StartsWith("SYS_", StringComparison.OrdinalIgnoreCase) && c != chunkSpalte).ToArray();

        if (columns.Length == 0) { prot.Write("FILTER", "Keine filterbare Spalte vorhanden"); return; }

        var row = rows[random.Next(rows.Length)];
        var column = columns[random.Next(columns.Length)];
        var wert = row.CellGetString(column);
        var zellFilter = new FilterItem(column, FilterType.Istgleich, wert);

        // Ohne Chunk-Filter liefert die Engine bei Chunk-Tabellen bewusst ein leeres Ergebnis.
        var chunkWert = string.Empty;
        List<FilterItem> filter;

        if (chunkSpalte is { IsDisposed: false } cvc) {
            chunkWert = row.CellGetString(cvc);
            filter = [new FilterItem(cvc, FilterType.Istgleich, chunkWert), zellFilter];
        } else {
            filter = [zellFilter];
        }

        var treffer = FilterCollection.CalculateFilteredRows(tbl, false, [.. filter]);
        var fremde = treffer.Count(r => !r.MatchesTo(zellFilter));
        var enthalten = treffer.Exists(r => r.KeyName.Equals(row.KeyName, StringComparison.OrdinalIgnoreCase));
        var quellePasst = row.MatchesTo(zellFilter);

        if (!quellePasst) {
            // Mehrzeilen-Spalten treffen per Konvention zeilenweise — der
            // Volltext-Istgleich der Stichprobe passt dann by design nicht.
            if (column.MultiLine || wert.Contains('\r')) {
                prot.Write("FILTER", $"Spalte={column.KeyName} Mehrzeilen-Semantik — Volltext-Selbstvergleich übersprungen");
                return;
            }

            // Konkurrenz: Eine fremde Änderung oder ein laufender Refresh kann
            // kurzzeitig einen anderen Stand liefern — der Zweitversuch muss stabil sein.
            Thread.Sleep(500);
            var jetzt = row.CellGetString(column);

            if (!string.Equals(jetzt, wert, StringComparison.Ordinal)) {
                prot.Write("FILTER", $"Spalte={column.KeyName} Wert inzwischen fremd geändert — Stichprobe neutral");
                return;
            }

            if (row.MatchesTo(zellFilter)) {
                prot.Write("FILTER", $"Spalte={column.KeyName} erstes Lesen kurzzeitig inkonsistent, Zweitversuch ok — neutral");
                return;
            }

            // Die Zeile kann zwischenzeitlich fremd gelöscht worden sein: Jede Prüfung
            // einer entsorgten Zeile liefert guard-leer bzw. false — kein echter Fehler.
            if (row.IsDisposed) {
                prot.Write("FILTER", "Quellzeile inzwischen gelöscht — Stichprobe neutral");
                return;
            }

            prot.Write("FEHLER", $"FILTER Quellzeile passt nicht auf den eigenen Wert: Key={row.KeyName} Spalte={column.KeyName} Wert={Logtext(wert)}");
        }

        if (fremde > 0) { prot.Write("FEHLER", $"FILTER {fremde} Treffer erfüllen den Filter nicht: Spalte={column.KeyName} Treffer={treffer.Count}"); }

        if (quellePasst && !enthalten) {
            // Konkurrenz: Ein fremder Chunk-Wechsel oder eine Zell-Änderung zwischen
            // Lesen und Suche nimmt der Zeile den Treffer legitim wieder weg —
            // der stabile Zweitversuch muss sie wieder liefern.
            Thread.Sleep(500);
            var wertJetzt = row.CellGetString(column);
            var chunkJetzt = chunkSpalte is { IsDisposed: false } cs ? row.CellGetString(cs) : string.Empty;

            if (row.IsDisposed || !string.Equals(wertJetzt, wert, StringComparison.Ordinal) || !string.Equals(chunkJetzt, chunkWert, StringComparison.Ordinal)) {
                prot.Write("FILTER", "Quellzeile inzwischen fremd geändert/gelöscht — Stichprobe neutral");
                return;
            }

            var trefferZwei = FilterCollection.CalculateFilteredRows(tbl, false, [.. filter]);

            if (trefferZwei.Exists(r => r.KeyName.Equals(row.KeyName, StringComparison.OrdinalIgnoreCase))) {
                prot.Write("FILTER", "erste Suche kurzzeitig inkonsistent, Zweitversuch ok — neutral");
                return;
            }

            prot.Write("FEHLER", $"FILTER Quellzeile fehlt im Ergebnis: Key={row.KeyName} Spalte={column.KeyName} Treffer={treffer.Count}");
        }

        prot.Write("FILTER", $"Spalte={column.KeyName} Wert={Logtext(wert)} Treffer={treffer.Count} QuelleEnthalten={(enthalten ? "ja" : "nein")} QuellePasst={(quellePasst ? "ja" : "NEIN")}");
    }

    private void ActionKomplettierung() {
        if (MasterIdle()) {
            prot.Write("KOMPLETTIERUNG", "Idle — Master setzen ist nur in den ersten 2 Minuten erlaubt");
            return;
        }

        tbl.BeSureToBeUpToDate(false);
        tbl.MasterMe();

        // Der Refresh-Zyklus stößt die Komplettierung an, wenn der letzte Snapshot zu alt ist.
        var refreshed = tbl.BeSureToBeUpToDate(false);
        var master = tbl.TemporaryTableMasterUser;
        var masterOk = string.Equals(master, user, StringComparison.OrdinalIgnoreCase);

        prot.Write("KOMPLETTIERUNG", $"Refresh={(refreshed ? "ok" : "fehlgeschlagen")} Master={master}{(masterOk ? string.Empty : " (MasterMe erfolglos)")} KomplettierungFällig={tbl.MasterNeeded}");
    }

    /// <summary>
    /// Leichte In-Memory-Konsistenzprüfung: doppelte Zeilen-Keys, leere Chunk-Ids
    /// (nur mit Chunk-Spalte — ohne sind alle Zeilen korrekt Kopf-Fragmente),
    /// leere Spalten-Keys.
    /// </summary>
    private void ActionKonsistenzPruefen() {
        List<string> probleme = [];
        var rows = tbl.Row.Where(r => !r.IsDisposed).ToList();

        var doppelte = rows.GroupBy(r => r.KeyName, StringComparer.OrdinalIgnoreCase).Count(g => g.Count() > 1);
        if (doppelte > 0) { probleme.Add($"DoppelteRowKeys={doppelte}"); }

        if (tbl.Column.ChunkValueColumn is { IsDisposed: false }) {
            var leereChunks = rows.Count(r => r.ChunkId() is not { Length: > 0 });
            if (leereChunks > 0) { probleme.Add($"LeereChunkIds={leereChunks}"); }
        }

        var leereKeys = tbl.Column.Count(c => c is { IsDisposed: false } && c.KeyName is not { Length: > 0 });
        if (leereKeys > 0) { probleme.Add($"LeereSpaltenKeys={leereKeys}"); }

        if (probleme.Count > 0) { prot.Write("FEHLER", "Konsistenz " + string.Join("; ", probleme)); }

        var chunkIds = tbl is TableFileNew tfn ? tfn.Saveable.KnownChunkIds().Count : 0;

        prot.Write("PRÜFUNG", $"Zeilen={rows.Count} Spalten={tbl.Column.Count} ChunkIds={chunkIds} Probleme={probleme.Count}");
    }

    private void ActionMasterSetzen() {
        if (MasterIdle()) {
            prot.Write("MASTER", "Idle — Master setzen ist nur in den ersten 2 Minuten erlaubt");
            return;
        }

        tbl.BeSureToBeUpToDate(false);
        tbl.MasterMe();
        var master = tbl.TemporaryTableMasterUser;

        // MasterMe kann still erfolglos bleiben (Chunk gesperrt, Daten nicht frisch genug) — sichtbar machen.
        if (string.Equals(master, user, StringComparison.OrdinalIgnoreCase)) {
            prot.Write("MASTER", $"Master={master} ZeitUtc={tbl.TemporaryTableMasterTimeUtc} Rechner={tbl.TemporaryTableMasterMachine}");
        } else {
            prot.Write("MASTER", $"MasterMe erfolglos, Master bleibt {master}");
        }
    }

    private void ActionNeueZeile() {
        WartenAufSpeichern();
        var opr = NeueMussZeile($"StressTest {user} Neue Zeile");

        if (opr.IsFailed || opr.Value is not RowItem row) {
            prot.Write("NEUEZEILE", "Fehlgeschlagen: " + opr.FailedReason);
            return;
        }

        prot.Write("NEUEZEILE", $"Key={row.KeyName} ok");
    }

    /// <summary>
    /// Bildet den Zufallswert 0-49 auf die Aktion ab.
    /// </summary>
    private static StressAction ActionOf(int value) {
        switch (value) {
            case <= 3: return StressAction.NeueZeile;
            case 4: return StressAction.ZeileLoeschen;
            case <= 20: return StressAction.ZelleAendern;
            case 21: return StressAction.MasterSetzen;
            case 22: return StressAction.Komplettierung;
            case 23: return StressAction.ScriptErstellen;
            case <= 26: return StressAction.AlleDatenLaden;
            case <= 28: return StressAction.SpalteAnlegen;
            case 29: return StressAction.SpalteLoeschen;
            case 30: return StressAction.ZelleSonderzeichen;
            case 31: return StressAction.ZeileDuplizieren;
            case 32: return StressAction.SpalteMetadaten;
            case 33: return StressAction.SortierungPruefen;
            case 34: return StressAction.DuplikatePruefen;
            case 35: return StressAction.ScriptLoeschen;
            case 36: return StressAction.KonsistenzPruefen;
            case 37: return StressAction.CsvExportPruefen;
            case <= 39: return StressAction.ChunkWechsel;
            case 40: return StressAction.SchreibPruefung;
            case 41: return StressAction.UniqueVerletzung;
            case <= 43: return StressAction.FilterPruefung;
            case 44: return StressAction.ZeilenSchub;
            case <= 46: return StressAction.ZeileVollLesen;
            case 47: return StressAction.TabelleNeuLaden;
            case 48: return StressAction.ScriptAusfuehren;
            default: return StressAction.SpeicherStand;
        }
    }

    /// <summary>
    /// Schreib-Pipeline-Stichprobe: Ändert eine Zelle und prüft, dass der Wert
    /// ankommt und die Änderung registriert wird — klassisches Format über den
    /// letzten Undo-Eintrag, Fragment-Format über SaveRequired und Undo-Sätze.
    /// </summary>
    private void ActionSchreibPruefung() {
        WartenAufSpeichern();
        var rows = tbl.Row.ToArray();

        if (rows.Length == 0) { prot.Write("SCHREIB", "Keine Zeile vorhanden"); return; }

        var chunkSpalte = ChunkSpalte();

        // Systemspalten (SYS_) sind nie beschreibbar; die Chunk-Spalte hat eigene Aktionen.
        var columns = tbl.Column.Where(c => c is { IsDisposed: false } && !c.KeyName.StartsWith("SYS_", StringComparison.OrdinalIgnoreCase) && c != chunkSpalte).ToArray();

        if (columns.Length == 0) { prot.Write("SCHREIB", "Keine bearbeitbare Spalte vorhanden"); return; }

        var row = rows[random.Next(rows.Length)];
        var column = columns[random.Next(columns.Length)];
        var vorher = row.CellGetString(column);
        var kommentar = $"StressTest {user} Schreib-Stichprobe";
        var failed = row.CellSet(column, RandomValue(), kommentar);

        if (failed is { Length: > 0 }) {
            prot.Write("SCHREIB", $"Key={row.KeyName} Spalte={column.KeyName} verweigert: {Logtext(failed)}");
            return;
        }

        // Der gelesene Wert muss sich durch den Schreibzugriff ändern.
        var nachher = row.CellGetString(column);
        var angekommen = !string.Equals(nachher, vorher, StringComparison.Ordinal);

        switch (tbl) {
            case TableFile tf: {
                var item = tf.Undo.Count > 0 ? tf.Undo[^1] : null;
                var geloggt = item is not null
                    && item.ColName.Equals(column.KeyName, StringComparison.OrdinalIgnoreCase)
                    && item.RowKey.Equals(row.KeyName, StringComparison.OrdinalIgnoreCase)
                    && item.Comment == kommentar
                    && string.Equals(item.User, user, StringComparison.OrdinalIgnoreCase);

                string historie;

                if (geloggt) {
                    historie = "ok";
                } else if (item is null) {
                    historie = "FEHLER: Historie leer";
                } else {
                    historie = "FEHLER: " + Logtext(item.ColName + "/" + item.RowKey);
                }

                prot.Write("SCHREIB", $"Key={row.KeyName} Spalte={column.KeyName} Wert={Logtext(nachher)} {(angekommen ? "übernommen" : "FEHLER: Wert unverändert")} UndoHistorie={historie}");

                if (!angekommen) { prot.Write("FEHLER", $"SCHREIB Zellwert kam nicht an: Key={row.KeyName} Spalte={column.KeyName}"); }
                if (!geloggt) { prot.Write("FEHLER", $"SCHREIB letzter Undo-Eintrag passt nicht zur Änderung: Key={row.KeyName} Spalte={column.KeyName}"); }
                break;
            }
            case TableFileNew tfn: {
                // Im Fragment-Format muss der Schreibzugriff den Persistenz-Bedarf setzen;
                // die Zahl der Undo-Sätze dient als Beobachtungsgröße.
                var saveRequired = tfn.Saveable.SaveRequired;

                prot.Write("SCHREIB", $"Key={row.KeyName} Spalte={column.KeyName} Wert={Logtext(nachher)} {(angekommen ? "übernommen" : "FEHLER: Wert unverändert")} SaveRequired={saveRequired} UndoSaetze={tfn.Saveable.Undos.Count}");

                if (!angekommen) { prot.Write("FEHLER", $"SCHREIB Zellwert kam nicht an: Key={row.KeyName} Spalte={column.KeyName}"); }
                if (!saveRequired) { prot.Write("FEHLER", $"SCHREIB SaveRequired nach Zelländerung false: Key={row.KeyName} Spalte={column.KeyName}"); }
                break;
            }
            default: {
                prot.Write("SCHREIB", $"Key={row.KeyName} Spalte={column.KeyName} Wert={Logtext(nachher)} {(angekommen ? "übernommen" : "FEHLER: Wert unverändert")}");

                if (!angekommen) { prot.Write("FEHLER", $"SCHREIB Zellwert kam nicht an: Key={row.KeyName} Spalte={column.KeyName}"); }
                break;
            }
        }
    }

    /// <summary>
    /// Führt das neueste eigene Stress-Script aus: Die Skript-Engine läuft sonst
    /// im ganzen Stresstest nie echt; ein fehlgeschlagener Durchlauf wird sichtbar.
    /// </summary>
    private void ActionScriptAusfuehren() {
        var praefix = $"Stress_{user}_";
        var eigene = tbl.EventScript.Where(s => s.KeyName.StartsWith(praefix, StringComparison.Ordinal)).ToList();

        if (eigene.Count == 0) { prot.Write("SCRIPT-AUS", "Kein eigenes Script vorhanden"); return; }

        var script = eigene[^1];
        var f = tbl.ExecuteScript(script, true, null, null, false, false, true, null);

        prot.Write("SCRIPT-AUS", f.Failed
            ? $"Name={script.KeyName} FEHLER: {Logtext(f.FailedReason)}"
            : $"Name={script.KeyName} ok (Break={f.BreakFired} Return={f.ReturnFired})");

        if (f.Failed) { prot.Write("FEHLER", $"SCRIPT-AUS Ausführung fehlgeschlagen: Name={script.KeyName}"); }
    }

    private void ActionScriptErstellen() {
        WartenAufSpeichern();
        scriptCount++;
        var name = $"Stress_{user}_{scriptCount:000}";

        if (!ScriptDescription.IsValidName(name)) {
            prot.Write("SCRIPT", $"Name ungültig: {name}");
            return;
        }

        var text = $"DebugPrint(\"Stress {user} {scriptCount:000}\");";
        var precheck = ScriptPreCheck.Check(text);

        if (precheck.HasSyntaxErrors) {
            prot.Write("SCRIPT", $"Syntaxfehler in {name}: {string.Join("; ", precheck.SyntaxErrors)}");
            return;
        }

        var script = new TableScriptDescription(tbl, name, text);
        tbl.EventScript = new TableScriptCollection(tbl, [.. tbl.EventScript, script]);

        prot.Write("SCRIPT", $"Name={name} ok");
    }

    private void ActionScriptLoeschen() {
        WartenAufSpeichern();
        var praefix = $"Stress_{user}_";
        var eigene = tbl.EventScript.Where(s => s.KeyName.StartsWith(praefix, StringComparison.Ordinal)).ToList();

        if (eigene.Count == 0) { prot.Write("SCRIPT-WEG", "Kein eigenes Script vorhanden"); return; }

        var ziel = eigene[^1];
        tbl.EventScript = new TableScriptCollection(tbl, [.. tbl.EventScript.Where(s => s.KeyName != ziel.KeyName)]);

        prot.Write("SCRIPT-WEG", $"Name={ziel.KeyName} ok");
    }

    private void ActionSortierungPruefen() {
        var rows = tbl.Row.ToArray();

        if (rows.Length == 0) { prot.Write("SORTIERUNG", "Keine Zeile vorhanden"); return; }

        var columns = tbl.Column.Where(c => c is { IsDisposed: false }).ToArray();

        if (columns.Length == 0) { prot.Write("SORTIERUNG", "Keine Spalte vorhanden"); return; }

        var column = columns[random.Next(columns.Length)];
        var sorted = new RowSortDefinition(tbl, column, random.Next(2) == 0).SortedRows(rows);

        prot.Write("SORTIERUNG", $"Spalte={column.KeyName} Zeilen={sorted.Count} Erste={sorted.FirstOrDefault()?.KeyName} Letzte={sorted.LastOrDefault()?.KeyName}");
    }

    /// <summary>
    /// Legt eine Stress-Spalte an; ab 25 Stress-Spalten wird die älteste zuvor entfernt.
    /// </summary>
    private void ActionSpalteAnlegen() {
        WartenAufSpeichern();
        var stressSpalten = StressSpalten().ToList();

        if (stressSpalten.Count >= 25) {
            var aelteste = stressSpalten[0];

            if (tbl.Column.Remove(aelteste, $"StressTest {user} Spaltenbegrenzung")) {
                prot.Write("SPALTE-WEG", $"Key={aelteste.KeyName} ok (Begrenzung)");
                stressSpalten.RemoveAt(0);
            }
        }

        spaltenCount++;
        var name = $"STRESS_{user}_{spaltenCount:000}";

        if (!ColumnItem.IsValidColumnKey(name)) {
            prot.Write("SPALTE-NEU", $"Name ungültig: {name}");
            return;
        }

        var column = tbl.Column.GenerateAndAdd(name, $"Stress {user} {spaltenCount:000}", null, string.Empty);

        prot.Write("SPALTE-NEU", column is { IsDisposed: false } c ? $"Key={c.KeyName} ok" : "Fehlgeschlagen");
    }

    private void ActionSpalteLoeschen() {
        WartenAufSpeichern();
        var spalten = StressSpalten().ToList();

        if (spalten.Count == 0) { prot.Write("SPALTE-WEG", "Keine Stress-Spalte vorhanden"); return; }

        var column = spalten[0];
        var ok = tbl.Column.Remove(column, $"StressTest {user} Spalte löschen");

        prot.Write("SPALTE-WEG", ok ? $"Key={column.KeyName} ok" : $"Key={column.KeyName} FEHLER: Remove verweigert");
    }

    private void ActionSpalteMetadaten() {
        WartenAufSpeichern();
        var spalten = StressSpalten().ToList();

        if (spalten.Count == 0) { prot.Write("SPALTE-META", "Keine Stress-Spalte vorhanden"); return; }

        var column = spalten[random.Next(spalten.Count)];
        var caption = $"Stress {user} Kap {random.Next(100):00}";
        var quickinfo = $"QI-{user}-{random.Next(1000, 9999)}";

        column.Caption = caption;
        column.QuickInfo = quickinfo;

        prot.Write("SPALTE-META", $"Key={column.KeyName} Caption='{caption}' QuickInfo='{quickinfo}' ok");
    }

    /// <summary>
    /// Speicher-Telemetrie: Managed Memory (mit erzwungener Sammlung) und Working
    /// Set je Zeilen-/Spaltenzahl — macht wachsende Lecks über mehrere Läufe sichtbar.
    /// </summary>
    private void ActionSpeicherStand() {
        var managed = GC.GetTotalMemory(true);
        var workingSet = Environment.WorkingSet;

        prot.Write("SPEICHERSTAND", $"Managed={managed} WorkingSet={workingSet} Zeilen={tbl.Row.Count} Spalten={tbl.Column.Count} Scripte={tbl.EventScript.Count} Ticks={tickCount}");
    }

    /// <summary>
    /// Schwergewichtiger Rundtrip: speichern, Tabelle freigeben, neu laden — der
    /// Reload läuft mitten im Lauf, während andere Benutzer weiterschreiben.
    /// Zeilen- und Spaltenzahlen dürfen sich durch fremde Änderungen verschieben.
    /// </summary>
    private void ActionTabelleNeuLaden() {
        string datei;

        switch (tbl) {
            case TableFileNew tfnDatei: datei = tfnDatei.Filename; break;
            case TableFile tfDatei: datei = tfDatei.Filename; break;
            default: datei = string.Empty; break;
        }

        if (datei is not { Length: > 0 }) { prot.Write("NEU-LADEN", "Kein Dateiname — übersprungen"); return; }

        FlushChanges("vor Neuladen");

        switch (tbl) {
            case TableFileNew tfn when tfn.Saveable.SaveRequired: {
                // Fremde Edit-Sperren halten das Chunk-Speichern bewusst offene —
                // die Änderungen sind im Kopf-Journal gesichert und gehen nicht verloren.
                var sperren = tfn.Saveable.ForeignLockedChunkIds();

                if (sperren.Count > 0) {
                    prot.Write("NEU-LADEN", $"Speichern ausständig wegen fremder Edit-Sperre ({string.Join(",", sperren)}) — ok");
                } else {
                    prot.Write("FEHLER", "NEU-LADEN Speichern unvollständig: SaveRequired bleibt aktiv (TableFileNew)");
                }

                break;
            }
            case TableFile tf when tf.SaveRequired: {
                // Auch im klassischen Format halten fremde Edit-Sperren das
                // Chunk-Speichern bewusst offen — die Änderungen bleiben im Speicher
                // und der nächste Versuch holt sie nach.
                List<string> sperren = tbl is TableChunk tch ? tch.ForeignLockedChunkIds() : [];

                if (sperren.Count > 0) {
                    prot.Write("NEU-LADEN", $"Speichern ausständig wegen fremder Edit-Sperre ({string.Join(",", sperren)}) — ok");
                } else {
                    prot.Write("FEHLER", "NEU-LADEN Speichern unvollständig: SaveRequired bleibt aktiv (TableFile)");
                }

                break;
            }
        }

        var zeilenVorher = tbl.Row.Count;
        var spaltenVorher = tbl.Column.Count;
        var masterVorher = tbl.TemporaryTableMasterUser;

        tbl.Dispose();
        Thread.Sleep(1500);

        var neu = Table.Get(datei);

        if (neu is not { IsDisposed: false }) {
            Thread.Sleep(3000);
            neu = Table.Get(datei);
        }

        if (neu is not { IsDisposed: false }) {
            tabellenVerlust = true;
            prot.Write("FEHLER", $"NEU-LADEN Tabelle nach dem Freigeben nicht ladbar: {datei} — Aktionen ausgesetzt");
            return;
        }

        tbl = neu;
        freezeReported = false;

        if (!tbl.BeSureToBeUpToDate(false)) { prot.Write("FEHLER", $"NEU-LADEN {tbl.KeyName} konnte nicht auf den aktuellen Stand gebracht werden"); }

        var geladen = tbl.LoadTableRows(false, -1);

        prot.Write("NEU-LADEN", $"{(geladen ? "ok" : "FEHLER: Nachladen fehlgeschlagen")} Zeilen {zeilenVorher}→{tbl.Row.Count} Spalten {spaltenVorher}→{tbl.Column.Count} Master {masterVorher}→{tbl.TemporaryTableMasterUser}");

        if (!geladen) { prot.Write("FEHLER", "NEU-LADEN LoadTableRows fehlgeschlagen"); }
    }

    /// <summary>
    /// Negativ-Test zur Unique-Durchsetzung: Zwei Zeilen desselben Chunks werden
    /// auf denselben Wert einer Unique-Spalte gesetzt — die zweite Änderung muss
    /// verweigert werden und den alten Wert erhalten (Unique gilt je Chunk).
    /// </summary>
    private void ActionUniqueVerletzung() {
        WartenAufSpeichern();
        var chunkSpalte = ChunkSpalte();
        var uniqueColumns = tbl.Column.ColumnsUsedInUniqueDefinition()
            .Where(c => c is { IsDisposed: false } && c != chunkSpalte && !c.KeyName.StartsWith("SYS_", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (uniqueColumns.Count == 0) { prot.Write("UNIQUE-NEG", "Keine Unique-Spalte vorhanden"); return; }

        var rows = tbl.Row.ToArray();

        if (rows.Length < 2) { prot.Write("UNIQUE-NEG", "Zu wenig Zeilen"); return; }

        // Zwei verschiedene Zeilen im selben Chunk suchen: Nur dann kollidiert die
        // Ziel-Änderung mit der Quelle über die ganze Unique-Definition.
        RowItem? quelle = null;
        RowItem? ziel = null;

        for (var versuch = 0; versuch < 6 && ziel is null; versuch++) {
            var a = rows[random.Next(rows.Length)];
            var b = rows[random.Next(rows.Length)];

            if (a.KeyName.Equals(b.KeyName, StringComparison.OrdinalIgnoreCase)) { continue; }
            if (chunkSpalte is { IsDisposed: false } && !string.Equals(a.CellGetString(chunkSpalte), b.CellGetString(chunkSpalte), StringComparison.OrdinalIgnoreCase)) { continue; }

            quelle = a;
            ziel = b;
        }

        if (quelle is null || ziel is null) { prot.Write("UNIQUE-NEG", "Keine Zeilenkombination im selben Chunk gefunden"); return; }

        var spalte = uniqueColumns[random.Next(uniqueColumns.Count)];
        var zielWert = ziel.CellGetString(spalte);
        var quellWert = quelle.CellGetString(spalte);

        if (string.Equals(zielWert, quellWert, StringComparison.Ordinal)) { prot.Write("UNIQUE-NEG", $"Spalte={spalte.KeyName} Quelle und Ziel tragen denselben Wert — Stichprobe übersprungen"); return; }

        var failed = ziel.CellSet(spalte, quellWert, $"StressTest {user} Unique-Verletzung");

        if (failed is { Length: > 0 }) {
            var erhalten = string.Equals(ziel.CellGetString(spalte), zielWert, StringComparison.Ordinal);

            prot.Write("UNIQUE-NEG", $"Key={ziel.KeyName} Quelle={quelle.KeyName} Spalte={spalte.KeyName} verweigert ({Logtext(failed)}){(erhalten ? ", alter Wert erhalten" : string.Empty)}");

            if (!erhalten) { prot.Write("FEHLER", $"UNIQUE-NEG Verweigerung hat den alten Wert nicht erhalten: Key={ziel.KeyName} Spalte={spalte.KeyName}"); }
            return;
        }

        // Nicht verweigert: Eine vollständige Kollision (alle Unique-Spalten, gleicher Chunk) darf nicht durchgehen.
        var kollision = uniqueColumns.TrueForAll(c => string.Equals(ziel.CellGetString(c), quelle.CellGetString(c), StringComparison.OrdinalIgnoreCase));

        if (kollision) {
            prot.Write("FEHLER", $"UNIQUE-NEG Verletzung akzeptiert: Key={ziel.KeyName} Quelle={quelle.KeyName} Spalten={string.Join(",", uniqueColumns.Select(c => c.KeyName))}");
        } else {
            prot.Write("UNIQUE-NEG", $"Key={ziel.KeyName} Spalte={spalte.KeyName} geändert — Definition kollidiert nicht vollständig, ok");
        }
    }

    /// <summary>
    /// Legt eine Zeile mit den Werten einer Zufallszeile an; Unique-Spalten erhalten neue Zufallswerte.
    /// </summary>
    private void ActionZeileDuplizieren() {
        WartenAufSpeichern();
        var rows = tbl.Row.ToArray();

        if (rows.Length == 0) { prot.Write("ZEILE-DUP", "Keine Zeile vorhanden"); return; }

        var quelle = rows[random.Next(rows.Length)];
        var uniqueColumns = tbl.Column.ColumnsUsedInUniqueDefinition();
        var chunkSpalte = ChunkSpalte();

        // Muss-Spalten-Ordnung wie beim Neuanlegen: Chunk-Spalte zuerst.
        List<FilterItem> filter = [];
        var ordered = tbl.Column.ToList();

        if (chunkSpalte is { IsDisposed: false } cvc) {
            ordered.Remove(cvc);
            ordered.Insert(0, cvc);
        }

        foreach (var thisColumn in ordered) {
            if (thisColumn is not { IsDisposed: false }) { continue; }
            if (thisColumn.KeyName.StartsWith("SYS_", StringComparison.OrdinalIgnoreCase)) { continue; }

            string wert;

            if (thisColumn == chunkSpalte) {
                // Die Chunk-Spalte ist per ColumnsIncludingChunk zugleich in den
                // Unique-Definitionen — sie muss VOR den Unique-Spalten geprüft
                // werden und erhält einen Pool-Wert.
                wert = ZufallsChunk();
            } else if (uniqueColumns.Contains(thisColumn)) {
                wert = RandomValue();
            } else {
                wert = quelle.CellGetString(thisColumn);
            }

            filter.Add(new FilterItem(thisColumn, FilterType.Istgleich, wert));
        }

        var opr = tbl.Row.GenerateAndAdd([.. filter], $"StressTest {user} Duplikat von {quelle.KeyName}");

        if (opr.IsFailed || opr.Value is not RowItem neu) {
            prot.Write("ZEILE-DUP", "Fehlgeschlagen: " + opr.FailedReason);
            return;
        }

        prot.Write("ZEILE-DUP", $"Quelle={quelle.KeyName} Neu={neu.KeyName} ok");
    }

    private void ActionZeileLoeschen() {
        WartenAufSpeichern();
        var rows = tbl.Row.ToArray();
        if (rows.Length == 0) { prot.Write("LÖSCHEN", "Keine Zeile vorhanden"); return; }

        var row = rows[random.Next(rows.Length)];
        var opr = RowCollection.Remove(row, $"StressTest {user} Zeile löschen");

        prot.Write("LÖSCHEN", opr.IsFailed ? $"Key={row.KeyName} FEHLER: {opr.FailedReason}" : $"Key={row.KeyName} ok");
    }

    /// <summary>
    /// Legt in einem Rutsch vier Zeilen an und löscht sie sofort wieder: übt das
    /// schnelle Anlegen und Löschen vor dem periodischen Speichern ab — die Sätze
    /// landen im Journal, ohne dass die Zeilen je einen Datenstand erreichen.
    /// </summary>
    private void ActionZeilenSchub() {
        WartenAufSpeichern();
        List<RowItem> angelegt = [];

        for (var i = 0; i < 4; i++) {
            var opr = NeueMussZeile($"StressTest {user} Zeilen-Schub {i + 1}");

            if (opr.IsFailed || opr.Value is not RowItem row) {
                prot.Write("ZEILEN-SCHUB", $"Anlegen {i + 1} fehlgeschlagen: {Logtext(opr.FailedReason)} BereitsAngelegt={angelegt.Count}");
                break;
            }

            angelegt.Add(row);
        }

        var gelöscht = 0;

        foreach (var row in angelegt) {
            var opr = RowCollection.Remove(row, $"StressTest {user} Zeilen-Schub löschen");

            if (opr.IsFailed) {
                prot.Write("FEHLER", $"ZEILEN-SCHUB Löschen fehlgeschlagen: Key={row.KeyName} {opr.FailedReason}");
                continue;
            }

            gelöscht++;
        }

        prot.Write("ZEILEN-SCHUB", $"Angelegt={angelegt.Count} Gelöscht={gelöscht} Keys={string.Join(",", angelegt.Select(r => r.KeyName))}");
    }

    /// <summary>
    /// Liest alle Zellen einer Zufallszeile zweimal: raucht die Lesepfade über die
    /// kritischen Werte ab und prüft, dass der Datenstand beim Lesen stabil bleibt.
    /// </summary>
    private void ActionZeileVollLesen() {
        var rows = tbl.Row.ToArray();

        if (rows.Length == 0) { prot.Write("ZEILE-LESEN", "Keine Zeile vorhanden"); return; }

        var row = rows[random.Next(rows.Length)];
        var columns = tbl.Column.Where(c => c is { IsDisposed: false }).ToList();

        if (columns.Count == 0) { prot.Write("ZEILE-LESEN", "Keine Spalte vorhanden"); return; }

        var erst = columns.ToDictionary(c => c.KeyName, c => row.CellGetString(c), StringComparer.OrdinalIgnoreCase);
        var zweit = columns.ToDictionary(c => c.KeyName, c => row.CellGetString(c), StringComparer.OrdinalIgnoreCase);
        var instabil = erst.Count(kv => !string.Equals(zweit[kv.Key], kv.Value, StringComparison.Ordinal));
        var längste = erst.OrderByDescending(kv => kv.Value.Length).First();

        if (instabil > 0) { prot.Write("FEHLER", $"ZEILE-LESEN {instabil} Zellen ändern sich beim erneuten Lesen: Key={row.KeyName}"); }

        prot.Write("ZEILE-LESEN", $"Key={row.KeyName} ChunkId={row.ChunkId()} Zellen={erst.Count} Zeichen={erst.Sum(kv => kv.Value.Length)} Längste={längste.Key}={längste.Value.Length} Instabil={instabil}");
    }

    private void ActionZelleAendern() {
        WartenAufSpeichern();
        var rows = tbl.Row.ToArray();
        if (rows.Length == 0) { prot.Write("ZELLE", "Keine Zeile vorhanden"); return; }

        // Systemspalten (SYS_) pflegt das System selbst und ist nie beschreibbar — für den Stresstest ausgenommen.
        var columns = tbl.Column.Where(c => c is { IsDisposed: false } && !c.KeyName.StartsWith("SYS_", StringComparison.OrdinalIgnoreCase)).ToArray();
        if (columns.Length == 0) { prot.Write("ZELLE", "Keine bearbeitbare Spalte vorhanden"); return; }

        var row = rows[random.Next(rows.Length)];
        var column = columns[random.Next(columns.Length)];
        var value = column == ChunkSpalte() ? ZufallsChunk() : RandomValue();
        var failed = row.CellSet(column, value, $"StressTest {user} Zelle ändern");

        prot.Write("ZELLE", failed is { Length: > 0 }
            ? $"Key={row.KeyName} Spalte={column.KeyName} FEHLER: {failed}"
            : $"Key={row.KeyName} Spalte={column.KeyName} Wert={value} ok");
    }

    /// <summary>
    /// Setzt einen kritischen Wert (Umbruch, Pipe, Unicode, leer, sehr lang) in eine Zufallszelle.
    /// </summary>
    private void ActionZelleSonderzeichen() {
        WartenAufSpeichern();
        var rows = tbl.Row.ToArray();

        if (rows.Length == 0) { prot.Write("ZELLE-SPEZIAL", "Keine Zeile vorhanden"); return; }

        // Systemspalten (SYS_) pflegt das System selbst und ist nie beschreibbar — für den Stresstest ausgenommen.
        var columns = tbl.Column.Where(c => c is { IsDisposed: false } && !c.KeyName.StartsWith("SYS_", StringComparison.OrdinalIgnoreCase)).ToArray();

        if (columns.Length == 0) { prot.Write("ZELLE-SPEZIAL", "Keine bearbeitbare Spalte vorhanden"); return; }

        var row = rows[random.Next(rows.Length)];
        var column = columns[random.Next(columns.Length)];
        var value = SonderWerte[random.Next(SonderWerte.Length)];
        var failed = row.CellSet(column, value, $"StressTest {user} Sonderzeichen");

        prot.Write("ZELLE-SPEZIAL", failed is { Length: > 0 }
            ? $"Key={row.KeyName} Spalte={column.KeyName} FEHLER: {failed}"
            : $"Key={row.KeyName} Spalte={column.KeyName} Wert={Logtext(value)} ok");
    }

    private void DoRandomAction() {
        if (tabellenVerlust) {
            tickCount++;

            // Herzschlag ohne Aktionen: Der Lauf bleibt beobachtbar, ohne die verlorene Tabelle zu berühren.
            if (tickCount % 15 == 0) { prot.Write("MELDUNG", "Tabelle nach NEU-LADEN verloren — Restlauf ohne Aktionen"); }
            return;
        }

        if (!freezeReported && tbl.IsFreezed) {
            freezeReported = true;
            prot.Write("FEHLER", $"Tabelle eingefroren: {tbl.FreezedReason}");
        }

        tickCount++;
        var value = random.Next(0, 50);
        var action = ActionOf(value);

        try {
            switch (action) {
                case StressAction.NeueZeile: ActionNeueZeile(); break;
                case StressAction.ZeileLoeschen: ActionZeileLoeschen(); break;
                case StressAction.ZelleAendern: ActionZelleAendern(); break;
                case StressAction.MasterSetzen: ActionMasterSetzen(); break;
                case StressAction.Komplettierung: ActionKomplettierung(); break;
                case StressAction.ScriptErstellen: ActionScriptErstellen(); break;
                case StressAction.AlleDatenLaden: ActionAlleDatenLaden(); break;
                case StressAction.SpalteAnlegen: ActionSpalteAnlegen(); break;
                case StressAction.SpalteLoeschen: ActionSpalteLoeschen(); break;
                case StressAction.ZelleSonderzeichen: ActionZelleSonderzeichen(); break;
                case StressAction.ZeileDuplizieren: ActionZeileDuplizieren(); break;
                case StressAction.SpalteMetadaten: ActionSpalteMetadaten(); break;
                case StressAction.SortierungPruefen: ActionSortierungPruefen(); break;
                case StressAction.DuplikatePruefen: ActionDuplikatePruefen(); break;
                case StressAction.ScriptLoeschen: ActionScriptLoeschen(); break;
                case StressAction.KonsistenzPruefen: ActionKonsistenzPruefen(); break;
                case StressAction.CsvExportPruefen: ActionCsvExportPruefen(); break;
                case StressAction.ChunkWechsel: ActionChunkWechsel(); break;
                case StressAction.SchreibPruefung: ActionSchreibPruefung(); break;
                case StressAction.UniqueVerletzung: ActionUniqueVerletzung(); break;
                case StressAction.FilterPruefung: ActionFilterPruefung(); break;
                case StressAction.ZeilenSchub: ActionZeilenSchub(); break;
                case StressAction.ZeileVollLesen: ActionZeileVollLesen(); break;
                case StressAction.TabelleNeuLaden: ActionTabelleNeuLaden(); break;
                case StressAction.ScriptAusfuehren: ActionScriptAusfuehren(); break;
                case StressAction.SpeicherStand: ActionSpeicherStand(); break;
                default: prot.Write("UNBEKANNT", "Zufallswert " + value); break;
            }
        } catch (Exception ex) {
            prot.Write("FEHLER", $"Aktion={action} Zufallswert={value} Typ={ex.GetType().Name} Meldung={ex.Message} Aufrufstack={Develop.DiagStack()}");
        }

        // Alle 15 Ticks die Änderungen bündeln und schreiben — so übersteht auch ein Watchdog-Kill.
        if (tickCount % 15 == 0) { FlushChanges("periodisch"); }
    }

    private int Finish() {
        if (tabellenVerlust) {
            prot.Write("ENDE", $"Tabelle verloren (NEU-LADEN) — Ticks={tickCount} Ende nach {runTime.TotalMinutes:0} Minuten");
            return 1;
        }

        FlushChanges("Programmende");
        WriteFragmentStatistik();
        prot.Write("ENDE", $"Zeilen={tbl.Row.Count} Spalten={tbl.Column.Count} Scripte={tbl.EventScript.Count} Ticks={tickCount} reguläres Ende nach {runTime.TotalMinutes:0} Minuten");
        tbl.Dispose();
        return 0;
    }

    private void FlushChanges(string reason) {
        try {
            RowCollection.InvalidatedRowsManager.DoAllInvalidatedRows(null, true, null);
            var opr = SaveTable();

            // Ein verwerfener oder durch eine Sperre ausgesetzter Versuch wird
            // einmal nachgezogen, bevor der Zustand bewertet wird.
            if (SaveRequiredAktiv()) {
                Thread.Sleep(3000);
                opr = SaveTable();
            }

            var ausständig = SaveRequiredAktiv();
            List<string> sperren = [];

            if (tbl is TableFileNew tfnn) { sperren = tfnn.Saveable.ForeignLockedChunkIds(); }
            else if (tbl is TableChunk tch) { sperren = tch.ForeignLockedChunkIds(); }
            var hinweis = ausständig
                ? sperren.Count > 0
                    ? " — SaveRequired bleibt aktiv (fremde Edit-Sperre: " + string.Join(",", sperren) + ", Daten im Kopf-Journal)"
                    : " — FEHLER: SaveRequired bleibt aktiv"
                : string.Empty;

            prot.Write("SPEICHERN", $"{reason}: {(opr.IsFailed ? "FEHLER: " + opr.FailedReason : "ok")}{hinweis}");

            if (ausständig && sperren.Count == 0) { prot.Write("FEHLER", $"SPEICHERN SaveRequired bleibt aktiv ohne erkennbare Sperre ({reason})"); }
        } catch (Exception ex) {
            prot.Write("FEHLER", $"SPEICHERN Typ={ex.GetType().Name} Meldung={ex.Message}");
        }
    }

    /// <summary>
    /// Wartet, bis kein eigener Speicher-Task läuft: unmittelbar vor Schreibaktionen
    /// aufgerufen, überbrückt es auch das bedarfsgesteuerte Speichern des Checkers —
    /// sonst erzeugt der Schreibzugriff nur ein Rollback mit Warnung.
    /// </summary>
    private void WartenAufSpeichern() {
        switch (tbl) {
            case TableFileNew tfn: tfn.Saveable.WaitDiskOperationFinished(); break;
            case TableFile tf: tf.WaitDiskOperationFinished(); break;
        }
    }

    /// <summary>
    /// True, wenn die Tabelle noch ungespeicherte Änderungen meldet.
    /// </summary>
    private bool SaveRequiredAktiv() {
        if (tbl is TableFileNew tfn) { return tfn.Saveable.SaveRequired; }
        if (tbl is TableFile tf) { return tf.SaveRequired; }
        return false;
    }

    /// <summary>
    /// Kürzt einen Wert fürs Protokoll und ersetzt das Trennzeichen der Protokollzeile.
    /// </summary>
    private static string Logtext(string text) => (text.Length > 80 ? text[..60] + "..." : text).Replace('|', '/');

    /// <summary>
    /// True, wenn die Master-Zeit (erste 2 Minuten) abgelaufen ist: Master setzen und Komplettierung laufen dann als Idle.
    /// </summary>
    private bool MasterIdle() => DateTime.UtcNow.Subtract(startUtc) >= masterTime;

    private string RandomValue() => $"{user}-{DateTime.Now:HHmmss.fff}-{random.Next(1000, 9999)}";

    /// <summary>
    /// Die Chunk-Spalte der Tabelle oder null, wenn keine vorhanden ist.
    /// </summary>
    private ColumnItem? ChunkSpalte() => tbl.Column.ChunkValueColumn is { IsDisposed: false } c ? c : null;

    /// <summary>
    /// Muss-Spalten-Wert beim Zeilen-Anlegen: die Chunk-Spalte erhält einen
    /// Pool-Wert, alles andere einen Zufallswert mit Benutzer-Stempel.
    /// </summary>
    private string MussWert(ColumnItem column, ColumnItem? chunkSpalte) => column == chunkSpalte ? ZufallsChunk() : RandomValue();

    /// <summary>
    /// Legt eine Zeile an und füllt die Muss-Spalten (Erstspalte, Chunk- und
    /// Unique-Spalten) per Istgleich-Filter; die Chunk-Spalte wird vor den
    /// Unique-Spalten geprüft. Rückgabe ist das Ergebnis mit der neuen Zeile.
    /// </summary>
    private OperationResult NeueMussZeile(string kommentar) {
        var chunkSpalte = ChunkSpalte();
        var ordered = tbl.Column.ToList();

        if (chunkSpalte is { IsDisposed: false } cvc) {
            ordered.Remove(cvc);
            ordered.Insert(0, cvc);
        }

        var uniqueColumns = tbl.Column.ColumnsUsedInUniqueDefinition();
        List<FilterItem> filter = [];

        foreach (var thisColumn in ordered) {
            if (thisColumn is not { IsDisposed: false }) { continue; }
            if (thisColumn.IsFirst || thisColumn.Value_for_Chunk != ChunkType.None || uniqueColumns.Contains(thisColumn)) {
                filter.Add(new FilterItem(thisColumn, FilterType.Istgleich, MussWert(thisColumn, chunkSpalte)));
            }
        }

        return tbl.Row.GenerateAndAdd([.. filter], kommentar);
    }

    /// <summary>
    /// Zufälliger Chunk-Pool-Wert, möglichst verschieden vom bisherigen Wert.
    /// </summary>
    private string ZufallsChunk(string bisher) {
        var neu = ChunkWerte[random.Next(ChunkWerte.Length)];

        if (neu.Equals(bisher, StringComparison.OrdinalIgnoreCase)) { neu = ChunkWerte[(Array.IndexOf(ChunkWerte, neu) + 1) % ChunkWerte.Length]; }

        return neu;
    }

    /// <summary>
    /// Zufälliger Chunk-Pool-Wert.
    /// </summary>
    private string ZufallsChunk() => ChunkWerte[random.Next(ChunkWerte.Length)];

    /// <summary>
    /// Startet den 2-Sekunden-Timer und läuft 10 Minuten; Rückgabe ist der Exit-Code.
    /// </summary>
    public int Run() {
        startUtc = DateTime.UtcNow;
        prot.Write("START", $"Laufzeit={runTime.TotalMinutes:0}min Masterzeit={masterTime.TotalMinutes:0}min Zeilen={tbl.Row.Count} Spalten={tbl.Column.Count} Master={tbl.TemporaryTableMasterUser}");

        timer = new System.Threading.Timer(TimerTick, null, 2000, 2000);

        while (DateTime.UtcNow.Subtract(startUtc) < runTime) {
            if (!tickSignal.WaitOne(1000)) { continue; }
            DoRandomAction();
        }

        StopTimer();
        return Finish();
    }

    private OperationResult SaveTable() {
        if (tbl is TableFileNew tfn) {
            var r = tfn.Save();
            tfn.Saveable.WaitDiskOperationFinished();
            return r;
        }

        if (tbl is TableFile tf) {
            var r = tf.Save();
            tf.WaitDiskOperationFinished();
            return r;
        }

        return OperationResult.Success;
    }

    private void StopTimer() {
        if (timer is not { } t) { return; }
        _ = t.Change(Timeout.Infinite, Timeout.Infinite);
        t.Dispose();
        timer = null;
    }

    /// <summary>
    /// Nicht veränderte Stress-Spalten (STRESS_-Präfix) der Tabelle.
    /// </summary>
    private IEnumerable<ColumnItem> StressSpalten() => tbl.Column.Where(c => c is { IsDisposed: false } && c.KeyName.StartsWith("STRESS_", StringComparison.OrdinalIgnoreCase));

    private void TimerTick(object? state) {
        _ = state;
        tickSignal.Set();
    }

    /// <summary>
    /// Protokolliert die Fragment-Dateien im Arbeitsverzeichnis: Kopf-Snapshots,
    /// Journale sowie die System- und Daten-Ordner der Tabelle (je Ordner die
    /// Datei-Anzahl — so ist die Chunk-Verteilung im Protokoll nachweisbar).
    /// </summary>
    private void WriteFragmentStatistik() {
        if (tbl is not TableFileNew tfn || tfn.Filename is not { Length: > 0 } dateiName) { return; }

        var basis = dateiName.FilePath() + dateiName.FileNameWithoutSuffix() + "\\";
        if (!Directory.Exists(basis)) { return; }

        var kopfDateien = Directory.GetFiles(basis, "*", SearchOption.TopDirectoryOnly);
        var snaps = 0;
        var journale = 0;

        // Kopf-Snapshots und -Journale in beiden Formaten zählen: neu (.snp/.frg) und Legacy (.tbls/.tblf).
        foreach (var f in kopfDateien) {
            switch (f.FileSuffix().ToLowerInvariant()) {
                case "snp" or "tbls": snaps++; break;
                case "frg" or "tblf": journale++; break;
            }
        }

        var (zeilenOrdnerGesamt, zeilenOrdnerLeer, systemOrdner, datenOrdner) = OrdnerStatistik(basis);

        var detail = zeilenOrdnerGesamt > 0 ? $" Zeilenordner={zeilenOrdnerGesamt} Leer={zeilenOrdnerLeer}" : string.Empty;
        var datenDetail = datenOrdner.Count > 0 ? " Daten: " + string.Join(" ", [.. datenOrdner.Take(40)]) : string.Empty;

        prot.Write("FRAGMENTE", $"Layout=ChunkFragment Snapshots={snaps} Journale={journale}{detail} " + string.Join(" ", systemOrdner) + datenDetail);
    }

    /// <summary>
    /// Zählt die Chunk-Ordner: System-Ordner („_“-Präfix) je Datei-Anzahl,
    /// Daten-Ordner gesamt, leer und je Datei-Anzahl.
    /// </summary>
    private static (int Gesamt, int Leer, List<string> SystemOrdner, List<string> DatenOrdner) OrdnerStatistik(string basis) {
        List<string> systemOrdner = [];
        List<string> datenOrdner = [];
        var zeilenOrdnerGesamt = 0;
        var zeilenOrdnerLeer = 0;

        foreach (var dir in Directory.GetDirectories(basis)) {
            var name = Path.GetFileName(dir.TrimEnd(Path.DirectorySeparatorChar));
            var dateien = Directory.GetFiles(dir, "*", SearchOption.AllDirectories);

            if (name.Length > 0 && name[0] == '_') {
                systemOrdner.Add($"{name}={dateien.Length}");
                continue;
            }

            zeilenOrdnerGesamt++;
            datenOrdner.Add($"{name}={dateien.Length}");

            if (dateien.Length == 0) { zeilenOrdnerLeer++; }
        }

        systemOrdner.Sort(StringComparer.Ordinal);
        datenOrdner.Sort(StringComparer.Ordinal);
        return (zeilenOrdnerGesamt, zeilenOrdnerLeer, systemOrdner, datenOrdner);
    }

    #endregion
}

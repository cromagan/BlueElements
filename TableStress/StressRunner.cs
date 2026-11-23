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
        // Muss-Spalten (Erstspalte, Chunk- und Unique-Spalten) per Istgleich-Filter setzen.
        List<FilterItem> filter = [];
        var chunkSpalte = ChunkSpalte();
        var ordered = tbl.Column.ToList();

        if (chunkSpalte is { IsDisposed: false } cvc) {
            ordered.Remove(cvc);
            ordered.Insert(0, cvc);
        }

        var uniqueColumns = tbl.Column.ColumnsUsedInUniqueDefinition();

        foreach (var thisColumn in ordered) {
            if (thisColumn is not { IsDisposed: false }) { continue; }
            if (thisColumn.IsFirst || thisColumn.Value_for_Chunk != ChunkType.None || uniqueColumns.Contains(thisColumn)) {
                filter.Add(new FilterItem(thisColumn, FilterType.Istgleich, MussWert(thisColumn, chunkSpalte)));
            }
        }

        var opr = tbl.Row.GenerateAndAdd([.. filter], $"StressTest {user} Neue Zeile");

        if (opr.IsFailed || opr.Value is not RowItem row) {
            prot.Write("NEUEZEILE", "Fehlgeschlagen: " + opr.FailedReason);
            return;
        }

        prot.Write("NEUEZEILE", $"Key={row.KeyName} ok");
    }

    /// <summary>
    /// Bildet den Zufallswert 0-39 auf die Aktion ab.
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
            default: return StressAction.ChunkWechsel;
        }
    }

    private void ActionScriptErstellen() {
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
        var spalten = StressSpalten().ToList();

        if (spalten.Count == 0) { prot.Write("SPALTE-WEG", "Keine Stress-Spalte vorhanden"); return; }

        var column = spalten[0];
        var ok = tbl.Column.Remove(column, $"StressTest {user} Spalte löschen");

        prot.Write("SPALTE-WEG", ok ? $"Key={column.KeyName} ok" : $"Key={column.KeyName} FEHLER: Remove verweigert");
    }

    private void ActionSpalteMetadaten() {
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
    /// Legt eine Zeile mit den Werten einer Zufallszeile an; Unique-Spalten erhalten neue Zufallswerte.
    /// </summary>
    private void ActionZeileDuplizieren() {
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
        var rows = tbl.Row.ToArray();
        if (rows.Length == 0) { prot.Write("LÖSCHEN", "Keine Zeile vorhanden"); return; }

        var row = rows[random.Next(rows.Length)];
        var opr = RowCollection.Remove(row, $"StressTest {user} Zeile löschen");

        prot.Write("LÖSCHEN", opr.IsFailed ? $"Key={row.KeyName} FEHLER: {opr.FailedReason}" : $"Key={row.KeyName} ok");
    }

    private void ActionZelleAendern() {
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
        if (!freezeReported && tbl.IsFreezed) {
            freezeReported = true;
            prot.Write("FEHLER", $"Tabelle eingefroren: {tbl.FreezedReason}");
        }

        tickCount++;
        var value = random.Next(0, 40);
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
                default: prot.Write("UNBEKANNT", "Zufallswert " + value); break;
            }
        } catch (Exception ex) {
            prot.Write("FEHLER", $"Aktion={action} Zufallswert={value} Typ={ex.GetType().Name} Meldung={ex.Message} Aufrufstack={Develop.DiagStack()}");
        }

        // Alle 15 Ticks die Änderungen bündeln und schreiben — so übersteht auch ein Watchdog-Kill.
        if (tickCount % 15 == 0) { FlushChanges("periodisch"); }
    }

    private int Finish() {
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
            prot.Write("SPEICHERN", $"{reason}: {(opr.IsFailed ? "FEHLER: " + opr.FailedReason : "ok")}");
        } catch (Exception ex) {
            prot.Write("FEHLER", $"SPEICHERN Typ={ex.GetType().Name} Meldung={ex.Message}");
        }
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
        if (tbl is TableFileNew tfn) { return tfn.Save(); }
        if (tbl is TableFile tf) { return tf.Save(); }
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
        var snaps = kopfDateien.Count(f => f.ToLowerInvariant().EndsWith(".tbls", StringComparison.Ordinal));
        var journale = kopfDateien.Count(f => f.ToLowerInvariant().EndsWith(".tblf", StringComparison.Ordinal));

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

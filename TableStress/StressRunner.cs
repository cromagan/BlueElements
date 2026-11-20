// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Diagnostics;
using System.Threading;

namespace TableStress;

/// <summary>
/// Führt den Stresstest aus: alle 2 Sekunden eine Zufalls-Aktion auf der Tabelle, 5 Minuten lang.
/// </summary>
public sealed class StressRunner(Table tbl, StressProtocol prot, string user) {

    #region Fields

    private readonly Random random = new();
    private readonly TimeSpan runTime = TimeSpan.FromMinutes(10);

    /// <summary>
    /// Master setzen und Komplettierung anstoßen sind nur in dieser Anfangszeit erlaubt; danach ist der Tick Idle.
    /// </summary>
    private readonly TimeSpan masterTime = TimeSpan.FromMinutes(2);
    private readonly AutoResetEvent tickSignal = new(false);
    private DateTime startUtc;
    private bool freezeReported;
    private int scriptCount;
    private System.Threading.Timer? timer;
    private int tickCount;

    #endregion

    #region Methods

    /// <summary>
    /// Startet den 2-Sekunden-Timer und läuft 5 Minuten; Rückgabe ist der Exit-Code.
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

    /// <summary>
    /// Bildet den Zufallswert 0-26 auf die Aktion ab: 0-3 neue Zeile, 4 löschen, 5-20 Zelle, 21 Master, 22 Komplettierung, 23 Script, 24-26 alle Daten laden.
    /// </summary>
    private static StressAction ActionOf(int value) {
        switch (value) {
            case <= 3: return StressAction.NeueZeile;
            case 4: return StressAction.ZeileLoeschen;
            case <= 20: return StressAction.ZelleAendern;
            case 21: return StressAction.MasterSetzen;
            case 22: return StressAction.Komplettierung;
            case 23: return StressAction.ScriptErstellen;
            default: return StressAction.AlleDatenLaden;
        }
    }

    private void ActionAlleDatenLaden() {
        var ok = tbl.LoadTableRows(false, -1);
        prot.Write("DATENLADEN", ok ? $"Alle Daten geladen, Zeilen={tbl.Row.Count}" : "Nachladen fehlgeschlagen");
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

        prot.Write("KOMPLETTIERUNG", $"Refresh={(refreshed ? "ok" : "fehlgeschlagen")} Master={tbl.TemporaryTableMasterUser} KomplettierungFällig={tbl.MasterNeeded}");
    }

    private void ActionMasterSetzen() {
        if (MasterIdle()) {
            prot.Write("MASTER", "Idle — Master setzen ist nur in den ersten 2 Minuten erlaubt");
            return;
        }

        tbl.BeSureToBeUpToDate(false);
        tbl.MasterMe();
        prot.Write("MASTER", $"Master={tbl.TemporaryTableMasterUser} ZeitUtc={tbl.TemporaryTableMasterTimeUtc} Rechner={tbl.TemporaryTableMasterMachine}");
    }

    private void ActionNeueZeile() {
        // Muss-Spalten (Erstspalte, Chunk- und Unique-Spalten) per Istgleich-Filter setzen.
        List<FilterItem> filter = [];
        var ordered = tbl.Column.ToList();

        if (tbl.Column.ChunkValueColumn is { IsDisposed: false } cvc) {
            ordered.Remove(cvc);
            ordered.Insert(0, cvc);
        }

        var uniqueColumns = tbl.Column.ColumnsUsedInUniqueDefinition();

        foreach (var thisColumn in ordered) {
            if (thisColumn is not { IsDisposed: false }) { continue; }
            if (thisColumn.IsFirst || thisColumn.Value_for_Chunk != ChunkType.None || uniqueColumns.Contains(thisColumn)) {
                filter.Add(new FilterItem(thisColumn, FilterType.Istgleich, RandomValue()));
            }
        }

        var opr = tbl.Row.GenerateAndAdd([.. filter], $"StressTest {user} Neue Zeile");

        if (opr.IsFailed || opr.Value is not RowItem row) {
            prot.Write("NEUEZEILE", "Fehlgeschlagen: " + opr.FailedReason);
            return;
        }

        prot.Write("NEUEZEILE", $"Key={row.KeyName} ok");
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

    private void ActionZelleAendern() {
        var rows = tbl.Row.ToArray();
        if (rows.Length == 0) { prot.Write("ZELLE", "Keine Zeile vorhanden"); return; }

        // Systemspalten (SYS_) pflegt das System selbst und ist nie beschreibbar — für den Stresstest ausgenommen.
        var columns = tbl.Column.Where(c => c is { IsDisposed: false } && !c.KeyName.StartsWith("SYS_", StringComparison.OrdinalIgnoreCase)).ToArray();
        if (columns.Length == 0) { prot.Write("ZELLE", "Keine bearbeitbare Spalte vorhanden"); return; }

        var row = rows[random.Next(rows.Length)];
        var column = columns[random.Next(columns.Length)];
        var value = RandomValue();
        var failed = row.CellSet(column, value, $"StressTest {user} Zelle ändern");

        prot.Write("ZELLE", failed is { Length: > 0 }
            ? $"Key={row.KeyName} Spalte={column.KeyName} FEHLER: {failed}"
            : $"Key={row.KeyName} Spalte={column.KeyName} Wert={value} ok");
    }

    private void ActionZeileLoeschen() {
        var rows = tbl.Row.ToArray();
        if (rows.Length == 0) { prot.Write("LÖSCHEN", "Keine Zeile vorhanden"); return; }

        var row = rows[random.Next(rows.Length)];
        var opr = RowCollection.Remove(row, $"StressTest {user} Zeile löschen");

        prot.Write("LÖSCHEN", opr.IsFailed ? $"Key={row.KeyName} FEHLER: {opr.FailedReason}" : $"Key={row.KeyName} ok");
    }

    private void DoRandomAction() {
        if (!freezeReported && tbl.IsFreezed) {
            freezeReported = true;
            prot.Write("FEHLER", $"Tabelle eingefroren: {tbl.FreezedReason}");
        }

        tickCount++;
        var value = random.Next(0, 27);
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
        prot.Write("ENDE", $"Zeilen={tbl.Row.Count} Scripte={tbl.EventScript.Count} Ticks={tickCount} reguläres Ende nach {runTime.TotalMinutes:0} Minuten");
        tbl.Dispose();
        return 0;
    }

    /// <summary>
    /// True, wenn die Master-Zeit (erste 2 Minuten) abgelaufen ist: Master setzen und Komplettierung laufen dann als Idle.
    /// </summary>
    private bool MasterIdle() => DateTime.UtcNow.Subtract(startUtc) >= masterTime;

    private void FlushChanges(string reason) {
        try {
            RowCollection.InvalidatedRowsManager.DoAllInvalidatedRows(null, true, null);
            var opr = SaveTable();
            prot.Write("SPEICHERN", $"{reason}: {(opr.IsFailed ? "FEHLER: " + opr.FailedReason : "ok")}");
        } catch (Exception ex) {
            prot.Write("FEHLER", $"SPEICHERN Typ={ex.GetType().Name} Meldung={ex.Message}");
        }
    }

    private string RandomValue() => $"{user}-{DateTime.Now:HHmmss.fff}-{random.Next(1000, 9999)}";

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

    private void TimerTick(object? state) {
        _ = state;
        tickSignal.Set();
    }

    #endregion
}

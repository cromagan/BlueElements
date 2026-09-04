// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;
using BlueTable.Interfaces;

namespace BlueControls.ControlStrategies;

/// <summary>
/// Zeigt nichts an: Ein einfacher Klick in die Zelle führt das gewählte
/// Zeilenskript der Tabelle sofort aus. Ein Doppelklick bleibt wirkungslos.
/// </summary>
public class ScriptExecuteControlStrategy : ControlStrategy, IHasColumn {

    #region Fields

    private const string _scriptKey = "script";

    #endregion

    #region Properties

    public static string ClassId => "scriptexecute";

    /// <summary>
    /// Die Spalte, deren Tabelle die Zeilenskripte zur Auswahl stellt.
    /// Wird vom Spalten-Editor bzw. beim Klick gesetzt.
    /// </summary>
    public ColumnItem? Column { get; set; }

    public override string Description => "Zeigt nichts an: Ein Klick in die Zelle führt sofort das gewählte Zeilenskript aus.";

    public override bool IsInstantAction => true;

    public override string KeyName => ClassId;

    /// <summary>
    /// Die Tabelle der Spalte; null, wenn keine gültige Spalte gesetzt ist.
    /// </summary>
    public Table? Table => Column is { IsDisposed: false } column ? column.Table : null;

    /// <summary>
    /// Das bei Klick auszuführende Zeilenskript (KeyName).
    /// </summary>
    public string ScriptName {
        get;
        set {
            if (IsDisposed || field == value) { return; }
            field = value;

            ControlStrategyParameter.Set(_scriptKey, value);
        }
    } = string.Empty;

    protected override System.Windows.Forms.Control? ControlCore => null;

    #endregion

    #region Methods

    /// <summary>
    /// Meldung, warum die Konfiguration ungültig ist — insbesondere, wenn
    /// das gewählte Skript in der Tabelle nicht vorhanden ist.
    /// </summary>
    public override string ErrorReason() {
        if (Table is not { IsDisposed: false } tb) { return string.Empty; }
        if (ScriptName is not { Length: > 0 }) { return "Kein Skript ausgewählt."; }
        if (tb.EventScript.GetByKey(ScriptName, StringComparison.OrdinalIgnoreCase) is null) { return $"Das Skript '{ScriptName}' ist in der Tabelle nicht vorhanden."; }
        return string.Empty;
    }

    public override List<GenericControl> GetProperties(int widthOfControl)
        => [new FlexiControlForProperty<string>(() => ScriptName, "Skript", ScriptItems())];

    public override string ReadableText() => "Tabellen-Skript-Knopf";

    public override void SubscribeEvents() { }

    public override QuickImage? SymbolForReadableText() => QuickImage.Get(ImageCode.Skript);

    public override void UnsubscribeEvents() { }

    protected override void ApplyStyle() { }

    protected override void CreateControlCore() { }

    /// <summary>
    /// Führt das gewählte Zeilenskript für die angeklickte Zeile aus.
    /// </summary>
    protected override void ExecuteInstantAction(ColumnItem column, RowItem row) {
        if (column.Table is not { IsDisposed: false } tb) { return; }

        if (ScriptName is not { Length: > 0 } scriptName) {
            TableView.NotEditableInfo("Kein Skript ausgewählt.");
            return;
        }

        var sc = tb.EventScript.GetByKey(scriptName, StringComparison.OrdinalIgnoreCase);
        if (sc is null || sc.Table is not { IsDisposed: false }) {
            TableView.NotEditableInfo($"Das Skript '{scriptName}' ist in der Tabelle nicht vorhanden.");
            return;
        }

        TableView.DoScript([row], false, sc, sc.KeyName);
    }

    protected override void ForceWriteBackValue() { }

    protected override void ReadParameters(JsonObject json) => ScriptName = json.GetString(_scriptKey, ScriptName);

    protected override void SetValueToControlInternal(string value) { }

    /// <summary>
    /// Alle Zeilenskripte der Tabelle als Auswahl für die Combobox.
    /// </summary>
    private List<ListItem> ScriptItems() {
        var items = new List<ListItem>();
        if (Table is not { IsDisposed: false } tb) { return items; }

        foreach (var script in tb.EventScript) {
            if (script is { IsDisposed: false, NeedRow: true }) { items.Add(ItemOf(script)); }
        }

        return items;
    }

    #endregion
}
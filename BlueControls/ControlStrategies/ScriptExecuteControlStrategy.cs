// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;
using BlueScript.Classes;
using BlueTable.Interfaces;

namespace BlueControls.ControlStrategies;

/// <summary>
/// Zeigt nichts an: Ein einfacher Klick in die Zelle führt das direkt
/// hinterlegte Skript sofort aus. Ein Doppelklick bleibt wirkungslos.
/// </summary>
public class ScriptExecuteControlStrategy : ControlStrategy, IHasColumn {

    #region Fields

    private const string _scriptKey = "script";

    private FlexiControlForDelegate? _button;

    #endregion

    #region Properties

    public static string ClassId => "scriptexecute";

    /// <summary>
    /// Die Spalte, deren Tabelle als Skript-Kontext dient.
    /// Wird vom Spalten-Editor bzw. beim Klick gesetzt.
    /// </summary>
    public ColumnItem? Column { get; set; }

    public override string Description => "Zeigt nichts an: Ein Klick in die Zelle führt sofort das hinterlegte Skript aus.";

    public override bool IsInstantAction => true;

    public override string KeyName => ClassId;

    /// <summary>
    /// Das Skript, das beim Anklicken der Zelle ausgeführt wird.
    /// </summary>
    public string Script {
        get;
        set {
            if (IsDisposed || field == value) { return; }
            field = value;

            ControlStrategyParameter.Set(_scriptKey, value);
        }
    } = string.Empty;

    /// <summary>
    /// Die Tabelle der Spalte; null, wenn keine gültige Spalte gesetzt ist.
    /// </summary>
    public Table? Table => Column is { IsDisposed: false } column ? column.Table : null;

    protected override System.Windows.Forms.Control? ControlCore => null;

    #endregion

    #region Methods

    /// <summary>
    /// Meldung, warum die Konfiguration ungültig ist — insbesondere, wenn kein Skript hinterlegt ist.
    /// </summary>
    public override string ErrorReason() {
        if (Table is not { IsDisposed: false }) { return string.Empty; }
        if (Script is not { Length: > 0 }) { return "Kein Skript angegeben."; }
        return string.Empty;
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<GenericControl> result = [.. base.GetProperties(widthOfControl)];

        _button = new FlexiControlForDelegate(OpenScriptEditor, "Skript Editor", ImageCode.Skript);
        result.Add(_button);
        result.Add(new FlexiControlForProperty<string>(() => Script, 3));

        return result;
    }

    public override string ReadableText() => "Tabellen-Skript-Knopf";

    public override void SubscribeEvents() { }

    public override QuickImage? SymbolForReadableText() => QuickImage.Get(ImageCode.Skript);

    public override void UnsubscribeEvents() { }

    protected override void ApplyStyle() { }

    protected override void CreateControlCore() { }

    /// <summary>
    /// Führt das hinterlegte Skript für die angeklickte Zeile aus und schreibt die Variablen zurück.
    /// </summary>
    protected override void ExecuteInstantAction(ColumnItem column, RowItem row) {
        if (column.Table is not { IsDisposed: false } tb) { return; }

        if (Script is not { Length: > 0 }) {
            TableView.NotEditableInfo("Kein Skript angegeben.");
            return;
        }

        var rowstamp = row.RowStamp();

        var t = ScriptButtonPadItem.ExecuteScript(Script, "Standard", true, null, row, tb, null, null);

        var errorreason = string.Empty;

        if (row.RowStamp() != rowstamp) { errorreason = "Die Zeile wurde während des Ausführens verändert."; }
        if (t.Failed) { errorreason = t.ProtocolText; }

        if (string.IsNullOrEmpty(errorreason) && t.Variables is { } vars) {
            tb.WriteBackVariables(row, vars, false, true, "Tabellen-Skript-Knopf", !t.Failed);
        } else {
            Forms.MessageBox.Show($"Dieser Knopfdruck wurde nicht komplett ausgeführt.\r\n\r\nGrund:\r\n{errorreason}", ImageCode.Kritisch, "Ok");
        }
    }

    protected override void ForceWriteBackValue() { }

    protected override void ReadParameters(JsonObject json) => Script = json.GetString(_scriptKey, Script);

    protected override void SetValueToControlInternal(string value) { }

    /// <summary>
    /// Öffnet den Skript-Editor für das hinterlegte Skript.
    /// </summary>
    public void OpenScriptEditor() {
        var f = _button?.ParentForm;

        f?.Opacity = 0f;

        try {
            var sd = new ScriptDescription(KeyName, Script);

            sd.ExecuteScript = ExecuteScriptTest;

            if (InputBoxEditor.Edit(sd)) {
                Script = sd.Script;
            }
        } finally {
            f?.Opacity = 1f;
        }
    }

    /// <summary>
    /// Führt das Skript für den Testmodus im Editor mit dem Tabellenkontext der Spalte aus.
    /// </summary>
    private ScriptEndedFeedback ExecuteScriptTest(string script, bool testmode) =>
        ScriptButtonPadItem.ExecuteScript(script, "Testmodus", !testmode, null, null, Table, null, null);

    #endregion
}

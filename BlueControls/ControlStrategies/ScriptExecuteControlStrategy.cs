// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueControls.Controls;
using BlueScript.Classes;
using BlueScript.Interfaces;
using BlueScript.ScriptVariables;
using System.Globalization;

namespace BlueControls.ControlStrategies;

/// <summary>
/// Zeigt nichts an: Ein einfacher Klick in die Zelle führt das direkt
/// hinterlegte Skript sofort aus. Ein Doppelklick bleibt wirkungslos.
/// </summary>
public class ScriptExecuteControlStrategy : ControlStrategy, IHasScript, INotifyPropertyChanged {

    #region Fields

    private const string _readOnlytKey = "ReadOnly";

    private const string _scriptKey = "script";

    private FlexiControlForDelegate? _button;

    private FlexiControlForProperty<string>? _scriptField;

    #endregion

    #region Events

    /// <summary>
    /// Meldet Script-Änderungen an gebundene Editoren (z. B. FlexiControlForProperty),
    /// damit deren Anzeige nach dem Schließen des Skript-Editors aktualisiert wird.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion

    #region Properties

    public static string ClassId => "scriptexecute";

    public override bool IsInstantAction => true;

    public override string KeyName => ClassId;

    /// <summary>
    /// Das Fenster, in dem die Zelle geklickt wurde; null im Testmodus.
    /// Wird vom Klick-Kontext (ControlStrategy.InstantActionClicked) gesetzt.
    /// </summary>
    public System.Windows.Forms.Form? OwnerForm { get; set; }

    /// <summary>
    /// Gibt an, ob das Script als ReadOnly ausgeführt wird.
    /// Dadurch werden Fehlermeldungen unterdrpckt, falls die Zeile während der bearbeitung verändert wurde.
    /// </summary>
    public bool ReadOnly {
        get;
        set {
            if (IsDisposed || field == value) { return; }
            field = value;

            ControlStrategyParameter.Set(_readOnlytKey, value);
            OnPropertyChanged(nameof(ReadOnly));
        }
    }

    /// <summary>
    /// Das Skript, das beim Anklicken der Zelle ausgeführt wird.
    /// </summary>
    public string Script {
        get;
        set {
            if (IsDisposed || field == value) { return; }
            field = value;

            ControlStrategyParameter.Set(_scriptKey, value);
            OnPropertyChanged(nameof(Script));
        }
    } = string.Empty;

    protected override System.Windows.Forms.Control? ControlCore => null;

    #endregion

    #region Methods

    /// <summary>
    /// Führt das Skript aus. Mit Zeile kommen alle Zeilen-Variablen zum Skript
    /// und werden bei Erfolg im Produktivmodus zurückgeschrieben.
    /// </summary>
    public static ScriptEndedFeedback ExecuteScript(string scripttext, bool produktiv, bool readOnly, RowItem? row, string? fensterId) {
        VariableCollection generatedVars =
        [
            new StringScriptVariable("Application", Develop.AppName(), true, "Der Name der App, die gerade geöffnet ist."),
            new StringScriptVariable("User", UserName, true, "ACHTUNG: Keinesfalls dürfen benutzerabhängig Werte verändert werden."),
            new StringScriptVariable("Usergroup", UserGroup, true, "ACHTUNG: Keinesfalls dürfen gruppenabhängig Werte verändert werden."),
            new RowScriptVariable("RowEmpty", null, true, "Dummy Zeile ohne Inhalt"),
            new StringScriptVariable("WindowID", fensterId ?? string.Empty, true, "Die ID des Fensters, aus dem das Skript gestartet wurde. Im Script Editor leer.")
        ];

        if (row?.Table is { IsDisposed: false } rowTb) {
            generatedVars.AddRange(rowTb.CreateVariableCollection(row, readOnly, false, false, true, null));
        }

        var scp = new ScriptProperties("ScriptButton", ScriptCommand.AllMethods.Instances, produktiv, [], row, "ScriptExecuteControlStrategy", "ScriptExecuteControlStrategy in Formular");

        var sc = new Script(generatedVars, scp) {
            ScriptText = scripttext
        };

        var rowstamp = row?.RowStamp();

        var t = sc.Parse(0, "Main", null);

        if (!t.Failed && !readOnly && produktiv && t.Variables is { } vars) {
            if (row?.RowStamp() != rowstamp) {
                return new ScriptEndedFeedback(vars, "Die Zeile wurde während des Ausführens verändert.");
            }
            if (row?.Table is { IsDisposed: false } wtb) {
                wtb.WriteBackVariables(row, vars, false, false, "Skript ausführen", true);
            }
        }

        return t;
    }

    /// <summary>
    /// Meldung, warum die Konfiguration ungültig ist — insbesondere, wenn kein Skript hinterlegt ist.
    /// </summary>
    public override string ErrorReason() => Script is { Length: > 0 } ? string.Empty : "Kein Skript angegeben.";

    /// <summary>
    /// IHasScript: Liefert das Klick-Skript.
    /// </summary>
    public IEnumerable<ScriptDescription> GetAllScripts() {
        if (Script is not { Length: > 0 } s) { return []; }
        return [new ScriptDescription(KeyName, s)];
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<GenericControl> result = [.. base.GetProperties(widthOfControl)];

        _button = new FlexiControlForDelegate(OpenScriptEditor, "Skript Editor", ImageCode.Skript);
        result.Add(_button);
        _scriptField = new FlexiControlForProperty<string>(() => Script, 3);
        result.Add(_scriptField);
        result.Add(new FlexiControlForProperty<bool>(() => ReadOnly, "Variablen schreibgeschützt"));

        return result;
    }

    /// <summary>
    /// Öffnet den Skript-Editor für das hinterlegte Skript.
    /// </summary>
    public void OpenScriptEditor() {
        var f = _button?.ParentForm;

        f?.Opacity = 0f;

        try {
            // Das kleine Textfeld meldet Änderungen verzögert (RaiseChangeDelay);
            // der aktuelle Stand wird deshalb direkt übernommen, bevor der Editor geöffnet wird.
            if (_scriptField?.Strategy.Control is TextBox txb) {
                Script = txb.Text;
            }

            var sd = new ScriptDescription(KeyName, Script);

            sd.ExecuteScript = ExecuteScriptTest;

            if (InputBoxEditor.Edit(sd)) {
                Script = sd.Script;
            }
        } finally {
            f?.Opacity = 1f;
        }
    }

    public override string ReadableText() => "Skript ausführen";

    public override void SubscribeEvents() { }

    public override QuickImage? SymbolForReadableText() => QuickImage.Get(ImageCode.Skript);

    public override void UnsubscribeEvents() { }

    protected override void ApplyStyle() { }

    protected override void CreateControlCore() { }

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);
        if (disposing) {
            PropertyChanged = null;
        }
    }

    /// <summary>
    /// Führt das hinterlegte Skript für die angeklickte Zeile aus.
    /// Das Zurückschreiben der Zeilen-Variablen erfolgt bei Erfolg in <see cref="ExecuteScript"/>.
    /// </summary>
    protected override void ExecuteInstantAction(ColumnItem column, RowItem row) {
        if (column.Table is not { IsDisposed: false }) { return; }

        if (Script is not { Length: > 0 }) {
            TableView.NotEditableInfo("Kein Skript angegeben.");
            return;
        }

        var fensterId = OwnerForm is { IsDisposed: false } owner ? owner.Handle.ToString(CultureInfo.InvariantCulture) : null;

        var t = ExecuteScript(Script, true, ReadOnly, row, fensterId);

        if (t.Failed) {
            Forms.MessageBox.Show($"Dieser Knopfdruck wurde nicht komplett ausgeführt.\r\n\r\nGrund:\r\n{t.ProtocolText}", ImageCode.Kritisch, "Ok");
        }
    }

    protected override void ForceWriteBackValue() { }

    /// <summary>
    /// Löst <see cref="PropertyChanged"/> aus, solange die Strategie nicht verworfen ist.
    /// </summary>
    protected virtual void OnPropertyChanged(string propertyName) {
        if (IsDisposed) { return; }
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected override void ReadParameters(JsonObject json) {
        Script = json.GetString(_scriptKey, Script);
        ReadOnly = json.GetBool(_readOnlytKey, ReadOnly);
    }

    protected override void SetValueToControlInternal(string value) { }

    /// <summary>
    /// Führt das Skript im Editor-Testmodus ohne Tabellen- und Zeilenkontext aus.
    /// Die Zeilen-Variablen stehen nur im Produktivdurchlauf zur Verfügung.
    /// </summary>
    private ScriptEndedFeedback ExecuteScriptTest(string script, bool testmode) => ExecuteScript(script, testmode, ReadOnly, null, null);

    #endregion
}
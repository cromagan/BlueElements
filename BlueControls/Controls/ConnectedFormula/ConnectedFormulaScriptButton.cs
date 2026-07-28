// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes.ItemCollectionPad.FunktionsItems_Formular;
using BlueControls.Controls.ConnectedFormula;
using System.Windows.Forms;

namespace BlueControls.Controls;

internal partial class ConnectedFormulaScriptButton : GenericControlReciver {

    #region Constructors

    public ConnectedFormulaScriptButton() : base(false, false, false) => InitializeComponent();

    #endregion

    #region Properties

    public ButtonArgs Drückbar_wenn {
        get;
        set {
            if (IsDisposed) { return; }
            if (field == value) { return; }
            field = value;
            Invalidate();
        }
    }

    public string ImageCode {
        get => mainButton.ImageCode;
        set => mainButton.ImageCode = value;
    }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Script {
        get;
        set {
            if (IsDisposed) { return; }
            if (field == value) { return; }
            field = value;
            Invalidate();
        }
    } = string.Empty;

    public override string Text {
        get => mainButton.Text;
        set => mainButton.Text = value;
    }

    #endregion

    #region Methods

    protected override void HandleChangesNow() {
        base.HandleChangesNow();

        if (IsDisposed) { return; }
        if (RowsInputChangedHandled && FilterInputChangedHandled) { return; }

        DoInputFilter(null, false);
        RowsInputChangedHandled = true;

        bool enabled;

        switch (Drückbar_wenn) {
            case ButtonArgs.Egal:
                enabled = true;
                break;

            case ButtonArgs.Keine_Zeile:
                enabled = FilterInput?.Rows is { Count: 0 };
                break;

            case ButtonArgs.Genau_eine_Zeile:
                enabled = FilterInput?.Rows is { Count: 1 };
                break;

            case ButtonArgs.Eine_oder_mehr_Zeilen:
                enabled = FilterInput?.Rows is { Count: > 0 };
                break;

            default:
                enabled = false;
                break;
        }

        if (string.IsNullOrEmpty(Script)) { enabled = false; }

        Enabled = enabled;
    }

    private void mainButton_MouseUp(object sender, MouseEventArgs e) {
        if (e.Button != MouseButtons.Left) { return; }

        mainButton.Enabled = false;
        mainButton.Refresh();

        HandleChangesNow();

        #region Zutaten für ExecuteScript sammeln

        var row = RowSingleOrNull();
        Table? tb = row?.Table is { IsDisposed: false } rowTb ? rowTb : FilterInput?.Table is { IsDisposed: false } fiTb ? fiTb : null;

        // Field-Variablen-Quellen: Parent selbst (falls IHasFieldVariable) und
        // alle Child-Controls, die IHasFieldVariable implementieren.
        List<IHasFieldVariable>? fieldSources = null;
        if (Parent is not null) {
            fieldSources = [];
            if (Parent is IHasFieldVariable hfvp) { fieldSources.Add(hfvp); }
            if (Parent.Controls is { } pControls) {
                foreach (var thisCon in pControls) {
                    if (thisCon is IHasFieldVariable hfv) { fieldSources.Add(hfv); }
                }
            }
        }

        var rowstamp = row?.RowStamp();

        #endregion

        var t = ScriptButtonPadItem.ExecuteScript(Script, Mode, true, null, row, tb, FilterInput, fieldSources);

        var errorreason = string.Empty;

        if (row?.RowStamp() != rowstamp) { errorreason = "Die Zeile wurde während des Ausführens verändert."; }

        if (t.Failed) { errorreason = t.ProtocolText; }

        if (string.IsNullOrEmpty(errorreason) && t.Variables is { } vars) {

            #region Variablen zurückschreiben

            if (fieldSources is not null) {
                foreach (var hfv in fieldSources) {
                    if (vars.GetByKey(hfv.FieldName) is { ReadOnly: false } v) {
                        hfv.SetValueFromVariable(v);
                    }
                }
            }
            tb?.WriteBackVariables(row, vars, false, true, "Script-Button-Press", !t.Failed);

            #endregion
        } else {
            Develop.Message(ErrorType.DevelopInfo, null, Develop.MonitorMessage, BlueBasics.Enums.ImageCode.Kritisch, $"Fehler: {t.ProtocolText}", 0);
            Forms.MessageBox.Show($"Dieser Knopfdruck wurde nicht komplett ausgeführt.\r\n\r\nGrund:\r\n{errorreason}", BlueBasics.Enums.ImageCode.Kritisch, "Ok");
        }

        mainButton.Enabled = true;
        mainButton.Refresh();
    }

    #endregion
}
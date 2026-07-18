// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes.ItemCollectionPad;
using BlueControls.Classes.ItemCollectionPad.Abstract;
using BlueControls.Classes.ItemCollectionPad.FunktionsItems_Formular;
using BlueScript.Classes;
using BlueScript.Variables;
using System.Windows.Forms;

namespace BlueControls.BlueTableDialogs;

public sealed partial class TimerScriptEditor : ScriptEditorGeneric {

    #region Fields

    private bool _allReadOnly;
    private RectanglePadItem? _item;

    #endregion

    #region Constructors

    public TimerScriptEditor() : base() {
        // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
        InitializeComponent();
    }

    #endregion

    #region Properties

    public override object? Object {
        get => IsDisposed ? null : (object?)_item;
        set {
            if (value is not TimerPadItem and not ScriptButtonPadItem) { value = null; }
            if (_item == value) { return; }

            WriteInfosBack();

            _item = null; // Um keine Werte zurück zu schreiben während des Anzeigens

            switch (value) {
                case TimerPadItem cpi:
                    tbcScriptEigenschaften.Enabled = true;
                    Script = cpi.Script;
                    _item = cpi;
                    _allReadOnly = true;
                    break;
                case ScriptButtonPadItem sbpi:
                    tbcScriptEigenschaften.Enabled = true;
                    Script = sbpi.Script;
                    _item = sbpi;
                    _allReadOnly = false;
                    break;
                default:
                    tbcScriptEigenschaften.Enabled = false;
                    Script = string.Empty;
                    _allReadOnly = true;
                    break;
            }
        }
    }

    #endregion

    #region Methods

    public override ScriptEndedFeedback ExecuteScript(bool testmode) {
        if (IsDisposed) {
            return new ScriptEndedFeedback("Objekt verworfen.", false, false, "Allgemein");
        }

        if (_item is null) {
            return new ScriptEndedFeedback("Kein Skript gewählt.", false, false, "Allgemein");
        }

        WriteInfosBack();

        if (_item is TimerPadItem tpi) {
            return TimerPadItem.ExecuteScript(tpi.Script, "Testmodus", string.Empty, string.Empty, string.Empty, !testmode, GetParseArgs());
        }
        if (_item is ScriptButtonPadItem sbpi) {

            #region Variablen erstellen

            VariableCollection vars;

            var row = sbpi.TableInput?.Row?.First();

            List<FilterItem>? fi = null;

            if (sbpi.Parents.Count > 0 && sbpi.TableInput is { IsDisposed: false } tbf && tbf.Column.First is { } c) {
                fi = [];
                for (var co = 0; co < sbpi.Parents.Count; co++) {
                    fi.Add(new FilterItem(c, FilterType.Istgleich_GroßKleinEgal, "DUMMY!"));
                }
            }

            if (row?.Table is { IsDisposed: false } tb) {
                vars = tb.CreateVariableCollection(row, _allReadOnly, false, false, true, fi); // Kein Zugriff auf tableHeadVariables, wegen Zeitmangel der Programmierung. Variablen müssten wieder zurückgeschrieben werden.
            } else if (sbpi.TableInput is { IsDisposed: false } tbf2) {
                vars = tbf2.CreateVariableCollection(null, _allReadOnly, false, false, true, fi); // Kein Zugriff auf tableHeadVariables, wegen Zeitmangel der Programmierung. Variablen müssten wieder zurückgeschrieben werden.
            } else {
                vars = [];
            }

            if (sbpi.Parent is ItemCollectionPadItem { IsDisposed: false } icpi) {
                foreach (var thisCon in icpi) {
                    if (thisCon is IHasFieldVariable hfv && hfv.GetFieldVariable() is { } v) {
                        vars.Add(v);
                    }
                }
            }

            #endregion

            return ScriptButtonPadItem.ExecuteScript(sbpi.Script, "Testmodus", vars, row, !testmode, GetParseArgs());
        }

        return new ScriptEndedFeedback("Interner Fehler", false, false, "Allgemein");
    }

    public override void WriteInfosBack() {
        //if (IsDisposed || TableView.ErrorMessage(Table, EditableErrorReasonType.EditNormaly) || Table is null || Table.IsDisposed) { return; }

        if (_item is TimerPadItem tpi) {
            tpi.Script = Script;
            ScriptChangedByUser = false;
        }
        if (_item is ScriptButtonPadItem sbpi) {
            sbpi.Script = Script;
            ScriptChangedByUser = false;
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e) {
        WriteInfosBack();
        base.OnFormClosing(e);

        Object = null; // erst das Item!
    }

    #endregion
}
// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
// https://github.com/cromagan/BlueElements
//
// License: GNU Affero General Public License v3.0
// https://github.com/cromagan/BlueElements/blob/master/LICENSE
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using BlueControls.Classes.ItemCollectionPad;
using BlueControls.Classes.ItemCollectionPad.Abstract;
using BlueControls.Classes.ItemCollectionPad.FunktionsItems_Formular;
using BlueControls.Interfaces;
using BlueScript.Classes;
using BlueScript.Variables;
using BlueTable.Classes;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BlueControls.BlueTableDialogs;

public sealed partial class TimerScriptEditor : ScriptEditorGeneric {

    #region Fields

    private bool _allReadOnly = false;
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

            if (value is TimerPadItem cpi) {
                tbcScriptEigenschaften.Enabled = true;
                Script = cpi.Script;
                _item = cpi;
                _allReadOnly = true;
            } else if (value is ScriptButtonPadItem sbpi) {
                tbcScriptEigenschaften.Enabled = true;
                Script = sbpi.Script;
                _item = sbpi;
                _allReadOnly = false;
            } else {
                tbcScriptEigenschaften.Enabled = false;
                Script = string.Empty;
                _allReadOnly = true;
            }
        }
    }

    #endregion

    #region Methods

    public override ScriptEndedFeedback ExecuteScript(bool testmode) {
        if (IsDisposed) {
            return new ScriptEndedFeedback("Objekt verworfen.", false, false, "Allgemein");
        }

        if (_item == null) {
            return new ScriptEndedFeedback("Kein Skript gewählt.", false, false, "Allgemein");
        }

        WriteInfosBack();

        if (_item is TimerPadItem tpi) {
            return TimerPadItem.ExecuteScript(tpi.Script, "Testmodus", string.Empty, string.Empty, string.Empty);
        }
        if (_item is ScriptButtonPadItem sbpi) {

            #region Variablen erstellen

            VariableCollection vars;

            var row = sbpi.TableInput?.Row?.First();

            List<FilterItem>? fi = null;

            if (sbpi.Parents.Count > 0 && sbpi.TableInput is { IsDisposed: false } tbf && tbf.Column.First is { } c) {
                fi = [];
                for (var co = 0; co < sbpi.Parents.Count; co++) {
                    fi.Add(new FilterItem(c, BlueTable.Enums.FilterType.Istgleich_GroßKleinEgal, "DUMMY!"));
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

            return ScriptButtonPadItem.ExecuteScript(sbpi.Script, "Testmodus", vars, row, false);
        }

        return new ScriptEndedFeedback("Interner Fehler", false, false, "Allgemein");
    }

    public override void WriteInfosBack() {
        //if (IsDisposed || TableView.ErrorMessage(Table, EditableErrorReasonType.EditNormaly) || Table == null || Table.IsDisposed) { return; }

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
// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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

#nullable enable

using System.Collections.Generic;
using System.Linq;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.ItemCollection.ItemCollectionList;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueControls.ConnectedFormula;

public sealed partial class ConnectedFormulaScriptEditor {

    #region Fields

    private FormulaScript? _item;

    #endregion

    #region Constructors

    public ConnectedFormulaScriptEditor(ConnectedFormula formula) {
        // Dieser Aufruf ist f�r den Windows Form-Designer erforderlich.
        InitializeComponent();
        grpEigenschaften.Enabled = false;
        eventScriptEditor.Enabled = false;
        Formula = formula;
        //Formula.Disposing += Database_Disposing;

        FormManager.Current.RegisterForm(this);
    }

    #endregion

    #region Properties

    public ConnectedFormula? Formula { get; private set; }

    public FormulaScript? Item {
        get {
            if (Formula == null || Formula.IsDisposed) { return null; }

            return _item;
        }
        set {
            if (_item == value) { return; }

            WriteInfosBack();

            _item = null; // Um keine werte zur�ck zu Schreiben werden des anzeigen

            if (value != null) {
                grpEigenschaften.Enabled = true;
                eventScriptEditor.Enabled = true;
                txbName.Text = value.Name;

                chkAusl�ser_valuechanged.Checked = value.EventTypes.HasFlag(ScriptEventTypes.value_changed);
                chkAusl�ser_valuechangedThread.Checked = value.EventTypes.HasFlag(ScriptEventTypes.value_changed_extra_thread);
                chkAusl�ser_prepaireformula.Checked = value.EventTypes.HasFlag(ScriptEventTypes.prepare_formula);
                chkAusl�ser_formulaloaded.Checked = value.EventTypes.HasFlag(ScriptEventTypes.loaded);
                chkAusl�ser_export.Checked = value.EventTypes.HasFlag(ScriptEventTypes.export);
                chkExternVerf�gbar.Checked = value.ManualExecutable;
                chkAendertWerte.Checked = value.ChangeValues;
                eventScriptEditor.ScriptText = value.Script;

                _item = value;
            } else {
                grpEigenschaften.Enabled = false;
                eventScriptEditor.Enabled = false;

                txbName.Text = string.Empty;
                eventScriptEditor.ScriptText = string.Empty;
                chkAusl�ser_valuechanged.Checked = false;
                chkAusl�ser_prepaireformula.Checked = false;
                chkExternVerf�gbar.Checked = false;
                chkAendertWerte.Checked = false;
                chkAusl�ser_valuechangedThread.Checked = false;
                chkAusl�ser_formulaloaded.Checked = false;
                chkAusl�ser_export.Checked = false;
            }
        }
    }

    #endregion

    #region Methods

    protected override void OnFormClosing(System.Windows.Forms.FormClosingEventArgs e) {
        base.OnFormClosing(e);
        if (Formula == null || Formula.IsDisposed) { return; }

        WriteInfosBack();
        //RemoveDatabase();
    }

    protected override void OnLoad(System.EventArgs e) {
        base.OnLoad(e);

        lstEventScripts.Item.Clear();
        if (Formula == null || Formula.IsDisposed) { return; }

        foreach (var thisSet in Formula.EventScript) {
            if (thisSet != null) {
                _ = lstEventScripts.Item.Add(thisSet);
            }
        }
    }

    protected override void OnShown(System.EventArgs e) => variableEditor.WriteVariablesToTable(Formula?.Variables);

    private void btnSave_Click(object sender, System.EventArgs e) {
        btnSave.Enabled = false;

        if (Formula != null) {
            WriteInfosBack();
            Formula.Save();
        }
        btnSave.Enabled = true;
    }

    private void chkAendertWerte_CheckedChanged(object sender, System.EventArgs e) {
        if (Item == null) { return; }
        Item.ChangeValues = chkAendertWerte.Checked;
    }

    private void chkAusl�ser_newrow_CheckedChanged(object sender, System.EventArgs e) {
        if (Item == null) { return; }

        ScriptEventTypes tmp = 0;
        if (chkAusl�ser_valuechanged.Checked) { tmp |= ScriptEventTypes.value_changed; }
        if (chkAusl�ser_prepaireformula.Checked) { tmp |= ScriptEventTypes.prepare_formula; }
        if (chkAusl�ser_valuechangedThread.Checked) { tmp |= ScriptEventTypes.value_changed_extra_thread; }
        if (chkAusl�ser_formulaloaded.Checked) { tmp |= ScriptEventTypes.loaded; }
        if (chkAusl�ser_export.Checked) { tmp |= ScriptEventTypes.export; }
        Item.EventTypes = tmp;
    }

    private void chkExternVerf�gbar_CheckedChanged(object sender, System.EventArgs e) {
        if (Item == null) { return; }
        Item.ManualExecutable = chkExternVerf�gbar.Checked;
    }

    private void eventScriptEditor_ExecuteScript(object sender, BlueScript.EventArgs.ScriptEventArgs e) {
        if (Formula == null || Formula.IsDisposed) {
            e.Feedback = new ScriptEndedFeedback("Keine Formular geladen.", false, "Allgemein");
            return;
        }

        if (_item == null) {
            e.Feedback = new ScriptEndedFeedback("Kein Skript gew�hlt.", false, "Allgemein");
            return;
        }

        if (!_item.IsOk()) {
            e.Feedback = new ScriptEndedFeedback("Bitte zuerst den Fehler korrigieren: " + _item.ErrorReason(), false, "Allgemein");
            return;
        }

        WriteInfosBack();

        //if (chkChangeValuesInTest.Checked) {
        //    if (MessageBox.Show("Skript �ndert Werte!<br>Fortfahren?", ImageCode.Warnung, "Fortfahren", "Abbruch") != 0) { return; }
        //}

        e.Feedback = Formula?.ExecuteScript(_item);
    }

    private void GlobalTab_SelectedIndexChanged(object sender, System.EventArgs e) => WriteInfosBack();

    private void lstEventScripts_AddClicked(object sender, System.EventArgs e) {
        if (Formula == null || Formula.IsDisposed) { return; }

        var newScriptItem = lstEventScripts.Item.Add(new FormulaScript(Formula));

        WriteInfosBack();

        newScriptItem.Checked = true;
    }

    private void lstEventScripts_ItemCheckedChanged(object sender, System.EventArgs e) {
        if (lstEventScripts.Item.Checked().Count != 1) {
            Item = null;
            return;
        }

        var selectedlstEventScripts = (FormulaScript)((ReadableListItem)lstEventScripts.Item.Checked()[0]).Item;
        Item = selectedlstEventScripts;
    }

    private void ScriptEditor_Changed(object sender, System.EventArgs e) {
        if (Item == null) { return; }
        Item.Script = eventScriptEditor.ScriptText;
    }

    private void txbName_TextChanged(object sender, System.EventArgs e) {
        if (Item == null) { return; }
        Item.Name = txbName.Text;
    }

    private void WriteInfosBack() {
        if (_item != null) {
            _item.Script = eventScriptEditor.ScriptText;
        }

        #region Items sicherheitshalber in die Datenbank zur�ck schreiben, nur so werden die gel�schten und neuen erfasst

        var t2 = new List<FormulaScript>();
        t2.AddRange(lstEventScripts.Item.Select(thisItem => (FormulaScript)((ReadableListItem)thisItem).Item));
        Formula.EventScript = new(t2);

        #endregion

        var l = variableEditor.GetVariables();
        var l2 = new List<VariableString>();
        foreach (var thisv in l) {
            if (thisv is VariableString vs) {
                l2.Add(vs);
            }
        }

        Formula.Variables = new VariableCollection(l2);
    }

    #endregion
}
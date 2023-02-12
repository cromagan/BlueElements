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

using BlueControls.Interfaces;
using BlueDatabase;
using BlueDatabase.Enums;

namespace BlueControls.Classes_Editor;

internal sealed partial class EventScript_Editor : AbstractClassEditor<EventScript>, IHasDatabase //System.Windows.Forms.UserControl//
{
    #region Constructors

    public EventScript_Editor() : base() => InitializeComponent();

    #endregion

    #region Properties

    public DatabaseAbstract Database {
        get => scriptEditor.Database;
        set => scriptEditor.Database = value;
    }

    #endregion

    //private void LoadScriptText() {
    //    if (_database == null || _database.IsDisposed) { return; }

    //    var sc = _database.EventScript.CloneWithClones();

    //    var ev = sc.Get(_skriptname);
    //    if (ev == null) {
    //        base.ScriptText = string.Empty;
    //        return;
    //    }

    //    base.ScriptText = ev.Script;
    //}

    //internal void WriteScriptBack() {
    //    if (_database == null || _database.IsDisposed) { return; }

    //    var sc = _database.EventScript.CloneWithClones();

    //    var ev = sc.Get(_skriptname);
    //    if (ev == null) { return; }

    //    ev.Script = base.ScriptText;

    //    _database.EventScript = new ReadOnlyCollection<EventScript?>(sc);
    //}

    #region Methods

    protected override void DisableAndClearFormula() {
        Enabled = false;

        txbName.Text = string.Empty;
        scriptEditor.ScriptText = string.Empty;
        scriptEditor.IsRowScript = false;
        chkAuslöser_newrow.Checked = false;
        chkAuslöser_valuechanged.Checked = false;
        chkAuslöser_errorcheck.Checked = false;
        chkExternVerfügbar.Checked = false;
        chkAendertWerte.Checked = false;
    }

    //public void WriteScriptBack() => scriptEditor.WriteScriptBack();
    protected override void EnabledAndFillFormula(EventScript data) {
        if (data == null) { return; }
        Enabled = true;

        txbName.Text = data.Name;
        scriptEditor.IsRowScript = data.NeedRow;
        chkZeile.Checked = data.NeedRow;
        scriptEditor.ScriptText = data.Script;
        chkAuslöser_newrow.Checked = data.EventTypes.HasFlag(EventTypes.new_row);
        chkAuslöser_valuechanged.Checked = data.EventTypes.HasFlag(EventTypes.value_changed);
        chkAuslöser_errorcheck.Checked = data.EventTypes.HasFlag(EventTypes.error_check);
        scriptEditor.EventTypes = data.EventTypes;
        chkExternVerfügbar.Checked = data.ManualExecutable;
        chkAendertWerte.Checked = data.ChangeValues;
    }

    protected override void PrepaireFormula(EventScript data) { }

    private void CheckEvents() {
        if (Item == null) { return; }

        EventTypes tmp = 0;
        if (chkAuslöser_newrow.Checked) { tmp |= EventTypes.new_row; }
        if (chkAuslöser_valuechanged.Checked) { tmp |= EventTypes.value_changed; }
        if (chkAuslöser_errorcheck.Checked) { tmp |= EventTypes.error_check; }
        Item.EventTypes = tmp;
    }

    private void chkAendertWerte_CheckedChanged(object sender, System.EventArgs e) {
        if (Item == null) { return; }
        Item.ChangeValues = chkAendertWerte.Checked;
    }

    private void chkAuslöser_newrow_CheckedChanged(object sender, System.EventArgs e) {
        CheckEvents();

        //if (chkAuslöser_errorcheck.Checked) {
        //    chkAendertWerte.Checked = false;
        //}
    }

    private void chkExternVerfügbar_CheckedChanged(object sender, System.EventArgs e) {
        if (Item == null) { return; }
        Item.ManualExecutable = chkExternVerfügbar.Checked;
    }

    private void chkZeile_CheckedChanged(object sender, System.EventArgs e) {
        if (Item == null) { return; }
        Item.NeedRow = chkZeile.Checked;
    }

    private void ScriptEditor_Changed(object sender, System.EventArgs e) {
        if (Item == null) { return; }
        Item.Script = scriptEditor.ScriptText;
    }

    private void txbName_TextChanged(object sender, System.EventArgs e) {
        if (Item == null) { return; }
        Item.Name = txbName.Text;
    }

    #endregion
}
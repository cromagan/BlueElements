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

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using BlueBasics.MultiUserFile;
using BlueControls.ConnectedFormula;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueDatabase;
using static BlueBasics.Develop;
using static BlueBasics.IO;

namespace BlueControls.Forms;

public partial class FormulaView : FormWithStatusBar {

    #region Constructors

    public FormulaView() => InitializeComponent();

    #endregion

    #region Methods

    public static void OpenScriptEditor(ConnectedFormula.ConnectedFormula? f) {
        if (f == null || f.IsDisposed) { return; }

        var se = new ConnectedFormulaScriptEditor(f);
        _ = se.ShowDialog();
    }

    protected override void OnLoad(System.EventArgs e) {
        base.OnLoad(e);
        CheckButtons();
    }

    private void Btb_Click(object sender, System.EventArgs e) {
        if (CFormula.ConnectedFormula == null || CFormula.ConnectedFormula.IsDisposed) { return; }

        if (sender is Button btb && btb.Tag is FormulaScript fs) {
            CFormula.ConnectedFormula.ExecuteScript(fs);
        }
    }

    private void btnFormular_Click(object sender, System.EventArgs e) {
        DebugPrint_InvokeRequired(InvokeRequired, true);
        if (CFormula.ConnectedFormula == null) { return; }

        var x = new ConnectedFormulaEditor(CFormula.ConnectedFormula.Filename, null);
        x.Show();
    }

    private void btnLetzteDateien_ItemClicked(object sender, BasicListItemEventArgs? e) {
        MultiUserFile.ForceLoadSaveAll();

        if (e?.Item == null) { return; }
        FormulaSet(e.Item.KeyName);
    }

    private void btnOeffnen_Click(object sender, System.EventArgs e) {
        MultiUserFile.SaveAll(false);
        DatabaseAbstract.ForceSaveAll();
        _ = LoadTab.ShowDialog();
    }

    private void btnSkripteBearbeiten_Click(object sender, System.EventArgs e) {
        OpenScriptEditor(CFormula.ConnectedFormula);
        UpdateScripts(CFormula.ConnectedFormula?.EventScript, grpSkripte);
    }

    private void CheckButtons() {
        btnSkripteBearbeiten.Enabled = CFormula.ConnectedFormula != null;
        btnFormular.Enabled = CFormula.ConnectedFormula != null;
    }

    private void FormulaSet(string? filename) {
        FormulaSet(null as ConnectedFormula.ConnectedFormula);

        if (filename == null || !FileExists(filename)) {
            //CheckButtons();
            return;
        }

        btnLastFormulas.AddFileName(filename, string.Empty);
        LoadTab.FileName = filename;
        var tmpFormula = ConnectedFormula.ConnectedFormula.GetByFilename(filename);
        if (tmpFormula == null) { return; }
        FormulaSet(tmpFormula);
    }

    private void FormulaSet(ConnectedFormula.ConnectedFormula? cf) {
        if (IsDisposed) { return; }

        if (cf != null && !cf.IsDisposed) {
            DropMessages = cf.IsAdministrator();
        }

        CFormula.SetData(cf, null, string.Empty);

        UpdateScripts(CFormula.ConnectedFormula?.EventScript, grpSkripte);
        CheckButtons();

        //var oldf = ConnectedFormula; // Zwischenspeichern wegen möglichen NULL verweisen

        //if (oldf == cf &&
        //    Database == database &&
        //    RowKey == rowKey) { return; }

        //SuspendLayout();

        //if (oldf != cf) {
        //    if (oldf != null) {
        //        RemoveRow();
        //        oldf.Loaded -= _cf_Loaded;
        //        oldf.Changed -= _cf_Changed;
        //    }

        //    InvalidateView();
        //    ConnectedFormula = cf;

        //    if (cf != null) {
        //        cf.Loaded += _cf_Loaded;
        //        cf.Changed += _cf_Changed;
        //    }
        //}

        //if (Database != database) {
        //    if (Database != null) {
        //        RemoveRow();
        //        Database.Disposing -= _Database_Disposing;
        //        Database.Row.RowRemoving -= Row_RowRemoving;
        //    }
        //    InvalidateView();
        //    Database = database;

        //    if (Database != null) {
        //        Database.Disposing += _Database_Disposing;
        //        Database.Row.RowRemoving += Row_RowRemoving;
        //    }
        //}

        //if (rowKey != -1 && Database != null && cf != null) {
        //    RowKey = rowKey;
        //    _tmpShowingRow = Database?.Row.SearchByKey(RowKey);
        //    SetInputRow();
        //} else {
        //    RemoveRow();
        //}

        //ResumeLayout();
        //Invalidate();
    }

    private void LoadTab_FileOk(object sender, CancelEventArgs e) {
        if (!FileExists(LoadTab.FileName)) { return; }

        FormulaSet(LoadTab.FileName);
    }

    private void UpdateScripts(ReadOnlyCollection<FormulaScript>? scripts, GroupBox groupBox) {

        #region Vorhanden Buttons löschen

        foreach (var thisControl in groupBox.Controls) {
            if (thisControl is BlueControls.Controls.Button btb) {
                btb.Click -= Btb_Click;
                btb.Dispose();
            }
        }
        groupBox.Controls.Clear();

        #endregion

        if (scripts == null || scripts.Count == 0) {
            groupBox.Visible = false;
            return;
        }

        groupBox.Visible = true;

        var l = Skin.PaddingSmal;

        foreach (var script in scripts) {
            if (script.ManualExecutable) {
                var btb = new BlueControls.Controls.Button();
                btb.Click += Btb_Click;
                btb.Name = "BTB_" + script.Name;
                groupBox.Controls.Add(btb);

                btb.Text = script.Name;
                btb.Tag = script;
                btb.ButtonStyle = ButtonStyle.Button_Big_Borderless;
                btb.Size = new Size(56, 66);
                btb.ImageCode = "Skript";
                btb.Location = new Point(l, 2);
                l = btb.Right;
            }
        }
        groupBox.Width = l + Skin.PaddingSmal;
    }

    #endregion
}
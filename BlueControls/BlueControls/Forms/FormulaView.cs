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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueBasics.MultiUserFile;
using BlueControls.BlueDatabaseDialogs;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.ItemCollection.ItemCollectionList;
using BlueDatabase;
using BlueDatabase.Enums;
using BlueDatabase.EventArgs;
using static BlueBasics.Develop;
using static BlueBasics.Generic;
using static BlueBasics.IO;

namespace BlueControls.Forms;

public partial class FormulaView : FormWithStatusBar {

    #region Constructors

    public FormulaView() => InitializeComponent();

    #endregion

    #region Methods

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

    private void CheckButtons() {
    }

    private void FormulaSet(string? filename) {
        FormulaSet(null as ConnectedFormula.ConnectedFormula);

        if (filename == null || !FileExists(filename)) {
            CheckButtons();
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
        CFO.SetData(cf, null, -1);

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

    #endregion
}
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
using System.Linq;
using System.Windows.Forms;
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.ItemCollection.ItemCollectionList;
using BlueDatabase;
using BlueDatabase.Enums;
using BlueScript.Variables;
using static BlueBasics.Converter;
using MessageBox = BlueControls.Forms.MessageBox;

namespace BlueControls.BlueDatabaseDialogs;

public sealed partial class DatabaseScriptEditor {

    #region Fields

    private DatabaseAbstract? _database;
    private bool _frmHeadEditorFormClosingIsin;

    #endregion

    #region Constructors

    public DatabaseScriptEditor(DatabaseAbstract database) {
        // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
        InitializeComponent();
        _database = database;
        _database.Disposing += Database_Disposing;
    }

    #endregion

    //public static void FormularWandeln(Database _database, string fn) {
    //    var x = new ConnectedFormula.ConnectedFormula();
    //    var tmp = new Formula();
    //    tmp.Size = x.PadData.SheetSizeInPix.ToSize();
    //    tmp.Database = _database;
    //    tmp.GenerateTabsToNewFormula(x);
    //    x.SaveAsAndChangeTo(fn);
    //}

    #region Methods

    protected override void OnFormClosing(FormClosingEventArgs e) {
        if (_frmHeadEditorFormClosingIsin) { return; }
        _frmHeadEditorFormClosingIsin = true;
        base.OnFormClosing(e);
        if (_database == null || _database.IsDisposed) {
            return;
        }

        WriteInfosBack();
        RemoveDatabase();
    }

    protected override void OnLoad(System.EventArgs e) {
        base.OnLoad(e);

        lstEventScripts.Item.Clear();
        foreach (var thisSet in _database.EventScript.Where(thisSet => thisSet != null)) {
            _ = lstEventScripts.Item.Add((EventScript)thisSet.Clone());
        }
    }

    protected override void OnShown(System.EventArgs e) {
        variableEditor.WriteVariablesToTable(_database?.Variables);
    }

    private void btnSave_Click(object sender, System.EventArgs e) {
        btnSave.Enabled = false;

        //scriptEditor.Message("Speichervorgang...");

        var ok = false;
        if (_database != null) {
            WriteInfosBack();
            ok = _database.Save();
        }
        if (ok) {
            MessageBox.Show("Speichern erfolgreich.", ImageCode.Häkchen, "Ok");
        } else {
            //scriptEditor.Message("Speichern fehlgeschlagen!");
            MessageBox.Show("Speichern fehlgeschlagen!", ImageCode.Kreuz, "Ok");
        }
        btnSave.Enabled = true;
    }

    private void Database_Disposing(object sender, System.EventArgs e) {
        RemoveDatabase();
        Close();
    }

    private void GlobalTab_Selecting(object sender, TabControlCancelEventArgs e) {
        if (e.TabPage == tabScripts) {
            eventScriptEditor.Database = _database;
        }
    }

    private void lstEventScripts_AddClicked(object sender, System.EventArgs e) {
        if (_database == null || _database.IsDisposed) { return; }

        var newScriptItem = lstEventScripts.Item.Add(new EventScript(_database));
        newScriptItem.Checked = true;
    }

    private void lstEventScripts_ItemCheckedChanged(object sender, System.EventArgs e) {
        if (lstEventScripts.Item.Checked().Count != 1) {
            eventScriptEditor.Item = null;
            return;
        }
        if (_database == null || _database.IsDisposed || _database.ReadOnly) {
            eventScriptEditor.Item = null;
            return;
        }
        var selectedlstEventScripts = (EventScript)((ReadableListItem)lstEventScripts.Item.Checked()[0]).Item;
        eventScriptEditor.Item = selectedlstEventScripts;

        WriteInfosBack();
    }

    private void OkBut_Click(object sender, System.EventArgs e) => Close();

    private void RemoveDatabase() {
        if (_database == null || _database.IsDisposed) { return; }
        _database.Disposing -= Database_Disposing;
        _database = null;
    }

    private void WriteInfosBack() {
        if (_database == null || _database.IsDisposed || _database.ReadOnly) { return; } // Disposed

        var t2 = new List<EventScript?>();
        t2.AddRange(lstEventScripts.Item.Select(thisItem => (EventScript)((ReadableListItem)thisItem).Item));
        _database.EventScript = new(t2);

        var l = variableEditor.GetVariables();
        var l2 = new List<VariableString>();
        foreach (var thisv in l) {
            if (thisv is VariableString vs) {
                l2.Add(vs);
            }
        }

        _database.Variables = new ReadOnlyCollection<VariableString>(l2);
    }

    #endregion
}
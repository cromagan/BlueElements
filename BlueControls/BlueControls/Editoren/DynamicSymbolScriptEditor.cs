// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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

using BlueControls.Forms;
using BlueScript.EventArgs;
using BlueScript.Structures;
using System.Windows.Forms;
using BlueBasics.MultiUserFile;
using BlueControls.ItemCollectionPad;
using BlueBasics;
using System.Drawing;
using System;

namespace BlueControls.BlueDatabaseDialogs;

public sealed partial class DynamicSymbolScriptEditor : FormWithStatusBar {

    #region Fields

    private DynamicSymbolPadItem? _item;

    #endregion

    #region Constructors

    public DynamicSymbolScriptEditor() {
        // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
        InitializeComponent();
        eventScriptEditor.Enabled = false;

        //var l = new List<string> {
        //        "In die Variable MENU muss das Menu erstellt werden.",
        //        "Dies muss folgendens Format sein:",
        //        "Backen",
        //        "Backen\\Zutaten",
        //        "Backen\\Zutaten\\Mehl",
        //        " ",
        //        "Parallel dazu kann die Variable Infos erstellt werden.",
        //        "Freie Texte."
        //    };
        //l.WriteAllText(TempFile("", "", "txt"), Win1252, true);

        FormManager.RegisterForm(this);
    }

    #endregion

    #region Properties

    public DynamicSymbolPadItem? Item {
        get {
            if (IsDisposed) { return null; }

            return _item;
        }
        set {
            if (_item == value) { return; }

            WriteInfosBack();

            _item = null; // Um keine werte zurück zu Schreiben werden des anzeigen

            if (value != null) {
                eventScriptEditor.Enabled = true;
                eventScriptEditor.Script = value.Script;
                _item = value;
            } else {
                eventScriptEditor.Enabled = false;

                eventScriptEditor.Script = string.Empty;
            }
        }
    }

    #endregion

    #region Methods

    protected override void OnFormClosing(FormClosingEventArgs e) {
        WriteInfosBack();

        if (DatabaseScriptEditor._befehlsReferenz is { Visible: true }) {
            DatabaseScriptEditor._befehlsReferenz.Close();
            DatabaseScriptEditor._befehlsReferenz.Dispose();
            DatabaseScriptEditor._befehlsReferenz = null;
        }

        base.OnFormClosing(e);

        Item = null; // erst das Item!
    }

    protected override void OnLoad(System.EventArgs e) => base.OnLoad(e);//var didMessage = false;//var im = QuickImage.Images();//foreach (var thisIm in im) {//    cbxPic.ItemAdd(ItemOf(thisIm, thisIm, QuickImage.Get(thisIm, 16)));//}//lstEventScripts.ItemClear();//if (IsDisposed || Database is not Database db || db.IsDisposed) { return; }//foreach (var thisSet in Database.EventScript) {//    if (thisSet != null) {//        var cap = "Sonstige";//        if (thisSet.EventTypes != 0) { cap = thisSet.EventTypes.ToString(); }//        var it = ItemOf(thisSet);//        it.UserDefCompareKey = cap + Constants.SecondSortChar;//        lstEventScripts.ItemAdd(it);//        if (lstEventScripts[cap] == null) {//            lstEventScripts.ItemAdd(ItemOf(cap, cap, true, cap + Constants.FirstSortChar));//        }//        if (!didMessage && thisSet.NeedRow && !Database.IsRowScriptPossible(false)) {//            didMessage = true;//            EnableScript();//        }//    }//}

    private void btnAusführen_Click(object sender, System.EventArgs e) => eventScriptEditor.TesteScript("MAIN");

    private void btnSave_Click(object sender, System.EventArgs e) {
        btnSaveLoad.Enabled = false;

        WriteInfosBack();
        MultiUserFile.SaveAll(false);

        btnSaveLoad.Enabled = true;
    }

    private void eventScriptEditor_ExecuteScript(object sender, ScriptEventArgs e) {
        if (IsDisposed) {
            e.Feedback = new ScriptEndedFeedback("Objekt verworfen.", false, false, "Allgemein");
            return;
        }

        if (_item == null) {
            e.Feedback = new ScriptEndedFeedback("Kein Skript gewählt.", false, false, "Allgemein");
            return;
        }

        WriteInfosBack();

        //if (!_item.IsOk()) {
        //    e.Feedback = new ScriptEndedFeedback("Bitte zuerst den Fehler korrigieren: " + _item.ErrorReason(), false, false, "Allgemein");
        //    return;
        //}

        //if (Database.Row.Count == 0) {
        //    e.Feedback = new ScriptEndedFeedback("Zum Test wird zumindest eine Zeile benötigt.", false, false, "Allgemein");
        //    return;
        //}
        //if (string.IsNullOrEmpty(txbTestZeile.Text)) {
        //    txbTestZeile.Text = Database?.Row.First()?.CellFirstString() ?? string.Empty;
        //}

        //RowItem? r = Database?.Row[txbTestZeile.Text];
        //if (r is not { IsDisposed: not true }) {
        //    e.Feedback = new ScriptEndedFeedback("Zeile nicht gefunden.", false, false, "Allgemein");
        //    return;
        //}

        var r = _item.UsedArea.ToRect();
        using var bmp = new Bitmap(Math.Max(r.Width, 16), Math.Max(r.Height, 16));

        e.Feedback = DynamicSymbolPadItem.ExecuteScript(_item.Script, "Testmodus", bmp);
    }

    private void WriteInfosBack() {
        //if (IsDisposed || TableView.ErrorMessage(Database, EditableErrorReasonType.EditNormaly) || Database == null || Database.IsDisposed) { return; }

        if (_item != null) {
            _item.Script = eventScriptEditor.Script;
        }
    }

    #endregion
}
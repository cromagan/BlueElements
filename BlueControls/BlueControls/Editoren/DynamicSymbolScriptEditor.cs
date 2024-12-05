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

using System;
using System.Drawing;
using System.Windows.Forms;
using BlueBasics;
using BlueBasics.MultiUserFile;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionPad;
using BlueScript.EventArgs;
using BlueScript.Structures;

namespace BlueControls.BlueDatabaseDialogs;

public sealed partial class DynamicSymbolScriptEditor : ScriptEditorGeneric {

    #region Fields

    private DynamicSymbolPadItem? _item;

    #endregion

    #region Constructors

    public DynamicSymbolScriptEditor() : base() {
        // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
        InitializeComponent();
    }

    #endregion

    #region Properties

    public object? Object {
        get {
            if (IsDisposed) { return null; }
            return _item;
        }
        set {
            if (value is not DynamicSymbolPadItem) { value = null; }
            if (_item == value) { return; }

            WriteInfosBack();

            _item = null; // Um keine Werte zurück zu schreiben während des Anzeigens

            if (value is DynamicSymbolPadItem cpi) {
                eventScriptEditor.Enabled = true;
                eventScriptEditor.Script = cpi.Script;
                _item = cpi;
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

        base.OnFormClosing(e);

        Object = null; // erst das Item!
    }

    private void btnAusführen_Click(object sender, System.EventArgs e) => eventScriptEditor.TesteScript("MAIN");

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
        //if (r is not { IsDisposed: false }) {
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
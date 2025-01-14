// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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
using BlueControls.ItemCollectionPad;
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

    public override object? Object {
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
                tbcScriptEigenschaften.Enabled = true;
                Script = cpi.Script;
                _item = cpi;
            } else {
                tbcScriptEigenschaften.Enabled = false;
                Script = string.Empty;
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

        var r = _item.UsedArea.ToRect();
        using var bmp = new Bitmap(Math.Max(r.Width, 16), Math.Max(r.Height, 16));

        return DynamicSymbolPadItem.ExecuteScript(_item.Script, "Testmodus", bmp);
    }

    public override void WriteInfosBack() {
        //if (IsDisposed || TableView.ErrorMessage(Database, EditableErrorReasonType.EditNormaly) || Database == null || Database.IsDisposed) { return; }

        if (_item != null) {
            _item.Script = Script;
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e) {
        WriteInfosBack();

        base.OnFormClosing(e);

        Object = null; // erst das Item!
    }

    #endregion
}
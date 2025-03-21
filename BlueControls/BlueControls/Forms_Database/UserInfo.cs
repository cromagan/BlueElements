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

using BlueControls.Forms;
using BlueDatabase;
using System.Windows.Forms;

namespace BlueControls.BlueDatabaseDialogs;

public sealed partial class UserInfo : FormWithStatusBar {

    #region Constructors

    public UserInfo() : base() =>
        // Dieser Aufruf ist f�r den Designer erforderlich.
        InitializeComponent();

    #endregion

    #region Methods

    protected override void OnFormClosing(FormClosingEventArgs e) {
        base.OnFormClosing(e);
        tblUndo.Database?.Dispose();
    }

    protected override void OnShown(System.EventArgs e) {
        base.OnShown(e);
        DatabaseHeadEditor.GenerateUndoTabelle(tblUndo);

        foreach (var thisDb in Database.AllFiles) {
            if (!string.IsNullOrEmpty(thisDb.Filename)) {
                DatabaseHeadEditor.AddUndosToTable(tblUndo, thisDb, 0.5f);
            }
        }

        tblUndo.Database?.Freeze("Nur Ansicht");
    }

    #endregion
}
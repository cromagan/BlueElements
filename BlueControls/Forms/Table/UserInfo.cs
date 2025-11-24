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
using BlueTable;
using System.Windows.Forms;

namespace BlueControls.BlueTableDialogs;

public sealed partial class UserInfo : FormWithStatusBar {

    #region Constructors

    public UserInfo() : base() =>
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();

    #endregion

    #region Methods

    protected override void OnFormClosing(FormClosingEventArgs e) {
        base.OnFormClosing(e);
        tblUndo.Table?.Dispose();
    }

    protected override void OnShown(System.EventArgs e) {
        base.OnShown(e);
        TableHeadEditor.GenerateUndoTabelle(tblUndo);

        foreach (var thisTb in Table.AllFiles) {
            if (thisTb is TableFile) {
                TableHeadEditor.AddUndosToTable(tblUndo, thisTb, 0.5f);
            }
        }

        tblUndo.Table?.Freeze("Nur Ansicht");
    }

    #endregion
}
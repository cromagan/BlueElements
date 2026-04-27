// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
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

using BlueControls.Classes.ItemCollectionList;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueScript.Methods;
using BlueTable.Classes;
using System;
using static BlueBasics.Extensions;

namespace BlueControls;

public partial class Befehlsreferenz : Form {

    #region Constructors

    public Befehlsreferenz() {
        InitializeComponent();
        WriteCommandsToList();
    }

    #endregion

    #region Methods

    private static void GetUses(Type methodType, int max) {
        var uses = Method.GetUsesInDb(methodType);
        if (uses.Count >= max) { return; }

        var commandName = Method.GetCommand(methodType);

        foreach (var thisTb in Table.AllFiles) {
            if (!thisTb.IsDisposed && thisTb is TableFile) {
                if (thisTb.EventScript.ToString(false).IndexOfWord(commandName, 0, System.Text.RegularExpressions.RegexOptions.IgnoreCase) >= 0) {
                    Method.AddUseInDb(methodType, "Tabelle: " + thisTb.Caption);
                    if (Method.GetUsesInDb(methodType).Count >= max) { return; }
                }
            }
        }
    }

    private void btnFilterDel_Click(object sender, System.EventArgs e) => txbFilter.Text = string.Empty;

    private void lstCommands_ItemClicked(object sender, AbstractListItemEventArgs e) {
        var co = string.Empty;
        if (e.Item is MethodListItem ml) {
            GetUses(ml.MethodType, 5);

            co += Method.GetHintText(ml.MethodType);
        }
        txbComms.Text = co;
    }

    private void txbFilter_TextChanged(object sender, System.EventArgs e) {
        lstCommands.FilterText = txbFilter.Text;
        btnFilterDel.Enabled = Enabled && !string.IsNullOrEmpty(txbFilter.Text);
    }

    private void WriteCommandsToList() {
        lstCommands.ItemClear();

        foreach (var thisc in Method.AllMethods) {
            lstCommands.ItemAdd(new MethodListItem(thisc, true));
        }
    }

    #endregion
}
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

using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.ItemCollectionList;
using BlueScript.Methods;
using BlueTable;
using static BlueBasics.Extensions;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls;

public partial class Befehlsreferenz : Form {

    #region Constructors

    public Befehlsreferenz() {
        InitializeComponent();
        WriteCommandsToList();
    }

    #endregion

    #region Methods

    private void btnFilterDel_Click(object sender, System.EventArgs e) => txbFilter.Text = string.Empty;

    private void GetUses(Method thisc, int max) {
        if (thisc.UsesInDB.Count >= max) { return; }

        foreach (var thisTb in Table.AllFiles) {
            if (!thisTb.IsDisposed && thisTb is TableFile) {
                if (thisTb.EventScript.ToString(false).ContainsWord(thisc.KeyName, System.Text.RegularExpressions.RegexOptions.IgnoreCase)) {
                    thisc.UsesInDB.AddIfNotExists("Tabelle: " + thisTb.Caption);
                    if (thisc.UsesInDB.Count >= max) { return; }
                }
            }
        }
    }

    private void lstCommands_ItemClicked(object sender, AbstractListItemEventArgs e) {
        var co = string.Empty;
        if (e.Item is ReadableListItem { Item: Method thisc }) {
            GetUses(thisc, 5);

            co += thisc.HintText();
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
            lstCommands.ItemAdd(ItemOf(thisc));
        }
    }

    #endregion
}
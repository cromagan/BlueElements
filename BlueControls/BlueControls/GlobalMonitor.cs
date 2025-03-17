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
using BlueDatabase;
using BlueScript.Methods;
using static BlueBasics.Extensions;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

#nullable enable

namespace BlueControls;

public partial class GlobalMonitor : Form {

    #region Constructors

    public GlobalMonitor() {
        InitializeComponent();
    }

    #endregion

    #region Methods

    private void btnFilterDel_Click(object sender, System.EventArgs e) => txbFilter.Text = string.Empty;




    private void txbFilter_TextChanged(object sender, System.EventArgs e) {
        lstLog.FilterText = txbFilter.Text;
        btnFilterDel.Enabled = Enabled && !string.IsNullOrEmpty(txbFilter.Text);
    }



    #endregion

    private void btnLeeren_Click(object sender, System.EventArgs e) {
        lstLog.ItemClear();
    }
}
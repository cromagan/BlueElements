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

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.EventArgs;
using BlueControls.Interfaces;
using BlueControls.ItemCollection.ItemCollectionList;
using BlueDatabase;
using BlueDatabase.Enums;
using System;
using System.Drawing;
using System.Linq;
using static BlueBasics.Converter;
using static BlueBasics.IO;

namespace BlueControls.Classes_Editor;

internal sealed partial class EventScript_Editor : AbstractClassEditor<EventScript>, IHasDatabase //System.Windows.Forms.UserControl//
{
    #region Constructors

    public EventScript_Editor() : base() => InitializeComponent();

    #endregion

    #region Properties

    public DatabaseAbstract Database {
        get => scriptEditor.Database;
        set => scriptEditor.Database = value;
    }

    #endregion

    #region Methods

    public void WriteScriptBack() => scriptEditor.WriteScriptBack();

    protected override void DisableAndClearFormula() {
        Enabled = false;
        txbName.Text = string.Empty;
        scriptEditor.SkriptName = string.Empty;
    }

    protected override void EnabledAndFillFormula() {
        if (Item == null) { return; }
        Enabled = true;

        txbName.Text = Item.Name;
        scriptEditor.IsRowScript = Item.NeedRow;
        scriptEditor.SkriptName = Item.Name;
    }

    protected override void PrepaireFormula() { }

    private void chkExternVerfügbar_CheckedChanged(object sender, System.EventArgs e) {
        Item.ManualExecutable = chkExternVerfügbar.Checked;
        OnChanged();
    }

    private void chkZeile_CheckedChanged(object sender, System.EventArgs e) {
        Item.NeedRow = chkZeile.Checked;
        OnChanged();
    }

    private void ScriptEditor_Changed(object sender, System.EventArgs e) => OnChanged();

    private void txbName_TextChanged(object sender, System.EventArgs e) {
        if (Item == null) { return; }

        Item.Name = txbName.Text;
        OnChanged();
    }

    #endregion
}
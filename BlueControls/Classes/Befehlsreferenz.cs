// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes.ItemCollectionList;
using BlueControls.EventArgs;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls;

public partial class Befehlsreferenz : Form {

    #region Constructors

    public Befehlsreferenz() {
        InitializeComponent();
        WriteCommandsToList();
    }

    #endregion

    #region Methods

    private static void GetUses(Method thisc, int max) {
        if (thisc.UsesInDB.Count >= max) { return; }

        foreach (var thisTb in Table.AllFiles) {
            if (!thisTb.IsDisposed && thisTb is TableFile) {
                if (thisTb.EventScript.ToString(false).IndexOfWord(thisc.KeyName, 0, System.Text.RegularExpressions.RegexOptions.IgnoreCase) >= 0) {
                    thisc.UsesInDB.AddIfNotExists("Tabelle: " + thisTb.Caption);
                    if (thisc.UsesInDB.Count >= max) { return; }
                }
            }
        }
    }

    private void btnFilterDel_Click(object sender, System.EventArgs e) => txbFilter.Text = string.Empty;

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

        foreach (var thisc in Method.AllMethods.Instances) {
            lstCommands.ItemAdd(ItemOf(thisc));
        }
    }

    #endregion
}
using BlueScript;
using BlueScript.Methods;
using System.Windows.Forms;
using BlueControls.ItemCollection.ItemCollectionList;

namespace BlueControls;

public partial class Befehlsreferenz : Form {

    #region Constructors

    public Befehlsreferenz() {
        InitializeComponent();
        WriteComandsToList();
    }

    #endregion

    #region Methods

    private void lstComands_ItemClicked(object sender, EventArgs.BasicListItemEventArgs e) {
        var co = string.Empty;
        if (e.Item is ReadableListItem r && r.Item is Method thisc) {
            co += thisc.HintText();
        }
        txbComms.Text = co;
    }

    private void WriteComandsToList() {
        lstComands.Item.Clear();

        if (Script.Comands == null) { return; }

        foreach (var thisc in Script.Comands) {
            _ = lstComands.Item.Add(thisc);
        }
    }

    #endregion
}
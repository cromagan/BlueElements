using System.Windows.Forms;
using BlueControls.EventArgs;
using BlueControls.ItemCollectionList;
using BlueScript;
using BlueScript.Methods;

namespace BlueControls;

public partial class Befehlsreferenz : Form {

    #region Constructors

    public Befehlsreferenz() {
        InitializeComponent();
        WriteComandsToList();
    }

    #endregion

    #region Methods

    private void lstComands_ItemClicked(object sender, AbstractListItemEventArgs e) {
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
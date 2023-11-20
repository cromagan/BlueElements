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
        WriteCommandsToList();
    }

    #endregion

    #region Methods

    private void lstCommands_ItemClicked(object sender, AbstractListItemEventArgs e) {
        var co = string.Empty;
        if (e.Item is ReadableListItem r && r.Item is Method thisc) {
            co += thisc.HintText();
        }
        txbComms.Text = co;
    }

    private void WriteCommandsToList() {
        lstCommands.Item.Clear();

        if (Script.Commands == null) { return; }

        foreach (var thisc in Script.Commands) {
            _ = lstCommands.Item.Add(thisc);
        }
    }

    #endregion
}
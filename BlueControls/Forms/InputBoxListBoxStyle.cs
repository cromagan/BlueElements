// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.Forms;

public partial class InputBoxListBoxStyle : DialogWithOkAndCancel {

    #region Fields

    private List<ListItem>? _giveBack;

    #endregion

    #region Constructors

    private InputBoxListBoxStyle() : this(string.Empty, null, CheckBehavior.SingleSelection, null, AddType.None, true, false) { }

    private InputBoxListBoxStyle(string txt, List<ListItem>? itemsOriginal, CheckBehavior checkBehavior, List<string>? check, AddType addNewAllowed, bool autosort, bool closeOnItemClick) : base(true, true) {
        InitializeComponent();

        txbText.CheckBehavior = checkBehavior;
        txbText.ItemAddRange(itemsOriginal);
        if (check is not null) { txbText.Check(check, true); }
        txbText.MoveAllowed = false;
        txbText.RemoveAllowed = false;
        txbText.AddAllowed = addNewAllowed;
        txbText.AutoSort = autosort;

        if (closeOnItemClick) {
            txbText.ItemAdd(ItemOf("Abbrechen", "Abbrechen", ImageCode.Kreuz, true, "~Abbrechen"));
            txbText.ItemClicked += (_, _) => Close();
        }

        Setup(txt, txbText, 250);

        if (closeOnItemClick) {
            butOK.Visible = false;
            butAbbrechen.Visible = false;
            Height -= butOK.Height + Skin.Padding;
        }
    }

    #endregion

    #region Methods

    public static string Show(string txt, List<string>? items) {
        if (items is not { Count: not 0 }) {
            return InputBox.Show(txt, string.Empty, BlueBasics.Classes.Formats.TextFormat.Instance);
        }

        List<ListItem> x = [];
        x.AddRange(ItemsOf(items));
        //x.Sort();
        var erg = Show(txt, x, CheckBehavior.SingleSelection, null, AddType.None);
        return erg?.Count != 1 ? string.Empty : erg[0].KeyName;
    }

    public static List<ListItem>? Show(string txt, List<ListItem> items, CheckBehavior checkBehavior, List<string>? check, AddType addNewAllowed) {
        var mb = new InputBoxListBoxStyle(txt, items, checkBehavior, check, addNewAllowed, true, false);
        mb.ShowDialog();
        return mb._giveBack;
    }

    public static void Show(string txt, List<ListItem> items, bool closeOnItemClick) {
        var mb = new InputBoxListBoxStyle(txt, items, CheckBehavior.SingleSelection, null, AddType.None, true, closeOnItemClick);
        mb.ShowDialog();
    }

    protected override bool SetValue() {
        _giveBack = Canceled ? null : [.. txbText.CheckedItems];
        return true;
    }

    private void InputBox_Shown(object sender, System.EventArgs e) => txbText.Focus();

    #endregion
}
// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.Forms;

public partial class InputBoxComboStyle : DialogWithOkAndCancel {

    #region Fields

    private string _giveBack;

    #endregion

    #region Constructors

    private InputBoxComboStyle() : this(string.Empty, string.Empty, BlueBasics.Classes.Formats.TextFormat.Instance, null, false) { }

    private InputBoxComboStyle(string txt, string vorschlagsText, IInputFormat textformat, List<ListItem>? suggestOriginal, bool texteingabeErlaubt) : base(true, true) {
        InitializeComponent();
        cbxText.Text = vorschlagsText;
        cbxText.ItemAddRange(suggestOriginal);
        cbxText.GetStyleFrom(textformat);
        cbxText.MultiLine = false;

        cbxText.DropDownStyle = texteingabeErlaubt ? DropDownMode.DropDown : DropDownMode.DropDownList;
        Setup(txt, cbxText, 250);
        _giveBack = vorschlagsText;
    }

    #endregion

    #region Methods

    public static string Show(string txt, IInputFormat textformat, List<ListItem>? suggest, bool texteingabeErlaubt) => Show(txt, string.Empty, textformat, suggest, texteingabeErlaubt);

    public static string Show(string txt, IInputFormat textformat, List<string> suggest, bool texteingabeErlaubt) {
        var suggestItems = new List<ListItem>();
        suggestItems.AddRange(ItemsOf(suggest));
        //Suggest.Sort();
        return Show(txt, string.Empty, textformat, suggestItems, texteingabeErlaubt);
    }

    protected override bool SetValue() {
        _giveBack = Canceled ? string.Empty : cbxText.Text;
        return true;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="txt"></param>
    /// <param name="vorschlagsText"></param>
    /// <param name="suggest">Wird geklont, es kann auch aus einer Listbox kommen, und dann stimmen die Events nicht mehr. Es muss auch einbe ItemCollection bleiben, damit aus der Tabelle auch Bilder etc. angezeigt werden können.</param>
    /// <param name="texteingabeErlaubt"></param>
    /// <returns></returns>

    private static string Show(string txt, string vorschlagsText, IInputFormat textformat, List<ListItem>? suggest, bool texteingabeErlaubt) {
        var MB = new InputBoxComboStyle(txt, vorschlagsText, textformat, suggest, texteingabeErlaubt);
        MB.ShowDialog();
        return MB._giveBack;
    }

    private void cbxText_EnterKey(object sender, System.EventArgs e) => Ok();

    private void cbxText_EscKey(object sender, System.EventArgs e) => Cancel();

    private void InputComboBox_Shown(object sender, System.EventArgs e) => cbxText.Focus();

    #endregion
}
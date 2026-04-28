// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Classes;
using BlueBasics.Interfaces;
using System.Windows.Forms;

namespace BlueControls.Forms;

public partial class InputBox : DialogWithOkAndCancel {

    #region Fields

    private string _giveBack;

    #endregion

    #region Constructors

    private InputBox() : this(string.Empty, string.Empty, FormatHolder_Text.Instance, false) { }

    private InputBox(string txt, string vorschlagsText, IInputFormat textformat, bool bigMultiLineBox) : base() {
        InitializeComponent();
        txbText.Text = vorschlagsText;
        txbText.GetStyleFrom(textformat);
        txbText.MultiLine = bigMultiLineBox;
        if (bigMultiLineBox) { txbText.Height += 200; }
        Setup(txt, txbText, 250);
        _giveBack = vorschlagsText;
    }

    #endregion

    #region Methods

    public static string Show(string txt) => Show(txt, string.Empty, FormatHolder_Text.Instance, false);

    /// <summary>
    ///
    /// </summary>
    /// <param name="txt"></param>
    /// <param name="vorschlagsText"></param>
    /// <param name="textformat">Beispiel: BlueBasics.FormatHolder oder BlueTable.FormatHolder_Text.Instance</param>
    /// <returns></returns>

    public static string Show(string txt, string vorschlagsText, IInputFormat textformat) => Show(txt, vorschlagsText, textformat, false);

    public static string Show(string txt, string vorschlagsText, IInputFormat textformat, bool bigMultiLineBox) {
        var mb = new InputBox(txt, vorschlagsText, textformat, bigMultiLineBox);
        mb.ShowDialog();
        return mb._giveBack;
    }

    protected override bool SetValue() {
        _giveBack = Canceled ? string.Empty : txbText.Text;
        return true;
    }

    private void InputBox_Shown(object sender, System.EventArgs e) => txbText.Focus();

    private void txbText_KeyDown(object sender, KeyEventArgs e) {
        if (e.KeyCode == Keys.Enter) { Ok(); }
    }

    private void txbText_ESC(object sender, System.EventArgs e) => Cancel();

    #endregion
}
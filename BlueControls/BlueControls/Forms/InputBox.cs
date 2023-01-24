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

using BlueBasics.Enums;
using BlueDatabase.Interfaces;
using BlueDatabase;

namespace BlueControls.Forms;

public partial class InputBox : DialogWithOkAndCancel {

    #region Fields

    private string _giveBack;

    #endregion

    #region Constructors

    private InputBox() : this(string.Empty, string.Empty, FormatHolder.Text, false) { }

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

    public static string Show(string txt) => Show(txt, "", FormatHolder.Text, false);

    public static string Show(string txt, string vorschlagsText, IInputFormat textformat) => Show(txt, vorschlagsText, textformat, false);

    public static string Show(string txt, string vorschlagsText, IInputFormat textformat, bool bigMultiLineBox) {
        InputBox mb = new(txt, vorschlagsText, textformat, bigMultiLineBox);
        mb.ShowDialog();
        return mb._giveBack;
    }

    protected override void SetValue(bool canceled) => _giveBack = canceled ? string.Empty : txbText.Text;

    private void InputBox_Shown(object sender, System.EventArgs e) => txbText.Focus();

    private void txbText_Enter(object sender, System.EventArgs e) => Ok();

    private void txbText_ESC(object sender, System.EventArgs e) => Cancel();

    #endregion
}
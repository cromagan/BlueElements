// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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

#nullable enable

using System.Collections.Generic;
using System.Windows.Forms;
using BlueControls.ItemCollectionList;

namespace BlueControls.Forms;

public partial class InputBoxComboStyle : DialogWithOkAndCancel {

    #region Fields

    private string _giveBack;

    #endregion

    #region Constructors

    private InputBoxComboStyle() : this(string.Empty, string.Empty, null, false) { }

    private InputBoxComboStyle(string txt, string vorschlagsText, List<AbstractListItem>? suggestOriginal, bool texteingabeErlaubt) : base(true, true) {
        InitializeComponent();
        cbxText.Text = vorschlagsText;
        if (suggestOriginal != null) {
            cbxText.Item.AddClonesFrom(suggestOriginal);
        }
        cbxText.DropDownStyle = texteingabeErlaubt ? ComboBoxStyle.DropDown : ComboBoxStyle.DropDownList;
        Setup(txt, cbxText, 250);
        _giveBack = vorschlagsText;
    }

    #endregion

    #region Methods

    public static string Show(string txt, List<AbstractListItem>? suggest, bool texteingabeErlaubt) => Show(txt, string.Empty, suggest, texteingabeErlaubt);

    public static string Show(string txt, List<string> suggest, bool texteingabeErlaubt) {
        List<AbstractListItem> Suggest = new(true);
        Suggest.AddRange(suggest);
        //Suggest.Sort();
        return Show(txt, string.Empty, Suggest, texteingabeErlaubt);
    }

    protected override void SetValue(bool canceled) => _giveBack = canceled ? string.Empty : cbxText.Text;

    /// <summary>
    ///
    /// </summary>
    /// <param name="txt"></param>
    /// <param name="vorschlagsText"></param>
    /// <param name="suggest">Wird geklont, es kann auch aus einer Listbox kommen, und dann stimmen die Events nicht mehr. Es muss auch einbe ItemCollection bleiben, damit aus der Datenbank auch Bilder etc. angezeigt werden können.</param>
    /// <returns></returns>

    private static string Show(string txt, string vorschlagsText, List<AbstractListItem>? suggest, bool texteingabeErlaubt) {
        var MB = new InputBoxComboStyle(txt, vorschlagsText, suggest, texteingabeErlaubt);
        _ = MB.ShowDialog();
        return MB._giveBack;
    }

    private void cbxText_Enter(object sender, System.EventArgs e) => Ok();

    private void cbxText_ESC(object sender, System.EventArgs e) => Cancel();

    private void InputComboBox_Shown(object sender, System.EventArgs e) => cbxText.Focus();

    #endregion
}
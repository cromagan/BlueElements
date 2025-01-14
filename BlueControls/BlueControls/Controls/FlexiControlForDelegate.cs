// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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

using System.Drawing;
using System.Windows.Forms;
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Enums;
using BlueDatabase.Enums;

namespace BlueControls.Controls;

public class FlexiControlForDelegate : FlexiControl {

    #region Fields

    private readonly DoThis? _doThis;

    #endregion

    #region Constructors

    public FlexiControlForDelegate() : this(null, string.Empty, null) { }

    public FlexiControlForDelegate(DoThis? doThis, string text, ImageCode? image) : base() {
        _doThis = doThis;

        Size = new Size(200, 24);

        EditType = EditTypeFormula.Button;
        CaptionPosition = CaptionPosition.ohne;
        var s0 = BlueControls.Controls.Caption.RequiredTextSize(text, SteuerelementVerhalten.Text_Abschneiden, Design.Caption, null, Translate, -1);

        Size = new Size(s0.Width + 50 + 22, 30);
        if (GetButton() is { IsDisposed: false } c0) {
            c0.Text = text;
            if (image is { } im) {
                c0.ImageCode = QuickImage.Get(im, 22).Code;
            }
        }

        GenFehlerText();

        _ = CheckEnabledState();
    }

    #endregion

    #region Delegates

    public delegate void DoThis();

    #endregion

    #region Methods

    protected override void OnButtonClicked() {
        base.OnButtonClicked();
        _doThis?.Invoke();
    }

    protected override void OnControlAdded(ControlEventArgs e) {
        _ = CheckEnabledState();
        base.OnControlAdded(e);
    }

    private bool CheckEnabledState() {
        if (DesignMode) {
            DisabledReason = string.Empty;
            return true;
        }

        if (_doThis == null) {
            DisabledReason = "Kein zugehöriges Objekt definiert.";
            return false;
        }

        DisabledReason = string.Empty;
        return true;
    }

    private void GenFehlerText() => InfoText = string.Empty;

    #endregion
}
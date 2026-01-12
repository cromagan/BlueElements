// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
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

using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.Enums;
using BlueControls.Forms;
using BlueTable.Enums;
using System.Drawing;
using System.Windows.Forms;

namespace BlueControls.Controls;

public class FlexiControlForDelegate : FlexiControl {

    #region Fields

    private DoThis? _doThis;

    #endregion

    #region Constructors

    public FlexiControlForDelegate() : this(null, string.Empty, ImageCode.Kreuz) { }

    public FlexiControlForDelegate(DoThis? doThis, string text, ImageCode image) : base() {
        Init(doThis, text, QuickImage.Get(image, 22));
    }

    /// <summary>
    /// Öffnet den Dialog zum DIREKTEN Bearbeiten.
    /// Soll es überschrieben werden  (z.B. Weil es readonly ist)
    /// FlexiControlForProperty benutzen.
    /// </summary>
    /// <param name="editable"></param>
    public FlexiControlForDelegate(IEditable editable) {
        if (editable is IReadableTextWithKey irt) {
            Init(editable.Edit, $"{editable.CaptionForEditor} '{irt.KeyName}' bearbeiten", irt.SymbolForReadableText());
        } else if (editable is IReadableText ir) {
            Init(editable.Edit, $"{editable.CaptionForEditor} bearbeiten", ir.SymbolForReadableText());
        } else {
            Init(editable.Edit, $"{editable.CaptionForEditor} bearbeiten", QuickImage.Get(ImageCode.Smiley, 22));
        }
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
        CheckEnabledState();
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

    private void Init(DoThis? doThis, string text, QuickImage image) {
        _doThis = doThis;

        Size = new Size(200, 24);

        EditType = EditTypeFormula.Button;
        CaptionPosition = CaptionPosition.ohne;
        var s0 = BlueControls.Controls.Caption.RequiredTextSize(text, SteuerelementVerhalten.Text_Abschneiden, Design.Caption, null, Translate, -1);

        Size = new Size(s0.Width + 50 + 22, 30);
        if (GetControl<Button>() is { IsDisposed: false } c0) {
            c0.Text = text;
            if (image is { } im) {
                c0.ImageCode = image.KeyName;
            }
        }

        GenFehlerText();

        CheckEnabledState();
    }

    #endregion
}
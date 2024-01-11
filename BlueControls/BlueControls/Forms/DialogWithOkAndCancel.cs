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

using System;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Controls;
using BlueControls.Enums;
using static BlueBasics.Develop;

namespace BlueControls.Forms;

public partial class DialogWithOkAndCancel : Form {

    #region Fields

    private readonly bool _cancelPossible;

    #endregion

    #region Constructors

    public DialogWithOkAndCancel() : this(Design.Form_MsgBox, true, false) { }

    public DialogWithOkAndCancel(bool cancelPossible, bool sizeable) : this(Design.Form_MsgBox, cancelPossible, sizeable) { }

    public DialogWithOkAndCancel(Design design, bool cancelPossible, bool sizeable) : base(design) {
        InitializeComponent();
        SetTopLevel(true);

        _cancelPossible = cancelPossible;

        if (Owner == null) {
            StartPosition = FormStartPosition.CenterScreen;
        }

        if (sizeable) {
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
        }
    }

    #endregion

    #region Properties

    protected bool OK_Enabled {
        get => butOK.Enabled;
        set => butOK.Enabled = value;
    }

    #endregion

    #region Methods

    public void Setup(int minWidth, int bottomOfLowestControl) {
        Text = AppName();

        minWidth = Math.Max(Width, minWidth);
        Size = new Size(minWidth, bottomOfLowestControl + butOK.Height + BorderHeight + Skin.Padding);
        if (_cancelPossible) {
            butAbbrechen.Left = minWidth - Skin.Padding - butAbbrechen.Width - BorderWidth;
            butOK.Left = butAbbrechen.Left - Skin.Padding - butOK.Width;
        } else {
            butAbbrechen.Visible = false;
            butAbbrechen.Enabled = false;
            butOK.Left = minWidth - Skin.Padding - butOK.Width - BorderWidth;
        }
        butAbbrechen.Visible = _cancelPossible;
        butOK.Top = bottomOfLowestControl;
        butAbbrechen.Top = bottomOfLowestControl;
    }

    public void Setup(string txt, GenericControl? centerControl, int minWidth) {
        var wi = Skin.Padding * 2;
        var he = Skin.Padding * 2;

        if (!string.IsNullOrEmpty(txt)) {
            capText.Visible = true;
            capText.Translate = false;
            capText.Text = txt;
            capText.Refresh();
            wi += capText.Width;
            he += capText.Height;
        }
        if (centerControl != null) {
            centerControl.Top = he;
            he = he + centerControl.Height + Skin.Padding;
        }
        wi = Math.Max(wi + BorderWidth, minWidth);
        Setup(wi, he);
        if (centerControl != null) {
            centerControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        }
    }

    protected void Cancel() {
        SetValue(true);
        Close();
    }

    protected void Ok() {
        SetValue(false);
        Close();
    }

    /// <summary>
    /// Must handle some layout operations manually because Visual Studio
    /// 2005 arbitrarily changes some properties of inherited controls.
    /// </summary>
    /// <param name="e">Data for event.</param>
    protected override void OnResize(System.EventArgs e) {
        base.OnResize(e);
        // https://stackoverflow.com/questions/4971768/incorrect-behavior-of-panel-on-inherited-windows-form
        if (butOK != null) {
            if (_cancelPossible) {
                butOK.Top = Height - 87;
                butOK.Left = Width - 193;
                butAbbrechen.Top = butOK.Top;
                butAbbrechen.Left = butOK.Right + Skin.Padding;
            } else {
                butOK.Top = Height - 87;
                butOK.Left = Width - 193 + butAbbrechen.Width + Skin.Padding;
                butAbbrechen.Visible = false;
            }
        }
    }

    /// <summary>
    /// Diese Routine wird aufgerufen, nachdem OK oder Cancel gedrückt wurde.
    /// </summary>
    protected virtual void SetValue(bool canceled) => DebugPrint_RoutineMussUeberschriebenWerden();

    private void butAbbrechen_Click(object sender, System.EventArgs e) => Cancel();

    private void butOK_Click(object sender, System.EventArgs e) => Ok();

    #endregion
}
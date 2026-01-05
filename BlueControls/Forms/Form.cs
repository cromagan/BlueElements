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
using BlueBasics.MultiUserFile;
using BlueControls.Enums;
using BlueTable;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace BlueControls.Forms;

public partial class Form : System.Windows.Forms.Form {

    #region Fields

    /// <summary>
    /// Die Dicke des oberen Balkens und unteren Randes einer Form in Pixel
    /// </summary>
    public const int BorderHeight = 39;

    /// <summary>
    /// Die Dicke des linken und rechen Randes einer Form in Pixel
    /// </summary>
    public const int BorderWidth = 16;

    #endregion

    ///// <summary>
    ///// Die Dicke des oberen Balken einer Form in Pixel
    ///// </summary>
    //public static readonly int BorderTop = 31;

    #region Constructors

    public Form() : this(Design.Form_Standard) {
    }

    public Form(Design design) : base() {
        Allgemein.CheckMemory();
        Design = design;
        if (!Skin.Inited) { Skin.LoadSkin(); }
        BackColor = Skin.Color_Back(Design, States.Standard);
        InitializeComponent();
        BackColor = Skin.Color_Back(Design, States.Standard);
    }

    #endregion

    #region Properties

    [DefaultValue(false)]
    public override bool AutoSize {
        get => false; //MyBase.AutoSize

        set => base.AutoSize = false;
    }

    public override sealed Color BackColor {
        get => base.BackColor;

        set => base.BackColor = value;
    }

    [DefaultValue(true)]
    public bool CloseButtonEnabled { get; set; } = true;

    [DefaultValue(Design.Form_Standard)]
    public Design Design { get; }

    public bool IsClosed { get; private set; }

    public bool IsClosing { get; private set; }

    protected override CreateParams CreateParams {
        get {
            var oParam = base.CreateParams;
            if (!CloseButtonEnabled) {
                oParam.ClassStyle |= (int)Cs.NOCLOSE;
            }
            return oParam;
        }
    }

    protected override bool ScaleChildren => false;

    #endregion

    #region Methods

    public bool IsMouseInForm() => new Rectangle(Location, Size).Contains(Cursor.Position);

    public new void PerformAutoScale() {
        // NIX TUN!!!!
    }

    // https://msdn.microsoft.com/de-de/library/ms229605(v=vs.110).aspx
    public void Scale() {
        // NIX TUN!!!!
    }

    public new void Show() {
        if (FormManager.Running) {
            FormManager.RegisterForm(this);
        }
        base.Show();
    }

    public new DialogResult ShowDialog() {
        if (FormManager.Running) {
            FormManager.RegisterForm(this);
        }
        var t = base.ShowDialog();
        return t;
    }

    //MyBase.ScaleChildren
    protected override Rectangle GetScaledBounds(Rectangle bounds, SizeF factor, BoundsSpecified specified) => bounds;

    protected override void OnCreateControl() {
        Develop.StartService();
        Allgemein.StartGlobalService();
        base.OnCreateControl();
    }

    protected override void OnFormClosing(FormClosingEventArgs e) {
        //https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.form.closed?view=netframework-4.8
        if (IsClosed) { return; }

        IsClosing = true;

        if (this is not FloatingForm and not MessageBox and not InputBox) {
            Table.SaveAll(false);
            MultiUserFile.SaveAll(false);
            Table.SaveAll(true);
            MultiUserFile.SaveAll(true);
        }

        base.OnFormClosing(e);
        if (!e.Cancel) { IsClosed = true; }
        IsClosing = IsClosed;
    }

    protected override void OnInvalidated(InvalidateEventArgs e) {
        if (!IsClosed) { base.OnInvalidated(e); }
    }

    protected override void OnLoad(System.EventArgs e) {
        BackColor = Skin.Color_Back(Design, States.Standard);
        base.OnLoad(e);
    }

    protected override void OnPaint(PaintEventArgs e) {
        if (!IsClosed && !IsDisposed) { base.OnPaint(e); }
    }

    protected override void OnResize(System.EventArgs e) {
        if (!IsClosed) { base.OnResize(e); }
    }

    protected override void OnResizeBegin(System.EventArgs e) {
        if (!IsClosed) { base.OnResizeBegin(e); }
    }

    protected override void OnResizeEnd(System.EventArgs e) {
        if (!IsClosed) { base.OnResizeEnd(e); }
    }

    protected override void OnSizeChanged(System.EventArgs e) {
        if (!IsClosed) { base.OnSizeChanged(e); }
    }

    protected override void ScaleControl(SizeF factor, BoundsSpecified specified) => base.ScaleControl(new SizeF(1, 1), specified);

    #endregion
}
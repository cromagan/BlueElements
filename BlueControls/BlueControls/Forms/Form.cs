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
using BlueBasics;
using BlueBasics.MultiUserFile;
using BlueControls.Enums;
using BlueDatabase;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
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

    /// <summary>
    /// Die Dicke des oberen Balken einer Form in Pixel
    /// </summary>
    public static readonly int BorderTop = 31;

    private const int LOGPIXELSX = 88;
    private float _currentScaleFactor = 1.0f;
    private float _userscale = 1.0f;

    #endregion

    #region Constructors

    public Form() : this(Design.Form_Standard) { }

    public Form(Design design) : base() {
        Design = design;
        if (!Skin.Inited) { Skin.LoadSkin(); }
        BackColor = Skin.Color_Back(Design, States.Standard);
        InitializeComponent();
        BackColor = Skin.Color_Back(Design, States.Standard);
        AutoScaleMode = AutoScaleMode.Font;
    }

    #endregion

    #region Properties

    public override sealed Color BackColor {
        get => base.BackColor;
        // ReSharper disable once ValueParameterNotUsed
        set => base.BackColor = value;
    }

    [DefaultValue(true)]
    public bool CloseButtonEnabled { get; set; } = true;

    public float CurrentScaleFactor {
        get => _currentScaleFactor;
        private set {
            value = Math.Max(0.1f, value);

            if (Math.Abs(value - _currentScaleFactor) < 0.01f) { return; }

            SuspendLayout();
            try {
                var tmp = value / _currentScaleFactor;
                ScaleControl(new SizeF(tmp, tmp), BoundsSpecified.All);

                foreach (Control control in Controls) {
                    ScaleControlAndFont(control, tmp);
                }
            } finally {
                ResumeLayout();
                PerformLayout();
            }

            _currentScaleFactor = value;
        }
    }

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
            oParam.ExStyle |= 0x02000000;  // WS_EX_COMPOSITED
            return oParam;
        }
    }

    #endregion

    #region Methods

    public bool IsMouseInForm() => new Rectangle(Location, Size).Contains(Cursor.Position);

    protected override void OnCreateControl() {
        Develop.StartService();
        Allgemein.StartGlobalService();
        base.OnCreateControl();
    }

    protected override void OnDpiChanged(DpiChangedEventArgs e) {
        base.OnDpiChanged(e);
        CurrentScaleFactor = GetWindowsDpiScale() * _userscale;
    }

    protected override void OnFormClosing(FormClosingEventArgs e) {
        //https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.form.closed?view=netframework-4.8
        if (IsClosed) { return; }

        IsClosing = true;

        if (this is not FloatingForm and not MessageBox) {
            Database.ForceSaveAll();
            MultiUserFile.SaveAll(false);
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

        CurrentScaleFactor = GetWindowsDpiScale() * _userscale;
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
        if (!IsClosed) {
            base.OnResizeEnd(e);
        }
    }

    protected override void OnSizeChanged(System.EventArgs e) {
        if (!IsClosed) { base.OnSizeChanged(e); }
    }

    // Windows API für DPI-Abfrage
    [DllImport("user32.dll")]
    private static extern IntPtr GetDC(IntPtr hWnd);

    [DllImport("gdi32.dll")]
    private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

    [DllImport("user32.dll")]
    private static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDC);

    private float GetWindowsDpiScale() {
        var desktopDc = GetDC(IntPtr.Zero);
        var dpiX = GetDeviceCaps(desktopDc, LOGPIXELSX);
        ReleaseDC(IntPtr.Zero, desktopDc);
        return 1; //dpiX / 96f;
    }

    private void ScaleControlAndFont(Control control, float scalingRatio) {
        control.Scale(new SizeF(scalingRatio, scalingRatio));
        control.Font = new Font(control.Font.FontFamily, control.Font.Size * scalingRatio, control.Font.Style);

        foreach (Control childControl in control.Controls) {
            ScaleControlAndFont(childControl, scalingRatio);
        }
    }

    #endregion
}
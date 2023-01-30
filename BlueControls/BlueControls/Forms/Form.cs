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

using BlueBasics;
using BlueControls.Controls;
using BlueControls.Enums;
using System.ComponentModel;
using System.Drawing;
using BlueBasics.MultiUserFile;
using BlueDatabase;
using BlueBasics.Enums;
using System;

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
    /// Die Dicke des unteren Rahmens einer Form in Pixel
    /// </summary>
    public static readonly int BorderBottom = 8;

    /// <summary>
    /// Die Dicke des oberen Balken einer Form in Pixel
    /// </summary>
    public static readonly int BorderTop = 31;

    #endregion

    #region Constructors

    public Form() : this(Design.Form_Standard) { }

    public Form(Design design) : base() {
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
    public Design Design {
        get;
    }

    public bool IsClosed { get; private set; }

    protected override System.Windows.Forms.CreateParams CreateParams {
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

    public bool IsMouseInForm() => new Rectangle(Location, Size).Contains(System.Windows.Forms.Cursor.Position);

    // https://msdn.microsoft.com/de-de/library/ms229605(v=vs.110).aspx
    public new void PerformAutoScale() {
        // NIX TUN!!!!
    }

    public void Scale() {
        // NIX TUN!!!!
    }

    public bool UpdateStatus(FehlerArt type, string message, bool didAlreadyMessagebox, Caption capStatusbar, bool dropMessages) {
        if (IsDisposed) { return false; }
        if (capStatusbar == null || capStatusbar.IsDisposed) { return false; }

        try {
            if (InvokeRequired) {
                return (bool)Invoke(new Func<bool>(() => UpdateStatus(type, message, didAlreadyMessagebox, capStatusbar, dropMessages)));
            }

            var imagecode = ImageCode.Information;

            if (string.IsNullOrEmpty(message)) {
                capStatusbar.Text = string.Empty;
                capStatusbar.Refresh();
                return false;
            }

            message = message.Replace("\r\n", "; ");
            message = message.Replace("\r", "; ");
            message = message.Replace("\n", "; ");
            message = message.Replace("<BR>", "; ", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            message = message.Replace("; ; ", "; ");
            message = message.TrimEnd("; ");

            if (type == FehlerArt.Warnung) { imagecode = ImageCode.Warnung; }
            if (type == FehlerArt.Fehler) { imagecode = ImageCode.Kritisch; }

            if (type == FehlerArt.Info || type == FehlerArt.DevelopInfo || !dropMessages || didAlreadyMessagebox) {
                capStatusbar.Text = "<imagecode=" + QuickImage.Get(imagecode, 16) + "> " + message;
                capStatusbar.Refresh();
                return false;
            }

            if (dropMessages) {
                Develop.DebugPrint(FehlerArt.Warnung, message);
                MessageBox.Show(message, imagecode, "Ok");
                return true;
            }

            return false;
        } catch {
            return false;
        }
    }

    //MyBase.ScaleChildren
    protected override Rectangle GetScaledBounds(Rectangle bounds, SizeF factor, System.Windows.Forms.BoundsSpecified specified) => bounds;

    protected override void OnCreateControl() {
        Develop.StartService();
        Allgemein.StartDatabaseService();
        base.OnCreateControl();
    }

    protected override void OnFormClosing(System.Windows.Forms.FormClosingEventArgs e) {
        //https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.form.closed?view=netframework-4.8
        if (IsClosed) { return; }

        if (this is not FloatingForm && this is not MessageBox) {
            DatabaseAbstract.ForceSaveAll();
            MultiUserFile.ForceLoadSaveAll();
        }

        base.OnFormClosing(e);
        if (!e.Cancel) {
            IsClosed = true;
        }
    }

    protected override void OnInvalidated(System.Windows.Forms.InvalidateEventArgs e) {
        if (!IsClosed) { base.OnInvalidated(e); }
    }

    protected override void OnLoad(System.EventArgs e) {
        BackColor = Skin.Color_Back(Design, States.Standard);
        base.OnLoad(e);
    }

    protected override void OnPaint(System.Windows.Forms.PaintEventArgs e) {
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

    protected override void ScaleControl(SizeF factor, System.Windows.Forms.BoundsSpecified specified) => base.ScaleControl(new SizeF(1, 1), specified);

    #endregion
}
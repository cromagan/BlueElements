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
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Imaging;
using System.Windows.Forms;
using BlueBasics;
using BlueControls.Enums;

#nullable enable

//Inherits UserControl ' -> Gibt Focus an Child!
//Inherits ContainerControl -> ?
//Inherits Panel '-> Alles ist ein Container!
//Inherits ScrollableControl - > keine Tastatur/Mouseabfragen

namespace BlueControls.Controls;

public class GenericControl : Control {

    #region Fields

    protected bool MouseHighlight = true;

    private readonly bool _useBackBitmap;

    private Bitmap? _bitmapOfControl;

    private bool _generatingBitmapOfControl;

    private bool _mousePressing;

    private PartentType _myParentType = PartentType.Unbekannt;

    private Form? _pform = null;

    // Dieser Codeblock ist im Interface IQuickInfo herauskopiert und muss überall Identisch sein.
    private string _quickInfo = string.Empty;

    #endregion

    #region Constructors

    protected GenericControl() : this(false, false) {
    }

    protected GenericControl(bool doubleBuffer, bool useBackgroundBitmap) : base() {
        // Dieser Aufruf ist für den Designer erforderlich.
        // InitializeComponent()
        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
        SetStyle(ControlStyles.ContainerControl, false);
        SetStyle(ControlStyles.ResizeRedraw, false);
        SetStyle(ControlStyles.SupportsTransparentBackColor, false);
        SetStyle(ControlStyles.Opaque, true);
        //The next 3 styles are allefor double buffering
        SetStyle(ControlStyles.UserPaint, true);
        SetStyle(ControlStyles.AllPaintingInWmPaint, true);
        if (doubleBuffer) {
            SetDoubleBuffering();
        }
        _useBackBitmap = useBackgroundBitmap;
    }

    #endregion

    #region Events

    public event EventHandler? DisposingEvent;

    #endregion

    #region Properties

    [DefaultValue(false)]
    public override bool AutoSize {
        get => false; //MyBase.AutoSize
        // ReSharper disable once ValueParameterNotUsed
        set => base.AutoSize = false;
    }

    [Category("Darstellung")]
    [DefaultValue("")]
    [Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
    [Description("QuickInfo des Steuerelementes - im extTXT-Format")]
    public string QuickInfo {
        get => _quickInfo;
        set {
            if (_quickInfo != value) {
                Forms.QuickInfo.Close();
                _quickInfo = value;
                OnQuickInfoChanged();
            }
        }
    }

    protected virtual string QuickInfoText => _quickInfo;

    protected override bool ScaleChildren => false;

    #endregion

    #region Methods

    public static Form? ParentForm(Control? o) {
        if (o == null || o.IsDisposed) { return null; }

        do {
            switch (o) {
                case System.Windows.Forms.Form frm:
                    return frm;

                case null:
                    return null;

                default:
                    o = o.Parent; //Manchmal ist o null. MultiThreading?
                    break;
            }
        } while (true);
    }

    public static PartentType Typ(Control control) {
        switch (control) {
            case null:
                return PartentType.Nothing;

            case GroupBox: {
                    if (control.Parent is TabPage tp) {
                        if (tp.Parent == null) { return PartentType.Unbekannt; }
                        if (tp.Parent is RibbonBar) { return PartentType.RibbonGroupBox; }
                    }
                    return PartentType.GroupBox;
                }

            case LastFilesCombo:
                return PartentType.LastFilesCombo;
            //Is = "BlueBasics.ComboBox"

            case ComboBox box when box.ParentType() == PartentType.RibbonPage:
                return PartentType.RibbonBarCombobox;

            case ComboBox:
                return PartentType.ComboBox;
            // Is = "BlueBasics.TabControl"

            case RibbonBar:
                return PartentType.RibbonControl;

            case TabControl:
                return PartentType.TabControl;
            // Is = "BlueBasics.TabPage"

            case TabPage when control.Parent is RibbonBar:
                return PartentType.RibbonPage;

            case TabPage:
                return PartentType.TabPage;
            //Is = "BlueBasics.Slider"

            case Slider:
                return PartentType.Slider;
            //Is = "FRMMSGBOX"

            case BlueControls.Forms.FloatingForm:
                return PartentType.MsgBox;

            case BlueControls.Forms.DialogWithOkAndCancel:
                return PartentType.MsgBox;

            case TextBox:
                return PartentType.TextBox;

            case ListBox:
                return PartentType.ListBox;

            case EasyPic:
                return PartentType.EasyPic;

            case Button:
                return PartentType.Button;

            case Line:
                return PartentType.Line;

            case Caption:
                return PartentType.Caption;

            //case Formula:
            //    return PartentType.Formula;

            case Forms.Form:
                return PartentType.Form;

            case Table:
                return PartentType.Table;

            case Panel:
                return PartentType.Panel;

            case FlexiControlForCell:
                return PartentType.FlexiControlForCell;

            case FlexiControl:
                return PartentType.FlexiControl;

            default:
                return PartentType.Nothing;
        }
    }

    public Bitmap? BitmapOfControl() {
        if (!_useBackBitmap || _generatingBitmapOfControl) { return null; }
        _generatingBitmapOfControl = true;
        if (_bitmapOfControl == null) { Refresh(); }
        _generatingBitmapOfControl = false;
        return _bitmapOfControl;
    }

    public void CheckBack() => _pform = ParentForm();

    public bool ContainsMouse() => DoDrawings() && ClientRectangle.Contains(PointToClient(Cursor.Position));

    public bool DoDrawings() {
        if (IsDisposed || Disposing) { return false; }
        if (_pform == null || _pform.IsDisposed || !_pform.Visible) { return false; }
        if (_pform is BlueControls.Forms.Form bf && bf.isClosing) { return false; }
        if (!Visible) { return false; }
        return true;
    }

    public void DoQuickInfo() {
        if (string.IsNullOrEmpty(_quickInfo) && string.IsNullOrEmpty(QuickInfoText)) {
            Forms.QuickInfo.Close();
        } else {
            if (ContainsMouse()) {
                Forms.QuickInfo.Show(QuickInfoText);
            } else {
                Forms.QuickInfo.Close();
            }
        }
    }

    public new void Invalidate() {
        if (!DoDrawings()) { return; }
        base.Invalidate();
    }

    public Point MousePos() {
        if (InvokeRequired) {
            return (Point)Invoke(new Func<Point>(MousePos));
        }
        if (!DoDrawings()) { return default; }
        return PointToClient(Cursor.Position);
    }

    public void OnDisposingEvent() => DisposingEvent?.Invoke(this, System.EventArgs.Empty);

    // https://msdn.microsoft.com/de-de/library/ms229605(v=vs.110).aspx
    public void PerformAutoScale() {
        // NIX TUN!!!!
    }

    /// <summary>
    /// Veranlaßt, das das Control neu gezeichnet wird.
    /// </summary>
    /// <remarks></remarks>
    public override void Refresh() {
        if (!DoDrawings()) { return; }
        DoDraw(CreateGraphics());
    }

    public void Scale() {
        // NIX TUN!!!!
    }

    internal static bool AllEnabled(Control control) {
        if (control.IsDisposed) { return false; }
        do {
            if (control == null) { return true; }
            if (control.IsDisposed) { return false; }
            if (!control.Enabled) { return false; }
            control = control.Parent;
        } while (true);
    }

    protected override void Dispose(bool disposing) {
        OnDisposingEvent();

        base.Dispose(disposing);
        if (disposing) {
            _bitmapOfControl?.Dispose();
            _bitmapOfControl = null;
        }
    }

    protected virtual void DrawControl(Graphics gr, States state) => Develop.DebugPrint_RoutineMussUeberschriebenWerden();

    //MyBase.ScaleChildren
    protected override Rectangle GetScaledBounds(Rectangle bounds, SizeF factor, BoundsSpecified specified) => bounds;

    protected bool MousePressing() => _mousePressing;

    protected override void OnEnabledChanged(System.EventArgs e) {
        if (InvokeRequired) {
            _ = Invoke(new Action(() => OnEnabledChanged(e)));
            return;
        }
        if (!DoDrawings()) { return; }
        base.OnEnabledChanged(e);
        Invalidate();
    }

    protected override void OnGotFocus(System.EventArgs e) {
        if (IsDisposed) { return; }
        if (!GetStyle(ControlStyles.Selectable)) {
            _ = Parent.SelectNextControl(this, true, true, true, true);
        } else {
            base.OnGotFocus(e);
            Invalidate();
        }
    }

    protected override void OnKeyDown(KeyEventArgs e) {
        if (IsDisposed) { return; }
        base.OnKeyDown(e);
    }

    protected override void OnKeyPress(KeyPressEventArgs e) {
        if (IsDisposed) { return; }
        base.OnKeyPress(e);
    }

    protected override void OnKeyUp(KeyEventArgs e) {
        if (IsDisposed) { return; }
        base.OnKeyUp(e);
    }

    protected override void OnLostFocus(System.EventArgs e) {
        if (IsDisposed) { return; }
        if (GetStyle(ControlStyles.Selectable)) {
            //if (_MousePressing) { OnMouseUp(new System.Windows.Forms.MouseEventArgs(System.Windows.Forms.MouseButtons.None, 0, 0, 0, 0)); }
            MouseHighlight = false;
            base.OnLostFocus(e);
            Invalidate();
        }
    }

    protected override void OnMouseDown(MouseEventArgs e) {
        lock (this) {
            if (_pform == null) { CheckBack(); }

            if (!DoDrawings()) { return; }
            if (_mousePressing) { return; }
            _mousePressing = true;
            Forms.QuickInfo.Close();
            if (Enabled) {
                if (GetStyle(ControlStyles.Selectable) && Focus()) { _ = Focus(); }
            }
            base.OnMouseDown(e);
        }
    }

    protected override void OnMouseEnter(System.EventArgs e) {
        if (_pform == null) { CheckBack(); }
        if (!DoDrawings()) { return; }

        base.OnMouseEnter(e);
        if (!string.IsNullOrEmpty(_quickInfo) || !string.IsNullOrEmpty(QuickInfoText)) { Forms.QuickInfo.Show(QuickInfoText); }
    }

    protected override void OnMouseLeave(System.EventArgs e) {
        if (_pform == null) { CheckBack(); }

        if (!DoDrawings()) { return; }
        base.OnMouseLeave(e);
        DoQuickInfo();
    }

    protected override void OnMouseMove(MouseEventArgs e) {
        lock (this) {
            if (_pform == null) { CheckBack(); }

            if (!DoDrawings()) { return; }
            base.OnMouseMove(e);
            DoQuickInfo();
        }
    }

    protected override void OnMouseUp(MouseEventArgs e) {
        if (_pform == null) { CheckBack(); }

        if (!DoDrawings()) { return; }
        if (!_mousePressing) { return; }
        Develop.SetUserDidSomething();
        _mousePressing = false;
        base.OnMouseUp(e);
    }

    protected override void OnMouseWheel(MouseEventArgs e) {
        if (_pform == null) { CheckBack(); }

        if (!DoDrawings()) { return; }
        Develop.SetUserDidSomething();
        _mousePressing = false;
        base.OnMouseWheel(e);
    }

    protected override void OnPaint(PaintEventArgs e) =>
        // MyBase.OnPaint(e) - comment out - do not call  http://stackoverflow.com/questions/592538/how-to-create-a-transparent-control-which-works-when-on-top-of-other-controls
        DoDraw(e.Graphics);

    protected override void OnPaintBackground(PaintEventArgs pevent) {
        // do not allow the background to be painted
        // Um flimmern zu vermeiden!
    }

    protected override void OnParentChanged(System.EventArgs e) {
        base.OnParentChanged(e);
        CheckBack();
    }

    protected override void OnParentEnabledChanged(System.EventArgs e) {
        CheckBack();
        base.OnParentEnabledChanged(e);
    }

    protected override void OnParentVisibleChanged(System.EventArgs e) {
        CheckBack();
        base.OnParentVisibleChanged(e);
    }

    protected virtual void OnQuickInfoChanged() {
        // Dummy, dass die angeleeiteten Controls reagieren können.
    }

    protected override void OnSizeChanged(System.EventArgs e) {
        if (IsDisposed) { return; }
        Invalidate();
        if (_bitmapOfControl != null) {
            if (_bitmapOfControl.Width < Width || _bitmapOfControl.Height < Height) {
                _bitmapOfControl?.Dispose();
                _bitmapOfControl = null;
            }
        }
        base.OnSizeChanged(e);
    }

    protected Form? ParentForm() => ParentForm(Parent);

    protected PartentType ParentType() {
        if (Parent == null) { return PartentType.Unbekannt; }
        if (_myParentType != PartentType.Unbekannt) { return _myParentType; }
        _myParentType = Typ(Parent);
        return _myParentType;
    }

    protected override void ScaleControl(SizeF factor, BoundsSpecified specified) => base.ScaleControl(new SizeF(1, 1), specified);

    protected void SetDoubleBuffering() {
        SetStyle(ControlStyles.DoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
        UpdateStyles();
    }

    //MyBase.GetScaledBounds(bounds, factor, specified)

    protected void SetNotFocusable() {
        if (IsDisposed) { return; }
        TabStop = false;
        TabIndex = 0;
        CausesValidation = false;
        SetStyle(ControlStyles.Selectable, false);
        //SetStyle(System.Windows.Forms.ControlStyles.StandardClick, false);
        //SetStyle(System.Windows.Forms.ControlStyles.StandardDoubleClick, false);
    }

    protected override void WndProc(ref Message m) {
        try {
            //https://www.vb-paradise.de/allgemeines/tipps-tricks-und-tutorials/windows-forms/50038-wndproc-kleine-liste-aller-messages/
            if (m.Msg == (int)Enums.WndProc.WM_ERASEBKGND) { return; }
            base.WndProc(ref m);
        } catch { }
    }

    private void DoDraw(Graphics gr) {
        if (_pform == null) { CheckBack(); }

        if (!DoDrawings()) {
            gr.Clear(Color.LightGray);
            return;
        }

        //gr.Clear(Skin.RandomColor);
        //return;

        if (Develop.Exited || IsDisposed || !Visible) { return; }
        lock (this) {
            if (!Skin.Inited) {
                if (DesignMode) {
                    Skin.LoadSkin();
                } else {
                    return;
                }
            }
            if (Width < 1 || Height < 1) { return; }
            try {
                if (_useBackBitmap) {
                    _bitmapOfControl ??= new Bitmap(ClientSize.Width, ClientSize.Height, PixelFormat.Format32bppPArgb);
                    var tmpgr = Graphics.FromImage(_bitmapOfControl);
                    DrawControl(tmpgr, IsStatus());
                    if (_bitmapOfControl != null) {
                        gr.DrawImage(_bitmapOfControl, 0, 0);
                    }
                    tmpgr.Dispose();
                } else {
                    DrawControl(gr, IsStatus());
                }
            } catch {
                return;
            }
            // UmRandung für DesignMode ------------
            if (DesignMode) {
                using Pen p = new(Color.FromArgb(128, 255, 0, 0));
                gr.DrawRectangle(p, 0, 0, Width - 1, Height - 1);
            }
        }
    }

    private States IsStatus() {
        if (!Enabled) { return States.Standard_Disabled; }
        var s = States.Standard;
        if (MouseHighlight && ContainsMouse()) { s |= States.Standard_MouseOver; }
        if (_mousePressing) {
            if (MouseHighlight) { s |= States.Standard_MousePressed; }
            if (GetStyle(ControlStyles.Selectable) && CanFocus) { s |= States.Standard_HasFocus; }
        } else {
            if (GetStyle(ControlStyles.Selectable) && CanFocus && Focused) { s |= States.Standard_HasFocus; }
        }
        return s;
    }

    #endregion
}
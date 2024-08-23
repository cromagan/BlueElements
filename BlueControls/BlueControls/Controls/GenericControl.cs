﻿// Authors:
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

using BlueBasics;
using BlueControls.Enums;
using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Imaging;
using System.Windows.Forms;
using BlueBasics.Interfaces;
using BlueControls.Interfaces;

//Inherits UserControl ' -> Gibt Focus an Child!
//Inherits ContainerControl -> ?
//Inherits Panel '-> Alles ist ein Container!
//Inherits ScrollableControl - > keine Tastatur/Mouseabfragen

namespace BlueControls.Controls;

public class GenericControl : Control, IDisposableExtendedWithEvent, ISendsFocusedChild {

    #region Fields

    protected bool MouseHighlight = true;
    private Bitmap? _bitmapOfControl;
    private bool _generatingBitmapOfControl;
    private bool _mousePressing;
    private ParentType _myParentType = ParentType.Unbekannt;
    private Form? _pform;
    private string _quickInfo = string.Empty;
    private bool _useBackBitmap;

    #endregion

    #region Constructors

    protected GenericControl() : this(false, false) { }

    protected GenericControl(bool doubleBuffer, bool useBackgroundBitmap) : base() {
        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
        SetStyle(ControlStyles.ContainerControl, false);
        SetStyle(ControlStyles.ResizeRedraw, false);
        SetStyle(ControlStyles.SupportsTransparentBackColor, false);
        SetStyle(ControlStyles.Opaque, true);
        //The next 3 styles are allefor double buffering
        SetStyle(ControlStyles.UserPaint, true);
        SetStyle(ControlStyles.AllPaintingInWmPaint, true);
        UpdateStyles();
        if (doubleBuffer) {
            SetDoubleBuffering();
        }
        _useBackBitmap = useBackgroundBitmap;
    }

    #endregion

    #region Events

    public event EventHandler<ControlEventArgs>? ChildGotFocus;

    public event EventHandler? DisposingEvent;

    #endregion

    #region Properties

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

    public Rectangle ScaledDisplayRectangle { get; private set; } = Rectangle.Empty;
    public float ScaleX { get; private set; } = 1;
    public float ScaleY { get; private set; } = 1;
    protected virtual string QuickInfoText => _quickInfo;

    protected bool UseBackgroundBitmap {
        get => _useBackBitmap;
        set {
            if (_useBackBitmap == value) { return; }
            _useBackBitmap = value;
            _bitmapOfControl = null;
        }
    }

    #endregion

    #region Methods

    public static Form? ParentForm(Control? o) {
        while (o is { IsDisposed: false }) {
            if (o is Form frm) { return frm; }
            o = o.Parent;
        }
        return null;
    }

    public static ParentType Typ(Control control) {
        switch (control) {
            case null:
                return ParentType.Nothing;

            case GroupBox: {
                    if (control.Parent is TabPage tp) {
                        if (tp.Parent == null) { return ParentType.Unbekannt; }
                        if (tp.Parent is RibbonBar) { return ParentType.RibbonGroupBox; }
                    }
                    return ParentType.GroupBox;
                }

            case LastFilesCombo:
                return ParentType.LastFilesCombo;
            //Is = "BlueBasics.ComboBox"

            case ComboBox box when box.GetParentType() == ParentType.RibbonPage:
                return ParentType.RibbonBarCombobox;

            case ComboBox:
                return ParentType.ComboBox;
            // Is = "BlueBasics.TabControl"

            case RibbonBar:
                return ParentType.RibbonControl;

            case TabControl:
                return ParentType.TabControl;
            // Is = "BlueBasics.TabPage"

            case TabPage when control.Parent is RibbonBar:
                return ParentType.RibbonPage;

            case TabPage:
                return ParentType.TabPage;
            //Is = "BlueBasics.Slider"

            case Slider:
                return ParentType.Slider;
            //Is = "FRMMSGBOX"

            case Forms.FloatingForm:
                return ParentType.MsgBox;

            case Forms.DialogWithOkAndCancel:
                return ParentType.MsgBox;

            case TextBox:
                return ParentType.TextBox;

            case ListBox:
                return ParentType.ListBox;

            case EasyPic:
                return ParentType.EasyPic;

            case Button:
                return ParentType.Button;

            case Line:
                return ParentType.Line;

            case Caption:
                return ParentType.Caption;

            //case Formula:
            //    return ParentType.Formula;

            case Forms.Form:
                return ParentType.Form;

            case Table:
                return ParentType.Table;

            case Panel:
                return ParentType.Panel;

            case FlexiControlForCell:
                return ParentType.FlexiControlForCell;

            case FlexiControl:
                return ParentType.FlexiControl;

            default:
                return ParentType.Nothing;
        }
    }

    public Bitmap? BitmapOfControl() {
        if (_generatingBitmapOfControl) { return null; }

        _generatingBitmapOfControl = true;

        UseBackgroundBitmap = true;

        try {
            if (_bitmapOfControl == null) { Refresh(); }
            return _bitmapOfControl;
        } finally {
            _generatingBitmapOfControl = false;
        }
    }

    public void CheckBack() => _pform = ParentForm();

    public bool ContainsMouse() => DoDrawings() && ClientRectangle.Contains(PointToClient(Cursor.Position));

    public bool DoDrawings() {
        if (IsDisposed || Disposing) { return false; }
        if (DesignMode) { return true; }
        if (_pform is not { IsDisposed: false, Visible: true }) { return false; }
        if (_pform is Forms.Form { IsClosing: true }) { return false; }
        return Visible;
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
        return DoDrawings() ? PointToClient(Cursor.Position) : default;
    }

    public override void Refresh() {
        if (DoDrawings()) {
            DoDraw(CreateGraphics());
        }
    }

    internal static bool AllEnabled(Control control) {
        while (control is { IsDisposed: false }) {
            if (!control.Enabled) { return false; }
            control = control.Parent;
        }
        return true;
    }

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);

        OnDisposingEvent();

        if (disposing) {
            _bitmapOfControl?.Dispose();
            _bitmapOfControl = null;
            Tag = null;
        }
    }

    protected virtual void DrawControl(Graphics gr, States state) => Develop.DebugPrint_RoutineMussUeberschriebenWerden(false);

    protected ParentType GetParentType() {
        if (Parent == null) { return ParentType.Unbekannt; }
        if (_myParentType != ParentType.Unbekannt) { return _myParentType; }
        _myParentType = Typ(Parent);
        return _myParentType;
    }

    protected bool MousePressing() => _mousePressing;

    protected override void OnControlAdded(ControlEventArgs e) {
        base.OnControlAdded(e);

        if (e.Control is ISendsFocusedChild sfc) {
            sfc.ChildGotFocus += Sfc_ChildGotFocus;
        }
        e.Control.GotFocus += Control_GotFocus;
    }

    protected override void OnControlRemoved(ControlEventArgs e) {
        base.OnControlRemoved(e);

        if (e.Control is ISendsFocusedChild sfc) {
            sfc.ChildGotFocus -= Sfc_ChildGotFocus;
        }
        e.Control.GotFocus -= Control_GotFocus;
    }

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
        base.OnSizeChanged(e);
        if (_bitmapOfControl != null) {
            if (_bitmapOfControl.Width < Width || _bitmapOfControl.Height < Height) {
                _bitmapOfControl?.Dispose();
                _bitmapOfControl = null;
            }
        }
        Invalidate();
    }

    protected Form? ParentForm() => ParentForm(Parent);

    protected void SetDoubleBuffering() {
        DoubleBuffered = true;
        SetStyle(ControlStyles.DoubleBuffer, true);
        SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        SetStyle(ControlStyles.AllPaintingInWmPaint, true);
        SetStyle(ControlStyles.UserPaint, true);
        UpdateStyles();
    }

    protected void SetNotFocusable() {
        if (IsDisposed) { return; }
        TabStop = false;
        TabIndex = 0;
        CausesValidation = false;
        SetStyle(ControlStyles.Selectable, false);
        UpdateStyles();
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

    private void Control_GotFocus(object sender, System.EventArgs e) {
        if (sender is not Control c) { return; }
        OnChildGotFocus(new ControlEventArgs(c));
    }

    private void DoDraw(Graphics gr) {
        if (_pform == null) { CheckBack(); }

        if (!DoDrawings()) {
            gr.Clear(Color.LightGray);
            return;
        }

        if (Develop.Exited || IsDisposed || !Visible) { return; }
        lock (this) {
            if (!Skin.Inited) {
                if (DesignMode) {
                    Skin.LoadSkin();
                } else {
                    return;
                }
            }

            ScaleX = gr.DpiX / 96f;
            ScaleY = gr.DpiY / 96f;

            ScaledDisplayRectangle = new Rectangle(0, 0, (int)(ClientSize.Width / ScaleX + 0.9999), (int)(ClientSize.Height / ScaleX + 0.9999));

            if (ScaledDisplayRectangle.Width < 2 || ScaledDisplayRectangle.Height < 2) { return; }

            try {
                if (_useBackBitmap) {
                    _bitmapOfControl ??= new Bitmap(DisplayRectangle.Width, DisplayRectangle.Height, PixelFormat.Format32bppPArgb);
                    using var tmpgr = Graphics.FromImage(_bitmapOfControl);
                    DrawControl(tmpgr, IsStatus());
                    if (_bitmapOfControl != null) {
                        gr.ScaleTransform(1, 1);
                        gr.DrawImage(_bitmapOfControl, 0, 0);
                    }
                } else {
                    DrawControl(gr, IsStatus());
                }

                if (DesignMode) {
                    using var p = new Pen(Color.FromArgb(128, 255, 0, 0));
                    gr.DrawRectangle(p, 0, 0, Width - 1, Height - 1);
                }
            } catch {
                // Consider logging the exception
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
        } else if (GetStyle(ControlStyles.Selectable) && CanFocus && Focused) {
            s |= States.Standard_HasFocus;
        }

        return s;
    }

    private void OnChildGotFocus(ControlEventArgs e) => ChildGotFocus?.Invoke(this, e);

    private void OnDisposingEvent() => DisposingEvent?.Invoke(this, System.EventArgs.Empty);

    private void Sfc_ChildGotFocus(object sender, ControlEventArgs e) => OnChildGotFocus(e);

    #endregion
}
// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Runtime.CompilerServices;
using BlueControls.Classes;

namespace BlueControls.Controls;

public class GenericControl : System.Windows.Forms.Control, IDisposableExtendedWithEvent {

    #region Fields

    private readonly bool _mouseHighlight;
    private Bitmap? _bitmapOfControl;
    private bool _generatingBitmapOfControl;
    private ParentType _myParentType = ParentType.Unbekannt;
    private States? _state;

    #endregion

    #region Constructors

    protected GenericControl() : this(false, false, false) { }

    protected GenericControl(bool doubleBuffer, bool useBackgroundBitmap, bool mouseHighlight) {
        UseBackgroundBitmap = useBackgroundBitmap;
        _mouseHighlight = mouseHighlight;

        // ResizeRedraw/Transparent aus -> wir malen alles selbst, kein Windows-Ballast
        SetStyle(System.Windows.Forms.ControlStyles.ResizeRedraw |
                 System.Windows.Forms.ControlStyles.SupportsTransparentBackColor, false);

        // Voll selbst zeichnen, Opaque -> Windows malt KEINEN Hintergrund
        SetStyle(System.Windows.Forms.ControlStyles.Opaque |
                 System.Windows.Forms.ControlStyles.UserPaint |
                 System.Windows.Forms.ControlStyles.AllPaintingInWmPaint, true);

        // DoubleBuffer nur wenn nicht eh ein Background-Bitmap als eigener Buffer dient
        SetStyle(System.Windows.Forms.ControlStyles.OptimizedDoubleBuffer, doubleBuffer && !UseBackgroundBitmap);

        UpdateStyles();
    }

    #endregion

    #region Events

    public event EventHandler<System.Windows.Forms.ControlEventArgs>? ChildGotFocus;

    public event EventHandler? DisposingEvent;

    #endregion

    #region Properties

    [DefaultValue(false)]
    public override bool AutoSize {
        get => false;
        set => base.AutoSize = false;
    }

    public bool ContainsMouse { get; private set; }
    public override bool Focused => base.Focused || ContainsFocus;

    public bool IsSelectable {
        get;
        protected set {
            if (IsDisposed) { return; }

            if (field == value) { return; }
            field = value;
            TabStop = field;
            TabIndex = 0;
            CausesValidation = field;
            SetStyle(System.Windows.Forms.ControlStyles.Selectable, field);
            UpdateStyles();
        }
    } = true;

    public System.Windows.Forms.Form? ParentForm {
        get {
            if (field is not null) { return field; }
            field = FindForm();
            return field;
        }
        private set {
            field = value;
        }
    }

    [Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
    [Category("Darstellung"), DefaultValue("")]
    public virtual string QuickInfo {
        get;
        set {
            var source = value.AsSpan().Trim();

            while (source.StartsWith("<br>", StringComparison.OrdinalIgnoreCase)) {
                source = source[4..].Trim();
            }
            while (source.EndsWith("<br>", StringComparison.OrdinalIgnoreCase)) {
                source = source[..^4].Trim();
            }

            if (!source.Equals(field.AsSpan(), StringComparison.Ordinal)) {
                Forms.QuickInfo.Close();
                field = source.ToString();
                OnQuickInfoChanged();
                DoQuickInfo();
            }
        }
    } = string.Empty;

    /// <summary>
    /// Gibt an, ob aktuell eine Maustaste über diesem Control gedrückt gehalten wird.
    /// Wird in <see cref="OnMouseDown"/> gesetzt und in <see cref="OnMouseUp"/> sowie
    /// <see cref="OnMouseMove"/> (bei keiner gedrückten Taste) zurückgesetzt.
    /// </summary>
    protected bool MousePressing { get; private set; }

    protected override bool ScaleChildren => false;

    protected bool UseBackgroundBitmap {
        get;
        set {
            if (field == value) { return; }
            field = value;
            DisposeBitmap();
        }
    }

    #endregion

    #region Methods

    public static ParentType Typ(System.Windows.Forms.Control? control) => control switch {
        null => ParentType.Nothing,
        System.Windows.Forms.GroupBox g when g.Parent is System.Windows.Forms.TabPage { Parent: RibbonBar } => ParentType.RibbonGroupBox,
        System.Windows.Forms.GroupBox => ParentType.GroupBox,
        LastFilesCombo => ParentType.LastFilesCombo,
        System.Windows.Forms.ComboBox b when Typ(b) == ParentType.RibbonPage => ParentType.RibbonBarCombobox,
        System.Windows.Forms.ComboBox => ParentType.ComboBox,
        RibbonBar => ParentType.RibbonControl,
        System.Windows.Forms.TabControl => ParentType.TabControl,
        System.Windows.Forms.TabPage p when p.Parent is RibbonBar => ParentType.RibbonPage,
        System.Windows.Forms.TabPage => ParentType.TabPage,
        Slider => ParentType.Slider,
        FloatingForm => ParentType.MsgBox,
        DialogWithOkAndCancel => ParentType.MsgBox,
        System.Windows.Forms.TextBox => ParentType.TextBox,
        System.Windows.Forms.ListBox => ParentType.ListBox,
        EasyPic => ParentType.EasyPic,
        System.Windows.Forms.Button => ParentType.Button,
        Line => ParentType.Line,
        Caption => ParentType.Caption,
        Form => ParentType.Form,
        TableView => ParentType.Table,
        System.Windows.Forms.Panel => ParentType.Panel,
        FlexiControlForCell => ParentType.FlexiControlForCell,
        FlexiControl => ParentType.FlexiControl,
        _ => ParentType.Nothing
    };

    public Bitmap? BitmapOfControl() {
        if (_generatingBitmapOfControl) { return null; }
        _generatingBitmapOfControl = true;
        UseBackgroundBitmap = true;

        try {
            if (_bitmapOfControl is null) { Refresh(); }
            return _bitmapOfControl;
        } finally {
            _generatingBitmapOfControl = false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool DoDrawings() => !Develop.Exited && !IsDisposed && !Disposing && (DesignMode || ParentForm is { IsDisposed: false, Visible: true });

    public void DoQuickInfo() {
        if (InvokeRequired) {
            BeginInvoke(DoQuickInfo);
            return;
        }

        if (!string.IsNullOrEmpty(QuickInfo) && ContainsMouse) {
            Forms.QuickInfo.Show(QuickInfo);
        } else {
            Forms.QuickInfo.Close();
        }
    }

    public new void Invalidate() {
        if (!DoDrawings()) { return; }
        _state = null;
        base.Invalidate();
    }

    public new void Invalidate(Rectangle rc) {
        if (!DoDrawings()) { return; }
        _state = null;
        base.Invalidate(rc);
    }

    public Point MousePos() {
        if (InvokeRequired) { return Invoke(new Func<Point>(MousePos)); }
        if (DoDrawings()) {
            return PointToClient(System.Windows.Forms.Cursor.Position);
        }
        return default;
    }

    public override void Refresh() {
        if (!DoDrawings()) { return; }
        _state = null;
        base.Invalidate();
        Update();
    }

    internal static bool AllEnabled(System.Windows.Forms.Control? control) {
        var current = control;
        while (current is { IsDisposed: false }) {
            if (!current.Enabled) { return false; }
            current = current.Parent;
        }
        return true;
    }

    protected override void Dispose(bool disposing) {
        if (IsDisposed) { return; }

        OnDisposingEvent();
        DisposingEvent = null;
        ChildGotFocus = null;

        if (disposing) {
            DisposeBitmap();
            Tag = null;
        }

        base.Dispose(disposing);
    }

    protected virtual void DrawControl(Graphics gr, States state) { }

    protected ParentType GetParentType() {
        if (Parent is null) { return ParentType.Unbekannt; }
        if (_myParentType != ParentType.Unbekannt) { return _myParentType; }
        _myParentType = Typ(Parent);
        return _myParentType;
    }

    protected override Rectangle GetScaledBounds(Rectangle bounds, SizeF factor, System.Windows.Forms.BoundsSpecified specified) => bounds;

    protected override void OnControlAdded(System.Windows.Forms.ControlEventArgs e) {
        base.OnControlAdded(e);
        if (e.Control is TabControl sfc) {
            sfc.ChildGotFocus += Sfc_ChildGotFocus;
        }
        e.Control.GotFocus += Control_GotFocus;
    }

    protected override void OnControlRemoved(System.Windows.Forms.ControlEventArgs e) {
        base.OnControlRemoved(e);
        if (e.Control is TabControl sfc) {
            sfc.ChildGotFocus -= Sfc_ChildGotFocus;
        }
        e.Control.GotFocus -= Control_GotFocus;
    }

    protected override void OnEnabledChanged(System.EventArgs e) {
        if (IsDisposed) { return; }
        if (InvokeRequired) {
            BeginInvoke(() => OnEnabledChanged(e));
            return;
        }
        _state = null;
        base.OnEnabledChanged(e);
        Invalidate();
    }

    protected override void OnGotFocus(System.EventArgs e) {
        if (IsDisposed) { return; }
        if (!IsSelectable) {
            Parent?.SelectNextControl(this, true, true, true, true);
        } else {
            base.OnGotFocus(e);
            _state = null;
            Invalidate();
        }
    }

    protected override void OnLostFocus(System.EventArgs e) {
        if (IsDisposed) { return; }
        if (IsSelectable) {
            base.OnLostFocus(e);
            _state = null;
            Invalidate();
        }
    }

    protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e) {
        Forms.QuickInfo.Close();
        if (!DoDrawings() || MousePressing) { return; }

        MousePressing = true;
        _state = null;
        if (Enabled && IsSelectable) {
            Focus();
        }
        base.OnMouseDown(e);
    }

    protected override void OnMouseEnter(System.EventArgs e) {
        ContainsMouse = true;
        if (_mouseHighlight) { _state = null; }
        DoQuickInfo();
        if (DoDrawings()) {
            base.OnMouseEnter(e);
        }
    }

    protected override void OnMouseLeave(System.EventArgs e) {
        ContainsMouse = false;
        if (_mouseHighlight) { _state = null; }
        DoQuickInfo();
        if (DoDrawings()) {
            base.OnMouseLeave(e);
        }
    }

    protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e) {
        if (e.Button == System.Windows.Forms.MouseButtons.None) { MousePressing = false; }

        if (!DoDrawings()) { return; }
        DoQuickInfo();
        base.OnMouseMove(e);
    }

    protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e) {
        Forms.QuickInfo.Close();
        MousePressing = false;
        _state = null;
        if (!DoDrawings()) { return; }
        base.OnMouseUp(e);
    }

    protected override void OnPaint(System.Windows.Forms.PaintEventArgs e) => DoDraw(e.Graphics);

    protected override void OnParentChanged(System.EventArgs e) {
        base.OnParentChanged(e);
        _myParentType = ParentType.Unbekannt;
        ParentForm = null;
        _state = null;
    }

    protected override void OnParentEnabledChanged(System.EventArgs e) {
        _state = null;
        base.OnParentEnabledChanged(e);
        Invalidate();
    }

    protected virtual void OnQuickInfoChanged() { }

    protected override void OnSizeChanged(System.EventArgs e) {
        if (IsDisposed) { return; }
        base.OnSizeChanged(e);

        // Bitmap nur neu anlegen wenn wirklich zu klein - schrumpfen wird ignoriert,
        // dadurch deutlich weniger Allokationen bei häufigem Resize
        if (_bitmapOfControl is { } bmp && (bmp.Width < Width || bmp.Height < Height)) {
            DisposeBitmap();
        }
        Invalidate();
    }

    protected override void ScaleControl(SizeF factor, System.Windows.Forms.BoundsSpecified specified) => base.ScaleControl(new SizeF(1, 1), specified);

    protected override void WndProc(ref System.Windows.Forms.Message m) {
        if (Develop.Exited) { return; }

        // Windows-Hintergrund-/Sync-Painting komplett blocken -> kein Flicker, kein Ballast
        switch (m.Msg) {
            case (int)Enums.WndProc.WM_ERASEBKGND:
                m.Result = (IntPtr)1; // "handled"
                return;

            case (int)Enums.WndProc.WM_SYNCPAINT:
            case (int)Enums.WndProc.WM_PRINTCLIENT:
                return;
        }

        base.WndProc(ref m);
    }

    private States ComputeState() {
        if (!AllEnabled(this)) { return States.Standard_Disabled; }

        var s = States.Standard;
        if (_mouseHighlight && ContainsMouse) { s |= States.Standard_MouseOver; }

        if (MousePressing) {
            if (_mouseHighlight) { s |= States.Standard_MousePressed; }
            if (IsSelectable && CanFocus) { s |= States.Standard_HasFocus; }
        } else if (Focused && IsSelectable) {
            s |= States.Standard_HasFocus;
        }

        return s;
    }

    private void Control_GotFocus(object? sender, System.EventArgs e) {
        if (sender is System.Windows.Forms.Control c) {
            OnChildGotFocus(new System.Windows.Forms.ControlEventArgs(c));
        }
    }

    private void DisposeBitmap() {
        _bitmapOfControl?.Dispose();
        _bitmapOfControl = null;
    }

    private void DoDraw(Graphics gr) {
        if (Develop.Exited || IsDisposed || !Visible || Width < 1 || Height < 1) {
            return;
        }

        if (!DoDrawings()) {
            gr.Clear(Color.LightGray);
            return;
        }

        if (!Skin.Inited) {
            if (DesignMode) { Skin.LoadSkin(); } else { return; }
        }

        _state ??= ComputeState();

        var status = (States)_state;
        var w = ClientRectangle.Width;
        var h = ClientRectangle.Height;

        if (UseBackgroundBitmap) {
            if (_bitmapOfControl is null || _bitmapOfControl.Width < w || _bitmapOfControl.Height < h) {
                DisposeBitmap();
                _bitmapOfControl = new Bitmap(Math.Max(w, 1), Math.Max(h, 1), PixelFormat.Format32bppPArgb);
            }

            using var tmpgr = Graphics.FromImage(_bitmapOfControl);
            tmpgr.CompositingQuality = CompositingQuality.HighSpeed;
            tmpgr.InterpolationMode = InterpolationMode.Low;
            DrawControl(tmpgr, status);
            gr.DrawImageUnscaled(_bitmapOfControl, 0, 0);
        } else {
            DrawControl(gr, status);
        }

        if (DesignMode) {
            using var p = new Pen(Color.FromArgb(128, Color.Red));
            gr.DrawRectangle(p, 0, 0, Width - 1, Height - 1);
        }
    }

    private void OnChildGotFocus(System.Windows.Forms.ControlEventArgs e) => ChildGotFocus?.Invoke(this, e);

    private void OnDisposingEvent() => DisposingEvent?.Invoke(this, System.EventArgs.Empty);

    private void Sfc_ChildGotFocus(object? sender, System.Windows.Forms.ControlEventArgs e) => OnChildGotFocus(e);

    #endregion
}
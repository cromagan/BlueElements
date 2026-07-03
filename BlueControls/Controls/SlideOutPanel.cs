// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes;
using BlueTable.Interfaces;
using System.Windows.Forms;

namespace BlueControls.Controls;

/// <summary>
/// Herausfahrendes Panel, das wie eine Registerkarte am oberen oder unteren
/// Rand seines Parents andockt. Im eingefahrenen Zustand ist nur die schmale
/// Registerkarte sichtbar (<see cref="TabSize" /> Pixel hoch). Fährt die Maus
/// über das Control, wird der Body smooth herausgefahren (200 ms, Smoothstep-
/// Easing analog <see cref="Forms.QuickNote" />). Verlässt die Maus das
/// Control, fährt es wieder ein.
/// Über <see cref="TabAtTop" /> wird festgelegt, ob die Registerkarte am
/// oberen (true) oder unteren (false) Rand liegt.
/// Im Designer wird das Panel frei platziert. Sobald es zur Laufzeit
/// erstmals angezeigt wird, rutscht es automatisch an den entsprechenden
/// Rand seines Parents (volle Breite, Registerkarte bündig mit dem Rand).
/// Der Body kann beliebige Child-Controls aufnehmen, die innerhalb von
/// <see cref="BodyClientArea" /> platziert werden sollten.
/// </summary>
/// <remarks>
/// Basisklasse ist die hauseigene <see cref="Controls.GroupBox" />, damit der
/// WinForms-Designer das Control als Container erkennt (GroupBox ist über
/// ihren framework-seitigen Designer automatisch ein Container) und beliebige
/// Child-Controls aufnehmen kann. Die GroupBox-eigene Rahmenzeichnung wird
/// durch eine eigene <see cref="OnPaint" />-Implementierung vollständig
/// ersetzt.
/// </remarks>
[DefaultEvent(nameof(ExpandedChanged))]
public sealed class SlideOutPanel : GroupBox, ITranslateable {

    #region Fields

    private const int DefaultTabSize = 25;
    private const int DefaultExpandedHeight = 400;
    private const double SlideDurationMs = 200;

    private double _animProgress;
    private DateTime _animStart;
    private double _animStartProgress;
    private double _animTargetProgress;
    private bool _containsMouse;
    private System.Windows.Forms.Control? _hookedParent;
    private System.Windows.Forms.Timer? _timer;

    #endregion

    #region Constructors

    public SlideOutPanel() {
        DoubleBuffered = true;
        SetStyle(System.Windows.Forms.ControlStyles.Selectable, false);
        // Initial eingefahren darstellen
        _animProgress = 0;
        UpdateSize();
    }

    #endregion

    #region Events

    public event EventHandler? ExpandedChanged;

    #endregion

    #region Properties

    /// <summary>
    /// Liefert den Bereich innerhalb des Panels, in dem Child-Controls
    /// platziert werden sollten (also ohne die Registerkarte).
    /// Im eingefahrenen Zustand hat dieser Bereich die Größe 0.
    /// </summary>
    public Rectangle BodyClientArea => BodyRectangle;

    private Rectangle BodyRectangle => TabAtTop
        ? new Rectangle(0, TabSize, Width, Math.Max(0, Height - TabSize))
        : new Rectangle(0, 0, Width, Math.Max(0, Height - TabSize));

    /// <summary>
    /// Vollständige Höhe des Panels im ausgefahrenen Zustand (inklusive
    /// Registerkarte). Die Breite richtet sich zur Laufzeit nach dem Parent.
    /// </summary>
    [DefaultValue(DefaultExpandedHeight)]
    public int ExpandedHeight {
        get;
        set {
            if (field == value) { return; }
            field = Math.Max(TabSize + 1, value);
            UpdateSize();
            Invalidate();
        }
    } = DefaultExpandedHeight;

    public bool IsExpanded { get; private set; }

    /// <summary>
    /// Bestimmt den Rand, an dem die Registerkarte liegt und an den das
    /// Panel zur Laufzeit andockt.
    /// true  → Registerkarte oben,  Body fährt nach unten heraus.
    /// false → Registerkarte unten, Body fährt nach oben heraus.
    /// </summary>
    [DefaultValue(true)]
    public bool TabAtTop {
        get;
        set {
            if (field == value) { return; }
            field = value;
            UpdateSize();
            Invalidate();
        }
    } = true;

    public QuickImage? TabImage {
        get;
        set {
            if (field == value) { return; }
            field = value;
            Invalidate();
        }
    }

    /// <summary>
    /// Benötigt, damit der Designer das nicht erstellt.
    /// </summary>
    [DefaultValue(0)]
    public new int TabIndex {
        get => 0;
        set { _ = value; base.TabIndex = 0; }
    }

    /// <summary>
    /// Höhe der Registerkarte in Pixel.
    /// </summary>
    [DefaultValue(DefaultTabSize)]
    public int TabSize {
        get;
        set {
            if (field == value) { return; }
            field = Math.Max(5, value);
            UpdateSize();
            Invalidate();
        }
    } = DefaultTabSize;

    /// <summary>
    /// Benötigt, damit der Designer das nicht erstellt.
    /// </summary>
    [DefaultValue(false)]
    public new bool TabStop {
        get => false;
        set { _ = value; base.TabStop = false; }
    }

    [DefaultValue("")]
    public string TabText {
        get;
        set {
            if (field == value) { return; }
            field = value;
            Invalidate();
        }
    } = string.Empty;

    [DefaultValue(true)]
    public bool Translate { get; set; } = true;

    private Rectangle TabRectangle {
        get {
            var ts = Math.Min(TabSize, Height);
            return TabAtTop
                ? new Rectangle(0, 0, Width, ts)
                : new Rectangle(0, Math.Max(0, Height - ts), Width, ts);
        }
    }

    #endregion

    #region Methods

    public void Collapse() {
        if (!IsExpanded) { return; }
        IsExpanded = false;
        StartAnimation(0.0);
        OnExpandedChanged();
    }

    public void Expand() {
        if (IsExpanded) { return; }
        IsExpanded = true;
        StartAnimation(1.0);
        OnExpandedChanged();
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            if (_hookedParent is not null) {
                _hookedParent.SizeChanged -= Parent_SizeChanged;
                _hookedParent = null;
            }
            if (_timer is not null) {
                _timer.Tick -= Timer_Tick;
                _timer.Dispose();
                _timer = null;
            }
        }
        base.Dispose(disposing);
    }

    /// <summary>
    /// Zeichnet Body- und Registerkarte vollständig selbst. Die
    /// GroupBox-Rahmenzeichnung der Basisklasse wird bewusst nicht
    /// aufgerufen (<c>base.OnPaint</c> fehlt absichtlich).
    /// </summary>
    protected override void OnPaint(PaintEventArgs e) {
        if (IsDisposed) { return; }

        var gr = e.Graphics;
        var bodyRect = BodyRectangle;
        var tabRect = TabRectangle;

        // Body nur zeichnen, wenn er überhaupt sichtbare Fläche hat.
        // Optik wie eine TabPage eines TabControls (inkl. Transparenz).
        if (bodyRect.Width > 0 && bodyRect.Height > 0) {
            Skin.Draw_Back(gr, Design.TabStrip_Body, States.Standard, bodyRect, this, true);
            Skin.Draw_Border(gr, Design.TabStrip_Body, States.Standard, bodyRect);
        }

        // Registerkarte wie ein echtes TabControl-Register darstellen:
        // aktiviert (Checked) wenn ausgefahren, Hover wenn die Maus darüber steht.
        var tabState = States.Standard;
        if (IsExpanded) { tabState |= States.Checked; }
        if (_containsMouse) { tabState |= States.Standard_MouseOver; }

        Skin.Draw_Back(gr, Design.TabStrip_Head, tabState, tabRect, this, true);
        Skin.Draw_Border(gr, Design.TabStrip_Head, tabState, tabRect);

        var text = LanguageTool.DoTranslate(TabText, Translate);
        Skin.Draw_FormatedText(gr, text, TabImage, Alignment.Horizontal_Vertical_Center,
            tabRect, Design.TabStrip_Head, tabState, this, false, false);
    }

    protected override void OnMouseEnter(System.EventArgs e) {
        base.OnMouseEnter(e);
        _containsMouse = true;
        if (DesignMode) { return; }
        Expand();
    }

    protected override void OnMouseLeave(System.EventArgs e) {
        base.OnMouseLeave(e);
        _containsMouse = false;
        if (DesignMode) { return; }
        // OnMouseLeave feuert auch, wenn die Maus über ein Child-Control bewegt
        // wird. Deshalb prüfen, ob die Maus wirklich außerhalb des Client-Bereichs ist.
        var clientPos = PointToClient(Cursor.Position);
        if (!ClientRectangle.Contains(clientPos)) {
            Collapse();
        }
    }

    protected override void OnParentChanged(System.EventArgs e) {
        base.OnParentChanged(e);
        HookParent();
        SnapToEdge();
    }

    /// <summary>
    /// Smoothstep-Easing (3t² - 2t³), gleicher Curve-Typ wie QuickNote.
    /// </summary>
    private static double EaseInOut(double t) => (3.0 - 2.0 * t) * t * t;

    private int ComputeHeight() {
        var bodyH = Math.Max(0, ExpandedHeight - TabSize);
        return (int)(TabSize + bodyH * _animProgress);
    }

    private void EnsureTimer() {
        if (_timer is null) {
            _timer = new System.Windows.Forms.Timer {
                Interval = 16
            };
            _timer.Tick += Timer_Tick;
        }
    }

    private void HookParent() {
        if (_hookedParent == Parent) { return; }
        if (_hookedParent is not null) {
            _hookedParent.SizeChanged -= Parent_SizeChanged;
            _hookedParent = null;
        }
        if (Parent is not null) {
            _hookedParent = Parent;
            _hookedParent.SizeChanged += Parent_SizeChanged;
        }
    }

    private void OnExpandedChanged() => ExpandedChanged?.Invoke(this, System.EventArgs.Empty);

    private void Parent_SizeChanged(object? sender, System.EventArgs e) => SnapToEdge();

    /// <summary>
    /// Setzt das Panel im Laufzeitmodus bündig an den Rand seines Parents
    /// (volle Breite, Registerkarte am oberen/unteren Rand). Im Designer
    /// bleibt das Panel dort, wo es platziert wurde.
    /// </summary>
    private void SnapToEdge() {
        if (DesignMode || IsDisposed || Disposing || Parent is null) { return; }
        if (Dock != DockStyle.None) { Dock = DockStyle.None; }
        BringToFront();
        UpdateSize();
    }

    private void StartAnimation(double targetProgress) {
        _animStartProgress = _animProgress;
        _animTargetProgress = targetProgress;
        _animStart = DateTime.UtcNow;
        EnsureTimer();
        if (!_timer!.Enabled) {
            _timer.Start();
        }
    }

    private void Timer_Tick(object? sender, System.EventArgs e) {
        var elapsed = DateTime.UtcNow - _animStart;
        var rawProgress = Math.Min(1.0, elapsed.TotalMilliseconds / SlideDurationMs);
        var eased = EaseInOut(rawProgress);

        _animProgress = _animStartProgress + (_animTargetProgress - _animStartProgress) * eased;
        UpdateSize();
        Invalidate();

        if (rawProgress >= 1.0) {
            _animProgress = _animTargetProgress;
            UpdateSize();
            _timer!.Stop();
        }
    }

    private void UpdateSize() {
        if (DesignMode) { return; }

        var h = ComputeHeight();

        if (Parent is null) {
            if (Height != h) { Height = h; }
            return;
        }

        var width = Parent.ClientSize.Width;
        var y = TabAtTop ? 0 : Math.Max(0, Parent.ClientSize.Height - h);
        var newBounds = new Rectangle(0, y, width, h);
        if (Bounds != newBounds) { Bounds = newBounds; }
    }

    #endregion
}

// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes;
using BlueTable.Interfaces;
using System.Windows.Forms;

namespace BlueControls.Controls;

/// <summary>
/// Herausfahrendes Panel, das am oberen oder unteren Rand seines Parents
/// andockt. Die Größe des Panels bleibt dabei immer unverändert
/// (<see cref="ExpandedHeight" />) — ein- und ausgefahren wird ausschließlich
/// über die Position (Y) gesteuert, und zwar von der <see cref="Animator" />-
/// Engine über das <see cref="IAnimatable" />-Interface.
/// Im eingefahrenen Zustand ragen nur <see cref="TabSize" /> Pixel in den
/// sichtbaren Bereich des Parents. Fährt die Maus über das Control, wird das
/// Panel smooth herausgefahren (200 ms, Smoothstep-Easing). Verlässt die Maus
/// das Control, fährt es wieder ein.
/// Über <see cref="TabAtTop" /> wird festgelegt, ob das Panel am oberen Rand
/// andockt (Body fährt nach unten, true) oder am unteren Rand (Body fährt nach
/// oben, false).
/// Die Zeichenroutine stammt vollständig von der Basisklasse <see cref="GroupBox" />
/// — es gibt kein eigenes OnPaint, GroupBox zeichnet Rahmen und Caption
/// (<see cref="Control.Text" />) wie gewohnt.
/// </summary>
[DefaultEvent(nameof(ExpandedChanged))]
public sealed class SlideOutPanel : GroupBox, IAnimatable, ITranslateable {

    #region Fields

    private const int DefaultTabSize = 25;
    private const int DefaultExpandedHeight = 400;
    private const double SlideDurationMs = 200;

    // Animations-Fortschritt: 0 = eingefahren, 1 = ausgefahren.
    // Wird von Animate() (Animations-Thread) geschrieben und von
    // StartSlide() (UI-Thread) gelesen — int/double Reads sind auf x86/x64
    // atomar, ein allenfalls veralteter Wert kostet maximal einen Frame.
    private double _animProgress;
    private double _animStartProgress;
    private double _animTargetProgress;

    // Y-Positionen (parent-relativ), vor dem Start der Animation auf dem
    // UI-Thread gecacht, damit Animate() keine WinForms-Properties liest.
    private int _animCollapsedY;
    private int _animExpandedY;

    private volatile bool _isAnimating;
    private System.Windows.Forms.Control? _hookedParent;

    #endregion

    #region Constructors

    public SlideOutPanel() {
        DoubleBuffered = true;
        SetStyle(System.Windows.Forms.ControlStyles.Selectable, false);
        _animProgress = 0;
    }

    #endregion

    #region Events

    public event EventHandler? ExpandedChanged;

    #endregion

    #region Properties

    /// <summary>
    /// Vollständige Höhe des Panels (immer konstant, ändert sich nie).
    /// Die Breite richtet sich zur Laufzeit nach dem Parent.
    /// </summary>
    [DefaultValue(DefaultExpandedHeight)]
    public int ExpandedHeight {
        get;
        set {
            if (field == value) { return; }
            field = Math.Max(TabSize + 1, value);
            UpdateLayout();
        }
    } = DefaultExpandedHeight;

    public bool IsExpanded { get; private set; }

    /// <summary>
    /// Bestimmt den Rand, an dem das Panel andockt und die Slide-Richtung.
    /// true  → Panel am oberen Rand, Body fährt nach unten heraus.
    /// false → Panel am unteren Rand, Body fährt nach oben heraus.
    /// </summary>
    [DefaultValue(true)]
    public bool TabAtTop {
        get;
        set {
            if (field == value) { return; }
            field = value;
            UpdateLayout();
        }
    } = true;

    /// <summary>
    /// Anzahl Pixel, die im eingefahrenen Zustand in den Parent hineinragen
    /// (als Interaktionsfläche für die Maus).
    /// </summary>
    [DefaultValue(DefaultTabSize)]
    public int TabSize {
        get;
        set {
            if (field == value) { return; }
            field = Math.Max(5, value);
            UpdateLayout();
        }
    } = DefaultTabSize;

    /// <summary>
    /// Benötigt, damit der Designer das nicht erstellt.
    /// </summary>
    [DefaultValue(0)]
    public new int TabIndex {
        get => 0;
        set { _ = value; base.TabIndex = 0; }
    }

    /// <summary>
    /// Benötigt, damit der Designer das nicht erstellt.
    /// </summary>
    [DefaultValue(false)]
    public new bool TabStop {
        get => false;
        set { _ = value; base.TabStop = false; }
    }

    [DefaultValue(true)]
    public bool Translate { get; set; } = true;

    #endregion

    #region IAnimatable

    /// <summary>
    /// Berechnet das Frame der Slide-Animation aus der seit Start verstrichenen
    /// Zeit. Wird auf dem Animations-Thread aufgerufen und liest ausschließlich
    /// gecachte Felder — keine WinForms-Properties.
    /// </summary>
    public AnimationFrame Animate(TimeSpan elapsed) {
        var rawProgress = Math.Min(1.0, elapsed.TotalMilliseconds / SlideDurationMs);
        var eased = EaseInOut(rawProgress);

        var progress = _animStartProgress + (_animTargetProgress - _animStartProgress) * eased;
        _animProgress = progress;

        var y = _animCollapsedY + (int)((_animExpandedY - _animCollapsedY) * progress);

        var finished = rawProgress >= 1.0;
        if (finished) {
            _animProgress = _animTargetProgress;
            y = _animTargetProgress > 0.5 ? _animExpandedY : _animCollapsedY;
        }

        return new AnimationFrame { Opacity = 1, X = 0, Y = y, Finished = finished };
    }

    void IAnimatable.Close() => ((IAnimatable)this).StopAnimation();

    /// <summary>
    /// Animation beendet — Panel bleibt an der Zielposition, wird NICHT
    /// geschlossen (anders als bei FloatingForms). Überschreibt den
    /// Default von <see cref="IAnimatable.OnAnimationFinished" />, der das
    /// Objekt schließen würde.
    /// </summary>
    void IAnimatable.OnAnimationFinished() {
        _isAnimating = false;
        if (!IsHandleCreated || Disposing || IsDisposed) { return; }
        try {
            this.BeginInvoke(new Action(UpdateLayout), null);
        } catch (Exception ex) {
            Develop.DebugPrint("OnAnimationFinished: UpdateLayout konnte nicht gemarshalled werden", ex);
        }
    }

    /// <summary>
    /// Startet die Animation über den <see cref="Animator" /> — ohne
    /// Layered-Window, da SlideOutPanel ein Child-Control ist, das nur seine
    /// Position ändert. Überschreibt den Default von
    /// <see cref="IAnimatable.StartAnimation" />, der WS_EX_LAYERED setzen würde.
    /// </summary>
    void IAnimatable.StartAnimation() {
        if (!IsHandleCreated) {
            Develop.DebugPrint("StartAnimation vor CreateHandle aufgerufen");
            return;
        }
        Animator.Start(this, false);
    }

    #endregion

    #region Methods

    public void Collapse() {
        if (!IsExpanded) { return; }
        IsExpanded = false;
        OnExpandedChanged();
        StartSlide(0.0);
    }

    public void Expand() {
        if (IsExpanded) { return; }
        IsExpanded = true;
        OnExpandedChanged();
        StartSlide(1.0);
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            ((IAnimatable)this).StopAnimation();
            if (_hookedParent is not null) {
                _hookedParent.SizeChanged -= Parent_SizeChanged;
                _hookedParent = null;
            }
        }
        base.Dispose(disposing);
    }

    protected override void OnMouseEnter(System.EventArgs e) {
        base.OnMouseEnter(e);
        if (DesignMode) { return; }
        Expand();
    }

    protected override void OnMouseLeave(System.EventArgs e) {
        base.OnMouseLeave(e);
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
        UpdateLayout();
    }

    /// <summary>
    /// Smoothstep-Easing (3t² - 2t³), gleicher Curve-Typ wie QuickNote.
    /// </summary>
    private static double EaseInOut(double t) => (3.0 - 2.0 * t) * t * t;

    private int ComputeCollapsedY() {
        if (Parent is null) { return 0; }
        return TabAtTop
            ? -(ExpandedHeight - TabSize)
            : Math.Max(0, Parent.ClientSize.Height - TabSize);
    }

    private int ComputeExpandedY() {
        if (Parent is null) { return 0; }
        return TabAtTop
            ? 0
            : Math.Max(0, Parent.ClientSize.Height - ExpandedHeight);
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

    private void Parent_SizeChanged(object? sender, System.EventArgs e) => UpdateLayout();

    /// <summary>
    /// Startet die Slide-Animation Richtung <paramref name="targetProgress" />
    /// (0 = eingefahren, 1 = ausgefahren). Positionen werden auf dem UI-Thread
    /// gecacht, damit Animate() thread-safe bleibt. Falls Handle/Parent noch
    /// nicht bereit sind, wird nur der Zustand synchronisiert — die Position
    /// korrigiert sich beim nächsten UpdateLayout.
    /// </summary>
    private void StartSlide(double targetProgress) {
        _animStartProgress = _animProgress;
        _animTargetProgress = targetProgress;

        if (DesignMode || IsDisposed || Disposing || Parent is null || !IsHandleCreated) {
            _animProgress = targetProgress;
            return;
        }

        _animCollapsedY = ComputeCollapsedY();
        _animExpandedY = ComputeExpandedY();
        _isAnimating = true;

        ((IAnimatable)this).StartAnimation();
    }

    /// <summary>
    /// Positioniert das Panel am Rand seines Parents (volle Breite, feste
    /// Höhe <see cref="ExpandedHeight" />). Ausgefahren: vollständig sichtbar.
    /// Eingefahren: nur <see cref="TabSize" /> Pixel ragen in den Parent.
    /// Wird während der Animation nicht aufgerufen — dann steuert der
    /// Animator die Y-Position via SetWindowPos.
    /// </summary>
    private void UpdateLayout() {
        if (DesignMode || IsDisposed || Disposing || Parent is null || _isAnimating) { return; }
        if (Dock != DockStyle.None) { Dock = DockStyle.None; }
        BringToFront();

        var width = Parent.ClientSize.Width;
        var y = IsExpanded ? ComputeExpandedY() : ComputeCollapsedY();
        var newBounds = new Rectangle(0, y, width, ExpandedHeight);
        if (Bounds != newBounds) { Bounds = newBounds; }
    }

    #endregion
}

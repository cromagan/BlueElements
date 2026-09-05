// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueTable.Interfaces;
using System.Windows.Forms;

namespace BlueControls.Controls;

/// <summary>
/// Herausfahrendes Panel, das schnurgerade von einer Parent-Kante in den
/// Parent hineinslidet (Richtung siehe SlideFrom). Die Kante,
/// von der es erscheint, gibt die Grenze vor: Erscheint es von unten, steht
/// es im ausgefahrenen Zustand an der im Designer entworfenen Oberkante und
/// berührt mit der Unterkante den Parent; von oben entsprechend umgekehrt.
/// Ein- und ausgefahren wird ausschließlich über die Position gesteuert, und
/// zwar von der Animator-Engine über das
/// IAnimatable-Interface.
/// Im eingefahrenen Zustand ragen nur TabSize Pixel an der
/// Einfahr-Kante in den sichtbaren Bereich des Parents. Fährt die Maus über
/// das Control, wird das Panel smooth herausgefahren (200 ms,
/// Smoothstep-Easing). Verlässt die Maus das Control, fährt es wieder ein.
/// Über SlideFrom wird auch der GroupBox-Stil festgelegt:
/// Top (Stil NormalBoldBottom, die dicke Kopfzeile unten bleibt als Tab
/// sichtbar) oder Bottom (Stil NormalBold, die Kopfzeile oben bleibt
/// sichtbar).
/// Die Zeichenroutine stammt vollständig von der Basisklasse GroupBox
/// — es gibt kein eigenes OnPaint, GroupBox zeichnet Rahmen und Caption
/// (Control.Text) wie gewohnt.
/// </summary>
[DefaultEvent(nameof(ExpandedChanged))]
public sealed class SlideOutPanel : GroupBox, IAnimatable, ITranslateable {

    #region Fields

    private const int DefaultTabSize = 25;
    private const double SlideDurationMs = 200;

    // Ziel-Geometrien (parent-relativ), vor dem Start der Animation auf dem
    // UI-Thread gecacht, damit Animate() keine WinForms-Properties liest.
    private Rectangle _animCollapsedBounds;

    private Rectangle _animExpandedBounds;

    // Animations-Fortschritt: 0 = eingefahren, 1 = ausgefahren.
    // Wird von Animate() (Animations-Thread) geschrieben und von
    // StartSlide() (UI-Thread) gelesen — int/double Reads sind auf x86/x64
    // atomar, ein allenfalls veralteter Wert kostet maximal einen Frame.
    private double _animProgress;

    private double _animStartProgress;
    private double _animTargetProgress;

    // Vom Designer entworfene Position und Größe (Referenz der Zielgeometrie).
    // Wird beim ersten Layout mit erzeugtem Handle gecacht, bevor das Panel
    // erstmals verschoben wird.
    private Rectangle _designBounds;

    private bool _designBoundsCaptured;
    private volatile bool _isAnimating;

    #endregion

    #region Constructors

    public SlideOutPanel() {
        DoubleBuffered = true;
        SetStyle(ControlStyles.Selectable, false);
        SetSlideStyle();
        _animProgress = 0;
    }

    #endregion

    #region Events

    public event EventHandler? ExpandedChanged;

    #endregion

    #region Properties

    /// <summary>
    /// Wird über SlideFrom festgelegt und ist nicht frei wählbar.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new GroupBoxStyle GroupBoxStyle => base.GroupBoxStyle;

    public bool IsExpanded { get; private set; }

    /// <summary>
    /// Legt fest, von welcher Parent-Kante das Panel erscheint, und legt den
    /// GroupBox-Stil fest:
    /// Top    → erscheint von oben, Stil NormalBoldBottom (Kopfzeile unten
    ///          bleibt als Tab sichtbar).
    /// Bottom → erscheint von unten, Stil NormalBold (Kopfzeile oben
    ///          bleibt als Tab sichtbar).
    /// </summary>
    [DefaultValue(SlideFrom.Top)]
    public SlideFrom SlideFrom {
        get;
        set {
            if (field == value) { return; }
            field = value;
            SetSlideStyle();
            UpdateLayout();
        }
    } = SlideFrom.Top;

    /// <summary>
    /// Benötigt, damit der Designer das nicht erstellt.
    /// </summary>
    [DefaultValue(0)]
    public new int TabIndex {
        get => 0;
        set { _ = value; base.TabIndex = 0; }
    }

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
    [DefaultValue(false)]
    public new bool TabStop {
        get => false;
        set { _ = value; base.TabStop = false; }
    }

    [DefaultValue(true)]
    public bool Translate { get; set; } = true;

    #endregion

    #region Methods

    /// <summary>
    /// Berechnet das Frame der Slide-Animation aus der seit Start verstrichenen
    /// Zeit. Wird auf dem Animations-Thread aufgerufen und liest ausschließlich
    /// gecachte Felder — keine WinForms-Properties. X und Breite bleiben
    /// konstant, Y und Höhe werden synchron interpoliert — dadurch slidet das
    /// Panel schnurgerade in Einfahr-Richtung.
    /// </summary>
    public AnimationFrame Animate(TimeSpan elapsed) {
        var rawProgress = Math.Min(1.0, elapsed.TotalMilliseconds / SlideDurationMs);
        var eased = EaseInOut(rawProgress);

        var progress = _animStartProgress + (_animTargetProgress - _animStartProgress) * eased;
        _animProgress = progress;

        var y = _animCollapsedBounds.Y + (int)((_animExpandedBounds.Y - _animCollapsedBounds.Y) * progress);
        var height = _animCollapsedBounds.Height + (int)((_animExpandedBounds.Height - _animCollapsedBounds.Height) * progress);

        var finished = rawProgress >= 1.0;
        if (finished) {
            _animProgress = _animTargetProgress;
            if (_animTargetProgress > 0.5) {
                y = _animExpandedBounds.Y;
                height = _animExpandedBounds.Height;
            } else {
                y = _animCollapsedBounds.Y;
                height = _animCollapsedBounds.Height;
            }
        }

        return new AnimationFrame { Opacity = 1, X = _designBounds.X, Y = y, Width = _designBounds.Width, Height = height, Finished = finished };
    }

    void IAnimatable.Close() => ((IAnimatable)this).StopAnimation();

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

    /// <summary>
    /// Animation beendet — Panel bleibt an der Zielposition, wird NICHT
    /// geschlossen (anders als bei FloatingForms). Überschreibt den
    /// Default von IAnimatable.OnAnimationFinished, der das
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
    /// Startet die Animation über den Animator — ohne
    /// Layered-Window, da SlideOutPanel ein Child-Control ist, das nur seine
    /// Position ändert. Überschreibt den Default von
    /// IAnimatable.StartAnimation, der WS_EX_LAYERED setzen würde.
    /// </summary>
    void IAnimatable.StartAnimation() {
        if (!IsHandleCreated) {
            Develop.DebugPrint("StartAnimation vor CreateHandle aufgerufen");
            return;
        }
        Animator.Start(this, false);
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            ((IAnimatable)this).StopAnimation();
        }
        base.Dispose(disposing);
    }

    protected override void OnHandleCreated(System.EventArgs e) {
        base.OnHandleCreated(e);
        UpdateLayout();
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
        UpdateLayout();
    }

    /// <summary>
    /// Smoothstep-Easing (3t² - 2t³), gleicher Curve-Typ wie QuickNote.
    /// </summary>
    private static double EaseInOut(double t) => (3.0 - 2.0 * t) * t * t;

    /// <summary>
    /// Geometrie im ausgefahrenen Zustand: Erscheint das Panel von unten,
    /// beginnt es an der entworfenen Oberkante und reicht bis zur Unterkante
    /// des Parents; von oben beginnt es an der Oberkante des Parents und
    /// reicht bis zur entworfenen Unterkante.
    /// </summary>
    private Rectangle ComputeExpandedBounds() {
        if (SlideFrom == SlideFrom.Top) {
            return new Rectangle(_designBounds.X, 0, _designBounds.Width, _designBounds.Bottom);
        }

        var parentBottom = Parent is null ? _designBounds.Bottom : Parent.ClientSize.Height;
        return new Rectangle(_designBounds.X, _designBounds.Y, _designBounds.Width, Math.Max(_designBounds.Height, parentBottom - _designBounds.Y));
    }

    /// <summary>
    /// Geometrie im eingefahrenen Zustand: unveränderte Größe, um
    /// TabSize an der Einfahr-Kante in den Parent verschoben —
    /// nur der Tab bleibt sichtbar, der Rest wird vom Parent abgeschnitten.
    /// </summary>
    private Rectangle ComputeCollapsedBounds() {
        var expanded = ComputeExpandedBounds();
        var y = SlideFrom == SlideFrom.Top ? TabSize - expanded.Height : expanded.Bottom - TabSize;
        return new Rectangle(expanded.X, y, expanded.Width, expanded.Height);
    }

    /// <summary>
    /// Merkt sich einmalig die entworfene Position und Größe, bevor das
    /// Panel erstmals verschoben wird.
    /// </summary>
    private void EnsureDesignBounds() {
        if (_designBoundsCaptured) { return; }
        _designBounds = Bounds;
        _designBoundsCaptured = true;
    }

    private void OnExpandedChanged() => ExpandedChanged?.Invoke(this, System.EventArgs.Empty);

    /// <summary>
    /// Legt den GroupBox-Stil passend zu SlideFrom fest —
    /// die Kopfzeile bleibt im eingefahrenen Zustand als Tab sichtbar.
    /// </summary>
    private void SetSlideStyle() => base.GroupBoxStyle = SlideFrom == SlideFrom.Top ? GroupBoxStyle.NormalBoldBottom : GroupBoxStyle.NormalBold;

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

        EnsureDesignBounds();
        _animCollapsedBounds = ComputeCollapsedBounds();
        _animExpandedBounds = ComputeExpandedBounds();
        _isAnimating = true;

        ((IAnimatable)this).StartAnimation();
    }

    /// <summary>
    /// Positioniert das Panel auf die Zielgeometrie: ausgefahren von der
    /// Einfahr-Kante bis zur entworfenen Gegenkante, eingefahren nur
    /// TabSize Pixel an der Kante sichtbar.
    /// Wird während der Animation nicht aufgerufen — dann steuert der
    /// Animator Position und Größe via SetWindowPos.
    /// </summary>
    private void UpdateLayout() {
        if (DesignMode || IsDisposed || Disposing || Parent is null || _isAnimating || !IsHandleCreated) { return; }
        if (Dock != DockStyle.None) { Dock = DockStyle.None; }
        BringToFront();
        EnsureDesignBounds();

        var newBounds = IsExpanded ? ComputeExpandedBounds() : ComputeCollapsedBounds();
        if (Bounds != newBounds) { Bounds = newBounds; }
    }

    #endregion
}
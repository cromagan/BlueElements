// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes;
using System.Windows.Forms;

namespace BlueControls.Forms;

public partial class QuickInfo : FloatingForm, IAnimatable {

    #region Fields

    private const double IntroDelayMs = 500;
    private const double MaxVisibleMs = 10000;

    private static IntPtr _activeFormHandle;
    private static string _autoClosedTxt = string.Empty;

    private static QuickInfo? _instance;
    private static string _shownTxt = string.Empty;
    private readonly (Rectangle Bounds, Rectangle WorkingArea)[] _screens;

    private int _cachedHeight;
    private int _cachedWidth;
    private volatile bool _externallyClosing;

    private volatile bool _wasAutoClose;

    #endregion

    #region Constructors

    private QuickInfo(string text) : base(Design.Form_QuickInfo) {
        InitializeComponent();
        DismissMode = DismissMode.ManualOnly;

        capText.Text = text;
        capText.FitSize();
        capText.Location = new Point(Skin.PaddingMedium, Skin.PaddingMedium);
        _cachedWidth = Math.Min((int)(Screen.PrimaryScreen.Bounds.Size.Width * 0.5), capText.Right + Skin.PaddingMedium);
        _cachedHeight = Math.Min((int)(Screen.PrimaryScreen.Bounds.Size.Height * 0.5), capText.Bottom + Skin.PaddingMedium);
        Size = new Size(_cachedWidth, _cachedHeight);

        _screens = Screen.AllScreens.Select(s => (s.Bounds, s.WorkingArea)).ToArray();

        Opacity = 0;
        Visible = false;
        CreateHandle();
    }

    #endregion

    #region Methods

    public new static void Close() => Close(false);

    public static void Show(string text) {
        if (text == _shownTxt) {
            // Gleicher Text wie bereits angezeigt — keine neue Instanz erzeugen.
            // Läuft die aktuelle Instanz gerade aus (externer Close, z.B. weil
            // die Maus kurz das Control verlassen hat), wird der Close
            // abgebrochen und die QuickInfo sofort wieder voll eingeblendet.
            // Eine neue Instanz mit 500ms Intro-Delay würde sonst flackern.
            // Bei Auto-Close (Fokusverlust / MaxVisibleMs) wird der Close nicht
            // abgebrochen — die QuickInfo bleibt ausgeblendet.
            if (_instance is { IsDisposed: false } inst && inst._externallyClosing && !inst._wasAutoClose) {
                inst._externallyClosing = false;
            }
            return;
        }

        // Wurde der Text automatisch geschlossen (z.B. 10 s oder Fokus-Verlust),
        // soll er nicht sofort wieder angezeigt werden — sonst flackert er,
        // solange der Nutzer still über demselben Control hovert.
        if (!string.IsNullOrEmpty(_autoClosedTxt) && text == _autoClosedTxt) {
            return;
        }

        // Alte Instanz sofort entfernen — kein Fade-Out bei Textwechsel,
        // sonst überlagern sich zwei QuickInfos.
        if (_instance is { IsDisposed: false } old) {
            ((IAnimatable)old).StopAnimation();
            try { old.Dispose(); } catch { /* ignoriert */ }
            _instance = null;
        }

        _shownTxt = string.Empty;
        _autoClosedTxt = string.Empty;

        if (string.IsNullOrEmpty(text)) { return; }

        _shownTxt = text;
        _activeFormHandle = Animator.GetForegroundWindowHandle();
        if (_activeFormHandle == IntPtr.Zero) {
            _activeFormHandle = ActiveForm?.Handle ?? IntPtr.Zero;
        }

        _instance = new QuickInfo(text);
        _instance.Show();
        ((IAnimatable)_instance).StartAnimation();
    }

    /// <summary>
    /// Berechnet das Frame der Animation. QuickInfo hängt am Mauszeiger und
    /// durchläuft: Intro-Delay (unsichtbar) -&gt; Stable (sofort voll
    /// sichtbar). Auslöser für sofortiges Schließen: externes
    /// <see cref="Close(bool)" />, Vordergrundfenster wechselt oder
    /// <see cref="MaxVisibleMs" /> abgelaufen. Fade-In/Fade-Out ist bewusst
    /// entfernt — die QuickInfo erscheint und verschwindet sofort.
    /// </summary>
    public AnimationFrame Animate(TimeSpan elapsed) {
        var mouse = Animator.GetCursorPos();
        var pos = ComputePosition(mouse);

        // Closing-Phase: sofort unsichtbar und beendet — kein Fade-Out.
        if (_externallyClosing) {
            return new AnimationFrame { Opacity = 0, X = pos.X, Y = pos.Y, Finished = true };
        }

        // Vordergrundfenster gewechselt? -> auto-close einleiten.
        if (_activeFormHandle != IntPtr.Zero && Animator.GetForegroundWindowHandle() != _activeFormHandle) {
            BeginAutoClose();
            return new AnimationFrame { Opacity = 0, X = pos.X, Y = pos.Y, Finished = true };
        }

        var ms = elapsed.TotalMilliseconds;

        // Auto-Close nach MaxVisibleMs.
        if (ms > IntroDelayMs + MaxVisibleMs) {
            BeginAutoClose();
            return new AnimationFrame { Opacity = 0, X = pos.X, Y = pos.Y, Finished = true };
        }

        // Intro-Delay: unsichtbar, aber schon an der Maus.
        if (ms < IntroDelayMs) {
            return new AnimationFrame { Opacity = 0, X = pos.X, Y = pos.Y };
        }

        // Stable — sofort voll sichtbar, kein Fade-In.
        return new AnimationFrame { Opacity = 1, X = pos.X, Y = pos.Y };
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing) {
        if (disposing) { components?.Dispose(); }
        base.Dispose(disposing);
    }

    protected override void OnFormClosed(FormClosedEventArgs e) {
        // Singleton-Zustand aufräumen, nachdem die Form geschlossen ist.
        if (_wasAutoClose) {
            _autoClosedTxt = _shownTxt;
        } else {
            _shownTxt = string.Empty;
            _autoClosedTxt = string.Empty;
        }
        _activeFormHandle = IntPtr.Zero;
        _instance = null;

        base.OnFormClosed(e);
    }

    private static void Close(bool autoClose) {
        if (_instance is not { IsDisposed: false } inst) { return; }

        inst._wasAutoClose = autoClose;
        inst._externallyClosing = true;
    }

    private void BeginAutoClose() {
        if (_externallyClosing) { return; }
        _wasAutoClose = true;
        _externallyClosing = true;
    }

    private Point ComputePosition(Point mouse) {
        // Screen finden, auf dem die Maus liegt.
        var screenIdx = -1;
        for (var i = 0; i < _screens.Length; i++) {
            if (_screens[i].Bounds.Contains(mouse)) {
                screenIdx = i;
                break;
            }
        }
        if (screenIdx < 0) { screenIdx = 0; }

        var (bounds, working) = _screens[screenIdx];

        var ypos = mouse.Y + 15;
        var xpos = mouse.X + 15;
        if (xpos + _cachedWidth > bounds.Right) { xpos = mouse.X - 5 - _cachedWidth; }
        if (ypos + _cachedHeight > bounds.Bottom) { ypos = mouse.Y - 5 - _cachedHeight; }

        if (xpos < working.Left) { xpos = working.Left; }
        if (ypos < working.Top) { ypos = working.Top; }
        if (xpos + _cachedWidth > working.Right) { xpos = working.Right - _cachedWidth; }
        if (ypos + _cachedHeight > working.Bottom) { ypos = working.Bottom - _cachedHeight; }

        return new Point(xpos, ypos);
    }

    #endregion
}
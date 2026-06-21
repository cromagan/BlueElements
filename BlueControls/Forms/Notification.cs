// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes;
using System.Windows.Forms;

namespace BlueControls.Forms;

public partial class Notification : FloatingForm, IAnimatable {

    #region Fields

    private const double SpeedIn = 250d;

    // Wegen Recheoperation
    private const double SpeedOut = 250d;

    // Handles aller beim Erzeugen sichtbaren Notifications, die unter dieser
    // liegen (Stacking-Kandidaten). Auf dem UI-Thread gesichert (im
    // Konstruktor) — Visible==true garantiert, dass das Handle erstellt ist.
    // Aus dem Animations-Thread heraus darf Control.Handle NICHT gelesen
    // werden (threadübergreifender Vorgang beim Handle-Erstellen). Der letzte
    // Eintrag ist der direkte Anker (NoteBelow); fällt dieser später weg,
    // rutscht diese Notification dynamisch auf den nächsten lebenden
    // Kandidaten nach unten, statt sofort auszufaden.
    private readonly IntPtr[] _belowCandidates = [];

    private readonly int _screenHeight;

    private readonly int _screenTime = -999;

    private readonly int _screenWidth;

    private Action? _buttonAction;

    private int _cachedHeight;
    private int _cachedWidth;
    private volatile bool _hiddenNow;
    private int _lowestY;
    private DateTime _outime = DateTime.MinValue;

    #endregion

    #region Constructors

    private Notification() : base(Design.Form_Notification) {
        InitializeComponent();
        DismissMode = DismissMode.ManualOnly;
    }

    private Notification(string text) : this() {
        capText.Text = text;
        capText.FitSize();
        capText.Location = new Point(Skin.Padding, Skin.Padding);
        var lines = (capText.Text ?? string.Empty).Split(["\r\n", "\n", "\r"], StringSplitOptions.None);
        var needsWider = false;
        if (lines.Length >= 2 && lines[0].Length >= lines[1].Length - 5) {
            needsWider = true;
        }
        var btnNeededWidth = btnClose.Width + 8;
        var primaryScreen = Screen.PrimaryScreen;
        _screenWidth = primaryScreen?.Bounds.Size.Width ?? 1920;
        _screenHeight = primaryScreen?.Bounds.Size.Height ?? 1080;
        var wi = Math.Min((int)(_screenWidth * 0.5), capText.Right + Skin.Padding);
        if (needsWider) {
            wi = Math.Max(wi, btnNeededWidth + Skin.Padding * 2);
        }
        var he = Math.Min((int)(_screenHeight * 0.5), capText.Bottom + Skin.Padding);
        Size = new Size(wi, he);

        btnClose.Location = new Point(Width - btnClose.Width - 4, 4);

        _screenTime = Math.Clamp(text.Length * 100, 3200, 20000);

        // Alle aktuell sichtbaren Notifications liegen unter dieser neuen
        // (AllBoxes ist insertion-geordnet: die letzte ist der direkte
        // Anker). Wir sichern ALLE Handles als Kandidaten, damit beim
        // Entfernen einer mittleren Notification die darüber liegenden
        // dynamisch auf den nächsten lebenden Kandidaten nachrutschen,
        // statt sofort auszufaden.
        var candidates = new List<IntPtr>();
        foreach (var thisParent in GetActiveForms()) {
            if (thisParent is Notification nf) {
                if (nf != this && nf is { Visible: true, IsDisposed: false }) {
                    NoteBelow = nf;
                    // Handle MUSS hier auf dem UI-Thread gesichert werden —
                    // Visible==true garantiert, dass das Handle erstellt ist.
                    candidates.Add(nf.Handle);
                }
            }
        }
        _belowCandidates = candidates.ToArray();

        CacheScreenValues();

        // Fade-In passiert durch die Engine; bis dahin unsichtbar.
        Opacity = 0;
    }

    private Notification(string text, string buttonName, Action buttonAction) : this(text) {
        _buttonAction = buttonAction;
        btnAction.Text = buttonName;
        btnAction.Visible = true;
        btnAction.FitSize();

        var wi = Math.Max(Width, btnAction.Width + Skin.Padding * 2);
        var he = capText.Bottom + Skin.Padding + btnAction.Height + Skin.Padding;
        Size = new Size(wi, he);

        btnAction.Location = new Point(Skin.Padding, capText.Bottom + Skin.Padding);
        btnClose.Location = new Point(Width - btnClose.Width - 4, 4);

        // Höhe hat sich geändert -> Werte neu cachen.
        CacheScreenValues();
    }

    #endregion

    #region Properties

    /// <summary>
    /// Notification, die beim Erzeugen direkt unter dieser lag (direkter
    /// Anker). Wird dieser später entfernt, rutscht diese Notification
    /// automatisch auf den nächsten lebenden Stacking-Kandidaten nach.
    /// Aus dem Animations-Thread heraus nur über die gesicherten Handles
    /// zugreifen (siehe <see cref="_belowCandidates" />).
    /// </summary>
    public Notification? NoteBelow { get; }

    #endregion

    #region Methods

    public static void Show(string text) {
        if (string.IsNullOrEmpty(text)) { return; }
        var x = new Notification(text);
        ShowAndAnimate(x);
    }

    public static void Show(string text, ImageCode? img) {
        if (img is not null) {
            text = "<imagecode=" + Enum.GetName(img.GetType(), img) + "|32> <zbx_store><top>" + text;
        }
        Show(text);
    }

    public static void Show(string text, string buttonName, Action buttonAction) {
        if (string.IsNullOrEmpty(text)) { return; }
        var x = new Notification(text, buttonName, buttonAction);
        ShowAndAnimate(x);
    }

    public static void Show(string text, ImageCode? img, string buttonName, Action buttonAction) {
        if (img is not null) {
            text = "<imagecode=" + Enum.GetName(img.GetType(), img) + "|32> <zbx_store><top>" + text;
        }
        Show(text, buttonName, buttonAction);
    }

    /// <summary>
    /// Berechnet das Frame der Animation direkt aus der verstrichenen Zeit.
    /// Phase 1: Fade-In (Opacity 0 -&gt; 1).
    /// Phase 2: Stable, Opacity = 1, Position unten (oder über NoteBelow).
    /// Phase 3: Fade-Out (Opacity 1 -&gt; 0), sofern kein lebender Stacking-
    /// Kandidat mehr unter uns liegt — sonst bleiben wir stabil über dem
    /// nächsten lebenden Anker.
    /// </summary>
    public AnimationFrame Animate(TimeSpan elapsed) {
        var ms = elapsed.TotalMilliseconds;

        var newLeft = _screenWidth - _cachedWidth - 1;
        var newTop = _lowestY;

        // Bester lebender Anker: der topmost (niedrigste Y) unter den noch
        // lebenden Kandidaten. Auf ihm positionieren wir uns direkt oben.
        // Fällt der direkte Anker weg (mittlere Notification entfernt),
        // rutschen wir automatisch auf den nächsten lebenden Kandidaten nach.
        var hasBelow = false;
        var bestY = int.MaxValue;
        foreach (var belowHwnd in _belowCandidates) {
            if (Animator.IsHwndAlive(belowHwnd) && Animator.IsHwndVisible(belowHwnd)) {
                var y = Animator.GetWindowY(belowHwnd);
                if (y < bestY) { bestY = y; }
            }
        }
        if (bestY != int.MaxValue) {
            newTop = Math.Min(bestY - _cachedHeight - 1, _lowestY);
            hasBelow = true;
        }

        if (_hiddenNow) {
            return new AnimationFrame { Opacity = 0, X = newLeft, Y = newTop, Finished = true };
        }

        double opacity;
        var finished = false;

        if (ms < SpeedIn) {
            // Fade-In
            opacity = ms / SpeedIn;
            newTop = Math.Min(newTop, _lowestY);
        } else if (ms > _screenTime - SpeedIn) {
            // Fade-Out-Phase
            if (!hasBelow) {
                if (_outime == DateTime.MinValue) { _outime = DateTime.UtcNow; }
                var mSo = DateTime.UtcNow.Subtract(_outime).TotalMilliseconds;
                opacity = 1 - (mSo / SpeedOut);
                if (opacity <= 0) {
                    opacity = 0;
                    finished = true;
                }
            } else {
                // Ein Anker unter uns hält uns noch oben — stabil bleiben.
                // _outime zurücksetzen, damit nicht ein veralteter Fade-Timer
                // beim nächsten Wegfallen des Ankers sofort auslöst.
                opacity = 1;
                _outime = DateTime.MinValue;
            }
        } else {
            // Stable
            opacity = 1;
            newTop = Math.Min(newTop, _lowestY);
        }

        // Hard-Stop nach 2 Minuten.
        if (elapsed.TotalMinutes > 2) { finished = true; }

        return new AnimationFrame { Opacity = opacity, X = newLeft, Y = newTop, Finished = finished };
    }

    private static void ShowAndAnimate(Notification x) {
        // Erst sichtbar machen (an der Endposition), dann Animation starten.
        x.Left = x._screenWidth - x._cachedWidth - 1;
        x.Top = x._lowestY;
        x.Show();
        ((IAnimatable)x).StartAnimation();
    }

    private void btnAction_Click(object sender, System.EventArgs e) {
        _buttonAction?.Invoke();
        _hiddenNow = true;
    }

    private void btnClose_Click(object sender, System.EventArgs e) {
        _hiddenNow = true;
    }

    private void CacheScreenValues() {
        _cachedWidth = Width;
        _cachedHeight = Height;
        var primaryScreen = Screen.PrimaryScreen;
        _lowestY = (primaryScreen?.WorkingArea.Bottom ?? 600) - _cachedHeight - 1;
    }

    #endregion
}
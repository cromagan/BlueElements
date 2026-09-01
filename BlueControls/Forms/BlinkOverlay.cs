// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Windows.Forms;

namespace BlueControls.Forms;

/// <summary>
/// Ein deckendes Overlay über einem Bildschirm-Rechteck, das dreimal voll aufflackert und erlischt.
/// </summary>
public sealed partial class BlinkOverlay : System.Windows.Forms.Form, IAnimatable {

    #region Fields

    private const int BlinkCount = 3;
    private const int BlinkPeriodMs = 300;

    private readonly Rectangle _bounds;

    #endregion

    #region Constructors

    private BlinkOverlay(Rectangle bounds, Color color) {
        _bounds = bounds;
        InitializeComponent();
        BackColor = color;

        // Erzwingt das Layered-Window, damit der Animator die Transparenz steuern kann.
        Opacity = 0;
    }

    #endregion

    #region Properties

    protected override CreateParams CreateParams {
        get {
            var p = base.CreateParams;
            p.ExStyle |= (int)(ExStyle.EX_NOACTIVATE | ExStyle.EX_TOOLWINDOW | ExStyle.EX_TOPMOST | ExStyle.EX_TRANSPARENT);
            p.Parent = IntPtr.Zero;
            return p;
        }
    }

    protected override bool ShowWithoutActivation => true;

    #endregion

    #region Methods

    /// <summary>
    /// Legt über das gegebene Bildschirm-Rechteck ein Overlay, das dreimal voll aufblinkt.
    /// Muss auf dem UI-Thread aufgerufen werden.
    /// </summary>
    public static void Blink(Rectangle cellScreenRectangle, Color color) {
        var b = new BlinkOverlay(cellScreenRectangle, color);
        b.ShowAt();
        ((IAnimatable)b).StartAnimation();
    }

    /// <summary>
    /// Berechnet das Frame: dreimal hart zwischen voll und transparent wechselnd, danach aus.
    /// Ohne Width/Height bleibt die Overlay-Größe konstant.
    /// </summary>
    public AnimationFrame Animate(TimeSpan elapsed) {
        var ms = elapsed.TotalMilliseconds;

        if (ms is >= (BlinkCount * BlinkPeriodMs) or > 2000) {
            return new AnimationFrame { Opacity = 0, X = _bounds.Left, Y = _bounds.Top, Finished = true };
        }

        // Hartes Blinken: erste Periodenhälfte voll sichtbar, zweite unsichtbar.
        var on = ms % BlinkPeriodMs < BlinkPeriodMs / 2d;
        return new AnimationFrame { Opacity = on ? 0.5f : 0.0, X = _bounds.Left, Y = _bounds.Top };
    }

    private void ShowAt() {
        Show();
        // Größe/Position erst nach Show() setzen: Windows klemmt Fenstergrößen bei der
        // Erstellung auf ein Systemminimum (~16 Pixel) und verschluckt so kleine Zellen.
        SetBounds(_bounds.Left, _bounds.Top, _bounds.Width, _bounds.Height);
    }

    #endregion
}
// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes;
using System.Windows.Forms;

namespace BlueControls.Controls;

public class ScrollPanel : ZoomPad {

    #region Fields

    private int _lastOffsetX = int.MinValue;
    private int _lastOffsetY = int.MinValue;

    #endregion

    #region Constructors

    public ScrollPanel() {
        AutoCenter = false;
    }

    #endregion

    #region Properties

    [DefaultValue(ChildLayout.None)]
    public ChildLayout ChildLayout {
        get;
        set {
            if (field == value) { return; }
            field = value;
            _lastOffsetY = int.MinValue;
            Invalidate();
        }
    } = ChildLayout.None;

    public override bool ControlMustPressedForZoomWithWheel => true;

    protected override bool ShowSliderX =>
        ChildLayout.HasFlag(ChildLayout.Slider) &&
        ChildLayout.HasFlag(ChildLayout.FullWidth) &&
        !ChildLayout.HasFlag(ChildLayout.StackVertical);

    protected override int SmallChangeY => 20;

    #endregion

    #region Methods

    protected override RectangleF CalculateCanvasMaxBounds() {
        if (ChildLayout == ChildLayout.None) { return RectangleF.Empty; }

        var minY = int.MaxValue;
        var maxY = int.MinValue;
        var minX = int.MaxValue;
        var maxX = int.MinValue;

        foreach (Control c in Controls) {
            if (c is Slider) { continue; }
            var baseTop = c.Tag is int t ? t : c.Top;
            minY = Math.Min(minY, baseTop);
            maxY = Math.Max(maxY, baseTop + c.Height);
            minX = Math.Min(minX, c.Left);
            maxX = Math.Max(maxX, c.Right);
        }

        if (minY >= maxY || minX >= maxX) { return RectangleF.Empty; }
        return new RectangleF(minX, minY, maxX - minX, maxY - minY);
    }

    protected override void DrawControl(Graphics gr, States state) {
        base.DrawControl(gr, state);

        if (ChildLayout == ChildLayout.None) { return; }
        if (Width < 10 || Height < 10) { return; }

        if (OffsetX == _lastOffsetX && OffsetY == _lastOffsetY) { return; }
        _lastOffsetX = OffsetX;
        _lastOffsetY = OffsetY;

        var children = new List<Control>();
        foreach (Control c in Controls) {
            if (c is Slider) { continue; }
            children.Add(c);
        }

        if (children.Count == 0) { return; }

        var isStackVertical = ChildLayout.HasFlag(ChildLayout.StackVertical);
        var isFullWidth = ChildLayout.HasFlag(ChildLayout.FullWidth);

        var currentTop = Skin.Padding;
        var effectiveWidth = Width - Skin.Padding * 2; // Annahme: Padding gilt f�r beide Seiten

        foreach (var c in children) {
            // 1. Layout-Berechnung
            if (isFullWidth) {
                c.Width = effectiveWidth;
            }

            if (isStackVertical) {
                c.Left = Skin.Padding;
                c.Top = currentTop;
                currentTop += c.Height + Skin.Padding;
            } else if (isFullWidth) {
                c.Left = Skin.Padding;
            }

            // 2. Offset-Anwendung
            c.Left += OffsetX;
            c.Top += OffsetY;
        }

        Invalidate_MaxBounds();
    }

    protected override void OnControlAdded(ControlEventArgs e) {
        base.OnControlAdded(e);
        _lastOffsetY = int.MinValue;
        Invalidate_MaxBounds();
    }

    protected override void OnControlRemoved(ControlEventArgs e) {
        base.OnControlRemoved(e);
        _lastOffsetY = int.MinValue;
        Invalidate_MaxBounds();
    }

    protected override void OnSizeChanged(System.EventArgs e) {
        base.OnSizeChanged(e);
        _lastOffsetY = int.MinValue;
    }

    #endregion
}
// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes;

namespace BlueControls.Extended_Text.MarkRendering;

public sealed class Zone {

    #region Constructors

    public Zone(MarkRenderer renderer, int startPos, int endPos, Color? fontColor) {
        _renderer = renderer;
        StartPos = startPos;
        EndPos = endPos;
        FontColor = fontColor;
    }

    #endregion

    #region Properties

    public bool BeforeText => !FontColor.HasValue;
    public int EndPos { get; internal set; }

    public Color? FontColor { get; }

    public object Priority => _renderer.Priority;
    public int StartPos { get; internal set; }
    public string Type => _renderer.KeyName;
    private MarkRenderer _renderer { get; }

    #endregion

    #region Methods

    public void Render(ExtText extText, Graphics gr, float zoom, int offsetX, int offsetY) {
        if (extText.Count == 0) { return; }
        var markStart = Math.Clamp(StartPos, 0, extText.Count - 1);
        var markEnd = Math.Clamp(EndPos, 0, extText.Count - 1);
        if (markStart > markEnd) { return; }

        var segStart = markStart;
        var segY = 0f;
        var segYInitialized = false;

        for (var i = markStart; i <= markEnd; i++) {
            var ch = extText[i];
            if (ch.SizeCanvas.Height > 0) {
                if (!segYInitialized) {
                    segY = ch.PosCanvas.Y;
                    segYInitialized = true;
                } else if (Math.Abs(ch.PosCanvas.Y - segY) > 0.5f) {
                    RenderLineSegment(extText, gr, zoom, segStart, i - 1, offsetX, offsetY);
                    segStart = i;
                    segY = ch.PosCanvas.Y;
                }
            }
        }

        RenderLineSegment(extText, gr, zoom, segStart, markEnd, offsetX, offsetY);

        if (FontColor is Color c) {
            DrawOverriddenText(extText, gr, zoom, offsetX, offsetY, c);
        }
    }

    private void DrawOverriddenText(ExtText extText, Graphics gr, float zoom, int offsetX, int offsetY, Color overrideColor) {
        if (extText.Count == 0) { return; }
        var markStart = Math.Clamp(StartPos, 0, extText.Count - 1);
        var markEnd = Math.Clamp(EndPos, 0, extText.Count - 1);

        for (var i = markStart; i <= markEnd; i++) {
            var t = extText[i];
            var controlPos = t.PosCanvas.CanvasToControl(zoom, offsetX, offsetY);
            var controlSize = t.SizeCanvas.CanvasToControl(zoom);
            if (ExtChar.IsVisible(extText.AreaControl, controlPos, controlSize)) {
                var f = t.Font;
                if (f is not null) {
                    var overrideFont = BlueFont.Get(f.FontName, f.Size, f.Bold, f.Italic, f.Underline, f.StrikeOut, overrideColor, f.ColorOutline, f.ColorBack);
                    t.DrawWithFont(gr, controlPos, controlSize, zoom, overrideFont);
                }
            }
        }
    }

    private void RenderLineSegment(ExtText _internal, Graphics gr, float zoom, int segStart, int segEnd, int offsetX, int offsetY) {
        int? firstVisible = null;
        var lastVisible = -1;
        var maxHeight = 0f;
        var lineY = 0f;

        for (var i = segStart; i <= segEnd; i++) {
            var ch = _internal[i];
            if (ch.SizeCanvas.Width > 0) {
                if (firstVisible is null) {
                    firstVisible = i;
                    lineY = ch.PosCanvas.Y;
                }
                lastVisible = i;
                maxHeight = Math.Max(maxHeight, ch.SizeCanvas.Height);
            }
        }

        if (firstVisible is null || lastVisible < 0 || maxHeight <= 0) { return; }

        var startX = _internal[firstVisible.Value].PosCanvas.X.CanvasToControl(zoom, offsetX);
        var startY = lineY.CanvasToControl(zoom, offsetY);
        var endX = (_internal[lastVisible].PosCanvas.X + _internal[lastVisible].SizeCanvas.Width).CanvasToControl(zoom, offsetX);
        var endY = (lineY + maxHeight).CanvasToControl(zoom, offsetY);
        var height = maxHeight.CanvasToControl(zoom);

        _renderer.Render(gr, zoom, startX, startY, endX, endY, height);
    }

    #endregion
}
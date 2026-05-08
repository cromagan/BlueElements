// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes;
using System.Threading;
using static BlueBasics.ClassesStatic.Converter;

namespace BlueControls.Extended_Text;

public abstract class ExtChar : IDisposableExtended {

    #region Fields

    public PointF PosCanvas = PointF.Empty;
    internal ExtText? _parent;
    private BlueFont? _font;
    private volatile int _isDisposedFlag;
    private SizeF _size;

    #endregion

    #region Constructors

    protected ExtChar() { }

    protected ExtChar(ExtText parent, List<string> overrideTags) {
        OverrideTags = [.. overrideTags];
        _parent = parent;
        _parent.StyleChanged += _parent_StyleChanged;
    }

    protected ExtChar(ExtText parent, int styleFromPos) {
        if (parent.Count == 0) {
            OverrideTags = [];
        } else {
            styleFromPos = Math.Clamp(styleFromPos, 0, parent.Count - 1);
            OverrideTags = [.. parent[styleFromPos].OverrideTags];
        }

        _parent = parent;
    }

    #endregion

    #region Properties

    public BlueFont BaseFont => _parent?.BaseFont ?? BlueFont.DefaultFont;

    public BlueFont? Font {
        get {
            _font ??= ResolveFont(BaseFont);
            return _font;
        }
        set {
            if (_font != value) {
                _font = value;
                _size = SizeF.Empty;
            }
        }
    }

    public bool IsDisposed => _isDisposedFlag == 1;
    public MarkState Marking { get; set; }
    public List<string> OverrideTags { get; private set; } = [];
    public virtual bool ResetsYPosition => false;
    public virtual Alignment RowAlignment => Alignment.Bottom;

    public SizeF SizeCanvas {
        get {
            if (_size.IsEmpty) { _size = CalculateSizeCanvas(); }
            return _size;
        }
    }

    public virtual bool StoresXPosition => false;
    internal virtual bool HandlesOwnLayout => false;
    internal virtual string? StructuralTag => null;

    #endregion

    #region Methods

    public static bool IsVisible(Rectangle areaControl, Point controlPos, Size controlSize) {
        if (areaControl.Width < 1 || areaControl.Height < 1) { return true; }
        return controlPos.X <= areaControl.Right
            && controlPos.X + controlSize.Width >= areaControl.Left
            && controlPos.Y <= areaControl.Bottom
            && controlPos.Y + controlSize.Height >= areaControl.Top;
    }

    public void Dispose() {
        Dispose(true);
        InvalidateFont();
        GC.SuppressFinalize(this);
    }

    public abstract void Draw(Graphics gr, Point controlPos, Size controlSize, float zoom);

    public abstract string HtmlText();

    public abstract bool IsLineBreak();

    public abstract bool IsPossibleLineBreak();

    public abstract bool IsSpace();

    public abstract bool IsWordSeparator();

    public abstract string PlainText();

    internal static BlueFont ResolveFont(BlueFont baseFont, List<string> tags) {
        if (tags.Count == 0) { return baseFont; }

        var bold = baseFont.Bold;
        var italic = baseFont.Italic;
        var underline = baseFont.Underline;
        var strikeOut = baseFont.StrikeOut;
        var size = baseFont.Size;
        var fontName = baseFont.FontName;
        var colorMain = baseFont.ColorMain;
        var colorOutline = baseFont.ColorOutline;
        var colorBack = baseFont.ColorBack;

        foreach (var tag in tags) {
            switch (tag) {
                case "b":
                    bold = true;
                    break;

                case "/b":
                    bold = false;
                    break;

                case "i":
                    italic = true;
                    break;

                case "/i":
                    italic = false;
                    break;

                case "u":
                    underline = true;
                    break;

                case "/u":
                    underline = false;
                    break;

                case "strike":
                    strikeOut = true;
                    break;

                case "/strike":
                    strikeOut = false;
                    break;

                default:
                    if (tag.StartsWith("fontsize=", StringComparison.OrdinalIgnoreCase)) {
                        FloatTryParse(tag[9..], out size);
                    } else if (tag.StartsWith("fontname=", StringComparison.OrdinalIgnoreCase)) {
                        fontName = tag[9..];
                    } else if (tag.StartsWith("fontcolor=", StringComparison.OrdinalIgnoreCase)) {
                        colorMain = ColorParse(tag[10..]);
                    } else if (tag.StartsWith("outlinecolor=", StringComparison.OrdinalIgnoreCase) ||
                               tag.StartsWith("coloroutline=", StringComparison.OrdinalIgnoreCase) ||
                               tag.StartsWith("fontoutline=", StringComparison.OrdinalIgnoreCase)) {
                        colorOutline = ColorParse(tag[(tag.IndexOf('=') + 1)..]);
                    } else if (tag.StartsWith("backcolor=", StringComparison.OrdinalIgnoreCase)) {
                        colorBack = ColorParse(tag[11..]);
                    }
                    break;
            }
        }

        return BlueFont.Get(fontName, size, bold, italic, underline, strikeOut, colorMain, colorOutline, colorBack);
    }

    internal virtual (float ContinueX, float ContinueY, float MaxRight, float MaxBottom) ComputeCharLayout(float startX, float startY, float maxWidth, float lineStartX, float lineSpacing) {
        PosCanvas = new PointF(startX, startY);
        var s = CalculateSizeCanvas();
        _size = s;
        return (startX + s.Width, startY, startX + s.Width, startY + s.Height);
    }

    internal virtual void DrawWithFont(Graphics gr, Point controlPos, Size controlSize, float zoom, BlueFont font) => Draw(gr, controlPos, controlSize, zoom);

    internal virtual void InitFromTag(ExtText parent, List<string> tags, string? attribut) {
        OverrideTags = [.. tags];
        _parent = parent;
        _parent.StyleChanged += _parent_StyleChanged;
    }

    internal void InvalidateFont() {
        _font = null;
        _size = SizeF.Empty;
    }

    protected abstract SizeF CalculateSizeCanvas();

    protected virtual void Dispose(bool disposing) {
        if (Interlocked.CompareExchange(ref _isDisposedFlag, 1, 0) != 0) { return; }

        if (disposing) {
            _parent?.StyleChanged -= _parent_StyleChanged;
            _parent = null;
        }
    }

    protected void SetSize(SizeF size) => _size = size;

    private void _parent_StyleChanged(object? sender, System.EventArgs e) => InvalidateFont();

    private BlueFont ResolveFont(BlueFont baseFont) => ResolveFont(baseFont, OverrideTags);

    #endregion
}
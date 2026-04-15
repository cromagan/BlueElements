// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
// https://github.com/cromagan/BlueElements
//
// License: GNU Affero General Public License v3.0
// https://github.com/cromagan/BlueElements/blob/master/LICENSE
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.Classes;
using BlueControls.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using static BlueBasics.ClassesStatic.Converter;

namespace BlueControls.Extended_Text;

public abstract class ExtChar : IDisposableExtended {

    #region Fields

    public PointF PosCanvas = PointF.Empty;
    private BlueFont? _font;
    internal ExtText? _parent;
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
            styleFromPos = Math.Max(0, Math.Min(styleFromPos, parent.Count - 1));
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

    public virtual Alignment RowAlignment => Alignment.Bottom;
    public bool IsDisposed { get; private set; }
    public MarkState Marking { get; set; }
    public List<string> OverrideTags { get; private set; } = [];
    public virtual bool ResetsYPosition => false;
    public virtual bool StoresXPosition => false;
    internal virtual string? StructuralTag => null;
    internal virtual void InitFromTag(ExtText parent, List<string> tags, string? attribut) {
        OverrideTags = [.. tags];
        _parent = parent;
        _parent.StyleChanged += _parent_StyleChanged;
    }

    public SizeF SizeCanvas {
        get {
            if (_size.IsEmpty) { _size = CalculateSizeCanvas(); }
            return _size;
        }
    }

    #endregion

    #region Methods

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

    public bool IsVisible(Rectangle areaControl, Point controlPos, Size controlSize) {
        if (areaControl.Width < 1 || areaControl.Height < 1) { return true; }
        return controlPos.X <= areaControl.Right
            && controlPos.X + controlSize.Width >= areaControl.Left
            && controlPos.Y <= areaControl.Bottom
            && controlPos.Y + controlSize.Height >= areaControl.Top;
    }

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

    internal virtual void DrawWithFont(Graphics gr, Point controlPos, Size controlSize, float zoom, BlueFont font) {
        Draw(gr, controlPos, controlSize, zoom);
    }

    internal virtual IEnumerable<ExtChar> GetChars() { yield return this; }

    internal void InvalidateFont() {
        _font = null;
        _size = SizeF.Empty;
    }

    protected abstract SizeF CalculateSizeCanvas();

    protected virtual void Dispose(bool disposing) {
        if (!IsDisposed) {
            if (disposing) {
                // TODO: Verwalteten Zustand (verwaltete Objekte) bereinigen

                _parent?.StyleChanged -= _parent_StyleChanged;
                _parent = null;
            }
            // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer überschreiben
            // TODO: Große Felder auf NULL setzen
            IsDisposed = true;
        }
    }

    private void _parent_StyleChanged(object sender, System.EventArgs e) => InvalidateFont();

    private BlueFont ResolveFont(BlueFont baseFont) => ResolveFont(baseFont, OverrideTags);

    #endregion
}
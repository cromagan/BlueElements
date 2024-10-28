// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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

#nullable enable

using System;
using System.Drawing;
using BlueBasics.Interfaces;
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueDatabase;

namespace BlueControls.Extended_Text;

public abstract class ExtChar : IStyleableOne, IDisposableExtended {

    #region Fields

    public PointF Pos = PointF.Empty;
    private BlueFont? _font;
    private ExtText? _parent;
    private SizeF _size;
    private PadStyles _style = PadStyles.Undefiniert;

    #endregion

    #region Constructors

    public ExtChar(ExtText parent, PadStyles stil, BlueFont font) : base() {
        _size = SizeF.Empty;
        _style = stil;
        _parent = parent;
        _font = font;
        _parent.StyleChanged += _parent_StyleChanged;
    }

    public ExtChar(ExtText parent, int styleFromPos) : base() {
        styleFromPos = Math.Max(styleFromPos, 0);
        styleFromPos = Math.Min(styleFromPos, parent.Count - 1);

        if (styleFromPos < 0) {
            Stil = PadStyles.Standard;
            _font = Skin.GetBlueFont(parent.SheetStyle, Stil);
        } else {
            _font = parent[styleFromPos].Font;
            Stil = parent[styleFromPos].Stil;
        }

        _size = SizeF.Empty;
        _parent = parent;
        _parent.StyleChanged += _parent_StyleChanged;
    }

    #endregion

    #region Properties

    public BlueFont? Font {
        get => _font;
        set {
            if (_font != value) {
                _font = value;
                _size = SizeF.Empty;
            }
        }
    }

    public bool IsDisposed { get; private set; }
    public MarkState Marking { get; set; }

    public string SheetStyle {
        get {
            if (_parent is IStyleable ist) { return ist.SheetStyle; }
            return string.Empty;
        }
    }

    public SizeF Size {
        get {
            if (_size.IsEmpty) { _size = CalculateSize(); }
            return _size;
        }
    }

    public PadStyles Stil {
        get => _style;
        set {
            if (_style == value) { return; }
            _style = value;
            this.InvalidateFont();
        }
    }

    #endregion

    #region Methods

    public void Dispose() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public abstract void Draw(Graphics gr, Point posModificator, float zoom);

    public PadStyles GetStyle() {
        if (Font == null || Skin.StyleDb is not Database db) { return PadStyles.Standard; }

        var f1 = new FilterItem(db.Column["Font"], BlueDatabase.Enums.FilterType.Istgleich_GroßKleinEgal, Font.KeyName);
        var f2 = new FilterItem(db.Column["Style"], BlueDatabase.Enums.FilterType.Istgleich_GroßKleinEgal, SheetStyle);

        var r = db.Row[f1, f2];

        if (r == null) { return PadStyles.Standard; }

        return (PadStyles)r.CellGetInteger("Style");
    }

    public abstract string HtmlText();

    public abstract bool IsLineBreak();

    public abstract bool IsPossibleLineBreak();

    public abstract bool IsSpace();

    /// <summary>
    ///
    /// </summary>
    /// <param name="zoom"></param>
    /// <param name="drawingPos">Muss bereits Skaliert sein</param>
    /// <returns></returns>
    public bool IsVisible(float zoom, Point drawingPos, Rectangle drawingArea) => drawingArea is { Width: < 1, Height: < 1 } ||
        ((drawingArea.Width <= 0 || (Pos.X * zoom) + drawingPos.X <= drawingArea.Right)
         && (drawingArea.Height <= 0 || (Pos.Y * zoom) + drawingPos.Y <= drawingArea.Bottom)
         && ((Pos.X + Size.Width) * zoom) + drawingPos.X >= drawingArea.Left
         && ((Pos.Y + Size.Height) * zoom) + drawingPos.Y >= drawingArea.Top);

    public abstract bool IsWordSeperator();

    public abstract string PlainText();

    protected abstract SizeF CalculateSize();

    protected virtual void Dispose(bool disposing) {
        if (!IsDisposed) {
            if (disposing) {
                // TODO: Verwalteten Zustand (verwaltete Objekte) bereinigen

                if (_parent != null) {
                    _parent.StyleChanged -= _parent_StyleChanged;
                    _parent = null;
                }
            }

            // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer überschreiben
            // TODO: Große Felder auf NULL setzen
            IsDisposed = true;
        }
    }

    private void _parent_StyleChanged(object sender, System.EventArgs e) {
        this.InvalidateFont();
    }

    #endregion

    // // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
    // ~ExtChar()
    // {
    //     // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
    //     Dispose(disposing: false);
    // }
}
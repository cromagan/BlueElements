// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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

using BlueBasics;
using BlueBasics.Interfaces;
using BlueControls.Enums;
using BlueControls.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using static BlueBasics.Converter;
using static BlueBasics.Extensions;

namespace BlueControls.Extended_Text;

public abstract class ExtChar : ParseableItem, IStyleableOne, IDisposableExtended {

    #region Fields

    public PointF Pos = PointF.Empty;
    private BlueFont? _font;
    private ExtText? _parent;
    private SizeF _size;
    private PadStyles _style = PadStyles.Undefiniert;

    #endregion

    #region Constructors

    protected ExtChar(ExtText parent, PadStyles style, BlueFont font) : base() {
        _size = SizeF.Empty;
        _style = style;
        _parent = parent;
        _font = font;
        _parent.StyleChanged += _parent_StyleChanged;
    }

    protected ExtChar(ExtText parent, int styleFromPos) : base() {
        styleFromPos = Math.Max(styleFromPos, 0);
        styleFromPos = Math.Min(styleFromPos, parent.Count - 1);

        if (styleFromPos < 0) {
            Style = PadStyles.Standard;
            _font = Skin.GetBlueFont(parent.SheetStyle, Style);
        } else {
            Style = parent[styleFromPos].Style;
            _font = parent[styleFromPos].Font;
        }

        _size = SizeF.Empty;
        _parent = parent;
        _parent.StyleChanged += _parent_StyleChanged;
    }

    #endregion

    #region Properties

    public BlueFont? Font {
        get {
            _font ??= Skin.GetBlueFont(SheetStyle, Style);
            return _font;
        }
        set {
            if (_font != value) {
                _font = value;
                _size = SizeF.Empty;
            }
        }
    }

    public bool IsDisposed { get; private set; }

    public MarkState Marking { get; set; }

    public string SheetStyle => _parent is IStyleable ist ? ist.SheetStyle : string.Empty;

    public SizeF Size {
        get {
            if (_size.IsEmpty) { _size = CalculateSize(); }
            return _size;
        }
    }

    public PadStyles Style {
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

    public abstract void Draw(Graphics gr, Point posModificator, float scale);

    public abstract string HtmlText();

    public abstract bool IsLineBreak();

    public abstract bool IsPossibleLineBreak();

    public abstract bool IsSpace();

    public bool IsVisible(float zoom, Point drawingPos, Rectangle area) {
        if (area.Width < 1 || area.Height < 1) { return true; }

        var px = (Pos.X * zoom) + drawingPos.X;
        if (px > area.Right) { return false; }

        var py = (Pos.Y * zoom) + drawingPos.Y;
        return py <= area.Bottom && px + (Size.Width * zoom) >= area.Left &&
               py + (Size.Height * zoom) >= area.Top;
    }

    public abstract bool IsWordSeparator();

    public override List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [.. base.ParseableItems()];
        result.ParseableAdd("Style", _style);
        result.ParseableAdd("Font", _font as IStringable);

        return result;
    }

    public override void ParseFinished(string parsed) => base.ParseFinished(parsed);//if (_parent != null) {//    _parent.StyleChanged -= _parent_StyleChanged;//    _parent = null;//}//Pos = PointF.Empty;//_font = null;//_parent = null;//_size = new Size(0, 0);

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "classid":
                return value.ToNonCritical() == MyClassId;

            case "style":
                _style = (PadStyles)IntParse(value.FromNonCritical());
                return true;

            case "font":
                _font = BlueFont.Get(value.FromNonCritical());
                return true;
        }
        return false;
    }

    ///// <summary>
    /////
    ///// </summary>
    ///// <param name="zoom"></param>
    ///// <param name="drawingPos">Muss bereits Skaliert sein</param>
    ///// <returns></returns>
    //public bool IsVisible(float zoom, Point drawingPos, Rectangle drawingArea) => drawingArea is { Width: < 1, Height: < 1 } ||
    //    ((drawingArea.Width <= 0 || (Pos.X * zoom) + drawingPos.X <= drawingArea.Right)
    //     && (drawingArea.Height <= 0 || (Pos.Y * zoom) + drawingPos.Y <= drawingArea.Bottom)
    //     && ((Pos.X + Size.Width) * zoom) + drawingPos.X >= drawingArea.Left
    //     && ((Pos.Y + Size.Height) * zoom) + drawingPos.Y >= drawingArea.Top);
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

    private void _parent_StyleChanged(object sender, System.EventArgs e) => this.InvalidateFont();

    #endregion

    // // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
    // ~ExtChar()
    // {
    //     // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
    //     Dispose(disposing: false);
    // }
}
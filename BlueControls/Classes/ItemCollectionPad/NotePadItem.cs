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

using BlueBasics;
using BlueBasics.Classes;
using BlueBasics.Enums;
using BlueControls.Classes.ItemCollectionPad.Abstract;
using BlueControls.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using static BlueBasics.ClassesStatic.Geometry;

namespace BlueControls.Classes.ItemCollectionPad;

public class NotePadItem : AbstractPadItem {

    #region Fields

    public const string PointSymbol = "Kreis2";
    private const int SymbolSize = 24;
    private readonly PointM _position;
    private readonly PrivateNoteEntry? _privateNote;

    #endregion

    #region Constructors

    public NotePadItem() : this(string.Empty) { }

    public NotePadItem(string keyName) : base(keyName) {
        _position = new PointM(this, "Pos", 0, 0);
        MovablePoint.Add(_position);
        PointsForSuccessfullyMove.Add(_position);
    }

    public NotePadItem(string keyName, float x, float y, string symbol, string note) : this(keyName) {
        _position.SetTo(x, y, false);
        Symbol = symbol;
        Note = note;
    }

    public NotePadItem(string keyName, float x, float y, PrivateNoteEntry privateNote) : this(keyName) {
        _privateNote = privateNote;
        _position.SetTo(x, y, false);
    }

    #endregion

    #region Properties

    public static string ClassId => "NOTE";

    public override string Description => "Eine Notiz mit Symbol und Text";

    public string Note {
        get => _privateNote?.Note ?? string.Empty;
        set {
            if (_privateNote != null) { _privateNote.Note = value; }
        }
    }

    public PrivateNoteEntry? PrivateNote => _privateNote;

    public string Symbol {
        get => _privateNote?.Symbol ?? PointSymbol;
        set {
            if (_privateNote != null) { _privateNote.Symbol = value; }
        }
    }

    protected override int SaveOrder => 999;

    #endregion

    #region Methods

    public override bool CanvasContains(PointF value, float zoom) {
        var ne = 10.ControlToCanvas(zoom) + 1;
        return GetLength(value, _position) < ne;
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        if (_privateNote != null) { return _privateNote.GetProperties(widthOfControl); }

        return [];
    }

    public override void InitialPosition(int x, int y, int width, int height) => _position.SetTo(x + width / 2f, y + height / 2f, false);

    public override List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [.. base.ParseableItems()];
        result.ParseableAdd("Symbol", Symbol);
        result.ParseableAdd("Note", Note.ToNonCritical());
        return result;
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "symbol":
                Symbol = value.FromNonCritical();
                return true;

            case "note":
                Note = value.FromNonCritical();
                return true;
        }
        return base.ParseThis(key, value);
    }

    public override string ReadableText() => string.IsNullOrEmpty(Note) ? "Notiz" : Note;

    public void SetPosition(float x, float y) => _position.SetTo(x, y, false);

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Kreis, 16);

    protected override RectangleF CalculateCanvasUsedArea() {
        var size = SymbolSize;
        if (!string.IsNullOrEmpty(Note)) {
            size += Note.Length * 8;
        }
        return new RectangleF(_position.X - SymbolSize / 2f, _position.Y - SymbolSize / 2f, size, SymbolSize);
    }

    protected override void DrawExplicit(Graphics gr, Rectangle visibleAreaControl, RectangleF positionControl, float zoom, float offsetX, float offsetY, bool forPrinting) {
        var pos = _position.CanvasToControl(zoom, offsetX, offsetY);

        var sz = SymbolSize.CanvasToControl(zoom);
        if (sz < 4) { sz = 4; }

        var symbolRect = new RectangleF(pos.X - sz / 2f, pos.Y - sz / 2f, sz, sz);

        var pen = GetPen();

        gr.DrawEllipse(pen, symbolRect);

        if (!string.IsNullOrEmpty(Note)) {
            var fontSize = Math.Max(8, (int)(12 * zoom));
            if (fontSize < 6) { fontSize = 6; }
            using var fn = new Font("Arial", fontSize, FontStyle.Regular);
            var textSize = gr.MeasureString(Note, fn);

            var textX = pos.X + sz / 2f + 4;
            var textY = pos.Y - textSize.Height / 2f;

            using var bgBrush = new SolidBrush(Color.FromArgb(200, 255, 255, 255));
            var bgRect = new RectangleF(textX - 2, textY - 1, textSize.Width + 4, textSize.Height + 2);
            gr.FillRectangle(bgBrush, bgRect);
            gr.DrawRectangle(pen, bgRect.X, bgRect.Y, bgRect.Width, bgRect.Height);

            BlueFont.DrawString(gr, Note, fn, new SolidBrush(pen.Color), textX, textY);
        }
    }

    private Pen GetPen() => PrivateNoteEntry.PenForSymbol(Symbol);

    #endregion
}
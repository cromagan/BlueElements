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

using System.Drawing;
using BlueControls.Enums;
using BlueDatabase;

namespace BlueControls.Extended_Text;

public abstract class ExtChar {

    #region Fields

    public PointF Pos = PointF.Empty;
    private BlueFont? _font;
    private SizeF _size;

    #endregion

    #region Constructors

    public ExtChar(BlueFont font) : base() {
        _font = font;
        _size = SizeF.Empty;
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

    public MarkState Marking { get; set; }

    public SizeF Size {
        get {
            if (_size.IsEmpty) { _size = CalculateSize(); }
            return _size;
        }
    }

    #endregion

    #region Methods

    public abstract void Draw(Graphics gr, Point posModificator, float zoom);

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

    public PadStyles Stufe(string sheetStyle) {
        if (Font == null || Skin.StyleDb is not Database db) { return  PadStyles.Standard; }

        var f1 = new FilterItem(db.Column["Font"], BlueDatabase.Enums.FilterType.Istgleich_GroßKleinEgal, Font.KeyName);
        var f2 = new FilterItem(db.Column["Style"], BlueDatabase.Enums.FilterType.Istgleich_GroßKleinEgal, sheetStyle);

        var r = db.Row[f1, f2];


        if( r== null) {  return  PadStyles.Standard; }


        return (PadStyles)r.CellGetInteger("Style");
    }

    protected abstract SizeF CalculateSize();

    #endregion
}
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
using BlueBasics.Interfaces;
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueDatabase;

namespace BlueControls.Extended_Text;

public abstract class ExtChar : IStyleableOne, IChild, IStyleableChild {

    #region Fields

    public PointF Pos = PointF.Empty;
    private BlueFont? _font;
    private IParent? _parent;

    private SizeF _size;

    private int _stufe;
    private PadStyles _style = PadStyles.Style_Standard;

    #endregion

    #region Constructors

    protected ExtChar(ExtText parent, BlueFont? font, int stufe) : base() {
        _parent = parent;
        _font = font;
        _stufe = stufe;
        _size = SizeF.Empty;
    }

    #endregion

    #region Properties

    public BlueFont? Font {
        get {
            _font ??= this.GetFont(_stufe);
            return _font;
        }
        set => _font = value;
    }

    public MarkState Marking { get; set; }

    public IParent? Parent {
        get => _parent;
        set {
            if (_parent != value) {
                _parent = value;
                this.InvalidateFont();
                _size = new Size(0, 0);
            }
        }
    }

    public RowItem? SheetStyle {
        get {
            if (_parent is IStyleable ist) { return ist.SheetStyle; }
            return null;
        }
    }

    public float SheetStyleScale {
        get {
            if (_parent is IStyleable ist) { return ist.SheetStyleScale; }
            return 1f;
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
            _size = new Size(0, 0);
        }
    }

    public int Stufe {
        get => _stufe;
        set {
            if (_stufe == value) { return; }
            _stufe = value;
            this.InvalidateFont();
            _size = new Size(0, 0);
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

    protected abstract SizeF CalculateSize();

    #endregion
}
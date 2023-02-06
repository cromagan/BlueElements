// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace BlueControls.ItemCollection;

public abstract class FixedRectangleBitmapPadItem : FixedRectanglePadItem {

    #region Fields

    private Bitmap? _generatedBitmap;

    #endregion

    #region Constructors

    protected FixedRectangleBitmapPadItem(string internalname) : base(internalname) => RemovePic();

    #endregion

    #region Properties

    public Bitmap? GeneratedBitmap {
        get {
            if (_generatedBitmap != null) { return _generatedBitmap; }
            GeneratePic();
            return _generatedBitmap;
        }
        protected set {
            _generatedBitmap = value;
            Size = _generatedBitmap?.Size ?? Size.Empty;
        }
    }

    #endregion

    #region Methods

    protected override RectangleF CalculateUsedArea() {
        if (_generatedBitmap == null) { GeneratePic(); } // Um die Size zu erhalten

        return base.CalculateUsedArea();
    }

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);

        if (!IsDisposed) {
            if (disposing) {
                // TODO: Verwalteten Zustand (verwaltete Objekte) bereinigen
            }
            RemovePic();

            // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer überschreiben
            // TODO: Große Felder auf NULL setzen
        }
    }

    protected override void DrawExplicit(Graphics gr, RectangleF positionModified, float zoom, float shiftX, float shiftY, bool forPrinting) {
        if (_generatedBitmap == null) { GeneratePic(); }

        #region Bild zeichnen

        try {
            if (_generatedBitmap != null) {
                if (forPrinting) {
                    gr.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    gr.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                } else {
                    gr.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
                    gr.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                }
                gr.DrawImage(_generatedBitmap, positionModified);
            }
        } catch { }

        #endregion

        base.DrawExplicit(gr, positionModified, zoom, shiftX, shiftY, forPrinting);
    }

    protected abstract void GeneratePic();

    protected override void ParseFinished() => SizeChanged();

    protected void RemovePic() {
        _generatedBitmap?.Dispose();
        _generatedBitmap = null;
    }

    #endregion
}
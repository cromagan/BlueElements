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

using System.Drawing;
using System.Drawing.Drawing2D;

namespace BlueControls.Classes.ItemCollectionPad.Abstract;

public abstract class FixedRectangleBitmapPadItem : FixedRectanglePadItem {

    #region Fields

    private Bitmap? _generatedBitmap;

    #endregion

    #region Constructors

    protected FixedRectangleBitmapPadItem(string keyName) : base(keyName) => RemovePic();

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
            CanvasSize = _generatedBitmap?.Size ?? SizeF.Empty;
        }
    }

    #endregion

    #region Methods

    public override void ParseFinished(string parsed) {
        base.ParseFinished(parsed);
        SizeChanged();
    }

    protected override RectangleF CalculateCanvasUsedArea() {
        if (_generatedBitmap == null) { GeneratePic(); } // Um die CanvasSize zu erhalten

        return base.CalculateCanvasUsedArea();
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

    protected override void DrawExplicit(Graphics gr, Rectangle visibleAreaControl, RectangleF positionControl, float zoom, float offsetX, float offsetY, bool forPrinting) {
        if (_generatedBitmap == null) { GeneratePic(); }

        #region Bild zeichnen

        try {
            if (_generatedBitmap != null) {
                if (forPrinting) {
                    gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    gr.PixelOffsetMode = PixelOffsetMode.Half;
                } else {
                    gr.InterpolationMode = InterpolationMode.Low;
                    gr.PixelOffsetMode = PixelOffsetMode.Half;
                }
                gr.DrawImage(_generatedBitmap, positionControl);
            }
        } catch { }

        #endregion
    }

    protected abstract void GeneratePic();

    protected void RemovePic() {
        _generatedBitmap?.Dispose();
        _generatedBitmap = null;
    }

    #endregion
}
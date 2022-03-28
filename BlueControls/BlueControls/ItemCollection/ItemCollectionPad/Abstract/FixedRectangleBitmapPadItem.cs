// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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

namespace BlueControls.ItemCollection {

    public abstract class FixedRectangleBitmapPadItem : FixedRectanglePadItem, IDisposable {

        #region Fields

        private bool _disposedValue;
        private Bitmap? _generatedBitmap;

        #endregion

        #region Constructors

        protected FixedRectangleBitmapPadItem(string internalname) : base(internalname) {
            RemovePic();
            //GeneratePic(); // Im Construcor nicht möglich, weil noch Werte fehlen.
        }

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
                if (_generatedBitmap == null) {
                    Size = Size.Empty;
                } else {
                    Size = _generatedBitmap.Size;
                }
            }
        }

        #endregion

        #region Methods

        public void Dispose() {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
            Dispose(true);
            GC.SuppressFinalize(this);
            base.Dispose();
        }

        protected override RectangleF CalculateUsedArea() {
            if (_generatedBitmap == null) { GeneratePic(); } // Um die Size zu erhalten

            return base.CalculateUsedArea();
        }

        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);

            if (!_disposedValue) {
                if (disposing) {
                    // TODO: Verwalteten Zustand (verwaltete Objekte) bereinigen
                }
                RemovePic();

                // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer überschreiben
                // TODO: Große Felder auf NULL setzen
            }
        }

        //public override string ToString() {
        //    var t = base.ToString();
        //    t = t.Substring(0, t.Length - 1) + ", ";
        //    if (Drehwinkel != 0) { t = t + "Rotation=" + Drehwinkel + ", "; }
        //    return t.Trim(", ") + "}";
        //}
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

        //public override List<GenericControl> GetStyleOptions() {
        //    List<FlexiControl> l = new()
        //    {
        //        new FlexiControl(),
        //        //new FlexiControlForProperty(()=> this.Drehwinkel"),
        //        //new FlexiControlForProperty(()=> this.Größe_fixiert")
        //    };
        //    l.AddRange(base.GetStyleOptions());
        //    return l;
        //}
        //    public void SetCoordinates(RectangleF r) {
        //    var vr = r.PointOf(enAlignment.Horizontal_Vertical_Center);
        //    var ur = UsedArea();
        //    p_LO.SetTo(vr.X - (ur.Width / 2), vr.Y - (ur.Height / 2));
        //    p_RU.SetTo(p_LO.X + ur.Width, p_LO.Y + ur.Height);
        //}
        //public virtual void SizeChanged() {
        //    // Punkte immer komplett setzen. Um eventuelle Parsing-Fehler auszugleichen
        //    p_L.SetTo(p_LO.X, p_LO.Y + ((p_LU.Y - p_LO.Y) / 2));
        //    p_R.SetTo(p_RO.X, p_LO.Y + ((p_LU.Y - p_LO.Y) / 2));
        //    p_U.SetTo(p_LO.X + ((p_RO.X - p_LO.X) / 2), p_RU.Y);
        //    p_O.SetTo(p_LO.X + ((p_RO.X - p_LO.X) / 2), p_RO.Y);
        //}
        protected abstract void GeneratePic();

        protected override void ParseFinished() => SizeChanged();

        protected void RemovePic() {
            _generatedBitmap?.Dispose();
            _generatedBitmap = null;
        }

        #endregion

        // // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
        // ~FixedRectangleBitmapPadItem()
        // {
        //     // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        //     Dispose(disposing: false);
        // }
    }
}
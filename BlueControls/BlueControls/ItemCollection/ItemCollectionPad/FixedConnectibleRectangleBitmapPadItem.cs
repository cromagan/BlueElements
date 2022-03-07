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

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.EventArgs;
using System;
using System.Drawing;

namespace BlueControls.ItemCollection {

    public abstract class FixedRectangleBitmapPadItem : BasicPadItem, IDisposable {

        #region Fields

        public readonly ListExt<ItemConnection> ConnectsTo = new();

        protected PointM? PL;

        /// <summary>
        /// Dieser Punkt bestimmt die ganzen Koordinaten. Die anderen werden nur mitgeschleift
        /// </summary>
        protected PointM? PLo;

        protected PointM? PLu;

        protected PointM? PO;

        protected PointM? PR;

        protected PointM? PRo;

        protected PointM? PRu;

        protected PointM? PU;

        private bool _disposedValue;
        private Bitmap? _generatedBitmap;

        #endregion

        #region Constructors

        protected FixedRectangleBitmapPadItem(string internalname) : base(internalname) {
            PLo = new PointM(this, "LO", 0, 0);
            PRo = new PointM(this, "RO", 0, 0);
            PRu = new PointM(this, "RU", 0, 0);
            PLu = new PointM(this, "LU", 0, 0);
            PL = new PointM(this, "L", 0, 0);
            PR = new PointM(this, "R", 0, 0);
            PO = new PointM(this, "O", 0, 0);
            PU = new PointM(this, "U", 0, 0);
            MovablePoint.Add(PLo);
            MovablePoint.Add(PRo);
            MovablePoint.Add(PLu);
            MovablePoint.Add(PRu);
            MovablePoint.Add(PL);
            MovablePoint.Add(PR);
            MovablePoint.Add(PU);
            MovablePoint.Add(PO);
            PointsForSuccesfullyMove.Add(PLo);

            ConnectsTo.ItemAdded += ConnectsTo_ItemAdded;
            ConnectsTo.ItemRemoving += ConnectsTo_ItemRemoving;

            RemovePic();
            //GeneratePic(true);
        }

        #endregion

        #region Properties

        public Bitmap GeneratedBitmap {
            get {
                if (_generatedBitmap != null) { return _generatedBitmap; }
                _generatedBitmap = GeneratePic();
                return _generatedBitmap;
            }
        }

        #endregion

        #region Methods

        public void Dispose() {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public override void PointMoved(object sender, MoveEventArgs e) {
            base.PointMoved(sender, e);
            var x = 0f;
            var y = 0f;

            var point = (PointM)sender;

            if (point != null) {
                x = point.X;
                y = point.Y;
            }

            if (point == PLo) {
                if (e.Y) {
                    PRu.Y = y + GeneratedBitmap.Height;
                    PO.Y = y;
                }
                if (e.X) {
                    PRu.X = x + GeneratedBitmap.Width;
                    PL.X = x;
                }
            }

            if (point == PRu) {
                if (e.X) {
                    PLo.X = x - GeneratedBitmap.Width;
                    PR.X = x;
                }
                if (e.Y) {
                    PLo.Y = y - GeneratedBitmap.Height;
                    PU.Y = y;
                }
            }

            if (point == PRo) {
                if (e.Y) { PO.Y = y; }
                if (e.X) { PR.X = x; }
            }

            if (point == PLu) {
                if (e.X) { PL.X = x; }
                if (e.Y) { PU.Y = y; }
            }

            if (point == PO && e.Y) {
                PLo.Y = y;
                PRo.Y = y;
            }

            if (point == PU && e.Y) {
                PLu.Y = y;
                PRu.Y = y;
            }

            if (point == PL && e.X) {
                PLo.X = x;
                PLu.X = x;
            }

            if (point == PR && e.X) {
                PRo.X = x;
                PRu.X = x;
            }

            SizeChanged();
        }

        //        case "rotation":
        //            _drehwinkel = int.Parse(value);
        //            return true;
        //    }
        //    return false;
        //}
        public void RemovePic() {
            _generatedBitmap?.Dispose();
            _generatedBitmap = null;
        }

        //public override bool ParseThis(string tag, string value) {
        //    if (base.ParseThis(tag, value)) { return true; }
        //    switch (tag) {
        //        case "fixsize": // TODO: Entfernt am 24.05.2021
        //            //_größe_fixiert = value.FromPlusMinus();
        //            return true;
        public void SetLeftTopPoint(float x, float y) => PLo.SetTo(x, y);

        public virtual void SizeChanged() {
            // Punkte immer komplett setzen. Um eventuelle Parsing-Fehler auszugleichen
            PL.SetTo(PLo.X, PLo.Y + ((PLu.Y - PLo.Y) / 2));
            PR.SetTo(PRo.X, PLo.Y + ((PLu.Y - PLo.Y) / 2));
            PU.SetTo(PLo.X + ((PRo.X - PLo.X) / 2), PRu.Y);
            PO.SetTo(PLo.X + ((PRo.X - PLo.X) / 2), PRo.Y);
        }

        protected override RectangleF CalculateUsedArea() {
            var bmp = GeneratedBitmap;
            if (bmp == null || PLo == null) { return RectangleF.Empty; }
            var r = new RectangleF(PLo.X, PLo.Y, bmp.Width, bmp.Height);

            if (ConnectsTo == null) { return r; }

            foreach (var thisc in ConnectsTo) {
                if (thisc.Item != null && !thisc.Item.UsedArea.IsEmpty) {
                    r = RectangleF.Union(r, thisc.Item.UsedArea);
                }
            }
            return r;
        }

        protected virtual void Dispose(bool disposing) {
            if (!_disposedValue) {
                if (disposing) {
                    // TODO: Verwalteten Zustand (verwaltete Objekte) bereinigen
                    ConnectsTo.ItemAdded -= ConnectsTo_ItemAdded;
                    ConnectsTo.Clear();
                    ConnectsTo.ItemRemoving -= ConnectsTo_ItemRemoving;
                }
                RemovePic();

                // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer überschreiben
                // TODO: Große Felder auf NULL setzen
                _disposedValue = true;
            }
        }

        //public override string ToString() {
        //    var t = base.ToString();
        //    t = t.Substring(0, t.Length - 1) + ", ";
        //    if (Drehwinkel != 0) { t = t + "Rotation=" + Drehwinkel + ", "; }
        //    return t.Trim(", ") + "}";
        //}
        protected override void DrawExplicit(Graphics gr, RectangleF drawingCoordinates, float zoom, float shiftX, float shiftY, bool forPrinting) {

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
                    gr.DrawImage(_generatedBitmap, drawingCoordinates);
                }
            } catch { }

            #endregion

            var line = 1f;
            if (zoom > 1) { line = zoom; }

            #region Umrandung Zeichnen

            try {
                if (!forPrinting || _generatedBitmap == null) {
                    gr.DrawRectangle(new Pen(Color.Gray, line), drawingCoordinates);

                    if (drawingCoordinates.Width < 1 || drawingCoordinates.Height < 1) {
                        gr.DrawEllipse(new Pen(Color.Gray, 3), drawingCoordinates.Left - 5, drawingCoordinates.Top + 5, 10, 10);
                        gr.DrawLine(ZoomPad.PenGray, drawingCoordinates.PointOf(enAlignment.Top_Left), drawingCoordinates.PointOf(enAlignment.Bottom_Right));
                    }
                }
            } catch { }

            #endregion

            #region Verknüpfte Pfeile Zeichnen

            foreach (var thisV in ConnectsTo) {
                if (Parent.Contains(thisV.Item) && thisV != null && thisV.Item != this) {
                    var m1 = UsedArea.PointOf(enAlignment.Horizontal_Vertical_Center);
                    var m2 = thisV.Item.UsedArea.PointOf(enAlignment.Horizontal_Vertical_Center);

                    var t2 = UsedArea.NearestLineMiddle(m2).ZoomAndMove(zoom, shiftX, shiftY);
                    var t1 = thisV.Item.UsedArea.NearestLineMiddle(m1).ZoomAndMove(zoom, shiftX, shiftY);

                    if (Geometry.GetLenght(t1, t2) > 1) {
                        gr.DrawLine(new Pen(Color.Gray, line), t1, t2);
                    }
                }
            }

            #endregion
        }

        //public override List<FlexiControl> GetStyleOptions() {
        //    List<FlexiControl> l = new()
        //    {
        //        new FlexiControl(),
        //        //new FlexiControlForProperty(this, "Drehwinkel"),
        //        //new FlexiControlForProperty(this, "Größe_fixiert")
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
        protected abstract Bitmap GeneratePic();

        protected override void ParseFinished() => SizeChanged();

        private void ConnectsTo_ItemAdded(object sender, BlueBasics.EventArgs.ListEventArgs e) {
            var x = (ItemConnection)e.Item;

            x.Item.Changed += Item_Changed;
        }

        private void ConnectsTo_ItemRemoving(object sender, BlueBasics.EventArgs.ListEventArgs e) {
            var x = (ItemConnection)e.Item;
            x.Item.Changed -= Item_Changed;
        }

        private void Item_Changed(object sender, System.EventArgs e) => OnChanged();

        #endregion

        // // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
        // ~FixedRectangleBitmapPadItem()
        // {
        //     // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        //     Dispose(disposing: false);
        // }
    }
}
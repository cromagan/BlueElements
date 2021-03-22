#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2020 Christian Peter
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
#endregion

using BlueBasics;
using BlueControls.Controls;
using BlueControls.Enums;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;

namespace BlueControls.ItemCollection {
    // LinenKollision
    //http://www.vb-fun.de/cgi-bin/loadframe.pl?ID=vb/tipps/tip0294.shtml

    //'Imports Microsoft.VisualBasic


    public class GridPadItem : BasicPadItem {




        #region  Variablen-Deklarationen 

        internal PointM NP;
        internal decimal GridShow = 10M; // in mm
                                         //   Dim _RasterSnap As Decimal = 50 ' in mm


        //Dim _Design As enDesign

        private readonly Metafile NPEMF = new Metafile(modAllgemein.GetEmmbedResource(Assembly.GetExecutingAssembly(), "Nullpunkt.emf"));

        #endregion


        #region  Event-Deklarationen + Delegaten 

        #endregion


        #region  Construktor + Initialize 


        public GridPadItem(ItemCollectionPad parent, string internalname) : this(parent, internalname, PadStyles.Style_Standard, new Point(0, 0)) { }

        public GridPadItem(ItemCollectionPad parent, PadStyles style, Point nullpunkt) : this(parent, string.Empty, style, nullpunkt) { }

        public GridPadItem(ItemCollectionPad parent, string internalname, PadStyles style, Point nullpunkt) : base(parent, internalname) {
            NP = new PointM(this, "Nullpunkt", 0, 0, true, false, true);
            NP.SetTo(nullpunkt);
            Stil = style;

            Stil = PadStyles.Style_Überschrift_Haupt;
            GridShow = 10M;

            Points.Add(NP);

        }





        #endregion


        #region  Properties 


        #endregion


        protected override string ClassId() {
            return "GRID";
        }

        public override bool Contains(PointF value, decimal zoomfactor) {
            return false;
        }



        protected override RectangleM CalculateUsedArea() {
            //var r = new RectangleM(Parent.DruckbereichRect()); // muss der gesamte druckbereich sein, ansonsten wirds ja nicht angezeigt, wenn der NP ausserhalb dem Bild ist
            //r.ExpandTo(NP);
            //return r;
            return new RectangleM(NP.X - 10, NP.Y - 10, 20, 20);
        }


        protected override void DrawExplicit(Graphics GR, RectangleF DCoordinates, decimal cZoom, decimal shiftX, decimal shiftY, enStates vState, Size SizeOfParentControl, bool ForPrinting) {
            if (Stil == PadStyles.Undefiniert) { return; }

            Color c = Skin.GetBlueFont(Stil, Parent.SheetStyle).Color_Main;


            Pen p = new Pen(Color.FromArgb(30, c.R, c.G, c.B), 1);
            float ex = 0;

            PointF po = NP.ZoomAndMove(cZoom, shiftX, shiftY);


            float mo = (float)(modConverter.mmToPixel(GridShow, ItemCollectionPad.DPI) * cZoom);

            int count = 0;
            do {
                count++;
                if (count > 20) { break; }
                if (mo < 5) { mo *= 2; }
            } while (true);

            if (mo >= 5) {
                do {
                    GR.DrawLine(p, po.X + (int)ex, 0, po.X + (int)ex, SizeOfParentControl.Height);
                    GR.DrawLine(p, 0, po.Y + (int)ex, SizeOfParentControl.Width, po.Y + (int)ex);

                    if (ex > 0) {
                        GR.DrawLine(p, po.X - (int)ex, 0, po.X - (int)ex, SizeOfParentControl.Height);
                        GR.DrawLine(p, 0, po.Y - (int)ex, SizeOfParentControl.Width, po.Y - (int)ex);
                    }

                    ex += mo;
                    if (po.X - ex < 0 && po.Y - ex < 0 && po.X + ex > SizeOfParentControl.Width && po.Y + ex > SizeOfParentControl.Height) {
                        break;
                    }
                } while (true);
            }


            if (!ForPrinting) {

                GR.DrawImage(NPEMF, new RectangleF(po.X - 7, po.Y - 7, 15, 15));
            }
        }
        public override void Move(decimal x, decimal y) {
            NP.SetTo(NP.X + x, NP.Y + y);
            base.Move(x, y);
        }

        protected override void GenerateInternalRelationExplicit() { }

        //public override void SetCoordinates(RectangleM r)
        //{
        //    NP.SetTo(r.PointOf(enAlignment.Horizontal_Vertical_Center));
        //    base.SetCoordinates(r);
        //}


        public override bool ParseThis(string tag, string value) {
            if (base.ParseThis(tag, value)) { return true; }

            switch (tag) {
                case "grid":
                    GridShow = int.Parse(value);
                    return true;
            }
            return false;
        }


        protected override void ParseFinished() { }

        public override string ToString() {
            string t = base.ToString();
            t = t.Substring(0, t.Length - 1) + ", ";
            return t + "Grid=" + GridShow + "}";
        }




        public override List<FlexiControl> GetStyleOptions() {
            List<FlexiControl> l = new List<FlexiControl>();

            AddLineStyleOption(l);



            l.AddRange(base.GetStyleOptions());
            return l;
        }

        //public override void DoStyleCommands(object sender, List<string> Tags, ref bool CloseMenu)
        //{
        //    Stil = (PadStyles)int.Parse(Tags.TagGet("Format"));
        //}
    }
}
#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2019 Christian Peter
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
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;

namespace BlueControls.ItemCollection
{
    // LinenKollision
    //http://www.vb-fun.de/cgi-bin/loadframe.pl?ID=vb/tipps/tip0294.shtml

    //'Imports Microsoft.VisualBasic


    public class GridPadItem : BasicPadItem
    {


        public override void DesignOrStyleChanged()
        {
            // Keine Variablen zum Reseten, ein Invalidate reicht
        }


        #region  Variablen-Deklarationen 

        internal PointDF NP;
        internal decimal GridShow = 10M; // in mm
                                         //   Dim _RasterSnap As Decimal = 50 ' in mm


        //Dim _Design As enDesign

        private readonly Metafile NPEMF = new Metafile(modAllgemein.GetEmmbedResource(Assembly.GetExecutingAssembly(), "Nullpunkt.emf"));

        #endregion


        #region  Event-Deklarationen + Delegaten 

        #endregion


        #region  Construktor + Initialize 


        public GridPadItem()
        { }


        public GridPadItem(PadStyles vFormat, Point cNP)
        {
            NP.SetTo(cNP);
            Format = vFormat;
        }


        protected override void Initialize()
        {
            base.Initialize();
            NP = new PointDF(this, "Nullpunkt", 0, 0, true, false, true);
            //NP = new PointDF(this, "Nullpunkt", 0, 0);
            Format = PadStyles.Style_Überschrift_Haupt;
            GridShow = 10M;
        }


        #endregion


        #region  Properties 


        #endregion


        protected override string ClassId()
        {
            return "GRID";
        }

        public override bool Contains(PointF value, decimal zoomfactor)
        {
            return false;
        }


        public override List<PointDF> PointList()
        {
            var l = new List<PointDF>();
            l.Add(NP);

            return l;
        }


        public override RectangleDF UsedArea()
        {
            return new RectangleDF(NP.X - 1, NP.Y - 1, 2, 2);
        }


        protected override void DrawExplicit(Graphics GR, Rectangle DCoordinates, decimal cZoom, decimal MoveX, decimal MoveY, enStates vState, Size SizeOfParentControl, bool ForPrinting)
        {
            if (Format == PadStyles.Undefiniert) { return; }

            var c = GenericControl.Skin.GetBlueFont(Format, Parent.SheetStyle).Color_Main;


            var p = new Pen(Color.FromArgb(30, c.R, c.G, c.B), 1);
            decimal ex = 0;

            var po = DCoordinates.PointOf(enAlignment.HorizontalCenter);


            var mo = modConverter.mmToPixel(GridShow, ItemCollectionPad.DPI) * cZoom;

            var count = 0;
            do
            {
                count += 1;
                if (count > 20) { break; }
                if (mo < 5) { mo = mo * 2; }
            } while (true);

            if (mo >= 5)
            {
                do
                {
                    GR.DrawLine(p, po.X + (int)(ex), 0, po.X + (int)(ex), SizeOfParentControl.Height);
                    GR.DrawLine(p, 0, po.Y + (int)(ex), SizeOfParentControl.Width, po.Y + (int)(ex));

                    if (ex > 0)
                    {
                        GR.DrawLine(p, po.X - (int)(ex), 0, po.X - (int)(ex), SizeOfParentControl.Height);
                        GR.DrawLine(p, 0, po.Y - (int)(ex), SizeOfParentControl.Width, po.Y - (int)(ex));
                    }

                    ex = ex + mo;
                    if (po.X - ex < 0 && po.Y - ex < 0 && po.X + ex > SizeOfParentControl.Width && po.Y + ex > SizeOfParentControl.Height)
                    {
                        break;
                    }
                } while (true);
            }


            if (!ForPrinting)
            {

                GR.DrawImage(NPEMF, new Rectangle(po.X - 7, po.Y - 7, 15, 15));
            }
        }

        public override void SetCoordinates(RectangleDF r)
        {
            NP.SetTo(r.PointOf(enAlignment.Horizontal_Vertical_Center));
            RecomputePointAndRelations();
        }


        protected override bool ParseExplicit(KeyValuePair<string, string> pair)
        {

            switch (pair.Key)
            {
                case "grid":
                    GridShow = int.Parse(pair.Value);
                    return true;


                case "checked":
                    return true;

            }

            return false;
        }

        public override string ToString()
        {
            var t = base.ToString();
            t = t.Substring(0, t.Length - 1) + ", ";
            return t + "Grid=" + GridShow + "}";
        }

        protected override void KeepInternalLogic()
        {
            //Nix
        }


        public override void GenerateInternalRelation(List<clsPointRelation> relations)
        {
            // Nix zu Tun
        }

        public override List<FlexiControl> GetStyleOptions(object sender, System.EventArgs e)
        {
            var l = new List<FlexiControl>();

            var Rahms = new ItemCollectionList();
            //   Rahms.Add(New ItemCollection.TextListItem(CInt(PadStyles.Undefiniert).ToString, "Ohne Rahmen", enImageCode.Kreuz))
            Rahms.Add(new TextListItem(((int)PadStyles.Style_Überschrift_Haupt).ToString(), "Haupt-Überschrift", GenericControl.Skin.GetBlueFont(PadStyles.Style_Überschrift_Haupt, Parent.SheetStyle).SymbolOfLine()));
            Rahms.Add(new TextListItem(((int)PadStyles.Style_Überschrift_Untertitel).ToString(), "Untertitel für Haupt-Überschrift", GenericControl.Skin.GetBlueFont(PadStyles.Style_Überschrift_Untertitel, Parent.SheetStyle).SymbolOfLine()));
            Rahms.Add(new TextListItem(((int)PadStyles.Style_Überschrift_Kapitel).ToString(), "Überschrift für Kapitel", GenericControl.Skin.GetBlueFont(PadStyles.Style_Überschrift_Kapitel, Parent.SheetStyle).SymbolOfLine()));
            Rahms.Add(new TextListItem(((int)PadStyles.Style_Standard).ToString(), "Standard", GenericControl.Skin.GetBlueFont(PadStyles.Style_Standard, Parent.SheetStyle).SymbolOfLine()));
            Rahms.Add(new TextListItem(((int)PadStyles.Style_StandardFett).ToString(), "Standard Fett", GenericControl.Skin.GetBlueFont(PadStyles.Style_StandardFett, Parent.SheetStyle).SymbolOfLine()));
            Rahms.Add(new TextListItem(((int)PadStyles.Style_StandardAlternativ).ToString(), "Standard Alternativ-Design", GenericControl.Skin.GetBlueFont(PadStyles.Style_StandardAlternativ, Parent.SheetStyle).SymbolOfLine()));
            Rahms.Add(new TextListItem(((int)PadStyles.Style_KleinerZusatz).ToString(), "Kleiner Zusatz", GenericControl.Skin.GetBlueFont(PadStyles.Style_KleinerZusatz, Parent.SheetStyle).SymbolOfLine()));
            Rahms.Sort();


            l.Add(new FlexiControl("Stil", ((int)Format).ToString(), Rahms));
            return l;
        }

        public override void DoStyleCommands(object sender, List<string> Tags, ref bool CloseMenu)
        {
            Format = (PadStyles)int.Parse(Tags.TagGet("Format"));
        }
    }
}
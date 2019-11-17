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


using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;

namespace BlueControls.ItemCollection
{
    public class SymbolPadItem : FormPadItemRectangle
    {
        public override void DesignOrStyleChanged()
        {
            // Keine Variablen zum Reseten, ein Invalidate reicht
        }

        protected override string ClassId()
        {
            return "Symbol";
        }

        public enSymbol Symbol = enSymbol.Pfeil;
        public bool Gefuellt = true;
        public bool WhiteBack = true;


        protected override void Initialize()
        {
            base.Initialize();
            Symbol = enSymbol.Pfeil;
            WhiteBack = true;
        }


        protected override void DrawExplicit(Graphics GR, Rectangle DCoordinates, decimal cZoom, decimal MoveX, decimal MoveY, enStates vState, Size SizeOfParentControl, bool ForPrinting)
        {

            var trp = DCoordinates.PointOf(enAlignment.Horizontal_Vertical_Center);

            GR.TranslateTransform(trp.X, trp.Y);
            GR.RotateTransform(-Rotation);


            GraphicsPath p = null;


            var d2 = DCoordinates;
            d2.X = -DCoordinates.Width / 2;
            d2.X = -DCoordinates.Width / 2;
            d2.Y = -DCoordinates.Height / 2;

            switch (Symbol)
            {
                case enSymbol.Ohne:
                    break;

                case enSymbol.Pfeil:
                    p = modAllgemein.Poly_Arrow(d2);
                    break;

                case enSymbol.Bruchlinie:
                    p = modAllgemein.Poly_Bruchlinie(d2);
                    break;

                default:
                    Develop.DebugPrint(Symbol);
                    break;

            }


            if (WhiteBack) { GR.FillPath(Brushes.White, p); }


            if (p != null && Style != PadStyles.Undefiniert)
            {


                var f = Skin.GetBlueFont(Style, Parent.SheetStyle);

                if (Gefuellt)
                {
                    GR.FillPath(f.Brush_Color_Main, p);
                }
                else
                {

                    GR.DrawPath(f.Pen(cZoom * Parent.SheetStyleScale), p);
                }
            }



            GR.TranslateTransform(-trp.X, -trp.Y);
            GR.ResetTransform();
            if (!ForPrinting)
            {
                GR.DrawRectangle(CreativePad.PenGray, DCoordinates);
            }
        }



        public override List<FlexiControl> GetStyleOptions(object sender, System.EventArgs e)
        {

            var l = new List<FlexiControl>();

            l.Add(new FlexiControl(true));

            var Comms = new ItemCollectionList();
            Comms.Add(new TextListItem(((int)enSymbol.Ohne).ToString(), "Ohne", QuickImage.Get("Datei|32")));
            Comms.Add(new TextListItem(((int)enSymbol.Pfeil).ToString(), "Pfeil", QuickImage.Get("Pfeil_Rechts|32")));
            Comms.Add(new TextListItem(((int)enSymbol.Bruchlinie).ToString(), "Bruchlinie"));
            l.Add(new FlexiControl(true));
            l.Add(new FlexiControl("Symbol", ((int)Symbol).ToString(), Comms));

            var x = Skin.GetRahmenArt(Parent.SheetStyle, true);
            l.Add(new FlexiControl("Farbe", ((int)Style).ToString(), x));


            l.Add(new FlexiControl("Gefüllt", Gefuellt));
            l.Add(new FlexiControl("Hintergrund weiß füllen", WhiteBack));

            l.AddRange(base.GetStyleOptions(sender, e));

            return l;

        }
        public override void DoStyleCommands(object sender, List<string> Tags, ref bool CloseMenu)
        {
            base.DoStyleCommands(sender, Tags, ref CloseMenu);
            Gefuellt = Tags.TagGet("Gefüllt").FromPlusMinus();
            Symbol = (enSymbol)int.Parse(Tags.TagGet("Symbol"));
            Style = (PadStyles)int.Parse(Tags.TagGet("Farbe"));
            WhiteBack = Tags.TagGet("Hintergrund weiß füllen").FromPlusMinus();
        }


        public override string ToString()
        {
            var t = base.ToString();
            t = t.Substring(0, t.Length - 1) + ", ";
            t = t + "Symbol=" + (int)Symbol + ", ";
            t = t + "Fill=" + Gefuellt.ToPlusMinus() + ", ";
            t = t + "WhiteBack=" + WhiteBack.ToPlusMinus() + ", ";

            return t.Trim(", ") + "}";
        }


        protected override bool ParseExplicit(KeyValuePair<string, string> pair)
        {
            switch (pair.Key)
            {
                case "symbol":
                    Symbol = (enSymbol)int.Parse(pair.Value);
                    return true;
                case "fill":
                    Gefuellt = pair.Value.FromPlusMinus();
                    return true;
                case "whiteback":
                    WhiteBack = pair.Value.FromPlusMinus();
                    return true;
                default:
                    return base.ParseExplicit(pair);
            }
        }



    }
}

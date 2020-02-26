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

        public SymbolPadItem(ItemCollectionPad parent) : this(parent, string.Empty) { }


        public SymbolPadItem(ItemCollectionPad parent, string internalname) : base(parent, internalname)
        {
            Symbol = enSymbol.Pfeil;
            BackColor = Color.White;
            BorderColor = Color.Black;
            BorderWidth = 1;
        }

        protected override string ClassId()
        {
            return "Symbol";
        }

        public enSymbol Symbol = enSymbol.Pfeil;

        public Color BackColor;
        public Color BorderColor;
        public decimal BorderWidth;





        protected override void DrawExplicit(Graphics GR, RectangleF DCoordinates, decimal cZoom, decimal MoveX, decimal MoveY, enStates vState, Size SizeOfParentControl, bool ForPrinting)
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
                    p = modAllgemein.Poly_Arrow(d2.ToRect());
                    break;

                case enSymbol.Bruchlinie:
                    p = modAllgemein.Poly_Bruchlinie(d2.ToRect());
                    break;

                default:
                    Develop.DebugPrint(Symbol);
                    break;

            }

            if (p != null)
            {
                GR.FillPath(new SolidBrush(BackColor), p);
                GR.DrawPath(new Pen(BorderColor, (float)(BorderWidth * cZoom * Parent.SheetStyleScale)), p);
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

            //var x = Skin.GetRahmenArt(Parent.SheetStyle, true);
            //l.Add(new FlexiControl("Farbe", ((int)Style).ToString(), x));


            l.Add(new FlexiControl("Randdicke", BorderWidth.ToString(), enDataFormat.Gleitkommazahl, 1));
            l.Add(new FlexiControl("Randfarbe", BorderColor.ToHTMLCode(), enDataFormat.Text, 1));
            l.Add(new FlexiControl("Hintergrundfarbe", BackColor.ToHTMLCode(), enDataFormat.Text, 1));

            //  l.AddRange(base.GetStyleOptions(sender, e));

            return l;

        }
        public override void DoStyleCommands(object sender, List<string> Tags, ref bool CloseMenu)
        {
            base.DoStyleCommands(sender, Tags, ref CloseMenu);


            BackColor = Tags.TagGet("Hintergrundfarbe").FromHTMLCode();
            BorderColor = Tags.TagGet("Randfarbe").FromHTMLCode();
            decimal.TryParse(Tags.TagGet("Randdicke"), out BorderWidth);
            Symbol = (enSymbol)int.Parse(Tags.TagGet("Symbol"));
            //Style = (PadStyles)int.Parse(Tags.TagGet("Farbe"));

        }


        public override string ToString()
        {
            var t = base.ToString();
            t = t.Substring(0, t.Length - 1) + ", ";
            t = t + "Symbol=" + (int)Symbol + ", ";
            t = t + "Backcolor=" + BorderColor.ToHTMLCode() + ", ";
            t = t + "BorderColor=" + BackColor.ToHTMLCode() + ", ";
            t = t + "BorderWidth=" + BorderWidth.ToString().ToNonCritical() + ", ";

            return t.Trim(", ") + "}";
        }


        public override bool ParseThis(string tag, string value)
        {
            if (base.ParseThis(tag, value)) { return true; }

            switch (tag)
            {
                case "symbol":
                    Symbol = (enSymbol)int.Parse(value);
                    return true;
                case "backcolor":
                    BackColor = value.FromHTMLCode();
                    return true;
                case "bordercolor":
                    BorderColor = value.FromHTMLCode();
                    return true;
                case "borderwidth":
                    decimal.TryParse(value.FromNonCritical(), out BorderWidth);
                    return true;
                case "fill": // alt: 28.11.2019
                case "whiteback": // alt: 28.11.2019
                    return true;
            }
            return false;
        }
    }
}

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


using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using System.Collections.Generic;
using System.Drawing;
using static BlueBasics.Extensions;

namespace BlueControls.ItemCollection
{
    public class CookPadItem : BasicPadItem
    {




        #region  Variablen-Deklarationen 

        public string Anzeige;

        public double Menge;


        public CookPadItem Container;

        public List<CookPadItem> Childs;

        public PointDF Middlex;

        #endregion


        #region  Event-Deklarationen + Delegaten 

        #endregion


        #region  Construktor + Initialize 




        protected override void Initialize()
        {
            base.Initialize();
            Childs = new List<CookPadItem>();
            Middlex = new PointDF(this, "Middle", 0, 0);
            Anzeige = string.Empty;
            Menge = 0;
            Container = null;
            Childs.Clear();
        }


        #endregion






        protected override void DrawExplicit(Graphics GR, Rectangle DCoordinates, decimal cZoom, decimal MoveX, decimal MoveY, enStates vState, Size SizeOfParentControl, bool ForPrinting)
        {


            GR.FillEllipse(Brushes.White, DCoordinates);


            GR.DrawEllipse(CreativePad.PenGray, DCoordinates);



            var f = new Font("Arial", (float)(5 * cZoom));
            var l = GR.MeasureString(Anzeige, f);

            var p = DCoordinates.PointOf(enAlignment.Bottom_HorizontalCenter);


            GR.FillRectangle(new SolidBrush(Color.FromArgb(128, 255, 255, 255)), new Rectangle((int)(p.X - l.Width / 2.0), (int)(p.Y - l.Height * 2), (int)l.Width, (int)l.Height));

            GR.DrawString(Anzeige, f, Brushes.Black, new PointF((float)(p.X - l.Width / 2.0), p.Y - l.Height * 2));


        }

        public override void GenerateInternalRelation(List<clsPointRelation> relations)
        {

        }

        public override void SetCoordinates(RectangleDF r)
        {
            Middlex.SetTo(r.PointOf(enAlignment.Horizontal_Vertical_Center));
        }

        public override void DesignOrStyleChanged()
        {

        }



        protected override void KeepInternalLogic()
        {

        }

        protected override string ClassId()
        {
            return "COOK";
        }

        public override bool Contains(PointF value, decimal zoomfactor)
        {
            var tmp = UsedArea();
            var ne = (int)(5 / zoomfactor);
            tmp.Inflate(-ne, -ne);
            return tmp.Contains(value.ToPointDF());
        }


        public override List<PointDF> PointList()
        {
            var l = new List<PointDF>();
            l.Add(Middlex);
            return l;
        }

        public override RectangleDF UsedArea()
        {
            return new RectangleDF(Middlex.X - 30, Middlex.Y - 30, 60, 60);
        }

        protected override bool ParseExplicit(KeyValuePair<string, string> pair)
        {
            return false;
        }



        public override List<FlexiControl> GetStyleOptions(object sender, System.EventArgs e)
        {
            var l = new List<FlexiControl>();
            return l;
        }

        public override void DoStyleCommands(object sender, List<string> Tags, ref bool CloseMenu)
        {
        }
    }
}
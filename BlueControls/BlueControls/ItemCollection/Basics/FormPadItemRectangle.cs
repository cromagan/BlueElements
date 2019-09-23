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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;

namespace BlueControls.ItemCollection
{
    public abstract class FormPadItemRectangle : BasicPadItem
    {

        #region  Variablen-Deklarationen 

        internal PointDF p_LO;
        internal PointDF p_RO;
        internal PointDF p_RU;
        internal PointDF p_LU;
        public int Rotation;
        public bool FixSize;
        #endregion

        #region  Event-Deklarationen + Delegaten 

        #endregion


        #region  Construktor + Initialize 


        public FormPadItemRectangle()
        { }



        protected override void Initialize()
        {
            base.Initialize();
            p_LO = new PointDF(this, "LO", 0, 0, false, true, true);
            p_RO = new PointDF(this, "RO", 0, 0);
            p_RU = new PointDF(this, "RU", 0, 0);
            p_LU = new PointDF(this, "LU", 0, 0);
            Rotation = 0;
        }


        #endregion









        public override bool Contains(PointF value, decimal zoomfactor)
        {
            var tmp = UsedArea();
            var ne = (int)(5 / zoomfactor);
            tmp.Inflate(-ne, -ne);
            return tmp.Contains(value.ToPointDF());
        }


        public override void DoStyleCommands(object sender, List<string> Tags, ref bool CloseMenu)
        {
            Rotation = int.Parse(Tags.TagGet("Drehwinkel"));
            var nFixSize = Tags.TagGet("Größe fixiert").FromPlusMinus();
            if (nFixSize != FixSize)
            {
                FixSize = nFixSize;
                ClearInternalRelations();
            }
        }

        public override void GenerateInternalRelation(List<clsPointRelation> relations)
        {


            p_LU.X = p_LO.X;
            p_RO.Y = p_LO.Y;
            p_RU.X = p_RO.X;
            p_RU.Y = p_LU.Y;

            if (FixSize)
            {
                relations.Add(new clsPointRelation(enRelationType.PositionZueinander, p_LO, p_RO));
                relations.Add(new clsPointRelation(enRelationType.PositionZueinander, p_LO, p_RU));
                relations.Add(new clsPointRelation(enRelationType.PositionZueinander, p_LO, p_LU));
            }
            else
            {
                relations.Add(new clsPointRelation(enRelationType.YPositionZueinander, p_LO, p_RU));

                relations.Add(new clsPointRelation(enRelationType.WaagerechtSenkrecht, p_LO, p_RO));
                relations.Add(new clsPointRelation(enRelationType.WaagerechtSenkrecht, p_RU, p_LU));

                relations.Add(new clsPointRelation(enRelationType.WaagerechtSenkrecht, p_LO, p_LU));
                relations.Add(new clsPointRelation(enRelationType.WaagerechtSenkrecht, p_RO, p_RU));
            }
        }

        public override List<FlexiControl> GetStyleOptions(object sender, System.EventArgs e)
        {
            var l = new List<FlexiControl>();

            l.Add(new FlexiControl(true));
            l.Add(new FlexiControl("Drehwinkel", Rotation.ToString(), enDataFormat.Ganzzahl, 1));

            var Relations = ((CreativePad)sender).AllRelations();

            if (!FixSize && !p_LO.CanMove(Relations) && !p_RU.CanMove(Relations))
            {
                l.Add(new FlexiControl(true));
                l.Add(new FlexiControl("Objekt fest definiert,<br>Größe kann nicht fixiert werden"));
            }
            else
            {
                l.Add(new FlexiControl("Größe fixiert", FixSize));
                l.Add(new FlexiControl("Skalieren", enImageCode.Formel));
            }


            return l;
        }

        public override List<PointDF> PointList()
        {
            var l = new List<PointDF>();
            l.Add(p_LO);
            l.Add(p_RU);
            l.Add(p_LU);
            l.Add(p_RO);
            return l;
        }

        public override void SetCoordinates(RectangleDF r)
        {
            p_LO.SetTo(r.PointOf(enAlignment.Top_Left));
            p_RU.SetTo(r.PointOf(enAlignment.Bottom_Right));
            RecomputePointAndRelations();
        }

        public override RectangleDF UsedArea()
        {
            if (p_LO == null || p_RU == null) { return new RectangleDF(); }
            return new RectangleDF(Math.Min(p_LO.X, p_RU.X), Math.Min(p_LO.Y, p_RU.Y), Math.Abs(p_RU.X - p_LO.X), Math.Abs(p_RU.Y - p_LO.Y));
        }



        protected override void KeepInternalLogic()
        {
            p_RO.SetTo(p_RU.X, p_LO.Y);
            p_LU.SetTo(p_LO.X, p_RU.Y);
        }


        protected override bool ParseExplicit(KeyValuePair<string, string> pair)
        {
            switch (pair.Key)
            {
                case "fixsize":
                    FixSize = pair.Value.FromPlusMinus();
                    return true;

                case "rotation":
                    Rotation = int.Parse(pair.Value);
                    return true;
            }
            return false;
        }
        public override string ToString()
        {
            var t = base.ToString();
            t = t.Substring(0, t.Length - 1) + ", ";
            if (Rotation != 0) { t = t + "Rotation=" + Rotation + ", "; }
            t = t + "Fixsize=" + FixSize.ToPlusMinus() + ", ";
            return t.Trim(", ") + "}";
        }



    }
}

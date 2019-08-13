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
using BlueControls.Interfaces;
using BlueDatabase;
using BlueDatabase.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;

namespace BlueControls.ItemCollection
{
    public class TextPadItem : BasicPadItem, ICanHaveColumnVariables
    {




        #region  Variablen-Deklarationen 

        internal PointDF p_LO;
        internal PointDF p_RO;
        internal PointDF p_RU;
        internal PointDF p_LU;

        private string _VariableText;
        private string _ReadableText;
        public bool FixSize;
        private enAlignment _Align;
        private enDataFormat _Format = enDataFormat.Text;

        private ExtText etxt;

        public int Rotation;

        //http://www.kurztutorial.info/programme/punkt-mm/rechner.html
        // Dim Ausgleich As Double = mmToPixel(1 / 72 * 25.4, 300)
        public decimal AdditionalScale = 3.07m;


        #endregion


        #region  Event-Deklarationen + Delegaten 

        #endregion


        #region  Construktor + Initialize 
        public TextPadItem()
        {

        }
        public TextPadItem(string vInternalAndReadableText)
        {
            if (vInternalAndReadableText.StartsWith("{")) { Develop.DebugPrint(enFehlerArt.Fehler, "Code zum Parsen übergeben!"); }

            _Internal = vInternalAndReadableText;
            _ReadableText = vInternalAndReadableText;
            _VariableText = _ReadableText;
            if (string.IsNullOrEmpty(_Internal))
            {
                Develop.DebugPrint(enFehlerArt.Fehler, "Interner Name nicht vergeben.");
            }
        }


        public TextPadItem(string vInternalAndReadableText, enDataFormat cFormat)
        {
            _Internal = vInternalAndReadableText;
            _ReadableText = vInternalAndReadableText;
            _VariableText = _ReadableText;
            _Format = cFormat;
            if (string.IsNullOrEmpty(_Internal)) { Develop.DebugPrint(enFehlerArt.Fehler, "Interner Name nicht vergeben."); }
        }




        public TextPadItem(string vInternal, string vReadableText)
        {
            _Internal = vInternal;
            _ReadableText = vReadableText;
            _VariableText = _ReadableText;
            if (string.IsNullOrEmpty(_Internal)) { Develop.DebugPrint(enFehlerArt.Fehler, "Interner Name nicht vergeben."); }
        }


        protected override void Initialize()
        {
            base.Initialize();

            p_LO = new PointDF(this, "LO", 0, 0, false, true, true);
            p_RO = new PointDF(this, "RO", 0, 0);
            p_RU = new PointDF(this, "RU", 0, 0);
            p_LU = new PointDF(this, "LU", 0, 0);

            _ReadableText = string.Empty;
            _VariableText = string.Empty;

            Style = PadStyles.Undefiniert;

            FixSize = false;
            etxt = null;
            _Align = enAlignment.Top_Left;
        }


        #endregion


        #region  Properties 


        public string Text
        {
            get
            {
                return _ReadableText;
            }
            set
            {
                if (value == _ReadableText) { return; }
                _ReadableText = value;
                etxt = null;
                //OnNeedRefresh();
                OnChanged();
            }
        }





        public enAlignment Alignment
        {
            get
            {
                return _Align;
            }
            set
            {
                if (value == _Align) { return; }
                _Align = value;
                etxt = null;
                //OnNeedRefresh();
                OnChanged();
            }
        }


        #endregion


        public override void DesignOrStyleChanged()
        {


            etxt = null;
        }


        protected override bool ParseExplicit(KeyValuePair<string, string> pair)
        {
            switch (pair.Key)
            {
                case "readabletext":
                    _ReadableText = pair.Value.FromNonCritical();
                    _VariableText = _ReadableText;
                    return true;

                case "alignment":
                    _Align = (enAlignment)byte.Parse(pair.Value);
                    return true;

                case "fixsize":
                    FixSize = pair.Value.FromPlusMinus();
                    return true;

                case "rotation":
                    Rotation = int.Parse(pair.Value);
                    return true;

                case "format":
                    _Format = (enDataFormat)int.Parse(pair.Value);
                    return true;

                case "additionalscale":
                    AdditionalScale = decimal.Parse(pair.Value.FromNonCritical());
                    return true;
            }
            return false;
        }


        public override string ToString()
        {
            var t = base.ToString();
            t = t.Substring(0, t.Length - 1) + ", ";

            if (!string.IsNullOrEmpty(_ReadableText)) { t = t + "ReadableText=" + _ReadableText.ToNonCritical() + ", "; }

            if (Rotation != 0) { t = t + "Rotation=" + Rotation + ", "; }

            if (_Format != enDataFormat.Text) { t = t + "Format=" + (int)_Format + ", "; }

            if (_Align != enAlignment.Top_Left) { t = t + "Alignment=" + (int)_Align + ", "; }

            t = t + "Fixsize=" + FixSize.ToPlusMinus() + ", ";
            t = t + "AdditionalScale=" + AdditionalScale.ToString().ToNonCritical();


            return t.Trim(", ") + "}";
        }


        protected override string ClassId()
        {
            return "TEXT";
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
            l.Add(p_LO);
            l.Add(p_RU);
            l.Add(p_LU);
            l.Add(p_RO);
            return l;
        }


        public override RectangleDF UsedArea()
        {

            if (p_LO == null || p_RU == null) { return new RectangleDF(); }


            //if ((int)(p_RU.X - p_LO.X) < 5) { p_RU.X += 5M; }
            //if ((int)(p_RU.Y - p_LO.Y) < 5) { p_RU.Y += 5M; }

            return new RectangleDF(Math.Min(p_LO.X, p_RU.X), Math.Min(p_LO.Y, p_RU.Y), Math.Abs(p_RU.X - p_LO.X), Math.Abs(p_RU.Y - p_LO.Y));

        }

        protected override void DrawExplicit(Graphics GR, Rectangle DCoordinates, decimal cZoom, decimal MoveX, decimal MoveY, enStates vState, Size SizeOfParentControl, bool ForPrinting)
        {

            if (Style == PadStyles.Undefiniert) { return; }

            etxt.Left = DCoordinates.Left;
            etxt.Top = DCoordinates.Top;

            if (!string.IsNullOrEmpty(_ReadableText) || !ForPrinting)
            {
                etxt.Draw(GR, (float)(cZoom * AdditionalScale * Parent.SheetStyleScale));
            }

            if (!ForPrinting)
            {
                GR.DrawRectangle(CreativePad.PenGray, DCoordinates);
            }
        }

        public override void SetCoordinates(RectangleDF r)
        {
            p_LO.SetTo(r.PointOf(enAlignment.Top_Left));
            p_RU.SetTo(r.PointOf(enAlignment.Bottom_Right));
            RecomputePointAndRelations();
        }


        private string ChangeText(string tmpBody)
        {


            var nt = tmpBody;


            do
            {
                var stx = nt.ToUpper().IndexOf("//TS/");
                if (stx < 0) { break; }
                var enx = nt.ToUpper().IndexOf("/E", stx + 4);
                if (enx < 0) { break; }
                var t1 = nt.Substring(stx, enx - stx + 2);

                if (string.IsNullOrEmpty(t1)) { break; }
                if (!t1.Contains("//TS/000")) { break; }

                var l = t1.SplitBy("/");
                if (l.Length < 3) { break; }

                var Nam = "";
                var Vor = "";
                var Nach = "";

                for (var tec = 0; tec <= l.GetUpperBound(0); tec++)
                {

                    if (l[tec].Length > 3)
                    {
                        switch (l[tec].Substring(0, 3))
                        {
                            case "000":
                                Nam = l[tec].Substring(3).FromNonCritical().GenerateSlash();
                                break;

                            case "103":
                                Vor = l[tec].Substring(3).FromNonCritical().GenerateSlash();
                                break;

                            case "104":
                                Nach = l[tec].Substring(3).FromNonCritical().GenerateSlash();
                                break;
                        }
                    }
                }


                var t2 = "<MarkState=2>" + Vor + Nam + Nach + "<MarkState=0>";

                nt = nt.Replace(t1, t2);

            } while (true);




            if (!string.IsNullOrEmpty(nt))
            {
                nt = nt.Replace("//XS/302", "<MarkState=2><ImageCode=Pinsel|16>{<MarkState=0>");
                nt = nt.Replace("/XE", "<MarkState=2>}<MarkState=0>");
            }


            return nt;
        }

        protected override void KeepInternalLogic()
        {

            if (Style != PadStyles.Undefiniert)
            {

                if (etxt == null)
                {

                    if (Parent == null)
                    {
                        Develop.DebugPrint(enFehlerArt.Fehler, "Parent is Nothing, wurde das Obct zu einer Collectin hinzugefügt?");
                    }
                    else
                    {
                        etxt = new ExtText(Style, Parent.SheetStyle);
                    }
                    etxt.Autoumbruch = true;
                    etxt.Ausrichtung = _Align;

                    if (!string.IsNullOrEmpty(_ReadableText))
                    {
                        etxt.HtmlText = ChangeText(_ReadableText);
                    }
                    else
                    {
                        etxt.HtmlText = "{Text}";
                        p_RU.X = Math.Max(p_RU.X, p_LO.X + 120);
                    }
                }


                // da die Font 1:1 berechnet wird, aber bei der Ausgabe evtl. skaliert,
                // muss etxt vorgegaukelt werden, daß der Drawberehich xxx% größer ist

                etxt.MaxWidth = (int)(UsedArea().Width / AdditionalScale / Parent.SheetStyleScale); // CInt(DCoordinates.Width / CSng(cZoom * (ausgleich + 0.05)))
                etxt.MaxHeight = 10000; //CInt(DCoordinates.Height / cZoom)

                p_RU.Y = Math.Max(p_LO.Y + etxt.Height() * AdditionalScale * Parent.SheetStyleScale, p_LO.Y + 10);
                p_RU.X = Math.Max(p_RU.X, p_LO.X + 10m * AdditionalScale * Parent.SheetStyleScale);
            }


            p_RO.SetTo(p_RU.X, p_LO.Y);
            p_LU.SetTo(p_LO.X, p_RU.Y);


            if ((p_RU.Y - p_LO.Y) * 0.2m > p_RU.X - p_LO.X)
            {
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


        public bool ParseVariable(string VariableName, enValueType ValueType, string Value)
        {

            var ot = _ReadableText;
            _ReadableText = Export.ParseVariable(_ReadableText, VariableName, Value, ValueType, enValueType.Text);

            if (ot == _ReadableText)
            {
                return false;
            }

            etxt = null;

            return true;
        }


        public bool ResetVariables()
        {

            if (_VariableText == _ReadableText) { return false; }

            _ReadableText = _VariableText;

            etxt = null;

            return true;
        }


        public bool ParseSpecialCodes()
        {
            var ot = _ReadableText;
            _ReadableText = Export.DoLayoutCode("XS", _ReadableText, null, "XE", false);

            if (ot == _ReadableText) { return false; }
            etxt = null;
            return true;
        }




        public bool RenameColumn(string oldName, ColumnItem cColumnItem)
        {
            _ReadableText = _VariableText;

            var ot = _ReadableText;

            _ReadableText = _ReadableText.Replace("//TS/000" + oldName + "/", "//TS/000" + cColumnItem.Name + "/", RegexOptions.IgnoreCase);
            _ReadableText = _ReadableText.Replace("//TS/001" + oldName + "/", "//TS/001" + cColumnItem.Name + "/", RegexOptions.IgnoreCase);

            if (ot == _ReadableText) { return false; }

            _VariableText = _ReadableText;

            etxt = null;
            return true;
        }





        public override List<FlexiControl> GetStyleOptions(object sender, System.EventArgs e)
        {
            var l = new List<FlexiControl>();

            l.Add(new FlexiControl("Text", _VariableText, enDataFormat.Text, 5));
            l.Add(new FlexiControl("Drehwinkel", Rotation.ToString(), enDataFormat.Ganzzahl, 1));

            l.Add(new FlexiControl("Stil", ((int)Style).ToString(), Skin.GetFonts(Parent.SheetStyle)));




            var Aursicht = new ItemCollectionList();
            Aursicht.Add(new TextListItem(((int)enAlignment.Top_Left).ToString(), "Linksbündig ausrichten", enImageCode.Linksbündig));
            Aursicht.Add(new TextListItem(((int)enAlignment.Top_HorizontalCenter).ToString(), "Zentrieren", enImageCode.Zentrieren));
            Aursicht.Add(new TextListItem(((int)enAlignment.Top_Right).ToString(), "Rechtsbündig ausrichten", enImageCode.Rechtsbündig));
            Aursicht.Sort();
            l.Add(new FlexiControl("Ausrichtung", ((int)_Align).ToString(), Aursicht));
            l.Add(new FlexiControl("Skalierung", AdditionalScale.ToString(), enDataFormat.Gleitkommazahl, 1));


            return l;
        }

        public override void DoStyleCommands(object sender, List<string> Tags, ref bool CloseMenu)
        {



            var txt = Tags.TagGet("text").FromNonCritical();

            if (txt != _VariableText)
            {
                _ReadableText = txt; //DirectCast(sender, TextBox).Text
                _VariableText = txt; //_ReadableText
                etxt = null;
                RecomputePointAndRelations();
            }


            var tmps = (PadStyles)int.Parse(Tags.TagGet("Stil"));

            if (tmps != Style)
            {
                Style = tmps;
                etxt = null;
            }


            var tmpa = (enAlignment)int.Parse(Tags.TagGet("Ausrichtung"));

            if (tmpa != _Align)
            {
                _Align = tmpa;
                etxt = null;
            }


            AdditionalScale = decimal.Parse(Tags.TagGet("Skalierung").FromNonCritical());


        }



    }
}
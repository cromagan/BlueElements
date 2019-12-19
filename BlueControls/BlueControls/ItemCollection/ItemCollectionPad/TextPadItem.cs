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
    public class TextPadItem : FormPadItemRectangle, ICanHaveColumnVariables
    {




        #region  Variablen-Deklarationen 
        private string _VariableText;
        private string _ReadableText;
        private enAlignment _Align;
        private enDataFormat _Format = enDataFormat.Text;

        private ExtText etxt;



        //http://www.kurztutorial.info/programme/punkt-mm/rechner.html
        // Dim Ausgleich As Double = mmToPixel(1 / 72 * 25.4, 300)
        public decimal AdditionalScale = 3.07m;


        #endregion


        #region  Event-Deklarationen + Delegaten 

        #endregion


        #region  Construktor + Initialize 
        public TextPadItem() : this(string.Empty, string.Empty) { }





        public TextPadItem(string internalname, string vReadableText) : base(internalname)
        {
            _ReadableText = vReadableText;
            _VariableText = _ReadableText;

            Style = PadStyles.Undefiniert;

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


        public override bool ParseThis(string tag, string value)
        {
            if (base.ParseThis(tag, value)) { return true; }

            switch (tag)
            {
                case "readabletext":
                    _ReadableText = value.FromNonCritical();
                    _VariableText = _ReadableText;
                    return true;

                case "alignment":
                    _Align = (enAlignment)byte.Parse(value);
                    return true;

                case "format":
                    _Format = (enDataFormat)int.Parse(value);
                    return true;

                case "additionalscale":
                    AdditionalScale = decimal.Parse(value.FromNonCritical());
                    return true;
            }

            return false;
        }


        public override string ToString()
        {
            var t = base.ToString();
            t = t.Substring(0, t.Length - 1) + ", ";
            if (!string.IsNullOrEmpty(_ReadableText)) { t = t + "ReadableText=" + _ReadableText.ToNonCritical() + ", "; }
            if (_Format != enDataFormat.Text) { t = t + "Format=" + (int)_Format + ", "; }
            if (_Align != enAlignment.Top_Left) { t = t + "Alignment=" + (int)_Align + ", "; }
            t = t + "AdditionalScale=" + AdditionalScale.ToString().ToNonCritical() + ", ";
            return t.Trim(", ") + "}";
        }


        protected override string ClassId()
        {
            return "TEXT";
        }



        protected override void DrawExplicit(Graphics GR, RectangleF DCoordinates, decimal cZoom, decimal MoveX, decimal MoveY, enStates vState, Size SizeOfParentControl, bool ForPrinting)
        {

            if (Style == PadStyles.Undefiniert) { return; }



            var trp = DCoordinates.PointOf(enAlignment.Horizontal_Vertical_Center);
            GR.TranslateTransform(trp.X, trp.Y);
            GR.RotateTransform(-Rotation);


            etxt.DrawingPos = new Point((int)(DCoordinates.Left - trp.X), (int)(DCoordinates.Top - trp.Y));
            etxt.DrawingArea = Rectangle.Empty; // new Rectangle(DCoordinates.Left, DCoordinates.Top, DCoordinates.Width, DCoordinates.Height);

            if (!string.IsNullOrEmpty(_ReadableText) || !ForPrinting)
            {
                etxt.Draw(GR, (float)(cZoom * AdditionalScale * Parent.SheetStyleScale));
            }


            GR.TranslateTransform(-trp.X, -trp.Y);
            GR.ResetTransform();


            if (!ForPrinting)
            {
                GR.DrawRectangle(CreativePad.PenGray, DCoordinates);
            }
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
                        Develop.DebugPrint(enFehlerArt.Fehler, "Parent is Nothing, wurde das Objekt zu einer Collection hinzugefügt?");
                    }
                    else
                    {
                        etxt = new ExtText(Style, Parent.SheetStyle);
                    }


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


                //// da die Font 1:1 berechnet wird, aber bei der Ausgabe evtl. skaliert,
                //// muss etxt vorgegaukelt werden, daß der Drawberehich xxx% größer ist

                //etxt.DrawingArea = new Rectangle((int)UsedArea().Left, (int)UsedArea().Top, (int)(UsedArea().Width / AdditionalScale / Parent.SheetStyleScale), -1);
                //etxt.LineBreakWidth = etxt.DrawingArea.Width;
                etxt.TextDimensions = new Size((int)(UsedArea().Width / AdditionalScale / Parent.SheetStyleScale), -1);
                etxt.Ausrichtung = _Align;

                p_RU.Y = Math.Max(p_LO.Y + etxt.Height() * AdditionalScale * Parent.SheetStyleScale, p_LO.Y + 10);
                p_RU.X = Math.Max(p_RU.X, p_LO.X + 10m * AdditionalScale * Parent.SheetStyleScale);
            }


            base.KeepInternalLogic();
        }





        public bool ParseVariable(string VariableName, enValueType ValueType, string Value)
        {

            var ot = _ReadableText;
            _ReadableText = Export.ParseVariable(_ReadableText, VariableName, Value, ValueType, enValueType.Text);

            if (ot == _ReadableText) { return false; }

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


            l.Add(new FlexiControl("Stil", ((int)Style).ToString(), Skin.GetFonts(Parent.SheetStyle)));




            var Aursicht = new ItemCollectionList();
            Aursicht.Add(new TextListItem(((int)enAlignment.Top_Left).ToString(), "Linksbündig ausrichten", enImageCode.Linksbündig));
            Aursicht.Add(new TextListItem(((int)enAlignment.Top_HorizontalCenter).ToString(), "Zentrieren", enImageCode.Zentrieren));
            Aursicht.Add(new TextListItem(((int)enAlignment.Top_Right).ToString(), "Rechtsbündig ausrichten", enImageCode.Rechtsbündig));
            Aursicht.Sort();
            l.Add(new FlexiControl("Ausrichtung", ((int)_Align).ToString(), Aursicht));
            l.Add(new FlexiControl("Skalierung", AdditionalScale.ToString(), enDataFormat.Gleitkommazahl, 1));


            l.AddRange(base.GetStyleOptions(sender, e));

            return l;
        }

        public override void DoStyleCommands(object sender, List<string> Tags, ref bool CloseMenu)
        {

            base.DoStyleCommands(sender, Tags, ref CloseMenu);

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
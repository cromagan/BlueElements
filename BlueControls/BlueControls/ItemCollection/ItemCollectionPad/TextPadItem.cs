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

        [PropertyAttributes("Text der angezeigt werden soll.<br>Kann Variablen aus dem Code-Generator enthalten.", true)]
        public string Interner_Text
        {
            get
            {
                return _VariableText;
            }
            set
            {
                if (value == _VariableText) { return; }
                _VariableText = value;
                Text = value;
                MakeNewETxt();
                RecalculateAndOnChanged();
            }
        }

        private enDataFormat Format { get; set; } = enDataFormat.Text;

        private ExtText etxt;
        private string _VariableText;
        private enAlignment _ausrichtung;



        //http://www.kurztutorial.info/programme/punkt-mm/rechner.html
        // Dim Ausgleich As Double = mmToPixel(1 / 72 * 25.4, 300)
        public decimal Skalierung { get; set; } = 3.07m;


        #endregion


        #region  Event-Deklarationen + Delegaten 

        #endregion


        #region  Construktor + Initialize 
        public TextPadItem(ItemCollectionPad parent) : this(parent, string.Empty, string.Empty) { }

        public TextPadItem(ItemCollectionPad parent, string internalname, string readableText) : base(parent, internalname)
        {
            Text = readableText;
            _VariableText = readableText;

            Stil = PadStyles.Undefiniert;

            _ausrichtung = enAlignment.Top_Left;
            MakeNewETxt();

        }

        #endregion


        #region  Properties 

        public string Text
        {
            get; private set;
            //set
            //{
            //    if (value == _ReadableText) { return; }
            //    _ReadableText = value;
            //    etxt = null;
            //    OnChanged();
            //}
        }

        public enAlignment Ausrichtung
        {
            get
            {
                return _ausrichtung;
            }
            set
            {
                if (value == _ausrichtung) { return; }
                _ausrichtung = value;
                MakeNewETxt();
                OnChanged();
            }
        }


        #endregion


        public override void DesignOrStyleChanged()
        {
            MakeNewETxt();
        }


        public override bool ParseThis(string tag, string value)
        {
            if (base.ParseThis(tag, value)) { return true; }

            switch (tag)
            {
                case "readabletext":
                    Text = value.FromNonCritical();
                    _VariableText = Text;
                    return true;

                case "alignment":
                    _ausrichtung = (enAlignment)byte.Parse(value);
                    return true;

                case "format":
                    Format = (enDataFormat)int.Parse(value);
                    return true;

                case "additionalscale":
                    Skalierung = decimal.Parse(value.FromNonCritical());
                    return true;
            }

            return false;
        }


        public override string ToString()
        {
            var t = base.ToString();
            t = t.Substring(0, t.Length - 1) + ", ";
            if (!string.IsNullOrEmpty(Text)) { t = t + "ReadableText=" + Text.ToNonCritical() + ", "; }
            if (Format != enDataFormat.Text) { t = t + "Format=" + (int)Format + ", "; }
            if (_ausrichtung != enAlignment.Top_Left) { t = t + "Alignment=" + (int)_ausrichtung + ", "; }
            t = t + "AdditionalScale=" + Skalierung.ToString().ToNonCritical() + ", ";
            return t.Trim(", ") + "}";
        }


        protected override string ClassId()
        {
            return "TEXT";
        }



        protected override void DrawExplicit(Graphics GR, RectangleF DCoordinates, decimal cZoom, decimal MoveX, decimal MoveY, enStates vState, Size SizeOfParentControl, bool ForPrinting)
        {

            if (Stil == PadStyles.Undefiniert) { return; }



            var trp = DCoordinates.PointOf(enAlignment.Horizontal_Vertical_Center);
            GR.TranslateTransform(trp.X, trp.Y);
            GR.RotateTransform(-Drehwinkel);


            if (etxt != null)
            {
                etxt.DrawingPos = new Point((int)(DCoordinates.Left - trp.X), (int)(DCoordinates.Top - trp.Y));
                etxt.DrawingArea = Rectangle.Empty; // new Rectangle(DCoordinates.Left, DCoordinates.Top, DCoordinates.Width, DCoordinates.Height);

                if (!string.IsNullOrEmpty(Text) || !ForPrinting)
                {
                    etxt.Draw(GR, (float)(cZoom * Skalierung * Parent.SheetStyleScale));
                }
            }


            GR.TranslateTransform(-trp.X, -trp.Y);
            GR.ResetTransform();


            base.DrawExplicit(GR, DCoordinates, cZoom, MoveX, MoveY, vState, SizeOfParentControl, ForPrinting);
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

        private void MakeNewETxt()
        {
            etxt = null;

            if (Stil != PadStyles.Undefiniert)
            {

                if (Parent == null)
                {
                    Develop.DebugPrint(enFehlerArt.Fehler, "Parent is Nothing, wurde das Objekt zu einer Collection hinzugefügt?");
                }
                else
                {
                    etxt = new ExtText(Stil, Parent.SheetStyle);
                }


                if (!string.IsNullOrEmpty(Text))
                {
                    etxt.HtmlText = ChangeText(Text);
                }
                else
                {
                    etxt.HtmlText = "{Text}";
                }


                //// da die Font 1:1 berechnet wird, aber bei der Ausgabe evtl. skaliert,
                //// muss etxt vorgegaukelt werden, daß der Drawberehich xxx% größer ist

                //etxt.DrawingArea = new Rectangle((int)UsedArea().Left, (int)UsedArea().Top, (int)(UsedArea().Width / AdditionalScale / Parent.SheetStyleScale), -1);
                //etxt.LineBreakWidth = etxt.DrawingArea.Width;
                etxt.TextDimensions = new Size((int)(UsedArea().Width / Skalierung / Parent.SheetStyleScale), -1);
                etxt.Ausrichtung = _ausrichtung;

            }

        }

        public override void CaluclatePointsWORelations()
        {

            if (etxt == null || etxt.Height() < 8)
            {
                p_RU.Y = Math.Max(p_LO.Y + 8 * Skalierung * Parent.SheetStyleScale, p_LO.Y + 10);
            }
            else
            {
                p_RU.Y = Math.Max(p_LO.Y + etxt.Height() * Skalierung * Parent.SheetStyleScale, p_LO.Y + 10);
            }


            p_RU.X = Math.Max(p_RU.X, p_LO.X + 10m * Skalierung * Parent.SheetStyleScale);

            base.CaluclatePointsWORelations();
        }


        public bool ReplaceVariable(string VariableName, enValueType ValueType, string Value)
        {

            var ot = Text;
            Text = Export.ParseVariable(Text, VariableName, Value, ValueType, enValueType.Text);

            if (ot == Text) { return false; }

            MakeNewETxt();
            RecalculateAndOnChanged();

            return true;
        }


        public bool ResetVariables()
        {
            if (_VariableText == Text) { return false; }
            Text = _VariableText;
            MakeNewETxt();
            RecalculateAndOnChanged();

            return true;
        }


        public bool DoSpecialCodes()
        {
            var ot = Text;
            Text = Export.DoLayoutCode("XS", Text, null, "XE", false);

            if (ot == Text) { return false; }
            MakeNewETxt();
            RecalculateAndOnChanged();

            return true;
        }

        public bool RenameColumn(string oldName, ColumnItem cColumnItem)
        {
            var ot = _VariableText;
            _VariableText = _VariableText.Replace("//TS/000" + oldName + "/", "//TS/000" + cColumnItem.Name + "/", RegexOptions.IgnoreCase);
            _VariableText = _VariableText.Replace("//TS/001" + oldName + "/", "//TS/001" + cColumnItem.Name + "/", RegexOptions.IgnoreCase);

            if (ot == _VariableText) { return false; }

            Text = _VariableText;
            MakeNewETxt();
            RecalculateAndOnChanged();
            return true;
        }


        public override List<FlexiControl> GetStyleOptions()
        {
            var l = new List<FlexiControl>
            {
                new FlexiControlForProperty(this, "Interner-Text", 5)
            };


            var Aursicht = new ItemCollectionList();
            Aursicht.Add(((int)enAlignment.Top_Left).ToString(), "Linksbündig ausrichten", enImageCode.Linksbündig);
            Aursicht.Add(((int)enAlignment.Top_HorizontalCenter).ToString(), "Zentrieren", enImageCode.Zentrieren);
            Aursicht.Add(((int)enAlignment.Top_Right).ToString(), "Rechtsbündig ausrichten", enImageCode.Rechtsbündig);
            Aursicht.Sort();

            l.Add(new FlexiControlForProperty(this, "Ausrichtung", Aursicht));
            l.Add(new FlexiControlForProperty(this, "Skalierung"));


            AddStyleOption(l);

            l.AddRange(base.GetStyleOptions());

            return l;
        }

        protected override void ParseFinished()
        {
            MakeNewETxt();
        }

        //public override void DoStyleCommands(object sender, List<string> Tags, ref bool CloseMenu)
        //{

        //    base.DoStyleCommands(sender, Tags, ref CloseMenu);

        //    var txt = Tags.TagGet("text").FromNonCritical();

        //    if (txt != _VariableText)
        //    {
        //        _ReadableText = txt; //DirectCast(sender, TextBox).Text
        //        _VariableText = txt; //_ReadableText
        //        etxt = null;
        //        RecomputePointAndRelations();
        //    }


        //    var tmps = (PadStyles)int.Parse(Tags.TagGet("Stil"));

        //    if (tmps != Stil)
        //    {
        //        Stil = tmps;
        //        etxt = null;
        //    }


        //    var tmpa = (enAlignment)int.Parse(Tags.TagGet("Ausrichtung"));

        //    if (tmpa != _Align)
        //    {
        //        _Align = tmpa;
        //        etxt = null;
        //    }


        //    AdditionalScale = decimal.Parse(Tags.TagGet("Skalierung").FromNonCritical());


        //}



    }
}
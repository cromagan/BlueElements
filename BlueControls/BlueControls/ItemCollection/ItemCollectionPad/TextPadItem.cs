// Authors:
// Christian Peter
//
// Copyright (c) 2021 Christian Peter
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

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueScript;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace BlueControls.ItemCollection {

    public class TextPadItem : FormPadItemRectangle, ICanHaveColumnVariables {

        #region Fields

        private enAlignment _ausrichtung;

        /// <summary>
        /// Der Original-Text. Bei änderungen deses Textes wird die Variable _text_replaced ebenfalls zurückgesetzt.
        /// </summary>
        private string _text_original;

        /// <summary>
        /// Kopie von _text_original - aber mit evtl. ersetzten Variablen
        /// </summary>
        private string _text_replaced;

        /// <summary>
        /// Dieses Element ist nur temporär und ist der tatsächlich angezeigte Text - mit Bildern, verschieden Größen, etc.
        /// Wird immer von _text_replaced abgeleitet.
        /// </summary>
        private ExtText _txt;

        #endregion

        #region Constructors

        public TextPadItem(ItemCollectionPad parent) : this(parent, string.Empty, string.Empty) { }

        public TextPadItem(ItemCollectionPad parent, string internalname, string readableText) : base(parent, internalname) {
            _text_replaced = readableText;
            _text_original = readableText;
            Stil = PadStyles.Undefiniert;
            _ausrichtung = enAlignment.Top_Left;
            InvalidateText();
        }

        #endregion

        #region Properties

        public enAlignment Ausrichtung {
            get => _ausrichtung;
            set {
                if (value == _ausrichtung) { return; }
                _ausrichtung = value;
                InvalidateText();
                OnChanged();
            }
        }

        //http://www.kurztutorial.info/programme/punkt-mm/rechner.html
        // Dim Ausgleich As Double = mmToPixel(1 / 72 * 25.4, 300)
        public double Skalierung { get; set; } = 3.07d;

        /// <summary>
        ///
        /// </summary>
        [Description("Text der angezeigt werden soll.<br>Alternativ kann ein (oder mehrere) Variablenname im Format ~Name~ angegeben werden.")]
        public string Text {
            get => _text_original;
            set {
                if (value == _text_original) { return; }
                _text_original = value;
                _text_replaced = value;
                InvalidateText();
                SizeChanged();
                OnChanged();
            }
        }

        private enDataFormat Format { get; set; } = enDataFormat.Text;

        #endregion

        #region Methods

        public override void DesignOrStyleChanged() => InvalidateText();

        public override List<FlexiControl> GetStyleOptions() {
            List<FlexiControl> l = new()
            {
                new FlexiControlForProperty(this, "Text", 5)
            };
            ItemCollectionList Aursicht = new()
            {
                { "Linksbündig ausrichten", ((int)enAlignment.Top_Left).ToString(), enImageCode.Linksbündig },
                { "Zentrieren", ((int)enAlignment.Top_HorizontalCenter).ToString(), enImageCode.Zentrieren },
                { "Rechtsbündig ausrichten", ((int)enAlignment.Top_Right).ToString(), enImageCode.Rechtsbündig }
            };
            Aursicht.Sort();
            l.Add(new FlexiControlForProperty(this, "Ausrichtung", Aursicht));
            l.Add(new FlexiControlForProperty(this, "Skalierung"));
            AddStyleOption(l);
            l.AddRange(base.GetStyleOptions());
            return l;
        }

        public override bool ParseThis(string tag, string value) {
            if (base.ParseThis(tag, value)) { return true; }
            switch (tag) {
                case "readabletext":
                    _text_replaced = value.FromNonCritical();
                    _text_original = _text_replaced;
                    return true;

                case "alignment":
                    _ausrichtung = (enAlignment)byte.Parse(value);
                    return true;

                case "format":
                    Format = (enDataFormat)int.Parse(value);
                    return true;

                case "additionalscale":
                    Skalierung = double.Parse(value.FromNonCritical());
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Löst die angegebene Variable in _text_replaced auf, falls diese (noch) vorhanden ist.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="variable"></param>
        /// <returns></returns>
        public bool ReplaceVariable(Script s, BlueScript.Variable variable) {
            if (!_text_replaced.ToLower().Contains("~" + variable.Name.ToLower() + "~")) { return false; }

            if (variable.Type is not Skript.Enums.enVariableDataType.String and
                                 not Skript.Enums.enVariableDataType.List and
                                 not Skript.Enums.enVariableDataType.Integer and
                                 not Skript.Enums.enVariableDataType.Bool and
                                 not Skript.Enums.enVariableDataType.Numeral) { return false; }

            var nt = _text_replaced.Replace("~" + variable.Name + "~", variable.ValueString, System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            if (nt is string txt) {
                if (txt == _text_replaced) { return false; }
                _text_replaced = txt;
                InvalidateText();
                OnChanged();
                return true;
            } else {
                return false;
            }
        }

        public bool ResetVariables() {
            if (_text_original == _text_replaced) { return false; }
            _text_replaced = _text_original;
            InvalidateText();
            OnChanged();
            return true;
        }

        public override void SizeChanged() {
            base.SizeChanged();
            InvalidateText();
        }

        public override string ToString() {
            var t = base.ToString();
            t = t.Substring(0, t.Length - 1) + ", ";
            if (!string.IsNullOrEmpty(_text_original)) { t = t + "ReadableText=" + _text_original.ToNonCritical() + ", "; }
            if (Format != enDataFormat.Text) { t = t + "Format=" + (int)Format + ", "; }
            if (_ausrichtung != enAlignment.Top_Left) { t = t + "Alignment=" + (int)_ausrichtung + ", "; }
            t = t + "AdditionalScale=" + Skalierung.ToString().ToNonCritical() + ", ";
            return t.Trim(", ") + "}";
        }

        protected override string ClassId() => "TEXT";

        protected override void DrawExplicit(Graphics gr, RectangleF drawingCoordinates, double zoom, double shiftX, double shiftY, enStates state, Size sizeOfParentControl, bool forPrinting) {
            if (Stil == PadStyles.Undefiniert) { return; }
            gr.SetClip(drawingCoordinates);
            var trp = drawingCoordinates.PointOf(enAlignment.Horizontal_Vertical_Center);
            gr.TranslateTransform(trp.X, trp.Y);
            gr.RotateTransform(-Drehwinkel);

            if (_txt == null) { MakeNewETxt(); }

            if (_txt != null) {
                _txt.DrawingPos = new Point((int)(drawingCoordinates.Left - trp.X), (int)(drawingCoordinates.Top - trp.Y));
                _txt.DrawingArea = Rectangle.Empty; // new Rectangle(drawingCoordinates.Left, drawingCoordinates.Top, drawingCoordinates.Width, drawingCoordinates.Height);
                if (!string.IsNullOrEmpty(_text_replaced) || !forPrinting) {
                    _txt.Draw(gr, (float)(zoom * Skalierung * Parent.SheetStyleScale));
                }
            }
            gr.TranslateTransform(-trp.X, -trp.Y);
            gr.ResetTransform();
            gr.ResetClip();
            base.DrawExplicit(gr, drawingCoordinates, zoom, shiftX, shiftY, state, sizeOfParentControl, forPrinting);
        }

        protected override void ParseFinished() => InvalidateText();

        private void InvalidateText() => _txt = null;

        private void MakeNewETxt() {
            _txt = null;
            if (Stil != PadStyles.Undefiniert) {
                if (Parent == null) {
                    Develop.DebugPrint(enFehlerArt.Fehler, "Parent is Nothing, wurde das Objekt zu einer Collection hinzugefügt?");
                } else {
                    _txt = new ExtText(Stil, Parent.SheetStyle);
                }
                _txt.HtmlText = !string.IsNullOrEmpty(_text_replaced) ? _text_replaced : "{Text}";
                //// da die Font 1:1 berechnet wird, aber bei der Ausgabe evtl. skaliert,
                //// muss etxt vorgegaukelt werden, daß der Drawberehich xxx% größer ist
                //etxt.DrawingArea = new Rectangle((int)UsedArea().Left, (int)UsedArea().Top, (int)(UsedArea().Width / AdditionalScale / Parent.SheetStyleScale), -1);
                //etxt.LineBreakWidth = etxt.DrawingArea.Width;
                _txt.TextDimensions = new Size((int)(UsedArea().Width / Skalierung / Parent.SheetStyleScale), -1);
                _txt.Ausrichtung = _ausrichtung;
            }
        }

        #endregion
    }
}
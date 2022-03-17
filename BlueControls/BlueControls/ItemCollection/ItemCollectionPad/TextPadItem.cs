﻿// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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
using BlueControls.Extended_Text;
using BlueControls.Interfaces;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using BlueScript.Variables;
using static BlueBasics.Converter;

namespace BlueControls.ItemCollection {

    public class TextPadItem : RectanglePadItem, ICanHaveColumnVariables {

        #region Fields

        private enAlignment _ausrichtung;

        /// <summary>
        /// Der Original-Text. Bei änderungen deses Textes wird die Variable _text_replaced ebenfalls zurückgesetzt.
        /// </summary>
        private string _textOriginal;

        /// <summary>
        /// Kopie von _text_original - aber mit evtl. ersetzten Variablen
        /// </summary>
        private string _textReplaced;

        /// <summary>
        /// Dieses Element ist nur temporär und ist der tatsächlich angezeigte Text - mit Bildern, verschieden Größen, etc.
        /// Wird immer von _text_replaced abgeleitet.
        /// </summary>
        private ExtText? _txt;

        #endregion

        #region Constructors

        public TextPadItem() : this(string.Empty, string.Empty) { }

        public TextPadItem(string internalname, string readableText) : base(internalname) {
            _textReplaced = readableText;
            _textOriginal = readableText;
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
        // Dim Ausgleich As float = MmToPixel(1 / 72 * 25.4, 300)
        public float Skalierung { get; set; } = 3.07f;

        /// <summary>
        ///
        /// </summary>
        [Description("Text der angezeigt werden soll.<br>Alternativ kann ein (oder mehrere) Variablenname im Format ~Name~ angegeben werden.")]
        public string Text {
            get => _textOriginal;
            set {
                if (value == _textOriginal) { return; }
                _textOriginal = value;
                _textReplaced = value;
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
            ItemCollectionList.ItemCollectionList aursicht = new()
            {
                { "Linksbündig ausrichten", ((int)enAlignment.Top_Left).ToString(), enImageCode.Linksbündig },
                { "Zentrieren", ((int)enAlignment.Top_HorizontalCenter).ToString(), enImageCode.Zentrieren },
                { "Rechtsbündig ausrichten", ((int)enAlignment.Top_Right).ToString(), enImageCode.Rechtsbündig }
            };
            aursicht.Sort();
            l.Add(new FlexiControlForProperty(this, "Ausrichtung", aursicht));
            l.Add(new FlexiControlForProperty(this, "Skalierung"));
            AddStyleOption(l);
            l.AddRange(base.GetStyleOptions());
            return l;
        }

        public override bool ParseThis(string tag, string value) {
            if (base.ParseThis(tag, value)) { return true; }
            switch (tag) {
                case "readabletext":
                    _textReplaced = value.FromNonCritical();
                    _textOriginal = _textReplaced;
                    return true;

                case "alignment":
                    _ausrichtung = (enAlignment)byte.Parse(value);
                    return true;

                case "format":
                    Format = (enDataFormat)IntParse(value);
                    return true;

                case "additionalscale":
                    Skalierung = FloatParse(value.FromNonCritical());
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
        public bool ReplaceVariable(Variable variable) {
            var nt = variable.ReplaceInText(_textReplaced);

            if (nt is string txt) {
                if (txt == _textReplaced) { return false; }
                _textReplaced = txt;
                InvalidateText();
                OnChanged();
                return true;
            }

            return false;
        }

        public bool ResetVariables() {
            if (_textOriginal == _textReplaced) { return false; }
            _textReplaced = _textOriginal;
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
            if (!string.IsNullOrEmpty(_textOriginal)) { t = t + "ReadableText=" + _textOriginal.ToNonCritical() + ", "; }
            if (Format != enDataFormat.Text) { t = t + "Format=" + (int)Format + ", "; }
            if (_ausrichtung != enAlignment.Top_Left) { t = t + "Alignment=" + (int)_ausrichtung + ", "; }
            t = t + "AdditionalScale=" + Skalierung.ToString(CultureInfo.InvariantCulture).ToNonCritical() + ", ";
            return t.Trim(", ") + "}";
        }

        protected override string ClassId() => "TEXT";

        protected override void DrawExplicit(Graphics gr, RectangleF drawingCoordinates, float zoom, float shiftX, float shiftY, bool forPrinting) {
            if (Stil == PadStyles.Undefiniert) { return; }
            gr.SetClip(drawingCoordinates);
            var trp = drawingCoordinates.PointOf(enAlignment.Horizontal_Vertical_Center);
            gr.TranslateTransform(trp.X, trp.Y);
            gr.RotateTransform(-Drehwinkel);

            if (_txt == null) { MakeNewETxt(); }

            if (_txt != null) {
                _txt.DrawingPos = new Point((int)(drawingCoordinates.Left - trp.X), (int)(drawingCoordinates.Top - trp.Y));
                _txt.DrawingArea = Rectangle.Empty; // new Rectangle(drawingCoordinates.Left, drawingCoordinates.Top, drawingCoordinates.Width, drawingCoordinates.Height);
                if (!string.IsNullOrEmpty(_textReplaced) || !forPrinting) {
                    _txt.Draw(gr, zoom * Skalierung * Parent.SheetStyleScale);
                }
            }
            gr.TranslateTransform(-trp.X, -trp.Y);
            gr.ResetTransform();
            gr.ResetClip();
            base.DrawExplicit(gr, drawingCoordinates, zoom, shiftX, shiftY, forPrinting);
        }

        protected override void ParseFinished() {
            base.ParseFinished();
            InvalidateText();
        }

        private void InvalidateText() => _txt = null;

        private void MakeNewETxt() {
            _txt = null;
            if (Stil != PadStyles.Undefiniert) {
                if (Parent == null) {
                    Develop.DebugPrint(enFehlerArt.Fehler, "Parent is Nothing, wurde das Objekt zu einer Collection hinzugefügt?");
                } else {
                    _txt = new ExtText(Stil, Parent.SheetStyle);
                }
                _txt.HtmlText = !string.IsNullOrEmpty(_textReplaced) ? _textReplaced : "{Text}";
                //// da die Font 1:1 berechnet wird, aber bei der Ausgabe evtl. skaliert,
                //// muss etxt vorgegaukelt werden, daß der Drawberehich xxx% größer ist
                //etxt.DrawingArea = new Rectangle((int)UsedArea().Left, (int)UsedArea().Top, (int)(UsedArea().Width / AdditionalScale / Parent.SheetStyleScale), -1);
                //etxt.LineBreakWidth = etxt.DrawingArea.Width;
                _txt.TextDimensions = new Size((int)(UsedArea.Width / Skalierung / Parent.SheetStyleScale), -1);
                _txt.Ausrichtung = _ausrichtung;
            }
        }

        #endregion
    }
}
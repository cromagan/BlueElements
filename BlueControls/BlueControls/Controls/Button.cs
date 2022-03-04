// Authors:
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
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.Extended_Text;
using BlueControls.Interfaces;
using BlueDatabase.Interfaces;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;

namespace BlueControls.Controls {

    [Designer(typeof(ButtonDesigner))]
    [DefaultEvent("Click")]
    public sealed class Button : GenericControl, IBackgroundNone, ITranslateable {

        #region Fields

        private readonly ExtText _etxt;

        private enButtonStyle _buttonStyle = enButtonStyle.Button;

        private bool _checked;

        /// <summary>
        /// Verhindert, dass ein Timer und vom System generierter Click ausgelöst wird.
        /// </summary>
        private bool _clickFired;

        /// <summary>
        /// Timer-Objekt und Variablen für Dauer-Feuer-Buttonn
        /// </summary>
        private Timer? _clickFirerer;

        private string _imageCode = string.Empty;

        private string _imageCodeChecked = string.Empty;

        private bool _isFireing;

        private string _text = string.Empty;

        #endregion

        #region Constructors

        public Button() : base(true, false) => _etxt = new ExtText(enDesign.Button, enStates.Standard);

        #endregion

        #region Events

        public event EventHandler CheckedChanged;

        #endregion

        #region Properties

        [DefaultValue(enButtonStyle.Button)]
        public enButtonStyle ButtonStyle {
            get => _buttonStyle;
            set {
                if (_buttonStyle == value) { return; }
                _buttonStyle = value;

                if (_clickFirerer != null) {
                    _clickFirerer.Enabled = false;
                    _clickFirerer.Tick -= ClickFirerer_Tick;
                    _clickFirerer.Dispose();
                    _clickFirerer = null;
                }

                if (value == enButtonStyle.SliderButton) {
                    if (_clickFirerer != null) { return; }
                    _clickFirerer = new Timer {
                        Enabled = false
                    };
                    _clickFirerer.Tick += ClickFirerer_Tick;
                }

                if (DesignMode) { DisableOtherOptionButtons(); }

                if (Parent is Slider) {
                    SetNotFocusable();
                } else {
                    SetStyle(ControlStyles.Selectable, true);
                }
                CheckType();

                Invalidate();
            }
        }

        [DefaultValue(false)]
        public bool Checked {
            get => _checked && (_buttonStyle.HasFlag(enButtonStyle.Checkbox) || _buttonStyle.HasFlag(enButtonStyle.Optionbox));
            set {
                if (_checked == value) { return; }
                _checked = value;
                DisableOtherOptionButtons();
                Invalidate();
                OnCheckedChanged();
            }
        }

        [Category("Darstellung")]
        [Editor(typeof(QuickPicSelector), typeof(UITypeEditor))]
        [DefaultValue("")]
        public string ImageCode {
            get => _imageCode;
            set {
                if (_imageCode == value) { return; }
                _imageCode = value;
                Invalidate();
            }
        }

        [Category("Darstellung")]
        [DefaultValue("")]
        [Editor(typeof(QuickPicSelector), typeof(UITypeEditor))]
        public string ImageCode_Checked {
            get => _imageCodeChecked;
            set {
                if (_imageCodeChecked == value) { return; }
                _imageCodeChecked = value;
                Invalidate();
            }
        }

        [DefaultValue("")]
        public new string Text {
            get => _text;
            set {
                if (_text == value) { return; }
                _text = value;
                Invalidate();
            }
        }

        [DefaultValue(true)]
        public bool Translate { get; set; } = true;

        #endregion

        #region Methods

        public void CheckType() {
            if (_buttonStyle.HasFlag(enButtonStyle.Text)) { return; }

            var par = ParentType();

            if (par is enPartentType.RibbonGroupBox or enPartentType.RibbonPage) {
                if (!_buttonStyle.HasFlag(enButtonStyle.Button_Big)) { _buttonStyle |= enButtonStyle.Button_Big; }
                if (!_buttonStyle.HasFlag(enButtonStyle.Borderless)) { _buttonStyle |= enButtonStyle.Borderless; }
            }

            if (par is enPartentType.ComboBox) { _buttonStyle = enButtonStyle.ComboBoxButton; }
            if (par is enPartentType.RibbonBarCombobox) { _buttonStyle = enButtonStyle.ComboBoxButton_Borderless; }
            if (par is enPartentType.Slider) { _buttonStyle = enButtonStyle.SliderButton; }
        }

        internal static void DrawButton(Control? control, Graphics gr, enDesign buttontype, enStates state, QuickImage? pic, enAlignment align, bool picHeight44, ExtText etxt, string text, Rectangle displayRectangle, bool translate) {
            var design = Skin.DesignOf(buttontype, state);
            Skin.Draw_Back(gr, design, displayRectangle, control, true);
            Skin.Draw_Border(gr, design, displayRectangle);

            picHeight44 = picHeight44 && control.Height >= 40 && pic != null;

            if (picHeight44) {
                // Großes Bild per automatik generieren und Zeichnen
                //if (pic.Width != -1 || pic.Height != -1) { Develop.DebugPrint("Bei Bildcode " + pic + " die Größenangabe entfernen, da es ein grosses Bild wird!"); }
                //var Zoom = Math.Min((control.Width - 6) / (double)pic.Width, 28 / (double)pic.Height);
                var tmpPic = QuickImage.Get(QuickImage.GenerateCode(pic.Name, control.Width - 6, 28, pic.Effekt, pic.Färbung, pic.ChangeGreenTo, pic.Sättigung, pic.Helligkeit, pic.DrehWinkel, pic.Transparenz, pic.Zweitsymbol));
                Skin.Draw_FormatedText(gr, string.Empty, tmpPic, design, enAlignment.Horizontal_Vertical_Center, new Rectangle(0, 0, control.Width, 44), control, false, translate);

                var tt = BlueDatabase.LanguageTool.DoTranslate(text, translate);

                if (!string.IsNullOrWhiteSpace(tt)) {
                    // Mehrzeiligen Text generieren und Zeichnen
                    etxt.Design = buttontype;
                    etxt.State = state; // Fall es nicht nothing ist
                    etxt.Zeilenabstand = 0.65f;
                    etxt.DrawingPos = new Point(0, 43);
                    etxt.TextDimensions = new Size(displayRectangle.Width - (Skin.PaddingSmal / 2), 22);
                    etxt.Ausrichtung = enAlignment.Horizontal_Vertical_Center;
                    etxt.HtmlText = tt;
                    etxt.Draw(gr, 1);
                }
            } else if (buttontype is not enDesign.OptionButton_TextStyle and not enDesign.CheckBox_TextStyle) {
                Skin.Draw_FormatedText(gr, text, pic, design, align, displayRectangle, control, false, translate);
            } else {
                var tt = "<ImageCode=" + design.Image + "> <zbx_store><top>" + BlueDatabase.LanguageTool.DoTranslate(text, translate);
                //etxt = new ExtText(buttontype, state);
                etxt.Design = buttontype;
                etxt.State = state;
                etxt.TextDimensions = displayRectangle.Size;
                etxt.HtmlText = tt;
                etxt.Draw(gr, 1);
            }
        }

        internal static Size StandardSize(string text, QuickImage? image) {
            var s = Skin.FormatedText_NeededSize(text, image, Skin.GetBlueFont(enDesign.Button_CheckBox, enStates.Standard), 16);
            s.Width += 10;
            s.Height += 4;
            return s;
        }

        protected override void DrawControl(Graphics gr, enStates state) {
            try {
                var pic = (enButtonStyle)((int)_buttonStyle % 1000) switch {
                    enButtonStyle.Yes_or_No => _checked || MousePressing() ? QuickImage.Get(enImageCode.Häkchen) : QuickImage.Get(enImageCode.Kreuz),
                    enButtonStyle.Pic1_or_Pic2 => _checked || MousePressing() ? QuickImage.Get(_imageCodeChecked) : QuickImage.Get(_imageCode),
                    _ => string.IsNullOrEmpty(ImageCode) ? null : QuickImage.Get(_imageCode)
                };

                #region State modifzieren

                if (_isFireing) { state = enStates.Standard_Disabled; } // Optische anzeige

                if (_buttonStyle.HasFlag(enButtonStyle.Checkbox) || _buttonStyle.HasFlag(enButtonStyle.Optionbox)) {
                    if (_checked || MousePressing()) { state |= enStates.Checked; }
                }

                #endregion State modifzieren

                switch (_buttonStyle) {
                    case enButtonStyle.Text:
                    case enButtonStyle.Borderless:
                    case enButtonStyle.Button:
                        DrawButton(this, gr, enDesign.Button, state, pic, enAlignment.Horizontal_Vertical_Center, false, _etxt, _text, DisplayRectangle, Translate);
                        break;

                    case enButtonStyle.Button_Big_Borderless:
                        DrawButton(this, gr, enDesign.Ribbonbar_Button, state, pic, enAlignment.VerticalCenter_Left, true, _etxt, _text, DisplayRectangle, Translate);
                        break;

                    case enButtonStyle.Button_Big:
                        DrawButton(this, gr, enDesign.Button, state, pic, enAlignment.VerticalCenter_Left, true, _etxt, _text, DisplayRectangle, Translate);
                        break;

                    case enButtonStyle.SliderButton:
                        DrawButton(this, gr, enDesign.Button_SliderDesign, state, pic, enAlignment.Horizontal_Vertical_Center, false, _etxt, _text, DisplayRectangle, Translate);
                        break;

                    case enButtonStyle.Yes_or_No:
                    case enButtonStyle.Pic1_or_Pic2:
                    case enButtonStyle.Checkbox:
                        DrawButton(this, gr, enDesign.Button_CheckBox, state, pic, enAlignment.Horizontal_Vertical_Center, false, _etxt, _text, DisplayRectangle, Translate);
                        break;

                    case enButtonStyle.Checkbox_Big_Borderless:
                        DrawButton(this, gr, enDesign.Ribbonbar_Button_CheckBox, state, pic, enAlignment.VerticalCenter_Left, true, _etxt, _text, DisplayRectangle, Translate);
                        break;

                    case enButtonStyle.Checkbox_Text:
                        DrawButton(this, gr, enDesign.CheckBox_TextStyle, state, pic, enAlignment.Horizontal_Vertical_Center, false, _etxt, _text, DisplayRectangle, Translate);
                        break;

                    case enButtonStyle.Optionbox:
                        DrawButton(this, gr, enDesign.Button_OptionButton, state, pic, enAlignment.Horizontal_Vertical_Center, false, _etxt, _text, DisplayRectangle, Translate);
                        break;

                    case enButtonStyle.Optionbox_Big_Borderless:
                        DrawButton(this, gr, enDesign.Ribbonbar_Button_OptionButton, state, pic, enAlignment.VerticalCenter_Left, true, _etxt, _text, DisplayRectangle, Translate);
                        break;

                    case enButtonStyle.Optionbox_Text:
                        DrawButton(this, gr, enDesign.OptionButton_TextStyle, state, pic, enAlignment.Horizontal_Vertical_Center, false, _etxt, _text, DisplayRectangle, Translate);
                        break;

                    case enButtonStyle.ComboBoxButton:
                        DrawButton(this, gr, enDesign.Button_ComboBox, state, pic, enAlignment.Horizontal_Vertical_Center, false, _etxt, _text, DisplayRectangle, Translate);
                        break;

                    case enButtonStyle.ComboBoxButton_Borderless:
                        DrawButton(this, gr, enDesign.Ribbonbar_Button_Combobox, state, pic, enAlignment.Horizontal_Vertical_Center, false, _etxt, _text, DisplayRectangle, Translate);
                        break;

                    default:
                        Develop.DebugPrint_MissingCommand(_buttonStyle.ToString());
                        break;
                }
            } catch { }
        }

        protected override void OnClick(System.EventArgs e) {
            // Click wird vor dem MouseUpEreigniss ausgelöst
            if (_isFireing || IsDisposed || !Enabled || _clickFired) { return; }

            _clickFired = true;
            _isFireing = true;
            Refresh();

            if (_buttonStyle.HasFlag(enButtonStyle.Checkbox)) {
                Checked = !Checked;
            }
            if (_buttonStyle.HasFlag(enButtonStyle.Optionbox)) {
                if (!_checked) {
                    Checked = true;
                    DisableOtherOptionButtons();
                }
            }

            base.OnClick(e);
            _isFireing = false;
            Invalidate();
        }

        protected override void OnCreateControl() {
            base.OnCreateControl();
            CheckType();
        }

        protected override void OnLocationChanged(System.EventArgs e) {
            base.OnLocationChanged(e);

            CheckType();

            if (DesignMode) { DisableOtherOptionButtons(); }
        }

        protected override void OnMouseDown(MouseEventArgs e) {
            _clickFired = false;

            base.OnMouseDown(e);

            if (!Enabled || IsDisposed) { return; }

            if (_buttonStyle == enButtonStyle.SliderButton) {
                _clickFirerer.Interval = 500;
                ClickFirerer_Tick(null, e);
            }

            Invalidate();
        }

        protected override void OnMouseEnter(System.EventArgs e) {
            base.OnMouseEnter(e);
            if (!Enabled) { return; }
            Invalidate();
        }

        protected override void OnMouseLeave(System.EventArgs e) {
            base.OnMouseLeave(e);
            if (!Enabled) { return; }
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e) {
            if (_clickFirerer != null) {
                _clickFirerer.Enabled = false;
                _clickFirerer.Interval = 500;
            }

            base.OnMouseUp(e);
            if (!Enabled) { return; }
            Invalidate();
        }

        private void ClickFirerer_Tick(object sender, System.EventArgs e) {
            if (_buttonStyle.HasFlag(enButtonStyle.SliderButton) && MousePressing() && ContainsMouse()) {
                // Focus egal, DauerFeuerbutton - Slider - Design kann keinen Focus erhalten!
                _clickFired = false;
                OnClick(e);
                if (sender != null) { _clickFirerer.Interval = 100; }
                _clickFirerer.Enabled = true;
            } else {
                _clickFirerer.Enabled = false;
            }
        }

        private void DisableOtherOptionButtons() {
            if (!_checked || !_buttonStyle.HasFlag(enButtonStyle.Optionbox) || Parent == null) { return; }

            if (string.IsNullOrEmpty(Name)) { return; }

            foreach (var thisControl in Parent.Controls) {
                if (thisControl is Button tmpButton) {
                    if (tmpButton.ButtonStyle.HasFlag(enButtonStyle.Optionbox) && tmpButton != this && tmpButton.Checked) { tmpButton.Checked = false; }
                }
            }
        }

        private void OnCheckedChanged() => CheckedChanged?.Invoke(this, System.EventArgs.Empty);

        #endregion
    }
}
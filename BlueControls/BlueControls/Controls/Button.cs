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
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueDatabase;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;

namespace BlueControls.Controls {
    [Designer(typeof(ButtonDesigner))]
    [DefaultEvent("Click")]
    public sealed class Button : GenericControl, IBackgroundNone {

        #region Constructor
        public Button() : base(true, false) { }
        #endregion




        #region  Variablen 
        private string _Text = "";
        private enButtonStyle _ButtonStyle = enButtonStyle.Button;
        private bool _Checked;
        private readonly ExtText etxt = null;

        private string _ImageCode = "";
        private string _ImageCode_Checked = "";
        private QuickImage _Pic;

        /// <summary>
        /// Verhindert, dass ein Timer und vom System generierter Click ausgelöst wird.
        /// </summary>
        private bool _ClickFired;

        // Timer-Objekt und Variablen für Dauer-Feuer-Buttonn
        private System.Windows.Forms.Timer _ClickFirerer;
        private const int _ShortIntervall = 100;
        private const int _FirstIntervall = 500;
        private bool _IsFireing;






        #endregion

        #region  Properties 







        [Category("Darstellung")]
        [Editor(typeof(QuickPicSelector), typeof(UITypeEditor))]
        [DefaultValue("")]
        public string ImageCode {
            get => _ImageCode;
            set {
                if (_ImageCode == value) { return; }
                _ImageCode = value;
                GetPic();
                Invalidate();
            }
        }

        [Category("Darstellung")]
        [DefaultValue("")]
        [Editor(typeof(QuickPicSelector), typeof(UITypeEditor))]
        public string ImageCode_Checked {
            get => _ImageCode_Checked;
            set {
                if (_ImageCode_Checked == value) { return; }
                _ImageCode_Checked = value;
                GetPic();
                Invalidate();
            }
        }

        [DefaultValue("")]
        public new string Text {
            get => _Text;

            set {
                if (_Text == value) { return; }
                _Text = value;
                Invalidate();
            }
        }

        [DefaultValue(enButtonStyle.Button)]
        public enButtonStyle ButtonStyle {
            get => _ButtonStyle;
            set {
                if (_ButtonStyle == value) { return; }
                _ButtonStyle = value;


                if (_ClickFirerer != null) {
                    _ClickFirerer.Enabled = false;
                    _ClickFirerer.Tick -= _ClickFirerer_Tick;
                    _ClickFirerer.Dispose();
                    _ClickFirerer = null;
                }


                if (value == enButtonStyle.SliderButton) {
                    if (_ClickFirerer != null) { return; }
                    _ClickFirerer = new System.Windows.Forms.Timer {
                        Enabled = false
                    };
                    _ClickFirerer.Tick += _ClickFirerer_Tick;
                }

                tmpSkinRow = null;

                if (DesignMode) { DisableOtherOptionButtons(); }

                if (Parent is Slider) {
                    SetNotFocusable();
                } else {
                    SetStyle(System.Windows.Forms.ControlStyles.Selectable, true);
                }
                GetPic();
                Invalidate();
            }
        }

        [DefaultValue(false)]
        public bool Checked {
            get {
                if (_ButtonStyle == enButtonStyle.Button || _ButtonStyle == enButtonStyle.SliderButton) { return false; }
                return _Checked;
            }
            set {

                if (_Checked == value) { return; }
                tmpSkinRow = null;
                _Checked = value;
                DisableOtherOptionButtons();
                GetPic();
                Invalidate();

                OnCheckedChanged();

            }
        }




        #endregion

        #region  Event-Deklarationen 



        public event EventHandler CheckedChanged;

        #endregion

        #region  Form-Ereignisse 


        private void OnCheckedChanged() {
            CheckedChanged?.Invoke(this, System.EventArgs.Empty);
        }

        protected override void OnMouseLeave(System.EventArgs e) {
            base.OnMouseLeave(e);
            if (!Enabled) { return; }
            Invalidate();
        }

        protected override void OnMouseEnter(System.EventArgs e) {

            base.OnMouseEnter(e);
            if (!Enabled) { return; }
            Invalidate();

        }
        protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e) {

            if (_ClickFirerer != null) {
                _ClickFirerer.Enabled = false;
                _ClickFirerer.Interval = _FirstIntervall;
            }

            base.OnMouseUp(e);

            if (!Enabled) { return; }
            GetPic();
            Invalidate();
        }

        protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e) {
            _ClickFired = false;
            base.OnMouseDown(e);
            if (!Enabled) { return; }
            if (IsDisposed) { return; }

            switch ((enButtonStyle)((int)_ButtonStyle % 1000)) {
                case enButtonStyle.Button:
                case enButtonStyle.Checkbox:
                case enButtonStyle.Optionbox:
                    break;

                case enButtonStyle.Yes_or_No:
                case enButtonStyle.Pic1_or_Pic2:
                    GetPic();
                    break;

                case enButtonStyle.SliderButton:
                    _ClickFirerer.Interval = _FirstIntervall;
                    _ClickFirerer_Tick(null, e);
                    break;

                default:
                    Develop.DebugPrint(_ButtonStyle);
                    break;
            }

            Invalidate();


        }


        #endregion


        private void DisableOtherOptionButtons() {
            if (_ButtonStyle != enButtonStyle.Optionbox && _ButtonStyle != enButtonStyle.Optionbox_Text && _ButtonStyle != enButtonStyle.Optionbox_RibbonBar) { return; }

            if (!_Checked) { return; }
            if (Parent == null) { return; }
            if (string.IsNullOrEmpty(Name)) { return; }


            foreach (var CLT in Parent.Controls) {
                if (CLT is Button tempVar) {
                    if (tempVar.ButtonStyle == _ButtonStyle && tempVar != this && tempVar.Checked) { tempVar.Checked = false; }

                }
            }


        }
        private void _ClickFirerer_Tick(object sender, System.EventArgs e) {

            var ok = _ButtonStyle == enButtonStyle.SliderButton;
            if (!MousePressing()) { ok = false; }
            if (!ContainsMouse()) { ok = false; }

            // Focus egal, DauerFeuerbutton - Slider - Design kann keinen Focus erhalten!



            if (ok) {
                _ClickFired = false;
                OnClick(e);
                if (sender != null) { _ClickFirerer.Interval = _ShortIntervall; }
                _ClickFirerer.Enabled = true;

            } else {
                _ClickFirerer.Enabled = false;
            }

        }

        protected override void OnClick(System.EventArgs e) {
            // Click wird vor dem MouseUpEreigniss ausgelöst
            if (_IsFireing) { return; }
            if (IsDisposed) { return; }
            if (!Enabled) { return; }
            if (_ClickFired) { return; }

            _ClickFired = true;
            _IsFireing = true;


            switch ((enButtonStyle)((int)_ButtonStyle % 1000)) {
                case enButtonStyle.Button:
                    break;

                case enButtonStyle.Checkbox:
                case enButtonStyle.Yes_or_No:
                case enButtonStyle.Pic1_or_Pic2:
                    Checked = !Checked;
                    break;

                case enButtonStyle.Optionbox:
                    if (!_Checked) {
                        Checked = true;
                        DisableOtherOptionButtons();
                    }
                    break;

                case enButtonStyle.SliderButton:
                    // Bei Dauerfeuerbutten bei Mouseup kein Klick ereigniss
                    // Grund: Weil sonst immer mindesten 2 Klicks ausgeführt werden: MouseDown und MouseUp
                    break;

                default:
                    Develop.DebugPrint(_ButtonStyle);
                    break;

            }

            base.OnClick(e);

            _IsFireing = false;
        }


        internal static void DrawButton(System.Windows.Forms.Control con, Graphics GR, ref RowItem _SkinRow, enDesign vButtonTypex, enStates vStatex, QuickImage p, enAlignment Align, bool PicHeight44, ExtText etxt, string vtext, Rectangle DisplayRectangle, bool Translate) {

            if (_SkinRow == null) { _SkinRow = Skin.SkinRow(vButtonTypex, vStatex); }


            Skin.Draw_Back(GR, _SkinRow, DisplayRectangle, con, true);
            Skin.Draw_Border(GR, _SkinRow, DisplayRectangle);


            if (PicHeight44 && con.Height < 40) { PicHeight44 = false; }
            if (PicHeight44 && p == null) { PicHeight44 = false; }

            if (PicHeight44) {

                if (p.Width != -1 || p.Height != -1) { Develop.DebugPrint("Bei Bildcode " + p + " die Größenangabe entfernen, da es ein grosses Bild wird!"); }



                var Zoom = Math.Min((con.Width - 6) / (double)p.BMP.Width, 28 / (double)p.BMP.Height);
                var p2 = QuickImage.Get(QuickImage.GenerateCode(p.Name, (int)(p.BMP.Width * Zoom), (int)(p.BMP.Height * Zoom), p.Effekt, p.Färbung, p.ChangeGreenTo, p.Sättigung, p.Helligkeit, p.DrehWinkel, p.Transparenz, p.Zweitsymbol));
                Skin.Draw_FormatedText(GR, "", p2, _SkinRow, vStatex, enAlignment.Horizontal_Vertical_Center, new Rectangle(0, 0, con.Width, 44), con, false, Translate);

                if (etxt == null) { etxt = new ExtText(vButtonTypex, vStatex, _SkinRow); }
                etxt.State = vStatex; // Fall es nicht nothing ist
                etxt.Zeilenabstand = 0.65;
                etxt.DrawingPos = new Point(0, 43);
                //etxt.DrawingArea = DisplayRectangle;
                etxt.TextDimensions = new Size(DisplayRectangle.Width - Skin.PaddingSmal / 2, 22);
                etxt.Ausrichtung = enAlignment.Horizontal_Vertical_Center;
                etxt.HtmlText = BlueDatabase.LanguageTool.DoTranslate(vtext, Translate);
                etxt.Draw(GR, 1);

            } else if (vButtonTypex != enDesign.OptionButton_TextStyle && vButtonTypex != enDesign.CheckBox_TextStyle) {
                Skin.Draw_FormatedText(GR, vtext, p, _SkinRow, vStatex, Align, DisplayRectangle, con, false, Translate);
            } else {


                var tt = "<ImageCode=" + Skin.PicCode(_SkinRow) + "> <zbx_store><top>" + BlueDatabase.LanguageTool.DoTranslate(vtext, Translate); //Skin.ZusatzTextAdder(vText, vButtonType, vState)

                if (etxt == null) { etxt = new ExtText(vButtonTypex, vStatex, _SkinRow); }
                etxt.State = vStatex;
                etxt.TextDimensions = DisplayRectangle.Size;
                //etxt.MaxHeight = DisplayRectangle.Height;
                //etxt.MaxWidth = DisplayRectangle.Width;
                etxt.HtmlText = tt;
                etxt.Draw(GR, 1);

            }
        }



        private void GetPic() {

            switch ((enButtonStyle)((int)_ButtonStyle % 1000)) {

                case enButtonStyle.Yes_or_No:
                    if (_Checked || MousePressing()) {
                        _Pic = QuickImage.Get(enImageCode.Häkchen);
                    } else {
                        _Pic = QuickImage.Get(enImageCode.Kreuz);
                    }
                    break;

                case enButtonStyle.Pic1_or_Pic2:
                    if (_Checked || MousePressing()) {
                        _Pic = QuickImage.Get(_ImageCode_Checked);
                    } else {
                        _Pic = QuickImage.Get(_ImageCode);
                    }
                    break;

                default:
                    if (string.IsNullOrEmpty(ImageCode)) {
                        _Pic = null;
                    } else {
                        _Pic = QuickImage.Get(_ImageCode);
                    }
                    break;
            }
        }


        protected override void DrawControl(Graphics gr, enStates state) {

            try {
                var PicHeight44 = false;
                var Par = ParentType();
                var DesignToolbar = false;
                var DesignText = false;

                if (_ButtonStyle != enButtonStyle.Button && _ButtonStyle != enButtonStyle.Button_RibbonBar && _ButtonStyle != enButtonStyle.SliderButton) {
                    if (_Checked || MousePressing()) { state |= enStates.Checked; }
                }


                // Groß machen?
                if (Par == enPartentType.RibbonPage || Par == enPartentType.RibbonGroupBox) {
                    DesignToolbar = true;
                    PicHeight44 = true;
                }


                if ((int)_ButtonStyle > 1000 && (int)_ButtonStyle < 2000) { PicHeight44 = true; }
                if ((int)_ButtonStyle > 2000) {
                    DesignText = true;
                    DesignToolbar = false;
                    PicHeight44 = false;
                }


                switch ((enButtonStyle)((int)_ButtonStyle % 1000)) {
                    case enButtonStyle.Button:
                    case enButtonStyle.SliderButton:
                        switch (Par) {
                            case enPartentType.RibbonPage:
                            case enPartentType.RibbonGroupBox:
                                DrawButton(this, gr, ref tmpSkinRow, enDesign.Ribbonbar_Button, state, _Pic, enAlignment.VerticalCenter_Left, PicHeight44, etxt, _Text, DisplayRectangle, Translate);
                                break;
                            case enPartentType.Slider:
                                DrawButton(this, gr, ref tmpSkinRow, enDesign.Button_SliderDesign, state, _Pic, enAlignment.Horizontal_Vertical_Center, PicHeight44, etxt, _Text, DisplayRectangle, Translate);
                                break;
                            case enPartentType.ComboBox:
                                DrawButton(this, gr, ref tmpSkinRow, enDesign.Button_ComboBox, state, _Pic, enAlignment.Horizontal_Vertical_Center, PicHeight44, etxt, _Text, DisplayRectangle, Translate);
                                break;
                            case enPartentType.RibbonBarCombobox:
                                DrawButton(this, gr, ref tmpSkinRow, enDesign.Ribbonbar_Button_Combobox, state, _Pic, enAlignment.Horizontal_Vertical_Center, PicHeight44, etxt, _Text, DisplayRectangle, Translate);
                                break;
                            default:
                                DrawButton(this, gr, ref tmpSkinRow, enDesign.Button, state, _Pic, enAlignment.Horizontal_Vertical_Center, PicHeight44, etxt, _Text, DisplayRectangle, Translate);
                                break;
                        }

                        break;
                    case enButtonStyle.Optionbox:
                        if (DesignToolbar) {
                            DrawButton(this, gr, ref tmpSkinRow, enDesign.Ribbonbar_Button_OptionButton, state, _Pic, enAlignment.VerticalCenter_Left, PicHeight44, etxt, _Text, DisplayRectangle, Translate);
                        } else if (DesignText) {
                            DrawButton(this, gr, ref tmpSkinRow, enDesign.OptionButton_TextStyle, state, _Pic, enAlignment.Horizontal_Vertical_Center, PicHeight44, etxt, _Text, DisplayRectangle, Translate);
                        } else {
                            DrawButton(this, gr, ref tmpSkinRow, enDesign.Button_OptionButton, state, _Pic, enAlignment.Horizontal_Vertical_Center, PicHeight44, etxt, _Text, DisplayRectangle, Translate);
                        }
                        break;

                    case enButtonStyle.Checkbox:
                        if (DesignToolbar) {
                            DrawButton(this, gr, ref tmpSkinRow, enDesign.Ribbonbar_Button_CheckBox, state, _Pic, enAlignment.VerticalCenter_Left, PicHeight44, etxt, _Text, DisplayRectangle, Translate);
                        } else if (DesignText) {
                            DrawButton(this, gr, ref tmpSkinRow, enDesign.CheckBox_TextStyle, state, _Pic, enAlignment.Horizontal_Vertical_Center, PicHeight44, etxt, _Text, DisplayRectangle, Translate);
                        } else {
                            DrawButton(this, gr, ref tmpSkinRow, enDesign.Button_CheckBox, state, _Pic, enAlignment.Horizontal_Vertical_Center, PicHeight44, etxt, _Text, DisplayRectangle, Translate);
                        }
                        break;

                    case enButtonStyle.Yes_or_No:
                        if (DesignToolbar) {
                            DrawButton(this, gr, ref tmpSkinRow, enDesign.Ribbonbar_Button_CheckBox, state, _Pic, enAlignment.VerticalCenter_Left, PicHeight44, etxt, _Text, DisplayRectangle, Translate);
                        } else {
                            DrawButton(this, gr, ref tmpSkinRow, enDesign.Button_CheckBox, state, _Pic, enAlignment.Horizontal_Vertical_Center, PicHeight44, etxt, _Text, DisplayRectangle, Translate);
                        }
                        break;

                    case enButtonStyle.Pic1_or_Pic2:
                        if (DesignToolbar) {
                            DrawButton(this, gr, ref tmpSkinRow, enDesign.Ribbonbar_Button_CheckBox, state, _Pic, enAlignment.VerticalCenter_Left, PicHeight44, etxt, _Text, DisplayRectangle, Translate);
                        } else {
                            DrawButton(this, gr, ref tmpSkinRow, enDesign.Button_CheckBox, state, _Pic, enAlignment.Horizontal_Vertical_Center, PicHeight44, etxt, _Text, DisplayRectangle, Translate);
                        }
                        break;

                    default:
                        Develop.DebugPrint("Button-Design nicht definiert!");
                        break;
                }

            } catch {
            }


        }



        protected override void OnLocationChanged(System.EventArgs e) {

            base.OnLocationChanged(e);
            if (DesignMode) { DisableOtherOptionButtons(); }
        }


        protected override void OnSizeChanged(System.EventArgs e) {
            base.OnSizeChanged(e);
            if (DesignMode) { DisableOtherOptionButtons(); }
        }

        internal static Size StandardSize(string text, QuickImage image) {
            var s = Skin.FormatedText_NeededSize(text, image, Skin.GetBlueFont(enDesign.Button_CheckBox, enStates.Standard), 16);

            s.Width += 10;
            s.Height += 4;
            return s;
        }
    }
}

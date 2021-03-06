﻿// Authors:
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
using BlueControls.Enums;
using System;
using System.Drawing;
using System.Drawing.Text;

namespace BlueControls {

    public class ExtChar {

        #region Fields

        public const char StoreX = (char)5;

        //  public const int ImagesStart = 5000;
        public const char Top = (char)4;

        public PointF Pos = PointF.Empty;
        private readonly char _Char;
        private enDesign _Design = enDesign.Undefiniert;
        private SizeF _Size = SizeF.Empty;
        private enStates _State = enStates.Undefiniert;
        private int _Stufe = 4;

        #endregion

        #region Constructors

        internal ExtChar(char charcode, enDesign cDesign, enStates cState, BlueFont cFont, int Stufe, enMarkState cMarkState) {
            _Design = cDesign;
            _Char = charcode;
            Font = cFont;
            _Stufe = Stufe;
            _State = cState;
            Marking = cMarkState;
        }

        #endregion

        #region Properties

        public int Char => _Char;

        public enDesign Design {
            get => _Design;
            set {
                if (value == _Design) { return; }
                ChangeState(value, _State, _Stufe);
            }
        }

        public enMarkState Marking { get; set; }

        public SizeF Size {
            get {
                if (!_Size.IsEmpty) { return _Size; }
                _Size = Font == null ? new SizeF(0, 16) : _Char < 0 ? Font.CharSize(0f) : Font.CharSize(_Char);
                return _Size;
            }
        }

        public enStates State {
            get => _State;
            set {
                if (value == _State) { return; }
                ChangeState(_Design, value, _Stufe);
            }
        }

        public int Stufe {
            get => _Stufe;
            set {
                if (_Stufe == value) { return; }
                ChangeState(_Design, _State, value);
            }
        }

        internal BlueFont Font { get; private set; } = null;

        #endregion

        #region Methods

        public void Draw(Graphics GR, Point PosModificator, float czoom) {
            if (_Char < 20) { return; }
            var DrawX = (Pos.X * czoom) + PosModificator.X;
            var DrawY = (Pos.Y * czoom) + PosModificator.Y;
            Font f = null;
            if (Font != null) { f = Font.FontWithoutLines(czoom); }
            if (Font == null) { return; }
            var IsCap = false;
            if (_Char < (int)enASCIIKey.ImageStart) {
                GR.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                var c = _Char;
                if (Font.Kapitälchen && c != char.ToUpper(c)) {
                    IsCap = true;
                    f = Font.FontWithoutLinesForCapitals(czoom);
                    c = char.ToUpper(c);
                } else if (Font.OnlyUpper) {
                    c = char.ToUpper(c);
                } else if (Font.OnlyLower) {
                    c = char.ToLower(c);
                }
                if (Font.Underline) {
                    GR.DrawLine(Font.Pen(czoom), DrawX, (int)(DrawY + Font.Oberlänge(czoom) + ((Font.Pen(1f).Width + 1) * czoom) + 0.5), DrawX + ((1 + Size.Width) * czoom), (int)(DrawY + Font.Oberlänge(czoom) + ((Font.Pen(1f).Width + 1) * czoom) + 0.5));
                }
                if (IsCap) {
                    DrawY += Font.KapitälchenPlus(czoom);
                }
                try {
                    if (Font.Outline) {
                        for (var PX = -1; PX <= 1; PX++) {
                            for (var PY = -1; PY <= 1; PY++) {
                                GR.DrawString(c.ToString(), f, Font.Brush_Color_Outline, DrawX + PX, DrawY + PY, StringFormat.GenericTypographic);
                            }
                        }
                    }
                    if (IsCap) {
                        GR.DrawString(c.ToString(), f, Font.Brush_Color_Main, DrawX + (0.3F * czoom), DrawY, StringFormat.GenericTypographic);
                    }
                    GR.DrawString(c.ToString(), f, Font.Brush_Color_Main, DrawX, DrawY, StringFormat.GenericTypographic);
                    if (Font.StrikeOut) {
                        GR.DrawLine(Font.Pen(czoom), DrawX - 1, (int)(DrawY + (Size.Height * 0.55)), (int)(DrawX + 1 + Size.Width), (int)(DrawY + (Size.Height * 0.55)));
                    }
                    if (_Size.Width < 1) {
                        GR.DrawLine(new Pen(Color.Red), DrawX + 1, DrawY - 4, DrawX + 1, DrawY + 16);
                    }
                } catch (Exception) {
                    //Develop.DebugPrint(Ex);
                }
                return;
            }
            {
                // Sind es KEINE Integer bei DrawX / DrawY, kommt es zu extrem unschönen Effekten. Gerade Linien scheinen verschwommen zu sein. (Checkbox-Kästchen)
                if (Math.Abs(czoom - 1) < 0.001) {
                    var BNR = QuickImage.Get(_Char - (int)enASCIIKey.ImageStart);
                    if (BNR == null) { return; }
                    GR.DrawImage(BNR.BMP, (int)DrawX, (int)DrawY);
                } else {
                    var l = QuickImage.Get(_Char - (int)enASCIIKey.ImageStart);
                    if (l == null || l.Width == 0) { l = QuickImage.Get("Warnung|16"); }
                    if (l.Width > 0) {
                        GR.DrawImage(QuickImage.Get(l.Name, (int)(l.Width * czoom)).BMP, (int)DrawX, (int)DrawY);
                    }
                }
            }
        }

        public bool isLineBreak() => (int)_Char switch {
            11 or 13 or Top => true,
            _ => false,
        };

        public bool isPossibleLineBreak() => _Char.isPossibleLineBreak();

        public bool isSpace() => (int)_Char switch {
            32 or 0 or 9 => true,
            _ => false,
        };

        /// <summary>
        ///
        /// </summary>
        /// <param name="zoom"></param>
        /// <param name="drawingPos">Muss bereits Skaliert sein</param>
        /// <returns></returns>
        public bool IsVisible(float zoom, Point drawingPos, Rectangle drawingArea) => (drawingArea.Width < 1 && drawingArea.Height < 1)
|| ((drawingArea.Width <= 0 || (Pos.X * zoom) + drawingPos.X <= drawingArea.Right)
&& (drawingArea.Height <= 0 || (Pos.Y * zoom) + drawingPos.Y <= drawingArea.Bottom)
&& ((Pos.X + Size.Width) * zoom) + drawingPos.X >= drawingArea.Left
&& ((Pos.Y + Size.Height) * zoom) + drawingPos.Y >= drawingArea.Top);

        public bool isWordSeperator() => _Char.isWordSeperator();

        public string ToHTML() => (int)_Char switch {
            13 => "<br>",
            //case enEtxtCodes.HorizontalLine:
            //    return "<hr>";
            11 => string.Empty,
            _ => Convert.ToChar(_Char).ToString().CreateHtmlCodes(true),
        };

        private void ChangeState(enDesign vDesign, enStates vState, int vStufe) {
            if (vState == _State && vStufe == _Stufe && vDesign == _Design) { return; }
            _Size = SizeF.Empty;
            _Design = vDesign;
            _State = vState;
            _Stufe = vStufe;
            Font = vDesign == enDesign.Undefiniert || vState == enStates.Undefiniert ? null : Skin.GetBlueFont(vDesign, vState, vStufe);
        }

        #endregion
    }
}
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
using BlueControls.Enums;
using BlueControls.Interfaces;
using System;
using System.Drawing;
using System.Drawing.Text;

namespace BlueControls
{
    public class ExtChar : IDesignAble
    {
        #region  Variablen-Deklarationen 

        public PointF Pos = PointF.Empty;
        private BlueFont _Font = null;
        private readonly char _Char;

        private enDesign _Design = enDesign.Undefiniert;
        private enStates _State = enStates.Undefiniert;
        private SizeF _Size = SizeF.Empty;
        private int _Stufe = 4;


        public const int ImagesStart = 5000;
        public const char Top = (char)4;
        public const char StoreX = (char)5;



        #endregion


        #region  Event-Deklarationen + Delegaten 

        #endregion


        #region  Construktor + Initialize 



        internal ExtChar(char charcode, enDesign cDesign, enStates cState, BlueFont cFont, int Stufe, enMarkState cMarkState)
        {
            _Design = cDesign;
            _Char = charcode;
            _Font = cFont;
            _Stufe = Stufe;
            _State = cState;
            Marking = cMarkState;
        }

        #endregion


        #region  Properties 

        internal BlueFont Font
        {
            get
            {
                return _Font;
            }
        }


        public int Char
        {
            get
            {
                return (int)_Char;
            }
        }



        public SizeF Size
        {
            get
            {
                if (!_Size.IsEmpty) { return _Size; }


                if (Font == null)
                {
                    _Size = new SizeF(0, 16);
                }
                else if (_Char < 0)
                {
                    _Size = _Font.CharSize(0f);
                }
                else
                {
                    _Size = _Font.CharSize(_Char);

                }

                return _Size;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="zoom"></param>
        /// <param name="drawingPos">Muss bereits Skaliert sein</param>
        /// <returns></returns>
        public bool IsVisible(float zoom, Point drawingPos, Rectangle drawingArea)
        {
            if (drawingArea.Width < 1 && drawingArea.Height < 1) { return true; }

            if (drawingArea.Width > 0 && Pos.X * zoom + drawingPos.X > drawingArea.Right) { return false; }
            if (drawingArea.Height > 0 && Pos.Y * zoom + drawingPos.Y > drawingArea.Bottom) { return false; }

            if ((Pos.X + Size.Width) * zoom + drawingPos.X < drawingArea.Left) { return false; }
            if ((Pos.Y + Size.Height) * zoom + drawingPos.Y < drawingArea.Top) { return false; }

            return true;
        }

        public enDesign Design
        {
            get
            {
                return _Design;
            }
            set
            {
                if (value == _Design) { return; }
                ChangeState(value, _State, _Stufe);
            }
        }


        public enStates State
        {
            get
            {
                return _State;
            }
            set
            {
                if (value == _State) { return; }
                ChangeState(_Design, value, _Stufe);
            }
        }


        public enMarkState Marking { get; set; }

        public int Stufe
        {
            get
            {
                return _Stufe;
            }
            set
            {
                if (_Stufe == value) { return; }
                ChangeState(_Design, _State, value);
            }
        }


        #endregion





        private void ChangeState(enDesign vDesign, enStates vState, int vStufe)
        {
            if (vState == _State && vStufe == _Stufe && vDesign == _Design) { return; }

            _Size = SizeF.Empty;
            _Design = vDesign;
            _State = vState;
            _Stufe = vStufe;

            if (vDesign == enDesign.Undefiniert || vState == enStates.Undefiniert)
            {
                _Font = null;
            }
            else
            {
                _Font = Skin.GetBlueFont(vDesign, vState, vStufe);
            }
        }


        public bool isSpace()
        {
            switch ((int)_Char)
            {
                case 32:
                case 0:
                case 9:
                    return true;
                default:
                    return false;
            }
        }

        public bool isPossibleLineBreak()
        {
            return _Char.isPossibleLineBreak();
        }

        public bool isWordSeperator()
        {
            return _Char.isWordSeperator();
        }

        public bool isLineBreak()
        {
            switch ((int)_Char)
            {
                case 11:
                case 13:
                case Top:
                    return true;
                default:
                    return false;
            }
        }



        public void Draw(Graphics GR, Point PosModificator, float czoom)
        {

            if (_Char < 20) { return; }

            var DrawX = Pos.X * czoom + PosModificator.X;
            var DrawY = Pos.Y * czoom + PosModificator.Y;

            Font f = null;

            if (_Font != null) { f = _Font.FontWithoutLines(czoom); }
            if (_Font == null) { return; }

            var IsCap = false;


            if (_Char < ImagesStart)
            {
                GR.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;


                var c = _Char;
                if (_Font.Kapitälchen && c != char.ToUpper(c))
                {
                    IsCap = true;
                    f = _Font.FontWithoutLinesForCapitals(czoom);
                    c = char.ToUpper(c);
                }
                else if (_Font.OnlyUpper)
                {
                    c = char.ToUpper(c);
                }
                else if (_Font.OnlyLower)
                {
                    c = char.ToLower(c);
                }
                if (_Font.Underline)
                {
                    GR.DrawLine(_Font.Pen(czoom), DrawX, (int)(DrawY + _Font.Oberlänge(czoom) + (_Font.Pen(1f).Width + 1) * czoom + 0.5), DrawX + (1 + Size.Width) * czoom, (int)(DrawY + _Font.Oberlänge(czoom) + (_Font.Pen(1f).Width + 1) * czoom + 0.5));
                }

                if (IsCap)
                {
                    DrawY += Font.KapitälchenPlus(czoom);
                }


                try
                {
                    if (_Font.Outline)
                    {
                        for (var PX = -1; PX <= 1; PX++)
                        {
                            for (var PY = -1; PY <= 1; PY++)
                            {
                                GR.DrawString(c.ToString(), f, _Font.Brush_Color_Outline, DrawX + PX, DrawY + PY, StringFormat.GenericTypographic);
                            }
                        }
                    }


                    if (IsCap)
                    {
                        GR.DrawString(c.ToString(), f, _Font.Brush_Color_Main, DrawX + 0.3F * czoom, DrawY, StringFormat.GenericTypographic);
                    }

                    GR.DrawString(c.ToString(), f, _Font.Brush_Color_Main, DrawX, DrawY, StringFormat.GenericTypographic);


                    if (_Font.StrikeOut)
                    {
                        GR.DrawLine(_Font.Pen(czoom), DrawX - 1, (int)(DrawY + Size.Height * 0.55), (int)(DrawX + 1 + Size.Width), (int)(DrawY + Size.Height * 0.55));
                    }


                    if (_Size.Width < 1)
                    {
                        GR.DrawLine(new Pen(Color.Red), DrawX + 1, DrawY - 4, DrawX + 1, DrawY + 16);
                    }


                }
                catch (Exception)
                {
                    //Develop.DebugPrint(Ex);
                }

                return;

            }


            {
                // Sind es KEINE Integer bei DrawX / DrawY, kommt es zu extrem unschönen Effekten. Gerade Linien scheinen verschwommen zu sein. (Checkbox-Kästchen)

                if (Math.Abs(czoom - 1) < 0.001)
                {
                    var BNR = QuickImage.Get((int)_Char - ImagesStart);
                    if (BNR == null) { return; }
                    GR.DrawImage(BNR.BMP, (int)DrawX, (int)DrawY);
                }
                else
                {
                    var l = QuickImage.Get((int)_Char - ImagesStart);

                    if (l == null || l.Width == 0) { l = QuickImage.Get("Warnung|16"); }

                    if (l.Width > 0)
                    {
                        GR.DrawImage(QuickImage.Get(l.Name, (int)(l.Width * czoom)).BMP, (int)DrawX, (int)DrawY);
                    }

                }

            }
        }

        public string ToHTML()
        {

            switch ((int)_Char)
            {
                case 13:
                    return "<br>";
                //case enEtxtCodes.HorizontalLine:
                //    return "<hr>";
                case 11:
                    return string.Empty;
                default:
                    return Convert.ToChar(_Char).ToString().CreateHtmlCodes();
            }
        }
    }
}



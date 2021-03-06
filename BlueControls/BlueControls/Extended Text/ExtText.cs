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
using BlueControls.Enums;
using BlueDatabase;
using System;
using System.Collections.Generic;
using System.Drawing;

// VTextTyp-Hirachie
// ~~~~~~~~~~~~~~~~~
// HTMLText, PlainText = Diese Texte wurden in den Speicher geschrieben und f�hren
//                       -> Folgestatus: ?_Converted
// ?_Converted         = Der Text ist immer noch f�hrend, es wurde aber schon konvertiert
// Chars_Converted     = Der Urspr�ngliche Text wurde verworfen, der Text wird Komplett �ber die Chars gehandlet
//                       Wird nur bei Textbearbeitung (Key Up o. �. aktiviert)
// HTML-Codes:
// B Fett
// I kursiv
// U Unterstrichen
// STRIKE Durchgestrichen
// 3 Outline
// BR Zeilenumbruch
// FontName
// FontColor
// FontOutline
// BackColor
// ImageCode
// ZBX_Store = Zeilenbeginn speichern
// TOP = Y auf 0 zur�cksetzen
// vState = vState Setzen (mit HTML_Code)

namespace BlueControls {

    public sealed class ExtText {

        #region Fields

        public string AllowedChars;
        public enAlignment Ausrichtung;

        /// <summary>
        /// Falls mit einer Skalierung gezeichnet wird, m�ssen die Angaben bereits skaleiert sein.
        /// </summary>
        public Rectangle DrawingArea;

        /// <summary>
        /// Falls mit einer Skalierung gezeichnet wird, m�ssen die Angaben bereits skaleiert sein.
        /// </summary>
        public Point DrawingPos;

        public bool Multiline;
        private enDesign _Design;
        private int? _Height = null;
        private RowItem _Row;
        private enStates _State;
        private Size _TextDimensions;
        private string _TMPHtmlText = string.Empty;
        private string _TMPPlainText = string.Empty;
        private int? _Width = null;
        private float _Zeilenabstand = 1;

        #endregion

        #region Constructors

        public ExtText(enDesign vDesign, enStates state) {
            Initialize();
            _Design = vDesign;
            _State = state;
            _Row = null;
        }

        public ExtText(PadStyles vDesign, RowItem SkinRow) {
            Initialize();
            _Design = (enDesign)vDesign;
            _State = enStates.Standard;
            _Row = SkinRow;
            if ((int)_Design < 10000 || _Row == null) {
                Develop.DebugPrint(enFehlerArt.Fehler, "Fehler!");
            }
        }

        #endregion

        #region Properties

        public List<ExtChar> Chars { get; private set; }

        public enDesign Design {
            get => _Design;
            set {
                if (value == _Design) { return; }
                _Design = value;
                foreach (var ch in Chars) {
                    ch.Design = _Design;
                }
            }
        }

        public string HtmlText {
            get {
                if (string.IsNullOrEmpty(_TMPHtmlText)) {
                    _TMPHtmlText = ConvertCharToHTMLText(0, Chars.Count - 1);
                }
                return _TMPHtmlText;
            }
            set {
                if (HtmlText == value) { return; }
                ConvertTextToChar(value, true);
            }
        }

        public string PlainText {
            get {
                if (string.IsNullOrEmpty(_TMPPlainText)) {
                    _TMPPlainText = ConvertCharToPlainText(0, Chars.Count - 1);
                }
                return _TMPPlainText;
            }
            set {
                if (PlainText == value) { return; }
                ConvertTextToChar(value, false);
            }
        }

        public enStates State {
            get => _State;
            set {
                if (value == _State) { return; }
                _State = value;
                foreach (var ch in Chars) {
                    ch.State = _State;
                }
            }
        }

        /// <summary>
        /// Nach wieviel Pixeln der Zeilenumbruch stattfinden soll. -1 wenn kein Umbruch sein soll. Auch das Alingement richtet sich nach diesen Gr��en.
        /// </summary>
        public Size TextDimensions {
            get => _TextDimensions;
            set {
                if (_TextDimensions.Width == value.Width && _TextDimensions.Height == value.Height) { return; }
                _TextDimensions = value;
                ResetPosition(false);
            }
        }

        public float Zeilenabstand {
            get => _Zeilenabstand;
            set {
                if (value == _Zeilenabstand) { return; }
                _Zeilenabstand = value;
                ResetPosition(false);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Sucht den aktuellen Buchstaben, der unter den angegeben Koordinaten liegt.
        ///     Wird kein Char gefunden, wird der logischste Char gew�hlt. (z.B. Nach ZeilenEnde = Letzzter Buchstabe der Zeile)
        /// </summary>
        /// <remarks></remarks>
        public int Char_Search(double PixX, double PixY) {
            var cZ = -1;
            var XDi = double.MaxValue;
            var YDi = double.MaxValue;
            var XNr = -1;
            var YNr = -1;
            // Passiert dann, wenn w�herend des Klickens der Text ge�ndert wird, z.B. PLZ ->Ort
            //  If Not CBool(_TextTyp And enTextTyp.Converted) Then HTML2Char()
            //  Dim RLC As Integer = ReallyLastChar()
            do {
                cZ++;
                if (cZ > Chars.Count - 1) // Das Ende des Textes
                {
                    break;
                }
                if (Chars[cZ].Char > 0 && Chars[cZ].Size.Width > 0) {
                    var X = Convert.ToBoolean(PixX >= DrawingPos.X + Chars[cZ].Pos.X && PixX <= DrawingPos.X + Chars[cZ].Pos.X + Chars[cZ].Size.Width);
                    var Y = Convert.ToBoolean(PixY >= DrawingPos.Y + Chars[cZ].Pos.Y && PixY <= DrawingPos.Y + Chars[cZ].Pos.Y + Chars[cZ].Size.Height);
                    //If PixX >= Left + vChars(cZ).Pos.X AndAlso PixX <= Left + vChars(cZ).Pos.X + vChars(cZ).Size.Width Then X = True
                    //If PixY >= Top + vChars(cZ).Pos.Y AndAlso PixY <= Top + vChars(cZ).Pos.Y + vChars(cZ).Size.Height Then Y = True
                    if (X && Y) {
                        return cZ;
                    }
                    double TmpDi;
                    if (X == false && Y) {
                        TmpDi = Math.Abs(PixX - (DrawingPos.X + Chars[cZ].Pos.X + (Chars[cZ].Size.Width / 2.0)));
                        if (TmpDi < XDi) {
                            XNr = cZ;
                            XDi = TmpDi;
                        }
                    } else if (X && Y == false) {
                        TmpDi = Math.Abs(PixY - (DrawingPos.Y + Chars[cZ].Pos.Y + (Chars[cZ].Size.Height / 2.0)));
                        if (TmpDi < YDi) {
                            YNr = cZ;
                            YDi = TmpDi;
                        }
                    }
                }
            } while (true);
            //   Dim RN As Integer = cZ
            cZ--;
            return XNr >= 0 ? XNr : YNr >= 0 ? YNr : cZ >= 0 ? cZ : 0;
            //Do
            //    If RN < 1 Then Return 0
            //    If vChars(RN - 1).Char <> 0 Then Return RN
            //    RN -= 1
            //Loop
        }

        public void Check(int Von, int Bis, bool Checkstat) {
            for (var cc = Von; cc <= Bis; cc++) {
                if (Chars[cc].State != enStates.Undefiniert) {
                    if (Checkstat) {
                        if (!Convert.ToBoolean(Chars[cc].State & enStates.Checked)) {
                            Chars[cc].State |= enStates.Checked;
                        }
                    } else {
                        if (Convert.ToBoolean(Chars[cc].State & enStates.Checked)) {
                            Chars[cc].State = Chars[cc].State ^ enStates.Checked;
                        }
                    }
                }
            }
        }

        public Rectangle CursorPixelPosx(int CharPos) {
            while (_Width == null) { ReBreak(); }
            if (CharPos > Chars.Count + 1) { CharPos = Chars.Count + 1; }
            if (Chars.Count > 0 && CharPos < 0) { CharPos = 0; }
            float X = 0;
            float Y = 0;
            float He = 14;
            if (Chars.Count == 0) {
                // Kein Text vorhanden
            } else if (CharPos < Chars.Count) {
                // Cursor vor einem Zeichen
                X = Chars[CharPos].Pos.X;
                Y = Chars[CharPos].Pos.Y;
                He = Chars[CharPos].Size.Height;
            } else if (CharPos > 0 && CharPos < Chars.Count + 1 && Chars[CharPos - 1].Char == 13) {
                // Vorzeichen = Zeilenumbruch
                Y = Chars[CharPos - 1].Pos.Y + Chars[CharPos - 1].Size.Height;
                He = Chars[CharPos - 1].Size.Height;
            } else if (CharPos > 0 && CharPos < Chars.Count + 1) {
                // Vorzeichen = Echtes Char
                X = Chars[CharPos - 1].Pos.X + Chars[CharPos - 1].Size.Width;
                Y = Chars[CharPos - 1].Pos.Y;
                He = Chars[CharPos - 1].Size.Height;
            }
            return new Rectangle((int)X, (int)(Y - 1), 0, (int)(He + 2));
        }

        public void Delete(int Von, int Bis) {
            var tempVar = Bis - Von;
            for (var z = 1; z <= tempVar; z++) {
                if (Von < Chars.Count) {
                    Chars.RemoveAt(Von);
                }
            }
            ResetPosition(true);
        }

        public void Draw(Graphics gr, float zoom) {
            while (_Width == null) { ReBreak(); }
            DrawStates(gr, zoom);
            foreach (var t in Chars) {
                if (t.Char > 0 && t.IsVisible(zoom, DrawingPos, DrawingArea)) { t.Draw(gr, DrawingPos, zoom); }
            }
        }

        public int Height() {
            while (_Width == null) { ReBreak(); }
            return (int)_Height;
        }

        public bool InsertChar(enASCIIKey KeyAscii, int Position) => InsertAnything(KeyAscii, string.Empty, Position);

        public bool InsertImage(string Img, int Position) => InsertAnything(enASCIIKey.Undefined, Img, Position);

        public Size LastSize() {
            while (_Width == null) { ReBreak(); }
            return _Width < 5 || _Height < 5 ? new Size(32, 16) : new Size((int)_Width, (int)_Height);
        }

        public void Stufe�ndern(int Von, int Bis, int stufe) {
            for (var cc = Von; cc <= Bis; cc++) {
                Chars[cc].Stufe = stufe;
            }
            ResetPosition(true);
        }

        public string Substring(int StartIndex, int lenght) => ConvertCharToPlainText(StartIndex, StartIndex + lenght - 1);

        public int Width() {
            while (_Width == null) { ReBreak(); }
            return (int)_Width;
        }

        public string Word(int Pos) {
            var S = WordStart(Pos);
            var E = WordEnd(Pos);
            return Substring(S, E - S);
        }

        internal string ConvertCharToPlainText(int Von, int Bis) {
            try {
                var T = string.Empty;
                var cZ = Von;
                Bis = Math.Min(Bis, Chars.Count - 1);
                while (cZ <= Bis && Chars[cZ].Char > 0) {
                    if (Chars[cZ].Char < (int)enASCIIKey.ImageStart) {
                        T += Convert.ToChar(Chars[cZ].Char).ToString();
                    }
                    cZ++;
                }
                T = T.Replace("\n", "");
                return T;
            } catch {
                // Wenn Chars ge�ndter wird (und dann der Count nimmer stimmt)
                return ConvertCharToPlainText(Von, Bis);
            }
        }

        internal void Mark(enMarkState markstate, int first, int last) {
            try {
                for (var z = first; z <= last; z++) {
                    if (z >= Chars.Count) { return; }
                    if (!Chars[z].Marking.HasFlag(markstate)) {
                        Chars[z].Marking = Chars[z].Marking | markstate;
                    }
                }
            } catch (Exception) {
                Mark(markstate, first, last);
            }
        }

        internal void Unmark(enMarkState markstate) {
            foreach (var t in Chars) {
                if (t.Marking.HasFlag(markstate)) {
                    t.Marking ^= markstate;
                }
            }
        }

        internal int WordEnd(int Pos) {
            if (Chars.Count == 0) { return -1; }
            if (Pos < 0 || Pos >= Chars.Count) { return -1; }
            if (Chars[Pos].isWordSeperator()) { return -1; }
            do {
                Pos++;
                if (Pos >= Chars.Count) { return Chars.Count; }
                if (Chars[Pos].isWordSeperator()) { return Pos; }
            } while (true);
        }

        internal int WordStart(int Pos) {
            if (Chars.Count == 0) { return -1; }
            if (Pos < 0 || Pos >= Chars.Count) { return -1; }
            if (Chars[Pos].isWordSeperator()) { return -1; }
            do {
                Pos--;
                if (Pos < 0) { return 0; }
                if (Chars[Pos].isWordSeperator()) { return Pos + 1; }
            } while (true);
        }

        private string ConvertCharToHTMLText(int Von, int Bis) {
            var T = "";
            var cZ = Von;
            Bis = Math.Min(Bis, Chars.Count - 1);
            var LastStufe = 4;
            while (cZ <= Bis && Chars[cZ].Char > 0) {
                if (Chars[cZ].Char < (int)enASCIIKey.ImageStart) {
                    if (LastStufe != Chars[cZ].Stufe) {
                        T = T + "<H" + Chars[cZ].Stufe + ">";
                        LastStufe = Chars[cZ].Stufe;
                    }
                    T += Chars[cZ].ToHTML();
                } else {
                    var index = Chars[cZ].Char - (int)enASCIIKey.ImageStart;
                    var x = QuickImage.Get(index);
                    if (x != null) { T += "<ImageCode=" + x.Name + ">"; }
                }
                cZ++;
            }
            return T;
        }

        private void ConvertTextToChar(string cactext, bool IsRich) {
            var Pos = 0;
            var Zeichen = -1;
            var Stufe = 4;
            var Markstate = enMarkState.None;
            Chars = new List<ExtChar>();
            ResetPosition(false);
            var BF = (int)_Design > 10000 ? Skin.GetBlueFont((PadStyles)_Design, _Row) : Skin.GetBlueFont(_Design, _State);
            if (BF == null) {
                return;
            }// Wenn die DAtenbanken entladen wurde, bei Programmende
            if (!string.IsNullOrEmpty(cactext)) {
                cactext = IsRich ? cactext.ConvertFromHtmlToRich() : cactext.Replace("\r\n", "\r");
                var Lang = cactext.Length - 1;
                do {
                    if (Pos > Lang) { break; }
                    var CH = cactext[Pos];
                    if (IsRich) {
                        switch (CH) {
                            case '<': {
                                    DoHTMLCode(cactext, Pos, ref Zeichen, ref BF, ref Stufe, ref Markstate);
                                    var OP = 1;
                                    do {
                                        Pos++;
                                        if (Pos > Lang) { break; }
                                        if (cactext[Pos] == '>') { OP--; }
                                        if (cactext[Pos] == '<') { OP++; }
                                        if (OP == 0) { break; }
                                    } while (true);
                                    break;
                                }

                            case '&':
                                DoSpecialEntities(cactext, ref Pos, ref Zeichen, ref BF, ref Stufe, ref Markstate);
                                break;

                            default:
                                // Normales Zeichen
                                Zeichen++;
                                Chars.Add(new ExtChar(CH, _Design, _State, BF, Stufe, Markstate));
                                break;
                        }
                    } else {
                        // Normales Zeichen
                        Zeichen++;
                        Chars.Add(new ExtChar(CH, _Design, _State, BF, Stufe, Markstate));
                    }
                    Pos++;
                } while (true);
            }
            ResetPosition(false);
        }

        private void DoHTMLCode(string HTMLText, int StartPos, ref int Position, ref BlueFont PF, ref int Stufe, ref enMarkState MarkState) {
            if (PF == null) { return; }  // wenn die Datenbanken entladen wurden bei Programmende
            var Endpos = HTMLText.IndexOf('>', StartPos + 1);
            if (Endpos <= StartPos) {
                Develop.DebugPrint("String-Fehler, > erwartet. " + HTMLText);
                return;
            }
            var Oricode = HTMLText.Substring(StartPos + 1, Endpos - StartPos - 1);
            var Istgleich = Oricode.IndexOf('=');
            string Cod, Attribut;
            if (Istgleich < 0) {
                // <H4> wird durch autopr�fung zu <H4 >
                Cod = Oricode.ToUpper().Trim();
                Attribut = string.Empty;
            } else {
                Cod = Oricode.Substring(0, Istgleich).Replace(" ", "").ToUpper().Trim();
                Attribut = Oricode.Substring(Istgleich + 1).Trim('\"');
            }
            switch (Cod) {
                case "B":
                    PF = BlueFont.Get(PF.FontName, PF.FontSize, true, PF.Italic, PF.Underline, PF.StrikeOut, PF.Outline, PF.Color_Main, PF.Color_Outline, PF.Kapit�lchen, PF.OnlyUpper, PF.OnlyLower);
                    break;

                case "/B":
                    PF = BlueFont.Get(PF.FontName, PF.FontSize, false, PF.Italic, PF.Underline, PF.StrikeOut, PF.Outline, PF.Color_Main, PF.Color_Outline, PF.Kapit�lchen, PF.OnlyUpper, PF.OnlyLower);
                    break;

                case "I":
                    PF = BlueFont.Get(PF.FontName, PF.FontSize, PF.Bold, true, PF.Underline, PF.StrikeOut, PF.Outline, PF.Color_Main, PF.Color_Outline, PF.Kapit�lchen, PF.OnlyUpper, PF.OnlyLower);
                    break;

                case "/I":
                    PF = BlueFont.Get(PF.FontName, PF.FontSize, PF.Bold, false, PF.Underline, PF.StrikeOut, PF.Outline, PF.Color_Main, PF.Color_Outline, PF.Kapit�lchen, PF.OnlyUpper, PF.OnlyLower);
                    break;

                case "U":
                    PF = BlueFont.Get(PF.FontName, PF.FontSize, PF.Bold, PF.Italic, true, PF.StrikeOut, PF.Outline, PF.Color_Main, PF.Color_Outline, PF.Kapit�lchen, PF.OnlyUpper, PF.OnlyLower);
                    break;

                case "/U":
                    PF = BlueFont.Get(PF.FontName, PF.FontSize, PF.Bold, PF.Italic, false, PF.StrikeOut, PF.Outline, PF.Color_Main, PF.Color_Outline, PF.Kapit�lchen, PF.OnlyUpper, PF.OnlyLower);
                    break;

                case "STRIKE":
                    PF = BlueFont.Get(PF.FontName, PF.FontSize, PF.Bold, PF.Italic, PF.Underline, true, PF.Outline, PF.Color_Main, PF.Color_Outline, PF.Kapit�lchen, PF.OnlyUpper, PF.OnlyLower);
                    break;

                case "/STRIKE":
                    PF = BlueFont.Get(PF.FontName, PF.FontSize, PF.Bold, PF.Italic, PF.Underline, false, PF.Outline, PF.Color_Main, PF.Color_Outline, PF.Kapit�lchen, PF.OnlyUpper, PF.OnlyLower);
                    break;

                case "3":
                    PF = BlueFont.Get(PF.FontName, PF.FontSize, PF.Bold, PF.Italic, PF.Underline, PF.StrikeOut, true, PF.Color_Main, PF.Color_Outline, PF.Kapit�lchen, PF.OnlyUpper, PF.OnlyLower);
                    break;

                case "/3":
                    PF = BlueFont.Get(PF.FontName, PF.FontSize, PF.Bold, PF.Italic, PF.Underline, PF.StrikeOut, false, PF.Color_Main, PF.Color_Outline, PF.Kapit�lchen, PF.OnlyUpper, PF.OnlyLower);
                    break;

                case "FONTSIZE":
                    PF = BlueFont.Get(PF.FontName, modConverter.FloatParse(Attribut), PF.Bold, PF.Italic, PF.Underline, PF.StrikeOut, PF.Outline, PF.Color_Main, PF.Color_Outline, PF.Kapit�lchen, PF.OnlyUpper, PF.OnlyLower);
                    break;

                case "FONTNAME":
                    PF = BlueFont.Get(Attribut, PF.FontSize, PF.Bold, PF.Italic, PF.Underline, PF.StrikeOut, PF.Outline, PF.Color_Main, PF.Color_Outline, PF.Kapit�lchen, PF.OnlyUpper, PF.OnlyLower);
                    break;

                case "FONTCOLOR":
                    PF = BlueFont.Get(PF.FontName, PF.FontSize, PF.Bold, PF.Italic, PF.Underline, PF.StrikeOut, PF.Outline, Attribut, PF.Color_Outline.ToHTMLCode(), PF.Kapit�lchen, PF.OnlyUpper, PF.OnlyLower);
                    break;

                case "FONTOUTLINE":
                    PF = BlueFont.Get(PF.FontName, PF.FontSize, PF.Bold, PF.Italic, PF.Underline, PF.StrikeOut, PF.Outline, PF.Color_Main.ToHTMLCode(), Attribut, PF.Kapit�lchen, PF.OnlyUpper, PF.OnlyLower);
                    break;

                case "BR":
                    Position++;
                    Chars.Add(new ExtChar((char)13, _Design, _State, PF, Stufe, enMarkState.None));
                    break;
                //
                case "HR":
                //    Position++;
                //    Chars.Add(new ExtChar(13, _Design, _State, PF, Stufe, MarkState));
                //    Position++;
                //    Chars.Add(new ExtChar((int)enEtxtCodes.HorizontalLine, _Design, _State, PF, Stufe, MarkState));
                //    break;

                case "TAB":
                    Position++;
                    Chars.Add(new ExtChar((char)9, _Design, _State, PF, Stufe, enMarkState.None));
                    break;

                case "ZBX_STORE":
                    Position++;
                    Chars.Add(new ExtChar(ExtChar.StoreX, _Design, _State, PF, Stufe, enMarkState.None));
                    break;
                //
                case "ZBX_RESET":
                //    Position++;
                //    Chars.Add(new ExtChar((int)enEtxtCodes.ZBX_RESET, _Design, _State, PF, Stufe, MarkState));
                //    break;

                case "TOP":
                    Position++;
                    Chars.Add(new ExtChar(ExtChar.Top, _Design, _State, PF, Stufe, enMarkState.None));
                    break;
                //
                case "ZBY_STORE":
                //    Position++;
                //    Chars.Add(new ExtChar((int)enEtxtCodes.ZBY_STORE, _Design, _State, PF, Stufe, MarkState));
                //    break;
                //
                case "ZBY_RESET":
                //    Position++;
                //    Chars.Add(new ExtChar((int)enEtxtCodes.ZBY_RESET, _Design, _State, PF, Stufe, MarkState));
                //    break;
                //
                case "LEFT":
                //    Position++;
                //    Chars.Add(new ExtChar((int)enEtxtCodes.Left, _Design, _State, PF, Stufe, MarkState));
                //    break;

                case "IMAGECODE":
                    QuickImage x;
                    x = !Attribut.Contains("|") && PF != null ? QuickImage.Get(Attribut, (int)PF.Oberl�nge(1)) : QuickImage.Get(Attribut);
                    Position++;
                    Chars.Add(new ExtChar((char)(QuickImage.GetIndex(x) + (int)enASCIIKey.ImageStart), _Design, _State, PF, Stufe, enMarkState.None));
                    break;
                //
                case "PROGRESSBAR":
                //    Position++;
                //    Chars.Add(new ExtChar((int)((int)enEtxtCodes.ProgressBar0 + int.Parse(Attribut)), _Design, _State, PF, Stufe, MarkState));
                //    break;

                case "H7":
                    Stufe = 7;
                    PF = Skin.GetBlueFont((int)_Design, _State, _Row, Stufe);
                    break;

                case "H6":
                    Stufe = 6;
                    PF = Skin.GetBlueFont((int)_Design, _State, _Row, Stufe);
                    break;

                case "H5":
                    Stufe = 5;
                    PF = Skin.GetBlueFont((int)_Design, _State, _Row, Stufe);
                    break;

                case "H4":
                    Stufe = 4;
                    PF = Skin.GetBlueFont((int)_Design, _State, _Row, Stufe);
                    break;

                case "H3":
                    Stufe = 3;
                    PF = Skin.GetBlueFont((int)_Design, _State, _Row, Stufe);
                    break;

                case "H2":
                    Stufe = 2;
                    PF = Skin.GetBlueFont((int)_Design, _State, _Row, Stufe);
                    break;

                case "H1":
                    Stufe = 1;
                    PF = Skin.GetBlueFont((int)_Design, _State, _Row, Stufe);
                    break;

                case "MARKSTATE":
                    MarkState = (enMarkState)int.Parse(Attribut);
                    break;

                case "":
                    // ist evtl. ein <> ausruck eines Textes
                    break;

                default:
                    // Develop.DebugPrint("Unbekannter HTML-Code: " + Oricode);
                    break;
            }
        }

        private void DoSpecialEntities(string xHTMLTextx, ref int xStartPosx, ref int xPosition, ref BlueFont f, ref int Stufe, ref enMarkState MarkState) {
            var Endpos = xHTMLTextx.IndexOf(';', xStartPosx + 1);
            xPosition++;
            if (Endpos <= xStartPosx || Endpos > xStartPosx + 10) {
                // Ein nicht konvertiertes &, einfach so �bernehmen.
                Chars.Add(new ExtChar('&', _Design, _State, f, Stufe, MarkState));
                return;
            }
            switch (xHTMLTextx.Substring(xStartPosx, Endpos - xStartPosx + 1)) {
                case "&uuml;":
                    Chars.Add(new ExtChar('�', _Design, _State, f, Stufe, MarkState));
                    break;

                case "&auml;":
                    Chars.Add(new ExtChar('�', _Design, _State, f, Stufe, MarkState));
                    break;

                case "&ouml;":
                    Chars.Add(new ExtChar('�', _Design, _State, f, Stufe, MarkState));
                    break;

                case "&Uuml;":
                    Chars.Add(new ExtChar('�', _Design, _State, f, Stufe, MarkState));
                    break;

                case "&Auml;":
                    Chars.Add(new ExtChar('�', _Design, _State, f, Stufe, MarkState));
                    break;

                case "&Ouml;":
                    Chars.Add(new ExtChar('�', _Design, _State, f, Stufe, MarkState));
                    break;

                case "&szlig;":
                    Chars.Add(new ExtChar('�', _Design, _State, f, Stufe, MarkState));
                    break;

                case "&quot;":
                    Chars.Add(new ExtChar('\"', _Design, _State, f, Stufe, MarkState));
                    break;

                case "&amp;":
                    Chars.Add(new ExtChar('&', _Design, _State, f, Stufe, MarkState));
                    break;

                case "&lt;":
                    Chars.Add(new ExtChar('<', _Design, _State, f, Stufe, MarkState));
                    break;

                case "&gt;":
                    Chars.Add(new ExtChar('>', _Design, _State, f, Stufe, MarkState));
                    break;

                case "&Oslash;":
                    Chars.Add(new ExtChar('�', _Design, _State, f, Stufe, MarkState));
                    break;

                case "&oslash;":
                    Chars.Add(new ExtChar('�', _Design, _State, f, Stufe, MarkState));
                    break;

                case "&bull;":
                    Chars.Add(new ExtChar('�', _Design, _State, f, Stufe, MarkState));
                    break;

                case "&eacute;":
                    Chars.Add(new ExtChar('�', _Design, _State, f, Stufe, MarkState));
                    break;

                case "&Eacute;":
                    Chars.Add(new ExtChar('�', _Design, _State, f, Stufe, MarkState));
                    break;

                case "&euro;":
                    Chars.Add(new ExtChar('�', _Design, _State, f, Stufe, MarkState));
                    break;

                default:
                    Develop.DebugPrint(enFehlerArt.Info, "Unbekannter Code: " + xHTMLTextx.Substring(xStartPosx, Endpos - xStartPosx + 1));
                    Chars.Add(new ExtChar('&', _Design, _State, f, Stufe, MarkState));
                    return;
            }
            xStartPosx = Endpos;
        }

        private void DrawState(Graphics GR, float czoom, enMarkState state) {
            var tmas = -1;
            for (var Pos = 0; Pos < Chars.Count; Pos++) {
                var tempVar = Chars[Pos];
                var marked = tempVar.Marking.HasFlag(state);
                if (marked && tmas < 0) {
                    tmas = Pos;
                }
                if (!marked || tempVar.Char == 0 || Pos == Chars.Count - 1) {
                    if (tmas > -1) {
                        if (Pos == Chars.Count - 1) {
                            DrawZone(GR, czoom, state, tmas, Pos);
                        } else {
                            DrawZone(GR, czoom, state, tmas, Pos - 1);
                        }
                        tmas = -1;
                    }
                }
            }
        }

        private void DrawStates(Graphics GR, float czoom) {
            DrawState(GR, czoom, enMarkState.Field);
            DrawState(GR, czoom, enMarkState.MyOwn);
            DrawState(GR, czoom, enMarkState.Other);
            DrawState(GR, czoom, enMarkState.Ringelchen);
        }

        private void DrawZone(Graphics GR, float czoom, enMarkState ThisState, int MarkStart, int MarkEnd) {
            var StartX = (Chars[MarkStart].Pos.X * czoom) + DrawingPos.X;
            var StartY = (Chars[MarkStart].Pos.Y * czoom) + DrawingPos.Y;
            var EndX = (Chars[MarkEnd].Pos.X * czoom) + DrawingPos.X + (Chars[MarkEnd].Size.Width * czoom);
            var Endy = (Chars[MarkEnd].Pos.Y * czoom) + DrawingPos.Y + (Chars[MarkEnd].Size.Height * czoom);
            switch (ThisState) {
                case enMarkState.None:
                    break;

                case enMarkState.Ringelchen:
                    GR.DrawLine(new Pen(Color.Red, 3 * czoom), StartX, (int)(StartY + (Chars[MarkStart].Size.Height * czoom * 0.9)), EndX, (int)(StartY + (Chars[MarkStart].Size.Height * czoom * 0.9)));
                    break;

                case enMarkState.Field:
                    GR.FillRectangle(new SolidBrush(Color.FromArgb(80, 128, 128, 128)), StartX, StartY, EndX - StartX, Endy - StartY);
                    break;

                case enMarkState.MyOwn:
                    GR.FillRectangle(new SolidBrush(Color.FromArgb(40, 50, 255, 50)), StartX, StartY, EndX - StartX, Endy - StartY);
                    break;

                case enMarkState.Other:
                    GR.FillRectangle(new SolidBrush(Color.FromArgb(80, 255, 255, 50)), StartX, StartY, EndX - StartX, Endy - StartY);
                    break;

                default:
                    Develop.DebugPrint(ThisState);
                    break;
            }
        }

        private void Initialize() {
            _Design = enDesign.Undefiniert;
            _State = enStates.Standard;
            _Row = null;
            DrawingPos = new Point(0, 0);
            Ausrichtung = enAlignment.Top_Left;
            Multiline = true;
            AllowedChars = string.Empty;
            Chars = new List<ExtChar>();
            DrawingArea = new Rectangle(0, 0, -1, -1);
            _TextDimensions = Size.Empty;
            _Width = null;
            _Height = null;
            _Zeilenabstand = 1;
            _TMPHtmlText = string.Empty;
            _TMPPlainText = string.Empty;
        }

        private bool InsertAnything(enASCIIKey KeyAscii, string img, int Position) {
            if (Position < 0 && !string.IsNullOrEmpty(PlainText)) { return false; }            // Text zwar da, aber kein Cursor angezeigt
            if (Position < 0) { Position = 0; }// Ist echt m�glich!
            BlueFont tmpFont = null;
            var tmpStufe = 4;
            var tmpState = enStates.Undefiniert;
            var tmpMarkState = enMarkState.None;
            if (Position > Chars.Count) { Position = Chars.Count; }
            if ((int)_Design > 10000) { Develop.DebugPrint(enFehlerArt.Fehler, "Falsche Art"); }
            if (Position < Chars.Count && Chars[Position].Char > 0) {
                tmpFont = Chars[Position].Font;
                tmpState = Chars[Position].State;
                tmpStufe = Chars[Position].Stufe;
                tmpMarkState = Chars[Position].Marking;
            }
            if (tmpFont == null) {
                tmpFont = Skin.GetBlueFont(_Design, _State);
                tmpState = _State;
                tmpStufe = 4;
                tmpMarkState = enMarkState.None;
            }
            if (KeyAscii != enASCIIKey.Undefined) {
                if (KeyAscii == enASCIIKey.ENTER) {
                    if (!Multiline) { return false; }
                } else {
                    if (!string.IsNullOrEmpty(AllowedChars) && !AllowedChars.Contains(Convert.ToChar(KeyAscii).ToString())) { return false; }
                }
                Chars.Insert(Position, new ExtChar((char)KeyAscii, _Design, tmpState, tmpFont, tmpStufe, tmpMarkState));
            } else if (!string.IsNullOrEmpty(img)) {
                var x = QuickImage.Get(img, (int)tmpFont.Oberl�nge(1));
                Chars.Insert(Position, new ExtChar((char)(QuickImage.GetIndex(x) + (int)enASCIIKey.ImageStart), _Design, tmpState, tmpFont, tmpStufe, tmpMarkState));
            } else {
                return false;
            }
            ResetPosition(true);
            return true;
        }

        /// <summary>
        /// Berechnet die Zeichen-Positionen mit korrekten Umbr�chen. Die enAlignment wird ebefalls mit eingerechnet.
        /// Cursor_ComputePixelXPos wird am Ende aufgerufen, einschlie�lich Cursor_Repair und SetNewAkt.
        /// </summary>
        /// <remarks></remarks>
        private void ReBreak() {
            _Width = 0;
            _Height = 0;
            if (Chars.Count == 0) { return; }
            List<string> RI = new();
            var vZBX_Pixel = 0f;
            var IsX = 0f;
            var IsY = 0f;
            var ZB_Char = 0;
            var Akt = -1;
            do {
                Akt++;
                if (Akt > Chars.Count - 1) {
                    RI.Add(ZB_Char + ";" + (Akt - 1));
                    break;
                }
                switch (Chars[Akt].Char) {
                    //
                    case 9:
                    //    Chars[Akt].Width = (float)((Math.Truncate(IsX / 100) + 1) * 100 - IsX);
                    //    break;

                    case ExtChar.StoreX:
                        vZBX_Pixel = IsX;
                        break;

                    case ExtChar.Top:
                        IsY = 0;
                        break;
                }
                if (!Chars[Akt].isSpace()) {
                    if (Akt > ZB_Char && _TextDimensions.Width > 0) {
                        if (IsX + Chars[Akt].Size.Width + 0.5 > _TextDimensions.Width) {
                            Akt = WordBreaker(Akt, ZB_Char);
                            IsX = vZBX_Pixel;
                            IsY += Row_SetOnLine(ZB_Char, Akt - 1) * _Zeilenabstand;
                            RI.Add(ZB_Char + ";" + (Akt - 1));
                            ZB_Char = Akt;
                        }
                    }
                    _Width = Math.Max((int)_Width, (int)(IsX + Chars[Akt].Size.Width + 0.5));
                    _Height = Math.Max((int)_Height, (int)(IsY + Chars[Akt].Size.Height + 0.5));
                }
                Chars[Akt].Pos.X = (float)IsX;
                Chars[Akt].Pos.Y = (float)IsY;
                // Diese Zeile garantiert, dass immer genau EIN Pixel frei ist zwischen zwei Buchstaben.
                IsX = (float)(IsX + Math.Truncate(Chars[Akt].Size.Width + 0.5));
                if (Chars[Akt].isLineBreak()) {
                    IsX = vZBX_Pixel;
                    if (Chars[Akt].Char == ExtChar.Top) {
                        Row_SetOnLine(ZB_Char, Akt);
                        RI.Add(ZB_Char + ";" + Akt);
                    } else {
                        IsY += (int)(Row_SetOnLine(ZB_Char, Akt) * _Zeilenabstand);
                        RI.Add(ZB_Char + ";" + Akt);
                    }
                    ZB_Char = Akt + 1;
                }
            } while (true);
            //    vHeight = CInt(Math.Max(vHeight, _Chars(ZB_Char).Pos.Y + _Chars(ZB_Char).Size.Height))
            // enAlignment berechnen -------------------------------------
            if (Ausrichtung != enAlignment.Top_Left) {
                float KY = 0;
                if (Convert.ToBoolean(Ausrichtung & enAlignment.VerticalCenter)) { KY = (float)((_TextDimensions.Height - (int)_Height) / 2.0); }
                if (Convert.ToBoolean(Ausrichtung & enAlignment.Bottom)) { KY = _TextDimensions.Height - (int)_Height; }
                foreach (var t in RI) {
                    var o = t.SplitBy(";");
                    var Z1 = int.Parse(o[0]);
                    var Z2 = int.Parse(o[1]);
                    float KX = 0;
                    if (Convert.ToBoolean(Ausrichtung & enAlignment.Right)) { KX = _TextDimensions.Width - Chars[Z2].Pos.X - Chars[Z2].Size.Width; }
                    if (Convert.ToBoolean(Ausrichtung & enAlignment.HorizontalCenter)) { KX = (_TextDimensions.Width - Chars[Z2].Pos.X - Chars[Z2].Size.Width) / 2; }
                    for (var Z3 = Z1; Z3 <= Z2; Z3++) {
                        Chars[Z3].Pos.X += KX;
                        Chars[Z3].Pos.Y += KY;
                    }
                }
            }
        }

        private void ResetPosition(bool AndTmpText) {
            _Width = null;
            _Height = null;
            if (AndTmpText) {
                _TMPHtmlText = string.Empty;
                _TMPPlainText = string.Empty;
            }
        }

        private float Row_SetOnLine(int Von, int Nach) {
            float Abstand = 0;
            for (var z = Von; z <= Nach; z++) {
                Abstand = Math.Max(Abstand, Chars[z].Size.Height);
            }
            for (var z = Von; z <= Nach; z++) {
                if (Chars[z].Char != ExtChar.Top) {
                    Chars[z].Pos.Y = Chars[z].Pos.Y + Abstand - Chars[z].Size.Height;
                }
            }
            return Abstand;
        }

        private int WordBreaker(int AugZeichen, int MinZeichen) {
            if (Chars.Count == 1) { return 0; }
            if (MinZeichen < 0) { MinZeichen = 0; }
            if (AugZeichen > Chars.Count - 1) { AugZeichen = Chars.Count - 1; }
            if (AugZeichen < MinZeichen + 1) { AugZeichen = MinZeichen + 1; }
            // AusnahmeFall auschlie�en:
            // Space-Zeichen - Dann Buchstabe
            if (Chars[AugZeichen - 1].isSpace() && !Chars[AugZeichen].isPossibleLineBreak()) { return AugZeichen; }
            var Started = AugZeichen;
            // Das Letzte Zeichen Search, das kein Trennzeichen ist
            do {
                if (Chars[AugZeichen].isPossibleLineBreak()) {
                    AugZeichen--;
                } else {
                    break;
                }
                if (AugZeichen <= MinZeichen) { return Started; }
            } while (true);
            do {
                if (Chars[AugZeichen].isPossibleLineBreak()) { return AugZeichen + 1; }
                AugZeichen--;
                if (AugZeichen <= MinZeichen) { return Started; }
            } while (true);
        }

        #endregion
    }
}
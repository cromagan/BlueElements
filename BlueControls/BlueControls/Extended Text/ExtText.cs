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
using BlueControls.Enums;
using BlueDatabase;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;

// VTextTyp-Hirachie
// ~~~~~~~~~~~~~~~~~
// HTMLText, PlainText = Diese Texte wurden in den Speicher geschrieben und führen
//                       -> Folgestatus: ?_Converted
// ?_Converted         = Der Text ist immer noch führend, es wurde aber schon konvertiert
// Chars_Converted     = Der Ursprüngliche Text wurde verworfen, der Text wird Komplett über die Chars gehandlet
//                       Wird nur bei Textbearbeitung (Key Up o. ä. aktiviert)
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
// TOP = Y auf 0 zurücksetzen
// vState = vState Setzen (mit HTML_Code)

namespace BlueControls {

    public sealed class ExtText : ListExt<ExtChar> {

        #region Fields

        //public readonly List<ExtChar> Chars = new();
        public string AllowedChars;

        public enAlignment Ausrichtung;

        /// <summary>
        /// Falls mit einer Skalierung gezeichnet wird, müssen die Angaben bereits skaliert sein.
        /// </summary>
        public Rectangle DrawingArea;

        /// <summary>
        /// Falls mit einer Skalierung gezeichnet wird, müssen die Angaben bereits skaliert sein.
        /// </summary>
        public Point DrawingPos;

        public bool Multiline;
        private enDesign _Design;
        private int? _Height = null;
        private RowItem _Row;
        private enStates _State;
        private Size _TextDimensions;
        private string? _TMPHtmlText = null;
        private string? _TMPPlainText = null;
        private int? _Width = null;
        private float _Zeilenabstand = 1;

        #endregion

        #region Constructors

        public ExtText(enDesign design, enStates state) : base() {
            _Design = enDesign.Undefiniert;
            _State = enStates.Standard;
            _Row = null;
            DrawingPos = new Point(0, 0);
            Ausrichtung = enAlignment.Top_Left;
            Multiline = true;
            AllowedChars = string.Empty;
            DrawingArea = new Rectangle(0, 0, -1, -1);
            _TextDimensions = Size.Empty;
            _Width = null;
            _Height = null;
            _Zeilenabstand = 1;
            _TMPHtmlText = null;
            _TMPPlainText = null;

            _Design = design;
            _State = state;
        }

        public ExtText(PadStyles design, RowItem skinRow) : this((enDesign)design, enStates.Standard) {
            _Row = skinRow;

            if ((int)_Design < 10000 || _Row == null) {
                Develop.DebugPrint(enFehlerArt.Fehler, "Fehler!");
            }
        }

        #endregion

        #region Properties

        public enDesign Design {
            get => _Design;
            set {
                if (value == _Design) { return; }
                _Design = value;

                Parallel.ForEach(this, ch => { ch.Design = _Design; });
                OnChanged();
            }
        }

        public string HtmlText {
            get {
                if (_TMPHtmlText == null) {
                    _TMPHtmlText = ConvertCharToHTMLText(0, Count - 1);
                }
                return _TMPHtmlText;
            }
            set {
                if (HtmlText == value) { return; }
                ConvertTextToChar(value, true);
                OnChanged();
            }
        }

        public string PlainText {
            get {
                if (_TMPPlainText == null) {
                    _TMPPlainText = ConvertCharToPlainText(0, this.Count - 1);
                }
                return _TMPPlainText;
            }
            set {
                if (PlainText == value) { return; }
                ConvertTextToChar(value, false);
                OnChanged();
            }
        }

        public enStates State {
            get => _State;
            set {
                if (value == _State) { return; }
                _State = value;

                Parallel.ForEach(this, ch => { ch.State = _State; });
                OnChanged();
            }
        }

        /// <summary>
        /// Nach wieviel Pixeln der Zeilenumbruch stattfinden soll. -1 wenn kein Umbruch sein soll. Auch das Alingement richtet sich nach diesen Größen.
        /// </summary>
        public Size TextDimensions {
            get => _TextDimensions;
            set {
                if (_TextDimensions.Width == value.Width && _TextDimensions.Height == value.Height) { return; }
                _TextDimensions = value;
                ResetPosition(false);
                OnChanged();
            }
        }

        public float Zeilenabstand {
            get => _Zeilenabstand;
            set {
                if (value == _Zeilenabstand) { return; }
                _Zeilenabstand = value;
                ResetPosition(false);
                OnChanged();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Sucht den aktuellen Buchstaben, der unter den angegeben Koordinaten liegt.
        ///     Wird kein Char gefunden, wird der logischste Char gewählt. (z.B. Nach ZeilenEnde = Letzzter Buchstabe der Zeile)
        /// </summary>
        /// <remarks></remarks>
        public int Char_Search(double pixX, double pixY) {
            var cZ = -1;
            var XDi = double.MaxValue;
            var YDi = double.MaxValue;
            var XNr = -1;
            var YNr = -1;

            do {
                cZ++;
                if (cZ > this.Count - 1) { break; }// Das Ende des Textes
                if (this[cZ].Size.Width > 0) {
                    var MatchX = pixX >= DrawingPos.X + this[cZ].Pos.X && pixX <= DrawingPos.X + this[cZ].Pos.X + this[cZ].Size.Width;
                    var MatchY = pixY >= DrawingPos.Y + this[cZ].Pos.Y && pixY <= DrawingPos.Y + this[cZ].Pos.Y + this[cZ].Size.Height;

                    if (MatchX && MatchY) { return cZ; }

                    double TmpDi;
                    if (!MatchX && MatchY) {
                        TmpDi = Math.Abs(pixX - (DrawingPos.X + this[cZ].Pos.X + (this[cZ].Size.Width / 2.0)));
                        if (TmpDi < XDi) {
                            XNr = cZ;
                            XDi = TmpDi;
                        }
                    } else if (MatchX && !MatchY) {
                        TmpDi = Math.Abs(pixY - (DrawingPos.Y + this[cZ].Pos.Y + (this[cZ].Size.Height / 2.0)));
                        if (TmpDi < YDi) {
                            YNr = cZ;
                            YDi = TmpDi;
                        }
                    }
                }
            } while (true);

            cZ--;
            return XNr >= 0 ? XNr : YNr >= 0 ? YNr : cZ >= 0 ? cZ : 0;
        }

        public void Check(int first, int last, bool checkstate) {
            for (var cc = first; cc <= last; cc++) {
                if (this[cc].State != enStates.Undefiniert) {
                    if (checkstate) {
                        if (!this[cc].State.HasFlag(enStates.Checked)) {
                            this[cc].State |= enStates.Checked;
                        }
                    } else {
                        if (this[cc].State.HasFlag(enStates.Checked)) {
                            this[cc].State = this[cc].State ^ enStates.Checked;
                        }
                    }
                }
            }
        }

        public Rectangle CursorPixelPosX(int charPos) {
            while (_Width == null) { ReBreak(); }

            if (charPos > this.Count + 1) { charPos = this.Count + 1; }
            if (this.Count > 0 && charPos < 0) { charPos = 0; }
            float X = 0;
            float Y = 0;
            float He = 14;

            if (this.Count == 0) {
                // Kein Text vorhanden
            } else if (charPos < this.Count) {
                // Cursor vor einem Zeichen
                X = this[charPos].Pos.X;
                Y = this[charPos].Pos.Y;
                He = this[charPos].Size.Height;
            } else if (charPos > 0 && charPos < this.Count + 1 && this[charPos - 1].isLineBreak()) {
                // Vorzeichen = Zeilenumbruch
                Y = this[charPos - 1].Pos.Y + this[charPos - 1].Size.Height;
                He = this[charPos - 1].Size.Height;
            } else if (charPos > 0 && charPos < this.Count + 1) {
                // Vorzeichen = Echtes Char
                X = this[charPos - 1].Pos.X + this[charPos - 1].Size.Width;
                Y = this[charPos - 1].Pos.Y;
                He = this[charPos - 1].Size.Height;
            }
            return new Rectangle((int)X, (int)(Y - 1), 0, (int)(He + 2));
        }

        public void Delete(int first, int last) {
            var tempVar = last - first;
            for (var z = 1; z <= tempVar; z++) {
                if (first < this.Count) {
                    this.RemoveAt(first);
                }
            }
            ResetPosition(true);
        }

        public void Draw(Graphics gr, float zoom) {
            while (_Width == null) { ReBreak(); }
            DrawStates(gr, zoom);

            var lockMe = new object();
            //var results = new List<string>();
            //Parallel.ForEach(all, (thisP, state) => {
            //    if (FileExists(thisP)) {
            //        lock (lockMe) {
            //            results.AddIfNotExists(thisP);
            //            state.Break();
            //        }
            //    }
            //});

            Parallel.ForEach(this, t => {
                if (t.IsVisible(zoom, DrawingPos, DrawingArea)) {
                    lock (lockMe) {
                        t.Draw(gr, DrawingPos, zoom);
                    }
                }
            });
        }

        public int Height() {
            while (_Width == null) { ReBreak(); }

            return (int)_Height;
        }

        public bool InsertChar(enASCIIKey ascii, int position) {
            if ((int)ascii < 13) { return false; }
            var c = new ExtCharASCII((char)ascii, Design, State, null, 4, enMarkState.None);
            Insert(position, c);
            return true;
        }

        public bool InsertImage(string imagecode, int position) {
            if (string.IsNullOrEmpty(imagecode)) { return false; }

            var c = new ExtCharImageCode(imagecode, Design, State, null, 4);

            Insert(position, c);
            return true;
        }

        public Size LastSize() {
            while (_Width == null) { ReBreak(); }
            return _Width < 5 || _Height < 5 ? new Size(32, 16) : new Size((int)_Width, (int)_Height);
        }

        public void StufeÄndern(int first, int last, int stufe) {
            for (var cc = first; cc <= Math.Min(last, this.Count - 1); cc++) {
                this[cc].Stufe = stufe;
            }
            ResetPosition(true);
        }

        public string Substring(int startIndex, int lenght) => ConvertCharToPlainText(startIndex, startIndex + lenght - 1);

        public int Width() {
            while (_Width == null) { ReBreak(); }

            return (int)_Width;
        }

        public string Word(int atPosition) {
            var S = WordStart(atPosition);
            var E = WordEnd(atPosition);
            return Substring(S, E - S);
        }

        internal string ConvertCharToPlainText(int first, int last) {
            try {
                var T = new StringBuilder();
                for (var cZ = first; cZ <= Math.Min(last, this.Count - 1); cZ++) {
                    T.Append(this[cZ].PlainText());
                }
                return T.ToString().Replace("\n", string.Empty);
            } catch {
                // Wenn Chars geändert wird (und dann der Count nimmer stimmt)
                return ConvertCharToPlainText(first, last);
            }
        }

        internal void InsertCRLF(int position) {
            Insert(position, new ExtCharCRLFCode());
        }

        internal void Mark(enMarkState markstate, int first, int last) {
            try {
                for (var z = first; z <= Math.Min(last, this.Count - 1); z++) {
                    if (!this[z].Marking.HasFlag(markstate)) {
                        this[z].Marking = this[z].Marking | markstate;
                    }
                }
            } catch {
                Mark(markstate, first, last);
            }
        }

        internal void Unmark(enMarkState markstate) {
            foreach (var t in this) {
                if (t.Marking.HasFlag(markstate)) {
                    t.Marking ^= markstate;
                }
            }
        }

        internal int WordEnd(int pos) {
            if (this.Count == 0) { return -1; }
            if (pos < 0 || pos >= this.Count) { return -1; }
            if (this[pos].isWordSeperator()) { return -1; }
            do {
                pos++;
                if (pos >= this.Count) { return this.Count; }
                if (this[pos].isWordSeperator()) { return pos; }
            } while (true);
        }

        internal int WordStart(int pos) {
            if (this.Count == 0) { return -1; }
            if (pos < 0 || pos >= this.Count) { return -1; }
            if (this[pos].isWordSeperator()) { return -1; }
            do {
                pos--;
                if (pos < 0) { return 0; }
                if (this[pos].isWordSeperator()) { return pos + 1; }
            } while (true);
        }

        private string ConvertCharToHTMLText(int first, int last) {
            var t = new StringBuilder();

            var cZ = first;
            last = Math.Min(last, this.Count - 1);
            var LastStufe = 4;

            for (var z = first; z <= last; z++) {
                if (z <= first) { LastStufe = this[cZ].Stufe; }
                if (z == first && LastStufe != 4) { t.Append("<H" + this[cZ].Stufe + ">"); }

                if (LastStufe != this[cZ].Stufe) {
                    t.Append("<H" + this[cZ].Stufe + ">");
                    LastStufe = this[cZ].Stufe;
                }

                t.Append(this[cZ].HTMLText());
            }

            return t.ToString();
        }

        private void ConvertTextToChar(string cactext, bool isRich) {
            var Pos = 0;
            var Zeichen = -1;
            var Stufe = 4;
            var Markstate = enMarkState.None;
            this.Clear();
            ResetPosition(true);
            var BF = (int)_Design > 10000 ? Skin.GetBlueFont((PadStyles)_Design, _Row) : Skin.GetBlueFont(_Design, _State);
            if (BF == null) { return; }// Wenn die DAtenbanken entladen wurde, bei Programmende

            if (!string.IsNullOrEmpty(cactext)) {
                cactext = isRich ? cactext.ConvertFromHtmlToRich() : cactext.Replace("\r\n", "\r");
                var Lang = cactext.Length - 1;
                do {
                    if (Pos > Lang) { break; }
                    var CH = cactext[Pos];
                    if (isRich) {
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
                                this.Add(new ExtCharASCII(CH, _Design, _State, BF, Stufe, Markstate));
                                break;
                        }
                    } else {
                        // Normales Zeichen
                        Zeichen++;
                        this.Add(new ExtCharASCII(CH, _Design, _State, BF, Stufe, Markstate));
                    }
                    Pos++;
                } while (true);
            }
            ResetPosition(true);
        }

        private void DoHTMLCode(string HTMLText, int start, ref int Position, ref BlueFont font, ref int Stufe, ref enMarkState MarkState) {
            if (font == null) { return; }  // wenn die Datenbanken entladen wurden bei Programmende

            var Endpos = HTMLText.IndexOf('>', start + 1);
            if (Endpos <= start) {
                Develop.DebugPrint("String-Fehler, > erwartet. " + HTMLText);
                return;
            }
            var Oricode = HTMLText.Substring(start + 1, Endpos - start - 1);
            var Istgleich = Oricode.IndexOf('=');
            string Cod, Attribut;
            if (Istgleich < 0) {
                // <H4> wird durch autoprüfung zu <H4 >
                Cod = Oricode.ToUpper().Trim();
                Attribut = string.Empty;
            } else {
                Cod = Oricode.Substring(0, Istgleich).Replace(" ", "").ToUpper().Trim();
                Attribut = Oricode.Substring(Istgleich + 1).Trim('\"');
            }
            switch (Cod) {
                case "B":
                    font = BlueFont.Get(font.FontName, font.FontSize, true, font.Italic, font.Underline, font.StrikeOut, font.Outline, font.Color_Main, font.Color_Outline, font.Kapitälchen, font.OnlyUpper, font.OnlyLower);
                    break;

                case "/B":
                    font = BlueFont.Get(font.FontName, font.FontSize, false, font.Italic, font.Underline, font.StrikeOut, font.Outline, font.Color_Main, font.Color_Outline, font.Kapitälchen, font.OnlyUpper, font.OnlyLower);
                    break;

                case "I":
                    font = BlueFont.Get(font.FontName, font.FontSize, font.Bold, true, font.Underline, font.StrikeOut, font.Outline, font.Color_Main, font.Color_Outline, font.Kapitälchen, font.OnlyUpper, font.OnlyLower);
                    break;

                case "/I":
                    font = BlueFont.Get(font.FontName, font.FontSize, font.Bold, false, font.Underline, font.StrikeOut, font.Outline, font.Color_Main, font.Color_Outline, font.Kapitälchen, font.OnlyUpper, font.OnlyLower);
                    break;

                case "U":
                    font = BlueFont.Get(font.FontName, font.FontSize, font.Bold, font.Italic, true, font.StrikeOut, font.Outline, font.Color_Main, font.Color_Outline, font.Kapitälchen, font.OnlyUpper, font.OnlyLower);
                    break;

                case "/U":
                    font = BlueFont.Get(font.FontName, font.FontSize, font.Bold, font.Italic, false, font.StrikeOut, font.Outline, font.Color_Main, font.Color_Outline, font.Kapitälchen, font.OnlyUpper, font.OnlyLower);
                    break;

                case "STRIKE":
                    font = BlueFont.Get(font.FontName, font.FontSize, font.Bold, font.Italic, font.Underline, true, font.Outline, font.Color_Main, font.Color_Outline, font.Kapitälchen, font.OnlyUpper, font.OnlyLower);
                    break;

                case "/STRIKE":
                    font = BlueFont.Get(font.FontName, font.FontSize, font.Bold, font.Italic, font.Underline, false, font.Outline, font.Color_Main, font.Color_Outline, font.Kapitälchen, font.OnlyUpper, font.OnlyLower);
                    break;

                case "3":
                    font = BlueFont.Get(font.FontName, font.FontSize, font.Bold, font.Italic, font.Underline, font.StrikeOut, true, font.Color_Main, font.Color_Outline, font.Kapitälchen, font.OnlyUpper, font.OnlyLower);
                    break;

                case "/3":
                    font = BlueFont.Get(font.FontName, font.FontSize, font.Bold, font.Italic, font.Underline, font.StrikeOut, false, font.Color_Main, font.Color_Outline, font.Kapitälchen, font.OnlyUpper, font.OnlyLower);
                    break;

                case "FONTSIZE":
                    font = BlueFont.Get(font.FontName, Converter.FloatParse(Attribut), font.Bold, font.Italic, font.Underline, font.StrikeOut, font.Outline, font.Color_Main, font.Color_Outline, font.Kapitälchen, font.OnlyUpper, font.OnlyLower);
                    break;

                case "FONTNAME":
                    font = BlueFont.Get(Attribut, font.FontSize, font.Bold, font.Italic, font.Underline, font.StrikeOut, font.Outline, font.Color_Main, font.Color_Outline, font.Kapitälchen, font.OnlyUpper, font.OnlyLower);
                    break;

                case "FONTCOLOR":
                    font = BlueFont.Get(font.FontName, font.FontSize, font.Bold, font.Italic, font.Underline, font.StrikeOut, font.Outline, Attribut, font.Color_Outline.ToHTMLCode(), font.Kapitälchen, font.OnlyUpper, font.OnlyLower);
                    break;

                case "FONTOUTLINE":
                    font = BlueFont.Get(font.FontName, font.FontSize, font.Bold, font.Italic, font.Underline, font.StrikeOut, font.Outline, font.Color_Main.ToHTMLCode(), Attribut, font.Kapitälchen, font.OnlyUpper, font.OnlyLower);
                    break;

                case "BR":
                    Position++;
                    this.Add(new ExtCharCRLFCode(_Design, _State, font, Stufe));
                    break;
                //
                case "HR":
                //    Position++;
                //    this.Add(new ExtChar(13, _Design, _State, PF, Stufe, MarkState));
                //    Position++;
                //    this.Add(new ExtChar((int)enEtxtCodes.HorizontalLine, _Design, _State, PF, Stufe, MarkState));
                //    break;

                case "TAB":
                    Position++;
                    this.Add(new ExtCharTabCode(_Design, _State, font, Stufe));
                    //this.Add(new ExtChar((char)9, _Design, _State, font, Stufe, enMarkState.None));
                    break;

                case "ZBX_STORE":
                    Position++;
                    this.Add(new ExtCharStoreXCode(_Design, _State, font, Stufe));
                    break;

                case "TOP":
                    Position++;
                    this.Add(new ExtCharTopCode(_Design, _State, font, Stufe));
                    break;

                case "IMAGECODE":
                    QuickImage x = !Attribut.Contains("|") && font != null ? QuickImage.Get(Attribut, (int)font.Oberlänge(1)) : QuickImage.Get(Attribut);
                    Position++;
                    this.Add(new ExtCharImageCode(x, _Design, _State, font, Stufe));
                    break;

                case "H7":
                    Stufe = 7;
                    font = Skin.GetBlueFont((int)_Design, _State, _Row, Stufe);
                    break;

                case "H6":
                    Stufe = 6;
                    font = Skin.GetBlueFont((int)_Design, _State, _Row, Stufe);
                    break;

                case "H5":
                    Stufe = 5;
                    font = Skin.GetBlueFont((int)_Design, _State, _Row, Stufe);
                    break;

                case "H4":
                    Stufe = 4;
                    font = Skin.GetBlueFont((int)_Design, _State, _Row, Stufe);
                    break;

                case "H3":
                    Stufe = 3;
                    font = Skin.GetBlueFont((int)_Design, _State, _Row, Stufe);
                    break;

                case "H2":
                    Stufe = 2;
                    font = Skin.GetBlueFont((int)_Design, _State, _Row, Stufe);
                    break;

                case "H1":
                    Stufe = 1;
                    font = Skin.GetBlueFont((int)_Design, _State, _Row, Stufe);
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
                // Ein nicht konvertiertes &, einfach so übernehmen.
                this.Add(new ExtCharASCII('&', _Design, _State, f, Stufe, MarkState));
                return;
            }
            switch (xHTMLTextx.Substring(xStartPosx, Endpos - xStartPosx + 1)) {
                case "&uuml;":
                    this.Add(new ExtCharASCII('ü', _Design, _State, f, Stufe, MarkState));
                    break;

                case "&auml;":
                    this.Add(new ExtCharASCII('ä', _Design, _State, f, Stufe, MarkState));
                    break;

                case "&ouml;":
                    this.Add(new ExtCharASCII('ö', _Design, _State, f, Stufe, MarkState));
                    break;

                case "&Uuml;":
                    this.Add(new ExtCharASCII('Ü', _Design, _State, f, Stufe, MarkState));
                    break;

                case "&Auml;":
                    this.Add(new ExtCharASCII('Ä', _Design, _State, f, Stufe, MarkState));
                    break;

                case "&Ouml;":
                    this.Add(new ExtCharASCII('Ö', _Design, _State, f, Stufe, MarkState));
                    break;

                case "&szlig;":
                    this.Add(new ExtCharASCII('ß', _Design, _State, f, Stufe, MarkState));
                    break;

                case "&quot;":
                    this.Add(new ExtCharASCII('\"', _Design, _State, f, Stufe, MarkState));
                    break;

                case "&amp;":
                    this.Add(new ExtCharASCII('&', _Design, _State, f, Stufe, MarkState));
                    break;

                case "&lt;":
                    this.Add(new ExtCharASCII('<', _Design, _State, f, Stufe, MarkState));
                    break;

                case "&gt;":
                    this.Add(new ExtCharASCII('>', _Design, _State, f, Stufe, MarkState));
                    break;

                case "&Oslash;":
                    this.Add(new ExtCharASCII('Ø', _Design, _State, f, Stufe, MarkState));
                    break;

                case "&oslash;":
                    this.Add(new ExtCharASCII('ø', _Design, _State, f, Stufe, MarkState));
                    break;

                case "&bull;":
                    this.Add(new ExtCharASCII('•', _Design, _State, f, Stufe, MarkState));
                    break;

                case "&eacute;":
                    this.Add(new ExtCharASCII('é', _Design, _State, f, Stufe, MarkState));
                    break;

                case "&Eacute;":
                    this.Add(new ExtCharASCII('É', _Design, _State, f, Stufe, MarkState));
                    break;

                case "&euro;":
                    this.Add(new ExtCharASCII('€', _Design, _State, f, Stufe, MarkState));
                    break;

                default:
                    Develop.DebugPrint(enFehlerArt.Info, "Unbekannter Code: " + xHTMLTextx.Substring(xStartPosx, Endpos - xStartPosx + 1));
                    this.Add(new ExtCharASCII('&', _Design, _State, f, Stufe, MarkState));
                    return;
            }
            xStartPosx = Endpos;
        }

        private void DrawState(Graphics GR, float czoom, enMarkState state) {
            var tmas = -1;
            for (var Pos = 0; Pos < this.Count; Pos++) {
                var tempVar = this[Pos];
                var marked = tempVar.Marking.HasFlag(state);

                if (marked && tmas < 0) {
                    tmas = Pos;
                }
                if (!marked || Pos == this.Count - 1) {
                    if (tmas > -1) {
                        if (Pos == this.Count - 1) {
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
            var StartX = (this[MarkStart].Pos.X * czoom) + DrawingPos.X;
            var StartY = (this[MarkStart].Pos.Y * czoom) + DrawingPos.Y;
            var EndX = (this[MarkEnd].Pos.X * czoom) + DrawingPos.X + (this[MarkEnd].Size.Width * czoom);
            var Endy = (this[MarkEnd].Pos.Y * czoom) + DrawingPos.Y + (this[MarkEnd].Size.Height * czoom);
            switch (ThisState) {
                case enMarkState.None:
                    break;

                case enMarkState.Ringelchen:
                    GR.DrawLine(new Pen(Color.Red, 3 * czoom), StartX, (int)(StartY + (this[MarkStart].Size.Height * czoom * 0.9)), EndX, (int)(StartY + (this[MarkStart].Size.Height * czoom * 0.9)));
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

        private new void Insert(int Position, ExtChar c) {
            if (Position < 0) { Position = 0; }
            if (Position > this.Count) { Position = this.Count; }

            ExtChar Style = null;

            if (Position < this.Count) {
                Style = this[Position];
            } else if (this.Count > 0) {
                Style = this[this.Count - 1];
            }

            if (Style != null) { c.GetStyleFrom(c); }

            base.Insert(Position, c);
            ResetPosition(true);
        }

        //private bool InsertAnything(enASCIIKey KeyAscii, string img, int Position) {
        //    q
        //    if (Position < 0 && !string.IsNullOrEmpty(PlainText)) { return false; }  // Text zwar da, aber kein Cursor angezeigt
        //    if (Position < 0) { Position = 0; }// Ist echt möglich!
        //    BlueFont tmpFont = null;
        //    var tmpStufe = 4;
        //    var tmpState = enStates.Undefiniert;
        //    var tmpMarkState = enMarkState.None;
        //    if (Position > this.Count) { Position = this.Count; }
        //    if ((int)_Design > 10000) { Develop.DebugPrint(enFehlerArt.Fehler, "Falsche Art"); }
        //    if (Position < this.Count) {
        //        tmpFont = this[Position].Font;
        //        tmpState = this[Position].State;
        //        tmpStufe = this[Position].Stufe;
        //        tmpMarkState = this[Position].Marking;
        //    }
        //    if (tmpFont == null) {
        //        tmpFont = Skin.GetBlueFont(_Design, _State);
        //        tmpState = _State;
        //        tmpStufe = 4;
        //        tmpMarkState = enMarkState.None;
        //    }
        //    if (KeyAscii != enASCIIKey.Undefined) {
        //        if (KeyAscii == enASCIIKey.ENTER) {
        //            if (!Multiline) { return false; }
        //        } else {
        //            if (!string.IsNullOrEmpty(AllowedChars) && !Allowedthis.Contains(Convert.ToChar(KeyAscii).ToString())) { return false; }
        //        }
        //        this.Insert(Position, new ExtChar((char)KeyAscii, _Design, tmpState, tmpFont, tmpStufe, tmpMarkState));
        //    } else if (!string.IsNullOrEmpty(img)) {
        //        var x = QuickImage.Get(img, (int)tmpFont.Oberlänge(1));
        //        this.Insert(Position, new ExtChar((char)(QuickImage.GetIndex(x) + (int)enASCIIKey.ImageStart), _Design, tmpState, tmpFont, tmpStufe, tmpMarkState));
        //    } else {
        //        return false;
        //    }
        //    ResetPosition(true);
        //    return true;
        //}

        /// <summary>
        /// Berechnet die Zeichen-Positionen mit korrekten Umbrüchen. Die enAlignment wird ebefalls mit eingerechnet.
        /// Cursor_ComputePixelXPos wird am Ende aufgerufen, einschließlich Cursor_Repair und SetNewAkt.
        /// </summary>
        /// <remarks></remarks>
        private void ReBreak() {
            _Width = 0;
            _Height = 0;
            if (Count == 0) { return; }

            List<string> RI = new();
            var vZBX_Pixel = 0f;
            var IsX = 0f;
            var IsY = 0f;
            var ZB_Char = 0;
            var Akt = -1;

            do {
                Akt++;
                if (Akt > this.Count - 1) {
                    Row_SetOnLine(ZB_Char, Akt - 1);
                    RI.Add(ZB_Char + ";" + (Akt - 1));
                    break;
                }

                if (this[Akt] is ExtCharStoreXCode) {
                    vZBX_Pixel = IsX;
                } else if (this[Akt] is ExtCharTopCode) {
                    IsY = 0;
                }

                if (!this[Akt].isSpace()) {
                    if (Akt > ZB_Char && _TextDimensions.Width > 0) {
                        if (IsX + this[Akt].Size.Width + 0.5 > _TextDimensions.Width) {
                            Akt = WordBreaker(Akt, ZB_Char);
                            IsX = vZBX_Pixel;
                            IsY += Row_SetOnLine(ZB_Char, Akt - 1) * _Zeilenabstand;
                            RI.Add(ZB_Char + ";" + (Akt - 1));
                            ZB_Char = Akt;
                        }
                    }
                    _Width = Math.Max((int)_Width, (int)(IsX + this[Akt].Size.Width + 0.5));
                    _Height = Math.Max((int)_Height, (int)(IsY + this[Akt].Size.Height + 0.5));
                }

                this[Akt].Pos.X = (float)IsX;
                this[Akt].Pos.Y = (float)IsY;

                // Diese Zeile garantiert, dass immer genau EIN Pixel frei ist zwischen zwei Buchstaben.
                IsX = (float)(IsX + Math.Truncate(this[Akt].Size.Width + 0.5));

                if (this[Akt].isLineBreak()) {
                    IsX = vZBX_Pixel;
                    if (this[Akt] is ExtCharTopCode) {
                        Row_SetOnLine(ZB_Char, Akt);
                        RI.Add(ZB_Char + ";" + Akt);
                    } else {
                        IsY += (int)(Row_SetOnLine(ZB_Char, Akt) * _Zeilenabstand);
                        RI.Add(ZB_Char + ";" + Akt);
                    }
                    ZB_Char = Akt + 1;
                }
            } while (true);

            #region enAlignment berechnen -------------------------------------

            if (Ausrichtung != enAlignment.Top_Left) {
                var KY = 0f;
                if (Ausrichtung.HasFlag(enAlignment.VerticalCenter)) { KY = (float)((_TextDimensions.Height - (int)_Height) / 2.0); }
                if (Ausrichtung.HasFlag(enAlignment.Bottom)) { KY = _TextDimensions.Height - (int)_Height; }
                foreach (var t in RI) {
                    var o = t.SplitAndCutBy(";");
                    var Z1 = int.Parse(o[0]);
                    var Z2 = int.Parse(o[1]);
                    float KX = 0;
                    if (Ausrichtung.HasFlag(enAlignment.Right)) { KX = _TextDimensions.Width - this[Z2].Pos.X - this[Z2].Size.Width; }
                    if (Ausrichtung.HasFlag(enAlignment.HorizontalCenter)) { KX = (_TextDimensions.Width - this[Z2].Pos.X - this[Z2].Size.Width) / 2; }
                    for (var Z3 = Z1; Z3 <= Z2; Z3++) {
                        this[Z3].Pos.X += KX;
                        this[Z3].Pos.Y += KY;
                    }
                }
            }

            #endregion
        }

        private void ResetPosition(bool AndTmpText) {
            _Width = null;
            _Height = null;
            if (AndTmpText) {
                _TMPHtmlText = null;
                _TMPPlainText = null;
            }
            OnChanged();
        }

        private float Row_SetOnLine(int first, int last) {
            float Abstand = 0;
            for (var z = first; z <= last; z++) {
                Abstand = Math.Max(Abstand, this[z].Size.Height);
            }
            for (var z = first; z <= last; z++) {
                if (this[z] is ExtCharTopCode) {
                    this[z].Pos.Y = this[z].Pos.Y + Abstand - this[z].Size.Height;
                }
            }
            return Abstand;
        }

        private int WordBreaker(int AugZeichen, int MinZeichen) {
            if (this.Count == 1) { return 0; }
            if (MinZeichen < 0) { MinZeichen = 0; }
            if (AugZeichen > this.Count - 1) { AugZeichen = this.Count - 1; }
            if (AugZeichen < MinZeichen + 1) { AugZeichen = MinZeichen + 1; }
            // AusnahmeFall auschließen:
            // Space-Zeichen - Dann Buchstabe
            if (this[AugZeichen - 1].isSpace() && !this[AugZeichen].isPossibleLineBreak()) { return AugZeichen; }
            var Started = AugZeichen;
            // Das Letzte Zeichen Search, das kein Trennzeichen ist
            do {
                if (this[AugZeichen].isPossibleLineBreak()) {
                    AugZeichen--;
                } else {
                    break;
                }
                if (AugZeichen <= MinZeichen) { return Started; }
            } while (true);
            do {
                if (this[AugZeichen].isPossibleLineBreak()) { return AugZeichen + 1; }
                AugZeichen--;
                if (AugZeichen <= MinZeichen) { return Started; }
            } while (true);
        }

        #endregion
    }
}
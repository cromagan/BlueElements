// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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

#nullable enable

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.Enums;
using BlueDatabase;
using static BlueBasics.Converter;

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

namespace BlueControls.Extended_Text;

public sealed class ExtText : List<ExtChar>, IChangedFeedback, IDisposableExtended {

    #region Fields

    private readonly RowItem? _row;
    private Design _design;
    private Rectangle _drawingArea;
    private Point _drawingPos;
    private int? _height;
    private States _state;
    private Size _textDimensions;
    private string? _tmpHtmlText;
    private string? _tmpPlainText;
    private int? _width;
    private float _zeilenabstand;

    #endregion

    #region Constructors

    public ExtText(Design design, States state) : base() {
        _design = Design.Undefiniert;
        _state = States.Standard;
        _row = null;
        _drawingPos = new Point(0, 0);
        Ausrichtung = Alignment.Top_Left;
        MaxTextLenght = 4000;
        Multiline = true;
        AllowedChars = string.Empty;
        _drawingArea = new Rectangle(0, 0, -1, -1);
        _textDimensions = Size.Empty;
        _width = null;
        _height = null;
        _zeilenabstand = 1;
        _tmpHtmlText = null;
        _tmpPlainText = null;

        _design = design;
        _state = state;
    }

    public ExtText(PadStyles design, RowItem? skinRow) : this((Design)design, States.Standard) {
        _row = skinRow;

        if ((int)_design < 10000 || _row == null) {
            Develop.DebugPrint(FehlerArt.Fehler, "Fehler!");
        }
    }

    #endregion

    #region Events

    public event EventHandler? Changed;

    #endregion

    #region Properties

    public string AllowedChars { get; set; }

    public Alignment Ausrichtung { get; set; }

    public Design Design {
        get => _design;
        set {
            if (IsDisposed) { return; }
            if (value == _design) { return; }
            _design = value;

            _ = Parallel.ForEach(this, ch => { ch.Design = _design; });
            OnChanged();
        }
    }

    /// <summary>
    /// Falls mit einer Skalierung gezeichnet wird, müssen die Angaben bereits skaliert sein.
    /// </summary>
    public Rectangle DrawingArea {
        get => _drawingArea;
        set => _drawingArea = value;
        //OnChanged();
    }

    /// <summary>
    /// Falls mit einer Skalierung gezeichnet wird, müssen die Angaben bereits skaliert sein.
    /// </summary>
    public Point DrawingPos {
        get => _drawingPos;
        set => _drawingPos = value;
        //OnChanged();
    }

    public string HtmlText {
        get {
            _tmpHtmlText ??= ConvertCharToHtmlText(0, Count - 1);
            return _tmpHtmlText;
        }
        set {
            if (IsDisposed) { return; }
            if (HtmlText == value) { return; }
            ConvertTextToChar(value, true);
            OnChanged();
        }
    }

    public bool IsDisposed { get; private set; }
    public int MaxTextLenght { get; }

    public bool Multiline { get; set; }

    public string PlainText {
        get {
            if (IsDisposed) { return string.Empty; }
            _tmpPlainText ??= ConvertCharToPlainText(0, Count - 1);
            return _tmpPlainText;
        }
        set {
            if (IsDisposed) { return; }
            if (PlainText == value) { return; }
            ConvertTextToChar(value, false);
            OnChanged();
        }
    }

    public States State {
        get => _state;
        set {
            if (IsDisposed) { return; }
            if (value == _state) { return; }
            _state = value;

            try {
                _ = Parallel.ForEach(this, ch => {
                    if (ch != null) { ch.State = _state; }
                });
            } catch { }
            OnChanged();
        }
    }

    /// <summary>
    /// Nach wieviel Pixeln der Zeilenumbruch stattfinden soll. -1 wenn kein Umbruch sein soll. Auch das Alingement richtet sich nach diesen Größen.
    /// </summary>
    public Size TextDimensions {
        get => _textDimensions;
        set {
            if (IsDisposed) { return; }
            if (_textDimensions.Width == value.Width && _textDimensions.Height == value.Height) { return; }
            _textDimensions = value;
            ResetPosition(false);
            OnChanged();
        }
    }

    public float Zeilenabstand {
        get => _zeilenabstand;
        set {
            if (IsDisposed) { return; }
            if (Math.Abs(value - _zeilenabstand) < 0.01) { return; }
            _zeilenabstand = value;
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
        var xDi = double.MaxValue;
        var yDi = double.MaxValue;
        var xNr = -1;
        var yNr = -1;

        do {
            cZ++;
            if (cZ > Count - 1) { break; }// Das Ende des Textes
            if (this[cZ].Size.Width > 0) {
                var matchX = pixX >= _drawingPos.X + this[cZ].Pos.X && pixX <= _drawingPos.X + this[cZ].Pos.X + this[cZ].Size.Width;
                var matchY = pixY >= _drawingPos.Y + this[cZ].Pos.Y && pixY <= _drawingPos.Y + this[cZ].Pos.Y + this[cZ].Size.Height;

                if (matchX && matchY) { return cZ; }

                double tmpDi;
                if (!matchX && matchY) {
                    tmpDi = Math.Abs(pixX - (_drawingPos.X + this[cZ].Pos.X + (this[cZ].Size.Width / 2.0)));
                    if (tmpDi < xDi) {
                        xNr = cZ;
                        xDi = tmpDi;
                    }
                } else if (matchX && !matchY) {
                    tmpDi = Math.Abs(pixY - (_drawingPos.Y + this[cZ].Pos.Y + (this[cZ].Size.Height / 2.0)));
                    if (tmpDi < yDi) {
                        yNr = cZ;
                        yDi = tmpDi;
                    }
                }
            }
        } while (true);

        cZ--;
        return xNr >= 0 ? xNr : yNr >= 0 ? yNr : cZ >= 0 ? cZ : 0;
    }

    public void Check(int first, int last, bool checkstate) {
        for (var cc = first; cc <= last; cc++) {
            if (this[cc].State != States.Undefiniert) {
                if (checkstate) {
                    if (!this[cc].State.HasFlag(States.Checked)) {
                        this[cc].State |= States.Checked;
                    }
                } else {
                    if (this[cc].State.HasFlag(States.Checked)) {
                        this[cc].State ^= States.Checked;
                    }
                }
            }
        }
    }

    public Rectangle CursorPixelPosX(int charPos) {
        while (_width == null) { ReBreak(); }

        if (charPos > Count + 1) { charPos = Count + 1; }
        if (Count > 0 && charPos < 0) { charPos = 0; }
        float x = 0;
        float y = 0;
        float he = 14;

        if (Count == 0) {
            // Kein Text vorhanden
        } else if (charPos < Count) {
            // Cursor vor einem Zeichen
            x = this[charPos].Pos.X;
            y = this[charPos].Pos.Y;
            he = this[charPos].Size.Height;
        } else if (charPos > 0 && charPos < Count + 1 && this[charPos - 1].IsLineBreak()) {
            // Vorzeichen = Zeilenumbruch
            y = this[charPos - 1].Pos.Y + this[charPos - 1].Size.Height;
            he = this[charPos - 1].Size.Height;
        } else if (charPos > 0 && charPos < Count + 1) {
            // Vorzeichen = Echtes Char
            x = this[charPos - 1].Pos.X + this[charPos - 1].Size.Width;
            y = this[charPos - 1].Pos.Y;
            he = this[charPos - 1].Size.Height;
        }
        return new Rectangle((int)x, (int)(y - 1), 0, (int)(he + 2));
    }

    public void Delete(int first, int last) {
        var tempVar = last - first;
        for (var z = 1; z <= tempVar; z++) {
            if (first < Count) {
                RemoveAt(first);
            }
        }
        ResetPosition(true);
    }

    public void Dispose() {
        IsDisposed = true;
        _row?.Dispose();
    }

    public void Draw(Graphics gr, float zoom) {
        while (_width == null) { ReBreak(); }
        DrawStates(gr, zoom);

        foreach (var t in this) {
            if (t.IsVisible(zoom, _drawingPos, _drawingArea)) {
                t.Draw(gr, _drawingPos, zoom);
            }
        }
    }

    public int Height() {
        while (_width == null) { ReBreak(); }

        if (_height == null) { return -1; }
        return (int)_height;
    }

    public bool InsertChar(AsciiKey ascii, int position) {
        if ((int)ascii < 13) { return false; }
        var c = new ExtCharAscii((char)ascii, Design, State, null, 4, MarkState.None);
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
        while (_width == null) { ReBreak(); }
        return _height == null || _width < 5 || _height < 5 ? new Size(32, 16) : new Size((int)_width + 1, (int)_height + 1);
    }

    public void OnChanged() => Changed?.Invoke(this, System.EventArgs.Empty);

    public void StufeÄndern(int first, int last, int stufe) {
        for (var cc = first; cc <= Math.Min(last, Count - 1); cc++) {
            this[cc].Stufe = stufe;
        }
        ResetPosition(true);
    }

    public string Substring(int startIndex, int lenght) => ConvertCharToPlainText(startIndex, startIndex + lenght - 1);

    public int Width() {
        while (_width == null) { ReBreak(); }

        return (int)_width;
    }

    public string Word(int atPosition) {
        var s = WordStart(atPosition);
        var e = WordEnd(atPosition);
        return Substring(s, e - s);
    }

    internal static Size MeasureString(string text, Design design, States state, int width) {
        var x = new ExtText(design, state) {
            HtmlText = text,
            Multiline = true,
            TextDimensions = new Size(width, -1)
        };

        return x.LastSize();
    }

    internal string ConvertCharToPlainText(int first, int last) {
        try {
            var T = new StringBuilder();
            for (var cZ = first; cZ <= Math.Min(last, Count - 1); cZ++) {
                _ = T.Append(this[cZ].PlainText());
            }
            return T.ToString().Replace("\n", string.Empty);
        } catch {
            // Wenn Chars geändert wird (und dann der Count nimmer stimmt)
            Develop.CheckStackForOverflow();
            return ConvertCharToPlainText(first, last);
        }
    }

    internal void InsertCrlf(int position) => Insert(position, new ExtCharCrlfCode());

    internal void Mark(MarkState markstate, int first, int last) {
        try {
            for (var z = first; z <= Math.Min(last, Count - 1); z++) {
                if (!this[z].Marking.HasFlag(markstate)) {
                    this[z].Marking |= markstate;
                }
            }
        } catch {
            Mark(markstate, first, last);
        }
    }

    internal void Unmark(MarkState markstate) {
        foreach (var t in this) {
            if (t.Marking.HasFlag(markstate)) {
                t.Marking ^= markstate;
            }
        }
    }

    internal int WordEnd(int pos) {
        if (Count == 0) { return -1; }
        if (pos < 0 || pos >= Count) { return -1; }
        if (this[pos].IsWordSeperator()) { return -1; }
        do {
            pos++;
            if (pos >= Count) { return Count; }
            if (this[pos].IsWordSeperator()) { return pos; }
        } while (true);
    }

    internal int WordStart(int pos) {
        if (Count == 0) { return -1; }
        if (pos < 0 || pos >= Count) { return -1; }
        if (this[pos].IsWordSeperator()) { return -1; }
        do {
            pos--;
            if (pos < 0) { return 0; }
            if (this[pos].IsWordSeperator()) { return pos + 1; }
        } while (true);
    }

    private string ConvertCharToHtmlText(int first, int last) {
        if (Count == 0) { return string.Empty; }

        var t = new StringBuilder();

        last = Math.Min(last, Count - 1);
        var lastStufe = 4;

        for (var z = first; z <= last; z++) {
            if (lastStufe != this[z].Stufe) {
                _ = t.Append("<H" + this[z].Stufe + ">");
                lastStufe = this[z].Stufe;
            }

            _ = t.Append(this[z].HtmlText());
        }

        return t.ToString();
    }

    private void ConvertTextToChar(string cactext, bool isRich) {
        var pos = 0;
        var zeichen = -1;
        var stufe = 4;
        var markstate = MarkState.None;
        Clear();
        ResetPosition(true);
        var bf = (int)_design > 10000 ? Skin.GetBlueFont((PadStyles)_design, _row) : Skin.GetBlueFont(_design, _state);

        if (!string.IsNullOrEmpty(cactext)) {
            cactext = isRich ? cactext.ConvertFromHtmlToRich() : cactext.Replace("\r\n", "\r");
            var lang = cactext.Length - 1;
            do {
                if (pos > lang) { break; }
                var ch = cactext[pos];
                if (isRich) {
                    switch (ch) {
                        case '<': {
                                DoHtmlCode(cactext, pos, ref zeichen, ref bf, ref stufe, ref markstate);
                                var op = 1;
                                do {
                                    pos++;
                                    if (pos > lang) { break; }
                                    if (cactext[pos] == '>') { op--; }
                                    if (cactext[pos] == '<') { op++; }
                                    if (op == 0) { break; }
                                } while (true);
                                break;
                            }

                        case '&':
                            DoSpecialEntities(cactext, ref pos, ref zeichen, ref bf, ref stufe, ref markstate);
                            break;

                        default:
                            // Normales Zeichen
                            zeichen++;
                            Add(new ExtCharAscii(ch, _design, _state, bf, stufe, markstate));
                            break;
                    }
                } else {
                    // Normales Zeichen
                    zeichen++;
                    Add(new ExtCharAscii(ch, _design, _state, bf, stufe, markstate));
                }
                pos++;
            } while (true);
        }
        ResetPosition(true);
    }

    private void DoHtmlCode(string htmlText, int start, ref int position, ref BlueFont? font, ref int stufe, ref MarkState markState) {
        if (font == null) { return; }  // wenn die Datenbanken entladen wurden bei Programmende

        var endpos = htmlText.IndexOf('>', start + 1);
        if (endpos <= start) {
            Develop.DebugPrint("String-Fehler, > erwartet. " + htmlText);
            return;
        }
        var oricode = htmlText.Substring(start + 1, endpos - start - 1);
        var istgleich = oricode.IndexOf('=');
        string cod;
        string? attribut;
        if (istgleich < 0) {
            // <H4> wird durch autoprüfung zu <H4 >
            cod = oricode.ToUpper().Trim();
            attribut = string.Empty;
        } else {
            cod = oricode.Substring(0, istgleich).Replace(" ", string.Empty).ToUpper().Trim();
            attribut = oricode.Substring(istgleich + 1).Trim('\"');
        }
        switch (cod) {
            case "B":
                font = BlueFont.Get(font.FontName, font.FontSize, true, font.Italic, font.Underline, font.StrikeOut, font.Outline, font.ColorMain, font.ColorOutline, font.Kapitälchen, font.OnlyUpper, font.OnlyLower);
                break;

            case "/B":
                font = BlueFont.Get(font.FontName, font.FontSize, false, font.Italic, font.Underline, font.StrikeOut, font.Outline, font.ColorMain, font.ColorOutline, font.Kapitälchen, font.OnlyUpper, font.OnlyLower);
                break;

            case "I":
                font = BlueFont.Get(font.FontName, font.FontSize, font.Bold, true, font.Underline, font.StrikeOut, font.Outline, font.ColorMain, font.ColorOutline, font.Kapitälchen, font.OnlyUpper, font.OnlyLower);
                break;

            case "/I":
                font = BlueFont.Get(font.FontName, font.FontSize, font.Bold, false, font.Underline, font.StrikeOut, font.Outline, font.ColorMain, font.ColorOutline, font.Kapitälchen, font.OnlyUpper, font.OnlyLower);
                break;

            case "U":
                font = BlueFont.Get(font.FontName, font.FontSize, font.Bold, font.Italic, true, font.StrikeOut, font.Outline, font.ColorMain, font.ColorOutline, font.Kapitälchen, font.OnlyUpper, font.OnlyLower);
                break;

            case "/U":
                font = BlueFont.Get(font.FontName, font.FontSize, font.Bold, font.Italic, false, font.StrikeOut, font.Outline, font.ColorMain, font.ColorOutline, font.Kapitälchen, font.OnlyUpper, font.OnlyLower);
                break;

            case "STRIKE":
                font = BlueFont.Get(font.FontName, font.FontSize, font.Bold, font.Italic, font.Underline, true, font.Outline, font.ColorMain, font.ColorOutline, font.Kapitälchen, font.OnlyUpper, font.OnlyLower);
                break;

            case "/STRIKE":
                font = BlueFont.Get(font.FontName, font.FontSize, font.Bold, font.Italic, font.Underline, false, font.Outline, font.ColorMain, font.ColorOutline, font.Kapitälchen, font.OnlyUpper, font.OnlyLower);
                break;

            case "3":
                font = BlueFont.Get(font.FontName, font.FontSize, font.Bold, font.Italic, font.Underline, font.StrikeOut, true, font.ColorMain, font.ColorOutline, font.Kapitälchen, font.OnlyUpper, font.OnlyLower);
                break;

            case "/3":
                font = BlueFont.Get(font.FontName, font.FontSize, font.Bold, font.Italic, font.Underline, font.StrikeOut, false, font.ColorMain, font.ColorOutline, font.Kapitälchen, font.OnlyUpper, font.OnlyLower);
                break;

            case "FONTSIZE":
                _ = FloatTryParse(attribut, out var fs);
                font = BlueFont.Get(font.FontName, fs, font.Bold, font.Italic, font.Underline, font.StrikeOut, font.Outline, font.ColorMain, font.ColorOutline, font.Kapitälchen, font.OnlyUpper, font.OnlyLower);
                break;

            case "FONTNAME":
                font = BlueFont.Get(attribut, font.FontSize, font.Bold, font.Italic, font.Underline, font.StrikeOut, font.Outline, font.ColorMain, font.ColorOutline, font.Kapitälchen, font.OnlyUpper, font.OnlyLower);
                break;

            case "FONTCOLOR":
                font = BlueFont.Get(font.FontName, font.FontSize, font.Bold, font.Italic, font.Underline, font.StrikeOut, font.Outline, attribut, font.ColorOutline.ToHtmlCode(), font.Kapitälchen, font.OnlyUpper, font.OnlyLower);
                break;

            case "FONTOUTLINE":
                font = BlueFont.Get(font.FontName, font.FontSize, font.Bold, font.Italic, font.Underline, font.StrikeOut, font.Outline, font.ColorMain.ToHtmlCode(), attribut, font.Kapitälchen, font.OnlyUpper, font.OnlyLower);
                break;

            case "HR":
            //    Position++;
            //    this.GenerateAndAdd(new ExtChar(13, _Design, _State, PF, Stufe, MarkState));
            //    Position++;
            //    this.GenerateAndAdd(new ExtChar((int)enEtxtCodes.HorizontalLine, _Design, _State, PF, Stufe, MarkState));
            //    break;
            case "BR":
                position++;
                Add(new ExtCharCrlfCode(_design, _state, font, stufe));
                break;

            case "TAB":
                position++;
                Add(new ExtCharTabCode(_design, _state, font, stufe));
                //this.GenerateAndAdd(new ExtChar((char)9, _Design, _State, font, Stufe, enMarkState.None));
                break;

            case "ZBX_STORE":
                position++;
                Add(new ExtCharStoreXCode(_design, _state, font, stufe));
                break;

            case "TOP":
                position++;
                Add(new ExtCharTopCode(_design, _state, font, stufe));
                break;

            case "IMAGECODE":
                var x = !attribut.Contains("|") ? QuickImage.Get(attribut, (int)font.Oberlänge(1)) : QuickImage.Get(attribut);
                position++;
                Add(new ExtCharImageCode(x, _design, _state, font, stufe));
                break;

            case "H7":
                stufe = 7;
                font = Skin.GetBlueFont((int)_design, _state, _row, stufe);
                break;

            case "H6":
                stufe = 6;
                font = Skin.GetBlueFont((int)_design, _state, _row, stufe);
                break;

            case "H5":
                stufe = 5;
                font = Skin.GetBlueFont((int)_design, _state, _row, stufe);
                break;

            case "H4":
                stufe = 4;
                font = Skin.GetBlueFont((int)_design, _state, _row, stufe);
                break;

            case "H3":
                stufe = 3;
                font = Skin.GetBlueFont((int)_design, _state, _row, stufe);
                break;

            case "H2":
                stufe = 2;
                font = Skin.GetBlueFont((int)_design, _state, _row, stufe);
                break;

            case "H1":
                stufe = 1;
                font = Skin.GetBlueFont((int)_design, _state, _row, stufe);
                break;

            case "MARKSTATE":
                markState = (MarkState)IntParse(attribut);
                break;

            case "":
                // ist evtl. ein <> ausruck eines Textes
                break;
        }
    }

    private void DoSpecialEntities(string xHtmlTextx, ref int xStartPosx, ref int xPosition, ref BlueFont? f, ref int stufe, ref MarkState markState) {
        var endpos = xHtmlTextx.IndexOf(';', xStartPosx + 1);
        xPosition++;
        if (endpos <= xStartPosx || endpos > xStartPosx + 10) {
            // Ein nicht konvertiertes &, einfach so übernehmen.
            Add(new ExtCharAscii('&', _design, _state, f, stufe, markState));
            return;
        }
        switch (xHtmlTextx.Substring(xStartPosx, endpos - xStartPosx + 1)) {
            case "&uuml;":
                Add(new ExtCharAscii('ü', _design, _state, f, stufe, markState));
                break;

            case "&auml;":
                Add(new ExtCharAscii('ä', _design, _state, f, stufe, markState));
                break;

            case "&ouml;":
                Add(new ExtCharAscii('ö', _design, _state, f, stufe, markState));
                break;

            case "&Uuml;":
                Add(new ExtCharAscii('Ü', _design, _state, f, stufe, markState));
                break;

            case "&Auml;":
                Add(new ExtCharAscii('Ä', _design, _state, f, stufe, markState));
                break;

            case "&Ouml;":
                Add(new ExtCharAscii('Ö', _design, _state, f, stufe, markState));
                break;

            case "&szlig;":
                Add(new ExtCharAscii('ß', _design, _state, f, stufe, markState));
                break;

            case "&quot;":
                Add(new ExtCharAscii('\"', _design, _state, f, stufe, markState));
                break;

            case "&amp;":
                Add(new ExtCharAscii('&', _design, _state, f, stufe, markState));
                break;

            case "&lt;":
                Add(new ExtCharAscii('<', _design, _state, f, stufe, markState));
                break;

            case "&gt;":
                Add(new ExtCharAscii('>', _design, _state, f, stufe, markState));
                break;

            case "&Oslash;":
                Add(new ExtCharAscii('Ø', _design, _state, f, stufe, markState));
                break;

            case "&oslash;":
                Add(new ExtCharAscii('ø', _design, _state, f, stufe, markState));
                break;

            case "&bull;":
                Add(new ExtCharAscii('•', _design, _state, f, stufe, markState));
                break;

            case "&eacute;":
                Add(new ExtCharAscii('é', _design, _state, f, stufe, markState));
                break;

            case "&Eacute;":
                Add(new ExtCharAscii('É', _design, _state, f, stufe, markState));
                break;

            case "&euro;":
                Add(new ExtCharAscii('€', _design, _state, f, stufe, markState));
                break;

            default:
                Develop.DebugPrint(FehlerArt.Info, "Unbekannter Code: " + xHtmlTextx.Substring(xStartPosx, endpos - xStartPosx + 1));
                Add(new ExtCharAscii('&', _design, _state, f, stufe, markState));
                return;
        }
        xStartPosx = endpos;
    }

    private void DrawState(Graphics gr, float czoom, MarkState state) {
        var tmas = -1;
        for (var pos = 0; pos < Count; pos++) {
            var tempVar = this[pos];
            var marked = tempVar.Marking.HasFlag(state);

            if (marked && tmas < 0) { tmas = pos; }

            if (!marked || pos == Count - 1) {
                if (tmas > -1) {
                    if (pos == Count - 1) {
                        DrawZone(gr, czoom, state, tmas, pos);
                    } else {
                        DrawZone(gr, czoom, state, tmas, pos - 1);
                    }
                    tmas = -1;
                }
            }
        }
    }

    private void DrawStates(Graphics gr, float czoom) {
        DrawState(gr, czoom, MarkState.Field);
        DrawState(gr, czoom, MarkState.MyOwn);
        DrawState(gr, czoom, MarkState.Other);
        DrawState(gr, czoom, MarkState.Ringelchen);
    }

    private void DrawZone(Graphics gr, float czoom, MarkState thisState, int markStart, int markEnd) {
        var startX = (this[markStart].Pos.X * czoom) + _drawingPos.X;
        var startY = (this[markStart].Pos.Y * czoom) + _drawingPos.Y;
        var endX = (this[markEnd].Pos.X * czoom) + _drawingPos.X + (this[markEnd].Size.Width * czoom);
        var endy = (this[markEnd].Pos.Y * czoom) + _drawingPos.Y + (this[markEnd].Size.Height * czoom);

        switch (thisState) {
            case MarkState.None:
                break;

            case MarkState.Ringelchen:
                gr.DrawLine(new Pen(Color.Red, 3 * czoom), startX, (int)(startY + (this[markStart].Size.Height * czoom * 0.9)), endX, (int)(startY + (this[markStart].Size.Height * czoom * 0.9)));
                break;

            case MarkState.Field:
                gr.FillRectangle(new SolidBrush(Color.FromArgb(80, 128, 128, 128)), startX, startY, endX - startX, endy - startY);
                break;

            case MarkState.MyOwn:
                gr.FillRectangle(new SolidBrush(Color.FromArgb(40, 50, 255, 50)), startX, startY, endX - startX, endy - startY);
                break;

            case MarkState.Other:
                gr.FillRectangle(new SolidBrush(Color.FromArgb(80, 255, 255, 50)), startX, startY, endX - startX, endy - startY);
                break;

            default:
                Develop.DebugPrint(thisState);
                break;
        }
    }

    private new void Insert(int position, ExtChar c) {
        if (position < 0) { position = 0; }
        if (position > Count) { position = Count; }

        ExtChar? style = null;

        if (position < Count) {
            style = this[position];
        } else if (Count > 0) {
            style = this[Count - 1];
        }

        if (style != null) { c.GetStyleFrom(c); }

        base.Insert(position, c);
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
    /// Berechnet die Zeichen-Positionen mit korrekten Umbrüchen. Die enAlignment wird ebenfalls mit eingerechnet.
    /// Cursor_ComputePixelXPos wird am Ende aufgerufen, einschließlich Cursor_Repair und SetNewAkt.
    /// </summary>
    /// <remarks></remarks>
    private void ReBreak() {
        _width = 0;
        _height = 0;
        if (Count == 0) { return; }

        List<string> ri = new();
        var vZbxPixel = 0f;
        var isX = 0f;
        var isY = 0f;
        var zbChar = 0;
        var akt = -1;

        do {
            akt++;
            if (akt > Count - 1) {
                _ = Row_SetOnLine(zbChar, akt - 1);
                ri.Add(zbChar + ";" + (akt - 1));
                break;
            }

            if (this[akt] is ExtCharStoreXCode) {
                vZbxPixel = isX;
            } else if (this[akt] is ExtCharTopCode) {
                isY = 0;
            }

            if (!this[akt].IsSpace()) {
                if (akt > zbChar && _textDimensions.Width > 0) {
                    if (isX + this[akt].Size.Width + 0.5 > _textDimensions.Width) {
                        akt = WordBreaker(akt, zbChar);
                        isX = vZbxPixel;
                        isY += Row_SetOnLine(zbChar, akt - 1) * _zeilenabstand;
                        ri.Add(zbChar + ";" + (akt - 1));
                        zbChar = akt;
                    }
                }
                _width = Math.Max((int)_width, (int)(isX + this[akt].Size.Width + 0.5));
                _height = Math.Max((int)_height, (int)(isY + this[akt].Size.Height + 0.5));
            }

            this[akt].Pos.X = isX;
            this[akt].Pos.Y = isY;

            // Diese Zeile garantiert, dass immer genau EIN Pixel frei ist zwischen zwei Buchstaben.
            isX = (float)(isX + Math.Truncate(this[akt].Size.Width + 0.5));

            if (this[akt].IsLineBreak()) {
                isX = vZbxPixel;
                if (this[akt] is ExtCharTopCode) {
                    _ = Row_SetOnLine(zbChar, akt);
                    ri.Add(zbChar + ";" + akt);
                } else {
                    isY += (int)(Row_SetOnLine(zbChar, akt) * _zeilenabstand);
                    ri.Add(zbChar + ";" + akt);
                }
                zbChar = akt + 1;
            }
        } while (true);

        #region enAlignment berechnen -------------------------------------

        if (Ausrichtung != Alignment.Top_Left) {
            var ky = 0f;
            if (Ausrichtung.HasFlag(Alignment.VerticalCenter)) { ky = (float)((_textDimensions.Height - (int)_height) / 2.0); }
            if (Ausrichtung.HasFlag(Alignment.Bottom)) { ky = _textDimensions.Height - (int)_height; }
            foreach (var t in ri) {
                var o = t.SplitAndCutBy(";");
                var z1 = IntParse(o[0]);
                var z2 = IntParse(o[1]);
                float kx = 0;
                if (Ausrichtung.HasFlag(Alignment.Right)) { kx = _textDimensions.Width - this[z2].Pos.X - this[z2].Size.Width; }
                if (Ausrichtung.HasFlag(Alignment.HorizontalCenter)) { kx = (_textDimensions.Width - this[z2].Pos.X - this[z2].Size.Width) / 2; }
                for (var z3 = z1; z3 <= z2; z3++) {
                    this[z3].Pos.X += kx;
                    this[z3].Pos.Y += ky;
                }
            }
        }

        #endregion
    }

    private void ResetPosition(bool andTmpText) {
        if (IsDisposed) { return; }
        _width = null;
        _height = null;

        if (andTmpText) {
            _tmpHtmlText = null;
            _tmpPlainText = null;
        }
        OnChanged();
    }

    private float Row_SetOnLine(int first, int last) {
        float abstand = 0;
        for (var z = first; z <= last; z++) {
            abstand = Math.Max(abstand, this[z].Size.Height);
        }
        for (var z = first; z <= last; z++) {
            if (this[z] is ExtCharTopCode) {
                this[z].Pos.Y = this[z].Pos.Y + abstand - this[z].Size.Height;
            }
        }
        return abstand;
    }

    private int WordBreaker(int augZeichen, int minZeichen) {
        if (Count == 1) { return 0; }
        if (minZeichen < 0) { minZeichen = 0; }
        if (augZeichen > Count - 1) { augZeichen = Count - 1; }
        if (augZeichen < minZeichen + 1) { augZeichen = minZeichen + 1; }
        // AusnahmeFall auschließen:
        // Space-Zeichen - Dann Buchstabe
        if (this[augZeichen - 1].IsSpace() && !this[augZeichen].IsPossibleLineBreak()) { return augZeichen; }
        var started = augZeichen;
        // Das Letzte Zeichen Search, das kein Trennzeichen ist
        do {
            if (this[augZeichen].IsPossibleLineBreak()) {
                augZeichen--;
            } else {
                break;
            }
            if (augZeichen <= minZeichen) { return started; }
        } while (true);
        do {
            if (this[augZeichen].IsPossibleLineBreak()) { return augZeichen + 1; }
            augZeichen--;
            if (augZeichen <= minZeichen) { return started; }
        } while (true);
    }

    #endregion
}
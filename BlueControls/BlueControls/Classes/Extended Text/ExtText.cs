// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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

using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.Enums;
using BlueControls.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;
using static BlueBasics.Converter;
using Color = System.Drawing.Color;
using Pen = System.Drawing.Pen;

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

public sealed class ExtText : List<ExtChar>, INotifyPropertyChanged, IDisposableExtended, IStyleable {

    #region Fields

    private int? _height;
    private string _sheetStyle = string.Empty;
    private Size _textDimensions;

    private string? _tmpHtmlText;

    private string? _tmpPlainText;

    private int? _width;

    private float _zeilenabstand;

    #endregion

    #region Constructors

    public ExtText() : base() {
        DrawingPos = new Point(0, 0);
        Ausrichtung = Alignment.Top_Left;
        MaxTextLenght = 4000;
        Multiline = true;
        AllowedChars = string.Empty;
        DrawingArea = new Rectangle(0, 0, -1, -1);
        _textDimensions = Size.Empty;
        _width = null;
        _height = null;
        _zeilenabstand = 1;
        _tmpHtmlText = null;
        _tmpPlainText = null;
    }

    public ExtText(Design design, States state) : this() {
        var sh = Skin.DesignOf(design, state);
        StyleBeginns = sh.Style;
        _sheetStyle = Constants.Win11;
    }

    public ExtText(string sheetStyle, PadStyles stylebeginns) : this() {
        _sheetStyle = sheetStyle;
        StyleBeginns = stylebeginns;
    }

    #endregion

    #region Events

    public event PropertyChangedEventHandler? PropertyChanged;

    public event EventHandler? StyleChanged;

    #endregion

    #region Properties

    public string AllowedChars { get; set; }    // Todo: Implementieren
    public Alignment Ausrichtung { get; set; }

    /// <summary>
    /// Falls mit einer Skalierung gezeichnet wird, müssen die Angaben bereits skaliert sein.
    /// </summary>
    public Rectangle DrawingArea { get; set; }

    /// <summary>
    /// Falls mit einer Skalierung gezeichnet wird, müssen die Angaben bereits skaliert sein.
    /// </summary>
    public Point DrawingPos { get; set; }

    public int Height {
        get {
            EnsurePositions();
            return _height ?? -1;
        }
    }

    public string HtmlText {
        get {
            _tmpHtmlText ??= ConvertCharToHtmlText();
            return _tmpHtmlText;
        }
        set {
            if (IsDisposed) { return; }
            if (HtmlText == value) { return; }
            ConvertTextToChar(value, true);
            OnPropertyChanged();
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
            OnPropertyChanged();
        }
    }

    public string SheetStyle {
        get => _sheetStyle;
        set {
            if (IsDisposed || _sheetStyle == value) { return; }
            _sheetStyle = value;
            OnStyleChanged();
            DoQuickFont();
            OnPropertyChanged();
        }
    }

    public PadStyles StyleBeginns { get; set; } = PadStyles.Standard;

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
            OnPropertyChanged();
        }
    }

    public int Width {
        get {
            EnsurePositions();
            return _width ?? 0;
        }
    }

    public float Zeilenabstand {
        get => _zeilenabstand;
        set {
            if (IsDisposed) { return; }
            if (Math.Abs(value - _zeilenabstand) < 0.01) { return; }
            _zeilenabstand = value;
            ResetPosition(false);
            OnPropertyChanged();
        }
    }

    #endregion

    #region Methods

    public void ChangeStyle(int first, int last, PadStyles style) {
        var changed = false;
        for (var cc = first; cc <= Math.Min(last, Count - 1); cc++) {
            if (this[cc].Style != style) {
                this[cc].Style = style;
                changed = true;
            }
        }
        if (changed) {
            ResetPosition(true);
        }
    }

    /// <summary>
    ///     Sucht den aktuellen Buchstaben, der unter den angegeben Koordinaten liegt.
    ///     Wird kein Char gefunden, wird der logischste Char gewählt. (z.B. Nach ZeilenEnde = Letzzter Buchstabe der Zeile)
    /// </summary>
    /// <remarks></remarks>
    public int Char_Search(double pixX, double pixY) {
        EnsurePositions();

        var cZ = -1;
        var xDi = double.MaxValue;
        var yDi = double.MaxValue;
        var xNr = -1;
        var yNr = -1;

        do {
            cZ++;
            if (cZ > Count - 1) { break; }// Das Ende des Textes
            if (this[cZ].Size.Width > 0) {
                var matchX = pixX >= DrawingPos.X + this[cZ].Pos.X && pixX <= DrawingPos.X + this[cZ].Pos.X + this[cZ].Size.Width;
                var matchY = pixY >= DrawingPos.Y + this[cZ].Pos.Y && pixY <= DrawingPos.Y + this[cZ].Pos.Y + this[cZ].Size.Height;

                if (matchX && matchY) { return cZ; }

                double tmpDi;
                if (!matchX && matchY) {
                    tmpDi = Math.Abs(pixX - (DrawingPos.X + this[cZ].Pos.X + (this[cZ].Size.Width / 2.0)));
                    if (tmpDi < xDi) {
                        xNr = cZ;
                        xDi = tmpDi;
                    }
                } else if (matchX && !matchY) {
                    tmpDi = Math.Abs(pixY - (DrawingPos.Y + this[cZ].Pos.Y + (this[cZ].Size.Height / 2.0)));
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

    public Rectangle CursorPixelPosX(int charPos) {
        EnsurePositions();

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

    public void Dispose() => IsDisposed = true;

    public void Draw(Graphics gr, float zoom) {
        EnsurePositions();
        DrawStates(gr, zoom);

        foreach (var t in this) {
            if (t.IsVisible(zoom, DrawingPos, DrawingArea)) {
                t.Draw(gr, DrawingPos, zoom);
            }
        }
    }

    public bool InsertChar(AsciiKey ascii, int position) {
        if ((int)ascii < 13) { return false; }
        var c = new ExtCharAscii(this, position, (char)ascii);
        Insert(position, c);
        return true;
    }

    public bool InsertImage(string imagecode, int position) {
        if (string.IsNullOrEmpty(imagecode)) { return false; }
        var c = new ExtCharImageCode(this, position, imagecode);
        Insert(position, c);
        return true;
    }

    public Size LastSize() {
        EnsurePositions();
        return _height == null || _width < 5 || _height < 5 ? new Size(32, 16) : new Size((int)_width + 1, (int)_height + 1);
    }

    public void OnStyleChanged() => StyleChanged?.Invoke(this, System.EventArgs.Empty);

    public string Substring(int startIndex, int lenght) => ConvertCharToPlainText(startIndex, startIndex + lenght - 1);

    public string Word(int atPosition) {
        var s = WordStart(atPosition);
        var e = WordEnd(atPosition);
        return s == -1 || e == -1 ? string.Empty : Substring(s, e - s);
    }

    internal string ConvertCharToPlainText(int first, int last) {
        try {
            var _stringBuilder = new StringBuilder(1024);
            for (var cZ = first; cZ <= Math.Min(last, Count - 1); cZ++) {
                _stringBuilder.Append(this[cZ].PlainText());
            }
            return _stringBuilder.ToString().Replace("\n", string.Empty);
        } catch {
            // Wenn Chars geändert wird (und dann der Count nimmer stimmt)
            Develop.CheckStackOverflow();
            return ConvertCharToPlainText(first, last);
        }
    }

    internal void InsertCrlf(int position) => Insert(position, new ExtCharCrlfCode(this, position));

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
        // Frühe Validierung und Abbruch
        if (Count == 0 || pos < 0 || pos >= Count || this[pos].IsWordSeparator()) {
            return -1;
        }

        // Direkte Suche ohne while-Schleife
        while (++pos < Count) {
            if (this[pos].IsWordSeparator()) {
                return pos;
            }
        }

        return Count;
    }

    internal int WordStart(int pos) {
        // Frühe Validierung und Abbruch
        if (Count == 0 || pos < 0 || pos >= Count || this[pos].IsWordSeparator()) {
            return -1;
        }

        // Direkte Suche ohne while-Schleife
        while (--pos >= 0) {
            if (this[pos].IsWordSeparator()) {
                return pos + 1;
            }
        }

        return 0;
    }

    private int AddSpecialEntities(string htmltext, int position, PadStyles style, BlueFont font) {
        var endpos = htmltext.IndexOf(';', position + 1);

        if (endpos <= position || endpos > position + 10) {
            Add(new ExtCharAscii(this, style, font, '&'));
            return position + 1;
        }

        var entity = htmltext.Substring(position, endpos - position + 1);
        if (Constants.ReverseHtmlEntities.TryGetValue(entity, out var c)) {
            Add(new ExtCharAscii(this, style, font, c));
            return endpos;
        }

        Develop.DebugPrint(ErrorType.Info, "Unbekannter Code: " + entity);
        Add(new ExtCharAscii(this, style, font, '&'));
        return position + 1;
    }

    private string AppendStyle(PadStyles ls, ExtChar newStufe) {
        if (ls == newStufe.Style) { return string.Empty; }

        if (ls == PadStyles.Standard && newStufe.Style == PadStyles.Überschrift) { return "<h3>"; }
        if (ls == PadStyles.Überschrift && newStufe.Style == PadStyles.Standard) { return "</h3>"; }
        if (ls == PadStyles.Standard && newStufe.Style == PadStyles.Hervorgehoben) { return "<strong>"; }
        if (ls == PadStyles.Hervorgehoben && newStufe.Style == PadStyles.Standard) { return "</strong>"; }
        if (ls == PadStyles.Hervorgehoben && newStufe.Style == PadStyles.Überschrift) { return "</strong><h3>"; }
        if (ls == PadStyles.Überschrift && newStufe.Style == PadStyles.Hervorgehoben) { return "</h3><strong>"; }

        return string.Empty;
    }

    private string ConvertCharToHtmlText() {
        if (Count == 0) { return string.Empty; }

        // Ungefähre Größe vorallokieren - reduziert Reallokationen
        var _stringBuilder = new StringBuilder(Count * 3);

        var lastStufe = PadStyles.Standard;

        for (var z = 0; z < Count; z++) {
            var s = AppendStyle(lastStufe, this[z]);
            if (!string.IsNullOrEmpty(s)) { _stringBuilder.Append(s); }

            lastStufe = this[z].Style;
            _stringBuilder.Append(this[z].HtmlText());
        }
        return _stringBuilder.ToString();
    }

    private void ConvertTextToChar(string cactext, bool isRich) {
        if (string.IsNullOrEmpty(cactext)) {
            Clear();
            ResetPosition(true);
            return;
        }

        // Vorverarbeitung des Texts
        cactext = isRich ? cactext.ConvertFromHtmlToRich() : cactext.Replace("\r\n", "\r");

        Clear();
        Capacity = Math.Max(Capacity, cactext.Length); // Pre-allocate capacity
        ResetPosition(true);
        var style = StyleBeginns;
        var font = Skin.GetBlueFont(SheetStyle, StyleBeginns);

        // StringBuilder für temporäre String-Operationen
        var temp = new StringBuilder(100);
        var pos = 0;
        var zeichen = -1;

        while (pos < cactext.Length) {
            var ch = cactext[pos];

            if (isRich) {
                switch (ch) {
                    case '<':
                        if (temp.Length > 0) {
                            zeichen++;
                            Add(new ExtCharAscii(this, style, font, temp.ToString()[0]));
                            temp.Clear();
                        }
                        // HTML-Code verarbeiten
                        DoHtmlCode(cactext, pos, ref zeichen, ref font, ref style);
                        // Position zum Ende des HTML-Tags bewegen
                        var endTag = cactext.IndexOf('>', pos + 1);
                        pos = endTag != -1 ? endTag : cactext.Length;
                        break;

                    case '&':
                        if (temp.Length > 0) {
                            zeichen++;
                            Add(new ExtCharAscii(this, style, font, temp.ToString()[0]));
                            temp.Clear();
                        }
                        pos = AddSpecialEntities(cactext, pos, style, font);
                        zeichen++;
                        break;

                    default:
                        zeichen++;
                        Add(new ExtCharAscii(this, style, font, ch));
                        break;
                }
            } else {
                zeichen++;
                Add(new ExtCharAscii(this, style, font, ch));
            }
            pos++;
        }

        ResetPosition(true);
    }

    private void DoHtmlCode(string htmlText, int start, ref int position, ref BlueFont font, ref PadStyles style) {
        var endpos = htmlText.IndexOf('>', start + 1);
        if (endpos <= start) {
            //Develop.DebugPrint("String-Fehler, > erwartet. " + htmlText);
            return;
        }
        var oricode = htmlText.Substring(start + 1, endpos - start - 1);
        var istgleich = oricode.IndexOf('=');
        string cod;
        string? attribut;
        if (istgleich < 0) {
            // <H4> wird durch autoprüfung zu <H4 >
            cod = oricode.ToUpperInvariant().Trim();
            attribut = string.Empty;
        } else {
            cod = oricode.Substring(0, istgleich).Replace(" ", string.Empty).ToUpperInvariant().Trim();
            attribut = oricode.Substring(istgleich + 1).Trim('\"');
        }

        switch (cod) {
            case "B":
                style = PadStyles.Undefiniert;
                font = BlueFont.Get(font.FontName, font.Size, true, font.Italic, font.Underline, font.StrikeOut, font.ColorMain, font.ColorOutline, font.Kapitälchen, font.OnlyUpper, font.OnlyLower, font.ColorBack);
                break;

            case "/B":
                style = PadStyles.Undefiniert;
                font = BlueFont.Get(font.FontName, font.Size, false, font.Italic, font.Underline, font.StrikeOut, font.ColorMain, font.ColorOutline, font.Kapitälchen, font.OnlyUpper, font.OnlyLower, font.ColorBack);
                break;

            case "I":
                style = PadStyles.Undefiniert;
                font = BlueFont.Get(font.FontName, font.Size, font.Bold, true, font.Underline, font.StrikeOut, font.ColorMain, font.ColorOutline, font.Kapitälchen, font.OnlyUpper, font.OnlyLower, font.ColorBack);
                break;

            case "/I":
                style = PadStyles.Undefiniert;
                font = BlueFont.Get(font.FontName, font.Size, font.Bold, false, font.Underline, font.StrikeOut, font.ColorMain, font.ColorOutline, font.Kapitälchen, font.OnlyUpper, font.OnlyLower, font.ColorBack);
                break;

            case "U":
                style = PadStyles.Undefiniert;
                font = BlueFont.Get(font.FontName, font.Size, font.Bold, font.Italic, true, font.StrikeOut, font.ColorMain, font.ColorOutline, font.Kapitälchen, font.OnlyUpper, font.OnlyLower, font.ColorBack);
                break;

            case "/U":
                style = PadStyles.Undefiniert;
                font = BlueFont.Get(font.FontName, font.Size, font.Bold, font.Italic, false, font.StrikeOut, font.ColorMain, font.ColorOutline, font.Kapitälchen, font.OnlyUpper, font.OnlyLower, font.ColorBack);
                break;

            case "STRIKE":
                style = PadStyles.Undefiniert;
                font = BlueFont.Get(font.FontName, font.Size, font.Bold, font.Italic, font.Underline, true, font.ColorMain, font.ColorOutline, font.Kapitälchen, font.OnlyUpper, font.OnlyLower, font.ColorBack);
                break;

            case "/STRIKE":
                style = PadStyles.Undefiniert;
                font = BlueFont.Get(font.FontName, font.Size, font.Bold, font.Italic, font.Underline, false, font.ColorMain, font.ColorOutline, font.Kapitälchen, font.OnlyUpper, font.OnlyLower, font.ColorBack);
                break;

            //case "3":
            //    style = PadStyles.Undefiniert;
            //    font = BlueFont.Get(font.FontName, font.Size, font.Bold, font.Italic, font.Underline, font.StrikeOut, true, font.ColorMain, font.ColorOutline, font.Kapitälchen, font.OnlyUpper, font.OnlyLower, font.ColorBack);
            //    break;

            //case "/3":
            //    style = PadStyles.Undefiniert;
            //    font = BlueFont.Get(font.FontName, font.Size, font.Bold, font.Italic, font.Underline, font.StrikeOut, false, font.ColorMain, font.ColorOutline, font.Kapitälchen, font.OnlyUpper, font.OnlyLower, font.ColorBack);
            //break;

            case "FONTSIZE":
                style = PadStyles.Undefiniert;
                _ = FloatTryParse(attribut, out var fs);
                font = BlueFont.Get(font.FontName, fs, font.Bold, font.Italic, font.Underline, font.StrikeOut, font.ColorMain, font.ColorOutline, font.Kapitälchen, font.OnlyUpper, font.OnlyLower, font.ColorBack);
                break;

            case "FONTNAME":
                style = PadStyles.Undefiniert;
                font = BlueFont.Get(attribut, font.Size, font.Bold, font.Italic, font.Underline, font.StrikeOut, font.ColorMain, font.ColorOutline, font.Kapitälchen, font.OnlyUpper, font.OnlyLower, font.ColorBack);
                break;

            case "FONTCOLOR":
                style = PadStyles.Undefiniert;
                font = BlueFont.Get(font.FontName, font.Size, font.Bold, font.Italic, font.Underline, font.StrikeOut, ColorParse(attribut), font.ColorOutline, font.Kapitälchen, font.OnlyUpper, font.OnlyLower, font.ColorBack);
                break;

            case "COLOROUTLINE":
            case "FONTOUTLINE":
                style = PadStyles.Undefiniert;
                font = BlueFont.Get(font.FontName, font.Size, font.Bold, font.Italic, font.Underline, font.StrikeOut, font.ColorMain, ColorParse(attribut), font.Kapitälchen, font.OnlyUpper, font.OnlyLower, font.ColorBack);
                break;

            case "HR":
            //    Position++;
            //    this.GenerateAndAdd(new ExtChar(13, _Design, _State, PF, Stufe, MarkState));
            //    Position++;
            //    this.GenerateAndAdd(new ExtChar((int)enEtxtCodes.HorizontalLine, _Design, _State, PF, Stufe, MarkState));
            //    break;
            case "BR":
                position++;
                Add(new ExtCharCrlfCode(this, style, font));
                break;

            case "TAB":
                position++;
                Add(new ExtCharTabCode(this, style, font));
                break;

            case "ZBX_STORE":
                position++;
                Add(new ExtCharStoreXCode(this, style, font));
                break;

            case "TOP":
                position++;
                Add(new ExtCharTopCode(this, style, font));
                break;

            case "IMAGECODE":
                var x = !attribut.Contains("|") ? QuickImage.Get(attribut, (int)font.Oberlänge(1)) : QuickImage.Get(attribut);
                position++;
                Add(new ExtCharImageCode(this, style, font, x));
                break;

            case "H7":
                style = PadStyles.Hervorgehoben;
                font = Skin.GetBlueFont(_sheetStyle, style);
                break;

            case "H6":
                style = PadStyles.Alternativ;
                font = Skin.GetBlueFont(_sheetStyle, style);
                break;

            case "H5":
                style = PadStyles.Kleiner_Zusatz;
                font = Skin.GetBlueFont(_sheetStyle, style);
                break;

            case "STRONG":
                style = PadStyles.Hervorgehoben;
                font = Skin.GetBlueFont(_sheetStyle, style);
                break;

            case "/STRONG":
            case "/H3":
            case "P":
            case "H0":
            case "H4":
                style = PadStyles.Standard;
                font = Skin.GetBlueFont(_sheetStyle, style);
                break;

            case "H3":
                style = PadStyles.Kapitel;
                font = Skin.GetBlueFont(_sheetStyle, style);
                break;

            case "H2":
                style = PadStyles.Untertitel;
                font = Skin.GetBlueFont(_sheetStyle, style);
                break;

            case "H1":
                style = PadStyles.Überschrift;
                font = Skin.GetBlueFont(_sheetStyle, style);
                break;

            //case "MARKSTATE":
            //    markState = (MarkState)IntParse(attribut);
            //    break;

            case "":
                // ist evtl. ein <> ausruck eines Textes
                break;
        }
    }

    private void DoQuickFont() {
        var last = PadStyles.Undefiniert;
        var f = BlueFont.DefaultFont;

        foreach (var thisChar in this) {
            if (thisChar.Style != last) {
                last = thisChar.Style;
                f = thisChar.Font;
            } else {
                thisChar.Font = f;
            }
        }
    }

    private void DrawState(Graphics gr, float scale, MarkState state) {
        var tmas = -1;
        for (var pos = 0; pos < Count; pos++) {
            var tempVar = this[pos];
            var marked = tempVar.Marking.HasFlag(state);

            if (marked && tmas < 0) { tmas = pos; }

            if (!marked || pos == Count - 1) {
                if (tmas > -1) {
                    if (pos == Count - 1) {
                        DrawZone(gr, scale, state, tmas, pos);
                    } else {
                        DrawZone(gr, scale, state, tmas, pos - 1);
                    }
                    tmas = -1;
                }
            }
        }
    }

    private void DrawStates(Graphics gr, float scale) {
        DrawState(gr, scale, MarkState.Field);
        DrawState(gr, scale, MarkState.MyOwn);
        DrawState(gr, scale, MarkState.Other);
        DrawState(gr, scale, MarkState.Ringelchen);
    }

    private void DrawZone(Graphics gr, float scale, MarkState thisState, int markStart, int markEnd) {
        var startX = (this[markStart].Pos.X * scale) + DrawingPos.X;
        var startY = (this[markStart].Pos.Y * scale) + DrawingPos.Y;
        var endX = (this[markEnd].Pos.X * scale) + DrawingPos.X + (this[markEnd].Size.Width * scale);
        var endy = (this[markEnd].Pos.Y * scale) + DrawingPos.Y + (this[markEnd].Size.Height * scale);

        switch (thisState) {
            case MarkState.None:
                break;

            case MarkState.Ringelchen:
                using (var pen = new Pen(Color.Red, 3 * scale)) {
                    gr.DrawLine(pen, startX, (int)(startY + (this[markStart].Size.Height * scale * 0.9)), endX, (int)(startY + (this[markStart].Size.Height * scale * 0.9)));
                }
                break;

            case MarkState.Field:
                using (var brush = new SolidBrush(Color.FromArgb(80, 128, 128, 128))) {
                    gr.FillRectangle(brush, startX, startY, endX - startX, endy - startY);
                }
                break;

            case MarkState.MyOwn:
                using (var brush = new SolidBrush(Color.FromArgb(40, 50, 255, 50))) {
                    gr.FillRectangle(brush, startX, startY, endX - startX, endy - startY);
                }
                break;

            case MarkState.Other:
                using (var brush = new SolidBrush(Color.FromArgb(80, 255, 255, 50))) {
                    gr.FillRectangle(brush, startX, startY, endX - startX, endy - startY);
                }
                break;

            default:
                Develop.DebugPrint(thisState);
                break;
        }
    }

    private void EnsurePositions() {
        if (_width == null) {
            ReBreak();
        }
    }

    private new void Insert(int position, ExtChar c) {
        if (position < 0) { position = 0; }
        if (position > Count) { position = Count; }

        base.Insert(position, c);
        ResetPosition(true);
    }

    private void OnPropertyChanged([CallerMemberName] string propertyName = "unknown") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    /// <summary>
    /// Berechnet die Zeichen-Positionen mit korrekten Umbrüchen. Die enAlignment wird ebenfalls mit eingerechnet.
    /// Cursor_ComputePixelXPos wird am Ende aufgerufen, einschließlich Cursor_Repair und SetNewAkt.
    /// </summary>
    /// <remarks></remarks>
    private void ReBreak() {
        _width = 0;
        _height = 0;
        if (Count == 0) { return; }

        var estimatedRows = Count / 50; // Geschätzte Zeilenanzahl
        var ri = new List<string>(estimatedRows);
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
        OnPropertyChanged("Position");
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
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

public sealed class ExtText : INotifyPropertyChanged, IDisposableExtended, IStyleable {

    #region Fields

    private readonly List<ExtChar> _internal = [];
    private int? _heightControl;
    private string _sheetStyle = Constants.Win11;

    private Size _textDimensions;

    private string? _tmpHtmlText;

    private string? _tmpPlainText;

    private int? _widthControl;

    private float _zeilenabstand;

    #endregion

    #region Constructors

    public ExtText() : base() {
        Ausrichtung = Alignment.Top_Left;
        MaxTextLength = 4000;
        Multiline = true;
        AllowedChars = string.Empty;
        AreaControl = Rectangle.Empty;
        _textDimensions = Size.Empty;
        _widthControl = null;
        _heightControl = null;
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

    public string AllowedChars { get; set; }

    /// <summary>
    /// Falls mit einer Skalierung gezeichnet wird, müssen die Angaben bereits skaliert sein.
    /// </summary>
    public Rectangle AreaControl { get; set; }

    // Todo: Implementieren
    public Alignment Ausrichtung { get; set; }

    public int Count => _internal.Count;

    public int HeightControl {
        get {
            EnsurePositions();
            return _heightControl ?? -1;
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

    public int MaxTextLength { get; }

    public bool Multiline { get; set; }

    public string PlainText {
        get {
            if (IsDisposed) { return string.Empty; }
            _tmpPlainText ??= ConvertCharToPlainText(0, _internal.Count - 1);
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

    public int WidthControl {
        get {
            EnsurePositions();
            return _widthControl ?? 0;
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

    #region Indexers

    public ExtChar this[int nr] => _internal[nr];

    #endregion

    #region Methods

    public void ChangeStyle(int first, int last, PadStyles style) {
        var changed = false;
        for (var cc = first; cc <= Math.Min(last, _internal.Count - 1); cc++) {
            if (_internal[cc].Style != style) {
                _internal[cc].Style = style;
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
    public int Char_Search(float canvasX, float canvasY) {
        EnsurePositions();

        var cZ = -1;
        var xDi = double.MaxValue;
        var yDi = double.MaxValue;
        var xNr = -1;
        var yNr = -1;

        do {
            cZ++;
            if (cZ > _internal.Count - 1) { break; }// Das Ende des Textes
            if (_internal[cZ].SizeCanvas.Width > 0) {
                var matchX = canvasX >= _internal[cZ].PosCanvas.X && canvasX <= _internal[cZ].PosCanvas.X + _internal[cZ].SizeCanvas.Width;
                var matchY = canvasY >= _internal[cZ].PosCanvas.Y && canvasY <= _internal[cZ].PosCanvas.Y + _internal[cZ].SizeCanvas.Height;

                if (matchX && matchY) { return cZ; }

                double tmpDi;
                if (!matchX && matchY) {
                    tmpDi = Math.Abs(canvasX - (_internal[cZ].PosCanvas.X + (_internal[cZ].SizeCanvas.Width / 2.0)));
                    if (tmpDi < xDi) {
                        xNr = cZ;
                        xDi = tmpDi;
                    }
                } else if (matchX && !matchY) {
                    tmpDi = Math.Abs(canvasY - (_internal[cZ].PosCanvas.Y + (_internal[cZ].SizeCanvas.Height / 2.0)));
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

    public Rectangle CursorCanvasPosX(int charPos) {
        EnsurePositions();

        if (charPos > _internal.Count + 1) { charPos = _internal.Count + 1; }
        if (_internal.Count > 0 && charPos < 0) { charPos = 0; }

        float x = 0;
        float y = 0;
        float he = 14;

        if (_internal.Count == 0) {
            // Kein Text vorhanden
        } else if (charPos < _internal.Count) {
            // Cursor vor einem Zeichen
            x = _internal[charPos].PosCanvas.X;
            y = _internal[charPos].PosCanvas.Y;
            he = _internal[charPos].SizeCanvas.Height;
        } else if (charPos > 0 && charPos < _internal.Count + 1 && _internal[charPos - 1].IsLineBreak()) {
            // Vorzeichen = Zeilenumbruch
            y = _internal[charPos - 1].PosCanvas.Y + _internal[charPos - 1].SizeCanvas.Height;
            he = _internal[charPos - 1].SizeCanvas.Height;
        } else if (charPos > 0 && charPos < _internal.Count + 1) {
            // Vorzeichen = Echtes Char
            x = _internal[charPos - 1].PosCanvas.X + _internal[charPos - 1].SizeCanvas.Width;
            y = _internal[charPos - 1].PosCanvas.Y;
            he = _internal[charPos - 1].SizeCanvas.Height;
        }
        return new Rectangle((int)x, (int)(y - 1), 0, (int)(he + 2));
    }

    public void Delete(int first, int last) {
        var tempVar = last - first;
        for (var z = 1; z <= tempVar; z++) {
            if (first < _internal.Count) {
                _internal.RemoveAt(first);
            }
        }
        ResetPosition(true);
    }

    public void Dispose() => IsDisposed = true;

    public void Draw(Graphics gr, float zoom, int offsetX, int offsetY) {
        EnsurePositions();
        DrawStates(gr, zoom, offsetY, offsetY);

        foreach (var t in _internal) {
            var controlPos = t.PosCanvas.CanvasToControl(zoom, offsetX, offsetY);
            var controlSize = t.SizeCanvas.CanvasToControl(zoom);

            if (t.IsVisible(AreaControl, controlPos, controlSize)) {
                t.Draw(gr, controlPos, controlSize, zoom);
            }
        }
    }

    public bool Insert(int position, ExtChar c) {
        if (position < 0) { return false; }
        if (position > _internal.Count) { return false; }

        _internal.Insert(position, c);
        ResetPosition(true);
        return true;
    }

    public Size LastSize() {
        EnsurePositions();
        return _heightControl == null || _widthControl < 5 || _heightControl < 5 ? new Size(32, 16) : new Size((int)_widthControl + 1, (int)_heightControl + 1);
    }

    public void OnStyleChanged() => StyleChanged?.Invoke(this, System.EventArgs.Empty);

    public string Substring(int startIndex, int length) => ConvertCharToPlainText(startIndex, startIndex + length - 1);

    public string Word(int atPosition) {
        var s = WordStart(atPosition);
        var e = WordEnd(atPosition);
        return s == -1 || e == -1 ? string.Empty : Substring(s, e - s);
    }

    internal string ConvertCharToPlainText(int first, int last) {
        try {
            var _stringBuilder = new StringBuilder(1024);
            for (var cZ = first; cZ <= Math.Min(last, _internal.Count - 1); cZ++) {
                _stringBuilder.Append(_internal[cZ].PlainText());
            }
            return _stringBuilder.ToString().Replace("\n", string.Empty);
        } catch {
            // Wenn Chars geändert wird (und dann der _internal.Count nimmer stimmt)
            Develop.AbortAppIfStackOverflow();
            return ConvertCharToPlainText(first, last);
        }
    }

    internal void Mark(MarkState markstate, int first, int last) {
        try {
            for (var z = first; z <= Math.Min(last, _internal.Count - 1); z++) {
                if (!_internal[z].Marking.HasFlag(markstate)) {
                    _internal[z].Marking |= markstate;
                }
            }
        } catch {
            Mark(markstate, first, last);
        }
    }

    internal void Unmark(MarkState markstate) {
        foreach (var t in _internal) {
            if (t.Marking.HasFlag(markstate)) {
                t.Marking ^= markstate;
            }
        }
    }

    internal int WordEnd(int pos) {
        // Frühe Validierung und Abbruch
        if (_internal.Count == 0 || pos < 0 || pos >= _internal.Count || _internal[pos].IsWordSeparator()) {
            return -1;
        }

        // Direkte Suche ohne while-Schleife
        while (++pos < _internal.Count) {
            if (_internal[pos].IsWordSeparator()) {
                return pos;
            }
        }

        return _internal.Count;
    }

    internal int WordStart(int pos) {
        // Frühe Validierung und Abbruch
        if (_internal.Count == 0 || pos < 0 || pos >= _internal.Count || _internal[pos].IsWordSeparator()) {
            return -1;
        }

        // Direkte Suche ohne while-Schleife
        while (--pos >= 0) {
            if (_internal[pos].IsWordSeparator()) {
                return pos + 1;
            }
        }

        return 0;
    }

    private int AddSpecialEntities(string htmltext, int position, PadStyles style, BlueFont font) {
        var endpos = htmltext.IndexOf(';', position + 1);

        if (endpos <= position || endpos > position + 10) {
            _internal.Add(new ExtCharAscii(this, style, font, '&'));
            return position + 1;
        }

        var entity = htmltext.Substring(position, endpos - position + 1);
        if (Constants.ReverseHtmlEntities.TryGetValue(entity, out var c)) {
            _internal.Add(new ExtCharAscii(this, style, font, c));
            return endpos;
        }

        Develop.DebugPrint(ErrorType.Info, "Unbekannter Code: " + entity);
        _internal.Add(new ExtCharAscii(this, style, font, '&'));
        return position + 1;
    }

    private string AppendStyle(ExtChar ls, ExtChar newStufe) {
        if (newStufe.Style == PadStyles.Undefiniert) {
            if (ls.Font == null || ls.Font == newStufe.Font || newStufe.Font == null) { return string.Empty; }

            var t = string.Empty;

            if (newStufe.Font.Bold != ls.Font.Bold && newStufe.Font.Bold) { t += "<b>"; }
            if (newStufe.Font.Bold != ls.Font.Bold && !newStufe.Font.Bold) { t += "</b>"; }
            if (newStufe.Font.Italic != ls.Font.Italic && newStufe.Font.Italic) { t += "<i>"; }
            if (newStufe.Font.Italic != ls.Font.Italic && !newStufe.Font.Italic) { t += "</i>"; }
            if (newStufe.Font.Underline != ls.Font.Underline && newStufe.Font.Underline) { t += "<u>"; }
            if (newStufe.Font.Underline != ls.Font.Underline && !newStufe.Font.Underline) { t += "</u>"; }

            return t;
        }

        if (ls.Style == newStufe.Style) { return string.Empty; }

        if (ls.Style == PadStyles.Standard && newStufe.Style == PadStyles.Überschrift) { return "<h1>"; }
        if (ls.Style == PadStyles.Überschrift && newStufe.Style == PadStyles.Standard) { return "</h1>"; }
        if (ls.Style == PadStyles.Standard && newStufe.Style == PadStyles.Hervorgehoben) { return "<strong>"; }
        if (ls.Style == PadStyles.Standard && newStufe.Style == PadStyles.Kapitel) { return "<strong>"; }

        if (ls.Style == PadStyles.Hervorgehoben && newStufe.Style == PadStyles.Standard) { return "</strong>"; }
        if (ls.Style == PadStyles.Hervorgehoben && newStufe.Style == PadStyles.Überschrift) { return "</strong><h1>"; }
        if (ls.Style == PadStyles.Kapitel && newStufe.Style == PadStyles.Standard) { return "</strong>"; }
        if (ls.Style == PadStyles.Kapitel && newStufe.Style == PadStyles.Überschrift) { return "</strong><h1>"; }

        if (ls.Style == PadStyles.Überschrift && newStufe.Style == PadStyles.Hervorgehoben) { return "</h1><strong>"; }

        return string.Empty;
    }

    private string ConvertCharToHtmlText() {
        if (_internal.Count == 0) { return string.Empty; }

        // Ungefähre Größe vorallokieren - reduziert Reallokationen
        var _stringBuilder = new StringBuilder(_internal.Count * 3);

        ExtChar lastStufe = new ExtCharAscii(this, PadStyles.Standard, Skin.GetBlueFont(string.Empty, PadStyles.Standard), 'x');

        for (var z = 0; z < _internal.Count; z++) {
            var s = AppendStyle(lastStufe, _internal[z]);
            if (!string.IsNullOrEmpty(s)) { _stringBuilder.Append(s); }

            lastStufe = _internal[z];
            _stringBuilder.Append(_internal[z].HtmlText());
        }
        return _stringBuilder.ToString();
    }

    private void ConvertTextToChar(string cactext, bool isRich) {
        if (string.IsNullOrEmpty(cactext)) {
            _internal.Clear();
            ResetPosition(true);
            return;
        }

        // Vorverarbeitung des Texts
        cactext = isRich ? cactext.ConvertFromHtmlToRich() : cactext.Replace("\r\n", "\r");

        _internal.Clear();
        _internal.Capacity = Math.Max(_internal.Capacity, cactext.Length); // Pre-allocate capacity
        ResetPosition(true);
        var style = StyleBeginns;
        var font = Skin.GetBlueFont(SheetStyle, StyleBeginns);

        // StringBuilder für temporäre String-Operationen
        var temp = new StringBuilder(_internal.Capacity);
        var pos = 0;
        var zeichen = -1;

        while (pos < cactext.Length) {
            var ch = cactext[pos];

            if (isRich) {
                switch (ch) {
                    case '<':
                        if (temp.Length > 0) {
                            zeichen++;
                            _internal.Add(new ExtCharAscii(this, style, font, temp.ToString()[0]));
                            temp.Clear();
                        }
                        // HTML-Code verarbeiten
                        DoHtmlCode(cactext, pos, ref zeichen, ref font, ref style);
                        // CanvasPosition zum Ende des HTML-Tags bewegen
                        var endTag = cactext.IndexOf('>', pos + 1);
                        pos = endTag != -1 ? endTag : cactext.Length;
                        break;

                    case '&':
                        if (temp.Length > 0) {
                            zeichen++;
                            _internal.Add(new ExtCharAscii(this, style, font, temp.ToString()[0]));
                            temp.Clear();
                        }
                        pos = AddSpecialEntities(cactext, pos, style, font);
                        zeichen++;
                        break;

                    default:
                        zeichen++;
                        _internal.Add(new ExtCharAscii(this, style, font, ch));
                        break;
                }
            } else {
                zeichen++;
                _internal.Add(new ExtCharAscii(this, style, font, ch));
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
                font = BlueFont.Get(font.FontName, font.Size, true, font.Italic, font.Underline, font.StrikeOut, font.ColorMain, font.ColorOutline, font.ColorBack);
                break;

            case "/B":
                style = PadStyles.Undefiniert;
                font = BlueFont.Get(font.FontName, font.Size, false, font.Italic, font.Underline, font.StrikeOut, font.ColorMain, font.ColorOutline, font.ColorBack);
                break;

            case "I":
                style = PadStyles.Undefiniert;
                font = BlueFont.Get(font.FontName, font.Size, font.Bold, true, font.Underline, font.StrikeOut, font.ColorMain, font.ColorOutline, font.ColorBack);
                break;

            case "/I":
                style = PadStyles.Undefiniert;
                font = BlueFont.Get(font.FontName, font.Size, font.Bold, false, font.Underline, font.StrikeOut, font.ColorMain, font.ColorOutline, font.ColorBack);
                break;

            case "U":
                style = PadStyles.Undefiniert;
                font = BlueFont.Get(font.FontName, font.Size, font.Bold, font.Italic, true, font.StrikeOut, font.ColorMain, font.ColorOutline, font.ColorBack);
                break;

            case "/U":
                style = PadStyles.Undefiniert;
                font = BlueFont.Get(font.FontName, font.Size, font.Bold, font.Italic, false, font.StrikeOut, font.ColorMain, font.ColorOutline, font.ColorBack);
                break;

            case "STRIKE":
                style = PadStyles.Undefiniert;
                font = BlueFont.Get(font.FontName, font.Size, font.Bold, font.Italic, font.Underline, true, font.ColorMain, font.ColorOutline, font.ColorBack);
                break;

            case "/STRIKE":
                style = PadStyles.Undefiniert;
                font = BlueFont.Get(font.FontName, font.Size, font.Bold, font.Italic, font.Underline, false, font.ColorMain, font.ColorOutline, font.ColorBack);
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
                FloatTryParse(attribut, out var fs);
                font = BlueFont.Get(font.FontName, fs, font.Bold, font.Italic, font.Underline, font.StrikeOut, font.ColorMain, font.ColorOutline, font.ColorBack);
                break;

            case "FONTNAME":
                style = PadStyles.Undefiniert;
                font = BlueFont.Get(attribut, font.Size, font.Bold, font.Italic, font.Underline, font.StrikeOut, font.ColorMain, font.ColorOutline, font.ColorBack);
                break;

            case "FONTCOLOR":
                style = PadStyles.Undefiniert;
                font = BlueFont.Get(font.FontName, font.Size, font.Bold, font.Italic, font.Underline, font.StrikeOut, ColorParse(attribut), font.ColorOutline, font.ColorBack);
                break;

            case "BACKCOLOR":
                style = PadStyles.Undefiniert;
                font = BlueFont.Get(font.FontName, font.Size, font.Bold, font.Italic, font.Underline, font.StrikeOut, font.ColorMain, font.ColorOutline, ColorParse(attribut));
                break;

            case "OUTLINECOLOR":
            case "COLOROUTLINE":
            case "FONTOUTLINE":
                style = PadStyles.Undefiniert;
                font = BlueFont.Get(font.FontName, font.Size, font.Bold, font.Italic, font.Underline, font.StrikeOut, font.ColorMain, ColorParse(attribut), font.ColorBack);
                break;

            case "HR":
            //    CanvasPosition++;
            //    this.GenerateAndAdd(new ExtChar(13, _Design, _State, PF, Stufe, MarkState));
            //    CanvasPosition++;
            //    this.GenerateAndAdd(new ExtChar((int)enEtxtCodes.HorizontalLine, _Design, _State, PF, Stufe, MarkState));
            //    break;
            case "BR":
                position++;
                _internal.Add(new ExtCharCrlfCode(this, style, font));
                break;

            case "TAB":
                position++;
                _internal.Add(new ExtCharTabCode(this, style, font));
                break;

            case "ZBX_STORE":
                position++;
                _internal.Add(new ExtCharStoreXCode(this, style, font));
                break;

            case "TOP":
                position++;
                _internal.Add(new ExtCharTopCode(this, style, font));
                break;

            case "IMAGECODE":
                var x = !attribut.Contains("|") ? QuickImage.Get(attribut, (int)font.Oberlänge(1)) : QuickImage.Get(attribut);
                position++;
                _internal.Add(new ExtCharImageCode(this, style, font, x));
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
            case "/H1":
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

            case "CELLLINK":
                var xl = (attribut + "|||").SplitBy("|");
                position++;
                _internal.Add(new ExtCharCellLink(this, style, font, xl[0], xl[1], xl[2]));

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

        foreach (var thisChar in _internal) {
            if (thisChar.Style != last) {
                last = thisChar.Style;
                f = thisChar.Font;
            } else {
                thisChar.Font = f;
            }
        }
    }

    private void DrawState(Graphics gr, MarkState state, float zoom, int offsetX, int offsetY) {
        var tmas = -1;
        for (var pos = 0; pos < _internal.Count; pos++) {
            var tempVar = _internal[pos];
            var marked = tempVar.Marking.HasFlag(state);

            if (marked && tmas < 0) { tmas = pos; }

            if (!marked || pos == _internal.Count - 1) {
                if (tmas > -1) {
                    if (pos == _internal.Count - 1) {
                        DrawZone(gr, zoom, state, tmas, pos, offsetX, offsetY);
                    } else {
                        DrawZone(gr, zoom, state, tmas, pos - 1, offsetX, offsetY);
                    }
                    tmas = -1;
                }
            }
        }
    }

    private void DrawStates(Graphics gr, float scale, int offsetX, int offsetY) {
        DrawState(gr, MarkState.Field, scale, offsetX, offsetY);
        DrawState(gr, MarkState.MyOwn, scale, offsetX, offsetY);
        DrawState(gr, MarkState.Other, scale, offsetX, offsetY);
        DrawState(gr, MarkState.Ringelchen, scale, offsetX, offsetY);
    }

    private void DrawZone(Graphics gr, float zoom, MarkState thisState, int markStart, int markEnd, int offsetX, int offsetY) {
        var startX = _internal[markStart].PosCanvas.X.CanvasToControl(zoom, offsetX);
        var startY = _internal[markStart].PosCanvas.Y.CanvasToControl(zoom, offsetY);
        var endX = _internal[markEnd].PosCanvas.X.CanvasToControl(zoom, offsetX) + _internal[markEnd].SizeCanvas.Width.CanvasToControl(zoom);
        var endy = _internal[markEnd].PosCanvas.Y.CanvasToControl(zoom, offsetY) + _internal[markEnd].SizeCanvas.Height.CanvasToControl(zoom);

        switch (thisState) {
            case MarkState.None:
                break;

            case MarkState.Ringelchen:
                using (var pen = new Pen(Color.Red, 3.CanvasToControl(zoom))) {
                    gr.DrawLine(pen, startX, (int)(startY + (_internal[markStart].SizeCanvas.Height.CanvasToControl(zoom) * 0.9)), endX, (int)(startY + (_internal[markStart].SizeCanvas.Height.CanvasToControl(zoom) * 0.9)));
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
        if (_widthControl == null) {
            ReBreak();
        }
    }

    private void OnPropertyChanged([CallerMemberName] string propertyName = "unknown") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    /// <summary>
    /// Berechnet die Zeichen-Positionen mit korrekten Umbrüchen. Die enAlignment wird ebenfalls mit eingerechnet.
    /// Cursor_ComputePixelXPos wird am Ende aufgerufen, einschließlich Cursor_Repair und SetNewAkt.
    /// </summary>
    /// <remarks></remarks>
    private void ReBreak() {
        _widthControl = 0;
        _heightControl = 0;
        if (_internal.Count == 0) { return; }

        var estimatedRows = _internal.Count / 50; // Geschätzte Zeilenanzahl
        var ri = new List<string>(estimatedRows);
        var vZbxPixel = 0f;
        var isX = 0f;
        var isY = 0f;
        var zbChar = 0;
        var akt = -1;

        do {
            akt++;
            if (akt > _internal.Count - 1) {
                Row_SetOnLine(zbChar, akt - 1);
                ri.Add(zbChar + ";" + (akt - 1));
                break;
            }

            if (_internal[akt] is ExtCharStoreXCode) {
                vZbxPixel = isX;
            } else if (_internal[akt] is ExtCharTopCode) {
                isY = 0;
            }

            if (!_internal[akt].IsSpace()) {
                if (akt > zbChar && _textDimensions.Width > 0) {
                    if (isX + _internal[akt].SizeCanvas.Width + 0.5 > _textDimensions.Width) {
                        akt = WordBreaker(akt, zbChar);
                        isX = vZbxPixel;
                        isY += Row_SetOnLine(zbChar, akt - 1) * _zeilenabstand;
                        ri.Add(zbChar + ";" + (akt - 1));
                        zbChar = akt;
                    }
                }
                _widthControl = Math.Max((int)_widthControl, (int)(isX + _internal[akt].SizeCanvas.Width + 0.5));
                _heightControl = Math.Max((int)_heightControl, (int)(isY + _internal[akt].SizeCanvas.Height + 0.5));
            }

            _internal[akt].PosCanvas.X = isX;
            _internal[akt].PosCanvas.Y = isY;

            // Diese Zeile garantiert, dass immer genau EIN Pixel frei ist zwischen zwei Buchstaben.
            //isX = (float)(isX + Math.Truncate(_internal[akt].Size.Width + 0.5));
            isX += _internal[akt].SizeCanvas.Width;

            if (_internal[akt].IsLineBreak()) {
                isX = vZbxPixel;
                if (_internal[akt] is ExtCharTopCode) {
                    Row_SetOnLine(zbChar, akt);
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
            if (Ausrichtung.HasFlag(Alignment.VerticalCenter)) { ky = (float)((_textDimensions.Height - (int)_heightControl) / 2.0); }
            if (Ausrichtung.HasFlag(Alignment.Bottom)) { ky = _textDimensions.Height - (int)_heightControl; }
            foreach (var t in ri) {
                var o = t.SplitAndCutBy(";");
                var z1 = IntParse(o[0]);
                var z2 = IntParse(o[1]);
                float kx = 0;
                if (Ausrichtung.HasFlag(Alignment.Right)) { kx = _textDimensions.Width - _internal[z2].PosCanvas.X - _internal[z2].SizeCanvas.Width; }
                if (Ausrichtung.HasFlag(Alignment.HorizontalCenter)) { kx = (_textDimensions.Width - _internal[z2].PosCanvas.X - _internal[z2].SizeCanvas.Width) / 2; }
                for (var z3 = z1; z3 <= z2; z3++) {
                    _internal[z3].PosCanvas.X += kx;
                    _internal[z3].PosCanvas.Y += ky;
                }
            }
        }

        #endregion
    }

    private void ResetPosition(bool andTmpText) {
        if (IsDisposed) { return; }
        _widthControl = null;
        _heightControl = null;

        if (andTmpText) {
            _tmpHtmlText = null;
            _tmpPlainText = null;
        }
        OnPropertyChanged("Position");
    }

    private float Row_SetOnLine(int first, int last) {
        float abstand = 0;
        for (var z = first; z <= last; z++) {
            abstand = Math.Max(abstand, _internal[z].SizeCanvas.Height);
        }
        for (var z = first; z <= last; z++) {
            if (_internal[z] is ExtCharTopCode) {
                _internal[z].PosCanvas.Y = _internal[z].PosCanvas.Y + abstand - _internal[z].SizeCanvas.Height;
            }
        }
        return abstand;
    }

    private int WordBreaker(int augZeichen, int minZeichen) {
        if (_internal.Count == 1) { return 0; }
        if (minZeichen < 0) { minZeichen = 0; }
        if (augZeichen > _internal.Count - 1) { augZeichen = _internal.Count - 1; }
        if (augZeichen < minZeichen + 1) { augZeichen = minZeichen + 1; }
        // AusnahmeFall auschließen:
        // Space-Zeichen - Dann Buchstabe
        if (_internal[augZeichen - 1].IsSpace() && !_internal[augZeichen].IsPossibleLineBreak()) { return augZeichen; }
        var started = augZeichen;
        // Das Letzte Zeichen Search, das kein Trennzeichen ist
        do {
            if (_internal[augZeichen].IsPossibleLineBreak()) {
                augZeichen--;
            } else {
                break;
            }
            if (augZeichen <= minZeichen) { return started; }
        } while (true);
        do {
            if (_internal[augZeichen].IsPossibleLineBreak()) { return augZeichen + 1; }
            augZeichen--;
            if (augZeichen <= minZeichen) { return started; }
        } while (true);
    }

    #endregion
}
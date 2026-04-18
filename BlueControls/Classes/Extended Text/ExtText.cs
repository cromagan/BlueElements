// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
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
using BlueBasics.ClassesStatic;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.Classes;
using BlueControls.Enums;
using BlueControls.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
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

    private static readonly SolidBrush _brushField = new(Color.FromArgb(80, 128, 128, 128));
    private static readonly SolidBrush _brushMyOwn = new(Color.FromArgb(40, 50, 255, 50));
    private static readonly SolidBrush _brushOther = new(Color.FromArgb(80, 255, 255, 50));
    private static readonly Dictionary<string, Type> _structuralTagFactories = BuildStructuralTagFactories();
    private readonly List<ExtChar> _internal = [];
    private int? _heightControl;
    private int _markedCharsCount;
    private string _originalHtml = string.Empty;
    private string _sheetStyle = Constants.Win11;

    private Size _textDimensions;

    private string? _tmpHtmlText;

    private string? _tmpPlainText;

    private int? _widthControl;

    private float _zeilenabstand;

    private List<ExtChar> _flatChars = [];

    #endregion

    #region Constructors

    public ExtText() {
        Ausrichtung = Alignment.Top_Left;
        MaxTextLength = 4000;
        Multiline = true;
        AllowedChars = string.Empty;
        AreaControl = Rectangle.Empty;
        _textDimensions = Size.Empty;
        _zeilenabstand = 1;
    }

    public ExtText(Design design, States state) : this() {
        var sh = Skin.DesignOf(design, state);
        BaseFont = sh.Font;
        _sheetStyle = Constants.Win11;
    }

    public ExtText(string sheetStyle, PadStyles stylebeginns) : this() {
        _sheetStyle = sheetStyle;
        StyleBeginns = stylebeginns;
        BaseFont = Skin.GetBlueFont(sheetStyle, stylebeginns);
    }

    public ExtText(string blueFontParseString) : this() {
        BaseFont = BlueFont.Get(blueFontParseString);
    }

    public ExtText(string blueFontParseString, string sheetStyle, PadStyles styleBeginns) : this(sheetStyle, styleBeginns) {
        BaseFont = BlueFont.Get(blueFontParseString);
    }

    #endregion

    #region Events

    public event PropertyChangedEventHandler? PropertyChanged;

    public event EventHandler? StyleChanged;

    #endregion

    #region Properties

    public string AllowedChars { get; set; }

    /// <summary>
    /// Bestimmt den Zeichenbereich. Zeichen außerhalb werden nicht dargsetellt.
    /// Falls mit einer Skalierung gezeichnet wird, müssen die Angaben bereits skaliert sein.
    /// </summary>
    public Rectangle AreaControl { get; set; }

    public Alignment Ausrichtung { get; set; }
    public BlueFont BaseFont { get; set; } = BlueFont.DefaultFont;
    public int Count => _internal.Count;

    public int HeightControl {
        get {
            EnsurePositions();
            return _heightControl ?? -1;
        }
    }

    public string HtmlText {
        get {
            _tmpHtmlText ??= BuildHtmlText();
            return _tmpHtmlText;
        }
        set {
            if (IsDisposed) { return; }
            if (_originalHtml == value) { return; }
            _originalHtml = value;
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
            _tmpPlainText ??= BuildPlainText(0, _internal.Count - 1);
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
            ResetPosition(false);
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

    public void ChangeStructuralTag(int first, int last, string? structTag) {
        var newTags = BuildTagsForStructuralStyle(structTag);
        var changed = false;
        for (var cc = first; cc <= Math.Min(last, _internal.Count - 1); cc++) {
            var tags = _internal[cc].OverrideTags;
            tags.Clear();
            tags.AddRange(newTags);
            _internal[cc].InvalidateFont();
            changed = true;
        }
        if (changed) { ResetPosition(true); }
    }

    public int Char_Search(float canvasX, float canvasY) {
        EnsurePositions();
        if (_flatChars.Count == 0) { return 0; }

        var bestXDist = double.MaxValue;
        var bestXPos = -1;
        var bestYDist = double.MaxValue;
        var bestYPos = -1;

        for (var i = 0; i < _flatChars.Count; i++) {
            var ch = _flatChars[i];
            if (ch.SizeCanvas.Width <= 0) { continue; }

            var matchX = canvasX >= ch.PosCanvas.X && canvasX <= ch.PosCanvas.X + ch.SizeCanvas.Width;
            var matchY = canvasY >= ch.PosCanvas.Y && canvasY <= ch.PosCanvas.Y + ch.SizeCanvas.Height;

            if (matchX && matchY) { return i; }

            if (matchX) {
                var dy = Math.Abs(canvasY - (ch.PosCanvas.Y + ch.SizeCanvas.Height / 2.0));
                if (dy < bestYDist) { bestYDist = dy; bestYPos = i; }
            } else if (matchY) {
                var dx = Math.Abs(canvasX - (ch.PosCanvas.X + ch.SizeCanvas.Width / 2.0));
                if (dx < bestXDist) { bestXDist = dx; bestXPos = i; }
            }
        }

        return bestXPos >= 0 ? bestXPos : bestYPos >= 0 ? bestYPos : _flatChars.Count - 1;
    }

    public Rectangle CursorCanvasPosX(int charPos) {
        EnsurePositions();

        charPos = Math.Max(0, Math.Min(charPos, _flatChars.Count + 1));

        float x = 0;
        float y = 0;
        float he = 14;

        if (_flatChars.Count == 0) {
        } else if (charPos < _flatChars.Count) {
            x = _flatChars[charPos].PosCanvas.X;
            y = _flatChars[charPos].PosCanvas.Y;
            he = _flatChars[charPos].SizeCanvas.Height;
        } else if (charPos > 0 && _flatChars[charPos - 1].IsLineBreak()) {
            y = _flatChars[charPos - 1].PosCanvas.Y + _flatChars[charPos - 1].SizeCanvas.Height;
            he = _flatChars[charPos - 1].SizeCanvas.Height;
        } else if (charPos > 0) {
            x = _flatChars[charPos - 1].PosCanvas.X + _flatChars[charPos - 1].SizeCanvas.Width;
            y = _flatChars[charPos - 1].PosCanvas.Y;
            he = _flatChars[charPos - 1].SizeCanvas.Height;
        }
        return new Rectangle((int)x, (int)(y - 1), 0, (int)(he + 2));
    }

    public void Delete(int first, int last) {
        if (first >= _internal.Count || first > last) { return; }
        var count = Math.Min(last - first + 1, _internal.Count - first);
        _internal.RemoveRange(first, count);
        ResetPosition(true);
    }

    public void Dispose() => IsDisposed = true;

    public void Draw(Graphics gr, float zoom, int offsetX, int offsetY) {
        EnsurePositions();
        if (_markedCharsCount > 0) {
            DrawMarkings(gr, zoom, offsetX, offsetY);
        }
        var count = _internal.Count;
        for (var i = 0; i < count; i++) {
            var t = _internal[i];
            var controlPos = t.PosCanvas.CanvasToControl(zoom, offsetX, offsetY);
            var controlSize = t.SizeCanvas.CanvasToControl(zoom);

            if (ExtChar.IsVisible(AreaControl, controlPos, controlSize)) {
                t.Draw(gr, controlPos, controlSize, zoom);
            }
        }
    }

    public bool Insert(int position, ExtChar c) {
        if (position < 0 || position > _internal.Count) { return false; }
        _internal.Insert(position, c);
        ResetPosition(true);
        return true;
    }

    public void InvalidateFonts(int first, int last) { //TODO: Unused
        for (var cc = first; cc <= Math.Min(last, _internal.Count - 1); cc++) {
            _internal[cc].InvalidateFont();
        }
        ResetPosition(true);
    }

    public Size LastSize() {
        EnsurePositions();
        return _heightControl == null || _widthControl < 5 || _heightControl < 5
            ? new Size(32, 16)
            : new Size((int)_widthControl + 1, (int)_heightControl + 1);
    }

    public void OnStyleChanged() => StyleChanged?.Invoke(this, System.EventArgs.Empty);

    public string Substring(int startIndex, int length) => BuildPlainText(startIndex, startIndex + length - 1);

    public void UpdateBaseFont(BlueFont font) {
        if (IsDisposed || BaseFont == font) { return; }
        BaseFont = font;
        OnStyleChanged();
        ResetPosition(true);
    }

    public string Word(int atPosition) {
        var s = WordStart(atPosition);
        var e = WordEnd(atPosition);
        return s == -1 || e == -1 ? string.Empty : Substring(s, e - s);
    }

    internal string BuildHtmlText(int first, int last) {
        var end = Math.Min(last, _internal.Count - 1);
        if (end < first || _internal.Count == 0) { return string.Empty; }

        var sb = new StringBuilder((end - first + 1) * 4);
        BlueFont? lastFont = null;
        string? lastStructTag = null;

        for (var z = first; z <= end; z++) {
            var ec = _internal[z];
            var currentStructTag = GetStructuralTag(ec.OverrideTags);
            var ecFont = ec.Font;

            if (lastStructTag != currentStructTag) {
                if (lastStructTag != null) {
                    sb.Append("</").Append(lastStructTag).Append('>');
                    lastFont = Skin.GetBlueFont(SheetStyle, PadStyles.Standard) ?? BaseFont;
                }
                if (currentStructTag != null) {
                    sb.Append('<').Append(currentStructTag).Append('>');
                    lastFont = GetStructuralTagFont(currentStructTag);
                }
            }

            sb.Append(BuildFontDiffTags(ecFont, lastFont));
            lastStructTag = currentStructTag;
            lastFont = ecFont;
            sb.Append(ec.HtmlText());
        }

        if (lastStructTag != null) { sb.Append("</").Append(lastStructTag).Append('>'); }

        return sb.ToString();
    }

    internal string BuildPlainText(int first, int last) {
        var end = Math.Min(last, _internal.Count - 1);
        if (end < first) { return string.Empty; }
        var sb = new StringBuilder(end - first + 1);
        for (var i = first; i <= end; i++) {
            var pt = _internal[i].PlainText();
            if (pt != "\n") { sb.Append(pt); }
        }
        return sb.ToString();
    }

    internal void Mark(MarkState markstate, int first, int last) {
        var end = Math.Min(last, _internal.Count - 1);
        for (var z = first; z <= end; z++) {
            if (!_internal[z].Marking.HasFlag(markstate)) {
                _internal[z].Marking |= markstate;
                _markedCharsCount++;
            }
        }
    }

    internal List<ExtChar> ParseHtmlToChars(string html) {
        var savedChars = new List<ExtChar>(_internal);
        _internal.Clear();
        try {
            ConvertTextToChar(html, true);
            return [.. _internal];
        } finally {
            _internal.Clear();
            _internal.AddRange(savedChars);
        }
    }

    internal void Unmark(MarkState markstate) {
        foreach (var t in _internal) {
            if (t.Marking.HasFlag(markstate)) {
                t.Marking ^= markstate;
                _markedCharsCount--;
            }
        }
    }

    internal int WordEnd(int pos) {
        if (_flatChars.Count == 0 || pos < 0 || pos >= _flatChars.Count || _flatChars[pos].IsWordSeparator()) {
            return -1;
        }
        while (++pos < _flatChars.Count) {
            if (_flatChars[pos].IsWordSeparator()) { return pos; }
        }
        return _flatChars.Count;
    }

    internal int WordStart(int pos) {
        if (_flatChars.Count == 0 || pos < 0 || pos >= _flatChars.Count || _flatChars[pos].IsWordSeparator()) {
            return -1;
        }
        while (--pos >= 0) {
            if (_flatChars[pos].IsWordSeparator()) { return pos + 1; }
        }
        return 0;
    }

    private static string BuildFontDiffTags(BlueFont? font, BlueFont? prevFont) {
        if (prevFont == null || font == null || prevFont == font) { return string.Empty; }
        var sb = new StringBuilder(64);

        if (font.Bold != prevFont.Bold) { sb.Append(font.Bold ? "<b>" : "</b>"); }
        if (font.Italic != prevFont.Italic) { sb.Append(font.Italic ? "<i>" : "</i>"); }
        if (font.Underline != prevFont.Underline) { sb.Append(font.Underline ? "<u>" : "</u>"); }
        if (font.StrikeOut != prevFont.StrikeOut) { sb.Append(font.StrikeOut ? "<strike>" : "</strike>"); }

        if (Math.Abs(font.Size - prevFont.Size) > 0.01f) {
            sb.Append("<fontsize=").Append(Math.Round(font.Size, 3)).Append('>');
        }

        if (font.FontName != prevFont.FontName) {
            sb.Append("<fontname=").Append(font.FontName).Append('>');
        }

        if (font.ColorMain != prevFont.ColorMain) {
            sb.Append("<fontcolor=").Append(font.ColorMain.ToHtmlCode()).Append('>');
        }

        if (font.ColorOutline != prevFont.ColorOutline && font.ColorOutline.A > 0) {
            sb.Append("<outlinecolor=").Append(font.ColorOutline.ToHtmlCode()).Append('>');
        } else if (prevFont.ColorOutline.A > 0 && font.ColorOutline.A == 0) {
            sb.Append("<outlinecolor=Transparent>");
        }

        if (font.ColorBack != prevFont.ColorBack && font.ColorBack.A > 0) {
            sb.Append("<backcolor=").Append(font.ColorBack.ToHtmlCode()).Append('>');
        } else if (prevFont.ColorBack.A > 0 && font.ColorBack.A == 0) {
            sb.Append("<backcolor=Transparent>");
        }

        return sb.ToString();
    }

    private static Dictionary<string, Type> BuildStructuralTagFactories() {
        var factories = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        foreach (var type in typeof(ExtChar).Assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(ExtChar)) && !t.IsAbstract && t.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, [], null) != null)) {
            var created = Activator.CreateInstance(type, true);
            if (created is not ExtChar instance) { continue; }
            var tagName = instance.StructuralTag;
            if (string.IsNullOrEmpty(tagName)) { continue; }
            factories[tagName] = type;
        }

        return factories;
    }

    private static PadStyles GetPadStyleFromStructTag(string structTag) => structTag switch {
        "h1" => PadStyles.Title,
        "h2" => PadStyles.Subtitle,
        "h3" => PadStyles.Chapter,
        "h5" => PadStyles.Footnote,
        "h6" => PadStyles.Alternative,
        "strong" => PadStyles.Emphasized,
        _ => PadStyles.Standard
    };

    private static string? GetStructuralTag(List<string> tags) =>
                                tags.Find(t => t is "h1" or "h2" or "h3" or "h5" or "h6" or "strong");

    private static void RemoveConflictingTag(List<string> tags, string newTag) {
        var eqIdx = newTag.IndexOf('=');
        var prefix = eqIdx >= 0 ? newTag[..(eqIdx + 1)] : null;

        for (var i = tags.Count - 1; i >= 0; i--) {
            if (prefix != null) {
                if (tags[i].StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) {
                    tags.RemoveAt(i);
                    return;
                }
            } else {
                if (tags[i] == newTag || tags[i] == "/" + newTag || newTag == "/" + tags[i]) {
                    tags.RemoveAt(i);
                    return;
                }
            }
        }
    }

    private void ApplyAlignment(List<ExtChar> chars, List<(int start, int end)> rows) {
        if (Ausrichtung == Alignment.Top_Left || rows.Count == 0) { return; }

        float offsetY = 0f;
        if (Ausrichtung.HasFlag(Alignment.VerticalCenter)) { offsetY = (_textDimensions.Height - (int)_heightControl) / 2f; }
        if (Ausrichtung.HasFlag(Alignment.Bottom)) { offsetY = _textDimensions.Height - (int)_heightControl; }

        foreach (var (start, end) in rows) {
            float offsetX = 0f;
            var lastChar = chars[end];

            if (Ausrichtung.HasFlag(Alignment.Right)) {
                offsetX = _textDimensions.Width - lastChar.PosCanvas.X - lastChar.SizeCanvas.Width;
            }
            if (Ausrichtung.HasFlag(Alignment.HorizontalCenter)) {
                offsetX = (_textDimensions.Width - lastChar.PosCanvas.X - lastChar.SizeCanvas.Width) / 2f;
            }

            if (offsetX == 0f && offsetY == 0f) { continue; }

            for (var i = start; i <= end; i++) {
                chars[i].PosCanvas.X += offsetX;
                chars[i].PosCanvas.Y += offsetY;
            }
        }
    }

    private static void ApplyFontTag(string cod, string? attribut, Stack<List<string>> stack) {
        var tags = stack.Pop();

        var tag = cod.ToLowerInvariant() switch {
            "b" => "b",
            "/b" => "/b",
            "i" => "i",
            "/i" => "/i",
            "u" => "u",
            "/u" => "/u",
            "strike" => "strike",
            "/strike" => "/strike",
            "fontsize" => "fontsize=" + (attribut ?? string.Empty),
            "fontname" => "fontname=" + (attribut ?? string.Empty),
            "fontcolor" => "fontcolor=" + (attribut ?? string.Empty),
            "backcolor" => "backcolor=" + (attribut ?? string.Empty),
            "outlinecolor" or "coloroutline" or "fontoutline" => "outlinecolor=" + (attribut ?? string.Empty),
            _ => null
        };

        if (tag != null) {
            RemoveConflictingTag(tags, tag);
            tags.Add(tag);
        }

        stack.Push(tags);
    }

    private void ApplyPadStyleStandard(List<string> tags) {
        tags.RemoveAll(t => t is "b" or "/b" or "i" or "/i" or "u" or "/u" or "strike" or "/strike"
            || t.StartsWith("fontsize=", StringComparison.OrdinalIgnoreCase)
            || t.StartsWith("fontname=", StringComparison.OrdinalIgnoreCase)
            || t.StartsWith("fontcolor=", StringComparison.OrdinalIgnoreCase)
            || t.StartsWith("outlinecolor=", StringComparison.OrdinalIgnoreCase)
            || t.StartsWith("coloroutline=", StringComparison.OrdinalIgnoreCase)
            || t.StartsWith("fontoutline=", StringComparison.OrdinalIgnoreCase)
            || t.StartsWith("backcolor=", StringComparison.OrdinalIgnoreCase));

        var standardFont = Skin.GetBlueFont(SheetStyle, PadStyles.Standard);
        if (standardFont != null && standardFont != BaseFont) {
            if (standardFont.Bold != BaseFont.Bold) { tags.Add(standardFont.Bold ? "b" : "/b"); }
            if (standardFont.Italic != BaseFont.Italic) { tags.Add(standardFont.Italic ? "i" : "/i"); }
            if (standardFont.Underline != BaseFont.Underline) { tags.Add(standardFont.Underline ? "u" : "/u"); }
            if (standardFont.StrikeOut != BaseFont.StrikeOut) { tags.Add(standardFont.StrikeOut ? "strike" : "/strike"); }
            if (Math.Abs(standardFont.Size - BaseFont.Size) > 0.01f) { tags.Add($"fontsize={Math.Round(standardFont.Size, 3)}"); }
            if (standardFont.FontName != BaseFont.FontName) { tags.Add($"fontname={standardFont.FontName}"); }
            if (standardFont.ColorMain != BaseFont.ColorMain) { tags.Add($"fontcolor={standardFont.ColorMain.ToHtmlCode()}"); }
            if (standardFont.ColorOutline != BaseFont.ColorOutline) { tags.Add($"outlinecolor={standardFont.ColorOutline.ToHtmlCode()}"); }
            if (standardFont.ColorBack != BaseFont.ColorBack) { tags.Add($"backcolor={standardFont.ColorBack.ToHtmlCode()}"); }
        }
    }

    private void ApplyStructuralTag(string cod, string? attribut, Stack<List<string>> stack) {
        var tags = stack.Peek();

        if (_structuralTagFactories.TryGetValue(cod, out var type)) {
            var created = Activator.CreateInstance(type);
            if (created is not ExtChar instance) { return; }
            instance.InitFromTag(this, tags, attribut);
            _internal.Add(instance);
        } else {
            _internal.Add(new ExtCharAscii(this, tags, '<'));
            foreach (var c in cod)
                _internal.Add(new ExtCharAscii(this, tags, c));
            if (!string.IsNullOrEmpty(attribut)) {
                _internal.Add(new ExtCharAscii(this, tags, '='));
                foreach (var c in attribut)
                    _internal.Add(new ExtCharAscii(this, tags, c));
            }
            _internal.Add(new ExtCharAscii(this, tags, '>'));
        }
    }

    private void ApplyStyleTag(string cod, Stack<List<string>> stack) {
        var tags = stack.Pop();

        tags.RemoveAll(t => t is "h1" or "h2" or "h3" or "h5" or "h6" or "strong"
            || t.StartsWith("fontsize=", StringComparison.OrdinalIgnoreCase)
            || t.StartsWith("fontname=", StringComparison.OrdinalIgnoreCase)
            || t.StartsWith("fontcolor=", StringComparison.OrdinalIgnoreCase)
            || t.StartsWith("outlinecolor=", StringComparison.OrdinalIgnoreCase)
            || t.StartsWith("coloroutline=", StringComparison.OrdinalIgnoreCase)
            || t.StartsWith("fontoutline=", StringComparison.OrdinalIgnoreCase)
            || t.StartsWith("backcolor=", StringComparison.OrdinalIgnoreCase)
            || t is "b" or "/b" or "i" or "/i" or "u" or "/u" or "strike" or "/strike");

        var structTag = cod switch {
            "H1" => "h1",
            "H2" => "h2",
            "H3" => "h3",
            "H5" => "h5",
            "H6" => "h6",
            "H7" or "STRONG" => "strong",
            _ => null
        };

        if (structTag != null) {
            tags.Add(structTag);
            var targetFont = Skin.GetBlueFont(SheetStyle, GetPadStyleFromStructTag(structTag));
            if (targetFont != null && targetFont != BaseFont) {
                if (targetFont.Bold != BaseFont.Bold) { tags.Add(targetFont.Bold ? "b" : "/b"); }
                if (targetFont.Italic != BaseFont.Italic) { tags.Add(targetFont.Italic ? "i" : "/i"); }
                if (targetFont.Underline != BaseFont.Underline) { tags.Add(targetFont.Underline ? "u" : "/u"); }
                if (targetFont.StrikeOut != BaseFont.StrikeOut) { tags.Add(targetFont.StrikeOut ? "strike" : "/strike"); }
                if (Math.Abs(targetFont.Size - BaseFont.Size) > 0.01f) { tags.Add($"fontsize={Math.Round(targetFont.Size, 3)}"); }
                if (targetFont.FontName != BaseFont.FontName) { tags.Add($"fontname={targetFont.FontName}"); }
                if (targetFont.ColorMain != BaseFont.ColorMain) { tags.Add($"fontcolor={targetFont.ColorMain.ToHtmlCode()}"); }
                if (targetFont.ColorOutline != BaseFont.ColorOutline) { tags.Add($"outlinecolor={targetFont.ColorOutline.ToHtmlCode()}"); }
                if (targetFont.ColorBack != BaseFont.ColorBack) { tags.Add($"backcolor={targetFont.ColorBack.ToHtmlCode()}"); }
            }
        } else {
            ApplyPadStyleStandard(tags);
        }

        stack.Push(tags);
    }

    private string BuildHtmlText() => BuildHtmlText(0, _internal.Count - 1);

    private List<string> BuildTagsForStructuralStyle(string? structTag) {
        if (structTag == null) { return []; }

        var padStyle = structTag switch {
            "h1" => PadStyles.Title,
            "h2" => PadStyles.Subtitle,
            "h3" => PadStyles.Chapter,
            "h5" => PadStyles.Footnote,
            "h6" => PadStyles.Alternative,
            "strong" => PadStyles.Emphasized,
            _ => PadStyles.Standard
        };

        var result = new List<string> { structTag };

        var targetFont = Skin.GetBlueFont(SheetStyle, padStyle);
        if (targetFont == null || targetFont == BaseFont) { return result; }

        if (targetFont.Bold != BaseFont.Bold) { result.Add(targetFont.Bold ? "b" : "/b"); }
        if (targetFont.Italic != BaseFont.Italic) { result.Add(targetFont.Italic ? "i" : "/i"); }
        if (targetFont.Underline != BaseFont.Underline) { result.Add(targetFont.Underline ? "u" : "/u"); }
        if (targetFont.StrikeOut != BaseFont.StrikeOut) { result.Add(targetFont.StrikeOut ? "strike" : "/strike"); }
        if (Math.Abs(targetFont.Size - BaseFont.Size) > 0.01f) { result.Add($"fontsize={Math.Round(targetFont.Size, 3)}"); }
        if (targetFont.FontName != BaseFont.FontName) { result.Add($"fontname={targetFont.FontName}"); }
        if (targetFont.ColorMain != BaseFont.ColorMain) { result.Add($"fontcolor={targetFont.ColorMain.ToHtmlCode()}"); }
        if (targetFont.ColorOutline != BaseFont.ColorOutline) { result.Add($"outlinecolor={targetFont.ColorOutline.ToHtmlCode()}"); }
        if (targetFont.ColorBack != BaseFont.ColorBack) { result.Add($"backcolor={targetFont.ColorBack.ToHtmlCode()}"); }

        return result;
    }

    private void ComputeLayout() {
        _widthControl = 0;
        _heightControl = 0;

        _flatChars = [];
        foreach (var ec in _internal) {
            _flatChars.AddRange(ec.GetChars());
        }

        var flat = _flatChars;
        var count = flat.Count;
        if (count == 0) { return; }

        var estimatedRows = Math.Max(1, count / 50);
        var rows = new List<(int start, int end)>(estimatedRows);

        float storedX = 0f;
        float currentX = 0f;
        float currentY = 0f;
        var rowStart = 0;

        for (var i = 0; i <= count; i++) {
            if (i == count) {
                NormalizeRowHeight(flat, rowStart, i - 1);
                rows.Add((rowStart, i - 1));
                break;
            }

            var ch = flat[i];

            if (ch.StoresXPosition) {
                storedX = currentX;
            } else if (ch.ResetsYPosition) {
                currentY = 0;
            }

            if (!ch.IsSpace()) {
                if (i > rowStart && _textDimensions.Width > 0) {
                    if (currentX + ch.SizeCanvas.Width + 0.5 > _textDimensions.Width) {
                        i = FindWordBreak(flat, i, rowStart);
                        currentX = storedX;
                        currentY += NormalizeRowHeight(flat, rowStart, i - 1) * _zeilenabstand;
                        rows.Add((rowStart, i - 1));
                        rowStart = i;
                        if (i < count) { ch = flat[i]; } else { break; }
                    }
                }
                _widthControl = Math.Max((int)_widthControl, (int)(currentX + ch.SizeCanvas.Width + 0.5));
                _heightControl = Math.Max((int)_heightControl, (int)(currentY + ch.SizeCanvas.Height + 0.5));
            }

            ch.PosCanvas.X = currentX;
            ch.PosCanvas.Y = currentY;
            currentX += ch.SizeCanvas.Width;

            if (ch.IsLineBreak()) {
                currentX = storedX;
                if (ch.ResetsYPosition) {
                    NormalizeRowHeight(flat, rowStart, i);
                    rows.Add((rowStart, i));
                } else {
                    currentY += NormalizeRowHeight(flat, rowStart, i) * _zeilenabstand;
                    rows.Add((rowStart, i));
                }
                rowStart = i + 1;
            }
        }

        ApplyAlignment(flat, rows);
    }

    private void ConvertTextToChar(string text, bool isRich) {
        if (string.IsNullOrEmpty(text)) {
            _internal.Clear();
            ResetPosition(true);
            return;
        }

        text = isRich ? text.ConvertFromHtmlToRich() : text.Replace("\r\n", "\r");

        _internal.Clear();
        _internal.Capacity = Math.Max(_internal.Capacity, text.Length);
        ResetPosition(true);

        var styleStack = new Stack<List<string>>();
        styleStack.Push([]);

        var pos = 0;
        while (pos < text.Length) {
            var ch = text[pos];

            if (isRich) {
                switch (ch) {
                    case '<': {
                            var endTag = text.IndexOf('>', pos + 1);
                            if (endTag != -1) {
                                ParseHtmlTag(text, pos, endTag, styleStack);
                                pos = endTag;
                            } else {
                                var top = styleStack.Peek();
                                _internal.Add(new ExtCharAscii(this, top, ch));
                            }
                            break;
                        }

                    case '&': {
                            var top = styleStack.Peek();
                            pos = ParseHtmlEntity(text, pos, top);
                            break;
                        }

                    default: {
                            var top = styleStack.Peek();
                            _internal.Add(new ExtCharAscii(this, top, ch));
                            break;
                        }
                }
            } else {
                var tags = styleStack.Peek();
                _internal.Add(new ExtCharAscii(this, tags, ch));
            }
            pos++;
        }

        ResetPosition(true);
    }

    private void DrawMarkings(Graphics gr, float scale, int offsetX, int offsetY) {
        if (_markedCharsCount == 0) { return; }
        DrawMarkingState(gr, scale, MarkState.Field, offsetX, offsetY);
        DrawMarkingState(gr, scale, MarkState.MyOwn, offsetX, offsetY);
        DrawMarkingState(gr, scale, MarkState.Other, offsetX, offsetY);
        DrawMarkingState(gr, scale, MarkState.Ringelchen, offsetX, offsetY);
    }

    private void DrawMarkingState(Graphics gr, float zoom, MarkState state, int offsetX, int offsetY) {
        var markStart = -1;

        for (var pos = 0; pos < _flatChars.Count; pos++) {
            var isMarked = _flatChars[pos].Marking.HasFlag(state);

            if (isMarked && markStart < 0) { markStart = pos; }

            if (!isMarked || pos == _flatChars.Count - 1) {
                if (markStart >= 0) {
                    var markEnd = pos == _flatChars.Count - 1 && isMarked ? pos : pos - 1;
                    DrawMarkingZone(gr, zoom, state, markStart, markEnd, offsetX, offsetY);
                    markStart = -1;
                }
            }
        }
    }

    private void DrawMarkingZone(Graphics gr, float zoom, MarkState state, int markStart, int markEnd, int offsetX, int offsetY) {
        var startX = _flatChars[markStart].PosCanvas.X.CanvasToControl(zoom, offsetX);
        var startY = _flatChars[markStart].PosCanvas.Y.CanvasToControl(zoom, offsetY);
        var endX = _flatChars[markEnd].PosCanvas.X.CanvasToControl(zoom, offsetX) + _flatChars[markEnd].SizeCanvas.Width.CanvasToControl(zoom);
        var endY = _flatChars[markEnd].PosCanvas.Y.CanvasToControl(zoom, offsetY) + _flatChars[markEnd].SizeCanvas.Height.CanvasToControl(zoom);

        switch (state) {
            case MarkState.Ringelchen:
                using (var pen = new Pen(Color.Red, 3.CanvasToControl(zoom))) {
                    var lineY = (int)(startY + (_flatChars[markStart].SizeCanvas.Height.CanvasToControl(zoom) * 0.9));
                    gr.DrawLine(pen, startX, lineY, endX, lineY);
                }
                break;

            case MarkState.Field:
                gr.FillRectangle(_brushField, startX, startY, endX - startX, endY - startY);
                break;

            case MarkState.MyOwn:
                gr.FillRectangle(_brushMyOwn, startX, startY, endX - startX, endY - startY);
                break;

            case MarkState.Other:
                gr.FillRectangle(_brushOther, startX, startY, endX - startX, endY - startY);
                break;

            default:
                Develop.DebugPrint(state);
                break;
        }
    }

    private void EnsurePositions() {
        if (_widthControl == null) {
            ComputeLayout();
        }
    }

    private static int FindWordBreak(List<ExtChar> chars, int fromPos, int minPos) {
        if (chars.Count <= 1) { return 0; }
        minPos = Math.Max(0, minPos);
        fromPos = Math.Min(fromPos, chars.Count - 1);
        fromPos = Math.Max(fromPos, minPos + 1);

        if (chars[fromPos - 1].IsSpace() && !chars[fromPos].IsPossibleLineBreak()) { return fromPos; }

        var started = fromPos;
        while (fromPos > minPos && chars[fromPos].IsPossibleLineBreak()) {
            fromPos--;
        }
        if (fromPos <= minPos) { return started; }

        while (fromPos > minPos) {
            if (chars[fromPos].IsPossibleLineBreak()) { return fromPos + 1; }
            fromPos--;
        }
        return started;
    }

    private BlueFont GetStructuralTagFont(string structTag) {
        var padStyle = structTag switch {
            "h1" => PadStyles.Title,
            "h2" => PadStyles.Subtitle,
            "h3" => PadStyles.Chapter,
            "h5" => PadStyles.Footnote,
            "h6" => PadStyles.Alternative,
            "strong" => PadStyles.Emphasized,
            _ => PadStyles.Standard
        };
        return Skin.GetBlueFont(SheetStyle, padStyle) ?? BaseFont;
    }

    private static float NormalizeRowHeight(List<ExtChar> chars, int first, int last) {
        if (first > last) { return 0f; }

        float maxHeight = 0;
        float rowBaseY = 0f;
        for (var i = first; i <= last; i++) {
            var ch = chars[i];
            if (ch.SizeCanvas.Height > maxHeight) {
                maxHeight = ch.SizeCanvas.Height;
                rowBaseY = ch.PosCanvas.Y;
            }
        }

        for (var i = first; i <= last; i++) {
            var ch = chars[i];
            if (ch.ResetsYPosition) {
                ch.PosCanvas.Y += maxHeight - ch.SizeCanvas.Height;
            } else if (ch.RowAlignment == Alignment.VerticalCenter) {
                ch.PosCanvas.Y = rowBaseY + (maxHeight - ch.SizeCanvas.Height) / 2f;
            } else if (ch.RowAlignment == Alignment.Top) {
                ch.PosCanvas.Y = rowBaseY;
            } else if (ch.SizeCanvas.Height > 0) {
                ch.PosCanvas.Y = rowBaseY + maxHeight - ch.SizeCanvas.Height;
            }
        }
        return maxHeight;
    }

    private void OnPropertyChanged([CallerMemberName] string propertyName = "unknown") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private int ParseHtmlEntity(string htmlText, int position, List<string> tags) {
        var endpos = htmlText.IndexOf(';', position + 1);

        if (endpos <= position || endpos > position + 10) {
            _internal.Add(new ExtCharAscii(this, tags, '&'));
            return position;
        }

        var entity = htmlText[position..(endpos + 1)];
        var decoded = System.Net.WebUtility.HtmlDecode(entity);
        if (decoded.Length == 1 && decoded[0] != '&') {
            _internal.Add(new ExtCharAscii(this, tags, decoded[0]));
            return endpos;
        }

        Develop.DebugPrint(ErrorType.Info, "Unbekannter Code: " + entity);
        _internal.Add(new ExtCharAscii(this, tags, '&'));
        return position;
    }

    private void ParseHtmlTag(string htmlText, int start, int endTagPos, Stack<List<string>> stack) {
        if (endTagPos <= start) { return; }

        var tagContent = htmlText[(start + 1)..endTagPos];
        var eqIdx = tagContent.IndexOf('=');

        string cod;
        string? attribut;

        if (eqIdx < 0) {
            cod = tagContent.ToUpperInvariant().Trim();
            attribut = string.Empty;
        } else {
            var beforeEq = tagContent[..eqIdx];
            var spaceIdx = beforeEq.IndexOf(' ');

            if (spaceIdx >= 0) {
                cod = beforeEq[..spaceIdx].ToUpperInvariant().Trim();
                attribut = tagContent[(spaceIdx + 1)..].Trim();
            } else {
                cod = beforeEq.Replace(" ", string.Empty).ToUpperInvariant().Trim();
                attribut = tagContent[(eqIdx + 1)..];
                if (attribut.IsEnclosedBy('"', '"')) { attribut = attribut[1..^1]; }
                attribut = attribut.Trim();
            }
        }

        switch (cod) {
            case "B" or "/B" or "I" or "/I" or "U" or "/U" or
                 "STRIKE" or "/STRIKE" or "FONTSIZE" or "FONTNAME" or
                 "FONTCOLOR" or "BACKCOLOR" or "OUTLINECOLOR" or
                 "COLOROUTLINE" or "FONTOUTLINE":
                ApplyFontTag(cod, attribut, stack);
                break;

            case "H1" or "/H1" or "H2" or "/H2" or "H3" or "/H3" or
                 "H4" or "H5" or "H6" or "H7" or "H0" or
                 "STRONG" or "/STRONG" or "P":
                ApplyStyleTag(cod, stack);
                break;

            default:
                ApplyStructuralTag(cod, attribut, stack);
                break;
        }
    }

    private void ResetPosition(bool clearTextCache) {
        if (IsDisposed) { return; }
        _widthControl = null;
        _heightControl = null;

        if (clearTextCache) {
            _tmpHtmlText = null;
            _tmpPlainText = null;
        }
        OnPropertyChanged("Position");
    }

    #endregion
}
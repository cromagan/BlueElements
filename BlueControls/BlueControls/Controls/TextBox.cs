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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Extended_Text;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueDatabase.EventArgs;
using static BlueBasics.Converter;
using Clipboard = System.Windows.Clipboard;
using MessageBox = BlueControls.Forms.MessageBox;
using Orientation = BlueBasics.Enums.Orientation;

namespace BlueControls.Controls;

[Designer(typeof(TextBoxDesigner))]
[DefaultEvent("TextChanged")]
public partial class TextBox : GenericControl, IContextMenu, IInputFormat {

    #region Fields

    private readonly ExtText _eTxt;
    private string _allowedChars = string.Empty;
    private int _cursorCharPos = -1;
    private bool _cursorVisible;
    private bool _formatierungErlaubt;
    private string _lastCheckedText = string.Empty;
    private DateTime _lastUserActionForSpellChecking = DateTime.UtcNow;
    private int _markEnd = -1;
    private int _markStart = -1;
    private int _maxTextLenght = 4000;
    private int _mouseValue;

    private bool _multiline;
    private bool _mustCheck = true;
    private string _regex = string.Empty;
    private Slider? _sliderY;

    private bool _spellChecking;

    private string _suffix = string.Empty;

    private SteuerelementVerhalten _verhalten = SteuerelementVerhalten.Scrollen_ohne_Textumbruch;

    #endregion

    #region Constructors

    public TextBox() : base(true, true) {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();
        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.

        _eTxt = new ExtText(GetDesign(), States.Standard);
        _eTxt.Changed += _eTxt_Changed;

        MouseHighlight = false;
    }

    #endregion

    #region Events

    public event EventHandler<ContextMenuInitEventArgs>? ContextMenuInit;

    public event EventHandler<ContextMenuItemClickedEventArgs>? ContextMenuItemClicked;

    public new event EventHandler? Enter;

    public event EventHandler? Esc;

    public event EventHandler<MultiUserFileGiveBackEventArgs>? NeedDatabaseOfAdditinalSpecialChars;

    public event EventHandler? Tab;

    public new event EventHandler? TextChanged;

    #endregion

    #region Properties

    [DefaultValue(AdditionalCheck.None)]
    public AdditionalCheck AdditionalFormatCheck { get; set; } = AdditionalCheck.None;

    [DefaultValue("")]
    public string AllowedChars {
        get => _allowedChars;
        set {
            if (value == _allowedChars) { return; }
            _allowedChars = value;
            GenerateEtxt(false);
        }
    }

    public override bool Focused => base.Focused || (_sliderY != null && _sliderY.Focused());

    [DefaultValue(false)]
    public bool FormatierungErlaubt {
        get => _formatierungErlaubt;
        set {
            if (value == FormatierungErlaubt) { return; }
            _formatierungErlaubt = value;
            GenerateEtxt(false);
        }
    }

    [DefaultValue(4000)]
    public int MaxTextLenght {
        get => _maxTextLenght;
        set {
            if (value == _maxTextLenght) { return; }
            _maxTextLenght = value;
            GenerateEtxt(false);
        }
    }

    [DefaultValue(false)]
    public bool MultiLine {
        get => _multiline;
        set {
            if (value == _multiline) { return; }
            _multiline = value;
            GenerateEtxt(false);
            if (_sliderY != null) {
                _sliderY.Visible = false;
                _sliderY.Value = 0;
            }
            Invalidate();
        }
    }

    [Browsable(false)]
    [DefaultValue("")]
    public string Prefix { get; set; } = string.Empty;

    [DefaultValue("")]
    public string Regex {
        get => _regex;
        set {
            if (value == _regex) { return; }
            _regex = value;
            GenerateEtxt(false);
        }
    }

    [DefaultValue(false)]
    public bool SpellCheckingEnabled {
        get => _spellChecking;
        set {
            if (_spellChecking == value) { return; }
            _spellChecking = value;
            AbortSpellChecking();
            Invalidate();
        }
    }

    [DefaultValue("")]
    public string Suffix {
        get => _suffix;
        set {
            if (value == _suffix) { return; }
            _suffix = value;
            Invalidate();
        }
    }

    [DefaultValue("")]
    public new string Text {
        get => _eTxt == null || IsDisposed ? string.Empty : _formatierungErlaubt ? _eTxt.HtmlText : _eTxt.PlainText;
        set {
            if (IsDisposed) { return; }
            if (!string.IsNullOrEmpty(value)) {
                value = value.Replace("\n", string.Empty);
                value = value.Replace("\r", "\r\n");
            }

            if (_formatierungErlaubt) {
                if (_eTxt != null && value == _eTxt.HtmlText) { return; }
            } else {
                if (_eTxt != null && value == _eTxt.PlainText) { return; }
            }
            AbortSpellChecking();

            lock (Dictionary.LockSpellChecking) {
                GenerateEtxt(true);
                if (_formatierungErlaubt) {
                    _eTxt.HtmlText = value;
                } else {
                    _eTxt.PlainText = value;
                }
                Invalidate();
                CheckIfTextIsChanded(value);
            }
        }
    }

    [DefaultValue(SteuerelementVerhalten.Scrollen_ohne_Textumbruch)]
    public SteuerelementVerhalten Verhalten {
        get => _verhalten;
        set {
            if (_verhalten == value) { return; }
            _verhalten = value;
            if (_sliderY != null) {
                _sliderY.Visible = false;
                _sliderY.Value = 0;
            }
            Invalidate();
        }
    }

    #endregion

    #region Methods

    public void Char_DelBereich(int von, int bis) {
        if (_markStart > -1 && _markEnd > -1) {
            von = _markStart;
            bis = _markEnd;
        }
        MarkClear();
        _cursorVisible = true;
        if (von < 0 || bis < 0) { return; }
        _cursorCharPos = von;
        _eTxt.Delete(von, bis);
    }

    public bool ContextMenuItemClickedInternalProcessig(object sender, ContextMenuItemClickedEventArgs e) {
        _ = Focus();
        var newWord = string.Empty;

        if (e.HotItem is not List<string> tags) { return false; }

        _markStart = IntParse(tags.TagGet("MarkStart"));
        _markEnd = IntParse(tags.TagGet("MarkEnd"));
        _cursorCharPos = IntParse(tags.TagGet("Cursorpos"));
        var word = tags.TagGet("Word");

        var tmp = e.ClickedComand;
        if (e.ClickedComand.StartsWith("#ChangeTo:")) {
            newWord = e.ClickedComand.Substring(10);
            tmp = "#ChangeTo";
        }

        switch (tmp) {
            case "#SpellAdd":
                Dictionary.WordAdd(word);
                _mustCheck = true;
                Invalidate();
                return true;

            case "#SpellAddLower":
                Dictionary.WordAdd(word.ToLower());
                _mustCheck = true;
                Invalidate();
                return true;

            case "#SpellChecking":
                FloatingForm.Close(this);
                if (_spellChecking) {
                    _mustCheck = false;
                    Dictionary.SpellCheckingAll(_eTxt, false);
                    _mustCheck = true;
                    Invalidate();
                }
                return true;

            case "#SpellChecking2":
                FloatingForm.Close(this);
                if (_spellChecking) {
                    _mustCheck = false;
                    Dictionary.SpellCheckingAll(_eTxt, true);
                    _mustCheck = true;
                    Invalidate();
                }
                break;

            case "Ausschneiden":
                Clipboard_Copy();
                Char_DelBereich(-1, -1);
                return true;

            case "Kopieren":
                Clipboard_Copy();
                return true;

            case "Einfügen":
                Clipboard_Paste();
                return true;

            case "#Caption":
                if (_markStart < 0 || _markEnd < 0) { return true; }
                Selection_Repair(true);
                _eTxt.StufeÄndern(_markStart, _markEnd - 1, 3);
                CheckIfTextIsChanded(_eTxt.HtmlText);
                return true;

            case "#NoCaption":
                if (_markStart < 0 || _markEnd < 0) { return true; }
                Selection_Repair(true);
                _eTxt.StufeÄndern(_markStart, _markEnd - 1, 4);
                CheckIfTextIsChanded(_eTxt.HtmlText);
                return true;

            case "#Bold":
                if (_markStart < 0 || _markEnd < 0) { return true; }
                Selection_Repair(true);
                _eTxt.StufeÄndern(_markStart, _markEnd - 1, 7);
                CheckIfTextIsChanded(_eTxt.HtmlText);
                return true;

            case "#Sonderzeichen":
                AddSpecialChar();
                return true;

            case "#ChangeTo":
                var ws = _eTxt.WordStart(_cursorCharPos);
                var we = _eTxt.WordEnd(_cursorCharPos);
                Char_DelBereich(ws, we);
                _cursorCharPos = ws;
                InsertText(newWord);
                _cursorCharPos = _eTxt.WordEnd(_cursorCharPos);
                return true;
        }
        return false;
    }

    //public new void Focus() {
    //    if (Focused()) { return; }
    //    base.Focus();
    //}

    public void GetContextMenuItems(MouseEventArgs? e, ItemCollectionList.ItemCollectionList items, out object? hotItem, ref bool cancel, ref bool translate) {
        AbortSpellChecking();

        var tmp = Cursor_PosAt(e.X, e.Y);
        var tmpWord = _eTxt.Word(tmp);

        var tags = new List<string>();
        tags.TagSet("MarkStart", _markStart.ToString());
        tags.TagSet("MarkEnd", _markEnd.ToString());
        tags.TagSet("Cursorpos", _cursorCharPos.ToString());
        tags.TagSet("Word", tmpWord);
        hotItem = tags;

        if (_spellChecking && !Dictionary.IsWordOk(tmpWord)) {
            _ = items.Add("Rechtschreibprüfung", true);
            if (Dictionary.IsSpellChecking) {
                _ = items.Add("Gerade ausgelastet...", "Gerade ausgelastet...", false);
                _ = items.AddSeparator();
            } else {
                var sim = Dictionary.SimilarTo(tmpWord);
                if (sim != null) {
                    foreach (var thisS in sim) {
                        _ = items.Add(" - " + thisS, "#ChangeTo:" + thisS);
                    }
                    _ = items.AddSeparator();
                }
                _ = items.Add("'" + tmpWord + "' ins Wörterbuch aufnehmen", "#SpellAdd", Dictionary.IsWriteable());
                if (tmpWord.ToLower() != tmpWord) {
                    _ = items.Add("'" + tmpWord.ToLower() + "' ins Wörterbuch aufnehmen", "#SpellAddLower", Dictionary.IsWriteable());
                }
                _ = items.Add("Schnelle Rechtschreibprüfung", "#SpellChecking", Dictionary.IsWriteable());
                _ = items.Add("Alle Wörter sind ok", "#SpellChecking2", Dictionary.IsWriteable());
                _ = items.AddSeparator();
            }
        }
        if (this is not ComboBox cbx || cbx.DropDownStyle == ComboBoxStyle.DropDown) {
            _ = items.Add(ContextMenuComands.Ausschneiden, (_markStart >= 0) && Enabled);
            _ = items.Add(ContextMenuComands.Kopieren, _markStart >= 0);
            _ = items.Add(ContextMenuComands.Einfügen, Clipboard.ContainsText() && Enabled);
            if (_formatierungErlaubt) {
                _ = items.AddSeparator();
                _ = items.Add("Sonderzeichen einfügen", "#Sonderzeichen", QuickImage.Get(ImageCode.Sonne, 16), _cursorCharPos > -1);
                if (_markEnd > -1) {
                    _ = items.AddSeparator();
                    _ = items.Add("Als Überschrift markieren", "#Caption", Skin.GetBlueFont(Design.TextBox_Stufe3, States.Standard).SymbolForReadableText(), _markEnd > -1);
                    _ = items.Add("Fettschrift", "#Bold", Skin.GetBlueFont(Design.TextBox_Bold, States.Standard).SymbolForReadableText(), _markEnd > -1);
                    _ = items.Add("Als normalen Text markieren", "#NoCaption", Skin.GetBlueFont(Design.TextBox, States.Standard).SymbolForReadableText(), _markEnd > -1);
                }
            }
        }
    }

    public void InsertText(string? nt) {
        if (nt == null) { return; }
        //nt = nt.Replace(Constants.beChrW1.ToString(), "\r");
        nt = nt.RemoveChars(Constants.Char_NotFromClip);
        if (!MultiLine) { nt = nt.RemoveChars("\r\n"); }

        foreach (var t in nt) {
            if (_eTxt.InsertChar((AsciiKey)t, _cursorCharPos)) { _cursorCharPos++; }
        }

        CheckIfTextIsChanded(_eTxt.HtmlText);
    }

    public void Mark(MarkState markstate, int first, int last) => _eTxt?.Mark(markstate, first, last);

    public void OnContextMenuInit(ContextMenuInitEventArgs e) => ContextMenuInit?.Invoke(this, e);

    public void OnContextMenuItemClicked(ContextMenuItemClickedEventArgs e) => ContextMenuItemClicked?.Invoke(this, e);

    /// <summary>
    /// Prüft - bei Multiline Zeile für Zeile - ob der Text in der Textbox zulässig ist.
    /// </summary>
    /// <param name="mitMeldung"></param>
    /// <returns>Ergibt Wahr, wenn der komplette Text dem Format entspricht. Andernfalls Falsch.</returns>
    /// <remarks></remarks>

    public bool Text_IsOkay(bool mitMeldung) {
        if (!_eTxt.PlainText.IsFormat(this)) {
            if (mitMeldung) { MessageBox.Show("Ihre Eingabe entspricht nicht<br>dem erwarteten Format.", ImageCode.Warnung, "OK"); }
            return false;
        }
        return true;
    }

    public void Unmark(MarkState markstate) => _eTxt?.Unmark(markstate);

    // Tastatur

    internal new void KeyPress(AsciiKey keyAscii) {
        // http://www.manderby.com/informatik/allgemeines/ascii.php
        if (_mouseValue != 0) { return; }

        switch (keyAscii) {
            case AsciiKey.DEL:
                // Eigentlich auch noch Ascii Code - steht bei ISO als Del
                Char_DelBereich(_cursorCharPos, _cursorCharPos + 1);
                break;

            case AsciiKey.ENTER:
                if (MultiLine) {
                    Char_DelBereich(-1, -1);
                    _eTxt.InsertCrlf(_cursorCharPos);
                    _cursorCharPos++;
                }
                break;

            case AsciiKey.BackSpace:
                Char_DelBereich(_cursorCharPos - 1, _cursorCharPos);
                break;

            case AsciiKey.StrgC:
                Clipboard_Copy();
                return;

            case AsciiKey.StrgV:
                Clipboard_Paste();
                break;

            case AsciiKey.StrgX:
                Clipboard_Copy();
                Char_DelBereich(-1, -1);
                break;

            case AsciiKey.StrgF:
            case AsciiKey.LineFeed:   //Zeilenumbruch, Kommt vor, wen man was aus einem anderen Programm kopiert,
            case AsciiKey.TAB:
                return;

            case AsciiKey.StrgA:
                MarkAll();
                break;

            default:
                if (keyAscii >= AsciiKey.Space) //Ascii-Codes (Außer 127 = DEL)
                {
                    Char_DelBereich(-1, -1);
                    if (_eTxt.InsertChar(keyAscii, _cursorCharPos)) { _cursorCharPos++; }
                }
                break;
        }

        CheckIfTextIsChanded(_eTxt.HtmlText);
    }

    internal bool WordStarts(string word, int position) {
        if (InvokeRequired) {
            return (bool)Invoke(new Func<bool>(() => WordStarts(word, position)));
        }
        try {
            //if (_eTxt == null) { GenerateEtxt(true); }
            //if (_eTxt == null) { return false; }
            if (position + word.Length > _eTxt.Count + 1) { return false; }
            if (position > 0 && !_eTxt[position - 1].IsWordSeperator()) { return false; }
            if (position + word.Length < _eTxt.Count && !_eTxt[position + word.Length].IsWordSeperator()) { return false; }
            var tt = _eTxt.ConvertCharToPlainText(position, position + word.Length - 1);
            return string.Equals(word, tt, StringComparison.OrdinalIgnoreCase);
        } catch {
            Develop.CheckStackForOverflow();
            return WordStarts(word, position);
        }
    }

    protected override void DrawControl(Graphics gr, States state) {
        //if (_eTxt == null) { GenerateETXT(true); }
        //if (state == enStates.Checked_Disabled) {
        //    Develop.DebugPrint("Checked Disabled");
        //    state = enStates.Checked_Disabled;
        //}
        //if (state == enStates.Standard_Disabled) {
        //    if (_MarkStart > -1) { Develop.DebugPrint("Disabled & Markstart:" + _MarkStart); }
        //    if (_MarkEnd > -1) { Develop.DebugPrint("Disabled & MarkEnd:" + _MarkStart); }
        //    MarkClear();
        //}
        // Erst den Typ richtig stellen, dann den State ändern!
        _eTxt.Design = GetDesign();
        _eTxt.State = state;

        var effectWidth = Width;
        var sliderVisible = _multiline ? _eTxt.Height() > Height - 16 : _eTxt.Height() > Height;
        if (sliderVisible) { effectWidth = Width - 18; }

        switch (_verhalten) {
            case SteuerelementVerhalten.Scrollen_mit_Textumbruch:
                _eTxt.TextDimensions = new Size(effectWidth - (Skin.PaddingSmal * 2), -1);
                _eTxt.DrawingArea = new Rectangle(0, 0, effectWidth, Height);
                break;

            case SteuerelementVerhalten.Scrollen_ohne_Textumbruch:
                var hp = HotPosition();
                _eTxt.TextDimensions = Size.Empty;
                _eTxt.DrawingArea = new Rectangle(0, 0, effectWidth, Height);
                var pos = _eTxt.DrawingPos;

                if (hp < 0) {
                    // Mach nix
                } else if (hp == 0) {
                    pos.X = Skin.PaddingSmal;
                } else if (hp > _eTxt.Count - 1) {
                    pos.X = _eTxt.Width() > Width - (Skin.PaddingSmal * 2) ? Width - _eTxt.Width() - (Skin.PaddingSmal * 2) : Skin.PaddingSmal;
                } else {
                    var r = _eTxt.CursorPixelPosX(hp);
                    if (r.X > Width - (Skin.PaddingSmal * 4) - pos.X) {
                        pos.X = Width - (Skin.PaddingSmal * 4) - r.X + 1;
                    } else if (r.X + pos.X < Skin.PaddingSmal * 2) {
                        pos.X = (Skin.PaddingSmal * 2) - r.X + 1;
                    }
                }
                if (pos.X > Skin.PaddingSmal) { pos.X = Skin.PaddingSmal; }

                _eTxt.DrawingPos = pos;
                break;

            case SteuerelementVerhalten.Steuerelement_Anpassen:
                sliderVisible = false;
                _eTxt.TextDimensions = Size.Empty;
                Width = this is ComboBox
                    ? Math.Max(_eTxt.Width() + (Skin.PaddingSmal * 3) + 20, Width)
                    : Math.Max(_eTxt.Width() + (Skin.PaddingSmal * 3), Width);
                Height = Math.Max(_eTxt.Height() + (Skin.PaddingSmal * 2), Height);
                _eTxt.DrawingArea = new Rectangle(0, 0, Width, Height);
                break;

            case SteuerelementVerhalten.Text_Abschneiden:
                sliderVisible = false;
                _eTxt.TextDimensions = Size.Empty;
                _eTxt.DrawingArea = new Rectangle(0, 0, Width, Height);
                break;

            default:
                Develop.DebugPrint(_verhalten);
                break;
        }

        if (sliderVisible) {
            GetSlider();
            _sliderY.Visible = true;
            _sliderY.Width = 18;
            _sliderY.Height = Height;
            _sliderY.Left = Width - _sliderY.Width;
            _sliderY.Top = 0;
            _eTxt.DrawingPos = _eTxt.DrawingPos with { Y = (int)-_sliderY.Value };
            _sliderY.Maximum = _eTxt.Height() + 16 - DisplayRectangle.Height;
        } else {
            if (_sliderY != null) {
                _sliderY.Visible = false;
                _sliderY.Value = 0;
            }
            _eTxt.DrawingPos = _eTxt.DrawingPos with { Y = Skin.PaddingSmal };
        }

        Skin.Draw_Back(gr, _eTxt.Design, state, DisplayRectangle, this, true);
        Cursor_Show(gr);
        MarkAndGenerateZone(gr);
        _eTxt.Draw(gr, 1);
        if (!string.IsNullOrEmpty(_suffix)) {
            Rectangle r = new(_eTxt.Width() + _eTxt.DrawingPos.X, _eTxt.DrawingPos.Y, 1000, 1000);
            if (_eTxt.Count > 0) {
                r.X += 2;
                Skin.Draw_FormatedText(gr, _suffix, _eTxt.Design, States.Standard_Disabled, null, Alignment.Top_Left, r, this, false, false);
            } else {
                Skin.Draw_FormatedText(gr, "[in " + _suffix + "]", _eTxt.Design, States.Standard_Disabled, null, Alignment.Top_Left, r, this, false, true);
            }
        }
        Skin.Draw_Border(gr, _eTxt.Design, state, DisplayRectangle);
        if (_mustCheck && !Dictionary.IsSpellChecking && Dictionary.DictionaryRunning(!DesignMode) && !SpellChecker.CancellationPending && !SpellChecker.IsBusy) { SpellChecker.RunWorkerAsync(); }
    }

    protected virtual Design GetDesign() => Design.TextBox;

    protected override bool IsInputKey(Keys keyData) =>
        // Ganz wichtig diese Routine!
        // Wenn diese NICHT ist, geht der Fokus weg, sobald der cursor gedrückt wird.
        // http://technet.microsoft.com/de-de/subscriptions/control.isinputkey%28v=vs.100%29
        keyData switch {
            Keys.Up or Keys.Down or Keys.Left or Keys.Right => true,
            _ => false
        };

    protected override void OnDoubleClick(System.EventArgs e) {
        base.OnDoubleClick(e);
        Selection_WortMarkieren(_markStart);
        _lastUserActionForSpellChecking = DateTime.UtcNow;
        _mouseValue = 9999;
        Invalidate();
    }

    protected override void OnEnabledChanged(System.EventArgs e) {
        MarkClear();
        base.OnEnabledChanged(e);
    }

    protected override void OnGotFocus(System.EventArgs e) {
        base.OnGotFocus(e);
        if (!Enabled) { return; }
        if (_eTxt == null) { GenerateEtxt(true); }
        if (!FloatingForm.IsShowing(this)) {
            SetCursorToEnd();
            if (!_eTxt.Multiline) { if (!ContainsMouse() || !MousePressing()) { MarkAll(); } }
            _lastUserActionForSpellChecking = DateTime.UtcNow.AddSeconds(-30);
        }
        Blinker.Enabled = true;
    }

    // Tastatur

    protected override void OnKeyDown(KeyEventArgs e) {
        base.OnKeyDown(e);

        if (!Enabled) { return; }
        _lastUserActionForSpellChecking = DateTime.UtcNow;
        if (_mouseValue != 0 || e == null) { return; }

        switch (e.KeyCode) {
            case Keys.Left:
                Cursor_Richtung(-1, 0);
                break;

            case Keys.Right:
                Cursor_Richtung(1, 0);
                break;

            case Keys.Down:
                Cursor_Richtung(0, 1);
                break;

            case Keys.Up:
                Cursor_Richtung(0, -1);
                break;

            case Keys.Delete:
                KeyPress(AsciiKey.DEL);
                CheckIfTextIsChanded(_eTxt.HtmlText);
                break;
        }

        _cursorVisible = true;
        Invalidate();
    }

    protected override void OnKeyPress(KeyPressEventArgs e) {
        base.OnKeyPress(e);
        _lastUserActionForSpellChecking = DateTime.UtcNow;
        if (!Enabled) {
            if (e.KeyChar != (int)AsciiKey.StrgX) { e.KeyChar = (char)AsciiKey.StrgC; }
            if (e.KeyChar != (int)AsciiKey.StrgC) { return; }
        }
        switch ((AsciiKey)e.KeyChar) {
            case AsciiKey.ENTER:
                KeyPress((AsciiKey)e.KeyChar);
                OnEnter();
                return;

            case AsciiKey.ESC:
                OnESC();
                return;

            case AsciiKey.TAB:
                OnTAB();
                return;

            default:
                KeyPress((AsciiKey)e.KeyChar);
                break;
        }
        Invalidate();
        if (IsDisposed) { return; }
        CheckIfTextIsChanded(_eTxt.HtmlText);
    }

    // Fokus
    protected override void OnLostFocus(System.EventArgs e) {
        base.OnLostFocus(e);
        _lastUserActionForSpellChecking = DateTime.UtcNow.AddSeconds(-30);
        if (!FloatingForm.IsShowing(this)) { MarkClear(); }
        Blinker.Enabled = false;
        CursorClear();
        Invalidate(); // Muss sein, weil evtl. der Cursor stehen bleibt
    }

    // Mouse
    protected override void OnMouseDown(MouseEventArgs e) {
        base.OnMouseDown(e);
        _lastUserActionForSpellChecking = DateTime.UtcNow;
        if (!Enabled) { return; }
        if (e.Button == MouseButtons.Right) { return; }
        _mouseValue = 1;
        _markStart = Cursor_PosAt(e.X, e.Y);
        _markEnd = _markStart;
        CursorClear();
        Selection_Repair(false);
        Invalidate();
    }

    protected override void OnMouseMove(MouseEventArgs e) {
        base.OnMouseMove(e);
        if (_eTxt == null) { return; }
        if (e.Button != MouseButtons.Left) { return; }
        if (!Enabled) { return; }
        _lastUserActionForSpellChecking = DateTime.UtcNow;
        CursorClear();
        _markEnd = Cursor_PosAt(e.X, e.Y);
        Selection_Repair(false);
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e) {
        base.OnMouseUp(e);
        _lastUserActionForSpellChecking = DateTime.UtcNow;
        if (Enabled) {
            if (_mouseValue == 9999) {
                //es Wurde Doppelgeklickt
            } else {
                if (_markStart == _markEnd || _markEnd < 0) {
                    _cursorCharPos = Cursor_PosAt(e.X, e.Y);
                    MarkClear();
                    if (e.Button == MouseButtons.Right) {
                        FloatingInputBoxListBoxStyle.ContextMenuShow(this, e);
                    }
                } else {
                    CursorClear();
                    Selection_Repair(true);
                    if (e.Button == MouseButtons.Right) {
                        FloatingInputBoxListBoxStyle.ContextMenuShow(this, e);
                    }
                }
            }
            _mouseValue = 0;
        } else {
            CursorClear();
        }
        Invalidate();
    }

    protected override void OnMouseWheel(MouseEventArgs e) {
        base.OnMouseWheel(e);
        if (_sliderY == null || !_sliderY.Visible) { return; }
        _lastUserActionForSpellChecking = DateTime.UtcNow;
        _sliderY.DoMouseWheel(e);
    }

    /// <summary>
    /// Löst das Ereignis aus und setzt _LastUserChangingTime auf NULL.
    /// </summary>
    protected virtual void OnTextChanged() {
        Develop.SetUserDidSomething();
        TextChanged?.Invoke(this, System.EventArgs.Empty);
    }

    private void _eTxt_Changed(object sender, System.EventArgs e) => Invalidate();

    private void AbortSpellChecking() {
        if (SpellChecker.IsBusy) { SpellChecker.CancelAsync(); }
    }

    private void AddSpecialChar() {
        var x = _cursorCharPos;
        MultiUserFileGiveBackEventArgs e = new();
        OnNeedDatabaseOfAdditinalSpecialChars(e);
        ItemCollectionList.ItemCollectionList i = new(BlueListBoxAppearance.Listbox, false)
        {
            //if (e.File is Database DB && DB.Bins.Count > 0) {
            //    foreach (var bmp in DB.Bins) {
            //        if (bmp.Picture != null) {
            //            if (!string.IsNullOrEmpty(bmp.Name)) {
            //                var crc = "DB_" + bmp.Name;
            //                i.GenerateAndAdd(bmp.Name, crc, QuickImage.Get(crc, 20));
            //            }
            //        }
            //    }
            //    i.AddSeparator();
            //}
            { "Kugel", "sphere", QuickImage.Get(ImageCode.Kugel, 20) },
            { "Warnung", "Warnung", QuickImage.Get(ImageCode.Warnung, 20) },
            { "Information", "Information", QuickImage.Get(ImageCode.Information, 20) },
            { "Kritisch", "Kritisch", QuickImage.Get(ImageCode.Kritisch, 20) },
            { "Frage", "Frage", QuickImage.Get(ImageCode.Frage, 20) }
        };
        var r = InputBoxListBoxStyle.Show("Wählen sie:", i, AddType.None, true);
        _cursorCharPos = x;
        if (r == null || r.Count != 1) { return; }
        Char_DelBereich(-1, -1);
        if (_eTxt.InsertImage(r[0], _cursorCharPos)) { _cursorCharPos++; }
        CheckIfTextIsChanded(_eTxt.HtmlText);
    }

    private void Blinker_Tick(object sender, System.EventArgs e) {
        if (!Focused) { return; }
        if (!Enabled) { return; }
        if (_markStart > -1 && _markEnd > -1) { _cursorCharPos = -1; }
        if (_cursorCharPos > -1) {
            _cursorVisible = !_cursorVisible;
            Invalidate();
        } else {
            if (_cursorVisible) {
                _cursorVisible = false;
                Invalidate();
            }
        }
    }

    private void CheckIfTextIsChanded(string newPlainText) {
        if (newPlainText == _lastCheckedText) { return; }
        _lastCheckedText = newPlainText;
        if (Dictionary.DictionaryRunning(!DesignMode)) { _mustCheck = true; }
        OnTextChanged();
    }

    private void Clipboard_Copy() {
        if (_markStart < 0 || _markEnd < 0) { return; }
        Selection_Repair(true);
        var tt = _eTxt.ConvertCharToPlainText(_markStart, _markEnd - 1);
        if (string.IsNullOrEmpty(tt)) { return; }
        tt = tt.Replace("\n", string.Empty);
        tt = tt.Replace("\r", "\r\n");
        _ = Generic.CopytoClipboard(tt);
    }

    private void Clipboard_Paste() {
        // VORSICHT!
        // Seltsames Verhalten von VB.NET
        // Anscheinend wird bei den Clipboard operationen ein DoEventXsx ausgelöst.
        // Dadurch kommt es zum Refresh des übergeordneten Steuerelementes, warscheinlich der Textbox.
        // Deshalb  muss 'Char_DelBereich' NACH den Clipboard-Operationen stattfinden.
        if (!Clipboard.ContainsText()) { return; }
        Char_DelBereich(-1, -1);
        InsertText(Clipboard.GetText());
    }

    /// <summary>
    /// Sucht den aktuellen Buchstaben, der unter den angegeben Koordinaten liegt.
    /// Wird auf die hintere Hälfte eines Zeichens gewählt, wird der nächste Buchstabe angegeben.
    /// </summary>
    /// <remarks></remarks>
    private int Cursor_PosAt(double pixX, double pixY) {
        if (_eTxt == null) { return -1; }
        // Das geht am Einfachsten....
        if (pixX < _eTxt.DrawingPos.X && pixY < _eTxt.DrawingPos.Y) { return 0; }
        pixX = Math.Max(pixX, _eTxt.DrawingPos.X);
        pixY = Math.Max(pixY, _eTxt.DrawingPos.Y);
        pixX = Math.Min(pixX, _eTxt.DrawingPos.X + _eTxt.Width());
        pixY = Math.Min(pixY, _eTxt.DrawingPos.Y + _eTxt.Height());
        var c = _eTxt.Char_Search(pixX, pixY);
        if (c < 0) { c = 0; }
        return c < _eTxt.Count && pixX > _eTxt.DrawingPos.X + _eTxt[c].Pos.X + (_eTxt[c].Size.Width / 2.0) ? c + 1 : c;
    }

    private void Cursor_Richtung(short x, short y) {
        if (x != 0) {
            if (_cursorCharPos > -1) {
                if (x == -1 && _cursorCharPos > 0) { _cursorCharPos--; }
                if (x == 1 && _cursorCharPos < _eTxt.Count) { _cursorCharPos++; }
            } else if (_markStart > -1 && _markEnd > -1) {
                if (x == -1) { _cursorCharPos = Math.Min(_markEnd, _markStart); }
                if (x == 1) { _cursorCharPos = Math.Max(_markEnd, _markStart) + 1; }
            } else {
                _cursorCharPos = 0;
            }
        }
        MarkClear();
        var ri = _eTxt.CursorPixelPosX(_cursorCharPos);
        if (_cursorCharPos < 0) { _cursorCharPos = 0; }
        if (y > 0) {
            _cursorCharPos = _cursorCharPos >= _eTxt.Count
                ? _eTxt.Count
                : Cursor_PosAt(ri.Left + _eTxt.DrawingPos.X, ri.Top + (ri.Height / 2.0) + _eTxt[_cursorCharPos].Size.Height + _eTxt.DrawingPos.Y);
        } else if (y < 0) {
            _cursorCharPos = _cursorCharPos >= _eTxt.Count
                ? _eTxt.Count > 0
                    ? Cursor_PosAt(ri.Left + _eTxt.DrawingPos.X, ri.Top + (ri.Height / 2.0) - _eTxt[_cursorCharPos - 1].Size.Height + _eTxt.DrawingPos.Y)
                    : 0
                : Cursor_PosAt(ri.Left + _eTxt.DrawingPos.X, ri.Top + (ri.Height / 2.0) - _eTxt[_cursorCharPos].Size.Height + _eTxt.DrawingPos.Y);
        }
        if (_cursorCharPos < 0) { _cursorCharPos = 0; }
        _cursorVisible = true;
    }

    private void Cursor_Show(Graphics gr) {
        if (!_cursorVisible) { return; }
        if (_cursorCharPos < 0) { return; }
        var r = _eTxt.CursorPixelPosX(_cursorCharPos);
        gr.DrawLine(new Pen(Color.Black), r.Left + _eTxt.DrawingPos.X, r.Top + _eTxt.DrawingPos.Y, r.Left + _eTxt.DrawingPos.X, r.Bottom + _eTxt.DrawingPos.Y);
    }

    private void CursorClear() {
        _cursorCharPos = -1;
        _cursorVisible = false;
    }

    /// <summary>
    /// Wenn das Format, die Maxlänge oder Multiline sich geändert haben,
    /// wird für das dementsprechende Format die Verbote/Erlaubnisse gesetzt.
    /// z.B. wird beim Format Datum die Maxlänge auf 10 gesetzt und nur noch Ziffern und Punkt erlaubt.
    /// </summary>
    /// <remarks></remarks>
    private void GenerateEtxt(bool resetCoords) {
        if (InvokeRequired) {
            _ = Invoke(new Action(() => GenerateEtxt(resetCoords)));
            return;
        }

        var state = States.Standard;
        if (!Enabled) { state = States.Standard_Disabled; }
        _eTxt.State = state;
        _eTxt.Design = GetDesign();

        _eTxt.Multiline = _multiline;
        _eTxt.AllowedChars = _allowedChars;

        if (resetCoords) {
            // Hier Standard-Werte Setzen, die Draw-Routine setzt bei Bedarf um
            if (_sliderY != null) {
                _sliderY.Visible = false;
                _sliderY.Value = 0;
            }
            _eTxt.DrawingPos = new Point(Skin.PaddingSmal, Skin.PaddingSmal);
            _eTxt.DrawingArea = new Rectangle(0, 0, -1, -1);
            _eTxt.TextDimensions = Size.Empty;
        }
    }

    private void GetSlider() {
        if (_sliderY != null) {
            return;
        }
        _sliderY = new Slider {
            Dock = DockStyle.Right,
            LargeChange = 10f,
            Location = new Point(Width - 18, 0),
            Maximum = 100f,
            Minimum = 0f,
            MouseChange = 1f,
            Name = "SliderY",
            Orientation = Orientation.Senkrecht,
            Size = new Size(18, Height),
            SmallChange = 48f,
            TabIndex = 0,
            TabStop = false,
            Value = 0f,
            Visible = true
        };
        _sliderY.ValueChanged += SliderY_ValueChange;
        Controls.Add(_sliderY);
    }

    private int HotPosition() => _cursorCharPos > -1 ? _cursorCharPos
        : _markStart > -1 && _markEnd < 0 ? _markEnd : _markStart > -1 && _markEnd > -1 ? _markEnd : -1;

    private void MarkAll() {
        if (_eTxt != null && _eTxt.Count > 0) {
            _markStart = 0;
            _markEnd = _eTxt.Count;
            _cursorVisible = false;
        } else {
            MarkClear();
        }
    }

    private void MarkAndGenerateZone(Graphics gr) {
        _eTxt.Check(0, _eTxt.Count - 1, false);
        if (_markStart < 0 || _markEnd < 0) { return; }
        Selection_Repair(false);
        var maS = Math.Min(_markStart, _markEnd);
        var maE = Math.Max(_markStart, _markEnd);
        if (maS == maE) { return; }
        _eTxt.Check(maS, maE - 1, true);
        var tmpcharS = maS;
        for (var cc = maS; cc <= maE; cc++) {
            if (cc == maE || _eTxt[cc].Pos.X < _eTxt[tmpcharS].Pos.X || Math.Abs(_eTxt[cc].Pos.Y - _eTxt[tmpcharS].Pos.Y) > 0.001) //Jetzt ist der Zeitpunkt zum Zeichen/start setzen
            {
                Rectangle r = new((int)(_eTxt[tmpcharS].Pos.X + _eTxt.DrawingPos.X),
                    (int)(_eTxt[tmpcharS].Pos.Y + 2 + _eTxt.DrawingPos.Y),
                    (int)(_eTxt[cc - 1].Pos.X + _eTxt[cc - 1].Size.Width - _eTxt[tmpcharS].Pos.X),
                    (int)(_eTxt[cc - 1].Pos.Y + _eTxt[cc - 1].Size.Height - _eTxt[tmpcharS].Pos.Y));
                if (r.Width < 2) { r = new Rectangle(r.Left, r.Top, 2, r.Height); }
                if (_eTxt[tmpcharS].State != States.Undefiniert) {
                    Skin.Draw_Back(gr, _eTxt.Design, _eTxt[tmpcharS].State, r, null, false);
                    Skin.Draw_Border(gr, _eTxt.Design, _eTxt[tmpcharS].State, r);
                }
                tmpcharS = cc;
            }
        }
    }

    private void MarkClear() {
        _markStart = -1;
        _markEnd = -1;
    }

    private void OnEnter() => Enter?.Invoke(this, System.EventArgs.Empty);

    private void OnESC() => Esc?.Invoke(this, System.EventArgs.Empty);

    private void OnNeedDatabaseOfAdditinalSpecialChars(MultiUserFileGiveBackEventArgs e) => NeedDatabaseOfAdditinalSpecialChars?.Invoke(this, e);

    private void OnTAB() => Tab?.Invoke(this, System.EventArgs.Empty);

    /// <summary>
    /// Prüft die den Selektions-Bereich auf nicht erlaubte Werte und Repariert diese.
    /// </summary>
    /// <param name="swapThem">Bei True werden MarkStart und MarkEnd richtig angeordnet. (Start kleiner/gleich End) </param>
    /// <remarks></remarks>
    private void Selection_Repair(bool swapThem) {
        if (_eTxt == null || _eTxt.Count == 0) { MarkClear(); }
        if (_markStart < 0 && _markEnd < 0) { return; }

        _markStart = Math.Max(_markStart, 0);
        _markStart = Math.Min(_markStart, _eTxt.Count);

        _markEnd = Math.Max(_markEnd, 0);
        _markEnd = Math.Min(_markEnd, _eTxt.Count);

        if (swapThem && _markStart > _markEnd) {
            Generic.Swap(ref _markStart, ref _markEnd);
        }
    }

    private void Selection_WortMarkieren(int pos) {
        if (_eTxt == null || _eTxt.Count == 0) {
            MarkClear();
            return;
        }
        _markStart = _eTxt.WordStart(pos);
        _markEnd = _eTxt.WordEnd(pos);
        CursorClear();
        Selection_Repair(true);
    }

    private void SetCursorToEnd() {
        if (_eTxt == null) {
            // Beim Durchtabben...
            _cursorCharPos = -1;
        } else {
            _cursorCharPos = _eTxt.Count;
        }
        _cursorVisible = true;
    }

    private void SliderY_ValueChange(object sender, System.EventArgs e) => Invalidate();

    private void SpellChecker_DoWork(object sender, DoWorkEventArgs e) {
        try {
            if (Dictionary.IsSpellChecking) { return; }
            if (DateTime.UtcNow.Subtract(_lastUserActionForSpellChecking).TotalSeconds < 2) { return; }

            Dictionary.IsSpellChecking = true;
            if (!_spellChecking) { return; }

            if (!Dictionary.DictionaryRunning(!DesignMode)) { return; }
            if (_eTxt == null) { return; }

            var pos = 0;
            var woEnd = -1;
            bool ok;
            do {
                ok = true;
                SpellChecker.ReportProgress(0, "Unmark");
                try {
                    do {
                        if (SpellChecker.CancellationPending) { return; }

                        pos = Math.Max(woEnd + 1, pos + 1);
                        if (_eTxt == null) { return; }// Das Textfeld ist spontan geschlossen worden.

                        if (pos >= _eTxt.Count) { break; }

                        var woStart = _eTxt.WordStart(pos);
                        if (woStart > -1) {
                            woEnd = _eTxt.WordEnd(pos);
                            var wort = _eTxt.Word(pos);
                            if (!Dictionary.IsWordOk(wort)) {
                                if (SpellChecker.CancellationPending) { return; }
                                SpellChecker.ReportProgress((int)(woEnd / (double)_eTxt.Count * 100), "Mark;" + woStart + ";" + (woEnd - 1));
                            }
                        }
                    } while (true);
                } catch {
                    ok = false;
                }
            } while (!ok);
        } catch (Exception ex) {
            Develop.DebugPrint("Rechtschreibprüfer-Fehler", ex);
        }
        SpellChecker.ReportProgress(100, "Done");
    }

    private void SpellChecker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
        if (SpellChecker.CancellationPending) { return; }//Ja, Multithreading ist kompliziert...

        var x = ((string)e.UserState).SplitAndCutBy(";");

        switch (x[0]) {
            case "Unmark":
                Unmark(MarkState.Ringelchen);
                Invalidate();
                break;

            case "Mark":
                Mark(MarkState.Ringelchen, IntParse(x[1]), IntParse(x[2]));
                Invalidate();
                break;

            case "Done":
                _mustCheck = false;
                break;

            default:
                Develop.DebugPrint(Convert.ToString(e.UserState));
                break;
        }
    }

    private void SpellChecker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) => Dictionary.IsSpellChecking = false;

    #endregion
}
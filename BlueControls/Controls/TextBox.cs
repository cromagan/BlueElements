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
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Extended_Text;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionList;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using static BlueBasics.Converter;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;
using Orientation = BlueBasics.Enums.Orientation;

namespace BlueControls.Controls;

[Designer(typeof(TextBoxDesigner))]
[DefaultEvent(nameof(TextChanged))]
public partial class TextBox : GenericControl, IContextMenu, IInputFormat {

    #region Fields

    private const string ExtCharFormat = "BlueElements.ExtChar";
    private readonly ExtText _eTxt;
    private int _blinkCount;
    private bool _cursorVisible;
    private string _lastCheckedText = string.Empty;
    private DateTime _lastUserActionForSpellChecking = DateTime.UtcNow;
    private int _markEnd = -1;
    private int _markStart = -1;
    private int _mouseValue;
    private bool _mustCheck = true;
    private int _raiseChangeDelay;
    private Slider? _sliderY;

    #endregion

    #region Constructors

    public TextBox() : base(true, true, false) {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();
        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.

        _eTxt = new ExtText(Design.TextBox, States.Standard); // Design auf Standard setzen wegen Virtal member call
        _eTxt.PropertyChanged += _eTxt_PropertyChanged;
    }

    #endregion

    #region Events

    public event EventHandler<ContextMenuInitEventArgs>? ContextMenuInit;

    public new event EventHandler? Enter;

    public event EventHandler? Esc;

    public event EventHandler? Tab;

    public new event EventHandler? TextChanged;

    #endregion

    #region Properties

    [DefaultValue(AdditionalCheck.None)]
    public AdditionalCheck AdditionalFormatCheck { get; set; } = AdditionalCheck.None;

    [DefaultValue("")]
    public string AllowedChars {
        get;
        set {
            if (value == field) { return; }
            field = value;
            GenerateEtxt(false);
        }
    } = string.Empty;

    public override bool Focused => base.Focused || _sliderY?.Focused == true;

    [DefaultValue(4000)]
    public int MaxTextLength {
        get;
        set {
            if (value == field) { return; }
            field = value;
            GenerateEtxt(false);
        }
    } = 4000;

    /// <summary>
    /// Falls das Steuerelement Multiline unterstützt, wird dieser angezeigt
    /// </summary>
    [DefaultValue(false)]
    public bool MultiLine {
        get;
        set {
            if (value == field) { return; }
            field = value;
            GenerateEtxt(false);
            if (_sliderY != null) {
                _sliderY.Visible = false;
                _sliderY.Value = 0;
            }
            Invalidate();
        }
    }

    [DefaultValue(0f)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int OffsetX { get; set; }

    [DefaultValue(0f)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int OffsetY { get; set; }

    [DefaultValue(0)]
    public int RaiseChangeDelay {
        get => _raiseChangeDelay / 2; // Umrechnung aus Sekunden
        set {
            if (_raiseChangeDelay == value * 2) { return; }
            _raiseChangeDelay = value * 2;
            RaiseEventIfTextChanged(false);
        }
    }

    [DefaultValue("")]
    public string RegexCheck {
        get;
        set {
            if (value == field) { return; }
            field = value;
            GenerateEtxt(false);
        }
    } = string.Empty;

    [DefaultValue(false)]
    public bool SpellCheckingEnabled {
        get;
        set {
            if (field == value) { return; }
            field = value;
            AbortSpellChecking();
            Invalidate();
        }
    }

    [DefaultValue("")]
    public string Suffix {
        get;
        set {
            if (value == field) { return; }
            field = value;
            Invalidate();
        }
    } = string.Empty;

    [DefaultValue("")]
    public new string Text {
        get => IsDisposed ? string.Empty : TextFormatingAllowed ? _eTxt.HtmlText : _eTxt.PlainText;
        set {
            if (IsDisposed) { return; }
            if (!string.IsNullOrEmpty(value)) {
                value = value.Replace("\n", string.Empty);
                value = value.Replace("\r", "\r\n");
            }

            if (TextFormatingAllowed) {
                if (value == _eTxt.HtmlText) { return; }
            } else {
                if (value == _eTxt.PlainText) { return; }
            }

            AbortSpellChecking();

            GenerateEtxt(true);

            if (TextFormatingAllowed) {
                _eTxt.HtmlText = value;
            } else {
                _eTxt.PlainText = value;
            }

            _mustCheck = true;
            Invalidate();
            RaiseEventIfTextChanged(true);  // Wichtig, z.B: für ComboBox
        }
    }

    [DefaultValue(false)]
    public bool TextFormatingAllowed {
        get;
        set {
            if (value == field) { return; }
            field = value;
            GenerateEtxt(false);
        }
    }

    [DefaultValue(SteuerelementVerhalten.Scrollen_ohne_Textumbruch)]
    public SteuerelementVerhalten Verhalten {
        get;
        set {
            if (field == value) { return; }
            field = value;
            if (_sliderY != null) {
                _sliderY.Visible = false;
                _sliderY.Value = 0;
            }
            Invalidate();
        }
    } = SteuerelementVerhalten.Scrollen_ohne_Textumbruch;

    protected virtual Design Design => Design.TextBox;

    #endregion

    #region Methods

    public void GetContextMenuItems(ContextMenuInitEventArgs e) {
        AbortSpellChecking();

        if (e.Mouse != null) {
            if (e.HotItem is not List<string> tags) { return; }

            var marS = IntParse(tags.TagGet("MarkStart"));
            var marE = IntParse(tags.TagGet("MarkEnd"));
            var tmpWord = _eTxt.Word(marS);

            if (SpellCheckingEnabled && !Dictionary.IsWordOk(tmpWord)) {
                e.ContextMenu.Add(ItemOf("Rechtschreibprüfung", true));
                if (Dictionary.IsSpellChecking) {
                    e.ContextMenu.Add(ItemOf("Gerade ausgelastet...", "Gerade ausgelastet...", false));
                    //_ = items.Add(AddSeparator());
                } else {
                    var sim = Dictionary.SimilarTo(tmpWord);
                    if (sim != null) {
                        foreach (var thisS in sim) {
                            e.ContextMenu.Add(ItemOf($" - {thisS}", null, Contextmenu_ChangeTo, new { NewWord = thisS, Start = marS, End = marE }, true));
                        }
                        e.ContextMenu.Add(Separator());
                    }
                    e.ContextMenu.Add(ItemOf($"'{tmpWord}' ins Wörterbuch aufnehmen", null, Contextmenu_SpellAdd, tmpWord, Dictionary.IsWriteable()));
                    if (tmpWord.ToLowerInvariant() != tmpWord) {
                        e.ContextMenu.Add(ItemOf($"'{tmpWord.ToLowerInvariant()}' ins Wörterbuch aufnehmen", null, Contextmenu_SpellAdd, tmpWord.ToLowerInvariant(), Dictionary.IsWriteable()));
                    }
                    e.ContextMenu.Add(ItemOf("Schnelle Rechtschreibprüfung", null, Contextmenu_SpellChecking, null, Dictionary.IsWriteable()));
                    e.ContextMenu.Add(ItemOf("Alle Wörter sind ok", null, Contextmenu_SpellAddAll, null, Dictionary.IsWriteable()));
                    e.ContextMenu.Add(Separator());
                }
            }
            if (this is not ComboBox { DropDownStyle: not ComboBoxStyle.DropDown }) {
                e.ContextMenu.Add(ItemOf("Ausschneiden", ImageCode.Schere, Contextmenu_Cut, new { Start = marS, End = marE }, marS >= 0));
                e.ContextMenu.Add(ItemOf("Kopieren", ImageCode.Kopieren, Contextmenu_Copy, new { Start = marS, End = marE }, marS >= 0));
                e.ContextMenu.Add(ItemOf("Einfügen (Text)", ImageCode.Clipboard, Contextmenu_Paste, new { Start = marS, End = marE, Link = false }, Clipboard.ContainsText() && Enabled));

                if (TextFormatingAllowed) {
                    e.ContextMenu.Add(ItemOf("Einfügen (Link)", ImageCode.Clipboard, Contextmenu_Paste, new { Start = marS, End = marE, Link = true }, Clipboard.ContainsData(TableView.CellDataFormat) && Enabled));
                    e.ContextMenu.Add(Separator());
                    e.ContextMenu.Add(ItemOf("Sonderzeichen einfügen", ImageCode.Sonne, Contextmenu_Sonderzeichen, new { Text = tmpWord, Start = marS, End = marE }, marS > -1));
                    if (marE > -1) {
                        e.ContextMenu.Add(Separator());
                        e.ContextMenu.Add(ItemOf("Als Überschrift markieren", Skin.GetBlueFont(Constants.Win11, PadStyles.Überschrift).SymbolForReadableText(), Contextmenu_Caption, new { Start = marS, End = marE }, marE > -1));
                        e.ContextMenu.Add(ItemOf("Fettschrift", Skin.GetBlueFont(Constants.Win11, PadStyles.Hervorgehoben).SymbolForReadableText(), Contextmenu_Bold, new { Start = marS, End = marE }, marE > -1));
                        e.ContextMenu.Add(ItemOf("Als normalen Text markieren", Skin.GetBlueFont(Constants.Win11, PadStyles.Standard).SymbolForReadableText(), Contextmenu_NoCaption, new { Start = marS, End = marE }, marE > -1));
                    }
                }
            }
        }

        OnContextMenuInit(e);
    }

    public void Mark(MarkState markstate, int first, int last) => _eTxt.Mark(markstate, first, last);

    public void OnContextMenuInit(ContextMenuInitEventArgs e) => ContextMenuInit?.Invoke(this, e);

    public void Unmark(MarkState markstate) => _eTxt.Unmark(markstate);

    internal new void KeyPress(AsciiKey keyAscii) {
        _blinkCount = 0;
        // http://www.manderby.com/informatik/allgemeines/ascii.php
        if (_mouseValue != 0) { return; }

        switch (keyAscii) {
            case AsciiKey.DEL:
                // Eigentlich auch noch Ascii Code - steht bei ISO als Del
                _markStart = Char_DelBereich(_markStart, Math.Max(_markStart + 1, _markEnd), true);
                _markEnd = -1;
                break;

            case AsciiKey.ENTER:
                if (MultiLine) {
                    _markStart = Char_DelBereich(_markStart, _markEnd, false);
                    _markEnd = -1;
                    _markStart = Insert(_markStart, new ExtCharCrlfCode(_eTxt, _markStart), true);
                }
                break;

            case AsciiKey.BackSpace:

                if (_markEnd < 0) {
                    _markStart = Char_DelBereich(Math.Max(_markStart - 1, 0), _markStart, true);
                } else {
                    _markStart = Char_DelBereich(_markStart, _markEnd, true);
                }

                _markEnd = -1;
                break;

            case AsciiKey.StrgC:
                Clipboard_Copy(_markStart, _markEnd);
                return;

            case AsciiKey.StrgV:
                _markStart = Char_DelBereich(_markStart, _markEnd, false);
                _markEnd = -1;
                _markStart = InsertClipboard(_markStart, false);
                break;

            case AsciiKey.StrgX:
                Clipboard_Copy(_markStart, _markEnd);
                _markStart = Char_DelBereich(_markStart, _markEnd, true);
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
                    _markStart = Char_DelBereich(_markStart, _markEnd, false);
                    _markEnd = -1;
                    _markStart = Insert(_markStart, keyAscii, true);
                }
                break;
        }
    }

    internal bool WordStarts(string word, int position) {
        if (InvokeRequired) {
            return (bool)Invoke(new Func<bool>(() => WordStarts(word, position)));
        }
        try {
            if (position + word.Length > _eTxt.Count + 1) { return false; }
            if (position > 0 && !_eTxt[position - 1].IsWordSeparator()) { return false; }
            if (position + word.Length < _eTxt.Count && !_eTxt[position + word.Length].IsWordSeparator()) { return false; }
            var tt = _eTxt.ConvertCharToPlainText(position, position + word.Length - 1);
            return string.Equals(word, tt, StringComparison.OrdinalIgnoreCase);
        } catch {
            Develop.AbortAppIfStackOverflow();
            return WordStarts(word, position);
        }
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            Blinker.Tick -= Blinker_Tick;
            _eTxt.PropertyChanged -= _eTxt_PropertyChanged;
            _eTxt.Dispose();
            components?.Dispose();
        }

        base.Dispose(disposing);
    }

    protected override void DrawControl(Graphics gr, States state) {
        if (IsDisposed) { return; }
        base.DrawControl(gr, state);

        var effectWidth = Width;
        var sliderVisible = MultiLine ? _eTxt.HeightControl > Height - 16 : _eTxt.HeightControl > Height;
        if (sliderVisible) { effectWidth = Width - 18; }

        switch (Verhalten) {
            case SteuerelementVerhalten.Scrollen_mit_Textumbruch:
                _eTxt.TextDimensions = new Size(effectWidth - (Skin.PaddingSmal * 2), -1);
                _eTxt.AreaControl = new Rectangle(0, 0, effectWidth, Height);
                break;

            case SteuerelementVerhalten.Scrollen_ohne_Textumbruch:
                var hp = HotPosition();
                _eTxt.TextDimensions = Size.Empty;
                _eTxt.AreaControl = new Rectangle(0, 0, effectWidth, Height);

                if (hp < 0) {
                    // Mach nix
                } else if (hp == 0) {
                    OffsetX = Skin.PaddingSmal;
                } else if (hp > _eTxt.Count - 1) {
                    OffsetX = _eTxt.WidthControl > Width - (Skin.PaddingSmal * 2) ? Width - _eTxt.WidthControl - (Skin.PaddingSmal * 2) : Skin.PaddingSmal;
                } else {
                    var r = _eTxt.CursorCanvasPosX(hp);
                    if (r.X > Width - (Skin.PaddingSmal * 4) - OffsetX) {
                        OffsetX = Width - (Skin.PaddingSmal * 4) - r.X + 1;
                    } else if (r.X + OffsetX < Skin.PaddingSmal * 2) {
                        OffsetX = (Skin.PaddingSmal * 2) - r.X + 1;
                    }
                }
                if (OffsetX > Skin.PaddingSmal) { OffsetX = Skin.PaddingSmal; }

                break;

            case SteuerelementVerhalten.Steuerelement_Anpassen:
                sliderVisible = false;
                _eTxt.TextDimensions = Size.Empty;
                Width = this is ComboBox
                    ? Math.Max(_eTxt.WidthControl + (Skin.PaddingSmal * 3) + 20, Width)
                    : Math.Max(_eTxt.WidthControl + (Skin.PaddingSmal * 3), Width);
                Height = Math.Max(_eTxt.HeightControl + (Skin.PaddingSmal * 2), Height);
                _eTxt.AreaControl = new Rectangle(0, 0, Width, Height);
                break;

            case SteuerelementVerhalten.Text_Abschneiden:
                sliderVisible = false;
                _eTxt.TextDimensions = Size.Empty;
                _eTxt.AreaControl = new Rectangle(0, 0, Width, Height);
                break;

            default:
                Develop.DebugPrint(Verhalten);
                break;
        }

        int offsetX = 0;
        int offsetY = 0;

        if (sliderVisible) {
            GenerateSlider();
            if (_sliderY != null) {
                _sliderY.Visible = true;
                _sliderY.Width = 18;
                _sliderY.Height = Height;
                _sliderY.Left = Width - _sliderY.Width;
                _sliderY.Top = 0;
                offsetY = -(int)_sliderY.Value;
                _sliderY.Maximum = _eTxt.HeightControl + 16 - DisplayRectangle.Height;
            }
        } else {
            if (_sliderY != null) {
                _sliderY.Visible = false;
                _sliderY.Value = 0;
            }

            offsetY = Skin.PaddingSmal;
        }

        Skin.Draw_Back(gr, Design, state, DisplayRectangle, this, true);
        Cursor_Show(gr);

        _eTxt.Draw(gr, 1, offsetX, offsetY);
        MarkAndGenerateZone(gr, state);

        if (!string.IsNullOrEmpty(Suffix)) {
            var r = new Rectangle(_eTxt.WidthControl + offsetX, offsetY, 1000, 1000);
            if (_eTxt.Count > 0) {
                r.X += 2;
                Skin.Draw_FormatedText(gr, Suffix, null, Alignment.Top_Left, r, Design, States.Standard_Disabled, this, false, false);
            } else {
                Skin.Draw_FormatedText(gr, "[in " + Suffix + "]", null, Alignment.Top_Left, r, Design, States.Standard_Disabled, this, false, true);
            }
        }
        Skin.Draw_Border(gr, Design, state, DisplayRectangle);
        if (_mustCheck && !Dictionary.IsSpellChecking && Dictionary.DictionaryRunning(!DesignMode) && SpellChecker is { CancellationPending: false, IsBusy: false }) { SpellChecker.RunWorkerAsync(); }
    }

    protected override bool IsInputKey(Keys keyData) {
        // Ganz wichtig diese Routine!
        // Wenn diese NICHT ist, geht der Fokus weg, sobald der cursor gedrückt wird.
        // http://technet.microsoft.com/de-de/subscriptions/control.isinputkey%28v=vs.100%29
        switch (keyData) {
            case Keys.Up or Keys.Down or Keys.Left or Keys.Right:
                return true;

            default:
                return false;
        }
    }

    protected override void OnDoubleClick(System.EventArgs e) {
        base.OnDoubleClick(e);
        Selection_WortMarkieren(_markStart);
        _lastUserActionForSpellChecking = DateTime.UtcNow;
        _mouseValue = 9999;
        Invalidate();
    }

    protected override void OnEnabledChanged(System.EventArgs e) {
        RaiseEventIfTextChanged(true);
        _markStart = -1;
        _markEnd = -1;

        base.OnEnabledChanged(e);
    }

    protected override void OnGotFocus(System.EventArgs e) {
        base.OnGotFocus(e);
        if (!Enabled) { return; }

        if (!FloatingForm.IsShowing(this)) {
            _markStart = _eTxt.Count;
            _markEnd = -1;

            if (!_eTxt.Multiline) { if (!ContainsMouse() || !MousePressing()) { MarkAll(); } }
            _lastUserActionForSpellChecking = DateTime.UtcNow.AddSeconds(-30);
        }
        Blinker.Enabled = true;
    }

    protected override void OnKeyDown(KeyEventArgs e) {
        base.OnKeyDown(e);
        _blinkCount = 0;

        if (!Enabled) { return; }
        _lastUserActionForSpellChecking = DateTime.UtcNow;
        if (_mouseValue != 0) { return; }

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
                break;
        }

        _cursorVisible = true;
        Invalidate();
    }

    // Tastatur
    protected override void OnKeyPress(KeyPressEventArgs e) {
        _blinkCount = 0;
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
        RaiseEventIfTextChanged(false);
    }

    protected override void OnLostFocus(System.EventArgs e) {
        RaiseEventIfTextChanged(true);
        base.OnLostFocus(e);
        _lastUserActionForSpellChecking = DateTime.UtcNow.AddSeconds(-30);
        if (!FloatingForm.IsShowing(this)) { _markStart = -1; _markEnd = -1; }
        Blinker.Enabled = false;
        _cursorVisible = false;
        Invalidate(); // Muss sein, weil evtl. der Cursor stehen bleibt
    }

    // Mouse
    protected override void OnMouseDown(MouseEventArgs e) {
        base.OnMouseDown(e);
        _lastUserActionForSpellChecking = DateTime.UtcNow;
        if (!Enabled) { return; }
        _cursorVisible = true;
        if (e.Button == MouseButtons.Right) { return; }
        _mouseValue = 1;
        _markStart = Cursor_PosAt(e.X, e.Y);
        _markEnd = -1;
        Selection_Repair(false);
        Invalidate();
    }

    protected override void OnMouseMove(MouseEventArgs e) {
        base.OnMouseMove(e);
        if (e.Button != MouseButtons.Left) { return; }
        if (!Enabled) { return; }
        _lastUserActionForSpellChecking = DateTime.UtcNow;
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
                if (e.Button == MouseButtons.Right) {
                    var tags = new List<string>();
                    tags.TagSet("MarkStart", _markStart.ToString());
                    tags.TagSet("MarkEnd", _markEnd.ToString());
                    FloatingInputBoxListBoxStyle.ContextMenuShow(this, tags, e);
                } else if (e.Button == MouseButtons.Left) {
                    _markEnd = Cursor_PosAt(e.X, e.Y);
                    Selection_Repair(true);
                }
            }
            _mouseValue = 0;
        } else {
            _markStart = -1;
            _markEnd = -1;
        }
        Invalidate();
    }

    protected override void OnMouseWheel(MouseEventArgs e) {
        base.OnMouseWheel(e);
        if (_sliderY is not { Visible: true }) { return; }
        _lastUserActionForSpellChecking = DateTime.UtcNow;
        _sliderY.DoMouseWheel(e);
    }

    /// <summary>
    /// Löst das Ereignis aus und setzt _LastUserChangingTime auf NULL.
    /// </summary>
    protected void OnTextChanged() {
        Develop.SetUserDidSomething();
        TextChanged?.Invoke(this, System.EventArgs.Empty);
    }

    protected override void OnVisibleChanged(System.EventArgs e) {
        RaiseEventIfTextChanged(true);
        base.OnVisibleChanged(e);
    }

    private void _eTxt_PropertyChanged(object sender, PropertyChangedEventArgs e) => Invalidate();

    private void AbortSpellChecking() {
        if (SpellChecker.IsBusy) { SpellChecker.CancelAsync(); }
    }

    private void Blinker_Tick(object sender, System.EventArgs e) {
        if (_blinkCount < _raiseChangeDelay + 1 && _raiseChangeDelay > 0) {
            _blinkCount++;
            RaiseEventIfTextChanged(false);
        }

        if (!Focused) { return; }
        if (!Enabled) { return; }

        if (_markStart > -1 && _markEnd == -1) {
            _cursorVisible = !_cursorVisible;
            Invalidate();
        } else {
            if (_cursorVisible) {
                _cursorVisible = false;
                Invalidate();
            }
        }
    }

    private int Char_DelBereich(int von, int bis, bool raiseEvent) {
        _cursorVisible = true;
        if (von < 0 && bis <= 0) { return 0; }
        if (von < 0 || bis < 0 || von == bis) { return von; }

        _eTxt.Delete(von, bis);

        if (raiseEvent) {
            RaiseEventIfTextChanged(false);
            Invalidate();
        }

        return Math.Min(von, bis);
    }

    private void Clipboard_Copy(int markStart, int markEnd) {
        if (markStart < 0 || markEnd < 0) { return; }
        Selection_Repair(true);

        var l = new List<string>();
        try {
            for (var i = markStart; i < markEnd; i++) {
                l.Add(_eTxt[i].ToString());
            }
        } catch { }

        var dataObject = new DataObject();
        dataObject.SetData(ExtCharFormat, l.JoinWithCr());// 1. Als ExtChar-Format (für interne Verwendung)
        dataObject.SetText(_eTxt.ConvertCharToPlainText(markStart, markEnd - 1));// 2. Als Plain Text (für externe Anwendungen)
        Clipboard.SetDataObject(dataObject, true);
    }

    private void Contextmenu_Bold(object sender, ObjectEventArgs e) {
        if (e.Data is not { } data) { return; }
        var start = (int)(data.GetType().GetProperty("Start")?.GetValue(data) ?? -1);
        var end = (int)(data.GetType().GetProperty("End")?.GetValue(data) ?? -1);

        if (start < 0 || end < 0) { return; }

        _markStart = start;
        _markEnd = end;
        Selection_Repair(true);
        _eTxt.ChangeStyle(_markStart, _markEnd - 1, PadStyles.Hervorgehoben);
        Invalidate();
        RaiseEventIfTextChanged(false);
    }

    private void Contextmenu_Caption(object sender, ObjectEventArgs e) {
        if (e.Data is not { } data) { return; }
        var start = (int)(data.GetType().GetProperty("Start")?.GetValue(data) ?? -1);
        var end = (int)(data.GetType().GetProperty("End")?.GetValue(data) ?? -1);

        if (start < 0 || end < 0) { return; }

        //_markStart = start;
        //_markEnd = end;
        Selection_Repair(true);
        _eTxt.ChangeStyle(start, end - 1, PadStyles.Überschrift);
        Invalidate();
        RaiseEventIfTextChanged(false);
    }

    private void Contextmenu_ChangeTo(object sender, ObjectEventArgs e) {
        if (e.Data is not { } data) { return; }
        var newWord = data.GetType().GetProperty("NewWord")?.GetValue(data)?.ToString() ?? string.Empty;
        var start = (int)(data.GetType().GetProperty("Start")?.GetValue(data) ?? -1);
        var end = (int)(data.GetType().GetProperty("End")?.GetValue(data) ?? -1);

        if (string.IsNullOrEmpty(newWord) || start < 0 || end < 0) { return; }

        _markStart = Char_DelBereich(start, end, false);
        _markEnd = -1;
        _markStart = Insert(_markStart, newWord, true);
    }

    private void Contextmenu_Copy(object sender, ObjectEventArgs e) {
        if (e.Data is not { } data) { return; }
        var start = (int)(data.GetType().GetProperty("Start")?.GetValue(data) ?? -1);
        var end = (int)(data.GetType().GetProperty("End")?.GetValue(data) ?? -1);

        Clipboard_Copy(start, end);
    }

    private void Contextmenu_Cut(object sender, ObjectEventArgs e) {
        if (e.Data is not { } data) { return; }
        var start = (int)(data.GetType().GetProperty("Start")?.GetValue(data) ?? -1);
        var end = (int)(data.GetType().GetProperty("End")?.GetValue(data) ?? -1);

        Clipboard_Copy(start, end);
        if (!Enabled) { return; }

        _markStart = Char_DelBereich(start, end, true);
        _markEnd = -1;
    }

    private void Contextmenu_NoCaption(object sender, ObjectEventArgs e) {
        if (e.Data is not { } data) { return; }
        var start = (int)(data.GetType().GetProperty("Start")?.GetValue(data) ?? -1);
        var end = (int)(data.GetType().GetProperty("End")?.GetValue(data) ?? -1);

        if (start < 0 || end < 0) { return; }

        _markStart = start;
        _markEnd = end;
        Selection_Repair(true);
        _eTxt.ChangeStyle(_markStart, _markEnd - 1, PadStyles.Standard);
        Invalidate();
        RaiseEventIfTextChanged(false);
    }

    private void Contextmenu_Paste(object sender, ObjectEventArgs e) {
        if (e.Data is not { } data) { return; }
        var start = (int)(data.GetType().GetProperty("Start")?.GetValue(data) ?? -1);
        var end = (int)(data.GetType().GetProperty("End")?.GetValue(data) ?? -1);
        var linkallowed = (bool)(data.GetType().GetProperty("Link")?.GetValue(data) ?? false);
        _markStart = Char_DelBereich(start, end, false);
        _markEnd = -1;

        _markStart = InsertClipboard(_markStart, linkallowed);
    }

    private void Contextmenu_Sonderzeichen(object sender, ObjectEventArgs e) {
        if (e.Data is not { } data) { return; }
        var start = (int)(data.GetType().GetProperty("Start")?.GetValue(data) ?? -1);
        var end = (int)(data.GetType().GetProperty("End")?.GetValue(data) ?? -1);

        List<AbstractListItem> i =
        [
            ItemOf("Kugel", "sphere", QuickImage.Get(ImageCode.Kugel, 20)),
        ItemOf("Warnung", "Warnung", QuickImage.Get(ImageCode.Warnung, 20)),
        ItemOf("Information", "Information", QuickImage.Get(ImageCode.Information, 20)),
        ItemOf("Kritisch", "Kritisch", QuickImage.Get(ImageCode.Kritisch, 20)),
        ItemOf("Frage", "Frage", QuickImage.Get(ImageCode.Frage, 20))
        ];

        var r = InputBoxListBoxStyle.Show("Wählen sie:", i, CheckBehavior.SingleSelection, null, AddType.None);

        if (r is not { Count: 1 }) { return; }

        _markStart = Char_DelBereich(start, end, false);
        _markStart = Insert(_markStart, new ExtCharImageCode(_eTxt, _markStart, QuickImage.Get(r[0])), true);
    }

    private void Contextmenu_SpellAdd(object sender, ObjectEventArgs e) {
        if (e.Data is not string word) { return; }

        if (string.IsNullOrEmpty(word)) { return; }

        Dictionary.WordAdd(word);
        _mustCheck = true;
        Invalidate();
    }

    private void Contextmenu_SpellAddAll(object sender, ObjectEventArgs e) {
        FloatingForm.Close(this);
        if (SpellCheckingEnabled) {
            _mustCheck = false;
            Dictionary.SpellCheckingAll(_eTxt, true);
            _mustCheck = true;
            Invalidate();
        }
    }

    private void Contextmenu_SpellChecking(object sender, ObjectEventArgs e) {
        FloatingForm.Close(this);
        if (SpellCheckingEnabled) {
            _mustCheck = false;
            Dictionary.SpellCheckingAll(_eTxt, false);
            _mustCheck = true;
            Invalidate();
        }
    }

    /// <summary>
    /// Sucht den aktuellen Buchstaben, der unter den angegeben Koordinaten liegt.
    /// Wird auf die hintere Hälfte eines Zeichens gewählt, wird der nächste Buchstabe angegeben.
    /// </summary>
    /// <remarks></remarks>
    private int Cursor_PosAt(int controlX, int controlY) {
        // Das geht am Einfachsten....
        if (controlX < OffsetX && controlY < OffsetY) { return 0; }
        controlX = Math.Max(controlX, OffsetX);
        controlY = Math.Max(controlY, OffsetY);
        controlX = Math.Min(controlX, OffsetX + _eTxt.WidthControl);
        controlY = Math.Min(controlY, OffsetY + _eTxt.HeightControl);
        var c = _eTxt.Char_Search(controlX, controlY);
        if (c < 0) { c = 0; }
        return c < _eTxt.Count && controlX > OffsetX + _eTxt[c].PosCanvas.X + (_eTxt[c].SizeCanvas.Width / 2.0) ? c + 1 : c;
    }

    private void Cursor_Richtung(short x, short y) {
        if (x != 0) {
            if (_markStart > -1 && _markEnd > -1) {
                if (x == -1) { _markStart = Math.Min(_markEnd, _markStart); }
                if (x == 1) { _markStart = Math.Max(_markEnd, _markStart) + 1; }
            } else if (_markStart > -1) {
                if (x == -1 && _markStart > 0) { _markStart--; }
                if (x == 1 && _markStart < _eTxt.Count) { _markStart++; }
            } else {
                _markStart = 0;
            }
        }

        if (y != 0) {
            var ri = _eTxt.CursorCanvasPosX(_markStart);
            if (_markStart < 0) { _markStart = 0; }

            if (y > 0) {
                _markStart = _markStart >= _eTxt.Count
                    ? _eTxt.Count
                    : Cursor_PosAt(ri.Left + OffsetX, (int)(ri.Top + (ri.Height / 2f) + _eTxt[_markStart].SizeCanvas.Height + OffsetY));
            } else if (y < 0) {
                _markStart = _markStart >= _eTxt.Count
                    ? _eTxt.Count > 0
                        ? Cursor_PosAt(ri.Left + OffsetX, (int)(ri.Top + (ri.Height / 2f) - _eTxt[_markStart - 1].SizeCanvas.Height + OffsetY))
                        : 0
                    : Cursor_PosAt(ri.Left + OffsetX, (int)(ri.Top + (ri.Height / 2f) - _eTxt[_markStart].SizeCanvas.Height + OffsetY));
            }
        }

        if (_markStart < 0) { _markStart = 0; }
        _markEnd = -1;
        _cursorVisible = true;
    }

    private void Cursor_Show(Graphics gr) {
        if (!_cursorVisible) { return; }
        if (_markStart < 0 || _markEnd > -1) { return; }
        var r = _eTxt.CursorCanvasPosX(_markStart);
        gr.DrawLine(new Pen(Color.Black), r.Left + OffsetX, r.Top + OffsetY, r.Left + OffsetX, r.Bottom + OffsetY);
    }

    /// <summary>
    /// Wenn das Format, die Maxlänge oder Multiline sich geändert haben,
    /// wird für das dementsprechende Format die Verbote/Erlaubnisse gesetzt.
    /// z.B. wird beim Format Datum die Maxlänge auf 10 gesetzt und nur noch Ziffern und Punkt erlaubt.
    /// </summary>
    /// <remarks></remarks>
    private void GenerateEtxt(bool resetCoords) {
        if (InvokeRequired) {
            Invoke(new Action(() => GenerateEtxt(resetCoords)));
            return;
        }

        //var state = States.Standard;
        //if (!Enabled) { state = States.Standard_Disabled; }
        //_eTxt.State = state;

        _eTxt.Multiline = MultiLine;
        _eTxt.AllowedChars = AllowedChars;

        if (resetCoords) {
            // Hier Standard-Werte Setzen, die Draw-Routine setzt bei Bedarf um
            if (_sliderY != null) {
                _sliderY.Visible = false;
                _sliderY.Value = 0;
            }
            OffsetX = Skin.PaddingSmal;
            OffsetY = Skin.PaddingSmal;
            _eTxt.AreaControl = new Rectangle(0, 0, -1, -1);
            _eTxt.TextDimensions = Size.Empty;
        }
    }

    private void GenerateSlider() {
        if (_sliderY != null) { return; }

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

    private int HotPosition() => _markStart > -1 ? _markStart
        : _markStart > -1 && _markEnd < 0 ? _markEnd : _markStart > -1 && _markEnd > -1 ? _markEnd : -1;

    private int Insert(int pos, string? nt, bool raiseEvent) {
        if (nt == null) { return pos; }
        nt = nt.RemoveChars(Constants.Char_NotFromClip);
        if (!MultiLine) { nt = nt.RemoveChars("\r\n"); }

        foreach (var t in nt) {
            pos = Insert(pos, t, false);
        }

        if (raiseEvent) {
            RaiseEventIfTextChanged(false);
            Invalidate();
        }

        return pos;
    }

    private int Insert(int pos, Char c, bool raiseEvent) {
        if (c < 13) { return pos; }
        return Insert(pos, new ExtCharAscii(_eTxt, pos, c), raiseEvent);
    }

    private int Insert(int pos, AsciiKey keyAscii, bool raiseEvent) => Insert(pos, (char)keyAscii, raiseEvent);

    private int Insert(int pos, ExtChar chr, bool raiseEvent) {
        if (_eTxt.Insert(pos, chr)) {
            if (raiseEvent) { RaiseEventIfTextChanged(false); }
            return pos + 1;
        }
        if (raiseEvent) { RaiseEventIfTextChanged(false); }
        return pos;
    }

    private int InsertClipboard(int pos, bool linkAllowed) {
        if (TextFormatingAllowed) {
            if (linkAllowed && Clipboard.ContainsData(TableView.CellDataFormat)) {
                if (Clipboard.GetData(TableView.CellDataFormat) is string sd && !string.IsNullOrEmpty(sd)) {
                    var t = sd.SplitByCr();
                    var c = new ExtCharCellLink(_eTxt, pos, t[0], t[1], t[2]);
                    pos = Insert(pos, c, true);
                    return pos;
                }
            }

            if (Clipboard.ContainsData(ExtCharFormat)) {
                if (Clipboard.GetData(ExtCharFormat) is not string sd || string.IsNullOrEmpty(sd)) { return pos; }

                foreach (var thiss in sd.SplitByCr()) {
                    if (_eTxt.Count < MaxTextLength) {
                        var extChar = ParseableItem.NewByParsing<ExtChar>(thiss, _eTxt, pos);

                        if (extChar != null) {
                            pos = Insert(pos, extChar, false);
                        }
                    }
                }

                RaiseEventIfTextChanged(false);
                return pos;
            }
        }

        if (Clipboard.ContainsText()) {
            return Insert(pos, Clipboard.GetText(), true);
        }

        // Eine vorherige Aktion kann Text löschen und kein Event auslösen. Also hier müssen wir!
        RaiseEventIfTextChanged(false);
        Invalidate();

        return pos;
    }

    private void MarkAll() {
        if (_eTxt.Count > 0) {
            _markStart = 0;
            _markEnd = _eTxt.Count;
            _cursorVisible = false;
        } else {
            _markStart = -1;
            _markEnd = -1;
        }
    }

    private void MarkAndGenerateZone(Graphics gr, States state) {
        Selection_Repair(false);
        if (_markEnd < 0) { return; }

        var mas = Math.Min(_markStart, _markEnd);
        var mae = Math.Max(_markStart, _markEnd);

        if (mas == mae) { return; }

        var sr = state | States.Checked;

        for (var cc = mas; cc < mae; cc++) {
            if (_eTxt[cc].IsVisible(_eTxt.AreaControl, 1f, OffsetX, OffsetY)) {
                var f = _eTxt[cc].Font;
                _eTxt[cc].Font = Skin.GetBlueFont(Design.TextBox, sr);
                _eTxt[cc].Draw(gr, 1f, OffsetX, OffsetY);
                _eTxt[cc].Font = f;
            }
        }
    }

    private void OnEnter() => Enter?.Invoke(this, System.EventArgs.Empty);

    private void OnESC() => Esc?.Invoke(this, System.EventArgs.Empty);

    private void OnTAB() => Tab?.Invoke(this, System.EventArgs.Empty);

    private void RaiseEventIfTextChanged(bool doChangeNow) {
        var newtext = _eTxt.HtmlText;

        if (newtext == _lastCheckedText) {
            _blinkCount = 0;
            return;
        }

        if (doChangeNow || !Blinker.Enabled || _blinkCount >= _raiseChangeDelay) {
            _lastCheckedText = newtext;
            _blinkCount = 0;
            OnTextChanged();
        }
    }

    /// <summary>
    /// Prüft die den Selektions-Bereich auf nicht erlaubte Werte und Repariert diese.
    /// </summary>
    /// <param name="swapThem">Bei True werden MarkStart und MarkEnd richtig angeordnet. (Start kleiner/gleich End) </param>
    /// <remarks></remarks>
    private void Selection_Repair(bool swapThem) {
        if (_markStart < 0 && _markEnd < 0) { return; }

        _markStart = Math.Max(_markStart, 0);
        _markStart = Math.Min(_markStart, _eTxt.Count);

        _markEnd = Math.Min(_markEnd, _eTxt.Count);

        if (_markStart == _markEnd) { _markEnd = -1; }

        if (swapThem && _markStart > _markEnd && _markEnd > -1) {
            Generic.Swap(ref _markStart, ref _markEnd);
        }
    }

    private void Selection_WortMarkieren(int pos) {
        if (_eTxt.Count == 0) {
            _markStart = -1;
            _markEnd = -1;
            return;
        }
        _markStart = _eTxt.WordStart(pos);
        _markEnd = _eTxt.WordEnd(pos);
        Selection_Repair(true);
    }

    private void SliderY_ValueChange(object sender, System.EventArgs e) => Invalidate();

    private void SpellChecker_DoWork(object sender, DoWorkEventArgs e) {
        try {
            if (Dictionary.IsSpellChecking) { return; }
            if (DateTime.UtcNow.Subtract(_lastUserActionForSpellChecking).TotalSeconds < 2) { return; }

            Dictionary.IsSpellChecking = true;
            if (!SpellCheckingEnabled) { return; }

            if (!Dictionary.DictionaryRunning(!DesignMode)) { return; }

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
        } catch {
            //Develop.DebugPrint("Rechtschreibprüfer-Fehler", ex);
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
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
using BlueBasics.EventArgs;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueControls.ItemCollection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace BlueControls.Controls {

    [Designer(typeof(BasicDesigner))]
    [DefaultEvent("TextChanged")]
    public partial class TextBox : GenericControl, IContextMenu {

        #region Fields

        private string _AllowedChars = string.Empty;

        private int _Cursor_CharPos = -1;

        private bool _Cursor_Visible;

        private ExtText _eTxt;

        private enDataFormat _Format = enDataFormat.Text;

        private string _LastCheckedText = string.Empty;

        private DateTime _LastUserActionForSpellChecking = DateTime.Now;

        private int _MarkEnd = -1;

        private int _MarkStart = -1;

        private int _MouseValue;

        private bool _Multiline;

        private bool _MustCheck = true;

        private Slider _SliderY = null;

        private bool _SpellChecking;

        private string _Suffix = string.Empty;

        private enSteuerelementVerhalten _Verhalten = enSteuerelementVerhalten.Scrollen_ohne_Textumbruch;

        #endregion

        #region Constructors

        public TextBox() : base(true, true) {
            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();
            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
            _MouseHighlight = false;
        }

        #endregion

        #region Events

        public event EventHandler<ContextMenuInitEventArgs> ContextMenuInit;

        public event EventHandler<ContextMenuItemClickedEventArgs> ContextMenuItemClicked;

        public new event EventHandler Enter;

        public event EventHandler ESC;

        public event EventHandler<MultiUserFileGiveBackEventArgs> NeedDatabaseOfAdditinalSpecialChars;

        public event EventHandler TAB;

        public new event EventHandler TextChanged;

        #endregion

        #region Properties

        [DefaultValue("")]
        public string AllowedChars {
            get => _AllowedChars;
            set {
                if (value == _AllowedChars) { return; }
                _AllowedChars = value;
                GenerateETXT(false);
            }
        }

        [DefaultValue(enDataFormat.Text)]
        public enDataFormat Format {
            get => _Format;
            set {
                if (_Format == value) { return; }
                _Format = value;
                GenerateETXT(false);
            }
        }

        [DefaultValue(false)]
        public bool MultiLine {
            get => _Multiline;
            set {
                if (value == _Multiline) { return; }
                _Multiline = value;
                GenerateETXT(false);
                if (_SliderY != null) {
                    _SliderY.Visible = false;
                    _SliderY.Value = 0;
                }
                Invalidate();
            }
        }

        //Zum Berechnen, was die Maus gerade macht
        [DefaultValue(false)]
        public bool SpellChecking {
            get => _SpellChecking;
            set {
                if (_SpellChecking == value) { return; }
                _SpellChecking = value;
                AbortSpellChecking();
                Invalidate();
            }
        }

        [DefaultValue("")]
        public string Suffix {
            get => _Suffix;
            set {
                if (value == _Suffix) { return; }
                _Suffix = value;
                Invalidate();
            }
        }

        [DefaultValue("")]
        public new string Text {
            get => _eTxt == null ? string.Empty : _Format == enDataFormat.Text_mit_Formatierung ? _eTxt.HtmlText : _eTxt.PlainText;
            set {
                if (!string.IsNullOrEmpty(value)) {
                    value = value.Replace("\n", string.Empty);
                    value = value.Replace("\r", "\r\n");
                }
                if (_Format == enDataFormat.Text_mit_Formatierung) {
                    if (_eTxt != null && value == _eTxt.HtmlText) { return; }
                } else {
                    if (_eTxt != null && value == _eTxt.PlainText) { return; }
                }
                AbortSpellChecking();
                lock (Dictionary.Lock_SpellChecking) {
                    _eTxt = null;
                    GenerateETXT(true);
                    if (_Format == enDataFormat.Text_mit_Formatierung) {
                        _eTxt.HtmlText = value;
                    } else {
                        _eTxt.PlainText = value;
                    }
                    Invalidate();
                    CheckIfTextIsChanded(value);
                }
            }
        }

        [DefaultValue(enSteuerelementVerhalten.Scrollen_ohne_Textumbruch)]
        public enSteuerelementVerhalten Verhalten {
            get => _Verhalten;
            set {
                if (_Verhalten == value) { return; }
                _Verhalten = value;
                if (_SliderY != null) {
                    _SliderY.Visible = false;
                    _SliderY.Value = 0;
                }
                Invalidate();
            }
        }

        #endregion

        #region Methods

        public void Char_DelBereich(int Von, int Bis) {
            if (_MarkStart > -1 && _MarkEnd > -1) {
                Von = _MarkStart;
                Bis = _MarkEnd;
            }
            MarkClear();
            _Cursor_Visible = true;
            if (Von < 0 || Bis < 0) { return; }
            _Cursor_CharPos = Von;
            _eTxt.Delete(Von, Bis);
        }

        public bool ContextMenuItemClickedInternalProcessig(object sender, ContextMenuItemClickedEventArgs e) {
            Focus();
            var NewWord = "";
            _MarkStart = int.Parse(e.Tags.TagGet("MarkStart"));
            _MarkEnd = int.Parse(e.Tags.TagGet("MarkEnd"));
            _Cursor_CharPos = int.Parse(e.Tags.TagGet("Cursorpos"));
            var tmp = e.ClickedComand;
            if (e.ClickedComand.StartsWith("#ChangeTo:")) {
                NewWord = e.ClickedComand.Substring(10);
                tmp = "#ChangeTo";
            }
            switch (tmp) {
                case "#SpellAdd":
                    Dictionary.WordAdd(e.Tags.TagGet("Word"));
                    _MustCheck = true;
                    Invalidate();
                    return true;

                case "#SpellAddLower":
                    Dictionary.WordAdd(e.Tags.TagGet("Word".ToLower()));
                    _MustCheck = true;
                    Invalidate();
                    return true;

                case "#SpellChecking":
                    FloatingForm.Close(this);
                    if (_SpellChecking) {
                        _MustCheck = false;
                        Dictionary.SpellCheckingAll(_eTxt, false);
                        _MustCheck = true;
                        Invalidate();
                    }
                    return true;

                case "#SpellChecking2":
                    FloatingForm.Close(this);
                    if (_SpellChecking) {
                        _MustCheck = false;
                        Dictionary.SpellCheckingAll(_eTxt, true);
                        _MustCheck = true;
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
                    if (_MarkStart < 0 || _MarkEnd < 0) { return true; }
                    Selection_Repair(true);
                    _eTxt.StufeÄndern(_MarkStart, _MarkEnd - 1, 3);
                    CheckIfTextIsChanded(_eTxt.HtmlText);
                    return true;

                case "#NoCaption":
                    if (_MarkStart < 0 || _MarkEnd < 0) { return true; }
                    Selection_Repair(true);
                    _eTxt.StufeÄndern(_MarkStart, _MarkEnd - 1, 4);
                    CheckIfTextIsChanded(_eTxt.HtmlText);
                    return true;

                case "#Bold":
                    if (_MarkStart < 0 || _MarkEnd < 0) { return true; }
                    Selection_Repair(true);
                    _eTxt.StufeÄndern(_MarkStart, _MarkEnd - 1, 7);
                    CheckIfTextIsChanded(_eTxt.HtmlText);
                    return true;

                case "#Sonderzeichen":
                    AddSpecialChar();
                    return true;

                case "#ChangeTo":
                    var ws = _eTxt.WordStart(_Cursor_CharPos);
                    var we = _eTxt.WordEnd(_Cursor_CharPos);
                    Char_DelBereich(ws, we);
                    _Cursor_CharPos = ws;
                    InsertText(NewWord);
                    _Cursor_CharPos = _eTxt.WordEnd(_Cursor_CharPos);
                    return true;
            }
            return false;
        }

        public new void Focus() {
            if (Focused()) { return; }
            base.Focus();
        }

        public new bool Focused() => base.Focused || (_SliderY != null && _SliderY.Focused());

        public void GetContextMenuItems(System.Windows.Forms.MouseEventArgs e, ItemCollectionList items, out object hotItem, List<string> tags, ref bool cancel, ref bool translate) {
            AbortSpellChecking();
            hotItem = null;
            tags.TagSet("CursorPosBeforeClick", _Cursor_CharPos.ToString());
            var tmp = Cursor_PosAt(e.X, e.Y);
            tags.TagSet("Char", tmp.ToString());
            tags.TagSet("MarkStart", _MarkStart.ToString());
            tags.TagSet("MarkEnd", _MarkEnd.ToString());
            tags.TagSet("Cursorpos", _Cursor_CharPos.ToString());
            var tmpWord = _eTxt.Word(tmp);
            tags.TagSet("Word", tmpWord);
            if (_SpellChecking && !Dictionary.IsWordOk(tmpWord)) {
                items.Add("Rechtschreibprüfung", true);
                if (Dictionary.IsSpellChecking) {
                    items.Add("Gerade ausgelastet...", "Gerade ausgelastet...", false);
                    items.AddSeparator();
                } else {
                    var sim = Dictionary.SimilarTo(tmpWord);
                    if (sim != null) {
                        foreach (var ThisS in sim) {
                            items.Add(" - " + ThisS, "#ChangeTo:" + ThisS);
                        }
                        items.AddSeparator();
                    }
                    items.Add("'" + tmpWord + "' ins Wörterbuch aufnehmen", "#SpellAdd", Dictionary.IsWriteable());
                    if (tmpWord.ToLower() != tmpWord) {
                        items.Add("'" + tmpWord.ToLower() + "' ins Wörterbuch aufnehmen", "#SpellAddLower", Dictionary.IsWriteable());
                    }
                    items.Add("Schnelle Rechtschreibprüfung", "#SpellChecking", Dictionary.IsWriteable());
                    items.Add("Alle Wörter sind ok", "#SpellChecking2", Dictionary.IsWriteable());
                    items.AddSeparator();
                }
            }
            if (this is not ComboBox cbx || cbx.DropDownStyle == System.Windows.Forms.ComboBoxStyle.DropDown) {
                items.Add(enContextMenuComands.Ausschneiden, Convert.ToBoolean(_MarkStart >= 0) && Enabled);
                items.Add(enContextMenuComands.Kopieren, Convert.ToBoolean(_MarkStart >= 0));
                items.Add(enContextMenuComands.Einfügen, System.Windows.Forms.Clipboard.ContainsText() && Enabled);
                if (_Format == enDataFormat.Text_mit_Formatierung) {
                    items.AddSeparator();
                    items.Add("Sonderzeichen einfügen", "#Sonderzeichen", QuickImage.Get(enImageCode.Sonne, 16), _Cursor_CharPos > -1);
                    if (Convert.ToBoolean(_MarkEnd > -1)) {
                        items.AddSeparator();
                        items.Add("Als Überschrift markieren", "#Caption", Skin.GetBlueFont(enDesign.TextBox_Stufe3, enStates.Standard).SymbolForReadableText(), _MarkEnd > -1);
                        items.Add("Fettschrift", "#Bold", Skin.GetBlueFont(enDesign.TextBox_Bold, enStates.Standard).SymbolForReadableText(), Convert.ToBoolean(_MarkEnd > -1));
                        items.Add("Als normalen Text markieren", "#NoCaption", Skin.GetBlueFont(enDesign.TextBox, enStates.Standard).SymbolForReadableText(), Convert.ToBoolean(_MarkEnd > -1));
                    }
                }
            }
        }

        public void InsertText(string nt) {
            if (nt == null) { nt = string.Empty; }
            nt = nt.Replace(Constants.beChrW1.ToString(), "\r");
            nt = nt.RemoveChars(Constants.Char_NotFromClip);
            if (!MultiLine) { nt = nt.RemoveChars("\r"); }
            foreach (var t in nt) {
                var a = (enASCIIKey)t;
                if (_eTxt.InsertChar(a, _Cursor_CharPos)) {
                    _Cursor_CharPos++;
                }
            }
            CheckIfTextIsChanded(_eTxt.HtmlText);
        }

        public void Mark(enMarkState markstate, int first, int last) => _eTxt?.Mark(markstate, first, last);

        public void OnContextMenuInit(ContextMenuInitEventArgs e) => ContextMenuInit?.Invoke(this, e);

        public void OnContextMenuItemClicked(ContextMenuItemClickedEventArgs e) => ContextMenuItemClicked?.Invoke(this, e);

        /// <summary>
        /// Prüft - bei Multiline Zeile für Zeile - ob der Text in der Textbox zulässig ist.
        /// </summary>
        /// <param name="mitMeldung"></param>
        /// <returns>Ergibt Wahr, wenn der komplette Text dem Format entspricht. Andernfalls Falsch.</returns>
        /// <remarks></remarks>
        public bool Text_IsOkay(bool mitMeldung) {
            if (!_eTxt.PlainText.IsFormat(_Format, _Multiline, null)) {
                if (mitMeldung) { MessageBox.Show("Ihre Eingabe entspricht nicht<br>dem erwarteten Format.", enImageCode.Warnung, "OK"); }
                return false;
            }
            return true;
        }

        public void Unmark(enMarkState markstate) => _eTxt?.Unmark(markstate);

        // Tastatur
        internal new void KeyPress(enASCIIKey KeyAscii) {
            // http://www.manderby.com/informatik/allgemeines/ascii.php
            if (_MouseValue != 0) { return; }

            switch (KeyAscii) {
                case enASCIIKey.DEL:
                    // Eigentlich auch noch Ascii Code - steht bei ISO als Del
                    Char_DelBereich(_Cursor_CharPos, _Cursor_CharPos + 1);
                    break;

                case enASCIIKey.ENTER: {
                        if (MultiLine) {
                            Char_DelBereich(-1, -1);
                            if (_eTxt.InsertChar(KeyAscii, _Cursor_CharPos)) {
                                _Cursor_CharPos++;
                            }
                        }
                        break;
                    }
                default: {
                        if (KeyAscii >= enASCIIKey.Space) //Ascii-Codes (Außer 127 = DEL)
                        {
                            Char_DelBereich(-1, -1);
                            if (_eTxt.InsertChar(KeyAscii, _Cursor_CharPos)) {
                                _Cursor_CharPos++;
                            }
                        } else {
                            switch (KeyAscii) {
                                case enASCIIKey.BackSpace:
                                    Char_DelBereich(_Cursor_CharPos - 1, _Cursor_CharPos);
                                    break;

                                case enASCIIKey.StrgC:
                                    Clipboard_Copy();
                                    return;

                                case enASCIIKey.StrgV:
                                    Clipboard_Paste();
                                    break;

                                case enASCIIKey.StrgX:
                                    Clipboard_Copy();
                                    Char_DelBereich(-1, -1);
                                    break;

                                case enASCIIKey.StrgF:
                                case enASCIIKey.LineFeed:   //Zeilenumbruch, Kommt vor, wen man was aus einem anderen Programm kopiert,
                                case enASCIIKey.TAB:
                                    return;

                                case enASCIIKey.StrgA:
                                    MarkAll();
                                    break;
                            }
                        }
                        break;
                    }
            }
            CheckIfTextIsChanded(_eTxt.HtmlText);
        }

        internal bool WordStarts(string word, int position) {
            if (InvokeRequired) {
                return (bool)Invoke(new Func<bool>(() => WordStarts(word, position)));
            }
            try {
                if (_eTxt == null) { GenerateETXT(true); }
                if (_eTxt == null) { return false; }
                if (position + word.Length > _eTxt.Chars.Count + 1) { return false; }
                if (position > 0 && !_eTxt.Chars[position - 1].isWordSeperator()) { return false; }
                if (position + word.Length < _eTxt.Chars.Count && !_eTxt.Chars[position + word.Length].isWordSeperator()) { return false; }
                var tt = _eTxt.ConvertCharToPlainText(position, position + word.Length - 1);
                return word.ToUpper() == tt.ToUpper();
            } catch (Exception) {
                return WordStarts(word, position);
            }
        }

        protected override void DrawControl(Graphics gr, enStates state) {
            if (_eTxt == null) { GenerateETXT(true); }
            if (state == enStates.Checked_Disabled) {
                Develop.DebugPrint("Checked Disabled");
                state = enStates.Checked_Disabled;
            }
            if (state == enStates.Standard_Disabled) {
                if (_MarkStart > -1) { Develop.DebugPrint("Disabled & Markstart:" + _MarkStart); }
                if (_MarkEnd > -1) { Develop.DebugPrint("Disabled & MarkEnd:" + _MarkStart); }
                MarkClear();
            }
            // Erst den Typ richtig stellen, dann den State ändern!
            _eTxt.Design = GetDesign();
            _eTxt.State = state;
            bool sliderVisible;
            var effectWidth = Width;
            sliderVisible = _Multiline ? _eTxt.Height() > (Height - 16) : _eTxt.Height() > Height;
            if (sliderVisible) { effectWidth = Width - 18; }
            switch (_Verhalten) {
                case enSteuerelementVerhalten.Scrollen_mit_Textumbruch:
                    _eTxt.TextDimensions = new Size(effectWidth - (Skin.PaddingSmal * 2), -1);
                    _eTxt.DrawingArea = new Rectangle(0, 0, effectWidth, Height);
                    break;

                case enSteuerelementVerhalten.Scrollen_ohne_Textumbruch:
                    var hp = HotPosition();
                    _eTxt.TextDimensions = Size.Empty;
                    _eTxt.DrawingArea = new Rectangle(0, 0, effectWidth, Height);
                    if (hp < 0) {
                        // Mach nix
                    } else if (hp == 0) {
                        _eTxt.DrawingPos.X = Skin.PaddingSmal;
                    } else if (hp > _eTxt.Chars.Count - 1) {
                        _eTxt.DrawingPos.X = _eTxt.Width() > Width - (Skin.PaddingSmal * 2) ? Width - _eTxt.Width() - (Skin.PaddingSmal * 2) : Skin.PaddingSmal;
                    } else {
                        var r = _eTxt.CursorPixelPosX(hp);
                        if (r.X > Width - (Skin.PaddingSmal * 4) - _eTxt.DrawingPos.X) {
                            _eTxt.DrawingPos.X = Width - (Skin.PaddingSmal * 4) - r.X + 1;
                        } else if (r.X + _eTxt.DrawingPos.X < Skin.PaddingSmal * 2) {
                            _eTxt.DrawingPos.X = (Skin.PaddingSmal * 2) - r.X + 1;
                        }
                    }
                    if (_eTxt.DrawingPos.X > Skin.PaddingSmal) { _eTxt.DrawingPos.X = Skin.PaddingSmal; }
                    break;

                case enSteuerelementVerhalten.Steuerelement_Anpassen:
                    sliderVisible = false;
                    _eTxt.TextDimensions = Size.Empty;
                    Width = this is ComboBox
                        ? Math.Max(_eTxt.Width() + (Skin.PaddingSmal * 3) + 20, Width)
                        : Math.Max(_eTxt.Width() + (Skin.PaddingSmal * 3), Width);
                    Height = Math.Max(_eTxt.Height() + (Skin.PaddingSmal * 2), Height);
                    _eTxt.DrawingArea = new Rectangle(0, 0, Width, Height);
                    break;

                case enSteuerelementVerhalten.Text_Abschneiden:
                    sliderVisible = false;
                    _eTxt.TextDimensions = Size.Empty;
                    _eTxt.DrawingArea = new Rectangle(0, 0, Width, Height);
                    break;

                default:
                    Develop.DebugPrint(_Verhalten);
                    break;
            }
            if (sliderVisible) {
                GetSlider();
                _SliderY.Visible = true;
                _SliderY.Width = 18;
                _SliderY.Height = Height;
                _SliderY.Left = Width - _SliderY.Width;
                _SliderY.Top = 0;
                _eTxt.DrawingPos.Y = (int)-_SliderY.Value;
                _SliderY.Maximum = _eTxt.Height() + 16 - DisplayRectangle.Height;
            } else {
                if (_SliderY != null) {
                    _SliderY.Visible = false;
                    _SliderY.Value = 0;
                }
                _eTxt.DrawingPos.Y = Skin.PaddingSmal;
            }
            Skin.Draw_Back(gr, _eTxt.Design, state, DisplayRectangle, this, true);
            Cursor_Show(gr);
            MarkAndGenerateZone(gr);
            _eTxt.Draw(gr, 1);
            if (!string.IsNullOrEmpty(_Suffix)) {
                Rectangle r = new(_eTxt.Width() + _eTxt.DrawingPos.X, _eTxt.DrawingPos.Y, 1000, 1000);
                if (_eTxt.Chars.Count > 0) {
                    r.X += 2;
                    Skin.Draw_FormatedText(gr, _Suffix, _eTxt.Design, enStates.Standard_Disabled, null, enAlignment.Top_Left, r, this, false, false);
                } else {
                    Skin.Draw_FormatedText(gr, "[in " + _Suffix + "]", _eTxt.Design, enStates.Standard_Disabled, null, enAlignment.Top_Left, r, this, false, Translate);
                }
            }
            Skin.Draw_Border(gr, _eTxt.Design, state, DisplayRectangle);
            if (_MustCheck && !Dictionary.IsSpellChecking && Dictionary.DictionaryRunning(true) && !SpellChecker.CancellationPending && !SpellChecker.IsBusy) { SpellChecker.RunWorkerAsync(); }
        }

        protected virtual enDesign GetDesign() => enDesign.TextBox;

        protected override bool IsInputKey(System.Windows.Forms.Keys keyData) =>
            // Ganz wichtig diese Routine!
            // Wenn diese NICHT ist, geht der Fokus weg, sobald der cursor gedrückt wird.
            // http://technet.microsoft.com/de-de/subscriptions/control.isinputkey%28v=vs.100%29
            keyData switch {
                System.Windows.Forms.Keys.Up or System.Windows.Forms.Keys.Down or System.Windows.Forms.Keys.Left or System.Windows.Forms.Keys.Right => true,
                _ => false,
            };

        protected override void OnDoubleClick(System.EventArgs e) {
            base.OnDoubleClick(e);
            Selection_WortMarkieren(_MarkStart);
            _LastUserActionForSpellChecking = DateTime.Now;
            _MouseValue = 9999;
            Invalidate();
        }

        protected override void OnEnabledChanged(System.EventArgs e) {
            MarkClear();
            base.OnEnabledChanged(e);
        }

        protected override void OnGotFocus(System.EventArgs e) {
            base.OnGotFocus(e);
            if (!Enabled) { return; }
            if (_eTxt == null) { GenerateETXT(true); }
            if (!FloatingForm.IsShowing(this)) {
                SetCursorToEnd();
                if (!_eTxt.Multiline) { if (!ContainsMouse() || !MousePressing()) { MarkAll(); } }
                _LastUserActionForSpellChecking = DateTime.Now.AddSeconds(-30);
            }
            Blinker.Enabled = true;
        }

        // Tastatur
        protected override void OnKeyDown(System.Windows.Forms.KeyEventArgs e) {
            base.OnKeyDown(e);

            if (!Enabled) { return; }
            _LastUserActionForSpellChecking = DateTime.Now;
            if (_MouseValue != 0 || e == null) { return; }

            switch (e.KeyCode) {
                case System.Windows.Forms.Keys.Left:
                    Cursor_Richtung(-1, 0);
                    break;

                case System.Windows.Forms.Keys.Right:
                    Cursor_Richtung(1, 0);
                    break;

                case System.Windows.Forms.Keys.Down:
                    Cursor_Richtung(0, 1);
                    break;

                case System.Windows.Forms.Keys.Up:
                    Cursor_Richtung(0, -1);
                    break;

                case System.Windows.Forms.Keys.Delete:
                    KeyPress(enASCIIKey.DEL);
                    CheckIfTextIsChanded(_eTxt.HtmlText);
                    break;
            }

            _Cursor_Visible = true;
            Invalidate();
        }

        protected override void OnKeyPress(System.Windows.Forms.KeyPressEventArgs e) {
            base.OnKeyPress(e);
            _LastUserActionForSpellChecking = DateTime.Now;
            if (!Enabled) {
                if (e.KeyChar != (int)enASCIIKey.StrgX) { e.KeyChar = (char)enASCIIKey.StrgC; }
                if (e.KeyChar != (int)enASCIIKey.StrgC) { return; }
            }
            switch ((enASCIIKey)e.KeyChar) {
                case enASCIIKey.ENTER:
                    KeyPress((enASCIIKey)e.KeyChar);
                    OnEnter();
                    return;

                case enASCIIKey.ESC:
                    OnESC();
                    return;

                case enASCIIKey.TAB:
                    OnTAB();
                    return;

                default:
                    KeyPress((enASCIIKey)e.KeyChar);
                    break;
            }
            Invalidate();
            if (IsDisposed) { return; }
            CheckIfTextIsChanded(_eTxt.HtmlText);
        }

        // Fokus
        protected override void OnLostFocus(System.EventArgs e) {
            base.OnLostFocus(e);
            _LastUserActionForSpellChecking = DateTime.Now.AddSeconds(-30);
            if (!FloatingForm.IsShowing(this)) { MarkClear(); }
            Blinker.Enabled = false;
            CursorClear();
            Invalidate(); // Muss sein, weil evtl. der Cursor stehen bleibt
        }

        // Mouse
        protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e) {
            base.OnMouseDown(e);
            _LastUserActionForSpellChecking = DateTime.Now;
            if (!Enabled) { return; }
            if (e.Button == System.Windows.Forms.MouseButtons.Right) { return; }
            _MouseValue = 1;
            _MarkStart = Cursor_PosAt(e.X, e.Y);
            _MarkEnd = _MarkStart;
            CursorClear();
            Selection_Repair(false);
            Invalidate();
        }

        protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e) {
            base.OnMouseMove(e);
            if (_eTxt == null) { return; }
            if (e.Button != System.Windows.Forms.MouseButtons.Left) { return; }
            if (!Enabled) { return; }
            _LastUserActionForSpellChecking = DateTime.Now;
            CursorClear();
            _MarkEnd = Cursor_PosAt(e.X, e.Y);
            Selection_Repair(false);
            Invalidate();
        }

        protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e) {
            base.OnMouseUp(e);
            _LastUserActionForSpellChecking = DateTime.Now;
            if (Enabled) {
                if (_MouseValue == 9999) {
                    //es Wurde Doppelgeklickt
                } else {
                    if (_MarkStart == _MarkEnd || _MarkEnd < 0) {
                        _Cursor_CharPos = Cursor_PosAt(e.X, e.Y);
                        MarkClear();
                        if (e.Button == System.Windows.Forms.MouseButtons.Right) {
                            FloatingInputBoxListBoxStyle.ContextMenuShow(this, e);
                        }
                    } else {
                        CursorClear();
                        Selection_Repair(true);
                        if (e.Button == System.Windows.Forms.MouseButtons.Right) {
                            FloatingInputBoxListBoxStyle.ContextMenuShow(this, e);
                        }
                    }
                }
                _MouseValue = 0;
            } else {
                CursorClear();
            }
            Invalidate();
        }

        protected override void OnMouseWheel(System.Windows.Forms.MouseEventArgs e) {
            base.OnMouseWheel(e);
            if (_SliderY == null || !_SliderY.Visible) { return; }
            _LastUserActionForSpellChecking = DateTime.Now;
            _SliderY.DoMouseWheel(e);
        }

        /// <summary>
        /// Löst das Ereignis aus und setzt _LastUserChangingTime auf NULL.
        /// </summary>
        protected virtual void OnTextChanged() => TextChanged?.Invoke(this, System.EventArgs.Empty);

        private void AbortSpellChecking() {
            if (SpellChecker.IsBusy) { SpellChecker.CancelAsync(); }
        }

        private void AddSpecialChar() {
            var x = _Cursor_CharPos;
            MultiUserFileGiveBackEventArgs e = new();
            OnNeedDatabaseOfAdditinalSpecialChars(e);
            ItemCollectionList i = new(enBlueListBoxAppearance.Listbox)
            {
                //if (e.File is Database DB && DB.Bins.Count > 0) {
                //    foreach (var bmp in DB.Bins) {
                //        if (bmp.Picture != null) {
                //            if (!string.IsNullOrEmpty(bmp.Name)) {
                //                var crc = "DB_" + bmp.Name;
                //                i.Add(bmp.Name, crc, QuickImage.Get(crc, 20));
                //            }
                //        }
                //    }
                //    i.AddSeparator();
                //}
                { "Kugel", "sphere", QuickImage.Get(enImageCode.Kugel, 20) },
                { "Warnung", "Warnung", QuickImage.Get(enImageCode.Warnung, 20) },
                { "Information", "Information", QuickImage.Get(enImageCode.Information, 20) },
                { "Kritisch", "Kritisch", QuickImage.Get(enImageCode.Kritisch, 20) },
                { "Frage", "Frage", QuickImage.Get(enImageCode.Frage, 20) }
            };
            var r = InputBoxListBoxStyle.Show("Wählen sie:", i, enAddType.None, true);
            _Cursor_CharPos = x;
            if (r == null || r.Count != 1) { return; }
            Char_DelBereich(-1, -1);
            if (_eTxt.InsertImage(r[0], _Cursor_CharPos)) { _Cursor_CharPos++; }
            CheckIfTextIsChanded(_eTxt.HtmlText);
        }

        private void Blinker_Tick(object sender, System.EventArgs e) {
            if (!Focused()) { return; }
            if (!Enabled) { return; }
            if (_MarkStart > -1 && _MarkEnd > -1) { _Cursor_CharPos = -1; }
            if (_Cursor_CharPos > -1) {
                _Cursor_Visible = !_Cursor_Visible;
                Invalidate();
            } else {
                if (_Cursor_Visible) {
                    _Cursor_Visible = false;
                    Invalidate();
                }
            }
        }

        private void CheckIfTextIsChanded(string NewPlainText) {
            if (NewPlainText == _LastCheckedText) { return; }
            _LastCheckedText = NewPlainText;
            if (Dictionary.DictionaryRunning(true)) { _MustCheck = true; }
            OnTextChanged();
            return;
        }

        private void Clipboard_Copy() {
            if (_MarkStart < 0 || _MarkEnd < 0) { return; }
            Selection_Repair(true);
            var tt = _eTxt.ConvertCharToPlainText(_MarkStart, _MarkEnd - 1);
            if (string.IsNullOrEmpty(tt)) { return; }
            tt = tt.Replace("\n", string.Empty);
            tt = tt.Replace("\r", "\r\n");
            Generic.CopytoClipboard(tt);
        }

        private void Clipboard_Paste() {
            // VORSICHT!
            // Seltsames Verhalten von VB.NET
            // Anscheinend wird bei den Clipboard operationen ein DoEventXsx ausgelöst.
            // Dadurch kommt es zum Refresh des übergeordneten Steuerelementes, warscheinlich der Textbox.
            // Deshalb  muss 'Char_DelBereich' NACH den Clipboard-Operationen stattfinden.
            if (!System.Windows.Forms.Clipboard.ContainsText()) { return; }
            Char_DelBereich(-1, -1);
            InsertText(System.Windows.Forms.Clipboard.GetText());
        }

        /// <summary>
        /// Sucht den aktuellen Buchstaben, der unter den angegeben Koordinaten liegt.
        /// Wird auf die hintere Hälfte eines Zeichens gewählt, wird der nächste Buchstabe angegeben.
        /// </summary>
        /// <remarks></remarks>
        private int Cursor_PosAt(double PixX, double PixY) {
            if (_eTxt == null) { return -1; }
            // Das geht am Einfachsten....
            if (PixX < _eTxt.DrawingPos.X && PixY < _eTxt.DrawingPos.Y) { return 0; }
            PixX = Math.Max(PixX, _eTxt.DrawingPos.X);
            PixY = Math.Max(PixY, _eTxt.DrawingPos.Y);
            PixX = Math.Min(PixX, _eTxt.DrawingPos.X + _eTxt.Width());
            PixY = Math.Min(PixY, _eTxt.DrawingPos.Y + _eTxt.Height());
            var c = _eTxt.Char_Search(PixX, PixY);
            if (c < 0) { c = 0; }
            return c < _eTxt.Chars.Count && PixX > _eTxt.DrawingPos.X + _eTxt.Chars[c].Pos.X + (_eTxt.Chars[c].Size.Width / 2.0) ? c + 1 : c;
        }

        private void Cursor_Richtung(short X, short Y) {
            if (X != 0) {
                if (_Cursor_CharPos > -1) {
                    if (X == -1 && _Cursor_CharPos > 0) { _Cursor_CharPos--; }
                    if (X == 1 && _Cursor_CharPos < _eTxt.Chars.Count) { _Cursor_CharPos++; }
                } else if (_MarkStart > -1 && _MarkEnd > -1) {
                    if (X == -1) { _Cursor_CharPos = Math.Min(_MarkEnd, _MarkStart); }
                    if (X == 1) { _Cursor_CharPos = Math.Max(_MarkEnd, _MarkStart) + 1; }
                } else {
                    _Cursor_CharPos = 0;
                }
            }
            MarkClear();
            var ri = _eTxt.CursorPixelPosX(_Cursor_CharPos);
            if (_Cursor_CharPos < 0) { _Cursor_CharPos = 0; }
            if (Y > 0) {
                _Cursor_CharPos = _Cursor_CharPos >= _eTxt.Chars.Count
                    ? _eTxt.Chars.Count
                    : Cursor_PosAt(ri.Left + _eTxt.DrawingPos.X, ri.Top + (ri.Height / 2.0) + _eTxt.Chars[_Cursor_CharPos].Size.Height + _eTxt.DrawingPos.Y);
            } else if (Y < 0) {
                _Cursor_CharPos = _Cursor_CharPos >= _eTxt.Chars.Count
                    ? _eTxt.Chars.Count > 0
                        ? Cursor_PosAt(ri.Left + _eTxt.DrawingPos.X, ri.Top + (ri.Height / 2.0) - _eTxt.Chars[_Cursor_CharPos - 1].Size.Height + _eTxt.DrawingPos.Y)
                        : 0
                    : Cursor_PosAt(ri.Left + _eTxt.DrawingPos.X, ri.Top + (ri.Height / 2.0) - _eTxt.Chars[_Cursor_CharPos].Size.Height + _eTxt.DrawingPos.Y);
            }
            if (_Cursor_CharPos < 0) { _Cursor_CharPos = 0; }
            _Cursor_Visible = true;
        }

        private void Cursor_Show(Graphics GR) {
            if (!_Cursor_Visible) { return; }
            if (_Cursor_CharPos < 0) { return; }
            var r = _eTxt.CursorPixelPosX(_Cursor_CharPos);
            GR.DrawLine(new Pen(Color.Black), r.Left + _eTxt.DrawingPos.X, r.Top + _eTxt.DrawingPos.Y, r.Left + _eTxt.DrawingPos.X, r.Bottom + _eTxt.DrawingPos.Y);
        }

        private void CursorClear() {
            _Cursor_CharPos = -1;
            _Cursor_Visible = false;
        }

        /// <summary>
        /// Wenn das Format, die Maxlänge oder Multiline sich geändert haben,
        /// wird für das dementsprechende Format die Verbote/Erlaubnisse gesetzt.
        /// z.B. wird beim Format Datum die Maxlänge auf 10 gesetzt und nur noch Ziffern und Punkt erlaubt.
        /// </summary>
        /// <remarks></remarks>
        private void GenerateETXT(bool ResetCoords) {
            if (InvokeRequired) {
                Invoke(new Action(() => GenerateETXT(ResetCoords)));
                return;
            }
            if (_eTxt == null) {
                var state = enStates.Standard;
                if (!Enabled) { state = enStates.Standard_Disabled; }
                _eTxt = new ExtText(GetDesign(), state);
                ResetCoords = true;
            }
            _eTxt.Multiline = _Multiline;
            _eTxt.AllowedChars = !string.IsNullOrEmpty(_AllowedChars) ? _AllowedChars : _Format.AllowedChars();
            if (ResetCoords) {
                // Hier Standard-Werte Setzen, die Draw-Routine setzt bei Bedarf um
                if (_SliderY != null) {
                    _SliderY.Visible = false;
                    _SliderY.Value = 0;
                }
                _eTxt.DrawingPos.X = Skin.PaddingSmal;
                _eTxt.DrawingPos.Y = Skin.PaddingSmal;
                _eTxt.DrawingArea = new Rectangle(0, 0, -1, -1);
                _eTxt.TextDimensions = Size.Empty;
            }
        }

        private Slider GetSlider() {
            if (_SliderY != null) { return _SliderY; }
            _SliderY = new Slider {
                Dock = System.Windows.Forms.DockStyle.Right,
                LargeChange = 10.0D,
                Location = new Point(Width - 18, 0),
                Maximum = 100.0D,
                Minimum = 0.0D,
                MouseChange = 1.0D,
                Name = "SliderY",
                Orientation = enOrientation.Senkrecht,
                Size = new Size(18, Height),
                SmallChange = 48.0D,
                TabIndex = 0,
                TabStop = false,
                Value = 0.0D,
                Visible = true
            };
            _SliderY.ValueChanged += new EventHandler(SliderY_ValueChange);
            Controls.Add(_SliderY);
            return _SliderY;
        }

        private int HotPosition() => _Cursor_CharPos > -1 ? _Cursor_CharPos
                                                          : _MarkStart > -1 && _MarkEnd < 0 ? _MarkEnd : _MarkStart > -1 && _MarkEnd > -1 ? _MarkEnd : -1;

        private void MarkAll() {
            if (_eTxt != null && _eTxt.Chars.Count > 0) {
                _MarkStart = 0;
                _MarkEnd = _eTxt.Chars.Count;
                _Cursor_Visible = false;
            } else {
                MarkClear();
            }
        }

        private void MarkAndGenerateZone(Graphics GR) {
            _eTxt.Check(0, _eTxt.Chars.Count - 1, false);
            if (_MarkStart < 0 || _MarkEnd < 0) { return; }
            Selection_Repair(false);
            var MaS = Math.Min(_MarkStart, _MarkEnd);
            var MaE = Math.Max(_MarkStart, _MarkEnd);
            if (MaS == MaE) { return; }
            _eTxt.Check(MaS, MaE - 1, true);
            var TmpcharS = MaS;
            for (var cc = MaS; cc <= MaE; cc++) {
                if (cc == MaE || _eTxt.Chars[cc].Pos.X < _eTxt.Chars[TmpcharS].Pos.X || Math.Abs(_eTxt.Chars[cc].Pos.Y - _eTxt.Chars[TmpcharS].Pos.Y) > 0.001) //Jetzt ist der Zeitpunkt zum Zeichen/start setzen
                {
                    Rectangle r = new((int)(_eTxt.Chars[TmpcharS].Pos.X + _eTxt.DrawingPos.X), (int)(_eTxt.Chars[TmpcharS].Pos.Y + 2 + _eTxt.DrawingPos.Y), (int)(_eTxt.Chars[cc - 1].Pos.X + _eTxt.Chars[cc - 1].Size.Width - _eTxt.Chars[TmpcharS].Pos.X), (int)(_eTxt.Chars[cc - 1].Pos.Y + _eTxt.Chars[cc - 1].Size.Height - _eTxt.Chars[TmpcharS].Pos.Y));
                    if (r.Width < 2) { r = new Rectangle(r.Left, r.Top, 2, r.Height); }
                    if (_eTxt.Chars[TmpcharS].State != enStates.Undefiniert) {
                        Skin.Draw_Back(GR, _eTxt.Design, _eTxt.Chars[TmpcharS].State, r, null, false);
                        Skin.Draw_Border(GR, _eTxt.Design, _eTxt.Chars[TmpcharS].State, r);
                    }
                    TmpcharS = cc;
                }
            }
        }

        private void MarkClear() {
            _MarkStart = -1;
            _MarkEnd = -1;
        }

        private void OnEnter() => Enter?.Invoke(this, System.EventArgs.Empty);

        private void OnESC() => ESC?.Invoke(this, System.EventArgs.Empty);

        private void OnNeedDatabaseOfAdditinalSpecialChars(MultiUserFileGiveBackEventArgs e) => NeedDatabaseOfAdditinalSpecialChars?.Invoke(this, e);

        private void OnTAB() => TAB?.Invoke(this, System.EventArgs.Empty);

        /// <summary>
        /// Prüft die den Selektions-Bereich auf nicht erlaubte Werte und Repariert diese.
        /// </summary>
        /// <param name="SwapThem">Bei True werden MarkStart und MarkEnd richtig angeordnet. (Start kleiner/gleich End) </param>
        /// <remarks></remarks>
        private void Selection_Repair(bool SwapThem) {
            if (_eTxt == null || _eTxt.Chars.Count == 0) { MarkClear(); }
            if (_MarkStart < 0 && _MarkEnd < 0) { return; }

            _MarkStart = Math.Max(_MarkStart, 0);
            _MarkStart = Math.Min(_MarkStart, _eTxt.Chars.Count);

            _MarkEnd = Math.Max(_MarkEnd, 0);
            _MarkEnd = Math.Min(_MarkEnd, _eTxt.Chars.Count);

            if (SwapThem && _MarkStart > _MarkEnd) {
                Generic.Swap(ref _MarkStart, ref _MarkEnd);
            }
        }

        private void Selection_WortMarkieren(int Pos) {
            if (_eTxt == null || _eTxt.Chars.Count == 0) {
                MarkClear();
                return;
            }
            _MarkStart = _eTxt.WordStart(Pos);
            _MarkEnd = _eTxt.WordEnd(Pos);
            CursorClear();
            Selection_Repair(true);
        }

        private void SetCursorToEnd() {
            if (_eTxt == null) {
                // Beim Durchtabben...
                _Cursor_CharPos = -1;
            } else {
                _Cursor_CharPos = _eTxt.Chars.Count;
            }
            _Cursor_Visible = true;
        }

        private void SliderY_ValueChange(object sender, System.EventArgs e) => Invalidate();

        private void SpellChecker_DoWork(object sender, DoWorkEventArgs e) {
            try {
                if (Dictionary.IsSpellChecking) { return; }
                if (DateTime.Now.Subtract(_LastUserActionForSpellChecking).TotalSeconds < 2) { return; }

                Dictionary.IsSpellChecking = true;
                if (!_SpellChecking) { return; }

                if (!Dictionary.DictionaryRunning(true)) { return; }
                if (_eTxt == null) { return; }

                var Pos = 0;
                var woEnd = -1;
                bool Ok;
                do {
                    Ok = true;
                    SpellChecker.ReportProgress(0, "Unmark");
                    try {
                        do {
                            if (SpellChecker.CancellationPending) { return; }

                            Pos = Math.Max(woEnd + 1, Pos + 1);
                            if (_eTxt == null) { return; }// Das Textfeld ist spontan geschlossen worden.

                            if (Pos >= _eTxt.Chars.Count) { break; }

                            var woStart = _eTxt.WordStart(Pos);
                            if (woStart > -1) {
                                woEnd = _eTxt.WordEnd(Pos);
                                var wort = _eTxt.Word(Pos);
                                if (!Dictionary.IsWordOk(wort)) {
                                    if (SpellChecker.CancellationPending) { return; }
                                    SpellChecker.ReportProgress((int)(woEnd / (double)_eTxt.Chars.Count * 100), "Mark;" + woStart + ";" + (woEnd - 1));
                                }
                            }
                        } while (true);
                    } catch {
                        Ok = false;
                    }
                } while (!Ok);
            } catch (Exception ex) {
                Develop.DebugPrint(ex);
            }
            SpellChecker.ReportProgress(100, "Done");
        }

        private void SpellChecker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            if (SpellChecker.CancellationPending) { return; }//Ja, Multithreading ist kompliziert...

            var x = ((string)e.UserState).SplitAndCutBy(";");

            switch (x[0]) {
                case "Unmark":
                    Unmark(enMarkState.Ringelchen);
                    Invalidate();
                    break;

                case "Mark":
                    Mark(enMarkState.Ringelchen, int.Parse(x[1]), int.Parse(x[2]));
                    Invalidate();
                    break;

                case "Done":
                    _MustCheck = false;
                    break;

                default:
                    Develop.DebugPrint(Convert.ToString(e.UserState));
                    break;
            }
        }

        private void SpellChecker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) => Dictionary.IsSpellChecking = false;

        #endregion
    }
}
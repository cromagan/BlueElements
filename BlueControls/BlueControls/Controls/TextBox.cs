#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2020 Christian Peter
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
using BlueBasics.Enums;
using BlueBasics.EventArgs;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueControls.ItemCollection;
using BlueDatabase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace BlueControls.Controls
{

    [Designer(typeof(BasicDesigner))]
    [DefaultEvent("TextChanged")]
    public partial class TextBox : GenericControl, IContextMenu
    {

        #region Constructor


        public TextBox() : base(true, true)
        {

            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();


            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
            _MouseHighlight = false;
        }
        #endregion


        #region  Variablen 
        private enDataFormat _Format = enDataFormat.Text;
        private enSteuerelementVerhalten _Verhalten = enSteuerelementVerhalten.Scrollen_ohne_Textumbruch;
        private ExtText _eTxt;
        private string _AllowedChars = string.Empty;
        private bool _SpellChecking;
        private bool _Multiline;
        private bool _MustCheck = true;
        private string _Suffix = string.Empty;
        private Slider _SliderY = null;

        private DateTime _LastUserActionForSpellChecking = DateTime.Now;

        //private bool _InstantChangedEvent = true;
        //private System.Windows.Forms.Timer _InstantChangeTimer;


        ///// <summary>
        ///// Speichert, wann die letzte Text-Änderung vorgenommen wurden.
        ///// Wenn NULL, dann wurde bereits ein Event ausgelöst.
        ///// </summary>
        //private DateTime? _LastTextChange;

        private string _LastCheckedText = string.Empty;

        private int _MarkStart = -1;
        private int _MarkEnd = -1;
        private int _Cursor_CharPos = -1;
        private int _MouseValue; //Zum Berechnen, was die Maus gerade macht
        private bool _Cursor_Visible;





        #endregion







        #region  Properties 


        //#region  QuickInfo 
        //// Dieser Codeblock ist im Interface IQuickInfo herauskopiert und muss überall Identisch sein.
        //private string _QuickInfo = "";
        //[Category("Darstellung")]
        //[DefaultValue("")]
        //[Description("QuickInfo des Steuerelementes - im extTXT-Format")]
        //public string QuickInfo
        //{
        //    get
        //    {
        //        return _QuickInfo;
        //    }
        //    set
        //    {
        //        if (_QuickInfo != value)
        //        {
        //            Forms.QuickInfo.Close();
        //            _QuickInfo = value;
        //        }
        //    }
        //}
        //#endregion

        [DefaultValue(false)]
        public bool SpellChecking
        {
            get
            {
                return _SpellChecking;
            }
            set
            {
                if (_SpellChecking == value) { return; }

                _SpellChecking = value;
                AbortSpellChecking();

                Invalidate();
            }
        }

        [DefaultValue(enSteuerelementVerhalten.Scrollen_ohne_Textumbruch)]
        public enSteuerelementVerhalten Verhalten
        {
            get
            {
                return _Verhalten;
            }
            set
            {
                if (_Verhalten == value) { return; }
                _Verhalten = value;

                if (_SliderY != null)
                {
                    _SliderY.Visible = false;
                    _SliderY.Value = 0;
                }


                Invalidate();
            }
        }

        [DefaultValue("")]
        public string AllowedChars
        {
            get
            {
                return _AllowedChars;
            }
            set
            {
                if (value == _AllowedChars) { return; }
                _AllowedChars = value;
                GenerateETXT(false);

            }
        }

        [DefaultValue("")]
        public string Suffix
        {
            get
            {
                return _Suffix;
            }
            set
            {
                if (value == _Suffix) { return; }
                _Suffix = value;
                Invalidate();
            }
        }

        [DefaultValue(enDataFormat.Text)]
        public enDataFormat Format
        {
            get
            {
                return _Format;
            }
            set
            {
                if (_Format == value) { return; }
                _Format = value;
                GenerateETXT(false);
            }
        }

        [DefaultValue("")]
        public new string Text
        {
            get
            {
                if (_eTxt == null) { return string.Empty; }

                if (_Format == enDataFormat.Text_mit_Formatierung)
                {
                    return _eTxt.HtmlText;
                }

                return _eTxt.PlainText;
            }

            set
            {

                if (!string.IsNullOrEmpty(value))
                {
                    value = value.Replace("\n", string.Empty);
                    value = value.Replace("\r", "\r\n");
                }


                if (_Format == enDataFormat.Text_mit_Formatierung)
                {
                    if (_eTxt != null && value == _eTxt.HtmlText) { return; }
                }
                else
                {
                    if (_eTxt != null && value == _eTxt.PlainText) { return; }
                }


                AbortSpellChecking();

                lock (Dictionary.Lock_SpellChecking)
                {
                    _eTxt = null;
                    GenerateETXT(true);
                    if (_Format == enDataFormat.Text_mit_Formatierung)
                    {
                        _eTxt.HtmlText = value;
                    }
                    else
                    {
                        _eTxt.PlainText = value;
                    }

                    Invalidate();
                    CheckIfTextIsChanded(value);
                }
            }
        }

        [DefaultValue(false)]
        public bool MultiLine
        {
            get
            {
                return _Multiline;
            }
            set
            {
                if (value == _Multiline) { return; }

                _Multiline = value;
                GenerateETXT(false);
                if (_SliderY != null)
                {
                    _SliderY.Visible = false;
                    _SliderY.Value = 0;
                }
                Invalidate();
            }
        }

        //[DefaultValue(true)]
        //public bool InstantChangedEvent
        //{
        //    get
        //    {
        //        return _InstantChangedEvent;
        //    }
        //    set
        //    {
        //        if (value == _InstantChangedEvent) { return; }


        //        if (!_InstantChangedEvent)
        //        {
        //            _InstantChangeTimer.Tick -= _InstantChangeTimer_Tick;
        //            _InstantChangeTimer.Enabled = true;
        //            _InstantChangeTimer.Dispose();

        //        }

        //        if (_LastTextChange != null) { OnTextChanged(); }
        //        _InstantChangedEvent = value;

        //        if (!_InstantChangedEvent)
        //        {
        //            _LastTextChange = null;
        //            _InstantChangeTimer = new System.Windows.Forms.Timer();
        //            _InstantChangeTimer.Interval = 1000;
        //            _InstantChangeTimer.Enabled = true;
        //            _InstantChangeTimer.Tick += _InstantChangeTimer_Tick;
        //        }



        //    }
        //}

        //private void _InstantChangeTimer_Tick(object sender, System.EventArgs e)
        //{
        //    if (_LastTextChange == null) { return; }
        //    if (DateTime.Now.Subtract((DateTime)_LastTextChange).TotalSeconds < 5) { return; }
        //    _InstantChangeTimer.Enabled = false;
        //    OnTextChanged();
        //}


        #endregion

        #region  Events 
        public new event EventHandler TextChanged;
        public new event EventHandler Enter;
        public event EventHandler ESC;
        public event EventHandler TAB;
        public event EventHandler<ContextMenuInitEventArgs> ContextMenuInit;
        public event EventHandler<ContextMenuItemClickedEventArgs> ContextMenuItemClicked;
        public event EventHandler<MultiUserFileGiveBackEventArgs> NeedDatabaseOfAdditinalSpecialChars;

        #endregion




        #region  Form Ereignisse 
        private void Blinker_Tick(object sender, System.EventArgs e)
        {
            if (!Focused()) { return; }
            if (!Enabled) { return; }

            if (_MarkStart > -1 && _MarkEnd > -1) { _Cursor_CharPos = -1; }

            if (_Cursor_CharPos > -1)
            {
                _Cursor_Visible = !_Cursor_Visible;
                Invalidate();
            }
            else
            {
                if (_Cursor_Visible)
                {
                    _Cursor_Visible = false;
                    Invalidate();
                }
            }

        }





        // Fokus

        protected override void OnLostFocus(System.EventArgs e)
        {
            base.OnLostFocus(e);


            //if (_LastTextChange != null) { OnTextChanged(); }


            _LastUserActionForSpellChecking = DateTime.Now.AddSeconds(-30);
            if (!FloatingInputBoxListBoxStyle.IsShowing(this)) { MarkClear(); }

            Blinker.Enabled = false;
            CursorClear();
            Invalidate(); // Muss sein, weil evtl. der Cursor stehen bleibt
        }


        protected override void OnGotFocus(System.EventArgs e)
        {
            base.OnGotFocus(e);

            if (!Enabled) { return; }

            if (_eTxt == null) { GenerateETXT(true); }

            if (!FloatingInputBoxListBoxStyle.IsShowing(this))
            {
                SetCursorToEnd();
                if (!_eTxt.Multiline) { if (!ContainsMouse() || !MousePressing()) { MarkAll(); } }

                _LastUserActionForSpellChecking = DateTime.Now.AddSeconds(-30);
            }

            Blinker.Enabled = true;

        }

        // Tastatur
        protected override void OnKeyDown(System.Windows.Forms.KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (!Enabled) { return; }
            _LastUserActionForSpellChecking = DateTime.Now;
            if (_MouseValue != 0) { return; }

            switch (e.KeyCode)
            {
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
                    CheckIfTextIsChanded(_eTxt.PlainText);
                    break;
            }

            _Cursor_Visible = true;

            Invalidate();
        }



        #region  Cursor Berechnung 


        private void Cursor_Richtung(short X, short Y)
        {

            if (X != 0)
            {
                if (_Cursor_CharPos > -1)
                {
                    if (X == -1 && _Cursor_CharPos > 0) { _Cursor_CharPos--; }
                    if (X == 1 && _Cursor_CharPos < _eTxt.Chars.Count) { _Cursor_CharPos++; }
                }
                else if (_MarkStart > -1 && _MarkEnd > -1)
                {
                    if (X == -1) { _Cursor_CharPos = Math.Min(_MarkEnd, _MarkStart); }
                    if (X == 1) { _Cursor_CharPos = Math.Max(_MarkEnd, _MarkStart) + 1; }
                }
                else
                {
                    _Cursor_CharPos = 0;
                }
            }

            MarkClear();

            var ri = _eTxt.CursorPixelPosx(_Cursor_CharPos);

            if (_Cursor_CharPos < 0) { _Cursor_CharPos = 0; }

            if (Y > 0)
            {

                if (_Cursor_CharPos >= _eTxt.Chars.Count)
                {
                    _Cursor_CharPos = _eTxt.Chars.Count;
                }
                else
                {
                    _Cursor_CharPos = Cursor_PosAt(ri.Left + _eTxt.DrawingPos.X, ri.Top + ri.Height / 2.0 + _eTxt.Chars[_Cursor_CharPos].Size.Height + _eTxt.DrawingPos.Y);
                }


            }
            else if (Y < 0)
            {
                if (_Cursor_CharPos >= _eTxt.Chars.Count)
                {
                    if (_eTxt.Chars.Count > 0)
                    {
                        _Cursor_CharPos = Cursor_PosAt(ri.Left + _eTxt.DrawingPos.X, ri.Top + ri.Height / 2.0 - _eTxt.Chars[_Cursor_CharPos - 1].Size.Height + _eTxt.DrawingPos.Y);
                    }
                    else
                    {
                        _Cursor_CharPos = 0;
                    }

                }
                else
                {
                    _Cursor_CharPos = Cursor_PosAt(ri.Left + _eTxt.DrawingPos.X, ri.Top + ri.Height / 2.0 - _eTxt.Chars[_Cursor_CharPos].Size.Height + _eTxt.DrawingPos.Y);
                }

            }


            if (_Cursor_CharPos < 0) { _Cursor_CharPos = 0; }
            _Cursor_Visible = true;


        }


        #endregion


        #region  Übergebene Form Ereignisse 

        // Tastatur
        internal new void KeyPress(enASCIIKey KeyAscii)
        {

            // http://www.manderby.com/informatik/allgemeines/ascii.php

            if (_MouseValue != 0) { return; }



            switch (KeyAscii)
            {
                case enASCIIKey.DEL:
                    // Eigentlich auch noch Ascii Code - steht bei ISO als Del
                    Char_DelBereich(_Cursor_CharPos, _Cursor_CharPos + 1);
                    break;
                case enASCIIKey.ENTER:
                    {
                        // war vorher mal vAutoumbruch
                        if (MultiLine)
                        {
                            Char_DelBereich(-1, -1);
                            if (_eTxt.InsertChar(KeyAscii, _Cursor_CharPos))
                            {
                                _Cursor_CharPos++;
                            }
                        }

                        break;
                    }
                default:
                    {
                        if (KeyAscii >= enASCIIKey.Space && KeyAscii <= enASCIIKey.Chr255) //Ascii-Codes (Außer 127 = DEL)
                        {
                            Char_DelBereich(-1, -1);
                            if (_eTxt.InsertChar(KeyAscii, _Cursor_CharPos))
                            {
                                _Cursor_CharPos++;
                            }


                        }
                        else
                        {
                            switch (KeyAscii)
                            {
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
                                case enASCIIKey.LineFeed:
                                //Zeilenumbruch
                                // Kommt vor, wen man was aus einem anderen Programm kopiert,
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

            CheckIfTextIsChanded(_eTxt.PlainText);
        }






        #endregion

        #region  Clipboard 
        private void Clipboard_Paste()
        {
            // VORSICHT!
            // Seltsames Verhalten von VB.NET
            // Anscheinend wird bei den Clipboard operationen ein DoEventXsx ausgelöst.
            // Dadurch kommt es zum Refresh des übergeordneten Steuerelementes, warscheinlich der Textbox.
            // Deshalb  muss 'Char_DelBereich' NACH den Clipboard-Operationen stattfinden.
            if (!System.Windows.Forms.Clipboard.GetDataObject().GetDataPresent(System.Windows.Forms.DataFormats.Text)) { return; }
            Char_DelBereich(-1, -1);
            InsertText(Convert.ToString(System.Windows.Clipboard.GetDataObject().GetData(System.Windows.Forms.DataFormats.UnicodeText)));
        }


        public void InsertText(string nt)
        {
            nt = nt.Replace(Constants.beChrW1.ToString(), "\r");
            nt = nt.RemoveChars(Constants.Char_NotFromClip);

            if (!MultiLine) { nt = nt.RemoveChars("\r"); }

            foreach (var t in nt)
            {
                var a = (enASCIIKey)t;
                if (_eTxt.InsertChar(a, _Cursor_CharPos))
                {
                    _Cursor_CharPos++;
                }
            }

            CheckIfTextIsChanded(_eTxt.PlainText);
        }

        private void Clipboard_Copy()
        {

            if (_MarkStart < 0 || _MarkEnd < 0) { return; }

            Selection_Repair(true);

            var tt = _eTxt.ConvertCharToPlainText(_MarkStart, _MarkEnd - 1);
            if (string.IsNullOrEmpty(tt)) { return; }


            tt = tt.Replace("\n", string.Empty);
            tt = tt.Replace("\r", "\r\n");

            var datobj = new System.Windows.Forms.DataObject();
            datobj.SetData(System.Windows.Forms.DataFormats.Text, tt);
            System.Windows.Clipboard.SetDataObject(datobj);
        }
        #endregion




        protected override void OnKeyPress(System.Windows.Forms.KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            _LastUserActionForSpellChecking = DateTime.Now;

            if (!Enabled)
            {
                if (e.KeyChar != (int)enASCIIKey.StrgX) { e.KeyChar = (char)enASCIIKey.StrgC; }
                if (e.KeyChar != (int)enASCIIKey.StrgC) { return; }
            }

            switch ((int)e.KeyChar)
            {
                case (int)enASCIIKey.ENTER:
                    KeyPress((enASCIIKey)e.KeyChar);
                    OnEnter();
                    return;

                case (int)enASCIIKey.ESC:
                    OnESC();
                    return;

                case (int)enASCIIKey.TAB:
                    OnTAB();
                    return;

                default:
                    KeyPress((enASCIIKey)e.KeyChar);
                    break;
            }

            Invalidate();

            if (IsDisposed) { return; }

            CheckIfTextIsChanded(_eTxt.PlainText);
        }

        internal bool WordStarts(string word, int position)
        {


            if (InvokeRequired)
            {
                return (bool)Invoke(new Func<bool>(() => WordStarts(word, position)));
            }

            try
            {

                //if (word.StartsWith("ACE A") && position == 11) { word = word; }

                if (_eTxt == null) { GenerateETXT(true); }

                if (_eTxt == null) { return false; }


                if (position + word.Length > _eTxt.Chars.Count + 1) { return false; }

                if (position > 0 && !_eTxt.Chars[position - 1].isWordSeperator()) { return false; }

                if (position + word.Length < _eTxt.Chars.Count && !_eTxt.Chars[position + word.Length].isWordSeperator()) { return false; }

                var tt = _eTxt.ConvertCharToPlainText(position, position + word.Length - 1);

                return word.ToUpper() == tt.ToUpper();

            }
            catch (Exception)
            {
                return WordStarts(word, position);
            }




        }

        private void OnTAB()
        {
            TAB?.Invoke(this, System.EventArgs.Empty);
        }

        private void OnESC()
        {
            ESC?.Invoke(this, System.EventArgs.Empty);
        }

        private void OnEnter()
        {
            Enter?.Invoke(this, System.EventArgs.Empty);
        }

        // Mouse
        protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e)
        {
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


        protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseUp(e);

            _LastUserActionForSpellChecking = DateTime.Now;

            if (Enabled)
            {


                if (_MouseValue == 9999)
                {
                    //es Wurde Doppelgeklickt
                }
                else
                {
                    if (_MarkStart == _MarkEnd || _MarkEnd < 0)
                    {
                        _Cursor_CharPos = Cursor_PosAt(e.X, e.Y);
                        MarkClear();
                        if (e.Button == System.Windows.Forms.MouseButtons.Right)
                        {
                            FloatingInputBoxListBoxStyle.ContextMenuShow(this, e);
                        }
                    }
                    else
                    {
                        CursorClear();
                        Selection_Repair(true);
                        if (e.Button == System.Windows.Forms.MouseButtons.Right)
                        {
                            FloatingInputBoxListBoxStyle.ContextMenuShow(this, e);
                        }
                    }
                }

                _MouseValue = 0;


            }
            else
            {
                CursorClear();
            }

            Invalidate();
        }


        protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e)
        {
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

        #endregion



        public new void Focus()
        {
            if (Focused()) { return; }
            base.Focus();
        }



        public new bool Focused()
        {
            if (base.Focused) { return true; }
            if (_SliderY != null && _SliderY.Focused()) { return true; }
            return false;
        }



        private void CheckIfTextIsChanded(string NewPlainText)
        {
            if (NewPlainText == _LastCheckedText) { return; }
            _LastCheckedText = NewPlainText;

            if (Dictionary.DictionaryRunning(true)) { _MustCheck = true; }

            OnTextChanged();
            return;
        }



        /// <summary>
        /// Löst das Ereignis aus und setzt _LastUserChangingTime auf NULL.
        /// </summary>
        protected virtual void OnTextChanged()
        {
            TextChanged?.Invoke(this, System.EventArgs.Empty);
        }

        protected override bool IsInputKey(System.Windows.Forms.Keys keyData)
        {
            // Ganz wichtig diese Routine!
            // Wenn diese NICHT ist, geht der Fokus weg, sobald der cursor gedrückt wird.
            // http://technet.microsoft.com/de-de/subscriptions/control.isinputkey%28v=vs.100%29

            switch (keyData)
            {
                case System.Windows.Forms.Keys.Up:
                case System.Windows.Forms.Keys.Down:
                case System.Windows.Forms.Keys.Left:
                case System.Windows.Forms.Keys.Right:
                    return true;
            }

            return false;
        }



        /// <summary>
        /// Prüft - bei Multiline Zeile für Zeile - ob der Text in der Textbox zulässig ist.
        /// </summary>
        /// <param name="MitMeldung"></param>
        /// <returns>Ergibt Wahr, wenn der komplette Text dem Format entspricht. Andernfalls Falsch.</returns>
        /// <remarks></remarks>
        public bool Text_IsOkay(bool MitMeldung)
        {

            if (!_eTxt.PlainText.IsFormat(_Format, _Multiline))
            {

                if (MitMeldung) { MessageBox.Show("Ihre Eingabe entspricht nicht<br>dem erwarteten Format.", enImageCode.Warnung, "OK"); }
                return false;

            }
            return true;
        }





        /// <summary>
        /// Wenn das Format, die Maxlänge oder Multiline sich geändert haben,
        /// wird für das dementsprechende Format die Verbote/Erlaubnisse gesetzt.
        /// z.B. wird beim Format Datum die Maxlänge auf 10 gesetzt und nur noch Ziffern und Punkt erlaubt.
        /// </summary>
        /// <remarks></remarks>
        private void GenerateETXT(bool ResetCoords)
        {

            if (InvokeRequired)
            {
                Invoke(new Action(() => GenerateETXT(ResetCoords)));
                return;
            }


            if (_eTxt == null)
            {

                var state = enStates.Standard;
                if (!Enabled) { state = enStates.Standard_Disabled; }

                _eTxt = new ExtText(GetDesign(), state);

                ResetCoords = true;
            }

            _eTxt.Multiline = _Multiline;


            if (!string.IsNullOrEmpty(_AllowedChars))
            {
                _eTxt.AllowedChars = _AllowedChars;
            }
            else
            {
                _eTxt.AllowedChars = _Format.AllowedChars();
            }

            if (ResetCoords)
            {
                // Hier Standard-Werte Setzen, die Draw-Routine setzt bei Bedarf um
                if (_SliderY != null)
                {
                    _SliderY.Visible = false;
                    _SliderY.Value = 0;
                }

                _eTxt.DrawingPos.X = Skin.PaddingSmal;
                _eTxt.DrawingPos.Y = Skin.PaddingSmal;
                _eTxt.DrawingArea = new Rectangle(0, 0, -1, -1);
                _eTxt.TextDimensions = Size.Empty;
            }


        }








        #region  Markierung (Selection)
        private void Selection_WortMarkieren(int Pos)
        {

            if (_eTxt == null || _eTxt.Chars.Count == 0)
            {
                MarkClear();
                return;
            }

            _MarkStart = _eTxt.WordStart(Pos);
            _MarkEnd = _eTxt.WordEnd(Pos);

            CursorClear();

            Selection_Repair(true);

        }


        /// <summary>
        /// Prüft die den Selektions-Bereich auf nicht erlaubte Werte und Repariert diese.
        /// </summary>
        /// <param name="SwapThem">Bei True werden MarkStart und MarkEnd richtig angeordnet. (Start kleiner/gleich End) </param>
        /// <remarks></remarks>
        private void Selection_Repair(bool SwapThem)
        {

            if (_eTxt == null || _eTxt.Chars.Count == 0) { MarkClear(); }
            if (_MarkStart < 0 && _MarkEnd < 0) { return; }


            _MarkStart = Math.Max(_MarkStart, 0);
            _MarkStart = Math.Min(_MarkStart, _eTxt.Chars.Count);

            _MarkEnd = Math.Max(_MarkEnd, 0);
            _MarkEnd = Math.Min(_MarkEnd, _eTxt.Chars.Count);


            if (SwapThem && _MarkStart > _MarkEnd)
            {
                modAllgemein.Swap(ref _MarkStart, ref _MarkEnd);
            }


        }
        #endregion



        protected override void OnDoubleClick(System.EventArgs e)
        {
            base.OnDoubleClick(e);
            Selection_WortMarkieren(_MarkStart);
            _LastUserActionForSpellChecking = DateTime.Now;
            _MouseValue = 9999;
            Invalidate();
        }



        private void MarkClear()
        {
            _MarkStart = -1;
            _MarkEnd = -1;
        }


        private void MarkAndGenerateZone(Graphics GR)
        {

            _eTxt.Check(0, _eTxt.Chars.Count - 1, false);


            if (_MarkStart < 0 || _MarkEnd < 0) { return; }

            Selection_Repair(false);
            var MaS = Math.Min(_MarkStart, _MarkEnd);
            var MaE = Math.Max(_MarkStart, _MarkEnd);


            if (MaS == MaE) { return; }

            _eTxt.Check(MaS, MaE - 1, true);

            var TmpcharS = MaS;

            for (var cc = MaS; cc <= MaE; cc++)
            {

                if (cc == MaE || _eTxt.Chars[cc].Pos.X < _eTxt.Chars[TmpcharS].Pos.X || Math.Abs(_eTxt.Chars[cc].Pos.Y - _eTxt.Chars[TmpcharS].Pos.Y) > 0.001) //Jetzt ist der Zeitpunkt zum Zeichen/start setzen
                {

                    var r = new Rectangle((int)(_eTxt.Chars[TmpcharS].Pos.X + _eTxt.DrawingPos.X), (int)(_eTxt.Chars[TmpcharS].Pos.Y + 2 + _eTxt.DrawingPos.Y), (int)(_eTxt.Chars[cc - 1].Pos.X + _eTxt.Chars[cc - 1].Size.Width - _eTxt.Chars[TmpcharS].Pos.X), (int)(_eTxt.Chars[cc - 1].Pos.Y + _eTxt.Chars[cc - 1].Size.Height - _eTxt.Chars[TmpcharS].Pos.Y));

                    if (r.Width < 2) { r = new Rectangle(r.Left, r.Top, 2, r.Height); }


                    if (_eTxt.Chars[TmpcharS].State != enStates.Undefiniert)
                    {
                        Skin.Draw_Back(GR, _eTxt.Design, _eTxt.Chars[TmpcharS].State, r, null, false);
                        Skin.Draw_Border(GR, _eTxt.Design, _eTxt.Chars[TmpcharS].State, r);
                    }

                    TmpcharS = cc;
                }


            }

        }



        private int HotPosition()
        {
            if (_Cursor_CharPos > -1) { return _Cursor_CharPos; }
            if (_MarkStart > -1 && _MarkEnd < 0) { return _MarkEnd; }
            if (_MarkStart > -1 && _MarkEnd > -1) { return _MarkEnd; }
            return -1;
        }





        protected virtual enDesign GetDesign()
        {
            return enDesign.TextBox;
        }


        protected override void DrawControl(Graphics gr, enStates state)
        {

            if (_eTxt == null) { GenerateETXT(true); }




            if (state == enStates.Checked_Disabled)
            {
                Develop.DebugPrint("Checked Disabled");
                state = enStates.Checked_Disabled;
            }


            if (state == enStates.Standard_Disabled)
            {
                if (_MarkStart > -1) { Develop.DebugPrint("Disabled & Markstart:" + _MarkStart); }
                if (_MarkEnd > -1) { Develop.DebugPrint("Disabled & MarkEnd:" + _MarkStart); }
                MarkClear();
            }



            // Erst den Typ richtig stellen, dann den State ändern!
            _eTxt.Design = GetDesign();
            _eTxt.State = state;



            var sliderVisible = false;
            var effectWidth = Width;
            if (_Multiline)
            {
                sliderVisible = _eTxt.Height() > (Height - 16);
            }
            else
            {
                sliderVisible = _eTxt.Height() > Height;
            }

            if (sliderVisible) { effectWidth = Width - 18; }


            switch (_Verhalten)
            {
                case enSteuerelementVerhalten.Scrollen_mit_Textumbruch:
                    _eTxt.TextDimensions = new Size(effectWidth - Skin.PaddingSmal * 2, -1);
                    _eTxt.DrawingArea = new Rectangle(0, 0, effectWidth, Height);
                    break;

                case enSteuerelementVerhalten.Scrollen_ohne_Textumbruch:
                    var hp = HotPosition();
                    _eTxt.TextDimensions = Size.Empty;
                    _eTxt.DrawingArea = new Rectangle(0, 0, effectWidth, Height);

                    if (hp < 0)
                    {
                        // Mach nix
                    }
                    else if (hp == 0)
                    {
                        _eTxt.DrawingPos.X = Skin.PaddingSmal;
                    }
                    else if (hp > _eTxt.Chars.Count - 1)
                    {
                        if (_eTxt.Width() > Width - Skin.PaddingSmal * 2)
                        {
                            _eTxt.DrawingPos.X = Width - _eTxt.Width() - Skin.PaddingSmal * 2;
                        }
                        else
                        {
                            _eTxt.DrawingPos.X = Skin.PaddingSmal;
                        }
                    }
                    else
                    {
                        var r = _eTxt.CursorPixelPosx(hp);

                        if (r.X > Width - Skin.PaddingSmal * 4 - _eTxt.DrawingPos.X)
                        {
                            _eTxt.DrawingPos.X = Width - Skin.PaddingSmal * 4 - r.X + 1;
                        }
                        else if (r.X + _eTxt.DrawingPos.X < Skin.PaddingSmal * 2)
                        {
                            _eTxt.DrawingPos.X = Skin.PaddingSmal * 2 - r.X + 1;
                        }
                    }

                    if (_eTxt.DrawingPos.X > Skin.PaddingSmal) { _eTxt.DrawingPos.X = Skin.PaddingSmal; }
                    break;

                case enSteuerelementVerhalten.Steuerelement_Anpassen:
                    sliderVisible = false;
                    _eTxt.TextDimensions = Size.Empty;

                    if (this is ComboBox)
                    {
                        Width = Math.Max(_eTxt.Width() + Skin.PaddingSmal * 3 + 20, Width);
                    }
                    else
                    {
                        Width = Math.Max(_eTxt.Width() + Skin.PaddingSmal * 3, Width);
                    }
                    Height = Math.Max(_eTxt.Height() + Skin.PaddingSmal * 2, Height);


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

            if (sliderVisible)
            {
                GetSlider();

                _SliderY.Visible = true;

                _SliderY.Width = 18;
                _SliderY.Height = Height;
                _SliderY.Left = Width - _SliderY.Width;
                _SliderY.Top = 0;
                _eTxt.DrawingPos.Y = (int)-_SliderY.Value;
                _SliderY.Maximum = _eTxt.Height() + 16 - DisplayRectangle.Height;
            }
            else
            {
                if (_SliderY != null)
                {
                    _SliderY.Visible = false;
                    _SliderY.Value = 0;
                }

                _eTxt.DrawingPos.Y = Skin.PaddingSmal;
            }






            Skin.Draw_Back(gr, _eTxt.Design, state, DisplayRectangle, this, true);
            Cursor_Show(gr);
            MarkAndGenerateZone(gr);

            _eTxt.Draw(gr, 1);


            if (!string.IsNullOrEmpty(_Suffix))
            {
                var r = new Rectangle(_eTxt.Width() + _eTxt.DrawingPos.X, _eTxt.DrawingPos.Y, 1000, 1000);

                if (_eTxt.Chars.Count > 0)
                {
                    r.X += 2;
                    Skin.Draw_FormatedText(gr, _Suffix, _eTxt.Design, enStates.Standard_Disabled, null, enAlignment.Top_Left, r, this, false, false);
                }
                else
                {
                    Skin.Draw_FormatedText(gr, "[in " + _Suffix + "]", _eTxt.Design, enStates.Standard_Disabled, null, enAlignment.Top_Left, r, this, false, Translate);
                }
            }

            Skin.Draw_Border(gr, _eTxt.Design, state, DisplayRectangle);





            if (_MustCheck && !Dictionary.IsSpellChecking && Dictionary.DictionaryRunning(true) && !SpellChecker.CancellationPending && !SpellChecker.IsBusy) { SpellChecker.RunWorkerAsync(); }
        }


        private void Cursor_Show(Graphics GR)
        {
            if (!_Cursor_Visible) { return; }
            if (_Cursor_CharPos < 0) { return; }

            var r = _eTxt.CursorPixelPosx(_Cursor_CharPos);

            GR.DrawLine(new Pen(Color.Black), r.Left + _eTxt.DrawingPos.X, r.Top + _eTxt.DrawingPos.Y, r.Left + _eTxt.DrawingPos.X, r.Bottom + _eTxt.DrawingPos.Y);
        }

        private void SliderY_ValueChange(object sender, System.EventArgs e)
        {
            Invalidate();
        }


        protected override void OnMouseWheel(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (_SliderY == null || !_SliderY.Visible) { return; }
            _LastUserActionForSpellChecking = DateTime.Now;
            _SliderY.DoMouseWheel(e);
        }












        #region  Einzel-Charakter - Manipulation



        /// <summary>
        /// Sucht den aktuellen Buchstaben, der unter den angegeben Koordinaten liegt.
        /// Wird auf die hintere Hälfte eines Zeichens gewählt, wird der nächste Buchstabe angegeben.
        /// </summary>
        /// <remarks></remarks>
        private int Cursor_PosAt(double PixX, double PixY)
        {


            if (_eTxt == null) { return -1; }

            // Das geht am Einfachsten.... 
            if (PixX < _eTxt.DrawingPos.X && PixY < _eTxt.DrawingPos.Y) { return 0; }


            PixX = Math.Max(PixX, _eTxt.DrawingPos.X);
            PixY = Math.Max(PixY, _eTxt.DrawingPos.Y);
            PixX = Math.Min(PixX, _eTxt.DrawingPos.X + _eTxt.Width());
            PixY = Math.Min(PixY, _eTxt.DrawingPos.Y + _eTxt.Height());

            var c = _eTxt.Char_Search(PixX, PixY);
            if (c < 0) { c = 0; }

            if (c < _eTxt.Chars.Count && PixX > _eTxt.DrawingPos.X + _eTxt.Chars[c].Pos.X + _eTxt.Chars[c].Size.Width / 2.0) { return c + 1; }

            return c;
        }




        public void Char_DelBereich(int Von, int Bis)
        {

            if (_MarkStart > -1 && _MarkEnd > -1)
            {
                Von = _MarkStart;
                Bis = _MarkEnd;
            }

            MarkClear();

            _Cursor_Visible = true;

            if (Von < 0 || Bis < 0) { return; }
            _Cursor_CharPos = Von;


            _eTxt.Delete(Von, Bis);

        }

        #endregion




        private void CursorClear()
        {
            _Cursor_CharPos = -1;
            _Cursor_Visible = false;
        }




        private void SetCursorToEnd()
        {

            if (_eTxt == null)
            {
                // Beim Durchtabben...
                _Cursor_CharPos = -1;
            }
            else
            {
                _Cursor_CharPos = _eTxt.Chars.Count;
            }


            _Cursor_Visible = true;
        }


        private void MarkAll()
        {
            if (_eTxt != null && _eTxt.Chars.Count > 0)
            {
                _MarkStart = 0;
                _MarkEnd = _eTxt.Chars.Count;
                _Cursor_Visible = false;
            }
            else
            {
                MarkClear();
            }
        }


        private void SpellChecker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {

                if (Dictionary.IsSpellChecking) { return; }

                if (DateTime.Now.Subtract(_LastUserActionForSpellChecking).TotalSeconds < 2) { return; }


                Dictionary.IsSpellChecking = true;


                if (!_SpellChecking) { return; }

                if (!Dictionary.DictionaryRunning(true)) { return; }

                if (_eTxt == null) { return; }


                var Pos = 0;
                var woEnd = -1;

                bool Ok;
                do
                {
                    Ok = true;

                    SpellChecker.ReportProgress(0, "Unmark");

                    try
                    {

                        do
                        {
                            if (SpellChecker.CancellationPending) { return; }
                            Pos = Math.Max(woEnd + 1, Pos + 1);

                            if (_eTxt == null) { return; }// Das TExtfeld ist spontan geschlossen worden.
                            if (Pos >= _eTxt.Chars.Count) { break; }

                            var woStart = _eTxt.WordStart(Pos);

                            if (woStart > -1)
                            {
                                woEnd = _eTxt.WordEnd(Pos);
                                var wort = _eTxt.Word(Pos);

                                if (!Dictionary.IsWordOk(wort))
                                {
                                    if (SpellChecker.CancellationPending) { return; }
                                    SpellChecker.ReportProgress((int)(woEnd / (double)_eTxt.Chars.Count * 100), "Mark;" + woStart + ";" + (woEnd - 1));
                                }
                            }


                        } while (true);

                    }
                    catch
                    {
                        Ok = false;
                    }

                } while (!Ok);



            }
            catch (Exception ex)
            {
                Develop.DebugPrint(ex);
            }



            SpellChecker.ReportProgress(100, "Done");


        }

        private void SpellChecker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //Ja, Multithreading ist kompliziert...
            if (SpellChecker.CancellationPending) { return; }

            var x = Convert.ToString(e.UserState).SplitBy(";");

            switch (x[0])
            {
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

        public void Mark(enMarkState markstate, int first, int last)
        {
            _eTxt?.Mark(markstate, first, last);
        }

        public void Unmark(enMarkState markstate)
        {
            _eTxt?.Unmark(markstate);
        }



        private void SpellChecker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Dictionary.IsSpellChecking = false;
        }

        private void AbortSpellChecking()
        {
            if (SpellChecker.IsBusy) { SpellChecker.CancelAsync(); }
        }


        public bool ContextMenuItemClickedInternalProcessig(object sender, ContextMenuItemClickedEventArgs e)
        {

            Focus();

            var NewWord = "";


            _MarkStart = int.Parse(e.Tags.TagGet("MarkStart"));
            _MarkEnd = int.Parse(e.Tags.TagGet("MarkEnd"));
            _Cursor_CharPos = int.Parse(e.Tags.TagGet("Cursorpos"));


            var tmp = e.ClickedComand;

            if (e.ClickedComand.StartsWith("#ChangeTo:"))
            {
                NewWord = e.ClickedComand.Substring(10);
                tmp = "#ChangeTo";
            }


            switch (tmp)
            {
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
                    FloatingInputBoxListBoxStyle.Close(this);
                    if (_SpellChecking)
                    {
                        _MustCheck = false;
                        Dictionary.SpellCheckingAll(_eTxt, false);
                        _MustCheck = true;
                        Invalidate();
                    }
                    return true;

                case "#SpellChecking2":
                    FloatingInputBoxListBoxStyle.Close(this);
                    if (_SpellChecking)
                    {
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
                    return true;

                case "#NoCaption":
                    if (_MarkStart < 0 || _MarkEnd < 0) { return true; }
                    Selection_Repair(true);
                    _eTxt.StufeÄndern(_MarkStart, _MarkEnd - 1, 4);
                    return true;

                case "#Bold":
                    if (_MarkStart < 0 || _MarkEnd < 0) { return true; }
                    Selection_Repair(true);
                    _eTxt.StufeÄndern(_MarkStart, _MarkEnd - 1, 7);
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


        public void OnContextMenuItemClicked(ContextMenuItemClickedEventArgs e)
        {
            ContextMenuItemClicked?.Invoke(this, e);
        }

        private void AddSpecialChar()
        {

            var x = _Cursor_CharPos;

            var e = new MultiUserFileGiveBackEventArgs();
            OnNeedDatabaseOfAdditinalSpecialChars(e);


            var i = new ItemCollectionList(enBlueListBoxAppearance.Listbox);


            if (e.File is Database DB && DB.Bins.Count > 0)
            {
                foreach (var bmp in DB.Bins)
                {

                    if (bmp.Picture != null)
                    {
                        if (!string.IsNullOrEmpty(bmp.Name))
                        {
                            var crc = "DB_" + bmp.Name;
                            i.Add(new TextListItem(crc, bmp.Name, QuickImage.Get(crc, 20)));
                        }
                    }


                }

                i.Add(new LineListItem());
            }

            i.Add(new TextListItem("sphere", "Kugel", QuickImage.Get(enImageCode.Kugel, 20)));
            i.Add(new TextListItem("Warnung", "Warnung", QuickImage.Get(enImageCode.Warnung, 20)));
            i.Add(new TextListItem("Information", "Information", QuickImage.Get(enImageCode.Information, 20)));
            i.Add(new TextListItem("Kritisch", "Kritisch", QuickImage.Get(enImageCode.Kritisch, 20)));
            i.Add(new TextListItem("Frage", "Frage", QuickImage.Get(enImageCode.Frage, 20)));

            var r = InputBoxListBoxStyle.Show("Wählen sie:", i, enAddType.None, true);

            _Cursor_CharPos = x;

            if (r == null || r.Count != 1) { return; }


            Char_DelBereich(-1, -1);
            if (_eTxt.InsertImage(r[0], _Cursor_CharPos)) { _Cursor_CharPos++; }
        }

        private void OnNeedDatabaseOfAdditinalSpecialChars(MultiUserFileGiveBackEventArgs e)
        {
            NeedDatabaseOfAdditinalSpecialChars?.Invoke(this, e);
        }


        public void GetContextMenuItems(System.Windows.Forms.MouseEventArgs e, ItemCollectionList Items, out object HotItem, List<string> Tags, ref bool Cancel, ref bool Translate)
        {
            AbortSpellChecking();
            HotItem = null;

            Tags.TagSet("CursorPosBeforeClick", _Cursor_CharPos.ToString());
            var tmp = Cursor_PosAt(e.X, e.Y);
            Tags.TagSet("Char", tmp.ToString());
            Tags.TagSet("MarkStart", _MarkStart.ToString());
            Tags.TagSet("MarkEnd", _MarkEnd.ToString());
            Tags.TagSet("Cursorpos", _Cursor_CharPos.ToString());

            var tmpWord = _eTxt.Word(tmp);
            Tags.TagSet("Word", tmpWord);


            if (_SpellChecking && !Dictionary.IsWordOk(tmpWord))
            {
                Items.Add(new TextListItem("Rechtschreibprüfung", true));


                if (Dictionary.IsSpellChecking)
                {
                    Items.Add(new TextListItem("Gerade ausgelastet...", "Gerade ausgelastet...", false));
                    Items.Add(new LineListItem());
                }
                else
                {
                    var sim = Dictionary.SimilarTo(tmpWord);
                    if (sim != null)
                    {
                        foreach (var ThisS in sim)
                        {
                            Items.Add(new TextListItem("#ChangeTo:" + ThisS, " - " + ThisS));
                        }

                        Items.Add(new LineListItem());
                    }



                    Items.Add(new TextListItem("#SpellAdd", "'" + tmpWord + "' ins Wörterbuch aufnehmen", Dictionary.IsWriteable()));
                    if (tmpWord.ToLower() != tmpWord)
                    {
                        Items.Add(new TextListItem("#SpellAddLower", "'" + tmpWord.ToLower() + "' ins Wörterbuch aufnehmen", Dictionary.IsWriteable()));
                    }

                    Items.Add(new TextListItem("#SpellChecking", "Schnelle Rechtschreibprüfung", Dictionary.IsWriteable()));
                    Items.Add(new TextListItem("#SpellChecking2", "Alle Wörter sind ok", Dictionary.IsWriteable()));
                    Items.Add(new LineListItem());
                }

            }


            if (!(this is ComboBox cbx) || cbx.DropDownStyle == System.Windows.Forms.ComboBoxStyle.DropDown)
            {
                Items.Add(enContextMenuComands.Ausschneiden, Convert.ToBoolean(_MarkStart >= 0) && Enabled);
                Items.Add(enContextMenuComands.Kopieren, Convert.ToBoolean(_MarkStart >= 0));
                Items.Add(enContextMenuComands.Einfügen, System.Windows.Forms.Clipboard.ContainsText() && Enabled);


                if (_Format == enDataFormat.Text_mit_Formatierung)
                {
                    Items.Add(new LineListItem());
                    Items.Add(new TextListItem("#Sonderzeichen", "Sonderzeichen einfügen", QuickImage.Get(enImageCode.Sonne, 16), _Cursor_CharPos > -1));

                    if (Convert.ToBoolean(_MarkEnd > -1))
                    {
                        Items.Add(new LineListItem());
                        Items.Add(new TextListItem("#Caption", "Als Überschrift markieren", Skin.GetBlueFont(enDesign.TextBox_Stufe3, enStates.Standard).SymbolForReadableText(), _MarkEnd > -1));
                        Items.Add(new TextListItem("#Bold", "Fettschrift", Skin.GetBlueFont(enDesign.TextBox_Bold, enStates.Standard).SymbolForReadableText(), Convert.ToBoolean(_MarkEnd > -1)));
                        Items.Add(new TextListItem("#NoCaption", "Als normalen Text markieren", Skin.GetBlueFont(enDesign.TextBox, enStates.Standard).SymbolForReadableText(), Convert.ToBoolean(_MarkEnd > -1)));
                    }
                }
            }



        }

        public void OnContextMenuInit(ContextMenuInitEventArgs e)
        {
            ContextMenuInit?.Invoke(this, e);
        }

        protected override void OnEnabledChanged(System.EventArgs e)
        {
            MarkClear();
            base.OnEnabledChanged(e);
        }


        private Slider GetSlider()
        {

            if (_SliderY != null) { return _SliderY; }

            _SliderY = new Slider
            {
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



    }
}

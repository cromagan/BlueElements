// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes;
using BlueControls.Classes.ItemCollectionList;
using BlueControls.Designer_Support;
using BlueControls.EventArgs;
using BlueControls.Extended_Text;
using System.Collections.ObjectModel;
using static BlueBasics.ClassesStatic.Converter;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Controls;

[Designer(typeof(TextBoxDesigner))]
[DefaultEvent(nameof(TextChanged))]
public partial class TextBox : GenericControl, IContextMenu, IInputFormat {

    #region Fields

    private const string ExtCharFormat = "BlueElements.ExtChar";

    private readonly ExtText _eTxt;

    private int _blinkCount;

    private System.Threading.Timer? _blinker;

    private bool _blinkerEnabled;

    private bool _cursorVisible;

    private bool _doubleClicked;

    private string _lastCheckedText = string.Empty;

    private DateTime _lastUserActionForSpellChecking = DateTime.UtcNow;

    private int _markEnd = -1;

    private int _markStart = -1;
    private bool _mustCheck = true;

    private int _raiseChangeDelay;

    private Slider? _sliderY;

    #endregion

    #region Constructors

    public TextBox() : base(true, true, false) {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();
        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.

        _eTxt = new ExtText(Design, States.Standard); // Design auf Standard setzen wegen Virtal member call
        _eTxt.PropertyChanged += _eTxt_PropertyChanged;
        _blinker = new System.Threading.Timer(_ => {
            if (!IsDisposed && IsHandleCreated) {
                try { BeginInvoke(new Action(Blinker_Tick)); } catch { /* Control disposed */ }
            }
        }, null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
    }

    #endregion

    #region Events

    public event EventHandler? EnterKey;

    public event EventHandler? EscKey;

    public event EventHandler<NavigationDirectionEventArgs>? NavigateToNext;

    public event EventHandler? TabKey;

    #endregion

    #region Properties

    public static bool IsSpellChecking { get; private set; }

    [DefaultValue(AdditionalCheck.None)]
    public AdditionalCheck AdditionalFormatCheck { get; set; } = AdditionalCheck.None;

    [DefaultValue("")]
    public string AllowedChars {
        get;
        set {
            if (IsDisposed || field == value) { return; }
            field = value;
            GenerateEtxt(false);
        }
    } = string.Empty;

    [DefaultValue(true)]
    public bool ContextMenuDefault { get; set; } = true;

    [DefaultValue(null)]
    public ReadOnlyCollection<AbstractListItem>? CustomContextMenuItems { get; set; } = null;

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IReadOnlySet<string>? CustomVocabulary {
        get;
        set {
            if (IsDisposed || field == value) { return; }
            field = value;
            _mustCheck = true;
            Invalidate();
        }
    }

    [DefaultValue(4000)]
    public int MaxTextLength {
        get;
        set {
            if (IsDisposed || field == value) { return; }
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
            if (IsDisposed || field == value) { return; }
            field = value;
            GenerateEtxt(false);
            if (_sliderY != null) {
                _sliderY.Visible = false;
                _sliderY.Value = 0;
            }
            Invalidate();
        }
    }

    [DefaultValue(0)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int OffsetX { get; set; }

    [DefaultValue(0)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int OffsetY { get; set; }

    [DefaultValue(0)]
    public int RaiseChangeDelay {
        get => _raiseChangeDelay / 2; // Umrechnung aus Sekunden
        set {
            if (IsDisposed || _raiseChangeDelay == value * 2) { return; }
            _raiseChangeDelay = value * 2;
            RaiseEventIfTextChanged(false);
        }
    }

    [DefaultValue("")]
    public string RegexCheck {
        get;
        set {
            if (IsDisposed || field == value) { return; }
            field = value;
            GenerateEtxt(false);
        }
    } = string.Empty;

    [DefaultValue(false)]
    public bool SpellCheckingEnabled {
        get;
        set {
            if (IsDisposed || field == value) { return; }
            field = value;
            AbortSpellChecking();
            Invalidate();
        }
    }

    [DefaultValue("")]
    public string Suffix {
        get;
        set {
            if (IsDisposed || field == value) { return; }
            field = value;
            Invalidate();
        }
    } = string.Empty;

    [DefaultValue("")]
    public override string Text {
        get {
            if (IsDisposed) { return string.Empty; }
            return TextFormatingAllowed ? _eTxt.HtmlText : _eTxt.PlainText;
        }

        set {
            if (IsDisposed) { return; }

            var processedValue = value ?? string.Empty;
            if (!string.IsNullOrEmpty(processedValue)) {
                processedValue = processedValue.Replace("\n", string.Empty).Replace("\r", "\r\n");
            }

            if (TextFormatingAllowed) {
                if (processedValue == _eTxt.HtmlText) { return; }
            } else {
                if (processedValue == _eTxt.PlainText) { return; }
            }

            AbortSpellChecking();
            GenerateEtxt(true);

            if (TextFormatingAllowed) {
                _eTxt.HtmlText = processedValue;
            } else {
                _eTxt.PlainText = processedValue;
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
            if (IsDisposed || field == value) { return; }
            field = value;
            GenerateEtxt(false);
        }
    }

    [DefaultValue(SteuerelementVerhalten.Scrollen_ohne_Textumbruch)]
    public SteuerelementVerhalten Verhalten {
        get;
        set {
            if (IsDisposed || field == value) { return; }
            field = value;
            if (_sliderY != null) {
                _sliderY.Visible = false;
                _sliderY.Value = 0;
            }
            Invalidate();
        }
    } = SteuerelementVerhalten.Scrollen_ohne_Textumbruch;

    internal int CursorPosition {
        get => Math.Max(0, Math.Max(_markStart, _markEnd));
        set {
            _markStart = Math.Clamp(value, 0, _eTxt.Count);
            _markEnd = -1;
            Invalidate();
        }
    }

    protected virtual Design Design => Design.TextBox;

    #endregion

    #region Methods

    public static (int markStart, int markEnd, string word) GetContextData(object? context) {
        if (context is not List<string> tags) { return (-1, -1, string.Empty); }

        return (
            IntParse(tags.TagGet("MarkStart")),
            IntParse(tags.TagGet("MarkEnd")),
            tags.TagGet("Word") ?? string.Empty
        );
    }

    public virtual List<AbstractListItem>? GetContextMenuItems(object? hotItem) {
        List<AbstractListItem> contextMenu = [];
        AbortSpellChecking();

        var (markStart, markEnd, word) = GetContextData(hotItem);

        if (SpellCheckingEnabled && !Dictionary.IsWordOk(word, CustomVocabulary)) {
            contextMenu.Add(ItemOf("Rechtschreibprüfung", true));
            if (IsSpellChecking) {
                contextMenu.Add(ItemOf("Gerade ausgelastet...", "Gerade ausgelastet...", false));
            } else {
                var sim = Dictionary.SimilarTo(word, CustomVocabulary);
                if (sim != null) {
                    var delStart = markEnd < 0 ? _eTxt.WordStart(markStart) : markStart;
                    var delEnd = markEnd < 0 ? _eTxt.WordEnd(markStart) : markEnd;
                    foreach (var thisS in sim) {
                        contextMenu.Add(ItemOf($" - {thisS}", null, (s, e) => {
                            _markStart = Char_DelBereich(delStart, delEnd, false);
                            _markEnd = -1;
                            _markStart = Insert(_markStart, thisS);
                        }, true, string.Empty));
                    }
                } else {
                    contextMenu.Add(ItemOf("Keine Vorschläge", "Keine Vorschläge", false));
                }
            }
            contextMenu.Add(Separator());
        }

        var marked = markEnd > markStart;
        if (this is not ComboBox { DropDownStyle: not System.Windows.Forms.ComboBoxStyle.DropDown }) {
            contextMenu.Add(ItemOf("Ausschneiden", ImageCode.Schere, Contextmenu_Cut, marked));
            contextMenu.Add(ItemOf("Kopieren", ImageCode.Kopieren, Contextmenu_Copy, marked));
            contextMenu.Add(ItemOf("Einfügen (Text)", ImageCode.Clipboard, Contextmenu_Paste, System.Windows.Forms.Clipboard.ContainsText() && Enabled));

            if (TextFormatingAllowed) {
                contextMenu.Add(ItemOf("Einfügen (Link)", ImageCode.Clipboard, Contextmenu_Paste_Link, System.Windows.Forms.Clipboard.ContainsData(TableView.CellDataFormat) && Enabled));
                contextMenu.Add(Separator());
                contextMenu.Add(ItemOf("Sonderzeichen einfügen", ImageCode.Sonne, Contextmenu_Sonderzeichen, true));

                contextMenu.Add(Separator());
                contextMenu.Add(ItemOf("Als Überschrift markieren", Skin.GetBlueFont(Constants.Win11, PadStyles.Title).SymbolForReadableText(), Contextmenu_Caption, marked, string.Empty));
                contextMenu.Add(ItemOf("Fettschrift", Skin.GetBlueFont(Constants.Win11, PadStyles.Emphasized).SymbolForReadableText(), Contextmenu_Bold, marked, string.Empty));
                contextMenu.Add(ItemOf("Als normalen Text markieren", Skin.GetBlueFont(Constants.Win11, PadStyles.Standard).SymbolForReadableText(), Contextmenu_NoCaption, marked, string.Empty));
            }
        }

        return contextMenu;
    }

    public int Insert(int pos, string? nt) {
        if (string.IsNullOrEmpty(nt)) { return pos; }

        var filtered = nt.RemoveChars(Constants.Char_NotFromClip);
        if (!MultiLine) { filtered = filtered.RemoveChars("\r\n"); }

        foreach (var t in filtered) {
            pos = Insert(pos, t, false);
        }

        RaiseEventIfTextChanged(false);
        Invalidate();

        return pos;
    }

    public void Mark(MarkState markstate, int first, int last) => _eTxt.Mark(markstate, first, last);

    public void Unmark(MarkState markstate) => _eTxt.Unmark(markstate);

    internal void ProcessKey(AsciiKey keyAscii) {
        _blinkCount = 0;
        // http://www.manderby.com/informatik/allgemeines/ascii.php
        if (MousePressing) { return; }

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
                    _markStart = Insert(_markStart, (char)keyAscii, true);
                    if (_eTxt.Count >= MaxTextLength) {
                        OnNavigateToNext(NavigationDirection.Next);
                    }
                }
                break;
        }
    }

    internal bool WordStarts(string word, int position) {
        if (InvokeRequired) {
            return Invoke(new Func<bool>(() => WordStarts(word, position)));
        }
        try {
            if (position + word.Length > _eTxt.Count + 1) { return false; }
            if (position > 0 && !_eTxt[position - 1].IsWordSeparator()) { return false; }
            if (position + word.Length < _eTxt.Count && !_eTxt[position + word.Length].IsWordSeparator()) { return false; }
            var tt = _eTxt.BuildPlainText(position, position + word.Length - 1);
            return string.Equals(word, tt, StringComparison.OrdinalIgnoreCase);
        } catch {
            Develop.AbortAppIfStackOverflow();
            return WordStarts(word, position);
        }
    }

    protected override void Dispose(bool disposing) {
        if (IsDisposed) { return; }

        if (disposing) {
            EnterKey = null;
            EscKey = null;
            NavigateToNext = null;
            TabKey = null;
            _blinker?.Dispose();
            _blinker = null;
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

        _eTxt.PropertyChanged -= _eTxt_PropertyChanged;
        switch (Verhalten) {
            case SteuerelementVerhalten.Scrollen_mit_Textumbruch:
                _eTxt.TextDimensions = new Size(Width - (Skin.PaddingSmal * 2), -1);
                if (MultiLine ? _eTxt.HeightControl > Height - 16 : _eTxt.HeightControl > Height) {
                    effectWidth = Width - 18;
                    sliderVisible = true;
                } else {
                    effectWidth = Width;
                    sliderVisible = false;
                }
                _eTxt.TextDimensions = new Size(effectWidth - (Skin.PaddingSmal * 2), -1);
                _eTxt.AreaControl = new Rectangle(0, 0, effectWidth, Height);
                break;

            case SteuerelementVerhalten.Scrollen_ohne_Textumbruch:
                var hp = HotPosition();
                _eTxt.TextDimensions = Size.Empty;
                _eTxt.AreaControl = new Rectangle(0, 0, effectWidth, Height);

                if (hp == 0) {
                    OffsetX = Skin.PaddingSmal;
                } else if (hp > _eTxt.Count - 1) {
                    OffsetX = _eTxt.WidthControl > Width - (Skin.PaddingSmal * 2) ? Width - _eTxt.WidthControl - (Skin.PaddingSmal * 2) : Skin.PaddingSmal;
                } else if (hp > 0) {
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
        _eTxt.PropertyChanged += _eTxt_PropertyChanged;

        if (sliderVisible) {
            GenerateSlider();
            if (_sliderY != null) {
                _sliderY.Visible = true;
                _sliderY.Width = 18;
                _sliderY.Height = Height;
                _sliderY.Left = Width - _sliderY.Width;
                _sliderY.Top = 0;
                OffsetY = -(int)_sliderY.Value;
                _sliderY.Maximum = _eTxt.HeightControl + 16 - DisplayRectangle.Height;
            }
        } else {
            if (_sliderY != null) {
                _sliderY.Visible = false;
                _sliderY.Value = 0;
            }
            OffsetY = Skin.PaddingSmal;
        }

        Skin.Draw_Back(gr, Design, state, DisplayRectangle, this, true);
        _eTxt.Draw(gr, 1, OffsetX, OffsetY);
        MarkAndGenerateZone(gr, state);

        if (!string.IsNullOrEmpty(Suffix)) {
            var r = new Rectangle(_eTxt.WidthControl + OffsetX, OffsetY, 1000, 1000);
            if (_eTxt.Count > 0) {
                r.X += 2;
                Skin.Draw_FormatedText(gr, Suffix, null, Alignment.Top_Left, r, Design, States.Standard_Disabled, this, false, false);
            } else {
                Skin.Draw_FormatedText(gr, $"[in {Suffix}]", null, Alignment.Top_Left, r, Design, States.Standard_Disabled, this, false, true);
            }
        }

        Cursor_Show(gr);
        Skin.Draw_Border(gr, Design, state, DisplayRectangle);
        if (_mustCheck && !IsSpellChecking && Dictionary.DictionaryRunning(!DesignMode) && SpellChecker is { CancellationPending: false, IsBusy: false }) { SpellChecker.RunWorkerAsync(); }
    }

    protected override bool IsInputKey(System.Windows.Forms.Keys keyData) {
        // Ganz wichtig diese Routine!
        // Wenn diese NICHT ist, geht der Fokus weg, sobald der cursor gedrückt wird.
        // http://technet.microsoft.com/de-de/subscriptions/control.isinputkey%28v=vs.100%29

        if (keyData is System.Windows.Forms.Keys.Up
                    or System.Windows.Forms.Keys.Down
                    or System.Windows.Forms.Keys.Left
                    or System.Windows.Forms.Keys.Right) {
            return true;
        }
        return base.IsInputKey(keyData);
    }

    protected override void OnDoubleClick(System.EventArgs e) {
        if (IsDisposed) { return; }
        base.OnDoubleClick(e);
        Selection_WortMarkieren(_markStart);
        _lastUserActionForSpellChecking = DateTime.UtcNow;
        _doubleClicked = true;
        Invalidate();
    }

    protected override void OnEnabledChanged(System.EventArgs e) {
        if (IsDisposed) { return; }
        base.OnEnabledChanged(e);
        RaiseEventIfTextChanged(true);
        _markStart = -1;
        _markEnd = -1;
    }

    protected override void OnGotFocus(System.EventArgs e) {
        if (IsDisposed) { return; }
        base.OnGotFocus(e);
        if (!Enabled) { return; }

        if (!FloatingForm.IsShowing(this)) {
            _markStart = _eTxt.Count;
            _markEnd = -1;

            if (!_eTxt.Multiline && (!ContainsMouse() || !MousePressing)) { MarkAll(); }
            _lastUserActionForSpellChecking = DateTime.UtcNow.AddSeconds(-30);
        }
        _blinkerEnabled = true;
        _blinker?.Change(500, 500);
    }

    protected override void OnKeyDown(System.Windows.Forms.KeyEventArgs e) {
        if (IsDisposed) { return; }
        base.OnKeyDown(e);
        _blinkCount = 0;

        if (!Enabled || MousePressing) { return; }
        _lastUserActionForSpellChecking = DateTime.UtcNow;

        switch (e.KeyCode) {
            case System.Windows.Forms.Keys.Left:
                if (_markStart == 0 && _markEnd < 0) {
                    OnNavigateToNext(NavigationDirection.Previous);
                    e.Handled = true;
                    return;
                }
                Cursor_Richtung(-1, 0);
                break;

            case System.Windows.Forms.Keys.Right:
                if (_markStart >= _eTxt.Count && _markEnd < 0) {
                    OnNavigateToNext(NavigationDirection.Next);
                    e.Handled = true;
                    return;
                }
                Cursor_Richtung(1, 0);
                break;

            case System.Windows.Forms.Keys.Down:
                Cursor_Richtung(0, 1);
                break;

            case System.Windows.Forms.Keys.Up:
                Cursor_Richtung(0, -1);
                break;

            case System.Windows.Forms.Keys.Delete:
                ProcessKey(AsciiKey.DEL);
                break;
        }

        _cursorVisible = true;
        Invalidate();
    }

    protected override void OnKeyPress(System.Windows.Forms.KeyPressEventArgs e) {
        if (IsDisposed) { return; }
        _blinkCount = 0;
        base.OnKeyPress(e);
        _lastUserActionForSpellChecking = DateTime.UtcNow;
        if (!Enabled) {
            if (e.KeyChar != (int)AsciiKey.StrgX) { e.KeyChar = (char)AsciiKey.StrgC; }
            if (e.KeyChar != (int)AsciiKey.StrgC) { return; }
        }
        switch ((AsciiKey)e.KeyChar) {
            case AsciiKey.ENTER:
                ProcessKey((AsciiKey)e.KeyChar);
                OnEnterKey();
                e.Handled = true;
                return;

            case AsciiKey.ESC:
                OnEscKey();
                e.Handled = true;
                return;

            case AsciiKey.TAB:
                OnTabKey();
                OnNavigateToNext(ModifierKeys.HasFlag(System.Windows.Forms.Keys.Shift) ? NavigationDirection.Previous : NavigationDirection.Next);
                e.Handled = true;
                return;

            default:
                ProcessKey((AsciiKey)e.KeyChar);
                break;
        }
        e.Handled = true;
        Invalidate();
        if (IsDisposed) { return; }
        RaiseEventIfTextChanged(false);
    }

    protected override void OnLostFocus(System.EventArgs e) {
        if (IsDisposed) { return; }

        RaiseEventIfTextChanged(true);
        base.OnLostFocus(e);
        _lastUserActionForSpellChecking = DateTime.UtcNow.AddSeconds(-30);
        if (!FloatingForm.IsShowing(this)) { _markStart = -1; _markEnd = -1; }
        _blinkerEnabled = false;
        _blinker?.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
        _cursorVisible = false;
        Invalidate(); // Muss sein, weil evtl. der Cursor stehen bleibt
    }

    protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e) {
        if (IsDisposed) { return; }
        base.OnMouseDown(e);
        _lastUserActionForSpellChecking = DateTime.UtcNow;
        if (!Enabled || e.Button == System.Windows.Forms.MouseButtons.Right) { return; }
        _cursorVisible = true;
        _doubleClicked = false;
        _markStart = Cursor_PosAt(e.X, e.Y);
        _markEnd = -1;
        Selection_Repair(false);
        Invalidate();
    }

    protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e) {
        if (IsDisposed) { return; }
        base.OnMouseMove(e);
        if (e.Button != System.Windows.Forms.MouseButtons.Left || !Enabled) { return; }
        _lastUserActionForSpellChecking = DateTime.UtcNow;
        _markEnd = Cursor_PosAt(e.X, e.Y);
        Selection_Repair(false);
        Invalidate();
    }

    protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e) {
        if (IsDisposed) { return; }
        base.OnMouseUp(e);
        _lastUserActionForSpellChecking = DateTime.UtcNow;
        if (Enabled) {
            if (!_doubleClicked) {
                if (e.Button == System.Windows.Forms.MouseButtons.Right) {
                    var tags = new List<string>();

                    if (_markEnd < 0) {
                        var cp = Cursor_PosAt(e.X, e.Y);
                        var ws = _eTxt.WordStart(cp);
                        tags.TagSet("MarkStart", cp.ToString1());
                        tags.TagSet("MarkEnd", "-1");
                        tags.TagSet("Word", _eTxt.Word(ws));
                    } else {
                        tags.TagSet("MarkStart", _markStart.ToString1());
                        tags.TagSet("MarkEnd", _markEnd.ToString1());
                        tags.TagSet("Word", _eTxt.Word(_markStart));
                    }

                    ((IContextMenu)this).ContextMenuShow(tags);
                } else if (e.Button == System.Windows.Forms.MouseButtons.Left) {
                    _markEnd = Cursor_PosAt(e.X, e.Y);
                    Selection_Repair(true);
                }
            }
            _doubleClicked = false;
        } else {
            _markStart = -1;
            _markEnd = -1;
        }
        Invalidate();
    }

    protected override void OnMouseWheel(System.Windows.Forms.MouseEventArgs e) {
        if (IsDisposed) { return; }
        base.OnMouseWheel(e);
        if (_sliderY is not { Visible: true }) { return; }
        _lastUserActionForSpellChecking = DateTime.UtcNow;
        _sliderY.DoMouseWheel(e);
    }

    protected override void OnVisibleChanged(System.EventArgs e) {
        if (IsDisposed) { return; }
        RaiseEventIfTextChanged(true);
        base.OnVisibleChanged(e);
    }

    private void _eTxt_PropertyChanged(object? sender, PropertyChangedEventArgs e) => Invalidate();

    private void AbortSpellChecking() {
        if (SpellChecker.IsBusy) { SpellChecker.CancelAsync(); }
    }

    private void Blinker_Tick() {
        if (IsDisposed) { return; }
        if (_blinkCount < _raiseChangeDelay + 1 && _raiseChangeDelay > 0) {
            _blinkCount++;
            RaiseEventIfTextChanged(false);
        }

        if (!Focused || !Enabled) { return; }

        if (_markStart > -1 && _markEnd == -1) {
            _cursorVisible = !_cursorVisible;
            Invalidate();
        } else if (_cursorVisible) {
            _cursorVisible = false;
            Invalidate();
        }
    }

    private int Char_DelBereich(int von, int bis, bool raiseEvent) {
        _cursorVisible = true;
        if (von < 0 && bis <= 0) { return 0; }
        if (von < 0 || bis < 0 || von == bis) { return von; }

        var actualFirst = _eTxt.Delete(von, bis - 1);

        if (raiseEvent) {
            RaiseEventIfTextChanged(false);
            Invalidate();
        }

        return actualFirst;
    }

    private void Clipboard_Copy(int markStart, int markEnd) {
        if (markStart < 0 || markEnd < 0) { return; }
        Selection_Repair(true);

        var html = _eTxt.BuildHtmlText(markStart, markEnd - 1);

        var dataObject = new System.Windows.Forms.DataObject();
        dataObject.SetData(ExtCharFormat, html);
        dataObject.SetText(_eTxt.BuildPlainText(markStart, markEnd - 1));
        System.Windows.Forms.Clipboard.SetDataObject(dataObject, true);
    }

    private void Contextmenu_Bold(object? sender, ContextMenuEventArgs e) {
        var (markStart, markEnd, _) = GetContextData(e.HotItem);
        if (markStart < 0 || markEnd < 0) { return; }

        _markStart = markStart;
        _markEnd = markEnd;
        Selection_Repair(true);
        _eTxt.ChangeStructuralTag(_markStart, _markEnd - 1, "strong");
        Invalidate();
        RaiseEventIfTextChanged(false);
    }

    private void Contextmenu_Caption(object? sender, ContextMenuEventArgs e) {
        var (markStart, markEnd, _) = GetContextData(e.HotItem);
        if (markStart < 0 || markEnd < 0) { return; }

        Selection_Repair(true);
        _eTxt.ChangeStructuralTag(markStart, markEnd - 1, "h1");
        Invalidate();
        RaiseEventIfTextChanged(false);
    }

    private void Contextmenu_Copy(object? sender, ContextMenuEventArgs e) {
        var (markStart, markEnd, _) = GetContextData(e.HotItem);
        Clipboard_Copy(markStart, markEnd);
    }

    private void Contextmenu_Cut(object? sender, ContextMenuEventArgs e) {
        var (markStart, markEnd, _) = GetContextData(e.HotItem);
        Clipboard_Copy(markStart, markEnd);
        if (!Enabled) { return; }

        _markStart = Char_DelBereich(markStart, markEnd, true);
        _markEnd = -1;
    }

    private void Contextmenu_NoCaption(object? sender, ContextMenuEventArgs e) {
        var (markStart, markEnd, _) = GetContextData(e.HotItem);
        if (markStart < 0 || markEnd < 0) { return; }

        _markStart = markStart;
        _markEnd = markEnd;
        Selection_Repair(true);
        _eTxt.ChangeStructuralTag(_markStart, _markEnd - 1, null);
        Invalidate();
        RaiseEventIfTextChanged(false);
    }

    private void Contextmenu_Paste(object? sender, ContextMenuEventArgs e) {
        var (markStart, markEnd, _) = GetContextData(e.HotItem);
        _markStart = Char_DelBereich(markStart, markEnd, false);
        _markEnd = -1;
        _markStart = InsertClipboard(_markStart, false);
    }

    private void Contextmenu_Paste_Link(object? sender, ContextMenuEventArgs e) {
        var (markStart, markEnd, _) = GetContextData(e.HotItem);
        _markStart = Char_DelBereich(markStart, markEnd, false);
        _markEnd = -1;
        _markStart = InsertClipboard(_markStart, true);
    }

    private void Contextmenu_Sonderzeichen(object? sender, ContextMenuEventArgs e) {
        var (markStart, markEnd, _) = GetContextData(e.HotItem);
        List<AbstractListItem> i = [
            ItemOf("Kugel", "sphere", QuickImage.Get(ImageCode.Kugel, 20)),
            ItemOf("Warnung", "Warnung", QuickImage.Get(ImageCode.Warnung, 20)),
            ItemOf("Information", "Information", QuickImage.Get(ImageCode.Information, 20)),
            ItemOf("Kritisch", "Kritisch", QuickImage.Get(ImageCode.Kritisch, 20)),
            ItemOf("Frage", "Frage", QuickImage.Get(ImageCode.Frage, 20))
        ];

        var r = InputBoxListBoxStyle.Show("Wählen sie:", i, CheckBehavior.SingleSelection, null, AddType.None);
        if (r is not { Count: 1 }) { return; }

        _markStart = Char_DelBereich(markStart, markEnd, false);
        _markStart = Insert(_markStart, new ExtCharImageCode(_eTxt, _markStart, QuickImage.Get(r[0].KeyName)), true);
    }

    /// <summary>
    /// Sucht den aktuellen Buchstaben, der unter den angegeben Koordinaten liegt.
    /// Wird auf die hintere Hälfte eines Zeichens gewählt, wird der nächste Buchstabe angegeben.
    /// </summary>
    /// <remarks></remarks>
    private int Cursor_PosAt(int controlX, int controlY) {
        if (controlX < OffsetX && controlY < OffsetY) { return 0; }

        controlX = Math.Clamp(controlX, OffsetX, OffsetX + _eTxt.WidthControl);
        controlY = Math.Clamp(controlY, OffsetY, OffsetY + _eTxt.HeightControl);

        var canvasY = controlY - OffsetY;
        var c = Math.Max(0, _eTxt.Char_Search(controlX - OffsetX, canvasY));

        if (c < _eTxt.Count && canvasY > _eTxt[c].PosCanvas.Y + _eTxt[c].SizeCanvas.Height) {
            while (c < _eTxt.Count && _eTxt[c].PosCanvas.Y + _eTxt[c].SizeCanvas.Height <= canvasY)
                c++;
            return c;
        }

        var pos = c < _eTxt.Count && controlX > OffsetX + _eTxt[c].PosCanvas.X + (_eTxt[c].SizeCanvas.Width / 2.0) ? c + 1 : c;
        if (_eTxt.IsInsideLink(pos)) {
            var (clStart, clEnd) = _eTxt.GetCellLinkBounds(pos);
            if (clStart >= 0) { pos = (pos - clStart) <= (clEnd + 1 - pos) ? clStart : clEnd + 1; }
        }
        return pos;
    }

    private void Cursor_Richtung(short x, short y) {
        if (x != 0) {
            if (_markStart > -1 && _markEnd > -1) {
                _markStart = x == -1 ? Math.Min(_markEnd, _markStart) : Math.Max(_markEnd, _markStart) + 1;
            } else if (_markStart > -1) {
                if (x == -1 && _markStart > 0)
                    _markStart--;
                if (x == 1 && _markStart < _eTxt.Count)
                    _markStart++;
            } else {
                _markStart = 0;
            }
            while (x > 0 && _markStart < _eTxt.Count && _eTxt[_markStart].SizeCanvas.Width <= 0 && !_eTxt[_markStart].IsLineBreak())
                _markStart++;
            while (x < 0 && _markStart > 0 && _eTxt[_markStart].SizeCanvas.Width <= 0 && !_eTxt[_markStart].IsLineBreak())
                _markStart--;

            if (_eTxt.IsInsideLink(_markStart)) {
                var (clStart, clEnd) = _eTxt.GetCellLinkBounds(_markStart);
                if (clStart >= 0) {
                    _markStart = x > 0 ? clEnd + 1 : clStart;
                }
            }
        }

        if (y != 0) {
            var ri = _eTxt.CursorCanvasPosX(Math.Max(0, _markStart));
            if (y > 0) {
                _markStart = _markStart >= _eTxt.Count ? _eTxt.Count : Cursor_PosAt(ri.Left + OffsetX, (int)(ri.Top + (ri.Height / 2f) + _eTxt[_markStart].SizeCanvas.Height + OffsetY));
            } else if (y < 0) {
                _markStart = _markStart >= _eTxt.Count
                    ? (_eTxt.Count > 0 ? Cursor_PosAt(ri.Left + OffsetX, (int)(ri.Top + (ri.Height / 2f) - _eTxt[_markStart - 1].SizeCanvas.Height + OffsetY)) : 0)
                    : Cursor_PosAt(ri.Left + OffsetX, (int)(ri.Top + (ri.Height / 2f) - _eTxt[_markStart].SizeCanvas.Height + OffsetY));
            }
        }

        _markStart = Math.Max(0, _markStart);
        _markEnd = -1;
        _cursorVisible = true;
    }

    private void Cursor_Show(Graphics gr) {
        if (!_cursorVisible || _markStart < 0 || _markEnd > -1) { return; }
        var r = _eTxt.CursorCanvasPosX(_markStart);
        using var pen = new Pen(Color.Black);
        gr.DrawLine(pen, r.Left + OffsetX, r.Top + OffsetY, r.Left + OffsetX, r.Bottom + OffsetY);
    }

    /// <summary>
    /// Wenn das Format, die Maxlänge oder Multiline sich geändert haben,
    /// wird für das dementsprechende Format die Verbote/Erlaubnisse gesetzt.
    /// z.B. wird beim Format Datum die Maxlänge auf 10 gesetzt und nur noch Ziffern und Punkt erlaubt.
    /// </summary>
    /// <remarks></remarks>
    private void GenerateEtxt(bool resetCoords) {
        if (IsDisposed) { return; }
        if (InvokeRequired) {
            Invoke(new Action(() => GenerateEtxt(resetCoords)));
            return;
        }

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
        if (IsDisposed || _sliderY != null) { return; }

        _sliderY = new Slider {
            Dock = System.Windows.Forms.DockStyle.Right,
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

    private int HotPosition() => _markStart > -1 ? (_markEnd < 0 ? _markStart : _markEnd) : -1;

    private int Insert(int pos, char c, bool raiseEvent) => c < 13 ? pos : Insert(pos, new ExtCharAscii(_eTxt, pos, c), raiseEvent);

    private int Insert(int pos, ExtChar chr, bool raiseEvent) {
        if (_eTxt.Insert(pos, chr)) {
            if (raiseEvent)
                RaiseEventIfTextChanged(false);
            return pos + 1;
        }
        if (raiseEvent)
            RaiseEventIfTextChanged(false);
        return pos;
    }

    private int InsertClipboard(int pos, bool linkAllowed) {
        if (TextFormatingAllowed) {
            if (linkAllowed && System.Windows.Forms.Clipboard.ContainsData(TableView.CellDataFormat)) {
                if (System.Windows.Forms.Clipboard.GetData(TableView.CellDataFormat) is string sd && !string.IsNullOrEmpty(sd)) {
                    var t = sd.SplitByCr();
                    var start = new ExtCharCellLinkStart(_eTxt, pos, t[0], t[1], t[2]);
                    var linkStartIdx = pos;
                    pos = Insert(pos, start, true);
                    if (!string.IsNullOrEmpty(start.DisplayText)) {
                        foreach (var c in start.DisplayText)
                            pos = Insert(pos, new ExtCharAscii(_eTxt, pos, c), true);
                    }
                    pos = Insert(pos, new ExtCharCellLinkEnd(_eTxt, pos), true);
                    _eTxt.Mark(MarkState.CellLink, linkStartIdx + 1, pos - 2);
                    return pos;
                }
            }

            if (System.Windows.Forms.Clipboard.ContainsData(ExtCharFormat)) {
                if (System.Windows.Forms.Clipboard.GetData(ExtCharFormat) is string sd && !string.IsNullOrEmpty(sd)) {
                    var parsedChars = _eTxt.ParseHtmlToChars(sd);
                    foreach (var extChar in parsedChars) {
                        if (_eTxt.Count < MaxTextLength)
                            pos = Insert(pos, extChar, false);
                    }
                    RaiseEventIfTextChanged(false);
                    return pos;
                }
            }
        }

        if (System.Windows.Forms.Clipboard.ContainsText()) {
            return Insert(pos, System.Windows.Forms.Clipboard.GetText());
        }

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
        var overrideFont = Skin.GetBlueFont(Design, sr);

        for (var cc = mas; cc < mae; cc++) {
            var controlPos = _eTxt[cc].PosCanvas.CanvasToControl(1f, OffsetX, OffsetY);
            var controlSize = _eTxt[cc].SizeCanvas.CanvasToControl(1f);

            if (ExtChar.IsVisible(_eTxt.AreaControl, controlPos, controlSize)) {
                _eTxt[cc].DrawWithFont(gr, controlPos, controlSize, 1f, overrideFont);
            }
        }
    }

    private void OnEnterKey() => EnterKey?.Invoke(this, System.EventArgs.Empty);

    private void OnEscKey() => EscKey?.Invoke(this, System.EventArgs.Empty);

    private void OnNavigateToNext(NavigationDirection direction) => NavigateToNext?.Invoke(this, new NavigationDirectionEventArgs(direction));

    private void OnTabKey() => TabKey?.Invoke(this, System.EventArgs.Empty);

    private void RaiseEventIfTextChanged(bool doChangeNow) {
        if (IsDisposed) { return; }

        var newtext = Text;
        if (newtext == _lastCheckedText) {
            _blinkCount = 0;
            return;
        }

        if (doChangeNow || !_blinkerEnabled || _blinkCount >= _raiseChangeDelay) {
            _lastCheckedText = newtext;
            _blinkCount = 0;
            OnTextChanged(System.EventArgs.Empty);
        }
    }

    private void Selection_Repair(bool swapThem) {
        if (_markStart < 0 && _markEnd < 0) { return; }

        _markStart = Math.Clamp(_markStart, 0, _eTxt.Count);
        _markEnd = Math.Min(_markEnd, _eTxt.Count);

        if (_markStart == _markEnd) { _markEnd = -1; }

        if (swapThem && _markStart > _markEnd && _markEnd > -1) {
            Generic.Swap(ref _markStart, ref _markEnd);
        }

        if (_markEnd >= 0 && _markStart >= 0) {
            var mas = Math.Min(_markStart, _markEnd);
            var mae = Math.Max(_markStart, _markEnd);

            var (s1, _) = _eTxt.GetCellLinkBounds(mas);
            if (s1 >= 0) { mas = s1; }

            var (_, e2) = _eTxt.GetCellLinkBounds(mae);
            if (e2 >= 0) { mae = e2 + 1; }

            if (_markStart <= _markEnd) {
                _markStart = mas;
                _markEnd = mae;
            } else {
                _markStart = mae;
                _markEnd = mas;
            }
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

    private void SliderY_ValueChange(object? sender, System.EventArgs e) => Invalidate();

    private void SpellChecker_DoWork(object? sender, DoWorkEventArgs e) {
        try {
            if (IsSpellChecking || (DateTime.UtcNow - _lastUserActionForSpellChecking).TotalSeconds < 2) { return; }

            IsSpellChecking = true;
            if (!SpellCheckingEnabled || !Dictionary.DictionaryRunning(!DesignMode)) { return; }

            var pos = 0;
            var woEnd = -1;
            bool ok;
            do {
                ok = true;
                SpellChecker.ReportProgress(0, "Unmark");
                try {
                    while (true) {
                        if (SpellChecker.CancellationPending) { return; }
                        pos = Math.Max(woEnd + 1, pos + 1);
                        if (pos >= _eTxt.Count)
                            break;

                        var woStart = _eTxt.WordStart(pos);
                        if (woStart > -1) {
                            woEnd = _eTxt.WordEnd(pos);
                            var wort = _eTxt.Word(pos);
                            if (!Dictionary.IsWordOk(wort, CustomVocabulary)) {
                                if (SpellChecker.CancellationPending) { return; }
                                SpellChecker.ReportProgress((int)(woEnd / (double)_eTxt.Count * 100), $"Mark;{woStart};{woEnd - 1}");
                            }
                        }
                    }
                } catch { ok = false; }
            } while (!ok);
        } catch { /* Error handling */ }
        SpellChecker.ReportProgress(100, "Done");
    }

    private void SpellChecker_ProgressChanged(object? sender, ProgressChangedEventArgs e) {
        if (SpellChecker.CancellationPending || e.UserState is not string us) { return; }

        var x = us.SplitAndCutBy(";");
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
        }
    }

    private void SpellChecker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e) => IsSpellChecking = false;

    #endregion
}
// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueBasics.Formats;
using BlueControls.Controls.ConnectedFormula;
using BlueControls.Designer_Support;
using BlueControls.EventArgs;
using BlueScript.ScriptVariables;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
public sealed partial class EasyText : GenericControlReciver, IHasFieldVariable, IAutoNext {

    #region Fields

    private readonly System.Threading.Timer? _panelMover;

    private int _panelMoveDirection;

    #endregion

    #region Constructors

    public EasyText() : base(true, false, false) {
        InitializeComponent();
        txbText.NavigateToNext += TxbText_NavigateToNext;
        txbText.MouseEnter += TxbText_MouseEnter;
        txbText.MouseLeave += TxbText_MouseLeave;

        // Die Leiste muss über dem Textfeld liegen, das die gesamte Fläche überdeckt.
        EditPanelFrame.BringToFront();

        _panelMover = new System.Threading.Timer(_ => {
            if (IsHandleCreated) { BeginInvoke(new Action(PanelMover_Tick)); }
        }, null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
    }

    #endregion

    #region Properties

    /// <summary>
    /// Wenn gewählt, springt der Cursor zum nächsten Eingabefeld, wenn am Ende des Textes die Nach-rechts-Taste gedrückt wird.
    /// </summary>
    [DefaultValue(false)]
    public bool AutoNext { get; init; }

    /// <summary>
    /// Wenn gewählt, blendet sich bei Mausberührung eine Knopfleiste mit Kopieren und Einfügen ein.
    /// </summary>
    [DefaultValue(true)]
    public bool Buttonleiste {
        get;
        set {
            if (value == field) { return; }
            field = value;
            if (!field && EditPanelFrame.Visible) {
                EditPanelFrame.Visible = false;
                _panelMoveDirection = 0;
            }
        }
    } = true;

    /// <summary>
    /// Der Text, den das Feld beim Erzeugen enthält.
    /// </summary>
    [DefaultValue("")]
    public string DefaultValue {
        get;
        set {
            if (IsDisposed || field == value) { return; }
            field = value;
            txbText.Text = value;
        }
    } = string.Empty;

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string FieldName => GeneratedFrom is EasyTextPadItem { IsDisposed: false } e ? e.FieldName : string.Empty;

    /// <summary>
    /// Wenn gewählt, kann der Benutzer den Text nicht ändern.
    /// </summary>
    [DefaultValue(false)]
    public bool ReadOnly {
        get;
        set {
            if (value == field) { return; }
            field = value;
            txbText.Enabled = !field;
            CheckButtons();
        }
    }

    [DefaultValue(0)]
    public new int TabIndex {
        get => 0;

        set => base.TabIndex = 0;
    }

    [DefaultValue(false)]
    public new bool TabStop {
        get => false;

        set => base.TabStop = false;
    }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    internal string FormatKey {
        get;
        set {
            if (value.Equals(field, StringComparison.OrdinalIgnoreCase)) { return; }
            field = value;

            if (Format.AllFormats.Instances.FirstOrDefault(thisF => thisF.KeyName.Equals(field, StringComparison.OrdinalIgnoreCase)) is not { } f) { return; }

            txbText.AllowedChars = f.AllowedChars;
            txbText.ForbiddenChars = f.ForbiddenChars;
            txbText.MaxTextLength = f.MaxTextLength;
            txbText.MinTextLength = f.MinTextLength;
            txbText.MultiLine = f.MultiLine;
            txbText.RegexCheck = f.RegexCheck;
            txbText.AdditionalFormatCheck = f.AdditionalFormatCheck;
        }
    } = string.Empty;

    /// <summary>
    /// True, wenn sich der Zeiger über dem Control oder einem seiner Kinder befindet.
    /// Die Standard-ContainsMouse-Eigenschaft funktioniert hier nicht, da das Textfeld
    /// als Kind-Control die gesamte Fläche überdeckt und die Mausereignisse an sich reißt.
    /// </summary>
    private bool MausÜberDemControl => IsHandleCreated && ClientRectangle.Contains(PointToClient(MousePosition));

    #endregion

    #region Methods

    public ScriptVariable? GetFieldVariable() {
        var fn = FieldName;

        if (!string.IsNullOrEmpty(fn)) {
            return new StringScriptVariable(fn, txbText.Text, false, "Wert des Formular-Feldes");
        }

        return null;
    }

    public void SetValueFromVariable(ScriptVariable v) => txbText.Text = v.ValueForCell;

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);

        if (disposing) { _panelMover?.Dispose(); }
    }

    protected override void DrawControl(Graphics gr, States state) {
        if (IsDisposed) { return; }
        base.DrawControl(gr, state);

        if (state.HasFlag(States.Standard_MouseOver)) { state ^= States.Standard_MouseOver; }
        if (state.HasFlag(States.Standard_MousePressed)) { state ^= States.Standard_MousePressed; }

        Skin.Draw_Back(gr, Design.TextBox, state, DisplayRectangle, this, true);
        Skin.Draw_Border(gr, Design.TextBox, state, DisplayRectangle);
    }

    protected override void FocusInput() => txbText.Focus();

    protected override void HandleChangesNow() {
        base.HandleChangesNow();
        if (IsDisposed) { return; }
        if (RowsInputChangedHandled && FilterInputChangedHandled) { return; }

        DoInputFilter(null, false);
        RowsInputChangedHandled = true;
    }

    protected override void OnEnabledChanged(System.EventArgs e) {
        base.OnEnabledChanged(e);
        if (!Enabled) {
            EditPanelFrame.Visible = false;
            _panelMover?.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            _panelMoveDirection = 0;
        }
    }

    protected override void OnHandleCreated(System.EventArgs e) {
        base.OnHandleCreated(e);
        CheckButtons();
    }

    protected override void OnMouseEnter(System.EventArgs e) {
        base.OnMouseEnter(e);
        PanelSlideIn();
    }

    protected override void OnMouseLeave(System.EventArgs e) {
        base.OnMouseLeave(e);
        _panelMover?.Change(5, 5);
    }

    private void BtnCopy_Click(object sender, System.EventArgs e) {
        if (CopytoClipboard(txbText.Text)) {
            QuickNote.Show(NoteSymbols.Ok, "Kopiert");
        } else {
            QuickNote.Show(NoteSymbols.Critical, "Fehlgeschlagen");
        }
    }

    private void BtnPaste_Click(object sender, System.EventArgs e) {
        if (System.Windows.Forms.Clipboard.ContainsText()) { txbText.Text = System.Windows.Forms.Clipboard.GetText(); }
    }

    private void CheckButtons() {
        btnCopy.Enabled = txbText is { Text: { Length: > 0 } };
        btnPaste.Enabled = txbText.Enabled && System.Windows.Forms.Clipboard.ContainsText();
    }

    private void PanelMover_Tick() {
        if (Ending || IsDisposed || Disposing) { return; }

        if (_panelMoveDirection == 0) {
            if (!EditPanelFrame.Visible) {
                _panelMover?.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                return;
            }
        }
        if (_panelMoveDirection >= 0) {
            if (!Buttonleiste || !MausÜberDemControl) { _panelMoveDirection = -1; }
        }
        if (_panelMoveDirection > 0) {
            if (!EditPanelFrame.Visible) {
                EditPanelFrame.Top = -EditPanelFrame.Height;
                EditPanelFrame.Visible = true;
                SetTextbox();
                return;
            }
            if (EditPanelFrame.Top >= 0) {
                EditPanelFrame.Top = 0;
                _panelMoveDirection = 0;
                SetTextbox();
                return;
            }
            EditPanelFrame.Top += 4;
            SetTextbox();
            return;
        }
        if (_panelMoveDirection < 0) {
            if (EditPanelFrame.Top < -EditPanelFrame.Height) {
                EditPanelFrame.Visible = false;
                _panelMoveDirection = 0;
                SetTextbox();
                return;
            }
            EditPanelFrame.Top -= 4;
            SetTextbox();
        }
    }

    /// <summary>
    /// Startet das Einblenden der Knopfleiste.
    /// Die Ereignisse kommen vom Textfeld, da dieses die gesamte Fläche überdeckt.
    /// </summary>
    private void PanelSlideIn() {
        if (!Buttonleiste) { return; }
        _panelMoveDirection = 1;
        _panelMover?.Change(5, 5);
    }

    private void SetTextbox() {
        txbText.SuspendLayout();
        txbText.Top = Math.Max(EditPanelFrame.Bottom, 0);
        txbText.Height = Height - txbText.Top;
        txbText.ResumeLayout();
    }

    private void TxbText_MouseEnter(object? sender, System.EventArgs e) => PanelSlideIn();

    private void TxbText_MouseLeave(object? sender, System.EventArgs e) => _panelMover?.Change(5, 5);

    private void TxbText_NavigateToNext(object? sender, NavigationDirectionEventArgs e) {
        if (AutoNext) { NextControl(e.Direction); }
    }

    private void TxbText_TextChanged(object sender, System.EventArgs e) => CheckButtons();

    #endregion
}
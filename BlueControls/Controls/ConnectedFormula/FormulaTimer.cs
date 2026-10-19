// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Threading;

namespace BlueControls.Controls;

internal partial class FormulaTimer : GenericControl, IBackgroundNone //System.Windows.Forms.UserControl  /// Usercontrol
{
    #region Fields

    private readonly Timer? _main;
    private int _last;
    private string _value0 = string.Empty;
    private string _value1 = string.Empty;
    private string _value2 = string.Empty;

    #endregion

    #region Constructors

    public FormulaTimer() : base(false, false, false) {
        InitializeComponent();
        chkAktiv.CheckedChanged += ChkAktiv_CheckedChanged;
        _last = -1;
        _main = new Timer(TickCallback, null, 1000, 1000);
    }

    #endregion

    #region Properties

    public ConnectedFormulaView? ConnectedFormula { get; internal init; }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public virtual string Mode { get; set; } = string.Empty;

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Script { get; internal set; } = string.Empty;

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int Seconds { get; init; }

    internal bool Deaktivierbar { get; init; }

    internal bool IsActive {
        get => !Deaktivierbar || chkAktiv.Checked;
        init => chkAktiv.Checked = value;
    }

    internal string ItemText { get; init; } = string.Empty;

    internal int MinIdleSekunden { get; init; }

    #endregion

    #region Methods

    protected override void Dispose(bool disposing) {
        _main?.Dispose(); // Timer zuerst stoppen, bevor das Handle freigegeben wird
        base.Dispose(disposing);
        Script = string.Empty;
    }

    protected override void DrawControl(Graphics gr, States state) {
        if (IsDisposed) { return; }
        base.DrawControl(gr, state);
        Skin.Draw_Back_Transparent(gr, ClientRectangle, this);//Intiall();
    }

    protected override void OnHandleCreated(System.EventArgs e) {
        base.OnHandleCreated(e);
        chkAktiv.Visible = Deaktivierbar;
        chkAktiv.Text = ItemText;
    }

    /// <summary>
    /// Setzt den Prüfzyklus zurück, wenn der Timer manuell wieder aktiviert wurde.
    /// </summary>
    private void ChkAktiv_CheckedChanged(object? sender, System.EventArgs e) {
        if (!chkAktiv.Checked) { return; }
        _last = 0;
        capMessage.Text = "Timer aktiv.";
    }

    private void Main_Tick() {
        if (Ending || IsDisposed || Disposing || !IsHandleCreated) { return; }

        if (!IsActive) { return; }

        _last++;
        if (_last < Seconds) { return; }

        if (MinIdleSekunden > 0 && Develop.GetUserIdleSeconds() < MinIdleSekunden) {
            capMessage.Text = "Warte auf Inaktivität...";
            return;
        }

        capAuslösezeit.Text = DateTime.Now.ToString5();

        if (ConnectedFormula?.GetConnectedFormula()?.IsEditing() == true) {
            capMessage.Text = "Editor geöffnet.";
            return;
        }

        var fieldSources = IHasFieldVariableExtension.GetFieldSources(this);

        var t = TimerPadItem.ExecuteScript(Script, Mode, _value0, _value1, _value2, true, fieldSources, null);

        if (t.Failed) {
            _last = -60; // Nach dem Intervall zzgl. 60 Sekunden erneut versuchen
            capMessage.Text = "Skript-Fehler: " + t.FailedReason;
            return;
        }

        capMessage.Text = t.Variables?.GetString("Feedback") ?? "-";

        _value0 = t.Variables?.GetString("value0") ?? string.Empty;
        _value1 = t.Variables?.GetString("value1") ?? string.Empty;
        _value2 = t.Variables?.GetString("value2") ?? string.Empty;
        _last = 0;

        IHasFieldVariableExtension.WriteBackFieldSources(fieldSources, t.Variables);
    }

    private void TickCallback(object? state) {
        if (!IsHandleCreated || IsDisposed || Disposing) { return; }
        try {
            BeginInvoke(new Action(Main_Tick));
        } catch {
            // Handle wurde zwischen Prüfung und Aufruf zerstört — der nächste Tick erneut selbstheilend
        }
    }

    #endregion
}
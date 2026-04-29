// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes;
using BlueControls.Classes.ItemCollectionPad.FunktionsItems_Formular;
using System.Threading;

namespace BlueControls.Controls;

internal partial class FormulaTimer : GenericControl, IBackgroundNone //System.Windows.Forms.UserControl  /// Usercontrol
{
    #region Fields

    private int _last;
    private Timer? _main;
    private string _value0 = string.Empty;
    private string _value1 = string.Empty;
    private string _value2 = string.Empty;
    private bool _wasok = true;

    #endregion

    #region Constructors

    public FormulaTimer() : base(false, false, false) {
        InitializeComponent();
        _last = -1;
        _main = new Timer(_ => {
            if (IsHandleCreated) { BeginInvoke(new Action(Main_Tick)); }
        }, null, 1000, 1000);
    }

    #endregion

    #region Properties

    public ConnectedFormulaView? ConnectedFormula { get; internal set; }

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
    public int Seconds { get; set; }

    internal bool Deaktivierbar {
        get;
        set {
            chkAktiv.Visible = value;
        }
    } = false;

    internal bool IsActive {
        get => !Deaktivierbar || chkAktiv.Checked;
        set {
            if (!Deaktivierbar) { value = true; }
            chkAktiv.Checked = value;
        }
    }

    internal string ItemText {
        get;
        set {
            chkAktiv.Text = value;
        }
    } = string.Empty;

    #endregion

    #region Methods

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);

        _main?.Change(Timeout.Infinite, Timeout.Infinite);
        Script = string.Empty;
    }

    protected override void DrawControl(Graphics gr, States state) {
        if (IsDisposed) { return; }
        base.DrawControl(gr, state);
        Skin.Draw_Back_Transparent(gr, ClientRectangle, this);//Intiall();
    }

    private void Main_Tick() {
        if (!_wasok || !IsActive) { return; }

        _last++;
        if (_last < Seconds) { return; }

        capAuslösezeit.Text = DateTime.Now.ToString5();

        if (ConnectedFormula?.GetConnectedFormula()?.IsEditing() ?? true) {
            capMessage.Text = "Editor geöffnet.";
            return;
        }

        var t = TimerPadItem.ExecuteScript(Script, Mode, _value0, _value1, _value2);

        if (t.Failed) {
            _wasok = false;
            capMessage.Text = "Skript fehlerhaft: " + t.FailedReason;
            return;
        }

        capMessage.Text = t.Variables?.GetString("Feedback") ?? "-";

        _value0 = t.Variables?.GetString("value0") ?? string.Empty;
        _value1 = t.Variables?.GetString("value1") ?? string.Empty;
        _value2 = t.Variables?.GetString("value2") ?? string.Empty;
        _last = 0;
    }

    #endregion
}
// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes;
using System.Windows.Forms;

namespace BlueControls.Forms;

public partial class QuickInfo : FloatingForm {

    #region Fields

    private static IntPtr _activeFormHandle;
    private static string _autoClosedTxt = string.Empty;

    private static QuickInfo? _instance;
    private static string _shownTxt = string.Empty;
    private int _counter;

    private bool _shown;

    private System.Threading.Timer? _timQI;

    private int _timQIInterval = 500;

    #endregion

    #region Constructors

    private QuickInfo() : base(Design.Form_QuickInfo) {
        InitializeComponent();
        DismissMode = DismissMode.ManualOnly;
    }

    private QuickInfo(string text) : this() {
        //InitializeComponent();
        capText.Text = text;
        capText.FitSize();
        capText.Location = new Point(Skin.PaddingMedium, Skin.PaddingMedium);
        var wi = Math.Min((int)(Screen.PrimaryScreen.Bounds.Size.Height * 0.7), capText.Right + Skin.PaddingMedium);
        var he = Math.Min((int)(Screen.PrimaryScreen.Bounds.Size.Height * 0.7), capText.Bottom + Skin.PaddingMedium);
        Size = new Size(wi, he);
        Visible = false;
        CreateHandle();
        _timQI = new System.Threading.Timer(_ => {
            if (IsHandleCreated) { BeginInvoke(new Action(TimQI_Tick)); }
        }, null, 500, 500);
    }

    #endregion

    #region Methods

    public new static void Close() => Close(false);

    public static void Show(string text) {
        if (text == _shownTxt) { return; }
        Close(false);
        if (text == _autoClosedTxt) { return; }
        _shownTxt = text;
        _activeFormHandle = Form.ActiveForm?.Handle ?? IntPtr.Zero;
        if (string.IsNullOrEmpty(text)) { return; }
        _instance = new QuickInfo(text);
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing) {
        if (disposing) { components?.Dispose(); }
        base.Dispose(disposing);
    }

    private static void Close(bool autoClose) {
        if (autoClose) {
            _autoClosedTxt = _shownTxt;
        } else {
            _shownTxt = string.Empty;
            _autoClosedTxt = string.Empty;
        }
        _activeFormHandle = IntPtr.Zero;
        if (_instance is { IsDisposed: false }) {
            try {
                _instance._timQI?.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                ((FloatingForm)_instance).Close();
            } catch (Exception ex) {
                Develop.DebugPrint("Fehler beim Schließen des QuickInfos", ex);
            }
        }
    }

    private void TimQI_Tick() {
        if (Generic.Ending || IsDisposed || Disposing) { return; }
        var currentHandle = Form.ActiveForm?.Handle ?? IntPtr.Zero;
        if (currentHandle != _activeFormHandle) {
            QuickInfo.Close(true);
            return;
        }
        Position_LocateToMouse();
        if (!_shown) {
            _shown = true;
            Show();
            _timQIInterval = 15;
            _timQI?.Change(15, 15);
        }
        _counter++;
        if (_counter * _timQIInterval > 10000) {
            _timQI?.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            QuickInfo.Close(true);
        }
    }

    #endregion
}
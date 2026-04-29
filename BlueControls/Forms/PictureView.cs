// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.Forms;

public partial class PictureView : FormWithStatusBar, IDisposableExtended {

    #region Fields

    private readonly List<string> _fileList = [];
    private int _nr = -1;

    #endregion

    #region Constructors

    public PictureView() : this(null, false, string.Empty, -1, -1) { }

    public PictureView(List<string>? fileList, bool mitScreenResize, string windowCaption, int imageno) : this(fileList, mitScreenResize, windowCaption, -1, imageno) { }

    public PictureView(Bitmap? bmp) : this(null, false, string.Empty, -1, -1) {
        Pad.Bmp = bmp;
        Pad.ZoomFit();
        btnZoomIn.Checked = true;
        btnChoose.Enabled = false;
    }

    public PictureView(List<string>? fileList, bool mitScreenResize, string windowCaption, int openOnScreen, int imageno) : base() {
        InitializeComponent();

        if (mitScreenResize) {
            if (System.Windows.Forms.Screen.AllScreens.Length == 1 || openOnScreen < 0) {
                var opScNr = Generic.PointOnScreenNr(System.Windows.Forms.Cursor.Position);
                Width = (int)(System.Windows.Forms.Screen.AllScreens[opScNr].WorkingArea.Width / 1.5);
                Height = (int)(System.Windows.Forms.Screen.AllScreens[opScNr].WorkingArea.Height / 1.5);
                Left = (int)(System.Windows.Forms.Screen.AllScreens[opScNr].WorkingArea.Left + ((System.Windows.Forms.Screen.AllScreens[opScNr].WorkingArea.Width - Width) / 2.0));
                Top = (int)(System.Windows.Forms.Screen.AllScreens[opScNr].WorkingArea.Top + ((System.Windows.Forms.Screen.AllScreens[opScNr].WorkingArea.Height - Height) / 2.0));
            } else {
                Width = System.Windows.Forms.Screen.AllScreens[openOnScreen].WorkingArea.Width;
                Height = System.Windows.Forms.Screen.AllScreens[openOnScreen].WorkingArea.Height;
                Left = System.Windows.Forms.Screen.AllScreens[openOnScreen].WorkingArea.Left;
                Top = System.Windows.Forms.Screen.AllScreens[openOnScreen].WorkingArea.Top;
            }
        }

        if (!string.IsNullOrEmpty(windowCaption)) { Text = windowCaption; }

        btnZoomIn.Checked = true;
        btnChoose.Enabled = false;

        SetFiles(fileList, imageno);
        LoadPic(imageno);
    }

    #endregion

    #region Properties

    public override sealed string Text {
        get => base.Text;
        set => base.Text = value;
    }

    #endregion

    #region Methods

    public void SetFiles(List<string>? fileList, int imageno) {
        _fileList.Clear();
        if (fileList != null) { _fileList.AddRange(fileList); }
        LoadPic(imageno);
    }

    protected void LoadPic(int nr) {
        Pad.Items?.Clear();

        _nr = nr;
        if (nr < _fileList.Count && nr > -1) {
            try {
                Pad.Bmp = Image_FromFile(_fileList[nr]) as Bitmap;
            } catch (Exception ex) {
                Pad.Bmp = null;
                Develop.DebugPrint("Fehler beim Laden des Bildes", ex);
            }
        } else {
            Pad.Bmp = null;
        }

        if (_fileList.Count < 2) {
            grpSeiten.Visible = false;
            grpSeiten.Enabled = false;
            btnZurueck.Enabled = false;
            btnVor.Enabled = false;
        } else {
            grpSeiten.Visible = true;
            grpSeiten.Enabled = true;
            btnZurueck.Enabled = _nr > 0;
            btnVor.Enabled = _nr < _fileList.Count - 1;
        }

        Pad.ZoomFit();
    }

    private void btnTopMost_CheckedChanged(object sender, System.EventArgs e) => TopMost = btnTopMost.Checked;

    private void btnVor_Click(object sender, System.EventArgs e) {
        if (_fileList.Count < 2) { return; }
        _nr++;
        if (_nr >= _fileList.Count) { _nr = _fileList.Count - 1; }
        LoadPic(_nr);
    }

    private void btnZoomFit_Click(object sender, System.EventArgs e) => Pad.ZoomFit();

    private void btnZurueck_Click(object sender, System.EventArgs e) {
        if (_fileList.Count < 2) { return; }
        _nr--;
        if (_nr <= 0) { _nr = 0; }
        LoadPic(_nr);
    }

    private void Pad_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e) {
        if (btnZoomIn.Checked) { Pad.ZoomIn(e); }
        if (btnZoomOut.Checked) { Pad.ZoomOut(e); }
    }

    #endregion
}
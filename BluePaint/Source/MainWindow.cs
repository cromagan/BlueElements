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

#nullable enable

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BluePaint.EventArgs;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using static BlueBasics.Extensions;
using static BlueBasics.IO;
using MessageBox = BlueControls.Forms.MessageBox;

namespace BluePaint;

public partial class MainWindow : BlueControls.Forms.Form {

    #region Fields

    private GenericTool? _currentTool;

    private string _filename = string.Empty;

    private bool _isSaved = true;

    private Bitmap? _picUndo;

    #endregion

    #region Constructors

    public MainWindow(bool loadSaveEnabled) : base() {
        InitializeComponent();
        Tab_Start.Enabled = loadSaveEnabled;
        btnOK.Visible = !loadSaveEnabled;
        tabRibbonbar.SelectedIndex = 1;
    }

    public MainWindow(string filename, bool loadSaveEnabled) : this(loadSaveEnabled) => LoadFromDisk(filename);

    public MainWindow() : this(true) { }

    #endregion

    #region Methods

    [BlueControls.StandaloneInfo("Bild-bearbeitung", ImageCode.Bild, "Allgemein", 802)]
    public static System.Windows.Forms.Form Start() => new MainWindow();

    /// <summary>
    /// Filename wird entfernt!
    /// </summary>
    /// <param name="bmp"></param>
    /// <param name="zoomfit"></param>
    public void SetPic(Bitmap? bmp, bool zoomfit) {
        CurrentTool_OverridePic(this, new ZoomBitmapEventArgs(bmp, zoomfit));
        _filename = string.Empty;
    }

    public void SetTool(GenericTool? newTool) {
        if (AreSame(newTool, _currentTool)) {
            MessageBox.Show("Das Werkzeug ist aktuell schon gewählt.", ImageCode.Information, "OK");
            return;
        }
        if (_currentTool != null) {
            _currentTool.OnToolChanging();
            _currentTool.Dispose();
            Split.Panel1.Controls.Remove(_currentTool);
            _currentTool.ZoomFit -= CurrentTool_ZoomFit;
            _currentTool.HideMainWindow -= CurrentTool_HideMainWindow;
            _currentTool.ShowMainWindow -= CurrentTool_ShowMainWindow;
            _currentTool.ForceUndoSaving -= CurrentTool_ForceUndoSaving;
            _currentTool.OverridePic -= CurrentTool_OverridePic;
            _currentTool.DoInvalidate -= CurrentTool_DoInvalidate;
            _currentTool.NeedCurrentPic -= CurrentTool_NeedCurrentPic;
            _currentTool = null;
        }
        P.Invalidate();

        if (newTool != null) {
            _currentTool = newTool;
            Split.Panel1.Controls.Add(newTool);
            newTool.Dock = DockStyle.Fill;
            _currentTool.ZoomFit += CurrentTool_ZoomFit;
            _currentTool.HideMainWindow += CurrentTool_HideMainWindow;
            _currentTool.ShowMainWindow += CurrentTool_ShowMainWindow;
            _currentTool.OverridePic += CurrentTool_OverridePic;
            _currentTool.ForceUndoSaving += CurrentTool_ForceUndoSaving;
            _currentTool.DoInvalidate += CurrentTool_DoInvalidate;
            _currentTool.NeedCurrentPic += CurrentTool_NeedCurrentPic;

            newTool.ToolFirstShown();
        }
    }

    public new Bitmap? ShowDialog() {
        if (Visible) { Visible = false; }
        base.ShowDialog();
        return P.Bmp;
    }

    protected override void OnFormClosing(FormClosingEventArgs e) {
        if (!IsSaved()) { e.Cancel = true; }
        base.OnFormClosing(e);
    }

    private static bool AreSame(object? a, object? b) {
        if (a == null || b == null) { return false; }
        var t = a.GetType();
        var u = b.GetType();
        if (t.IsAssignableFrom(u) || u.IsAssignableFrom(t)) {
            // x.IsAssignableFrom(y) returns true if:
            //   (1) x and y are the same type
            //   (2) x and y are in the same inheritance hierarchy
            //   (3) y is implemented by x
            //   (4) y is a generic type parameter and one of its constraints is x
            return true;
        }
        return false;
    }

    private void Bruchlinie_Click(object sender, System.EventArgs e) => SetTool(new Tool_Bruchlinie());

    private void btn100_Click(object sender, System.EventArgs e) => P.Zoom = 1f;

    private void btnCopy_Click(object sender, System.EventArgs e) {
        SetTool(null); // um OnToolChangeAuszulösen
        if (P.Bmp is { } pic && pic.IsValid()) {
            Clipboard.SetImage(pic);
            //System.Windows.Clipboard.SetDataObject(P.Bmp, false);
            Notification.Show("Das Bild ist nun<br>in der Zwischenablage.", ImageCode.Clipboard);

            return;
        }
        MessageBox.Show("Kein Bild vorhanden.");
    }

    private void btnEinfügen_Click(object sender, System.EventArgs e) {
        if (!IsSaved()) { return; }
        if (!System.Windows.Clipboard.ContainsImage()) {
            Notification.Show("Abbruch,<br>kein Bild im Zwischenspeicher!", ImageCode.Information);
            return;
        }
        SetPic((Bitmap)Clipboard.GetImage(), true);
        _isSaved = false;
        _filename = "*";
        P.ZoomFit();
    }

    private void btnGrößeÄndern_Click(object sender, System.EventArgs e) => SetTool(new Tool_Resize());

    private void btnLetzteDateien_ItemClicked(object sender, AbstractListItemEventArgs e) {
        if (!IsSaved()) { return; }
        LoadFromDisk(e.Item.KeyName);
    }

    private void btnNeu_Click(object sender, System.EventArgs e) {
        if (!IsSaved()) { return; }
        SetPic(new Bitmap(100, 100), true);
        _filename = "*";
    }

    private void btnOeffnen_Click(object sender, System.EventArgs e) {
        if (!IsSaved()) { return; }
        LoadTab.ShowDialog();
    }

    private void btnSave_Click(object sender, System.EventArgs e) => Speichern();

    private void btnSaveAs_Click(object? sender, System.EventArgs e) {
        SaveTab.ShowDialog();
        if (!DirectoryExists(SaveTab.FileName.FilePath())) { return; }
        if (string.IsNullOrEmpty(SaveTab.FileName)) { return; }
        if (FileExists(SaveTab.FileName)) {
            if (MessageBox.Show("Datei bereits vorhanden.<br>Überschreiben?", ImageCode.Frage, "Ja", "Nein") != 0) { return; }
        }
        _filename = SaveTab.FileName;
        _isSaved = false;
        Speichern();
    }

    private void btnZoomFit_Click(object sender, System.EventArgs e) => P.ZoomFit();

    private void Clipping_Click(object sender, System.EventArgs e) => SetTool(new Tool_Clipping());

    private void CurrentTool_DoInvalidate(object sender, System.EventArgs e) => P.Invalidate();

    private void CurrentTool_ForceUndoSaving(object sender, System.EventArgs e) {
        _isSaved = false;
        if (_picUndo != null) {
            _picUndo?.Dispose();
            _picUndo = null;
            GC.Collect();
        }

        if (P.Bmp is not Bitmap bmp) {
            btnRückgänig.Enabled = false;
            return;
        }

        _picUndo = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(_picUndo);
        g.Clear(Color.Transparent);
        g.DrawImage(bmp, 0, 0, bmp.Width, bmp.Height);

        btnRückgänig.Enabled = true;
    }

    private void CurrentTool_HideMainWindow(object sender, System.EventArgs e) => Hide();

    private void CurrentTool_NeedCurrentPic(object sender, BitmapEventArgs e) => e.Bmp = P.Bmp;

    private void CurrentTool_OverridePic(object sender, ZoomBitmapEventArgs e) {
        CurrentTool_ForceUndoSaving(this, System.EventArgs.Empty);

        if (e.Bmp != null) {
            P.Bmp = new Bitmap(e.Bmp.Width, e.Bmp.Height, PixelFormat.Format32bppArgb);

            // Inhalt kopieren
            using var g = Graphics.FromImage(P.Bmp);
            g.Clear(Color.Transparent);
            g.DrawImage(e.Bmp, 0, 0, e.Bmp.Width, e.Bmp.Height);
        } else {
            P.Bmp = null;
        }
        //P.Bmp = e.Bmp?.Clone() as Bitmap;
        if (e.DoZoomFit) { P.ZoomFit(); }
        P.Invalidate();
    }

    private void CurrentTool_ShowMainWindow(object sender, System.EventArgs e) => Show();

    private void CurrentTool_ZoomFit(object sender, System.EventArgs e) => P.ZoomFit();

    private void Dummy_Click(object sender, System.EventArgs e) => SetTool(new Tool_DummyGenerator());

    private bool IsSaved() {
        while (true) {
            if (_isSaved) {
                return true;
            }

            if (string.IsNullOrEmpty(_filename)) {
                return true;
            }

            switch (MessageBox.Show("Es sind ungespeicherte Änderungen vorhanden.<br>Was möchten sie tun?", ImageCode.Diskette, "Speichern", "Verwerfen", "Abbrechen")) {
                case 0:
                    Speichern();
                    break;

                case 1:
                    _isSaved = true;
                    return true;

                case 2:
                    return false;
            }
        }
    }

    private void Kontrast_Click(object sender, System.EventArgs e) => SetTool(new Tool_Kontrast());

    private void LoadFromDisk(string filename) {
        if (!IsSaved()) { return; }
        if (FileExists(filename)) {
            SetPic(Image_FromFile(filename) as Bitmap, true);
            _filename = filename;
            _isSaved = true;
            btnLetzteDateien.AddFileName(filename, string.Empty);
        }
        P.ZoomFit();
    }

    private void LoadTab_FileOk(object sender, CancelEventArgs e) => LoadFromDisk(LoadTab.FileName);

    private void OK_Click(object sender, System.EventArgs e) {
        SetTool(null); // um OnToolChangeAuszulösen
        Close();
    }

    private void P_DoAdditionalDrawing(object sender, AdditionalDrawing e) => _currentTool?.DoAdditionalDrawing(e, P.Bmp);

    private void P_ImageMouseDown(object sender, MouseEventArgs1_1 e) => _currentTool?.MouseDown(e, P.Bmp);

    private void P_ImageMouseMove(object sender, MouseEventArgs1_1DownAndCurrent e) {
        _currentTool?.MouseMove(e, P.Bmp);
        if (e.Current.IsInPic && P.Bmp is { } bmp && bmp.IsValid()) {
            var c = bmp.GetPixel(e.Current.TrimmedX, e.Current.TrimmedY);
            InfoText.Text = "X: " + e.Current.TrimmedX +
                            "<br>Y: " + e.Current.TrimmedY +
                            "<br>Farbe: " + c.ToHtmlCode().ToUpperInvariant();
        } else {
            InfoText.Text = string.Empty;
        }
    }

    private void P_ImageMouseUp(object sender, MouseEventArgs1_1DownAndCurrent e) => _currentTool?.MouseUp(e, P.Bmp);

    private void P_MouseLeave(object sender, System.EventArgs e) => InfoText.Text = string.Empty;

    private void Radiergummi_Click(object sender, System.EventArgs e) => SetTool(new Tool_Eraser());

    private void Rückg_Click(object sender, System.EventArgs e) {
        if (_picUndo == null) { return; }
        btnRückgänig.Enabled = false;
        _isSaved = false;
        var bmp = P.Bmp;
        if (bmp == null) { return; }
        Generic.Swap(ref bmp, ref _picUndo);
        P.Bmp = bmp;
        if (P.Bmp.Width != _picUndo.Width || P.Bmp.Height != _picUndo.Height) {
            P.ZoomFit();
        } else {
            P.Invalidate();
        }

        _currentTool?.PictureChangedByMainWindow();
    }

    private void Screenshot_Click(object sender, System.EventArgs e) => SetTool(new Tool_Screenshot());

    private void Speichern() {
        SetTool(null); // um OnToolChangeAuszulösen
        if (_filename == "*") {
            btnSaveAs_Click(null, System.EventArgs.Empty);
            return;
        }

        if (P.Bmp is not { } bmp || !bmp.IsValid()) { return; }

        try {
            switch (_filename.FileSuffix().ToUpperInvariant()) {
                case "JPG":
                    bmp.Save(_filename, ImageFormat.Jpeg);
                    _isSaved = true;
                    break;

                case "BMP":
                    bmp.Save(_filename, ImageFormat.Bmp);
                    _isSaved = true;
                    break;

                case "PNG":
                    bmp.Save(_filename, ImageFormat.Png);
                    _isSaved = true;
                    break;

                default:
                    bmp.Save(_filename);
                    _isSaved = true;
                    break;
            }
        } catch {
            _isSaved = false;
        }
    }

    private void Spiegeln_Click(object sender, System.EventArgs e) => SetTool(new Tool_Spiegeln());

    private void Zeichnen_Click(object sender, System.EventArgs e) => SetTool(new Tool_Paint());

    #endregion
}
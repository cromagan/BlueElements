// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
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

using BlueBasics.ClassesStatic;
using BlueBasics.Interfaces;
using BlueControls.Classes.ItemCollectionPad;
using BlueControls.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using static BlueBasics.Extensions;

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
            if (Screen.AllScreens.Length == 1 || openOnScreen < 0) {
                var opScNr = Generic.PointOnScreenNr(Cursor.Position);
                Width = (int)(Screen.AllScreens[opScNr].WorkingArea.Width / 1.5);
                Height = (int)(Screen.AllScreens[opScNr].WorkingArea.Height / 1.5);
                Left = (int)(Screen.AllScreens[opScNr].WorkingArea.Left + ((Screen.AllScreens[opScNr].WorkingArea.Width - Width) / 2.0));
                Top = (int)(Screen.AllScreens[opScNr].WorkingArea.Top + ((Screen.AllScreens[opScNr].WorkingArea.Height - Height) / 2.0));
            } else {
                Width = Screen.AllScreens[openOnScreen].WorkingArea.Width;
                Height = Screen.AllScreens[openOnScreen].WorkingArea.Height;
                Left = Screen.AllScreens[openOnScreen].WorkingArea.Left;
                Top = Screen.AllScreens[openOnScreen].WorkingArea.Top;
            }
        }

        if (!string.IsNullOrEmpty(windowCaption)) { Text = windowCaption; }

        btnZoomIn.Checked = true;
        btnChoose.Enabled = false;

        Pad.EditAllowed = true;

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
        Pad.SaveNotes();

        _nr = nr;
        if (nr < _fileList.Count && nr > -1) {
            try {
                Pad.Bmp = Image_FromFile(_fileList[nr]) as Bitmap;
            } catch (Exception ex) {
                Pad.Bmp = null;
                Develop.DebugPrint("Fehler beim Laden des Bildes", ex);
            }

            var tags = ZoomPicNew.LoadTags(_fileList[nr]);
            Pad.Tags.Clear();
            Pad.Tags.AddRange(tags);
        } else {
            Pad.Bmp = null;
            Pad.Tags.Clear();
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

        Pad.NoteOrigin = CurrentNoteOrigin();
        Pad.Items?.Clear();
        Pad.LoadNotes();
        LoadPoints();
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

    private string CurrentNoteOrigin() {
        if (_nr < 0 || _nr >= _fileList.Count) { return string.Empty; }
        return _fileList[_nr] + "|" + _nr;
    }

    private void LoadPoints() {
        var allPointNames = Pad.Tags.TagGet("AllPointNames").FromNonCritical().SplitAndCutBy("|");
        foreach (var pointName in allPointNames) {
            if (string.IsNullOrEmpty(pointName)) { continue; }
            var posStr = Pad.Tags.TagGet(pointName);
            if (string.IsNullOrEmpty(posStr)) { continue; }
            var parts = posStr.SplitAndCutBy("|");
            if (parts.Length >= 2 &&
                float.TryParse(parts[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var x) &&
                float.TryParse(parts[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var y)) {
                var item = new NotePadItem("POINT_" + pointName, x, y, NotePadItem.PointSymbol, pointName);
                Pad.Items?.Add(item);
            }
        }
    }

    private void Pad_MouseUp(object sender, MouseEventArgs e) {
        if (btnZoomIn.Checked) { Pad.ZoomIn(e); }
        if (btnZoomOut.Checked) { Pad.ZoomOut(e); }
    }

    #endregion
}
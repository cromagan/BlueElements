// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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
using BlueControls.EventArgs;
using System.ComponentModel;
using System.Windows.Forms;
using BlueBasics.Enums;
using static BlueBasics.IO;

namespace BlueControls.Forms;

public partial class PadEditorWithFileAccess : PadEditor {

    #region Fields

    private string _lastFileName = string.Empty;

    #endregion

    #region Constructors

    public PadEditorWithFileAccess() : base() => InitializeComponent();

    #endregion

    /// <summary>
    /// löscht den kompletten Inhalt des Pads auch die ID und setzt es auf Disabled
    /// </summary>

    #region Methods

    public void DisablePad() {
        CheckSave();
        _lastFileName = string.Empty;
        Pad.Item = [];
        Pad.Enabled = false;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="fileName"></param>
    public void LoadFile(string fileName) {
        CheckSave();
        Pad.Enabled = true;
        Pad.Item = new ItemCollectionPad.ItemCollectionPad(fileName);
        btnLastFiles.AddFileName(fileName, fileName.FileNameWithSuffix());
        _lastFileName = fileName;
    }

    protected override void OnFormClosing(FormClosingEventArgs e) {
        CheckSave();
        base.OnFormClosing(e);
    }

    private void btnLastFiles_ItemClicked(object sender, AbstractListItemEventArgs e) => LoadFile(e.Item.KeyName);

    private void btnNeu_Click(object sender, System.EventArgs e) {
        CheckSave();
        _lastFileName = string.Empty;
        Pad?.Item?.Clear();
        Pad?.ZoomFit();
    }

    private void btnOeffnen_Click(object sender, System.EventArgs e) {
        LoadTab.Tag = sender;
        _ = LoadTab.ShowDialog();
    }

    private void btnSpeichern_Click(object sender, System.EventArgs e) => SaveTab.ShowDialog();

    private void CheckSave() {
        if (string.IsNullOrWhiteSpace(_lastFileName)) { return; }
        if (Pad?.Item is not { IsSaved: not true }) { return; }

        Pad.Item.IsSaved = true;

        if (MessageBox.Show("Die Änderungen sind nicht gespeichert.\r\nJetzt speichern?", ImageCode.Diskette, "Speichern", "Verwerfen") != 0) { return; }

        var t = Pad.Item.ToParseableString();
        WriteAllText(_lastFileName, t, Constants.Win1252, false);
    }

    private void LoadTab_FileOk(object sender, CancelEventArgs e) => LoadFile(LoadTab.FileName);

    private void SaveTab_FileOk(object sender, CancelEventArgs e) {
        if (Pad?.Item == null) { return; }

        var t = Pad.Item.ToParseableString();
        WriteAllText(SaveTab.FileName, t, Constants.Win1252, false);
        btnLastFiles.AddFileName(SaveTab.FileName, string.Empty);
        _lastFileName = SaveTab.FileName;
    }

    #endregion
}
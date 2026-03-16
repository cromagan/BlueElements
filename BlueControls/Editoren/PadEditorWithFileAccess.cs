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

using BlueBasics;
using BlueBasics.Classes;
using BlueBasics.ClassesStatic;
using BlueBasics.Enums;
using BlueControls.Classes;
using BlueControls.Classes.ItemCollectionList;
using BlueControls.Classes.ItemCollectionPad;
using BlueControls.Classes.ItemCollectionPad.Abstract;
using BlueControls.Enums;
using BlueControls.EventArgs;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using static BlueBasics.ClassesStatic.IO;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Forms;

public partial class PadEditorWithFileAccess : PadEditor {

    #region Fields

    private string _lastFileName = string.Empty;

    #endregion

    #region Constructors

    public PadEditorWithFileAccess() : base() => InitializeComponent();

    #endregion

    #region Methods

    [StandaloneInfo("Layout-Editor", "Layout|32|||||||||Stift", "Admin", "Allgemeiner Layout-Editor (für Exporte von Zeilen)", 900)]
    public static System.Windows.Forms.Form Start() => new PadEditorWithFileAccess();

    /// <summary>
    ///
    /// </summary>
    /// <param name="fileName"></param>
    public void LoadFile(string fileName) {
        CheckSave();
        Pad.Enabled = true;
        Pad.Items = new ItemCollectionPadItem(fileName);
        btnLastFiles.AddFileName(fileName, fileName.FileNameWithSuffix());
        _lastFileName = fileName;
        Pad?.ZoomFit();
    }

    /// <summary>
    /// löscht den kompletten Inhalt des Pads auch die ID und setzt es auf Disabled
    /// </summary>
    protected override void OnFormClosing(FormClosingEventArgs e) {
        CheckSave();
        base.OnFormClosing(e);
    }

    private void btnAddDimension_Click(object sender, System.EventArgs e) {
        var b = new DimensionPadItem(new PointF(300, 300), new PointF(400, 300), 30);
        Pad.AddCentered(b);
    }

    private void btnAddImage_Click(object sender, System.EventArgs e) {
        var b = new BitmapPadItem(string.Empty, QuickImage.Get(ImageCode.Fragezeichen), new Size(1000, 1000));
        Pad.AddCentered(b);
    }

    private void btnAddLine_Click(object sender, System.EventArgs e) {
        var p = Pad.MiddleOfVisiblesScreen();
        var w = (int)(300 / Pad.Zoom);
        var b = new LinePadItem(PadStyles.Standard, p with { X = p.X - w }, p with { X = p.X + w });
        Pad.AddCentered(b);
    }

    private void btnAddSymbol_Click(object sender, System.EventArgs e) {
        var b = new SymbolPadItem();
        b.SetCoordinates(new RectangleF(100, 100, 300, 300));
        Pad.AddCentered(b);
    }

    private void btnAddText_Click(object sender, System.EventArgs e) {
        var b = new TextPadItem() {
            Text = string.Empty,
            Style = PadStyles.Standard
        };
        Pad.AddCentered(b);
        b.SetCoordinates(new RectangleF(10, 10, 200, 200));
    }

    private void btnAddUnterStufe_Click(object sender, System.EventArgs e) {
        ItemCollectionPadItem b = [];
        Pad.AddCentered(b);
        b.SetCoordinates(new RectangleF(10, 10, 200, 200));
    }

    private void btnLastFiles_ItemClicked(object sender, AbstractListItemEventArgs e) => LoadFile(e.Item.KeyName);

    private void btnNeu_Click(object sender, System.EventArgs e) {
        CheckSave();
        _lastFileName = string.Empty;
        Pad?.Items?.Clear();
        Pad?.ZoomFit();
    }

    private void btnOeffnen_Click(object sender, System.EventArgs e) {
        LoadTab.Tag = sender;
        LoadTab.ShowDialog();
    }

    private void btnSpeichern_Click(object sender, System.EventArgs e) => SaveTab.ShowDialog();

    private void btnSymbolLaden_Click(object sender, System.EventArgs e) {
        if (!string.IsNullOrEmpty(LastFilePath)) { LoadSymbol.InitialDirectory = LastFilePath; }

        LoadSymbol.ShowDialog();
    }

    private void btnWeitereAllItem_Click(object sender, System.EventArgs e) {
        var l = Generic.GetInstaceOfType<AbstractPadItem>();

        if (l.Count == 0) { return; }

        var i = new List<AbstractListItem>();

        foreach (var thisl in l) {
            i.Add(ItemOf(thisl));
        }

        var x = InputBoxListBoxStyle.Show("Hinzufügen:", i, CheckBehavior.SingleSelection, null, AddType.None);

        if (x is not { Count: 1 }) { return; }

        var toadd = i.GetByKey(x[0]);

        if (toadd is not ReadableListItem { Item: AbstractPadItem api }) { return; }

        //if (toadd is not AbstractPadItem api) {  return; }

        //var x = new FileExplorerPadItem(string.Empty);

        Pad.AddCentered(api);
    }

    private void CheckSave() {
        if (string.IsNullOrWhiteSpace(_lastFileName)) { return; }
        if (Pad?.Items is not { IsSaved: false }) { return; }

        Pad.Items.IsSaved = true;

        if (MessageBox.Show("Die Änderungen sind nicht gespeichert.\r\nJetzt speichern?", ImageCode.Diskette, "Speichern", "Verwerfen") != 0) { return; }

        var t = Pad.Items.ParseableItems().FinishParseable();
        WriteAllText(_lastFileName, t, Constants.Win1252, false);
    }

    private void LoadSymbol_FileOk(object sender, CancelEventArgs e) {
        if (Pad.Items == null) { return; }

        if (string.IsNullOrEmpty(LoadSymbol.FileName)) { return; }
        var x = ReadAllText(LoadSymbol.FileName, Constants.Win1252);
        LastFilePath = LoadSymbol.FileName.FilePath();

        var i = ParseableItem.NewByParsing<AbstractPadItem>(x);
        if (i is not { }) { return; }
        i.GetNewIdsForEverything();
        Pad.Items.Add(i);
    }

    private void LoadTab_FileOk(object sender, CancelEventArgs e) => LoadFile(LoadTab.FileName);

    private void SaveTab_FileOk(object sender, CancelEventArgs e) {
        if (Pad?.Items == null) { return; }

        var t = Pad.Items.ParseableItems().FinishParseable();
        WriteAllText(SaveTab.FileName, t, Constants.Win1252, false);
        btnLastFiles.AddFileName(SaveTab.FileName, string.Empty);
        _lastFileName = SaveTab.FileName;
    }

    #endregion
}
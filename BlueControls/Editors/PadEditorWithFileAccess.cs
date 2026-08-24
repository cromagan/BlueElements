// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls.ConnectedFormula;
using BlueControls.EventArgs;
using BlueControls.PadItems.Abstract;
using System.Windows.Forms;
using static BlueBasics.ClassesStatic.IO;

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
        Pad.Items = new CollectionPadItem(fileName);
        btnLastFiles.AddFileName(fileName, fileName.FileNameWithSuffix());
        _lastFileName = fileName;
        Pad.ZoomFit();
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
            TextValue = string.Empty,
            Style = PadStyles.Standard
        };
        Pad.AddCentered(b);
        b.SetCoordinates(new RectangleF(10, 10, 200, 200));
    }

    private void btnAddUnterStufe_Click(object sender, System.EventArgs e) {
        CollectionPadItem b = [];
        Pad.AddCentered(b);
        b.SetCoordinates(new RectangleF(10, 10, 200, 200));
    }

    private void btnLastFiles_ItemClicked(object sender, ListItemEventArgs e) => LoadFile(e.Item.KeyName);

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
        var l = GetInstanceOfType<PadItem>();

        if (!l.Any()) { return; }

        var i = new List<ListItem>();

        foreach (var thisl in l) {
            i.Add(ItemOf(thisl));
        }

        var x = InputBoxListBoxStyle.Show("Hinzufügen:", i, CheckBehavior.SingleSelection, null, AddType.None);

        if (x is not { Count: 1 }) { return; }
        if (x[0] is not ReadableListItem { Item: PadItem api }) { return; }
        Pad.AddCentered(api);
    }

    private void CheckSave() {
        if (string.IsNullOrWhiteSpace(_lastFileName)) { return; }
        if (Pad?.Items is not { IsSaved: false }) { return; }

        Pad.Items.IsSaved = true;

        if (MessageBox.Show("Die Änderungen sind nicht gespeichert.\r\nJetzt speichern?", ImageCode.Diskette, "Speichern", "Verwerfen") != 0) { return; }

        var t = Pad.Items.ParseableItems().FinishParseable();
        SaveLayoutToDisk(_lastFileName, t);
    }

    /// <summary>
    /// Schreibt den Layout-Inhalt auf die Festplatte und invalidiert den
    /// zugehörigen <see cref="ConnectedFormula" />-Cache-Eintrag. Ohne
    /// Invalidierung würde ein sofortiges Wiederöffnen des Editors die
    /// veralteten gecachten Bytes liefern (Stale-Cache-Bug).
    /// </summary>
    private static void SaveLayoutToDisk(string fileName, string content) {
        WriteAllText(fileName, content, Win1252, false);
        ConnectedFormula.Get(fileName)?.Invalidate();
    }

    private void LoadSymbol_FileOk(object sender, CancelEventArgs e) {
        if (Pad.Items is null) { return; }

        if (string.IsNullOrEmpty(LoadSymbol.FileName)) { return; }
        var x = ReadAllText(LoadSymbol.FileName, Win1252);
        LastFilePath = LoadSymbol.FileName.FilePath();

        var i = ParseableItem.NewByParsing<PadItem>(x);
        if (i is null) { return; }
        i.GetNewIdsForEverything();
        Pad.Items.Add(i);
    }

    private void LoadTab_FileOk(object sender, CancelEventArgs e) => LoadFile(LoadTab.FileName);

    private void SaveTab_FileOk(object sender, CancelEventArgs e) {
        if (Pad?.Items is null) { return; }

        var t = Pad.Items.ParseableItems().FinishParseable();
        SaveLayoutToDisk(SaveTab.FileName, t);
        btnLastFiles.AddFileName(SaveTab.FileName, string.Empty);
        _lastFileName = SaveTab.FileName;
    }

    #endregion
}

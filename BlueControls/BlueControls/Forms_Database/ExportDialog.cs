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
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.ItemCollectionList;
using BlueControls.ItemCollectionPad;
using BlueDatabase;
using BlueDatabase.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using static BlueBasics.Converter;
using static BlueBasics.IO;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;
using ComboBox = BlueControls.Controls.ComboBox;

namespace BlueControls.Forms;

public sealed partial class ExportDialog : IHasDatabase {

    #region Fields

    private readonly List<RowItem>? _rowsForExport;
    private readonly string _saveTo = string.Empty;
    private readonly string _zielPfad;
    private Database? _database;
    private int _itemNrForPrint;

    #endregion

    #region Constructors

    public ExportDialog(Database db, string autosaveFile) : this(db, null, autosaveFile) { }

    public ExportDialog(Database db, List<RowItem>? rows) : this(db, rows, string.Empty) { }

    public ExportDialog(Database db, List<RowItem>? rows, string autosaveFile) {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();
        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
        Database = db;
        _rowsForExport = rows;
        if (!string.IsNullOrEmpty(autosaveFile)) {
            _zielPfad = autosaveFile.FilePath();
            _saveTo = autosaveFile;
        } else {
            _zielPfad = Path.GetTempPath();
        }
        try {
            if (!DirectoryExists(_zielPfad)) {
                _ = Directory.CreateDirectory(_zielPfad);
            }
        } catch { }

        BefülleLayoutDropdowns();
        EintragsText();
        NurStartEnablen();
    }

    #endregion

    #region Properties

    public Database? Database {
        get => _database;
        private set {
            if (IsDisposed || (value?.IsDisposed ?? true)) { value = null; }
            if (value == _database) { return; }

            if (_database != null) {
                _database.DisposingEvent -= _database_Disposing;
            }
            _database = value;

            if (_database != null) {
                _database.DisposingEvent += _database_Disposing;
            }
        }
    }

    #endregion

    #region Methods

    public static void AddLayoutsOff(ComboBox addHere, Database? db) {
        if (db is null || db.IsDisposed) { return; }
        var r = db.GetAllLayoutsFileNames();

        foreach (var thisFile in r) {
            if (addHere[thisFile] == null) { addHere.ItemAdd(ItemOf(thisFile.FileNameWithSuffix(), thisFile, QuickImage.Get(thisFile.FileType(), 16))); }
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="pad"></param>
    /// <param name="startNr"></param>
    /// <param name="layoutFileName"></param>
    /// <param name="rowsForExport"></param>
    /// <param name="abstandMm"></param>
    /// <returns>Gibt das Item zurück, dass nicht mehr auf den Druckbereich gepasst hat. -1 falls keine (gültige) Liste übergeben wurde.</returns>
    public static int GeneratePrintPad(CreativePad pad, int startNr, string layoutFileName, List<RowItem>? rowsForExport, float abstandMm) {
        if (pad.Items == null) { return -1; }

        pad.Items.Clear();
        Generic.CollectGarbage();

        if (rowsForExport is not { Count: > 0 }) { return -1; }

        var tmp = new ItemCollectionPadItem(layoutFileName);
        _ = tmp.ResetVariables();
        var scx = tmp.ReplaceVariables(rowsForExport[0]);
        if (!scx.AllOk) { return -1; }

        var oneItem = tmp.UsedArea;
        pad.Items.SheetStyle = tmp.SheetStyle;
        pad.ShowInPrintMode = true;
        pad.Items.GridShow = -1;
        pad.Items.BackColor = Color.LightGray;
        tmp.Dispose();

        var druckB = pad.Items.UsedArea;
        var abstand = (float)Math.Round(MmToPixel(abstandMm, ItemCollectionPadItem.Dpi), MidpointRounding.AwayFromZero);

        var maxX = Math.Max(1, (int)Math.Floor(druckB.Width / (oneItem.Width + abstand + 0.01)));
        var maxY = Math.Max(1, (int)Math.Floor(druckB.Height / (oneItem.Height + abstand + 0.01)));

        var offx = (druckB.Width - (oneItem.Width * maxX) - (abstand * (maxX - 1))) / 2;
        var offy = (druckB.Height - (oneItem.Height * maxY) - (abstand * (maxY - 1))) / 2;

        for (var y = 0; y < maxY; y++) {
            for (var x = 0; x < maxX; x++) {
                var it = new ItemCollectionPadItem(layoutFileName);

                //if (it._internal is { }) {
                _ = it.ReplaceVariables(rowsForExport[startNr]);
                //    it.GridShow = -1;
                //}
                pad.Items.Add(it);
                it.SetCoordinates(oneItem with { X = druckB.Left + (x * (oneItem.Width + abstand)) + offx, Y = druckB.Top + (y * (oneItem.Height + abstand)) + offy });

                //it.SetLeftTopPoint(druckB.Left + x * (oneItem.Width + abstand) + offx, druckB.Top + y * (oneItem.Height + abstand) + offy);

                startNr++;
                if (startNr >= rowsForExport.Count) { break; }
            }
            if (startNr >= rowsForExport.Count) { break; }
        }

        pad.ZoomFit();
        return startNr;
    }

    protected override void OnFormClosing(FormClosingEventArgs e) {
        Database = null;
        base.OnFormClosing(e);
    }

    private void _database_Disposing(object sender, System.EventArgs e) {
        Database = null;
        Close();
    }

    private void Attribute_Changed(object? sender, System.EventArgs? e) {
        _ = FloatTryParse(flxBreite.Value, out var b);
        _ = FloatTryParse(flxHöhe.Value, out var h);
        _ = FloatTryParse(flxAbstand.Value, out var ab);
        if (ab < 1) { ab = 0; }
        if (b < 10) { b = 10; }
        if (h < 10) { h = 10; }

        if (padSchachteln.Items != null) {
            padSchachteln.Items.Breite = b;
            padSchachteln.Items.Höhe = h;
            padSchachteln.Items.RandinMm = Padding.Empty;
            padSchachteln.Items.BackColor = Color.Transparent;
        }

        _ = GeneratePrintPad(padSchachteln, 0, cbxLayoutWahl.Text, _rowsForExport, ab);
    }

    private void BefülleLayoutDropdowns() {
        cbxLayoutWahl.ItemClear();
        AddLayoutsOff(cbxLayoutWahl, Database);
    }

    private void btnDrucken_Click(object sender, System.EventArgs e) => padPrint.Print();

    private void btnEinstellung_Click(object sender, System.EventArgs e) {
        switch (MessageBox.Show("Einstellung laden:", ImageCode.Stift, "A4", "Cricut Maker", "A4 Printer", "Abbrechen")) {
            case 0:
                flxBreite.ValueSet("210", true);
                flxHöhe.ValueSet("297", true);
                flxAbstand.ValueSet("0", true);
                break;

            case 1:
                flxBreite.ValueSet("171,1", true);
                flxHöhe.ValueSet("234,9", true);
                flxAbstand.ValueSet("2", true);
                break;

            case 2:
                flxBreite.ValueSet("190", true);
                flxHöhe.ValueSet("277", true);
                flxAbstand.ValueSet("1", true);
                break;
        }
    }

    private void btnSchachtelnSpeichern_Click(object sender, System.EventArgs e) {

        if(padSchachteln.Items is not {IsDisposed: false }) { return; }

        _ = FloatTryParse(flxBreite.Value, out var b);
        _ = FloatTryParse(flxHöhe.Value, out var h);
        _ = FloatTryParse(flxAbstand.Value, out var ab);
        if (ab < 1) { ab = 0; }
        if (b < 10) { b = 10; }
        if (h < 10) { h = 10; }
        padSchachteln.Items.Breite = b;
        padSchachteln.Items.Höhe = h;
        padSchachteln.Items.RandinMm = Padding.Empty;
        List<string> l = [];
        _itemNrForPrint = 0;
        do {
            var nr = _itemNrForPrint;
            _itemNrForPrint = GeneratePrintPad(padSchachteln, _itemNrForPrint, cbxLayoutWahl.Text, _rowsForExport, ab);

            var x = TempFile(_zielPfad, _rowsForExport[0].Database.Caption + "_" + b + "x" + h + "_" + ab, "png");
            padSchachteln.Items.BackColor = Color.Transparent;
            padSchachteln.Items.SaveAsBitmap(x);
            l.Add(x);
            if (nr == _itemNrForPrint) { break; }
            if (_itemNrForPrint >= _rowsForExport.Count) { break; }
        } while (true);
        tabStart.Enabled = false;
        tabBildSchachteln.Enabled = false;
        tabDateiExport.Enabled = true;
        Tabs.SelectedTab = tabDateiExport;
        lstExported.ItemAddRange(l);
    }

    private void Button_PageSetup_Click(object? sender, System.EventArgs e) {
        padPrint.ShowPrinterPageSetup();
        padPrint.CopyPrinterSettingsToWorkingArea();
        _ = GeneratePrintPad(padPrint, 0, cbxLayoutWahl.Text, _rowsForExport, 0);
    }

    private void Button1_Click(object sender, System.EventArgs e) => ExecuteFile(_zielPfad);

    private void cbxLayoutWahl_TextChanged(object sender, System.EventArgs e) {
        if (Database == null || string.IsNullOrEmpty(cbxLayoutWahl.Text) || _rowsForExport == null || !_rowsForExport.Any()) {
            padVorschau.Items?.Clear();
        } else {
            padVorschau.ShowInPrintMode = true;
            padVorschau.Items = new ItemCollectionPadItem(cbxLayoutWahl.Text);
            _ = padVorschau.Items.ResetVariables();
            _ = padVorschau.Items.ReplaceVariables(_rowsForExport[0]);
            padVorschau.ZoomFit();
        }
    }

    private void EintragsText() => capAnzahlInfo.Text = _rowsForExport is not { Count: not 0 }
        ? "Bitte wählen sie die Einträge für den Export."
        : _rowsForExport.Count == 1
            ? "Es ist genau ein Eintrag gewählt:<br> <b>-" + _rowsForExport[0].CellFirstString().Replace("\r\n", " ")
            : "Es sind <b>" + _rowsForExport.Count + "</b> Einträge gewählt.";

    private void Exported_ItemClicked(object sender, AbstractListItemEventArgs e) => ExecuteFile(e.Item.KeyName);

    private string Fehler() {
        if (IsDisposed || Database is not { IsDisposed: false }) { return "Datenbank verworfen"; }
        if (_rowsForExport is not { Count: not 0 }) { return "Es sind keine Einträge für den Export gewählt."; }
        if (string.IsNullOrEmpty(cbxLayoutWahl.Text)) { return "Es sind keine Layout für den Export gewählt."; }
        ////TODO: Schachteln implementieren
        //if (!optSpezialFormat.Checked && !optEinzelnSpeichern.Checked) { return "Das gewählte Layout kann nur gespeichtert oder im Spezialformat bearbeitet werden."; }

        return string.Empty;
    }

    private void FrmDrucken_Drucken_Click(object sender, System.EventArgs e) => Close();

    private void LayoutEditor_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Database is not { IsDisposed: false } db) { return; }

        Enabled = false;
        var n = cbxLayoutWahl.Text;
        cbxLayoutWahl.Text = string.Empty;
        TableView.OpenLayoutEditor(db, n);
        BefülleLayoutDropdowns();
        if (cbxLayoutWahl[n] != null) {
            cbxLayoutWahl.Text = n;
        }
        Enabled = true;
    }

    private void lstExported_ContextMenuInit(object sender, ContextMenuInitEventArgs e) {
        e.ContextMenu.Add(ItemOf(ContextMenuCommands.DateiPfadÖffnen));
        e.ContextMenu.Add(ItemOf(ContextMenuCommands.Kopieren));
    }

    private void lstExported_ContextMenuItemClicked(object sender, ContextMenuItemClickedEventArgs e) {
        if (e.HotItem is not TextListItem tl) { return; }

        switch (e.Item.KeyName) {
            case "DateiPfadÖffnen":
                _ = ExecuteFile(tl.KeyName.FilePath());
                break;

            case "Kopieren":
                var x = new StringCollection {
                    tl.KeyName
                };
                Clipboard.SetFileDropList(x);
                break;
        }
    }

    private void NurStartEnablen() {
        tabStart.Enabled = true;
        tabDrucken.Enabled = false;
        tabDateiExport.Enabled = false;
        tabBildSchachteln.Enabled = false;
    }

    private void PrintPad_BeginnPrint(object sender, PrintEventArgs e) => _itemNrForPrint = 0;

    private void PrintPad_PrintPage(object sender, PrintPageEventArgs e) {
        var l = _itemNrForPrint;
        _itemNrForPrint = GeneratePrintPad(padPrint, _itemNrForPrint, cbxLayoutWahl.Text, _rowsForExport, 0);
        if (l == _itemNrForPrint) { return; }
        e.HasMorePages = _itemNrForPrint < _rowsForExport.Count;
    }

    private void Tabs_SelectedIndexChanged(object sender, System.EventArgs e) {
        if (Tabs.SelectedTab == tabStart && tabStart.Enabled) {
            NurStartEnablen();
        }
    }

    // Den Rest mach 'PrintPad.PrintPage'
    private void Vorschau_Click(object sender, System.EventArgs e) => padPrint.ShowPrintPreview();

    private void WeiterAktion_Click(object sender, System.EventArgs e) {
        var f = Fehler();
        if (!string.IsNullOrEmpty(f)) {
            MessageBox.Show(f);
            return;
        }

        if (Database is null || _rowsForExport == null) { return; } // wird mit Fehler schon abgedeckt

        if (optBildSchateln.Checked) {
            tabBildSchachteln.Enabled = true;
            Tabs.SelectedTab = tabBildSchachteln;
            Attribute_Changed(null, System.EventArgs.Empty);
        }

        if (optDrucken.Checked) {
            tabDrucken.Enabled = true;
            Tabs.SelectedTab = tabDrucken;
            Button_PageSetup_Click(null, System.EventArgs.Empty);
            _ = GeneratePrintPad(padPrint, 0, cbxLayoutWahl.Text, _rowsForExport, 0);
        }

        if (optEinzelnSpeichern.Checked || optSpezialFormat.Checked) {
            tabStart.Enabled = false; // Geht ja gleich los
            tabDateiExport.Enabled = true;
            Tabs.SelectedTab = tabDateiExport;

            //var (files, error) = !string.IsNullOrEmpty(cbxLayoutWahl.Text)

            //    ? Export.SaveAsBitmap(_rowsForExport, cbxLayoutWahl.Text, _zielPfad)
            //    : Export.GenerateLayout_FileSystem(_rowsForExport, cbxLayoutWahl.Text, _saveTo, optSpezialFormat.Checked, _zielPfad);

            var (files, error) = Export.GenerateLayout_FileSystem(_rowsForExport, cbxLayoutWahl.Text, _saveTo, optSpezialFormat.Checked, _zielPfad);
            lstExported.ItemAddRange(files);

            if (!string.IsNullOrEmpty(error)) {
                MessageBox.Show(error);
            }
        }
    }

    #endregion
}
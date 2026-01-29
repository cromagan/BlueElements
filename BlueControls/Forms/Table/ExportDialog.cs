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

using BlueBasics.Classes;
using BlueBasics.Enums;
using BlueControls.Classes;
using BlueControls.Classes.ItemCollectionList;
using BlueControls.Classes.ItemCollectionPad;
using BlueControls.Controls;
using BlueControls.EventArgs;
using BlueTable.Classes;
using BlueTable.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Printing;

using System.Linq;
using System.Windows.Forms;
using static BlueBasics.ClassesStatic.Converter;
using static BlueBasics.ClassesStatic.IO;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;
using ComboBox = BlueControls.Controls.ComboBox;
using BlueBasics.ClassesStatic;

namespace BlueControls.Forms;

public sealed partial class ExportDialog : IHasTable {

    #region Fields

    private readonly IReadOnlyList<RowItem>? _rowsForExport;
    private readonly string _saveTo = string.Empty;
    private readonly string _zielPfad;
    private int _itemNrForPrint;

    #endregion

    #region Constructors

    public ExportDialog(Table tb, string autosaveFile) : this(tb, null, autosaveFile) {
    }

    public ExportDialog(Table tb, IReadOnlyList<RowItem>? rows) : this(tb, rows, string.Empty) {
    }

    public ExportDialog(Table tb, IReadOnlyList<RowItem>? rows, string autosaveFile) {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();
        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
        Table = tb;
        _rowsForExport = rows;
        if (!string.IsNullOrEmpty(autosaveFile)) {
            _zielPfad = autosaveFile.FilePath();
            _saveTo = autosaveFile;
        } else {
            _zielPfad = System.IO.Path.GetTempPath();
        }

        CreateDirectory(_zielPfad);

        BefülleLayoutDropdowns();
        EintragsText();
        NurStartEnablen();
    }

    #endregion

    #region Properties

    public Table? Table {
        get;
        private set {
            if (IsDisposed || (value?.IsDisposed ?? true)) { value = null; }
            if (value == field) { return; }

            if (field != null) {
                field.DisposingEvent -= _table_Disposing;
            }
            field = value;

            if (field != null) {
                field.DisposingEvent += _table_Disposing;
            }
        }
    }

    #endregion

    #region Methods

    public static void AddLayoutsOff(ComboBox addHere, Table? tb) {
        if (tb?.IsDisposed != false) { return; }
        var r = tb.GetAllLayoutsFileNames();

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
    public static int GeneratePrintPad(CreativePad pad, int startNr, string layoutFileName, IReadOnlyList<RowItem>? rowsForExport, float abstandMm) {
        if (pad.Items == null) { return -1; }

        pad.Items.Clear();
        Generic.CollectGarbage();

        if (rowsForExport is not { Count: > 0 }) { return -1; }

        var tmp = new ItemCollectionPadItem(layoutFileName);
        tmp.ResetVariables();
        var scx = tmp.ReplaceVariables(rowsForExport[0]);
        if (scx.Failed) { return -1; }

        var oneItem = tmp.CanvasUsedArea;
        pad.Items.SheetStyle = tmp.SheetStyle;
        pad.ShowInPrintMode = true;
        pad.Items.GridShow = -1;
        pad.Items.BackColor = Color.LightGray;
        tmp.Dispose();

        var druckB = pad.Items.CanvasUsedArea;
        var abstand = (float)Math.Round(MmToPixel(abstandMm, ItemCollectionPadItem.Dpi), MidpointRounding.AwayFromZero);

        var maxX = Math.Max(1, (int)Math.Floor(druckB.Width / (oneItem.Width + abstand + 0.01)));
        var maxY = Math.Max(1, (int)Math.Floor(druckB.Height / (oneItem.Height + abstand + 0.01)));

        var offx = (druckB.Width - (oneItem.Width * maxX) - (abstand * (maxX - 1))) / 2;
        var offy = (druckB.Height - (oneItem.Height * maxY) - (abstand * (maxY - 1))) / 2;

        for (var y = 0; y < maxY; y++) {
            for (var x = 0; x < maxX; x++) {
                var it = new ItemCollectionPadItem(layoutFileName);

                //if (it._internal is { }) {
                it.ReplaceVariables(rowsForExport[startNr]);
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
        Table = null;
        base.OnFormClosing(e);
    }

    private void _table_Disposing(object sender, System.EventArgs e) {
        Table = null;
        Close();
    }

    private void Attribute_Changed(object? sender, System.EventArgs? e) {
        FloatTryParse(flxBreite.Value, out var b);
        FloatTryParse(flxHöhe.Value, out var h);
        FloatTryParse(flxAbstand.Value, out var ab);
        if (ab < 1) { ab = 0; }
        if (b < 10) { b = 10; }
        if (h < 10) { h = 10; }

        if (padSchachteln.Items != null) {
            padSchachteln.Items.Breite = b;
            padSchachteln.Items.Höhe = h;
            padSchachteln.Items.RandinMm = Padding.Empty;
            padSchachteln.Items.BackColor = Color.Transparent;
        }

        GeneratePrintPad(padSchachteln, 0, cbxLayoutWahl.Text, _rowsForExport, ab);
    }

    private void BefülleLayoutDropdowns() {
        cbxLayoutWahl.ItemClear();
        AddLayoutsOff(cbxLayoutWahl, Table);
    }

    private void btnDrucken_Click(object sender, System.EventArgs e) => padPrint.Print();

    private void btnEinstellung_Click(object sender, System.EventArgs e) {
        switch (MessageBox.Show("Einstellung laden:", ImageCode.Stift, "A4", "A4 Printer", "Abbrechen")) {
            case 0:
                flxBreite.ValueSet("210", true);
                flxHöhe.ValueSet("297", true);
                flxAbstand.ValueSet("0", true);
                break;

            case 1:
                flxBreite.ValueSet("190", true);
                flxHöhe.ValueSet("277", true);
                flxAbstand.ValueSet("1", true);
                break;
        }
    }

    private void btnSchachtelnSpeichern_Click(object sender, System.EventArgs e) {
        if (padSchachteln.Items is not { IsDisposed: false }) { return; }

        FloatTryParse(flxBreite.Value, out var b);
        FloatTryParse(flxHöhe.Value, out var h);
        FloatTryParse(flxAbstand.Value, out var ab);
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

            var x = TempFile(_zielPfad, _rowsForExport[0].Table.Caption + "_" + b + "x" + h + "_" + ab, "png");
            padSchachteln.Items.BackColor = Color.Transparent;
            padSchachteln.ShowInPrintMode = true;
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
        GeneratePrintPad(padPrint, 0, cbxLayoutWahl.Text, _rowsForExport, 0);
    }

    private void Button1_Click(object sender, System.EventArgs e) => ExecuteFile(_zielPfad);

    private void cbxLayoutWahl_TextChanged(object sender, System.EventArgs e) {
        if (Table == null || string.IsNullOrEmpty(cbxLayoutWahl.Text) || _rowsForExport?.Any() != true) {
            padVorschau.Items?.Clear();
        } else {
            padVorschau.ShowInPrintMode = true;
            padVorschau.Items = new ItemCollectionPadItem(cbxLayoutWahl.Text);
            padVorschau.Items.ResetVariables();
            padVorschau.Items.ReplaceVariables(_rowsForExport[0]);
            padVorschau.ZoomFit();
        }
    }

    private void Contextmenu_CopyPath(object sender, ObjectEventArgs e) {
        if (e.Data is not TextListItem tl) { return; }
        var x = new StringCollection { tl.KeyName };
        Clipboard.SetFileDropList(x);
    }

    private void Contextmenu_OpenPath(object sender, ObjectEventArgs e) {
        if (e.Data is not TextListItem tl) { return; }
        ExecuteFile(tl.KeyName.FilePath());
    }

    private void EintragsText() => capAnzahlInfo.Text = _rowsForExport is not { Count: not 0 }
                ? "Bitte wählen sie die Einträge für den Export."
        : _rowsForExport.Count == 1
            ? "Es ist genau ein Eintrag gewählt:<br> <b>-" + _rowsForExport[0].CellFirstString().Replace("\r\n", " ")
            : "Es sind <b>" + _rowsForExport.Count + "</b> Einträge gewählt.";

    private void Exported_ItemClicked(object sender, AbstractListItemEventArgs e) => ExecuteFile(e.Item.KeyName);

    private string Fehler() {
        if (IsDisposed || Table is not { IsDisposed: false }) { return "Tabelle verworfen"; }
        if (_rowsForExport is not { Count: not 0 }) { return "Es sind keine Einträge für den Export gewählt."; }
        if (string.IsNullOrEmpty(cbxLayoutWahl.Text)) { return "Es sind keine Layout für den Export gewählt."; }
        ////TODO: Schachteln implementieren
        //if (!optSpezialFormat.Checked && !optEinzelnSpeichern.Checked) { return "Das gewählte Layout kann nur gespeichtert oder im Spezialformat bearbeitet werden."; }

        return string.Empty;
    }

    private void FrmDrucken_Drucken_Click(object sender, System.EventArgs e) => Close();

    private void LayoutEditor_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return; }

        Enabled = false;
        var n = cbxLayoutWahl.Text;
        cbxLayoutWahl.Text = string.Empty;
        TableViewForm.OpenLayoutEditor(tb, n);
        BefülleLayoutDropdowns();
        if (cbxLayoutWahl[n] != null) {
            cbxLayoutWahl.Text = n;
        }
        Enabled = true;
    }

    private void lstExported_ContextMenuInit(object sender, ContextMenuInitEventArgs e) {
        e.ContextMenu.Add(ItemOf("Dateipfad öffnen", QuickImage.Get(ImageCode.Ordner), Contextmenu_OpenPath, e.HotItem, true, string.Empty));
        e.ContextMenu.Add(ItemOf("Kopieren", QuickImage.Get(ImageCode.Kopieren), Contextmenu_CopyPath, e.HotItem, true, string.Empty));
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

        if (Table is null || _rowsForExport == null) { return; } // wird mit Fehler schon abgedeckt

        if (optBildSchateln.Checked) {
            tabBildSchachteln.Enabled = true;
            Tabs.SelectedTab = tabBildSchachteln;
            Attribute_Changed(null, System.EventArgs.Empty);
        }

        if (optDrucken.Checked) {
            tabDrucken.Enabled = true;
            Tabs.SelectedTab = tabDrucken;
            Button_PageSetup_Click(null, System.EventArgs.Empty);
            GeneratePrintPad(padPrint, 0, cbxLayoutWahl.Text, _rowsForExport, 0);
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
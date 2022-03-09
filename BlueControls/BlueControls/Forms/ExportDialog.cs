// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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
using BlueControls.BlueDatabaseDialogs;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.ItemCollection;
using BlueDatabase;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Windows.Forms;
using BlueControls.ItemCollection.ItemCollectionList;
using static BlueBasics.Converter;
using static BlueBasics.FileOperations;

namespace BlueControls.Forms {

    public sealed partial class ExportDialog {

        #region Fields

        private readonly List<RowItem>? _rowsForExport;
        private readonly string _saveTo = "";
        private readonly string _zielPfad;
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
            Database.Disposing += _Database_Disposing;
            _rowsForExport = rows;
            if (!string.IsNullOrEmpty(autosaveFile)) {
                _zielPfad = autosaveFile.FilePath();
                _saveTo = autosaveFile;
            } else {
                _zielPfad = Path.GetTempPath();
            }
            try {
                if (!PathExists(_zielPfad)) {
                    Directory.CreateDirectory(_zielPfad);
                }
            } catch { }

            BefülleLayoutDropdowns();
            EintragsText();
            NurStartEnablen();
        }

        #endregion

        #region Properties

        public Database? Database { get; private set; }

        #endregion

        // Nullable wegen Dispose

        #region Methods

        public static void AddLayoutsOff(ItemCollectionList addHere, Database database, bool addDiskLayouts) {
            if (database != null) {
                foreach (var t in database.Layouts) {
                    ItemCollectionPad p = new(t, string.Empty);
                    addHere.Add(p.Caption, p.Id, enImageCode.Stern);
                }
            }
            if (!addDiskLayouts) { return; }
            List<string> path = new();
            if (database != null) { path.Add(database.DefaultLayoutPath()); }
            if (!string.IsNullOrEmpty(database.AdditionaFilesPfadWhole())) { path.Add(database.AdditionaFilesPfadWhole()); }
            foreach (var thisP in path) {
                if (PathExists(thisP)) {
                    var e = Directory.GetFiles(thisP);
                    foreach (var thisFile in e) {
                        if (thisFile.FileType() is enFileFormat.HTML or enFileFormat.Textdocument or enFileFormat.Visitenkarte or enFileFormat.BlueCreativeFile or enFileFormat.XMLFile) {
                            if (addHere[thisFile] == null) { addHere.Add(thisFile.FileNameWithSuffix(), thisFile, QuickImage.Get(thisFile.FileType(), 16)); }
                        }
                    }
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="pad"></param>
        /// <param name="startNr"></param>
        /// <param name="layout"></param>
        /// <param name="rowsForExport"></param>
        /// <returns>Gibt das Item zurück, dass nicht mehr auf den Druckbereich gepasst hat. -1 falls keine (gültige) Liste übergeben wurde.</returns>
        public static int GeneratePrintPad(CreativePad pad, int startNr, string layout, List<RowItem>? rowsForExport, float abstandMm) {
            pad.Item.Clear();
            Generic.CollectGarbage();

            if (rowsForExport == null || rowsForExport.Count < 1) { return -1; }

            CreativePad tmp = new(new ItemCollectionPad(layout, rowsForExport[0].Database, rowsForExport[0].Key));
            var oneItem = tmp.Item.MaxBounds(null);
            pad.Item.SheetStyle = tmp.Item.SheetStyle;
            pad.Item.SheetStyleScale = tmp.Item.SheetStyleScale;
            tmp.Dispose();
            var druckB = pad.Item.DruckbereichRect();
            var abstand = (float)Math.Round(MmToPixel(abstandMm, ItemCollectionPad.Dpi), 1);
            var tempVar = Math.Max(1, (int)Math.Floor((druckB.Width / (double)(oneItem.Width + abstand)) + 0.01));
            for (var x = 0; x < tempVar; x++) {
                var tempVar2 = Math.Max(1, (int)Math.Floor((druckB.Height / (double)(oneItem.Height + abstand)) + 0.01));
                for (var y = 0; y < tempVar2; y++) {
                    ChildPadItem it = new() {
                        PadInternal = new CreativePad(new ItemCollectionPad(layout, rowsForExport[startNr].Database, rowsForExport[startNr].Key))
                    };
                    pad.Item.Add(it);
                    it.SetCoordinates(new RectangleF(druckB.Left + (x * (oneItem.Width + abstand)), druckB.Top + (y * (oneItem.Height + abstand)), oneItem.Width, oneItem.Height), true);
                    startNr++;
                    if (startNr >= rowsForExport.Count) { break; }
                }
                if (startNr >= rowsForExport.Count) { break; }
            }
            pad.ZoomFit();
            return startNr;
        }

        protected override void OnFormClosing(FormClosingEventArgs e) {
            if (Database != null) {
                Database.Disposing -= _Database_Disposing;
                Database = null;
            }
            base.OnFormClosing(e);
        }

        private void _Database_Disposing(object sender, System.EventArgs e) => Close();

        private void Attribute_Changed(object? sender, System.EventArgs? e) {
            var b = FloatParse(flxBreite.Value);
            var h = FloatParse(flxHöhe.Value);
            var ab = FloatParse(flxAbstand.Value);
            if (ab < 1) { ab = 0; }
            if (b < 10) { b = 10; }
            if (h < 10) { h = 10; }
            padSchachteln.Item.SheetSizeInMm = new SizeF(b, h);
            padSchachteln.Item.RandinMm = Padding.Empty;
            padSchachteln.Item.BackColor = Color.Transparent;
            GeneratePrintPad(padSchachteln, 0, cbxLayoutWahl.Text, _rowsForExport, ab);
        }

        private void BefülleLayoutDropdowns() {
            cbxLayoutWahl.Item.Clear();
            AddLayoutsOff(cbxLayoutWahl.Item, Database, true);
        }

        private void btnDrucken_Click(object sender, System.EventArgs e) => padPrint.Print();

        private void btnEinstellung_Click(object sender, System.EventArgs e) {
            switch (MessageBox.Show("Einstellung laden:", enImageCode.Stift, "A4", "Cricut Maker", "Abbrechen")) {
                case 0:
                    flxBreite.ValueSet("210", true, false);
                    flxHöhe.ValueSet("297", true, false);
                    flxAbstand.ValueSet("0", true, false);
                    break;

                case 1:
                    flxBreite.ValueSet("171,1", true, false);
                    flxHöhe.ValueSet("234,9", true, false);
                    flxAbstand.ValueSet("2", true, false);
                    break;
            }
        }

        private void btnSchachtelnSpeichern_Click(object sender, System.EventArgs e) {
            var b = FloatParse(flxBreite.Value);
            var h = FloatParse(flxHöhe.Value);
            var ab = FloatParse(flxAbstand.Value);
            if (ab < 1) { ab = 0; }
            if (b < 10) { b = 10; }
            if (h < 10) { h = 10; }
            padSchachteln.Item.SheetSizeInMm = new SizeF(b, h);
            padSchachteln.Item.RandinMm = Padding.Empty;
            List<string> l = new();
            _itemNrForPrint = 0;
            do {
                var nr = _itemNrForPrint;
                _itemNrForPrint = GeneratePrintPad(padSchachteln, _itemNrForPrint, cbxLayoutWahl.Text, _rowsForExport, ab);

                var x = TempFile(_zielPfad, _rowsForExport[0].Database.Caption + "_" + b + "x" + h + "_" + ab, "png");
                padSchachteln.Item.BackColor = Color.Transparent;
                padSchachteln.Item.SaveAsBitmap(x);
                l.Add(x);
                if (nr == _itemNrForPrint) { break; }
                if (_itemNrForPrint >= _rowsForExport.Count) { break; }
            } while (true);
            tabStart.Enabled = false;
            tabBildSchachteln.Enabled = false;
            tabDateiExport.Enabled = true;
            Tabs.SelectedTab = tabDateiExport;
            lstExported.Item.AddRange(l);
        }

        private void Button_PageSetup_Click(object? sender, System.EventArgs e) {
            padPrint.ShowPrinterPageSetup();
            padPrint.CopyPrinterSettingsToWorkingArea();
            GeneratePrintPad(padPrint, 0, cbxLayoutWahl.Text, _rowsForExport, 0);
        }

        private void Button1_Click(object sender, System.EventArgs e) => ExecuteFile(_zielPfad);

        private void cbxLayoutWahl_TextChanged(object sender, System.EventArgs e) {
            if (Database.Layouts.LayoutIDToIndex(cbxLayoutWahl.Text) > -1) {
                padVorschau.ShowInPrintMode = true;
                padVorschau.Item = new ItemCollectionPad(cbxLayoutWahl.Text, _rowsForExport[0].Database, _rowsForExport[0].Key);
                padVorschau.ZoomFit();
            } else {
                padVorschau.Item.Clear();
            }
        }

        private void EintragsText() => capAnzahlInfo.Text = _rowsForExport == null || _rowsForExport.Count == 0
? "Bitte wählen sie die Einträge für den Export."
: _rowsForExport.Count == 1
? "Es ist genau ein Eintrag gewählt:<br> <b>-" + _rowsForExport[0].CellFirstString().Replace("\r\n", " ")
: "Es sind <b>" + _rowsForExport.Count + "</b> Einträge gewählt.";

        private void Exported_ItemClicked(object sender, BasicListItemEventArgs e) => ExecuteFile(e.Item.Internal);

        private string Fehler() {
            if (_rowsForExport == null || _rowsForExport.Count == 0) { return "Es sind keine Einträge für den Export gewählt."; }
            if (string.IsNullOrEmpty(cbxLayoutWahl.Text)) { return "Es sind keine Layout für den Export gewählt."; }
            if (Database.Layouts.LayoutIDToIndex(cbxLayoutWahl.Text) > -1) {
                if (!optBildSchateln.Checked && !optDrucken.Checked && !optSpeichern.Checked) { return "Das gewählte Layout kann nur gedruckt, geschachtelt oder gespeichtert werden."; }
            } else {
                if (!optSpezialFormat.Checked && !optSpeichern.Checked) { return "Das gewählte Layout kann nur gespeichtert oder im Spezialformat bearbeitet werden."; }
            }
            return string.Empty;
        }

        private void FrmDrucken_Drucken_Click(object sender, System.EventArgs e) => Close();

        private void LayoutEditor_Click(object sender, System.EventArgs e) {
            Enabled = false;
            var n = cbxLayoutWahl.Text;
            cbxLayoutWahl.Text = string.Empty;
            TabAdministration.OpenLayoutEditor(Database, n);
            BefülleLayoutDropdowns();
            if (cbxLayoutWahl.Item[n] != null) {
                cbxLayoutWahl.Text = n;
            }
            Enabled = true;
        }

        private void lstExported_ContextMenuInit(object sender, ContextMenuInitEventArgs e) {
            e.UserMenu.Add(enContextMenuComands.DateiPfadÖffnen);
            e.UserMenu.Add(enContextMenuComands.Kopieren);
        }

        private void lstExported_ContextMenuItemClicked(object sender, ContextMenuItemClickedEventArgs e) {
            if (e.HotItem is not TextListItem tl) { return; }

            switch (e.ClickedComand) {
                case "DateiPfadÖffnen":
                    ExecuteFile(tl.Internal.FilePath());
                    break;

                case "Kopieren":
                    var x = new System.Collections.Specialized.StringCollection {
                        tl.Internal
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
            e.HasMorePages = Convert.ToBoolean(_itemNrForPrint < _rowsForExport.Count);
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
            if (optSpeichern.Checked || optSpezialFormat.Checked) {
                tabStart.Enabled = false; // Geht ja gleich los
                tabDateiExport.Enabled = true;
                Tabs.SelectedTab = tabDateiExport;
                var l = Database.Layouts.LayoutIDToIndex(cbxLayoutWahl.Text) > -1
                    ? Export.SaveAsBitmap(_rowsForExport, cbxLayoutWahl.Text, _zielPfad)
                    : Export.GenerateLayout_FileSystem(_rowsForExport, cbxLayoutWahl.Text, _saveTo, optSpezialFormat.Checked, _zielPfad);
                lstExported.Item.AddRange(l);
            }
        }

        #endregion
    }
}
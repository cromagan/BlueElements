#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2021 Christian Peter
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
#endregion

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.BlueDatabaseDialogs;
using BlueControls.Controls;
using BlueControls.EventArgs;
using BlueControls.ItemCollection;
using BlueDatabase;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Windows.Forms;
using static BlueBasics.FileOperations;
using static BlueBasics.modConverter;

namespace BlueControls.Forms {
    public sealed partial class ExportDialog {

        private readonly string _AdditionalLayoutPath = "";
        private readonly List<RowItem> _RowsForExport;
        private Database _Database;
        private readonly string _ZielPfad = "";
        private readonly string _SaveTo = "";

        private int _ItemNrForPrint;

        public ExportDialog(Database db, string additionalLayoutPath, string autosaveFile) : this(db, null, additionalLayoutPath, autosaveFile) { }
        public ExportDialog(Database db, List<RowItem> rows) : this(db, rows, string.Empty, string.Empty) { }
        public ExportDialog(Database db, List<RowItem> rows, string additionalLayoutPath, string autosaveFile) {

            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();

            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
            _Database = db;
            _Database.Disposing += _Database_Disposing;
            _RowsForExport = rows;

            _AdditionalLayoutPath = additionalLayoutPath;

            if (!string.IsNullOrEmpty(autosaveFile)) {
                _ZielPfad = autosaveFile.FilePath();
                _SaveTo = autosaveFile;
            } else {
                _ZielPfad = Path.GetTempPath();
            }

            try {
                if (!string.IsNullOrEmpty(_AdditionalLayoutPath) && !PathExists(_AdditionalLayoutPath)) {
                    Directory.CreateDirectory(_AdditionalLayoutPath);
                }
                if (!PathExists(_ZielPfad)) {
                    Directory.CreateDirectory(_ZielPfad);
                }
            } catch (Exception) {

            }

            BefülleLayoutDropdowns();
            EintragsText();

            NurStartEnablen();

        }

        private void NurStartEnablen() {

            tabStart.Enabled = true;
            tabDrucken.Enabled = false;
            tabDateiExport.Enabled = false;
            tabBildSchachteln.Enabled = false;

        }

        private void _Database_Disposing(object sender, System.EventArgs e) {
            Close();
        }

        private string Fehler() {

            if (_RowsForExport == null || _RowsForExport.Count == 0) { return "Es sind keine Einträge für den Export gewählt."; }

            if (string.IsNullOrEmpty(cbxLayoutWahl.Text)) { return "Es sind keine Layout für den Export gewählt."; }

            if (_Database.Layouts.LayoutIDToIndex(cbxLayoutWahl.Text) > -1) {
                if (!optBildSchateln.Checked && !optDrucken.Checked && !optSpeichern.Checked) { return "Das gewählte Layout kann nur gedruckt, geschachtelt oder gespeichtert werden."; }
            } else {
                if (!optSpezialFormat.Checked && !optSpeichern.Checked) { return "Das gewählte Layout kann nur gespeichtert oder im Spezialformat bearbeitet werden."; }
            }

            return string.Empty;
        }

        private void EintragsText() {
            capAnzahlInfo.Text = _RowsForExport == null || _RowsForExport.Count == 0
                ? "Bitte wählen sie die Einträge für den Export."
                : _RowsForExport.Count == 1
                    ? "Es ist genau ein Eintrag gewählt:<br> <b>-" + _RowsForExport[0].CellFirstString().Replace("\r\n", " ")
                    : "Es sind <b>" + _RowsForExport.Count + "</b> Einträge gewählt.";
        }

        public static void AddLayoutsOff(ItemCollectionList addHere, Database database, bool addDiskLayouts, string additionalLayoutPath) {

            if (database != null) {
                for (var z = 0; z < database.Layouts.Count; z++) {
                    var p = new ItemCollectionPad(database.Layouts[z], string.Empty);
                    addHere.Add(p.Caption, p.ID, enImageCode.Stern);
                }
            }

            if (!addDiskLayouts) { return; }

            var path = new List<string>();
            if (database != null) { path.Add(database.DefaultLayoutPath()); }
            if (!string.IsNullOrEmpty(additionalLayoutPath)) { path.Add(additionalLayoutPath); }

            foreach (var thisP in path) {
                if (PathExists(thisP)) {
                    var e = Directory.GetFiles(thisP);
                    foreach (var ThisFile in e) {
                        //if (ThisFile.FilePath() == database.DefaultLayoutPath()) { ThisFile.TrimStart(database.DefaultLayoutPath()); }
                        if (addHere[ThisFile] == null) { addHere.Add(ThisFile.FileNameWithSuffix(), ThisFile, QuickImage.Get(ThisFile.FileType(), 16)); }
                    }
                }
            }
        }

        private void BefülleLayoutDropdowns() {
            cbxLayoutWahl.Item.Clear();
            ExportDialog.AddLayoutsOff(cbxLayoutWahl.Item, _Database, true, _AdditionalLayoutPath);
        }

        private void LayoutEditor_Click(object sender, System.EventArgs e) {

            Enabled = false;

            var n = cbxLayoutWahl.Text;

            cbxLayoutWahl.Text = string.Empty;
            tabAdministration.OpenLayoutEditor(_Database, _AdditionalLayoutPath, n);

            BefülleLayoutDropdowns();

            if (cbxLayoutWahl.Item[n] != null) {
                cbxLayoutWahl.Text = n;
            }

            Enabled = true;
        }

        private void Button1_Click(object sender, System.EventArgs e) {
            ExecuteFile(_ZielPfad);
        }

        private void cbxLayoutWahl_TextChanged(object sender, System.EventArgs e) {

            if (_Database.Layouts.LayoutIDToIndex(cbxLayoutWahl.Text) > -1) {

                padVorschau.ShowInPrintMode = true;
                padVorschau.Item = new ItemCollectionPad(cbxLayoutWahl.Text, _RowsForExport[0].Database, _RowsForExport[0].Key);

                padVorschau.ZoomFit();
            } else {
                padVorschau.Item.Clear();
            }
        }

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
                GeneratePrintPad(padPrint, 0, cbxLayoutWahl.Text, _RowsForExport, 0);
            }

            if (optSpeichern.Checked || optSpezialFormat.Checked) {
                tabStart.Enabled = false; // Geht ja gleich los
                tabDateiExport.Enabled = true;
                Tabs.SelectedTab = tabDateiExport;

                var l = _Database.Layouts.LayoutIDToIndex(cbxLayoutWahl.Text) > -1
                    ? Export.SaveAsBitmap(_RowsForExport, cbxLayoutWahl.Text, _ZielPfad)
                    : Export.GenerateLayout_FileSystem(_RowsForExport, cbxLayoutWahl.Text, _SaveTo, optSpezialFormat.Checked, _ZielPfad);
                Exported.Item.AddRange(l);
            }
        }

        private void Exported_ItemClicked(object sender, BasicListItemEventArgs e) {
            ExecuteFile(e.Item.Internal);
        }

        private void FrmDrucken_Drucken_Click(object sender, System.EventArgs e) {
            Close();
        }

        private void Button_PageSetup_Click(object sender, System.EventArgs e) {
            padPrint.ShowPrinterPageSetup();

            padPrint.CopyPrinterSettingsToWorkingArea();

            GeneratePrintPad(padPrint, 0, cbxLayoutWahl.Text, _RowsForExport, 0);
        }

        private void btnDrucken_Click(object sender, System.EventArgs e) {
            padPrint.Print();
            // Den Rest mach 'PrintPad.PrintPage'
        }

        private void PrintPad_PrintPage(object sender, PrintPageEventArgs e) {

            var l = _ItemNrForPrint;

            _ItemNrForPrint = GeneratePrintPad(padPrint, _ItemNrForPrint, cbxLayoutWahl.Text, _RowsForExport, 0);

            if (l == _ItemNrForPrint) { return; }

            e.HasMorePages = Convert.ToBoolean(_ItemNrForPrint < _RowsForExport.Count);
        }

        private void Vorschau_Click(object sender, System.EventArgs e) {
            padPrint.ShowPrintPreview();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pad"></param>
        /// <param name="startNr"></param>
        /// <param name="layout"></param>
        /// <param name="rowsForExport"></param>
        /// <returns>Gibt das Item zurück, dass nicht mehr auf den Druckbereich gepasst hat</returns>
        public static int GeneratePrintPad(CreativePad pad, int startNr, string layout, List<RowItem> rowsForExport, float abstandMM) {
            pad.Item.Clear();
            modAllgemein.CollectGarbage();

            var tmp = new CreativePad(new ItemCollectionPad(layout, rowsForExport[0].Database, rowsForExport[0].Key));

            var OneItem = tmp.Item.MaxBounds(null);

            pad.Item.SheetStyle = tmp.Item.SheetStyle;
            pad.Item.SheetStyleScale = tmp.Item.SheetStyleScale;

            tmp.Dispose();

            var DruckB = pad.Item.DruckbereichRect();

            var abstand = Math.Round(modConverter.mmToPixel((decimal)abstandMM, ItemCollectionPad.DPI), 1);

            var tempVar = Math.Max(1, (int)Math.Floor((DruckB.Width / (double)(OneItem.Width + abstand)) + 0.01));
            for (var x = 0; x < tempVar; x++) {
                var tempVar2 = Math.Max(1, (int)Math.Floor((DruckB.Height / (double)(OneItem.Height + abstand)) + 0.01));
                for (var y = 0; y < tempVar2; y++) {

                    var It = new ChildPadItem(pad.Item) {
                        PadInternal = new CreativePad(new ItemCollectionPad(layout, rowsForExport[startNr].Database, rowsForExport[startNr].Key))
                    };
                    pad.Item.Add(It);
                    It.SetCoordinates(new RectangleM(DruckB.Left + (x * (OneItem.Width + abstand)), DruckB.Top + (y * (OneItem.Height + abstand)), OneItem.Width, OneItem.Height), true);

                    startNr++;

                    if (startNr >= rowsForExport.Count) { break; }
                }

                if (startNr >= rowsForExport.Count) { break; }
            }

            pad.ZoomFit();

            return startNr;
        }

        private void PrintPad_BeginnPrint(object sender, PrintEventArgs e) {
            _ItemNrForPrint = 0;
        }

        protected override void OnFormClosing(FormClosingEventArgs e) {

            if (_Database != null) {
                _Database.Disposing -= _Database_Disposing;
                _Database = null;
            }
            base.OnFormClosing(e);
        }

        private void Tabs_SelectedIndexChanged(object sender, System.EventArgs e) {

            if (Tabs.SelectedTab == tabStart && tabStart.Enabled) {
                NurStartEnablen();
            }
        }

        private void Attribute_Changed(object sender, System.EventArgs e) {

            var b = FloatParse(flxBreite.Value);
            var h = FloatParse(flxHöhe.Value);
            var ab = FloatParse(flxAbstand.Value);
            if (ab < 1) { ab = 0; }
            if (b < 10) { b = 10; }
            if (h < 10) { h = 10; }

            padSchachteln.Item.SheetSizeInMM = new System.Drawing.SizeF(b, h);
            padSchachteln.Item.RandinMM = Padding.Empty;

            GeneratePrintPad(padSchachteln, 0, cbxLayoutWahl.Text, _RowsForExport, ab);

        }

        private void btnSchachtelnSpeichern_Click(object sender, System.EventArgs e) {
            var b = FloatParse(flxBreite.Value);
            var h = FloatParse(flxHöhe.Value);
            var ab = FloatParse(flxAbstand.Value);
            if (ab < 1) { ab = 0; }
            if (b < 10) { b = 10; }
            if (h < 10) { h = 10; }

            padSchachteln.Item.SheetSizeInMM = new System.Drawing.SizeF(b, h);
            padSchachteln.Item.RandinMM = Padding.Empty;

            List<string> l = new();
            _ItemNrForPrint = 0;

            do {
                var nr = _ItemNrForPrint;

                _ItemNrForPrint = GeneratePrintPad(padSchachteln, _ItemNrForPrint, cbxLayoutWahl.Text, _RowsForExport, ab);
                if (nr == _ItemNrForPrint) { break; }

                if (_ItemNrForPrint >= _RowsForExport.Count) { break; }

                var x = TempFile(_ZielPfad, "Schachteln", "png");
                padSchachteln.Item.BackColor = System.Drawing.Color.Transparent;
                padSchachteln.Item.SaveAsBitmap(x);

                l.Add(x);

            } while (true);

            tabStart.Enabled = false;
            tabBildSchachteln.Enabled = false;
            tabDateiExport.Enabled = true;
            Tabs.SelectedTab = tabDateiExport;

            Exported.Item.AddRange(l);
        }
    }
}
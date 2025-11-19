using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueTable.Enums;
using Button = BlueControls.Controls.Button;
using ComboBox = BlueControls.Controls.ComboBox;
using GroupBox = BlueControls.Controls.GroupBox;
using ListBox = BlueControls.Controls.ListBox;
using TabControl = BlueControls.Controls.TabControl;
using TabPage = System.Windows.Forms.TabPage;

namespace BlueControls.Forms {
    public sealed partial class ExportDialog : Form {
        //Das Formular überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
        [DebuggerNonUserCode()]
        protected override void Dispose(bool disposing) {
            try {
                if (disposing) {
                }
            } finally {
                base.Dispose(disposing);
            }
        }
        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent() {
            this.capAnzahlInfo = new Caption();
            this.cbxLayoutWahl = new ComboBox();
            this.c_Layoutx = new Caption();
            this.Caption3 = new Caption();
            this.btnMachZu = new Button();
            this.btnLayoutEditorÖffnen = new Button();
            this.btnDrucken_ExportVerzeichnis = new Button();
            this.capLayout = new Caption();
            this.padVorschau = new CreativePad();
            this.Tabs = new TabControl();
            this.tabStart = new TabPage();
            this.grpArt = new GroupBox();
            this.optSpezialFormat = new Button();
            this.optBildSchateln = new Button();
            this.optEinzelnSpeichern = new Button();
            this.optDrucken = new Button();
            this.btnWeiter = new Button();
            this.grpEinträge = new GroupBox();
            this.tabDrucken = new TabPage();
            this.Vorschau = new Button();
            this.btnDrucken = new Button();
            this.Button_PageSetup = new Button();
            this.padPrint = new CreativePad();
            this.tabBildSchachteln = new TabPage();
            this.btnEinstellung = new Button();
            this.capDpi = new Caption();
            this.flxAbstand = new FlexiControl();
            this.flxHöhe = new FlexiControl();
            this.flxBreite = new FlexiControl();
            this.btnSchachtelnSpeichern = new Button();
            this.padSchachteln = new CreativePad();
            this.tabDateiExport = new TabPage();
            this.Caption4 = new Caption();
            this.lstExported = new ListBox();
            this.Tabs.SuspendLayout();
            this.tabStart.SuspendLayout();
            this.grpArt.SuspendLayout();
            this.grpEinträge.SuspendLayout();
            this.tabDrucken.SuspendLayout();
            this.tabBildSchachteln.SuspendLayout();
            this.tabDateiExport.SuspendLayout();
            this.SuspendLayout();
            // 
            // capAnzahlInfo
            // 
            this.capAnzahlInfo.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left)
                                                         | AnchorStyles.Right)));
            this.capAnzahlInfo.CausesValidation = false;
            this.capAnzahlInfo.Location = new Point(8, 16);
            this.capAnzahlInfo.Name = "capAnzahlInfo";
            this.capAnzahlInfo.Size = new Size(847, 40);
            // 
            // cbxLayoutWahl
            // 
            this.cbxLayoutWahl.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left)
                                                         | AnchorStyles.Right)));
            this.cbxLayoutWahl.Cursor = Cursors.IBeam;
            this.cbxLayoutWahl.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cbxLayoutWahl.Location = new Point(232, 48);
            this.cbxLayoutWahl.Name = "cbxLayoutWahl";
            this.cbxLayoutWahl.Size = new Size(623, 24);
            this.cbxLayoutWahl.TabIndex = 80;
            this.cbxLayoutWahl.TextChanged += new EventHandler(this.cbxLayoutWahl_TextChanged);
            // 
            // c_Layoutx
            // 
            this.c_Layoutx.CausesValidation = false;
            this.c_Layoutx.Location = new Point(-86, 51);
            this.c_Layoutx.Name = "c_Layoutx";
            this.c_Layoutx.Size = new Size(80, 16);
            this.c_Layoutx.Text = "c_Layoutx";
            // 
            // Caption3
            // 
            this.Caption3.CausesValidation = false;
            this.Caption3.Location = new Point(512, -208);
            this.Caption3.Name = "Caption3";
            this.Caption3.Size = new Size(46, 20);
            this.Caption3.Text = "Layout:";
            // 
            // btnMachZu
            // 
            this.btnMachZu.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
            this.btnMachZu.ImageCode = "Häkchen|16";
            this.btnMachZu.Location = new Point(743, 641);
            this.btnMachZu.Name = "btnMachZu";
            this.btnMachZu.Size = new Size(112, 40);
            this.btnMachZu.TabIndex = 85;
            this.btnMachZu.Text = "Beenden";
            this.btnMachZu.Click += new EventHandler(this.FrmDrucken_Drucken_Click);
            // 
            // btnLayoutEditorÖffnen
            // 
            this.btnLayoutEditorÖffnen.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Right)));
            this.btnLayoutEditorÖffnen.ImageCode = "Layout|16";
            this.btnLayoutEditorÖffnen.Location = new Point(704, 16);
            this.btnLayoutEditorÖffnen.Name = "btnLayoutEditorÖffnen";
            this.btnLayoutEditorÖffnen.Size = new Size(152, 32);
            this.btnLayoutEditorÖffnen.TabIndex = 86;
            this.btnLayoutEditorÖffnen.Text = "Layout bearbeiten";
            this.btnLayoutEditorÖffnen.Click += new EventHandler(this.LayoutEditor_Click);
            // 
            // btnDrucken_ExportVerzeichnis
            // 
            this.btnDrucken_ExportVerzeichnis.ImageCode = "Ordner|16";
            this.btnDrucken_ExportVerzeichnis.Location = new Point(8, 448);
            this.btnDrucken_ExportVerzeichnis.Name = "btnDrucken_ExportVerzeichnis";
            this.btnDrucken_ExportVerzeichnis.Size = new Size(208, 40);
            this.btnDrucken_ExportVerzeichnis.TabIndex = 87;
            this.btnDrucken_ExportVerzeichnis.Text = "Export Verzeichnis öffnen";
            this.btnDrucken_ExportVerzeichnis.Click += new EventHandler(this.Button1_Click);
            // 
            // capLayout
            // 
            this.capLayout.CausesValidation = false;
            this.capLayout.Location = new Point(232, 24);
            this.capLayout.Name = "capLayout";
            this.capLayout.Size = new Size(82, 24);
            this.capLayout.Text = "Layout:";
            // 
            // padVorschau
            // 
            this.padVorschau.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom)
                                                        | AnchorStyles.Left)
                                                       | AnchorStyles.Right)));
            this.padVorschau.EditAllowed = false;
            this.padVorschau.Location = new Point(232, 80);
            this.padVorschau.Name = "padVorschau";
            this.padVorschau.ShiftX = 0F;
            this.padVorschau.ShiftY = 0F;
            this.padVorschau.ShowInPrintMode = true;
            this.padVorschau.Size = new Size(623, 480);
            this.padVorschau.TabIndex = 1;
            this.padVorschau.Zoom = 1F;
            // 
            // Tabs
            // 
            this.Tabs.Controls.Add(this.tabStart);
            this.Tabs.Controls.Add(this.tabDrucken);
            this.Tabs.Controls.Add(this.tabBildSchachteln);
            this.Tabs.Controls.Add(this.tabDateiExport);
            this.Tabs.Dock = DockStyle.Fill;
            this.Tabs.HotTrack = true;
            this.Tabs.Location = new Point(0, 0);
            this.Tabs.Name = "Tabs";
            this.Tabs.SelectedIndex = 0;
            this.Tabs.Size = new Size(868, 716);
            this.Tabs.TabDefault = null;
            this.Tabs.TabDefaultOrder = new string[0];
            this.Tabs.TabIndex = 81;
            this.Tabs.SelectedIndexChanged += new EventHandler(this.Tabs_SelectedIndexChanged);
            // 
            // tabStart
            // 
            this.tabStart.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabStart.Controls.Add(this.grpArt);
            this.tabStart.Controls.Add(this.btnWeiter);
            this.tabStart.Controls.Add(this.grpEinträge);
            this.tabStart.Location = new Point(4, 25);
            this.tabStart.Name = "tabStart";
            this.tabStart.Size = new Size(860, 687);
            this.tabStart.TabIndex = 3;
            this.tabStart.Text = "Start";
            // 
            // grpArt
            // 
            this.grpArt.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom)
                                                   | AnchorStyles.Left)
                                                  | AnchorStyles.Right)));
            this.grpArt.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpArt.Controls.Add(this.padVorschau);
            this.grpArt.Controls.Add(this.cbxLayoutWahl);
            this.grpArt.Controls.Add(this.btnLayoutEditorÖffnen);
            this.grpArt.Controls.Add(this.optSpezialFormat);
            this.grpArt.Controls.Add(this.optBildSchateln);
            this.grpArt.Controls.Add(this.capLayout);
            this.grpArt.Controls.Add(this.optEinzelnSpeichern);
            this.grpArt.Controls.Add(this.optDrucken);
            this.grpArt.Location = new Point(0, 64);
            this.grpArt.Name = "grpArt";
            this.grpArt.Size = new Size(860, 568);
            this.grpArt.TabIndex = 89;
            this.grpArt.TabStop = false;
            this.grpArt.Text = "Art des Exportes";
            // 
            // optSpezialFormat
            // 
            this.optSpezialFormat.ButtonStyle = ButtonStyle.Optionbox_Text;
            this.optSpezialFormat.ImageCode = "Diskette";
            this.optSpezialFormat.Location = new Point(8, 168);
            this.optSpezialFormat.Name = "optSpezialFormat";
            this.optSpezialFormat.Size = new Size(216, 96);
            this.optSpezialFormat.TabIndex = 88;
            this.optSpezialFormat.Text = "<b>Spezial-Dateiformat</b><br>Das Vorlagen Layout enthält einen speziellen Code, " +
    "so dass alle Einträge in eine Datei geschrieben werden";
            // 
            // optBildSchateln
            // 
            this.optBildSchateln.ButtonStyle = ButtonStyle.Optionbox_Text;
            this.optBildSchateln.ImageCode = "Diskette";
            this.optBildSchateln.Location = new Point(8, 120);
            this.optBildSchateln.Name = "optBildSchateln";
            this.optBildSchateln.Size = new Size(216, 40);
            this.optBildSchateln.TabIndex = 87;
            this.optBildSchateln.Text = "<b>Als Bild speichern</b><br>Einträge auf einem Bild schachteln";
            // 
            // optSpeichern
            // 
            this.optEinzelnSpeichern.ButtonStyle = ButtonStyle.Optionbox_Text;
            this.optEinzelnSpeichern.Checked = true;
            this.optEinzelnSpeichern.ImageCode = "Diskette";
            this.optEinzelnSpeichern.Location = new Point(8, 24);
            this.optEinzelnSpeichern.Name = "optEinzelnSpeichern";
            this.optEinzelnSpeichern.Size = new Size(216, 40);
            this.optEinzelnSpeichern.TabIndex = 86;
            this.optEinzelnSpeichern.Text = "<b>Einzeln Speichern</b><br>Auf einem Datenträger schreiben";
            // 
            // optDrucken
            // 
            this.optDrucken.ButtonStyle = ButtonStyle.Optionbox_Text;
            this.optDrucken.ImageCode = "Drucker";
            this.optDrucken.Location = new Point(8, 72);
            this.optDrucken.Name = "optDrucken";
            this.optDrucken.Size = new Size(216, 40);
            this.optDrucken.TabIndex = 85;
            this.optDrucken.Text = "<b>Drucken</b><br>Auf einem Drucker ausgeben";
            // 
            // btnWeiter
            // 
            this.btnWeiter.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
            this.btnWeiter.ImageCode = "Pfeil_Rechts|24";
            this.btnWeiter.Location = new Point(711, 640);
            this.btnWeiter.Name = "btnWeiter";
            this.btnWeiter.Size = new Size(144, 41);
            this.btnWeiter.TabIndex = 88;
            this.btnWeiter.Text = "Weiter";
            this.btnWeiter.Click += new EventHandler(this.WeiterAktion_Click);
            // 
            // grpEinträge
            // 
            this.grpEinträge.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left)
                                                       | AnchorStyles.Right)));
            this.grpEinträge.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpEinträge.Controls.Add(this.capAnzahlInfo);
            this.grpEinträge.Location = new Point(0, 0);
            this.grpEinträge.Name = "grpEinträge";
            this.grpEinträge.Size = new Size(860, 64);
            this.grpEinträge.TabIndex = 91;
            this.grpEinträge.TabStop = false;
            this.grpEinträge.Text = "Einträge";
            // 
            // tabDrucken
            // 
            this.tabDrucken.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabDrucken.Controls.Add(this.Vorschau);
            this.tabDrucken.Controls.Add(this.btnDrucken);
            this.tabDrucken.Controls.Add(this.Button_PageSetup);
            this.tabDrucken.Controls.Add(this.padPrint);
            this.tabDrucken.Location = new Point(4, 25);
            this.tabDrucken.Name = "tabDrucken";
            this.tabDrucken.Size = new Size(860, 687);
            this.tabDrucken.TabIndex = 4;
            this.tabDrucken.Text = "Drucken";
            // 
            // Vorschau
            // 
            this.Vorschau.ImageCode = "Datei|36|||||||||Lupe";
            this.Vorschau.Location = new Point(184, 8);
            this.Vorschau.Name = "Vorschau";
            this.Vorschau.Size = new Size(168, 48);
            this.Vorschau.TabIndex = 15;
            this.Vorschau.Text = "btnVorschau";
            this.Vorschau.Click += new EventHandler(this.Vorschau_Click);
            // 
            // btnDrucken
            // 
            this.btnDrucken.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
            this.btnDrucken.ImageCode = "Drucker|24";
            this.btnDrucken.Location = new Point(743, 633);
            this.btnDrucken.Name = "btnDrucken";
            this.btnDrucken.QuickInfo = "Öffnet den Drucker-Dialog.";
            this.btnDrucken.Size = new Size(112, 48);
            this.btnDrucken.TabIndex = 14;
            this.btnDrucken.Text = "Drucken";
            this.btnDrucken.Click += new EventHandler(this.btnDrucken_Click);
            // 
            // Button_PageSetup
            // 
            this.Button_PageSetup.ImageCode = "SeiteEinrichten|36";
            this.Button_PageSetup.Location = new Point(8, 8);
            this.Button_PageSetup.Name = "Button_PageSetup";
            this.Button_PageSetup.Size = new Size(168, 48);
            this.Button_PageSetup.TabIndex = 13;
            this.Button_PageSetup.Text = "Seite einrichten";
            this.Button_PageSetup.Click += new EventHandler(this.Button_PageSetup_Click);
            // 
            // padPrint
            // 
            this.padPrint.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom)
                                                     | AnchorStyles.Left)
                                                    | AnchorStyles.Right)));
            this.padPrint.EditAllowed = false;
            this.padPrint.Location = new Point(5, 60);
            this.padPrint.Name = "padPrint";
            this.padPrint.ShiftX = 0F;
            this.padPrint.ShiftY = 0F;
            this.padPrint.ShowInPrintMode = true;
            this.padPrint.Size = new Size(849, 568);
            this.padPrint.TabIndex = 2;
            this.padPrint.Zoom = 1F;
            this.padPrint.BeginnPrint += new PrintEventHandler(this.PrintPad_BeginnPrint);
            this.padPrint.PrintPage += new PrintPageEventHandler(this.PrintPad_PrintPage);
            // 
            // tabBildSchachteln
            // 
            this.tabBildSchachteln.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabBildSchachteln.Controls.Add(this.btnEinstellung);
            this.tabBildSchachteln.Controls.Add(this.capDpi);
            this.tabBildSchachteln.Controls.Add(this.flxAbstand);
            this.tabBildSchachteln.Controls.Add(this.flxHöhe);
            this.tabBildSchachteln.Controls.Add(this.flxBreite);
            this.tabBildSchachteln.Controls.Add(this.btnSchachtelnSpeichern);
            this.tabBildSchachteln.Controls.Add(this.padSchachteln);
            this.tabBildSchachteln.Location = new Point(4, 25);
            this.tabBildSchachteln.Name = "tabBildSchachteln";
            this.tabBildSchachteln.Size = new Size(860, 687);
            this.tabBildSchachteln.TabIndex = 5;
            this.tabBildSchachteln.Text = "Bild Schachteln";
            // 
            // btnEinstellung
            // 
            this.btnEinstellung.Location = new Point(592, 8);
            this.btnEinstellung.Name = "btnEinstellung";
            this.btnEinstellung.Size = new Size(128, 24);
            this.btnEinstellung.TabIndex = 19;
            this.btnEinstellung.Text = "Einstellung laden";
            this.btnEinstellung.Click += new EventHandler(this.btnEinstellung_Click);
            // 
            // capDpi
            // 
            this.capDpi.CausesValidation = false;
            this.capDpi.Location = new Point(440, 8);
            this.capDpi.Name = "capDpi";
            this.capDpi.Size = new Size(112, 24);
            this.capDpi.Text = "Dpi: 300";
            // 
            // flxAbstand
            // 
            this.flxAbstand.Caption = "Abstand:";
            this.flxAbstand.CaptionPosition = CaptionPosition.Links_neben_dem_Feld;
            this.flxAbstand.EditType = EditTypeFormula.Textfeld;
            this.flxAbstand.Location = new Point(296, 8);
            this.flxAbstand.Name = "flxAbstand";
            this.flxAbstand.QuickInfo = "Abstand zwischen zwei Einträgen";
            this.flxAbstand.Size = new Size(136, 24);
            this.flxAbstand.Suffix = "mm";
            this.flxAbstand.TabIndex = 18;
            this.flxAbstand.ValueChanged += new EventHandler(this.Attribute_Changed);
            // 
            // flxHöhe
            // 
            this.flxHöhe.Caption = "Höhe";
            this.flxHöhe.CaptionPosition = CaptionPosition.Links_neben_dem_Feld;
            this.flxHöhe.EditType = EditTypeFormula.Textfeld;
            this.flxHöhe.Location = new Point(152, 8);
            this.flxHöhe.Name = "flxHöhe";
            this.flxHöhe.QuickInfo = "Höhe des endgültigen Bildes";
            this.flxHöhe.Size = new Size(136, 24);
            this.flxHöhe.Suffix = "mm";
            this.flxHöhe.TabIndex = 17;
            this.flxHöhe.ValueChanged += new EventHandler(this.Attribute_Changed);
            // 
            // flxBreite
            // 
            this.flxBreite.Caption = "Breite:";
            this.flxBreite.CaptionPosition = CaptionPosition.Links_neben_dem_Feld;
            this.flxBreite.EditType = EditTypeFormula.Textfeld;
            this.flxBreite.Location = new Point(8, 8);
            this.flxBreite.Name = "flxBreite";
            this.flxBreite.QuickInfo = "Breite des endgültigen Bildes";
            this.flxBreite.Size = new Size(136, 24);
            this.flxBreite.Suffix = "mm";
            this.flxBreite.TabIndex = 16;
            this.flxBreite.ValueChanged += new EventHandler(this.Attribute_Changed);
            // 
            // btnSchachtelnSpeichern
            // 
            this.btnSchachtelnSpeichern.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
            this.btnSchachtelnSpeichern.ImageCode = "Diskette|24";
            this.btnSchachtelnSpeichern.Location = new Point(743, 632);
            this.btnSchachtelnSpeichern.Name = "btnSchachtelnSpeichern";
            this.btnSchachtelnSpeichern.Size = new Size(112, 48);
            this.btnSchachtelnSpeichern.TabIndex = 15;
            this.btnSchachtelnSpeichern.Text = "Speichern";
            this.btnSchachtelnSpeichern.Click += new EventHandler(this.btnSchachtelnSpeichern_Click);
            // 
            // padSchachteln
            // 
            this.padSchachteln.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom)
                                                          | AnchorStyles.Left)
                                                         | AnchorStyles.Right)));
            this.padSchachteln.EditAllowed = false;
            this.padSchachteln.Location = new Point(8, 40);
            this.padSchachteln.Name = "padSchachteln";
            this.padSchachteln.ShiftX = 0F;
            this.padSchachteln.ShiftY = 0F;
            this.padSchachteln.ShowInPrintMode = true;
            this.padSchachteln.Size = new Size(849, 584);
            this.padSchachteln.TabIndex = 3;
            this.padSchachteln.Zoom = 1F;
            // 
            // tabDateiExport
            // 
            this.tabDateiExport.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabDateiExport.Controls.Add(this.Caption4);
            this.tabDateiExport.Controls.Add(this.lstExported);
            this.tabDateiExport.Controls.Add(this.btnDrucken_ExportVerzeichnis);
            this.tabDateiExport.Controls.Add(this.btnMachZu);
            this.tabDateiExport.Location = new Point(4, 25);
            this.tabDateiExport.Name = "tabDateiExport";
            this.tabDateiExport.Size = new Size(860, 687);
            this.tabDateiExport.TabIndex = 2;
            this.tabDateiExport.Text = "Datei-Export";
            // 
            // Caption4
            // 
            this.Caption4.CausesValidation = false;
            this.Caption4.Location = new Point(8, 8);
            this.Caption4.Name = "Caption4";
            this.Caption4.Size = new Size(328, 24);
            this.Caption4.Text = "Erstellte Dateien:";
            // 
            // lstExported
            // 
            this.lstExported.AddAllowed = AddType.None;
            this.lstExported.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom)
                                                        | AnchorStyles.Left)
                                                       | AnchorStyles.Right)));
            this.lstExported.CheckBehavior = CheckBehavior.NoSelection;
            this.lstExported.Location = new Point(8, 40);
            this.lstExported.Name = "lstExported";
            this.lstExported.Size = new Size(847, 593);
            this.lstExported.TabIndex = 88;
            this.lstExported.Text = "Exported";
            this.lstExported.ContextMenuInit += new EventHandler<ContextMenuInitEventArgs>(this.lstExported_ContextMenuInit);
            this.lstExported.ItemClicked += new EventHandler<AbstractListItemEventArgs>(this.Exported_ItemClicked);
            // 
            // ExportDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new Size(868, 716);
            this.Controls.Add(this.Tabs);
            this.Controls.Add(this.c_Layoutx);
            this.Controls.Add(this.Caption3);
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.Name = "ExportDialog";
            this.ShowIcon = false;
            this.Text = "Drucken / Exportieren";
            this.Tabs.ResumeLayout(false);
            this.tabStart.ResumeLayout(false);
            this.grpArt.ResumeLayout(false);
            this.grpEinträge.ResumeLayout(false);
            this.tabDrucken.ResumeLayout(false);
            this.tabBildSchachteln.ResumeLayout(false);
            this.tabDateiExport.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        private ComboBox cbxLayoutWahl;
        private Caption c_Layoutx;
        private Caption capAnzahlInfo;
        private Caption Caption3;
        internal Button btnMachZu;
        internal Button btnLayoutEditorÖffnen;
        internal Button btnDrucken_ExportVerzeichnis;
        internal Caption capLayout;
        internal CreativePad padVorschau;
        internal TabPage tabDateiExport;
        internal TabPage tabStart;
        private Button optEinzelnSpeichern;
        private Button optDrucken;
        internal TabPage tabDrucken;
        internal Caption Caption4;
        internal ListBox lstExported;
        internal Button btnWeiter;
        internal CreativePad padPrint;
        internal Button Button_PageSetup;
        private Button btnDrucken;
        private Button Vorschau;
        internal TabControl Tabs;
        private GroupBox grpArt;
        private Button optBildSchateln;
        private GroupBox grpEinträge;
        private Button optSpezialFormat;
        private TabPage tabBildSchachteln;
        internal CreativePad padSchachteln;
        private Button btnSchachtelnSpeichern;
        private FlexiControl flxAbstand;
        private FlexiControl flxHöhe;
        private FlexiControl flxBreite;
        private Caption capDpi;
        private Button btnEinstellung;
    }
}

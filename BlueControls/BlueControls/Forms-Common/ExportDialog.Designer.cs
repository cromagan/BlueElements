using System.Diagnostics;
using BlueControls.Controls;

namespace BlueControls.Forms
    {


        public sealed partial class ExportDialog : Form
		{
			//Das Formular überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
			[DebuggerNonUserCode()]
			protected override void Dispose(bool disposing)
			{
				try
				{
					if (disposing )
					{

					}
				}
				finally
				{
					base.Dispose(disposing);
				}
			}



			//Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
			//Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
			//Das Bearbeiten mit dem Code-Editor ist nicht möglich.
			[DebuggerStepThrough()]
			private void InitializeComponent()
			{
            this.FrmDrucken_Zusammen = new BlueControls.Controls.Button();
            this.FrmDrucken_Einzeln = new BlueControls.Controls.Button();
            this.FrmDrucken_EinWahl = new BlueControls.Controls.Button();
            this.FrmDrucken_Info = new BlueControls.Controls.Caption();
            this.cbxDrucken_Layout1 = new BlueControls.Controls.ComboBox();
            this.c_Layoutx = new BlueControls.Controls.Caption();
            this.Caption3 = new BlueControls.Controls.Caption();
            this.MachZu = new BlueControls.Controls.Button();
            this.btnLayoutEditorÖffnen = new BlueControls.Controls.Button();
            this.FrmDrucken_ExportVerzeichniss = new BlueControls.Controls.Button();
            this.Captionxx1 = new BlueControls.Controls.Caption();
            this.padVorschau = new BlueControls.Controls.CreativePad();
            this.Tabs = new BlueControls.Controls.TabControl();
            this.TabAktion = new BlueControls.Controls.TabPage();
            this.WeiterAktion = new BlueControls.Controls.Button();
            this.optSpeichern = new BlueControls.Controls.Button();
            this.optDrucken = new BlueControls.Controls.Button();
            this.TabLayout = new BlueControls.Controls.TabPage();
            this.Weiter2 = new BlueControls.Controls.Button();
            this.TabEinträge = new BlueControls.Controls.TabPage();
            this.GroupBox1 = new BlueControls.Controls.GroupBox();
            this.WeiterEinträge = new BlueControls.Controls.Button();
            this.TabDiskExport = new BlueControls.Controls.TabPage();
            this.Caption4 = new BlueControls.Controls.Caption();
            this.Exported = new BlueControls.Controls.ListBox();
            this.TabDrucken = new BlueControls.Controls.TabPage();
            this.Vorschau = new BlueControls.Controls.Button();
            this.btnDrucken = new BlueControls.Controls.Button();
            this.Button_PageSetup = new BlueControls.Controls.Button();
            this.PrintPad = new BlueControls.Controls.CreativePad();
            this.grpArt = new BlueControls.Controls.GroupBox();
            this.optBild = new BlueControls.Controls.Button();
            this.grpLayout = new BlueControls.Controls.GroupBox();
            this.grpEinträge = new BlueControls.Controls.GroupBox();
            this.button1 = new BlueControls.Controls.Button();
            this.Tabs.SuspendLayout();
            this.TabAktion.SuspendLayout();
            this.TabEinträge.SuspendLayout();
            this.GroupBox1.SuspendLayout();
            this.TabDiskExport.SuspendLayout();
            this.TabDrucken.SuspendLayout();
            this.grpArt.SuspendLayout();
            this.grpLayout.SuspendLayout();
            this.grpEinträge.SuspendLayout();
            this.SuspendLayout();
            // 
            // FrmDrucken_Zusammen
            // 
            this.FrmDrucken_Zusammen.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FrmDrucken_Zusammen.ButtonStyle = BlueControls.Enums.enButtonStyle.Optionbox_Text;
            this.FrmDrucken_Zusammen.Location = new System.Drawing.Point(16, 80);
            this.FrmDrucken_Zusammen.Name = "FrmDrucken_Zusammen";
            this.FrmDrucken_Zusammen.Size = new System.Drawing.Size(480, 40);
            this.FrmDrucken_Zusammen.TabIndex = 1;
            this.FrmDrucken_Zusammen.Text = "<b>Zusammenfassen</b><br>Soweit möglich, alle Einträge zusammenfassen";
            // 
            // FrmDrucken_Einzeln
            // 
            this.FrmDrucken_Einzeln.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FrmDrucken_Einzeln.ButtonStyle = BlueControls.Enums.enButtonStyle.Optionbox_Text;
            this.FrmDrucken_Einzeln.Checked = true;
            this.FrmDrucken_Einzeln.Location = new System.Drawing.Point(16, 24);
            this.FrmDrucken_Einzeln.Name = "FrmDrucken_Einzeln";
            this.FrmDrucken_Einzeln.Size = new System.Drawing.Size(480, 40);
            this.FrmDrucken_Einzeln.TabIndex = 0;
            this.FrmDrucken_Einzeln.Text = "<b>Einzeln</b><br>Jeden Eintrag nacheinander verarbeiten";
            // 
            // FrmDrucken_EinWahl
            // 
            this.FrmDrucken_EinWahl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.FrmDrucken_EinWahl.ButtonStyle = BlueControls.Enums.enButtonStyle.Button_RibbonBar;
            this.FrmDrucken_EinWahl.ImageCode = "Stern";
            this.FrmDrucken_EinWahl.Location = new System.Drawing.Point(648, 24);
            this.FrmDrucken_EinWahl.Name = "FrmDrucken_EinWahl";
            this.FrmDrucken_EinWahl.QuickInfo = "Öffnet einen Dialog, wo die zu exportierenden<br>Einträge gewählt werden können.";
            this.FrmDrucken_EinWahl.Size = new System.Drawing.Size(60, 80);
            this.FrmDrucken_EinWahl.TabIndex = 84;
            this.FrmDrucken_EinWahl.Text = "Einträge wählen / ändern";
            this.FrmDrucken_EinWahl.Click += new System.EventHandler(this.EinWahl_Click);
            // 
            // FrmDrucken_Info
            // 
            this.FrmDrucken_Info.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FrmDrucken_Info.CausesValidation = false;
            this.FrmDrucken_Info.Location = new System.Drawing.Point(8, 24);
            this.FrmDrucken_Info.Name = "FrmDrucken_Info";
            this.FrmDrucken_Info.Size = new System.Drawing.Size(632, 80);
            this.FrmDrucken_Info.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // cbxDrucken_Layout1
            // 
            this.cbxDrucken_Layout1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxDrucken_Layout1.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxDrucken_Layout1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxDrucken_Layout1.Location = new System.Drawing.Point(8, 40);
            this.cbxDrucken_Layout1.Name = "cbxDrucken_Layout1";
            this.cbxDrucken_Layout1.Size = new System.Drawing.Size(360, 24);
            this.cbxDrucken_Layout1.TabIndex = 80;
            this.cbxDrucken_Layout1.TextChanged += new System.EventHandler(this.cbxDrucken_Layout1_TextChanged);
            // 
            // c_Layoutx
            // 
            this.c_Layoutx.CausesValidation = false;
            this.c_Layoutx.Location = new System.Drawing.Point(-86, 51);
            this.c_Layoutx.Name = "c_Layoutx";
            this.c_Layoutx.Size = new System.Drawing.Size(80, 16);
            this.c_Layoutx.Text = "c_Layoutx";
            this.c_Layoutx.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // Caption3
            // 
            this.Caption3.CausesValidation = false;
            this.Caption3.Location = new System.Drawing.Point(512, -208);
            this.Caption3.Name = "Caption3";
            this.Caption3.Size = new System.Drawing.Size(46, 20);
            this.Caption3.Text = "Layout:";
            // 
            // MachZu
            // 
            this.MachZu.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.MachZu.ImageCode = "Häkchen|16";
            this.MachZu.Location = new System.Drawing.Point(600, 505);
            this.MachZu.Name = "MachZu";
            this.MachZu.Size = new System.Drawing.Size(112, 40);
            this.MachZu.TabIndex = 85;
            this.MachZu.Text = "Beenden";
            this.MachZu.Click += new System.EventHandler(this.FrmDrucken_Drucken_Click);
            // 
            // btnLayoutEditorÖffnen
            // 
            this.btnLayoutEditorÖffnen.ImageCode = "Layout|16";
            this.btnLayoutEditorÖffnen.Location = new System.Drawing.Point(8, 64);
            this.btnLayoutEditorÖffnen.Name = "btnLayoutEditorÖffnen";
            this.btnLayoutEditorÖffnen.Size = new System.Drawing.Size(152, 32);
            this.btnLayoutEditorÖffnen.TabIndex = 86;
            this.btnLayoutEditorÖffnen.Text = "Layout Editor öffnen";
            this.btnLayoutEditorÖffnen.Click += new System.EventHandler(this.LayoutEditor_Click);
            // 
            // FrmDrucken_ExportVerzeichniss
            // 
            this.FrmDrucken_ExportVerzeichniss.ImageCode = "Ordner|16";
            this.FrmDrucken_ExportVerzeichniss.Location = new System.Drawing.Point(8, 448);
            this.FrmDrucken_ExportVerzeichniss.Name = "FrmDrucken_ExportVerzeichniss";
            this.FrmDrucken_ExportVerzeichniss.Size = new System.Drawing.Size(208, 40);
            this.FrmDrucken_ExportVerzeichniss.TabIndex = 87;
            this.FrmDrucken_ExportVerzeichniss.Text = "Export Verzeichnis öffnen";
            this.FrmDrucken_ExportVerzeichniss.Click += new System.EventHandler(this.Button1_Click);
            // 
            // Captionxx1
            // 
            this.Captionxx1.CausesValidation = false;
            this.Captionxx1.Location = new System.Drawing.Point(8, 24);
            this.Captionxx1.Name = "Captionxx1";
            this.Captionxx1.Size = new System.Drawing.Size(82, 22);
            this.Captionxx1.Text = "Layout:";
            this.Captionxx1.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_ohne_Textumbruch;
            // 
            // padVorschau
            // 
            this.padVorschau.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.padVorschau.Location = new System.Drawing.Point(376, 16);
            this.padVorschau.Name = "padVorschau";
            this.padVorschau.Size = new System.Drawing.Size(336, 184);
            this.padVorschau.TabIndex = 1;
            // 
            // Tabs
            // 
            this.Tabs.Controls.Add(this.TabAktion);
            this.Tabs.Controls.Add(this.TabLayout);
            this.Tabs.Controls.Add(this.TabEinträge);
            this.Tabs.Controls.Add(this.TabDiskExport);
            this.Tabs.Controls.Add(this.TabDrucken);
            this.Tabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Tabs.HotTrack = true;
            this.Tabs.Location = new System.Drawing.Point(0, 0);
            this.Tabs.Name = "Tabs";
            this.Tabs.SelectedIndex = 0;
            this.Tabs.Size = new System.Drawing.Size(725, 716);
            this.Tabs.TabIndex = 81;
            // 
            // TabAktion
            // 
            this.TabAktion.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.TabAktion.Controls.Add(this.grpEinträge);
            this.TabAktion.Controls.Add(this.grpLayout);
            this.TabAktion.Controls.Add(this.grpArt);
            this.TabAktion.Controls.Add(this.WeiterAktion);
            this.TabAktion.Location = new System.Drawing.Point(4, 25);
            this.TabAktion.Name = "TabAktion";
            this.TabAktion.Size = new System.Drawing.Size(717, 687);
            this.TabAktion.TabIndex = 3;
            this.TabAktion.Text = "Aktion";
            // 
            // WeiterAktion
            // 
            this.WeiterAktion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.WeiterAktion.ImageCode = "Häkchen|16";
            this.WeiterAktion.Location = new System.Drawing.Point(568, 649);
            this.WeiterAktion.Name = "WeiterAktion";
            this.WeiterAktion.Size = new System.Drawing.Size(144, 32);
            this.WeiterAktion.TabIndex = 88;
            this.WeiterAktion.Text = "Weiter";
            this.WeiterAktion.Click += new System.EventHandler(this.WeiterAktion_Click);
            // 
            // optSpeichern
            // 
            this.optSpeichern.ButtonStyle = BlueControls.Enums.enButtonStyle.Optionbox_Text;
            this.optSpeichern.ImageCode = "Diskette";
            this.optSpeichern.Location = new System.Drawing.Point(8, 24);
            this.optSpeichern.Name = "optSpeichern";
            this.optSpeichern.Size = new System.Drawing.Size(216, 40);
            this.optSpeichern.TabIndex = 86;
            this.optSpeichern.Text = "<b>Einzeln Speichern</b><br>Auf einem Datenträger schreiben";
            this.optSpeichern.CheckedChanged += new System.EventHandler(this.Speichern_CheckedChanged);
            // 
            // optDrucken
            // 
            this.optDrucken.ButtonStyle = BlueControls.Enums.enButtonStyle.Optionbox_Text;
            this.optDrucken.ImageCode = "Drucker";
            this.optDrucken.Location = new System.Drawing.Point(224, 24);
            this.optDrucken.Name = "optDrucken";
            this.optDrucken.Size = new System.Drawing.Size(216, 40);
            this.optDrucken.TabIndex = 85;
            this.optDrucken.Text = "<b>Drucken</b><br>Auf einem Drucker ausgeben";
            this.optDrucken.CheckedChanged += new System.EventHandler(this.Speichern_CheckedChanged);
            // 
            // TabLayout
            // 
            this.TabLayout.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.TabLayout.Location = new System.Drawing.Point(4, 25);
            this.TabLayout.Name = "TabLayout";
            this.TabLayout.Size = new System.Drawing.Size(717, 687);
            this.TabLayout.TabIndex = 1;
            this.TabLayout.Text = "Layout";
            // 
            // Weiter2
            // 
            this.Weiter2.ImageCode = "Häkchen|16";
            this.Weiter2.Location = new System.Drawing.Point(144, 168);
            this.Weiter2.Name = "Weiter2";
            this.Weiter2.Size = new System.Drawing.Size(144, 32);
            this.Weiter2.TabIndex = 87;
            this.Weiter2.Text = "Weiter";
            this.Weiter2.Click += new System.EventHandler(this.Weiter2_Click);
            // 
            // TabEinträge
            // 
            this.TabEinträge.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.TabEinträge.Controls.Add(this.WeiterEinträge);
            this.TabEinträge.Location = new System.Drawing.Point(4, 25);
            this.TabEinträge.Name = "TabEinträge";
            this.TabEinträge.Size = new System.Drawing.Size(717, 551);
            this.TabEinträge.TabIndex = 0;
            this.TabEinträge.Text = "Einträge";
            // 
            // GroupBox1
            // 
            this.GroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.GroupBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.GroupBox1.CausesValidation = false;
            this.GroupBox1.Controls.Add(this.FrmDrucken_Einzeln);
            this.GroupBox1.Controls.Add(this.FrmDrucken_Zusammen);
            this.GroupBox1.Location = new System.Drawing.Point(8, 112);
            this.GroupBox1.Name = "GroupBox1";
            this.GroupBox1.Size = new System.Drawing.Size(504, 144);
            this.GroupBox1.TabIndex = 0;
            this.GroupBox1.TabStop = false;
            this.GroupBox1.Text = "Verarbeitung";
            // 
            // WeiterEinträge
            // 
            this.WeiterEinträge.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.WeiterEinträge.ImageCode = "Häkchen|16";
            this.WeiterEinträge.Location = new System.Drawing.Point(570, 512);
            this.WeiterEinträge.Name = "WeiterEinträge";
            this.WeiterEinträge.Size = new System.Drawing.Size(144, 32);
            this.WeiterEinträge.TabIndex = 85;
            this.WeiterEinträge.Text = "Weiter";
            this.WeiterEinträge.Click += new System.EventHandler(this.WeiterEinträge_Click);
            // 
            // TabDiskExport
            // 
            this.TabDiskExport.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.TabDiskExport.Controls.Add(this.Caption4);
            this.TabDiskExport.Controls.Add(this.Exported);
            this.TabDiskExport.Controls.Add(this.FrmDrucken_ExportVerzeichniss);
            this.TabDiskExport.Controls.Add(this.MachZu);
            this.TabDiskExport.Location = new System.Drawing.Point(4, 25);
            this.TabDiskExport.Name = "TabDiskExport";
            this.TabDiskExport.Size = new System.Drawing.Size(717, 551);
            this.TabDiskExport.TabIndex = 2;
            this.TabDiskExport.Text = "Datei-Export";
            // 
            // Caption4
            // 
            this.Caption4.CausesValidation = false;
            this.Caption4.Location = new System.Drawing.Point(8, 8);
            this.Caption4.Name = "Caption4";
            this.Caption4.Size = new System.Drawing.Size(328, 24);
            this.Caption4.Text = "Erstellte Dateien:";
            // 
            // Exported
            // 
            this.Exported.AddAllowed = BlueControls.Enums.enAddType.None;
            this.Exported.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Exported.CheckBehavior = BlueControls.Enums.enCheckBehavior.NoSelection;
            this.Exported.FilterAllowed = true;
            this.Exported.LastFilePath = null;
            this.Exported.Location = new System.Drawing.Point(8, 40);
            this.Exported.Name = "Exported";
            this.Exported.Size = new System.Drawing.Size(704, 457);
            this.Exported.TabIndex = 88;
            this.Exported.Text = "Exported";
            this.Exported.ItemClicked += new System.EventHandler<BlueControls.EventArgs.BasicListItemEventArgs>(this.Exported_ItemClicked);
            // 
            // TabDrucken
            // 
            this.TabDrucken.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.TabDrucken.Controls.Add(this.Vorschau);
            this.TabDrucken.Controls.Add(this.btnDrucken);
            this.TabDrucken.Controls.Add(this.Button_PageSetup);
            this.TabDrucken.Controls.Add(this.PrintPad);
            this.TabDrucken.Location = new System.Drawing.Point(4, 25);
            this.TabDrucken.Name = "TabDrucken";
            this.TabDrucken.Size = new System.Drawing.Size(717, 551);
            this.TabDrucken.TabIndex = 4;
            this.TabDrucken.Text = "Drucken";
            // 
            // Vorschau
            // 
            this.Vorschau.ImageCode = "Datei|36|||||||||Lupe";
            this.Vorschau.Location = new System.Drawing.Point(184, 8);
            this.Vorschau.Name = "Vorschau";
            this.Vorschau.Size = new System.Drawing.Size(168, 48);
            this.Vorschau.TabIndex = 15;
            this.Vorschau.Text = "Vorschau";
            this.Vorschau.Click += new System.EventHandler(this.Vorschau_Click);
            // 
            // btnDrucken
            // 
            this.btnDrucken.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDrucken.ImageCode = "Drucker|24";
            this.btnDrucken.Location = new System.Drawing.Point(600, 497);
            this.btnDrucken.Name = "btnDrucken";
            this.btnDrucken.QuickInfo = "Öffnet den Drucker-Dialog.";
            this.btnDrucken.Size = new System.Drawing.Size(112, 48);
            this.btnDrucken.TabIndex = 14;
            this.btnDrucken.Text = "Drucken";
            this.btnDrucken.Click += new System.EventHandler(this.btnDrucken_Click);
            // 
            // Button_PageSetup
            // 
            this.Button_PageSetup.ImageCode = "SeiteEinrichten|36";
            this.Button_PageSetup.Location = new System.Drawing.Point(8, 8);
            this.Button_PageSetup.Name = "Button_PageSetup";
            this.Button_PageSetup.Size = new System.Drawing.Size(168, 48);
            this.Button_PageSetup.TabIndex = 13;
            this.Button_PageSetup.Text = "Seite einrichten";
            this.Button_PageSetup.Click += new System.EventHandler(this.Button_PageSetup_Click);
            // 
            // PrintPad
            // 
            this.PrintPad.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PrintPad.Location = new System.Drawing.Point(5, 60);
            this.PrintPad.Name = "PrintPad";
            this.PrintPad.Size = new System.Drawing.Size(706, 432);
            this.PrintPad.TabIndex = 2;
            this.PrintPad.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(this.PrintPad_PrintPage);
            this.PrintPad.BeginnPrint += new System.Drawing.Printing.PrintEventHandler(this.PrintPad_BeginnPrint);
            // 
            // grpArt
            // 
            this.grpArt.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpArt.Controls.Add(this.button1);
            this.grpArt.Controls.Add(this.optBild);
            this.grpArt.Controls.Add(this.optSpeichern);
            this.grpArt.Controls.Add(this.optDrucken);
            this.grpArt.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpArt.Location = new System.Drawing.Point(0, 0);
            this.grpArt.Name = "grpArt";
            this.grpArt.Size = new System.Drawing.Size(717, 112);
            this.grpArt.TabIndex = 89;
            this.grpArt.TabStop = false;
            this.grpArt.Text = "Art des Exportes";
            // 
            // optBild
            // 
            this.optBild.ButtonStyle = BlueControls.Enums.enButtonStyle.Optionbox_Text;
            this.optBild.ImageCode = "Diskette";
            this.optBild.Location = new System.Drawing.Point(440, 24);
            this.optBild.Name = "optBild";
            this.optBild.Size = new System.Drawing.Size(216, 40);
            this.optBild.TabIndex = 87;
            this.optBild.Text = "<b>Als Bild speichern</b><br>Einträge auf einem Bild schachteln";
            // 
            // grpLayout
            // 
            this.grpLayout.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpLayout.Controls.Add(this.cbxDrucken_Layout1);
            this.grpLayout.Controls.Add(this.Weiter2);
            this.grpLayout.Controls.Add(this.padVorschau);
            this.grpLayout.Controls.Add(this.Captionxx1);
            this.grpLayout.Controls.Add(this.btnLayoutEditorÖffnen);
            this.grpLayout.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpLayout.Location = new System.Drawing.Point(0, 112);
            this.grpLayout.Name = "grpLayout";
            this.grpLayout.Size = new System.Drawing.Size(717, 208);
            this.grpLayout.TabIndex = 90;
            this.grpLayout.TabStop = false;
            this.grpLayout.Text = "Layout wählen";
            // 
            // grpEinträge
            // 
            this.grpEinträge.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpEinträge.Controls.Add(this.GroupBox1);
            this.grpEinträge.Controls.Add(this.FrmDrucken_Info);
            this.grpEinträge.Controls.Add(this.FrmDrucken_EinWahl);
            this.grpEinträge.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpEinträge.Location = new System.Drawing.Point(0, 320);
            this.grpEinträge.Name = "grpEinträge";
            this.grpEinträge.Size = new System.Drawing.Size(717, 264);
            this.grpEinträge.TabIndex = 91;
            this.grpEinträge.TabStop = false;
            this.grpEinträge.Text = "Einträge";
            // 
            // button1
            // 
            this.button1.ButtonStyle = BlueControls.Enums.enButtonStyle.Optionbox_Text;
            this.button1.ImageCode = "Diskette";
            this.button1.Location = new System.Drawing.Point(8, 64);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(648, 40);
            this.button1.TabIndex = 88;
            this.button1.Text = "<b>Spezial-Dateiformat</b><br>Das Vorlagen Layout enthält einen speziellen Code, " +
    "so dass alle Einräge in eine Datei geschrieben werden";
            // 
            // ExportDialog
            // 
            this.ClientSize = new System.Drawing.Size(725, 716);
            this.Controls.Add(this.Tabs);
            this.Controls.Add(this.c_Layoutx);
            this.Controls.Add(this.Caption3);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.Name = "ExportDialog";
            this.ShowIcon = false;
            this.Text = "Drucken / Exportieren";
            this.Tabs.ResumeLayout(false);
            this.TabAktion.ResumeLayout(false);
            this.TabEinträge.ResumeLayout(false);
            this.GroupBox1.ResumeLayout(false);
            this.TabDiskExport.ResumeLayout(false);
            this.TabDrucken.ResumeLayout(false);
            this.grpArt.ResumeLayout(false);
            this.grpLayout.ResumeLayout(false);
            this.grpEinträge.ResumeLayout(false);
            this.ResumeLayout(false);

			}
			private ComboBox cbxDrucken_Layout1;
			private Caption c_Layoutx;
			private Caption FrmDrucken_Info;
			private Button FrmDrucken_EinWahl;
			private Button FrmDrucken_Zusammen;
			private Button FrmDrucken_Einzeln;
			private Caption Caption3;
			internal Button MachZu;
			internal Button btnLayoutEditorÖffnen;
			internal Button FrmDrucken_ExportVerzeichniss;
			internal Caption Captionxx1;
			internal CreativePad padVorschau;
			internal TabPage TabEinträge;
			internal TabPage TabLayout;
			internal TabPage TabDiskExport;
			internal Button WeiterEinträge;
			internal Button Weiter2;
			internal TabPage TabAktion;
			private Button optSpeichern;
			private Button optDrucken;
			internal TabPage TabDrucken;
			internal GroupBox GroupBox1;
			internal Caption Caption4;
			internal ListBox Exported;
			internal Button WeiterAktion;
			internal CreativePad PrintPad;
			internal Button Button_PageSetup;
			private Button btnDrucken;
			private Button Vorschau;
        internal TabControl Tabs;
        private GroupBox grpLayout;
        private GroupBox grpArt;
        private Button optBild;
        private GroupBox grpEinträge;
        private Button button1;
    }
	}

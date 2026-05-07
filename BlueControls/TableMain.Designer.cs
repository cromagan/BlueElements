using BlueControls.Controls;
using BlueControls.Enums;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Button = BlueControls.Controls.Button;
using GroupBox = BlueControls.Controls.GroupBox;
using TabPage = System.Windows.Forms.TabPage;
using TextBox = BlueControls.Controls.TextBox;

namespace BildzeichenListe {
    public partial class TableMain
    {

        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TableMain));
            this.tabSpezial = new System.Windows.Forms.TabPage();
            this.grpTabelleAdministration = new BlueControls.Controls.GroupBox();
            this.btnAlleTabellenLaden = new BlueControls.Controls.Button();
            this.btnClearSAPCache = new BlueControls.Controls.Button();
            this.btnVorschauVerwaltung = new BlueControls.Controls.Button();
            this.btnPreRelease = new BlueControls.Controls.Button();
            this.grpWorkflow = new BlueControls.Controls.GroupBox();
            this.btnWFMassenAenderung = new BlueControls.Controls.Button();
            this.grpSonstigeWerkzeuge = new BlueControls.Controls.GroupBox();
            this.btnDraftAbleitung = new BlueControls.Controls.Button();
            this.btnBildzeichenVerwendungAllerZeilen = new BlueControls.Controls.Button();
            this.grpKrones = new BlueControls.Controls.GroupBox();
            this.btnKplot = new BlueControls.Controls.Button();
            this.btnPrio3 = new BlueControls.Controls.Button();
            this.ribMain.SuspendLayout();
            this.pnlTableSelect.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SplitContainer1)).BeginInit();
            this.SplitContainer1.Panel1.SuspendLayout();
            this.SplitContainer1.Panel2.SuspendLayout();
            this.SplitContainer1.SuspendLayout();
            this.tbcSidebar.SuspendLayout();
            this.tabAllgemein.SuspendLayout();
            this.grpAnsicht.SuspendLayout();
            this.tabExport.SuspendLayout();
            this.pnlStatusBar.SuspendLayout();
            this.tabSpezial.SuspendLayout();
            this.grpTabelleAdministration.SuspendLayout();
            this.grpWorkflow.SuspendLayout();
            this.grpSonstigeWerkzeuge.SuspendLayout();
            this.grpKrones.SuspendLayout();
            this.SuspendLayout();
            // 
            // capInfo
            // 
            this.capInfo.Location = new System.Drawing.Point(1295, 0);
            this.capInfo.Size = new System.Drawing.Size(260, 81);
            // 
            // btnTabellen
            // 
            this.btnTabellen.ButtonStyle = BlueControls.Enums.ButtonStyle.Button;
            // 
            // ribMain
            // 
            this.ribMain.Controls.Add(this.tabSpezial);
            this.ribMain.Size = new System.Drawing.Size(1563, 110);
            this.ribMain.TabDefault = this.tabAllgemein;
            this.ribMain.TabDefaultOrder = new string[] {
        "Allgemein",
        "Spezial",
        "Import/Export",
        "Administration"};
            this.ribMain.Controls.SetChildIndex(this.tabAdmin, 0);
            this.ribMain.Controls.SetChildIndex(this.tabExport, 0);
            this.ribMain.Controls.SetChildIndex(this.tabSpezial, 0);
            this.ribMain.Controls.SetChildIndex(this.tabAllgemein, 0);
            // 
            // pnlTableSelect
            // 
            this.pnlTableSelect.Size = new System.Drawing.Size(1144, 24);
            // 
            // tbcTableSelector
            // 
            this.tbcTableSelector.Size = new System.Drawing.Size(1048, 24);
            // 
            // TableView
            // 
            this.TableView.Size = new System.Drawing.Size(1144, 700);
            // 
            // SplitContainer1
            // 
            this.SplitContainer1.Size = new System.Drawing.Size(1563, 764);
            this.SplitContainer1.SplitterDistance = 1144;
            // 
            // tabAdmin
            // 
            this.tabAdmin.Size = new System.Drawing.Size(1000, 81);
            // 
            // tbcSidebar
            // 
            this.tbcSidebar.Size = new System.Drawing.Size(408, 764);
            // 
            // tabAllgemein
            // 
            this.tabAllgemein.Size = new System.Drawing.Size(1555, 81);
            // 
            // grpAnsicht
            // 
            this.grpAnsicht.Controls.Add(this.btnPrio3);
            this.grpAnsicht.Size = new System.Drawing.Size(435, 81);
            this.grpAnsicht.Controls.SetChildIndex(this.btnZoomIn, 0);
            this.grpAnsicht.Controls.SetChildIndex(this.btnZoomOut, 0);
            this.grpAnsicht.Controls.SetChildIndex(this.btnZoomFit, 0);
            this.grpAnsicht.Controls.SetChildIndex(this.btnAlleErweitern, 0);
            this.grpAnsicht.Controls.SetChildIndex(this.btnAlleSchließen, 0);
            this.grpAnsicht.Controls.SetChildIndex(this.btnPrio3, 0);
            // 
            // btnAlleSchließen
            // 
            this.btnAlleSchließen.Location = new System.Drawing.Point(264, 24);
            // 
            // btnAlleErweitern
            // 
            this.btnAlleErweitern.Location = new System.Drawing.Point(264, 2);
            // 
            // tabExport
            // 
            this.tabExport.Controls.Add(this.grpKrones);
            this.tabExport.Size = new System.Drawing.Size(1000, 81);
            this.tabExport.Controls.SetChildIndex(this.grpKrones, 0);
            // 
            // tabFormula
            // 
            this.tabFormula.Size = new System.Drawing.Size(400, 735);
            // 
            // grpAufgaben
            // 
            this.grpAufgaben.Location = new System.Drawing.Point(1003, 0);
            // 
            // btnZoomOut
            // 
            this.btnZoomOut.Location = new System.Drawing.Point(400, 2);
            // 
            // btnZoomIn
            // 
            this.btnZoomIn.Location = new System.Drawing.Point(376, 2);
            // 
            // btnZoomFit
            // 
            this.btnZoomFit.Location = new System.Drawing.Point(376, 24);
            // 
            // capStatusBar
            // 
            this.capStatusBar.Size = new System.Drawing.Size(1259, 24);
            // 
            // pnlStatusBar
            // 
            this.pnlStatusBar.Location = new System.Drawing.Point(0, 874);
            this.pnlStatusBar.Size = new System.Drawing.Size(1563, 24);
            // 
            // tabSpezial
            // 
            this.tabSpezial.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.tabSpezial.Controls.Add(this.grpTabelleAdministration);
            this.tabSpezial.Controls.Add(this.grpWorkflow);
            this.tabSpezial.Controls.Add(this.grpSonstigeWerkzeuge);
            this.tabSpezial.Location = new System.Drawing.Point(4, 25);
            this.tabSpezial.Name = "tabSpezial";
            this.tabSpezial.Size = new System.Drawing.Size(1000, 81);
            this.tabSpezial.TabIndex = 1;
            this.tabSpezial.Text = "Spezial";
            // 
            // grpTabelleAdministration
            // 
            this.grpTabelleAdministration.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpTabelleAdministration.CausesValidation = false;
            this.grpTabelleAdministration.Controls.Add(this.btnAlleTabellenLaden);
            this.grpTabelleAdministration.Controls.Add(this.btnClearSAPCache);
            this.grpTabelleAdministration.Controls.Add(this.btnVorschauVerwaltung);
            this.grpTabelleAdministration.Controls.Add(this.btnPreRelease);
            this.grpTabelleAdministration.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpTabelleAdministration.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpTabelleAdministration.Location = new System.Drawing.Point(751, 0);
            this.grpTabelleAdministration.Name = "grpTabelleAdministration";
            this.grpTabelleAdministration.Size = new System.Drawing.Size(340, 81);
            this.grpTabelleAdministration.TabIndex = 4;
            this.grpTabelleAdministration.TabStop = false;
            this.grpTabelleAdministration.Text = "Tabelle Administration";
            // 
            // btnAlleTabellenLaden
            // 
            this.btnAlleTabellenLaden.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnAlleTabellenLaden.ImageCode = "Tabelle|16";
            this.btnAlleTabellenLaden.Location = new System.Drawing.Point(8, 24);
            this.btnAlleTabellenLaden.Name = "btnAlleTabellenLaden";
            this.btnAlleTabellenLaden.Size = new System.Drawing.Size(136, 22);
            this.btnAlleTabellenLaden.TabIndex = 23;
            this.btnAlleTabellenLaden.Text = "Alle Tabellen laden";
            this.btnAlleTabellenLaden.Click += new System.EventHandler(this.btnAlleTabellenLaden_Click);
            // 
            // btnClearSAPCache
            // 
            this.btnClearSAPCache.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnClearSAPCache.ImageCode = "SAP|16|||||||||Kreuz";
            this.btnClearSAPCache.Location = new System.Drawing.Point(168, 2);
            this.btnClearSAPCache.Name = "btnClearSAPCache";
            this.btnClearSAPCache.Size = new System.Drawing.Size(152, 22);
            this.btnClearSAPCache.TabIndex = 24;
            this.btnClearSAPCache.Text = "SAP Cache löschen";
            this.btnClearSAPCache.Click += new System.EventHandler(this.btnClearSAPCache_Click);
            // 
            // btnVorschauVerwaltung
            // 
            this.btnVorschauVerwaltung.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnVorschauVerwaltung.ImageCode = "Datei|16|||||||||Stift";
            this.btnVorschauVerwaltung.Location = new System.Drawing.Point(8, 2);
            this.btnVorschauVerwaltung.Name = "btnVorschauVerwaltung";
            this.btnVorschauVerwaltung.QuickInfo = "Befehle für Vorschauen";
            this.btnVorschauVerwaltung.Size = new System.Drawing.Size(152, 22);
            this.btnVorschauVerwaltung.TabIndex = 24;
            this.btnVorschauVerwaltung.Text = "Vorschau-Verwaltung";
            this.btnVorschauVerwaltung.Click += new System.EventHandler(this.btnVorschauVerwaltung_Click);
            // 
            // btnPreRelease
            // 
            this.btnPreRelease.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnPreRelease.Location = new System.Drawing.Point(8, 46);
            this.btnPreRelease.Name = "btnPreRelease";
            this.btnPreRelease.Size = new System.Drawing.Size(136, 22);
            this.btnPreRelease.TabIndex = 22;
            this.btnPreRelease.Text = "PreRelease erstellen";
            this.btnPreRelease.Click += new System.EventHandler(this.btnPreRelease_Click);
            // 
            // grpWorkflow
            // 
            this.grpWorkflow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpWorkflow.CausesValidation = false;
            this.grpWorkflow.Controls.Add(this.btnWFMassenAenderung);
            this.grpWorkflow.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpWorkflow.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpWorkflow.Location = new System.Drawing.Point(575, 0);
            this.grpWorkflow.Name = "grpWorkflow";
            this.grpWorkflow.Size = new System.Drawing.Size(176, 81);
            this.grpWorkflow.TabIndex = 3;
            this.grpWorkflow.TabStop = false;
            this.grpWorkflow.Text = "Workflow";
            // 
            // btnWFMassenAenderung
            // 
            this.btnWFMassenAenderung.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnWFMassenAenderung.ImageCode = "Stift|16";
            this.btnWFMassenAenderung.Location = new System.Drawing.Point(8, 24);
            this.btnWFMassenAenderung.Name = "btnWFMassenAenderung";
            this.btnWFMassenAenderung.Size = new System.Drawing.Size(152, 22);
            this.btnWFMassenAenderung.TabIndex = 5;
            this.btnWFMassenAenderung.Text = "WF Massenänderung";
            this.btnWFMassenAenderung.Click += new System.EventHandler(this.btnWFMassenAenderung_Click);
            // 
            // grpSonstigeWerkzeuge
            // 
            this.grpSonstigeWerkzeuge.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpSonstigeWerkzeuge.CausesValidation = false;
            this.grpSonstigeWerkzeuge.Controls.Add(this.btnDraftAbleitung);
            this.grpSonstigeWerkzeuge.Controls.Add(this.btnBildzeichenVerwendungAllerZeilen);
            this.grpSonstigeWerkzeuge.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpSonstigeWerkzeuge.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpSonstigeWerkzeuge.Location = new System.Drawing.Point(0, 0);
            this.grpSonstigeWerkzeuge.Name = "grpSonstigeWerkzeuge";
            this.grpSonstigeWerkzeuge.Size = new System.Drawing.Size(155, 81);
            this.grpSonstigeWerkzeuge.TabIndex = 1;
            this.grpSonstigeWerkzeuge.TabStop = false;
            this.grpSonstigeWerkzeuge.Text = "Sonstige Werkzeuge";
            // 
            // btnDraftAbleitung
            // 
            this.btnDraftAbleitung.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnDraftAbleitung.ImageCode = "Bild|16";
            this.btnDraftAbleitung.Location = new System.Drawing.Point(88, 2);
            this.btnDraftAbleitung.Name = "btnDraftAbleitung";
            this.btnDraftAbleitung.QuickInfo = "Von allen angezeigten Zeilen";
            this.btnDraftAbleitung.Size = new System.Drawing.Size(60, 66);
            this.btnDraftAbleitung.TabIndex = 9;
            this.btnDraftAbleitung.Text = "Draft-Ableitung";
            this.btnDraftAbleitung.Click += new System.EventHandler(this.btnDraftAbleitung_Click);
            // 
            // btnBildzeichenVerwendungAllerZeilen
            // 
            this.btnBildzeichenVerwendungAllerZeilen.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnBildzeichenVerwendungAllerZeilen.ImageCode = "Hierarchie||||||||||Information";
            this.btnBildzeichenVerwendungAllerZeilen.Location = new System.Drawing.Point(8, 2);
            this.btnBildzeichenVerwendungAllerZeilen.Name = "btnBildzeichenVerwendungAllerZeilen";
            this.btnBildzeichenVerwendungAllerZeilen.QuickInfo = "Von allen angezeigten Zeilen";
            this.btnBildzeichenVerwendungAllerZeilen.Size = new System.Drawing.Size(80, 66);
            this.btnBildzeichenVerwendungAllerZeilen.TabIndex = 8;
            this.btnBildzeichenVerwendungAllerZeilen.Text = "Bildzeichen-Verwendung";
            this.btnBildzeichenVerwendungAllerZeilen.Click += new System.EventHandler(this.btnBildzeichenVerwendungAllerZeilen_Click);
            // 
            // grpKrones
            // 
            this.grpKrones.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpKrones.CausesValidation = false;
            this.grpKrones.Controls.Add(this.btnKplot);
            this.grpKrones.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpKrones.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpKrones.Location = new System.Drawing.Point(368, 0);
            this.grpKrones.Name = "grpKrones";
            this.grpKrones.Size = new System.Drawing.Size(72, 81);
            this.grpKrones.TabIndex = 0;
            this.grpKrones.TabStop = false;
            this.grpKrones.Text = "Krones";
            // 
            // btnKplot
            // 
            this.btnKplot.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnKplot.ImageCode = "Drucker";
            this.btnKplot.Location = new System.Drawing.Point(8, 2);
            this.btnKplot.Name = "btnKplot";
            this.btnKplot.Size = new System.Drawing.Size(56, 66);
            this.btnKplot.TabIndex = 2;
            this.btnKplot.Text = "kPlot";
            this.btnKplot.Click += new System.EventHandler(this.btnKplot_Click);
            // 
            // btnPrio3
            // 
            this.btnPrio3.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Big_Borderless;
            this.btnPrio3.ImageCode = "Warnung";
            this.btnPrio3.Location = new System.Drawing.Point(208, 2);
            this.btnPrio3.Margin = new System.Windows.Forms.Padding(4);
            this.btnPrio3.Name = "btnPrio3";
            this.btnPrio3.QuickInfo = "Ist diese Option aktiviert, werden<br>Prio 3 Zeichnungen eingeblendet.";
            this.btnPrio3.Size = new System.Drawing.Size(48, 66);
            this.btnPrio3.TabIndex = 12;
            this.btnPrio3.Text = "Prio 3";
            this.btnPrio3.CheckedChanged += new System.EventHandler(this.btnPrio3_CheckedChanged);
            // 
            // TableMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1563, 898);
            this.Icon = (System.Drawing.Icon)global::BildzeichenListe.Properties.Resources.TBB;
            this.Name = "TableMain";
            this.ribMain.ResumeLayout(false);
            this.pnlTableSelect.ResumeLayout(false);
            this.SplitContainer1.Panel1.ResumeLayout(false);
            this.SplitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.SplitContainer1)).EndInit();
            this.SplitContainer1.ResumeLayout(false);
            this.tbcSidebar.ResumeLayout(false);
            this.tabAllgemein.ResumeLayout(false);
            this.grpAnsicht.ResumeLayout(false);
            this.tabExport.ResumeLayout(false);
            this.pnlStatusBar.ResumeLayout(false);
            this.tabSpezial.ResumeLayout(false);
            this.grpTabelleAdministration.ResumeLayout(false);
            this.grpWorkflow.ResumeLayout(false);
            this.grpSonstigeWerkzeuge.ResumeLayout(false);
            this.grpKrones.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        private GroupBox grpKrones;
        private GroupBox grpSonstigeWerkzeuge;
        private Button btnKplot;
        private Button btnBildzeichenVerwendungAllerZeilen;
        protected  TabPage tabSpezial;
        private GroupBox grpWorkflow;
        private Button btnWFMassenAenderung;
        private GroupBox grpTabelleAdministration;
        private Button btnAlleTabellenLaden;
        private Button btnVorschauVerwaltung;
        private Button btnPreRelease;
        protected Button btnPrio3;
        private Button btnDraftAbleitung;
        private Button btnClearSAPCache;
    }
}
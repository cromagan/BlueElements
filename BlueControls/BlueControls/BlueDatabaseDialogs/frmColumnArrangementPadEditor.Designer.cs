
namespace BlueControls.BlueDatabaseDialogs {
    partial class frmColumnArrangementPadEditor {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.tabAnsichten = new System.Windows.Forms.TabPage();
            this.grpAktuelleAnsicht = new BlueControls.Controls.GroupBox();
            this.btnSystemspaltenAusblenden = new BlueControls.Controls.Button();
            this.btnSpalteEinblenden = new BlueControls.Controls.Button();
            this.btnAlleSpaltenEinblenden = new BlueControls.Controls.Button();
            this.btnBerechtigungsgruppen = new BlueControls.Controls.Button();
            this.grpAnsichtsVerwaltung = new BlueControls.Controls.GroupBox();
            this.btnAnsichtUmbenennen = new BlueControls.Controls.Button();
            this.capAktuellAngezeigteAnsicht = new BlueControls.Controls.Caption();
            this.cbxInternalColumnArrangementSelector = new BlueControls.Controls.ComboBox();
            this.btnNeueAnsichtErstellen = new BlueControls.Controls.Button();
            this.btnAktuelleAnsichtLoeschen = new BlueControls.Controls.Button();
            this.grpDesign.SuspendLayout();
            this.Ribbon.SuspendLayout();
            this.tabStart.SuspendLayout();
            this.grpAssistent.SuspendLayout();
            this.tabAnsichten.SuspendLayout();
            this.grpAktuelleAnsicht.SuspendLayout();
            this.grpAnsichtsVerwaltung.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabRightSide
            // 
            this.tabRightSide.Location = new System.Drawing.Point(428, 110);
            this.tabRightSide.Size = new System.Drawing.Size(372, 340);
            // 
            // Pad
            // 
            this.Pad.Size = new System.Drawing.Size(428, 340);
            // 
            // Ribbon
            // 
            this.Ribbon.Controls.Add(this.tabAnsichten);
            this.Ribbon.Size = new System.Drawing.Size(800, 110);
            this.Ribbon.TabDefault = this.tabAnsichten;
            this.Ribbon.TabDefaultOrder = new string[] {
        "Ansichten",
        "Start"};
            this.Ribbon.Controls.SetChildIndex(this.tabAnsichten, 0);
            this.Ribbon.Controls.SetChildIndex(this.tabExport, 0);
            this.Ribbon.Controls.SetChildIndex(this.tabStart, 0);
            // 
            // tabStart
            // 
            this.tabStart.Size = new System.Drawing.Size(792, 81);
            // 
            // tabAnsichten
            // 
            this.tabAnsichten.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.tabAnsichten.Controls.Add(this.grpAktuelleAnsicht);
            this.tabAnsichten.Controls.Add(this.grpAnsichtsVerwaltung);
            this.tabAnsichten.Location = new System.Drawing.Point(4, 25);
            this.tabAnsichten.Name = "tabAnsichten";
            this.tabAnsichten.Size = new System.Drawing.Size(792, 81);
            this.tabAnsichten.TabIndex = 4;
            this.tabAnsichten.Text = "Ansichten";
            // 
            // grpAktuelleAnsicht
            // 
            this.grpAktuelleAnsicht.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpAktuelleAnsicht.CausesValidation = false;
            this.grpAktuelleAnsicht.Controls.Add(this.btnSystemspaltenAusblenden);
            this.grpAktuelleAnsicht.Controls.Add(this.btnSpalteEinblenden);
            this.grpAktuelleAnsicht.Controls.Add(this.btnAlleSpaltenEinblenden);
            this.grpAktuelleAnsicht.Controls.Add(this.btnBerechtigungsgruppen);
            this.grpAktuelleAnsicht.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpAktuelleAnsicht.GroupBoxStyle = BlueControls.Enums.enGroupBoxStyle.RibbonBar;
            this.grpAktuelleAnsicht.Location = new System.Drawing.Point(432, 0);
            this.grpAktuelleAnsicht.Name = "grpAktuelleAnsicht";
            this.grpAktuelleAnsicht.Size = new System.Drawing.Size(264, 81);
            this.grpAktuelleAnsicht.TabIndex = 5;
            this.grpAktuelleAnsicht.TabStop = false;
            this.grpAktuelleAnsicht.Text = "Aktuelle Ansicht";
            // 
            // btnSystemspaltenAusblenden
            // 
            this.btnSystemspaltenAusblenden.ButtonStyle = ((BlueControls.Enums.enButtonStyle)((BlueControls.Enums.enButtonStyle.Button_Big | BlueControls.Enums.enButtonStyle.Borderless)));
            this.btnSystemspaltenAusblenden.ImageCode = "Lupe|16||1";
            this.btnSystemspaltenAusblenden.Location = new System.Drawing.Point(80, 24);
            this.btnSystemspaltenAusblenden.Name = "btnSystemspaltenAusblenden";
            this.btnSystemspaltenAusblenden.Size = new System.Drawing.Size(176, 22);
            this.btnSystemspaltenAusblenden.TabIndex = 33;
            this.btnSystemspaltenAusblenden.Text = "Systemspalten ausblenden";
            this.btnSystemspaltenAusblenden.Click += new System.EventHandler(this.btnSystemspaltenAusblenden_Click);
            // 
            // btnSpalteEinblenden
            // 
            this.btnSpalteEinblenden.ButtonStyle = ((BlueControls.Enums.enButtonStyle)((BlueControls.Enums.enButtonStyle.Button_Big | BlueControls.Enums.enButtonStyle.Borderless)));
            this.btnSpalteEinblenden.ImageCode = "Lupe";
            this.btnSpalteEinblenden.Location = new System.Drawing.Point(8, 2);
            this.btnSpalteEinblenden.Name = "btnSpalteEinblenden";
            this.btnSpalteEinblenden.Size = new System.Drawing.Size(64, 66);
            this.btnSpalteEinblenden.TabIndex = 7;
            this.btnSpalteEinblenden.Text = "Spalte einblenden";
            this.btnSpalteEinblenden.Click += new System.EventHandler(this.btnSpalteEinblenden_Click);
            // 
            // btnAlleSpaltenEinblenden
            // 
            this.btnAlleSpaltenEinblenden.ButtonStyle = ((BlueControls.Enums.enButtonStyle)((BlueControls.Enums.enButtonStyle.Button_Big | BlueControls.Enums.enButtonStyle.Borderless)));
            this.btnAlleSpaltenEinblenden.ImageCode = "Lupe|16|||FF0000";
            this.btnAlleSpaltenEinblenden.Location = new System.Drawing.Point(80, 2);
            this.btnAlleSpaltenEinblenden.Name = "btnAlleSpaltenEinblenden";
            this.btnAlleSpaltenEinblenden.Size = new System.Drawing.Size(176, 22);
            this.btnAlleSpaltenEinblenden.TabIndex = 3;
            this.btnAlleSpaltenEinblenden.Text = "Alle Spalten einblenden";
            this.btnAlleSpaltenEinblenden.Click += new System.EventHandler(this.btnAlleSpaltenEinblenden_Click);
            // 
            // btnBerechtigungsgruppen
            // 
            this.btnBerechtigungsgruppen.ButtonStyle = ((BlueControls.Enums.enButtonStyle)((BlueControls.Enums.enButtonStyle.Button_Big | BlueControls.Enums.enButtonStyle.Borderless)));
            this.btnBerechtigungsgruppen.ImageCode = "Schild|16";
            this.btnBerechtigungsgruppen.Location = new System.Drawing.Point(80, 46);
            this.btnBerechtigungsgruppen.Name = "btnBerechtigungsgruppen";
            this.btnBerechtigungsgruppen.Size = new System.Drawing.Size(176, 22);
            this.btnBerechtigungsgruppen.TabIndex = 32;
            this.btnBerechtigungsgruppen.Text = "Berechtigungsgruppen";
            this.btnBerechtigungsgruppen.Click += new System.EventHandler(this.btnBerechtigungsgruppen_Click);
            // 
            // grpAnsichtsVerwaltung
            // 
            this.grpAnsichtsVerwaltung.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpAnsichtsVerwaltung.CausesValidation = false;
            this.grpAnsichtsVerwaltung.Controls.Add(this.btnAnsichtUmbenennen);
            this.grpAnsichtsVerwaltung.Controls.Add(this.capAktuellAngezeigteAnsicht);
            this.grpAnsichtsVerwaltung.Controls.Add(this.cbxInternalColumnArrangementSelector);
            this.grpAnsichtsVerwaltung.Controls.Add(this.btnNeueAnsichtErstellen);
            this.grpAnsichtsVerwaltung.Controls.Add(this.btnAktuelleAnsichtLoeschen);
            this.grpAnsichtsVerwaltung.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpAnsichtsVerwaltung.GroupBoxStyle = BlueControls.Enums.enGroupBoxStyle.RibbonBar;
            this.grpAnsichtsVerwaltung.Location = new System.Drawing.Point(0, 0);
            this.grpAnsichtsVerwaltung.Name = "grpAnsichtsVerwaltung";
            this.grpAnsichtsVerwaltung.Size = new System.Drawing.Size(432, 81);
            this.grpAnsichtsVerwaltung.TabIndex = 4;
            this.grpAnsichtsVerwaltung.TabStop = false;
            this.grpAnsichtsVerwaltung.Text = "Ansichtverwaltung";
            // 
            // btnAnsichtUmbenennen
            // 
            this.btnAnsichtUmbenennen.ButtonStyle = ((BlueControls.Enums.enButtonStyle)((BlueControls.Enums.enButtonStyle.Button_Big | BlueControls.Enums.enButtonStyle.Borderless)));
            this.btnAnsichtUmbenennen.ImageCode = "Stift";
            this.btnAnsichtUmbenennen.Location = new System.Drawing.Point(360, 2);
            this.btnAnsichtUmbenennen.Name = "btnAnsichtUmbenennen";
            this.btnAnsichtUmbenennen.Size = new System.Drawing.Size(64, 66);
            this.btnAnsichtUmbenennen.TabIndex = 35;
            this.btnAnsichtUmbenennen.Text = "umbe-nennen";
            this.btnAnsichtUmbenennen.Click += new System.EventHandler(this.btnAnsichtUmbenennen_Click);
            // 
            // capAktuellAngezeigteAnsicht
            // 
            this.capAktuellAngezeigteAnsicht.CausesValidation = false;
            this.capAktuellAngezeigteAnsicht.Location = new System.Drawing.Point(8, 2);
            this.capAktuellAngezeigteAnsicht.Name = "capAktuellAngezeigteAnsicht";
            this.capAktuellAngezeigteAnsicht.Size = new System.Drawing.Size(160, 22);
            this.capAktuellAngezeigteAnsicht.Text = "Aktuell angezeigte Ansicht:";
            this.capAktuellAngezeigteAnsicht.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // cbxInternalColumnArrangementSelector
            // 
            this.cbxInternalColumnArrangementSelector.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxInternalColumnArrangementSelector.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxInternalColumnArrangementSelector.Location = new System.Drawing.Point(8, 24);
            this.cbxInternalColumnArrangementSelector.Name = "cbxInternalColumnArrangementSelector";
            this.cbxInternalColumnArrangementSelector.Size = new System.Drawing.Size(216, 22);
            this.cbxInternalColumnArrangementSelector.TabIndex = 3;
            this.cbxInternalColumnArrangementSelector.ItemClicked += new System.EventHandler<BlueControls.EventArgs.BasicListItemEventArgs>(this.cbxInternalColumnArrangementSelector_ItemClicked);
            // 
            // btnNeueAnsichtErstellen
            // 
            this.btnNeueAnsichtErstellen.ButtonStyle = ((BlueControls.Enums.enButtonStyle)((BlueControls.Enums.enButtonStyle.Button_Big | BlueControls.Enums.enButtonStyle.Borderless)));
            this.btnNeueAnsichtErstellen.ImageCode = "PlusZeichen";
            this.btnNeueAnsichtErstellen.Location = new System.Drawing.Point(232, 2);
            this.btnNeueAnsichtErstellen.Name = "btnNeueAnsichtErstellen";
            this.btnNeueAnsichtErstellen.Size = new System.Drawing.Size(64, 66);
            this.btnNeueAnsichtErstellen.TabIndex = 2;
            this.btnNeueAnsichtErstellen.Text = "neue Ansicht";
            this.btnNeueAnsichtErstellen.Click += new System.EventHandler(this.btnNeueAnsichtErstellen_Click);
            // 
            // btnAktuelleAnsichtLoeschen
            // 
            this.btnAktuelleAnsichtLoeschen.ButtonStyle = ((BlueControls.Enums.enButtonStyle)((BlueControls.Enums.enButtonStyle.Button_Big | BlueControls.Enums.enButtonStyle.Borderless)));
            this.btnAktuelleAnsichtLoeschen.ImageCode = "MinusZeichen";
            this.btnAktuelleAnsichtLoeschen.Location = new System.Drawing.Point(296, 2);
            this.btnAktuelleAnsichtLoeschen.Name = "btnAktuelleAnsichtLoeschen";
            this.btnAktuelleAnsichtLoeschen.Size = new System.Drawing.Size(64, 66);
            this.btnAktuelleAnsichtLoeschen.TabIndex = 6;
            this.btnAktuelleAnsichtLoeschen.Text = "Ansicht löschen";
            this.btnAktuelleAnsichtLoeschen.Click += new System.EventHandler(this.btnAktuelleAnsichtLoeschen_Click);
            // 
            // frmColumnArrangementPadEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Name = "frmColumnArrangementPadEditor";
            this.Text = "frmColumnArrangementPadEditor";
            this.grpDesign.ResumeLayout(false);
            this.Ribbon.ResumeLayout(false);
            this.tabStart.ResumeLayout(false);
            this.grpAssistent.ResumeLayout(false);
            this.tabAnsichten.ResumeLayout(false);
            this.grpAktuelleAnsicht.ResumeLayout(false);
            this.grpAnsichtsVerwaltung.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabPage tabAnsichten;
        internal Controls.GroupBox grpAnsichtsVerwaltung;
        private Controls.Button btnAnsichtUmbenennen;
        internal Controls.Caption capAktuellAngezeigteAnsicht;
        private Controls.ComboBox cbxInternalColumnArrangementSelector;
        private Controls.Button btnNeueAnsichtErstellen;
        private Controls.Button btnAktuelleAnsichtLoeschen;
        internal Controls.GroupBox grpAktuelleAnsicht;
        private Controls.Button btnSystemspaltenAusblenden;
        private Controls.Button btnSpalteEinblenden;
        private Controls.Button btnAlleSpaltenEinblenden;
        private Controls.Button btnBerechtigungsgruppen;
    }
}
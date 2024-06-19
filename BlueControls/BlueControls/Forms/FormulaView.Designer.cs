using System.ComponentModel;
using System.Windows.Forms;
using BlueControls.Controls;
using Button = BlueControls.Controls.Button;
using ComboBox = BlueControls.Controls.ComboBox;
using GroupBox = BlueControls.Controls.GroupBox;
using TextBox = BlueControls.Controls.TextBox;

namespace BlueControls.Forms {
    partial class FormulaView {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing) {
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.ribMain = new BlueControls.Controls.RibbonBar();
            this.tabFile = new System.Windows.Forms.TabPage();
            this.grpDatei = new BlueControls.Controls.GroupBox();
            this.btnLastFormulas = new BlueControls.Controls.LastFilesCombo();
            this.btnOeffnen = new BlueControls.Controls.Button();
            this.tabAllgemein = new System.Windows.Forms.TabPage();
            this.tabAdmin = new System.Windows.Forms.TabPage();
            this.grpAdminAllgemein = new BlueControls.Controls.GroupBox();
            this.btnSaveLoad = new BlueControls.Controls.Button();
            this.grpAdminBearbeiten = new BlueControls.Controls.GroupBox();
            this.btnFormular = new BlueControls.Controls.Button();
            this.LoadTab = new System.Windows.Forms.OpenFileDialog();
            this.CFormula = new BlueControls.Controls.ConnectedFormulaView();
            this.pnlStatusBar.SuspendLayout();
            this.ribMain.SuspendLayout();
            this.tabFile.SuspendLayout();
            this.grpDatei.SuspendLayout();
            this.tabAdmin.SuspendLayout();
            this.grpAdminAllgemein.SuspendLayout();
            this.grpAdminBearbeiten.SuspendLayout();
            this.SuspendLayout();
            // 
            // capStatusBar
            // 
            this.capStatusBar.Size = new System.Drawing.Size(1202, 24);
            // 
            // pnlStatusBar
            // 
            this.pnlStatusBar.Location = new System.Drawing.Point(0, 426);
            this.pnlStatusBar.Size = new System.Drawing.Size(1202, 24);
            // 
            // ribMain
            // 
            this.ribMain.Controls.Add(this.tabFile);
            this.ribMain.Controls.Add(this.tabAllgemein);
            this.ribMain.Controls.Add(this.tabAdmin);
            this.ribMain.Dock = System.Windows.Forms.DockStyle.Top;
            this.ribMain.HotTrack = true;
            this.ribMain.Location = new System.Drawing.Point(0, 0);
            this.ribMain.Name = "ribMain";
            this.ribMain.SelectedIndex = 1;
            this.ribMain.Size = new System.Drawing.Size(1202, 110);
            this.ribMain.TabDefault = this.tabFile;
            this.ribMain.TabDefaultOrder = null;
            this.ribMain.TabIndex = 97;
            // 
            // tabFile
            // 
            this.tabFile.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.tabFile.Controls.Add(this.grpDatei);
            this.tabFile.Location = new System.Drawing.Point(4, 25);
            this.tabFile.Margin = new System.Windows.Forms.Padding(0);
            this.tabFile.Name = "tabFile";
            this.tabFile.Size = new System.Drawing.Size(1194, 81);
            this.tabFile.TabIndex = 3;
            this.tabFile.Text = "Datei";
            // 
            // grpDatei
            // 
            this.grpDatei.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpDatei.CausesValidation = false;
            this.grpDatei.Controls.Add(this.btnLastFormulas);
            this.grpDatei.Controls.Add(this.btnOeffnen);
            this.grpDatei.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpDatei.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpDatei.Location = new System.Drawing.Point(0, 0);
            this.grpDatei.Name = "grpDatei";
            this.grpDatei.Size = new System.Drawing.Size(176, 81);
            this.grpDatei.TabIndex = 4;
            this.grpDatei.TabStop = false;
            this.grpDatei.Text = "Datei";
            // 
            // btnLastFormulas
            // 
            this.btnLastFormulas.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.btnLastFormulas.DrawStyle = BlueControls.Enums.ComboboxStyle.RibbonBar;
            this.btnLastFormulas.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.btnLastFormulas.Enabled = false;
            this.btnLastFormulas.ImageCode = "Ordner";
            this.btnLastFormulas.Location = new System.Drawing.Point(64, 2);
            this.btnLastFormulas.Name = "btnLastFormulas";
            this.btnLastFormulas.Size = new System.Drawing.Size(104, 66);
            this.btnLastFormulas.TabIndex = 1;
            this.btnLastFormulas.Text = "zuletzt geöffnete Dateien";
            this.btnLastFormulas.ItemClicked += new System.EventHandler<BlueControls.EventArgs.AbstractListItemEventArgs>(this.btnLetzteDateien_ItemClicked);
            // 
            // btnOeffnen
            // 
            this.btnOeffnen.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnOeffnen.ImageCode = "Ordner";
            this.btnOeffnen.Location = new System.Drawing.Point(8, 2);
            this.btnOeffnen.Name = "btnOeffnen";
            this.btnOeffnen.Size = new System.Drawing.Size(56, 66);
            this.btnOeffnen.TabIndex = 1;
            this.btnOeffnen.Text = "Öffnen";
            this.btnOeffnen.Click += new System.EventHandler(this.btnOeffnen_Click);
            // 
            // tabAllgemein
            // 
            this.tabAllgemein.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.tabAllgemein.Location = new System.Drawing.Point(4, 25);
            this.tabAllgemein.Name = "tabAllgemein";
            this.tabAllgemein.Size = new System.Drawing.Size(1194, 81);
            this.tabAllgemein.TabIndex = 1;
            this.tabAllgemein.Text = "Allgemein";
            // 
            // tabAdmin
            // 
            this.tabAdmin.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.tabAdmin.Controls.Add(this.grpAdminAllgemein);
            this.tabAdmin.Controls.Add(this.grpAdminBearbeiten);
            this.tabAdmin.Location = new System.Drawing.Point(4, 25);
            this.tabAdmin.Name = "tabAdmin";
            this.tabAdmin.Size = new System.Drawing.Size(1194, 81);
            this.tabAdmin.TabIndex = 0;
            this.tabAdmin.Text = "Administration";
            // 
            // grpAdminAllgemein
            // 
            this.grpAdminAllgemein.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpAdminAllgemein.CausesValidation = false;
            this.grpAdminAllgemein.Controls.Add(this.btnSaveLoad);
            this.grpAdminAllgemein.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpAdminAllgemein.Enabled = false;
            this.grpAdminAllgemein.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpAdminAllgemein.Location = new System.Drawing.Point(72, 0);
            this.grpAdminAllgemein.Name = "grpAdminAllgemein";
            this.grpAdminAllgemein.Size = new System.Drawing.Size(72, 81);
            this.grpAdminAllgemein.TabIndex = 7;
            this.grpAdminAllgemein.TabStop = false;
            this.grpAdminAllgemein.Text = "Allgemein";
            // 
            // btnSaveLoad
            // 
            this.btnSaveLoad.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnSaveLoad.ImageCode = "Diskette|16";
            this.btnSaveLoad.Location = new System.Drawing.Point(8, 2);
            this.btnSaveLoad.Name = "btnSaveLoad";
            this.btnSaveLoad.QuickInfo = "Aktualisiert die Datenbank-Daten. (Speichern, neu Laden)";
            this.btnSaveLoad.Size = new System.Drawing.Size(56, 66);
            this.btnSaveLoad.TabIndex = 43;
            this.btnSaveLoad.Text = "Daten aktual.";
            // 
            // grpAdminBearbeiten
            // 
            this.grpAdminBearbeiten.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpAdminBearbeiten.CausesValidation = false;
            this.grpAdminBearbeiten.Controls.Add(this.btnFormular);
            this.grpAdminBearbeiten.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpAdminBearbeiten.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpAdminBearbeiten.Location = new System.Drawing.Point(0, 0);
            this.grpAdminBearbeiten.Name = "grpAdminBearbeiten";
            this.grpAdminBearbeiten.Size = new System.Drawing.Size(72, 81);
            this.grpAdminBearbeiten.TabIndex = 9;
            this.grpAdminBearbeiten.TabStop = false;
            this.grpAdminBearbeiten.Text = "Bearbeiten";
            // 
            // btnFormular
            // 
            this.btnFormular.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnFormular.ImageCode = "Textfeld|16|||||||||Stift";
            this.btnFormular.Location = new System.Drawing.Point(8, 2);
            this.btnFormular.Name = "btnFormular";
            this.btnFormular.Size = new System.Drawing.Size(56, 66);
            this.btnFormular.TabIndex = 44;
            this.btnFormular.Text = "Formular-Editor";
            this.btnFormular.Click += new System.EventHandler(this.btnFormular_Click);
            // 
            // LoadTab
            // 
            this.LoadTab.Filter = "*.CFO Formulare|*.CFO|*.* Alle Dateien|*";
            this.LoadTab.Title = "Bitte Datenbank laden!";
            this.LoadTab.FileOk += new System.ComponentModel.CancelEventHandler(this.LoadTab_FileOk);
            // 
            // CFormula
            // 
            this.CFormula.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CFormula.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.Nothing;
            this.CFormula.Location = new System.Drawing.Point(0, 110);
            this.CFormula.Mode = "";
            this.CFormula.Name = "CFormula";
            this.CFormula.Size = new System.Drawing.Size(1202, 316);
            this.CFormula.TabIndex = 98;
            this.CFormula.TabStop = false;
            this.CFormula.Text = "CFO";
            // 
            // FormulaView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1202, 450);
            this.Controls.Add(this.CFormula);
            this.Controls.Add(this.ribMain);
            this.Name = "FormulaView";
            this.Text = "FormulaView";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Controls.SetChildIndex(this.pnlStatusBar, 0);
            this.Controls.SetChildIndex(this.ribMain, 0);
            this.Controls.SetChildIndex(this.CFormula, 0);
            this.pnlStatusBar.ResumeLayout(false);
            this.ribMain.ResumeLayout(false);
            this.tabFile.ResumeLayout(false);
            this.grpDatei.ResumeLayout(false);
            this.tabAdmin.ResumeLayout(false);
            this.grpAdminAllgemein.ResumeLayout(false);
            this.grpAdminBearbeiten.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        protected RibbonBar ribMain;
        private TabPage tabFile;
        private GroupBox grpDatei;
        private LastFilesCombo btnLastFormulas;
        private Button btnOeffnen;
        protected TabPage tabAllgemein;
        protected TabPage tabAdmin;
        private GroupBox grpAdminAllgemein;
        private Button btnSaveLoad;
        private GroupBox grpAdminBearbeiten;
        private Button btnFormular;
        private OpenFileDialog LoadTab;
        protected ConnectedFormulaView CFormula;
    }
}
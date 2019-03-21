using System;
using System.Diagnostics;
using System.Drawing;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Forms;

namespace BlueControls.BlueDatabaseDialogs
    {


        internal sealed partial class Search 
        {
			//Das Formular überschreibt den Deletevorgang, um die Komponentenliste zu bereinigen.
			[DebuggerNonUserCode()]
			protected override void Dispose(bool disposing)
			{
				if (disposing )
				{

				}
				base.Dispose(disposing);
			}



			//Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
			//Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
			//Das Bearbeiten mit dem Code-Editor ist nicht möglich.
			[DebuggerStepThrough()]
			private void InitializeComponent()
			{
            this.capSonderzeichen = new BlueControls.Controls.Caption();
            this.btnSuchInCell = new BlueControls.Controls.Button();
            this.txbSuchText = new BlueControls.Controls.TextBox();
            this.grpSonderzeichen = new BlueControls.Controls.GroupBox();
            this.caption1 = new BlueControls.Controls.Caption();
            this.btnAehnliches = new BlueControls.Controls.Button();
            this.btnSuchSpalte = new BlueControls.Controls.Button();
            this.grpSonderzeichen.SuspendLayout();
            this.SuspendLayout();
            // 
            // capSonderzeichen
            // 
            this.capSonderzeichen.CausesValidation = false;
            this.capSonderzeichen.Location = new System.Drawing.Point(8, 16);
            this.capSonderzeichen.Name = "capSonderzeichen";
            this.capSonderzeichen.Size = new System.Drawing.Size(128, 32);
            this.capSonderzeichen.Text = ";cr; = Zeilenumbruch<br>;tab; = Tabulator";
            this.capSonderzeichen.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // btnSuchInCell
            // 
            this.btnSuchInCell.ImageCode = "Lupe|20";
            this.btnSuchInCell.Location = new System.Drawing.Point(392, 80);
            this.btnSuchInCell.Name = "btnSuchInCell";
            this.btnSuchInCell.Size = new System.Drawing.Size(184, 32);
            this.btnSuchInCell.TabIndex = 4;
            this.btnSuchInCell.Text = "Suche in sichtbaren Zellen";
            this.btnSuchInCell.Click += new System.EventHandler(this.btnSuchInCell_Click);
            // 
            // txbSuchText
            // 
            this.txbSuchText.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbSuchText.Location = new System.Drawing.Point(8, 24);
            this.txbSuchText.Name = "txbSuchText";
            this.txbSuchText.Size = new System.Drawing.Size(568, 24);
            this.txbSuchText.TabIndex = 2;
            this.txbSuchText.TextChanged += new System.EventHandler(this.txbSuchText_TextChanged);
            this.txbSuchText.Enter += new System.EventHandler(this.txbSuchText_Enter);
            // 
            // grpSonderzeichen
            // 
            this.grpSonderzeichen.CausesValidation = false;
            this.grpSonderzeichen.Controls.Add(this.capSonderzeichen);
            this.grpSonderzeichen.Location = new System.Drawing.Point(8, 56);
            this.grpSonderzeichen.Name = "grpSonderzeichen";
            this.grpSonderzeichen.Size = new System.Drawing.Size(144, 56);
            this.grpSonderzeichen.Text = "Sonderzeichen";
            // 
            // caption1
            // 
            this.caption1.CausesValidation = false;
            this.caption1.Location = new System.Drawing.Point(8, 8);
            this.caption1.Name = "caption1";
            this.caption1.Size = new System.Drawing.Size(136, 16);
            this.caption1.Text = "Suchtext:";
            // 
            // btnAehnliches
            // 
            this.btnAehnliches.ButtonStyle = BlueControls.Enums.enButtonStyle.Checkbox_Text;
            this.btnAehnliches.Location = new System.Drawing.Point(192, 56);
            this.btnAehnliches.Name = "btnAehnliches";
            this.btnAehnliches.Size = new System.Drawing.Size(384, 16);
            this.btnAehnliches.TabIndex = 6;
            this.btnAehnliches.Text = "auch Ähnliches finden (z.B. ue = ü)";
            // 
            // btnSuchSpalte
            // 
            this.btnSuchSpalte.ImageCode = "Lupe|20|||||||||Spalte";
            this.btnSuchSpalte.Location = new System.Drawing.Point(192, 80);
            this.btnSuchSpalte.Name = "btnSuchSpalte";
            this.btnSuchSpalte.Size = new System.Drawing.Size(184, 32);
            this.btnSuchSpalte.TabIndex = 5;
            this.btnSuchSpalte.Text = "Suche Spaltennamen";
            this.btnSuchSpalte.Click += new System.EventHandler(this.btnSuchSpalte_Click);
            // 
            // Search
            // 
            this.ClientSize = new System.Drawing.Size(584, 117);
            this.Controls.Add(this.btnAehnliches);
            this.Controls.Add(this.txbSuchText);
            this.Controls.Add(this.btnSuchSpalte);
            this.Controls.Add(this.caption1);
            this.Controls.Add(this.grpSonderzeichen);
            this.Controls.Add(this.btnSuchInCell);
            this.Design = BlueControls.Enums.enDesign.Form_Standard;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "Search";
            this.ShowInTaskbar = false;
            this.Text = "Suchen";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.Search_Load);
            this.grpSonderzeichen.ResumeLayout(false);
            this.ResumeLayout(false);

			}
			private TextBox txbSuchText;
			private Button btnSuchInCell;
			private Caption capSonderzeichen;
			private GroupBox grpSonderzeichen;
        private Caption caption1;
        private Button btnAehnliches;
        private Button btnSuchSpalte;
    }
	}

using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Controls;
using BlueControls.Enums;
using Button = BlueControls.Controls.Button;
using GroupBox = BlueControls.Controls.GroupBox;
using TextBox = BlueControls.Controls.TextBox;

namespace BlueControls.BlueTableDialogs
    {
    public sealed partial class OpenSearchInCells 
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
            this.capSonderzeichen = new Caption();
            this.btnSuchInCell = new Button();
            this.txbSuchText = new TextBox();
            this.grpSonderzeichen = new GroupBox();
            this.caption1 = new Caption();
            this.btnAehnliches = new Button();
            this.btnSuchSpalte = new Button();
            this.grpSonderzeichen.SuspendLayout();
            this.SuspendLayout();
            // 
            // capSonderzeichen
            // 
            this.capSonderzeichen.CausesValidation = false;
            this.capSonderzeichen.Location = new Point(8, 16);
            this.capSonderzeichen.Name = "capSonderzeichen";
            this.capSonderzeichen.Size = new Size(128, 32);
            this.capSonderzeichen.Text = ";cr; = Zeilenumbruch<br>;tab; = Tabulator";
            // 
            // btnSuchInCell
            // 
            this.btnSuchInCell.ImageCode = "Lupe|20";
            this.btnSuchInCell.Location = new Point(392, 80);
            this.btnSuchInCell.Name = "btnSuchInCell";
            this.btnSuchInCell.Size = new Size(184, 32);
            this.btnSuchInCell.TabIndex = 4;
            this.btnSuchInCell.Text = "Suche in sichtbaren Zellen";
            this.btnSuchInCell.Click += new EventHandler(this.btnSuchInCell_Click);
            // 
            // txbSuchText
            // 
            this.txbSuchText.Cursor = Cursors.IBeam;
            this.txbSuchText.Location = new Point(8, 24);
            this.txbSuchText.Name = "txbSuchText";
            this.txbSuchText.Size = new Size(568, 24);
            this.txbSuchText.TabIndex = 2;
            this.txbSuchText.Enter += new EventHandler(this.txbSuchText_Enter);
            this.txbSuchText.TextChanged += new EventHandler(this.txbSuchText_TextChanged);
            // 
            // grpSonderzeichen
            // 
            this.grpSonderzeichen.BackColor = Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.grpSonderzeichen.CausesValidation = false;
            this.grpSonderzeichen.Controls.Add(this.capSonderzeichen);
            this.grpSonderzeichen.Location = new Point(8, 56);
            this.grpSonderzeichen.Name = "grpSonderzeichen";
            this.grpSonderzeichen.Size = new Size(144, 56);
            this.grpSonderzeichen.TabIndex = 7;
            this.grpSonderzeichen.TabStop = false;
            this.grpSonderzeichen.Text = "Sonderzeichen";
            // 
            // caption1
            // 
            this.caption1.CausesValidation = false;
            this.caption1.Location = new Point(8, 8);
            this.caption1.Name = "caption1";
            this.caption1.Size = new Size(136, 16);
            this.caption1.Text = "Suchtext:";
            // 
            // btnAehnliches
            // 
            this.btnAehnliches.ButtonStyle = ButtonStyle.Checkbox_Text;
            this.btnAehnliches.Location = new Point(192, 56);
            this.btnAehnliches.Name = "btnAehnliches";
            this.btnAehnliches.Size = new Size(384, 16);
            this.btnAehnliches.TabIndex = 6;
            this.btnAehnliches.Text = "auch Ähnliches finden (z.B. ue = ü)";
            // 
            // btnSuchSpalte
            // 
            this.btnSuchSpalte.ImageCode = "Lupe|20|||||||||Spalte";
            this.btnSuchSpalte.Location = new Point(192, 80);
            this.btnSuchSpalte.Name = "btnSuchSpalte";
            this.btnSuchSpalte.Size = new Size(184, 32);
            this.btnSuchSpalte.TabIndex = 5;
            this.btnSuchSpalte.Text = "Suche Spaltennamen";
            this.btnSuchSpalte.Click += new EventHandler(this.btnSuchSpalte_Click);
            // 
            // Search
            // 
            this.ClientSize = new Size(584, 117);
            this.Controls.Add(this.btnAehnliches);
            this.Controls.Add(this.txbSuchText);
            this.Controls.Add(this.btnSuchSpalte);
            this.Controls.Add(this.caption1);
            this.Controls.Add(this.grpSonderzeichen);
            this.Controls.Add(this.btnSuchInCell);
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.Name = "Search";
            this.ShowInTaskbar = false;
            this.Text = "Suchen";
            this.Load += new EventHandler(this.Search_Load);
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

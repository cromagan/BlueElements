using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Controls;
using BlueControls.Enums;
using Button = BlueControls.Controls.Button;
using GroupBox = BlueControls.Controls.GroupBox;
using TextBox = BlueControls.Controls.TextBox;

namespace BlueControls.BlueDatabaseDialogs
{
    public sealed partial class RowCleanUp {
        //Das Formular überschreibt den Deletevorgang, um die Komponentenliste zu bereinigen.
        [DebuggerNonUserCode()]
        protected override void Dispose(bool disposing)
        {
            if (disposing)
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
            this.btnExecute = new BlueControls.Controls.Button();
            this.txtInfo = new BlueControls.Controls.Caption();
            this.btnCancel = new BlueControls.Controls.Button();
            this.lstColumns = new BlueControls.Controls.ListBox();
            this.capSpalten = new BlueControls.Controls.Caption();
            this.grpVerhalten = new BlueControls.Controls.GroupBox();
            this.optLöschen = new BlueControls.Controls.Button();
            this.optFülle = new BlueControls.Controls.Button();
            this.pnlStatusBar.SuspendLayout();
            this.grpVerhalten.SuspendLayout();
            this.SuspendLayout();
            // 
            // capStatusBar
            // 
            this.capStatusBar.Size = new System.Drawing.Size(731, 24);
            // 
            // pnlStatusBar
            // 
            this.pnlStatusBar.Location = new System.Drawing.Point(0, 289);
            this.pnlStatusBar.Size = new System.Drawing.Size(731, 24);
            // 
            // btnExecute
            // 
            this.btnExecute.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExecute.Enabled = false;
            this.btnExecute.ImageCode = "Abspielen|16";
            this.btnExecute.Location = new System.Drawing.Point(616, 252);
            this.btnExecute.Name = "btnExecute";
            this.btnExecute.Size = new System.Drawing.Size(104, 32);
            this.btnExecute.TabIndex = 9;
            this.btnExecute.Text = "Ausführen";
            this.btnExecute.Click += new System.EventHandler(this.Fertig_Click);
            // 
            // txtInfo
            // 
            this.txtInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtInfo.CausesValidation = false;
            this.txtInfo.Location = new System.Drawing.Point(288, 8);
            this.txtInfo.Name = "txtInfo";
            this.txtInfo.Size = new System.Drawing.Size(432, 40);
            this.txtInfo.TextAnzeigeVerhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.ImageCode = "Kreuz|16";
            this.btnCancel.Location = new System.Drawing.Point(512, 252);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(104, 32);
            this.btnCancel.TabIndex = 11;
            this.btnCancel.Text = "Abbrechen";
            this.btnCancel.Click += new System.EventHandler(this.Cancel_Click);
            // 
            // lstColumns
            // 
            this.lstColumns.AddAllowed = BlueControls.Enums.AddType.None;
            this.lstColumns.Appearance = BlueControls.Enums.ListBoxAppearance.Listbox_Boxes;
            this.lstColumns.CheckBehavior = BlueControls.Enums.CheckBehavior.MultiSelection;
            this.lstColumns.Location = new System.Drawing.Point(8, 64);
            this.lstColumns.Name = "lstColumns";
            this.lstColumns.Size = new System.Drawing.Size(272, 224);
            this.lstColumns.TabIndex = 101;
            this.lstColumns.ItemClicked += new System.EventHandler<BlueControls.EventArgs.AbstractListItemEventArgs>(this.lstColumns_ItemClicked);
            // 
            // capSpalten
            // 
            this.capSpalten.CausesValidation = false;
            this.capSpalten.Location = new System.Drawing.Point(8, 8);
            this.capSpalten.Name = "capSpalten";
            this.capSpalten.Size = new System.Drawing.Size(272, 56);
            this.capSpalten.Text = "<b><u>Spalten</b></u>,<br>die verwendet werden, um Zeilen, die mehrfach vorkommen" +
    ", zu identifizieren:";
            this.capSpalten.TextAnzeigeVerhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // grpVerhalten
            // 
            this.grpVerhalten.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.grpVerhalten.Controls.Add(this.optLöschen);
            this.grpVerhalten.Controls.Add(this.optFülle);
            this.grpVerhalten.Location = new System.Drawing.Point(288, 56);
            this.grpVerhalten.Name = "grpVerhalten";
            this.grpVerhalten.Size = new System.Drawing.Size(432, 128);
            this.grpVerhalten.TabIndex = 102;
            this.grpVerhalten.TabStop = false;
            this.grpVerhalten.Text = "Verhalten";
            // 
            // optLöschen
            // 
            this.optLöschen.ButtonStyle = BlueControls.Enums.ButtonStyle.Optionbox_Text;
            this.optLöschen.Location = new System.Drawing.Point(8, 64);
            this.optLöschen.Name = "optLöschen";
            this.optLöschen.Size = new System.Drawing.Size(416, 56);
            this.optLöschen.TabIndex = 2;
            this.optLöschen.Text = "Die jüngste(n) Zeile <b>löschen</b><i><br>Sind die Zeilen gleich alt, wird zufäll" +
    "ig eine gewählt<br>Um alle doppelten zu löschen, Befehl mehrfach ausführen.";
            // 
            // optFülle
            // 
            this.optFülle.ButtonStyle = BlueControls.Enums.ButtonStyle.Optionbox_Text;
            this.optFülle.Checked = true;
            this.optFülle.Location = new System.Drawing.Point(8, 24);
            this.optFülle.Name = "optFülle";
            this.optFülle.Size = new System.Drawing.Size(416, 40);
            this.optFülle.TabIndex = 1;
            this.optFülle.Text = "Leere Zellen mit Werten anderer Zeilen <b>befüllen</b><br><i>Dardurch entstehen e" +
    "vtl. doppelte Zeilen.";
            // 
            // RowCleanUp
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(731, 313);
            this.Controls.Add(this.grpVerhalten);
            this.Controls.Add(this.capSpalten);
            this.Controls.Add(this.lstColumns);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnExecute);
            this.Controls.Add(this.txtInfo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "RowCleanUp";
            this.ShowInTaskbar = false;
            this.Text = "Zeilen aufräumen:";
            this.Controls.SetChildIndex(this.txtInfo, 0);
            this.Controls.SetChildIndex(this.btnExecute, 0);
            this.Controls.SetChildIndex(this.btnCancel, 0);
            this.Controls.SetChildIndex(this.pnlStatusBar, 0);
            this.Controls.SetChildIndex(this.lstColumns, 0);
            this.Controls.SetChildIndex(this.capSpalten, 0);
            this.Controls.SetChildIndex(this.grpVerhalten, 0);
            this.pnlStatusBar.ResumeLayout(false);
            this.grpVerhalten.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        private Caption txtInfo;
        private Button btnExecute;
        private Button btnCancel;
        private Controls.ListBox lstColumns;
        private Caption capSpalten;
        private GroupBox grpVerhalten;
        private Button optLöschen;
        private Button optFülle;
    }
}

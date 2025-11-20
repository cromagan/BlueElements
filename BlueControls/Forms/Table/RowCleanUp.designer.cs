using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;
using Button = BlueControls.Controls.Button;
using GroupBox = BlueControls.Controls.GroupBox;
using ListBox = BlueControls.Controls.ListBox;

namespace BlueControls.BlueTableDialogs
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
            this.btnExecute = new Button();
            this.txtInfo = new Caption();
            this.btnCancel = new Button();
            this.lstColumns = new ListBox();
            this.capSpalten = new Caption();
            this.grpVerhalten = new GroupBox();
            this.optLöschen = new Button();
            this.optFülle = new Button();
            this.pnlStatusBar.SuspendLayout();
            this.grpVerhalten.SuspendLayout();
            this.SuspendLayout();
            // 
            // capStatusBar
            // 
            this.capStatusBar.Size = new Size(731, 24);
            // 
            // pnlStatusBar
            // 
            this.pnlStatusBar.Location = new Point(0, 289);
            this.pnlStatusBar.Size = new Size(731, 24);
            // 
            // btnExecute
            // 
            this.btnExecute.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
            this.btnExecute.Enabled = false;
            this.btnExecute.ImageCode = "Abspielen|16";
            this.btnExecute.Location = new Point(616, 252);
            this.btnExecute.Name = "btnExecute";
            this.btnExecute.Size = new Size(104, 32);
            this.btnExecute.TabIndex = 9;
            this.btnExecute.Text = "Ausführen";
            this.btnExecute.Click += new EventHandler(this.Fertig_Click);
            // 
            // txtInfo
            // 
            this.txtInfo.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) 
                                                   | AnchorStyles.Right)));
            this.txtInfo.CausesValidation = false;
            this.txtInfo.Location = new Point(288, 8);
            this.txtInfo.Name = "txtInfo";
            this.txtInfo.Size = new Size(432, 40);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
            this.btnCancel.ImageCode = "Kreuz|16";
            this.btnCancel.Location = new Point(512, 252);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new Size(104, 32);
            this.btnCancel.TabIndex = 11;
            this.btnCancel.Text = "Abbrechen";
            this.btnCancel.Click += new EventHandler(this.Cancel_Click);
            // 
            // lstColumns
            // 
            this.lstColumns.AddAllowed = AddType.None;
            this.lstColumns.Appearance = ListBoxAppearance.Listbox_Boxes;
            this.lstColumns.CheckBehavior = CheckBehavior.MultiSelection;
            this.lstColumns.Location = new Point(8, 64);
            this.lstColumns.Name = "lstColumns";
            this.lstColumns.Size = new Size(272, 224);
            this.lstColumns.TabIndex = 101;
            this.lstColumns.ItemClicked += new EventHandler<AbstractListItemEventArgs>(this.lstColumns_ItemClicked);
            // 
            // capSpalten
            // 
            this.capSpalten.CausesValidation = false;
            this.capSpalten.Location = new Point(8, 8);
            this.capSpalten.Name = "capSpalten";
            this.capSpalten.Size = new Size(272, 56);
            this.capSpalten.Text = "<b><u>Spalten</b></u>,<br>die verwendet werden, um Zeilen, die mehrfach vorkommen" +
    ", zu identifizieren:";
            // 
            // grpVerhalten
            // 
            this.grpVerhalten.BackColor = Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.grpVerhalten.Controls.Add(this.optLöschen);
            this.grpVerhalten.Controls.Add(this.optFülle);
            this.grpVerhalten.Location = new Point(288, 56);
            this.grpVerhalten.Name = "grpVerhalten";
            this.grpVerhalten.Size = new Size(432, 128);
            this.grpVerhalten.TabIndex = 102;
            this.grpVerhalten.TabStop = false;
            this.grpVerhalten.Text = "Verhalten";
            // 
            // optLöschen
            // 
            this.optLöschen.ButtonStyle = ButtonStyle.Optionbox_Text;
            this.optLöschen.Location = new Point(8, 64);
            this.optLöschen.Name = "optLöschen";
            this.optLöschen.Size = new Size(416, 56);
            this.optLöschen.TabIndex = 2;
            this.optLöschen.Text = "Die jüngste(n) Zeile <b>löschen</b><i><br>Sind die Zeilen gleich alt, wird zufäll" +
    "ig eine gewählt<br>Um alle doppelten zu löschen, Befehl mehrfach ausführen.";
            // 
            // optFülle
            // 
            this.optFülle.ButtonStyle = ButtonStyle.Optionbox_Text;
            this.optFülle.Checked = true;
            this.optFülle.Location = new Point(8, 24);
            this.optFülle.Name = "optFülle";
            this.optFülle.Size = new Size(416, 40);
            this.optFülle.TabIndex = 1;
            this.optFülle.Text = "Leere Zellen mit Werten anderer Zeilen <b>befüllen</b><br><i>Dardurch entstehen e" +
    "vtl. doppelte Zeilen.";
            // 
            // RowCleanUp
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new Size(731, 313);
            this.Controls.Add(this.grpVerhalten);
            this.Controls.Add(this.capSpalten);
            this.Controls.Add(this.lstColumns);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnExecute);
            this.Controls.Add(this.txtInfo);
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
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
        private ListBox lstColumns;
        private Caption capSpalten;
        private GroupBox grpVerhalten;
        private Button optLöschen;
        private Button optFülle;
    }
}

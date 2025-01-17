using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Controls;
using BlueControls.Enums;
using Button = BlueControls.Controls.Button;
using ComboBox = BlueControls.Controls.ComboBox;

namespace BlueControls.BlueDatabaseDialogs
{
    public sealed partial class ImportBdb {
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
            this.btnImport = new Button();
            this.txtInfo = new Caption();
            this.btnCancel = new Button();
            this.LoadTab = new OpenFileDialog();
            this.btnDatenbankwählen = new Button();
            this.cbxColDateiname = new ComboBox();
            this.capColumn = new Caption();
            this.btnDateienlöschen = new Button();
            this.pnlStatusBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // capStatusBar
            // 
            this.capStatusBar.Size = new Size(731, 24);
            // 
            // pnlStatusBar
            // 
            this.pnlStatusBar.Location = new Point(0, 140);
            this.pnlStatusBar.Size = new Size(731, 24);
            // 
            // btnImport
            // 
            this.btnImport.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
            this.btnImport.Enabled = false;
            this.btnImport.ImageCode = "Textfeld|16";
            this.btnImport.Location = new Point(621, 102);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new Size(104, 32);
            this.btnImport.TabIndex = 9;
            this.btnImport.Text = "Importieren";
            this.btnImport.Click += new EventHandler(this.Fertig_Click);
            // 
            // txtInfo
            // 
            this.txtInfo.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) 
                                                   | AnchorStyles.Right)));
            this.txtInfo.CausesValidation = false;
            this.txtInfo.Location = new Point(200, 8);
            this.txtInfo.Name = "txtInfo";
            this.txtInfo.Size = new Size(528, 40);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
            this.btnCancel.ImageCode = "Kreuz|16";
            this.btnCancel.Location = new Point(509, 102);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new Size(104, 32);
            this.btnCancel.TabIndex = 11;
            this.btnCancel.Text = "Abbrechen";
            this.btnCancel.Click += new EventHandler(this.Cancel_Click);
            // 
            // LoadTab
            // 
            this.LoadTab.Filter = "Datenbanken (*.BDB;*.MBDB)|*.BDB;*.MBDB;*.CBDB|Alle Dateien (*.*)|*.*";
            this.LoadTab.Multiselect = true;
            this.LoadTab.Title = "Bitte Datenbank wählen.";
            this.LoadTab.FileOk += new CancelEventHandler(this.LoadTab_FileOk);
            // 
            // btnDatenbankwählen
            // 
            this.btnDatenbankwählen.ImageCode = "Datenbank|24";
            this.btnDatenbankwählen.Location = new Point(8, 8);
            this.btnDatenbankwählen.Name = "btnDatenbankwählen";
            this.btnDatenbankwählen.Size = new Size(184, 40);
            this.btnDatenbankwählen.TabIndex = 97;
            this.btnDatenbankwählen.Text = "Datenbank(en) wählen";
            this.btnDatenbankwählen.Click += new EventHandler(this.btnDatenbankwählen_Click);
            // 
            // cbxColDateiname
            // 
            this.cbxColDateiname.Cursor = Cursors.IBeam;
            this.cbxColDateiname.Location = new Point(208, 56);
            this.cbxColDateiname.Name = "cbxColDateiname";
            this.cbxColDateiname.QuickInfo = "In diese Spalte wird der Dateiname\r\nder importierten Datei geschrieben.";
            this.cbxColDateiname.Size = new Size(248, 24);
            this.cbxColDateiname.TabIndex = 98;
            this.cbxColDateiname.TextChanged += new EventHandler(this.cbxColDateiname_TextChanged);
            // 
            // capColumn
            // 
            this.capColumn.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) 
                                                     | AnchorStyles.Right)));
            this.capColumn.CausesValidation = false;
            this.capColumn.Location = new Point(8, 56);
            this.capColumn.Name = "capColumn";
            this.capColumn.Size = new Size(192, 24);
            this.capColumn.Text = "Spalte für Dateinamen:";
            // 
            // btnDateienlöschen
            // 
            this.btnDateienlöschen.ButtonStyle = ButtonStyle.Checkbox_Text;
            this.btnDateienlöschen.Location = new Point(8, 88);
            this.btnDateienlöschen.Name = "btnDateienlöschen";
            this.btnDateienlöschen.Size = new Size(296, 24);
            this.btnDateienlöschen.TabIndex = 100;
            this.btnDateienlöschen.Text = "Dateien nach erfolgreichen Import löschen";
            // 
            // ImportBdb
            // 
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(731, 164);
            this.Controls.Add(this.btnDateienlöschen);
            this.Controls.Add(this.capColumn);
            this.Controls.Add(this.cbxColDateiname);
            this.Controls.Add(this.btnDatenbankwählen);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnImport);
            this.Controls.Add(this.txtInfo);
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.Name = "ImportBdb";
            this.ShowInTaskbar = false;
            this.Text = "Einträge importieren:";
            this.Controls.SetChildIndex(this.txtInfo, 0);
            this.Controls.SetChildIndex(this.btnImport, 0);
            this.Controls.SetChildIndex(this.btnCancel, 0);
            this.Controls.SetChildIndex(this.pnlStatusBar, 0);
            this.Controls.SetChildIndex(this.btnDatenbankwählen, 0);
            this.Controls.SetChildIndex(this.cbxColDateiname, 0);
            this.Controls.SetChildIndex(this.capColumn, 0);
            this.Controls.SetChildIndex(this.btnDateienlöschen, 0);
            this.pnlStatusBar.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        private Caption txtInfo;
        private Button btnImport;
        private Button btnCancel;
        private OpenFileDialog LoadTab;
        private Button btnDatenbankwählen;
        private ComboBox cbxColDateiname;
        private Caption capColumn;
        private Button btnDateienlöschen;
    }
}

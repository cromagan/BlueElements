using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Controls;
using BlueControls.Enums;
using Button = BlueControls.Controls.Button;
using TextBox = BlueControls.Controls.TextBox;

namespace BlueControls.Classes_Editor {
    internal partial class EventScript_Editor {
        //UserControl überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
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
            this.txbName = new TextBox();
            this.capName = new Caption();
            this.scriptEditor = new ScriptEditorDatabase();
            this.chkZeile = new Button();
            this.chkExternVerfügbar = new Button();
            this.chkAendertWerte = new Button();
            this.chkAuslöser_newrow = new Button();
            this.chkAuslöser_valuechanged = new Button();
            this.chkAuslöser_errorcheck = new Button();
            this.SuspendLayout();
            // 
            // txbName
            // 
            this.txbName.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) 
                                                   | AnchorStyles.Right)));
            this.txbName.Cursor = Cursors.IBeam;
            this.txbName.Location = new Point(64, 16);
            this.txbName.Name = "txbName";
            this.txbName.Size = new Size(760, 22);
            this.txbName.TabIndex = 4;
            this.txbName.TextChanged += new EventHandler(this.txbName_TextChanged);
            // 
            // capName
            // 
            this.capName.CausesValidation = false;
            this.capName.Location = new Point(8, 16);
            this.capName.Name = "capName";
            this.capName.Size = new Size(56, 22);
            this.capName.Text = "Name:";
            // 
            // scriptEditor
            // 
            this.scriptEditor.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) 
                                                         | AnchorStyles.Left) 
                                                        | AnchorStyles.Right)));
            this.scriptEditor.Database = null;
            this.scriptEditor.IsRowScript = false;
            this.scriptEditor.Location = new Point(8, 56);
            this.scriptEditor.Name = "scriptEditor";
            this.scriptEditor.ScriptText = "";
            this.scriptEditor.Size = new Size(1008, 392);
            this.scriptEditor.TabIndex = 5;
            this.scriptEditor.TabStop = false;
            this.scriptEditor.Text = "Skript";
            this.scriptEditor.Changed += new EventHandler(this.ScriptEditor_Changed);
            // 
            // chkZeile
            // 
            this.chkZeile.ButtonStyle = ((ButtonStyle)((ButtonStyle.Checkbox | ButtonStyle.Text)));
            this.chkZeile.Location = new Point(64, 40);
            this.chkZeile.Name = "chkZeile";
            this.chkZeile.QuickInfo = "Wenn das Skript Zellwerte der aktuellen Zeile ändern können soll,\r\nmuss dieses Hä" +
    "kchen gesetzt sein.";
            this.chkZeile.Size = new Size(88, 16);
            this.chkZeile.TabIndex = 6;
            this.chkZeile.Text = "Zeilen-Skript";
            this.chkZeile.CheckedChanged += new EventHandler(this.chkZeile_CheckedChanged);
            // 
            // chkExternVerfügbar
            // 
            this.chkExternVerfügbar.ButtonStyle = ((ButtonStyle)((ButtonStyle.Checkbox | ButtonStyle.Text)));
            this.chkExternVerfügbar.Location = new Point(160, 40);
            this.chkExternVerfügbar.Name = "chkExternVerfügbar";
            this.chkExternVerfügbar.QuickInfo = "Wenn das Skript über eine Menüleiste oder dem Kontextmenü\r\nwählbar sein soll, mus" +
    "s dieses Häkchen gesetzt sein.";
            this.chkExternVerfügbar.Size = new Size(120, 16);
            this.chkExternVerfügbar.TabIndex = 7;
            this.chkExternVerfügbar.Text = "Extern verfügbar";
            this.chkExternVerfügbar.CheckedChanged += new EventHandler(this.chkExternVerfügbar_CheckedChanged);
            // 
            // chkAendertWerte
            // 
            this.chkAendertWerte.ButtonStyle = ((ButtonStyle)((ButtonStyle.Checkbox | ButtonStyle.Text)));
            this.chkAendertWerte.Location = new Point(288, 40);
            this.chkAendertWerte.Name = "chkAendertWerte";
            this.chkAendertWerte.QuickInfo = "Das Skript wird nur ausgeführt um dessen\r\nBerechnungen abzugreifen.\r\nÄnderungen w" +
    "erden nicht in die Datenbank\r\nzurückgespielt";
            this.chkAendertWerte.Size = new Size(120, 16);
            this.chkAendertWerte.TabIndex = 8;
            this.chkAendertWerte.Text = "Ändert Werte";
            this.chkAendertWerte.CheckedChanged += new EventHandler(this.chkAendertWerte_CheckedChanged);
            // 
            // chkAuslöser_newrow
            // 
            this.chkAuslöser_newrow.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Right)));
            this.chkAuslöser_newrow.ButtonStyle = ((ButtonStyle)((ButtonStyle.Checkbox | ButtonStyle.Text)));
            this.chkAuslöser_newrow.Location = new Point(840, 8);
            this.chkAuslöser_newrow.Name = "chkAuslöser_newrow";
            this.chkAuslöser_newrow.QuickInfo = "Das Skript wir nach dem Erstellen einer\r\nneuen Zeile ausgeführt.";
            this.chkAuslöser_newrow.Size = new Size(176, 16);
            this.chkAuslöser_newrow.TabIndex = 9;
            this.chkAuslöser_newrow.Text = "Auslöser: Neue Zeile";
            this.chkAuslöser_newrow.CheckedChanged += new EventHandler(this.chkAuslöser_newrow_CheckedChanged);
            // 
            // chkAuslöser_valuechanged
            // 
            this.chkAuslöser_valuechanged.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Right)));
            this.chkAuslöser_valuechanged.ButtonStyle = ((ButtonStyle)((ButtonStyle.Checkbox | ButtonStyle.Text)));
            this.chkAuslöser_valuechanged.Location = new Point(840, 24);
            this.chkAuslöser_valuechanged.Name = "chkAuslöser_valuechanged";
            this.chkAuslöser_valuechanged.QuickInfo = "Das Skript wir nach dem Ändern eines\r\nWertes einer Zelle ausgeführt";
            this.chkAuslöser_valuechanged.Size = new Size(176, 16);
            this.chkAuslöser_valuechanged.TabIndex = 10;
            this.chkAuslöser_valuechanged.Text = "Auslöser: Wert geändert";
            this.chkAuslöser_valuechanged.CheckedChanged += new EventHandler(this.chkAuslöser_newrow_CheckedChanged);
            // 
            // chkAuslöser_errorcheck
            // 
            this.chkAuslöser_errorcheck.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Right)));
            this.chkAuslöser_errorcheck.ButtonStyle = ((ButtonStyle)((ButtonStyle.Checkbox | ButtonStyle.Text)));
            this.chkAuslöser_errorcheck.Location = new Point(840, 40);
            this.chkAuslöser_errorcheck.Name = "chkAuslöser_errorcheck";
            this.chkAuslöser_errorcheck.QuickInfo = "Das Skript wird nur zur Datenkonsitenzprüfung\r\nverendet und ändert keine Daten.";
            this.chkAuslöser_errorcheck.Size = new Size(176, 16);
            this.chkAuslöser_errorcheck.TabIndex = 11;
            this.chkAuslöser_errorcheck.Text = "Auslöser: Fehlerprüfung";
            this.chkAuslöser_errorcheck.CheckedChanged += new EventHandler(this.chkAuslöser_newrow_CheckedChanged);
            // 
            // EventScript_Editor
            // 
            this.Controls.Add(this.chkAuslöser_errorcheck);
            this.Controls.Add(this.chkAuslöser_valuechanged);
            this.Controls.Add(this.chkAuslöser_newrow);
            this.Controls.Add(this.chkAendertWerte);
            this.Controls.Add(this.chkExternVerfügbar);
            this.Controls.Add(this.chkZeile);
            this.Controls.Add(this.scriptEditor);
            this.Controls.Add(this.txbName);
            this.Controls.Add(this.capName);
            this.Name = "EventScript_Editor";
            this.Size = new Size(1031, 459);
            this.ResumeLayout(false);

        }


        private TextBox txbName;
        private Caption capName;
        private ScriptEditorDatabase scriptEditor;
        private Button chkZeile;
        private Button chkExternVerfügbar;
        private Button chkAendertWerte;
        private Button chkAuslöser_newrow;
        private Button chkAuslöser_valuechanged;
        private Button chkAuslöser_errorcheck;
    }
}

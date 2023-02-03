using System.Diagnostics;
using BlueControls.Controls;

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
            this.txbName = new BlueControls.Controls.TextBox();
            this.capName = new BlueControls.Controls.Caption();
            this.scriptEditor = new BlueControls.ScriptEditorDatabase();
            this.chkZeile = new BlueControls.Controls.Button();
            this.chkExternVerfügbar = new BlueControls.Controls.Button();
            this.chkAendertWerte = new BlueControls.Controls.Button();
            this.chkAuslöser_newrow = new BlueControls.Controls.Button();
            this.chkAuslöser_valuechanged = new BlueControls.Controls.Button();
            this.SuspendLayout();
            // 
            // txbName
            // 
            this.txbName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txbName.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbName.Location = new System.Drawing.Point(64, 16);
            this.txbName.Name = "txbName";
            this.txbName.Size = new System.Drawing.Size(784, 22);
            this.txbName.TabIndex = 4;
            this.txbName.TextChanged += new System.EventHandler(this.txbName_TextChanged);
            // 
            // capName
            // 
            this.capName.CausesValidation = false;
            this.capName.Location = new System.Drawing.Point(8, 16);
            this.capName.Name = "capName";
            this.capName.Size = new System.Drawing.Size(56, 22);
            this.capName.Text = "Name:";
            // 
            // scriptEditor
            // 
            this.scriptEditor.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.scriptEditor.Database = null;
            this.scriptEditor.IsRowScript = false;
            this.scriptEditor.Location = new System.Drawing.Point(8, 56);
            this.scriptEditor.Name = "scriptEditor";
            this.scriptEditor.ScriptText = "";
            this.scriptEditor.Size = new System.Drawing.Size(1008, 392);
            this.scriptEditor.TabIndex = 5;
            this.scriptEditor.TabStop = false;
            this.scriptEditor.Text = "Skript";
            this.scriptEditor.Changed += new System.EventHandler(this.ScriptEditor_Changed);
            // 
            // chkZeile
            // 
            this.chkZeile.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Checkbox | BlueControls.Enums.ButtonStyle.Text)));
            this.chkZeile.Location = new System.Drawing.Point(64, 40);
            this.chkZeile.Name = "chkZeile";
            this.chkZeile.QuickInfo = "Wenn das Skript Zellwerte der aktuellen Zeile ändern können soll,\r\nmuss dieses Hä" +
    "kchen gesetzt sein.";
            this.chkZeile.Size = new System.Drawing.Size(88, 16);
            this.chkZeile.TabIndex = 6;
            this.chkZeile.Text = "Zeilen-Skript";
            this.chkZeile.CheckedChanged += new System.EventHandler(this.chkZeile_CheckedChanged);
            // 
            // chkExternVerfügbar
            // 
            this.chkExternVerfügbar.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Checkbox | BlueControls.Enums.ButtonStyle.Text)));
            this.chkExternVerfügbar.Location = new System.Drawing.Point(160, 40);
            this.chkExternVerfügbar.Name = "chkExternVerfügbar";
            this.chkExternVerfügbar.QuickInfo = "Wenn das Skript über eine Menüleiste oder dem Kontextmenü\r\nwählbar sein soll, mus" +
    "s dieses Häkchen gesetzt sein.";
            this.chkExternVerfügbar.Size = new System.Drawing.Size(120, 16);
            this.chkExternVerfügbar.TabIndex = 7;
            this.chkExternVerfügbar.Text = "Extern verfügbar";
            this.chkExternVerfügbar.CheckedChanged += new System.EventHandler(this.chkExternVerfügbar_CheckedChanged);
            // 
            // chkAendertWerte
            // 
            this.chkAendertWerte.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Checkbox | BlueControls.Enums.ButtonStyle.Text)));
            this.chkAendertWerte.Location = new System.Drawing.Point(288, 40);
            this.chkAendertWerte.Name = "chkAendertWerte";
            this.chkAendertWerte.QuickInfo = "Das Skript wird nur ausgeführt um dessen\r\nBerechnungen abzugreifen.\r\nÄnderungen w" +
    "erden nicht in die Datenbank\r\nzurückgespielt";
            this.chkAendertWerte.Size = new System.Drawing.Size(120, 16);
            this.chkAendertWerte.TabIndex = 8;
            this.chkAendertWerte.Text = "Ändert Werte";
            this.chkAendertWerte.CheckedChanged += new System.EventHandler(this.chkAendertWerte_CheckedChanged);
            // 
            // chkAuslöser_newrow
            // 
            this.chkAuslöser_newrow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkAuslöser_newrow.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Checkbox | BlueControls.Enums.ButtonStyle.Text)));
            this.chkAuslöser_newrow.Location = new System.Drawing.Point(864, 16);
            this.chkAuslöser_newrow.Name = "chkAuslöser_newrow";
            this.chkAuslöser_newrow.QuickInfo = "Das Script wir nach dem Erstellen einer\r\nneuen Zeile ausgeführt.";
            this.chkAuslöser_newrow.Size = new System.Drawing.Size(152, 16);
            this.chkAuslöser_newrow.TabIndex = 9;
            this.chkAuslöser_newrow.Text = "Auslöser: Neue Zeile";
            this.chkAuslöser_newrow.CheckedChanged += new System.EventHandler(this.chkAuslöser_newrow_CheckedChanged);
            // 
            // chkAuslöser_valuechanged
            // 
            this.chkAuslöser_valuechanged.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkAuslöser_valuechanged.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Checkbox | BlueControls.Enums.ButtonStyle.Text)));
            this.chkAuslöser_valuechanged.Location = new System.Drawing.Point(864, 40);
            this.chkAuslöser_valuechanged.Name = "chkAuslöser_valuechanged";
            this.chkAuslöser_valuechanged.QuickInfo = "Das Script wir nach dem Ändern eines\r\nWertes einer Zelle ausgeführt";
            this.chkAuslöser_valuechanged.Size = new System.Drawing.Size(152, 16);
            this.chkAuslöser_valuechanged.TabIndex = 10;
            this.chkAuslöser_valuechanged.Text = "Auslöser: Wert geändert";
            this.chkAuslöser_valuechanged.CheckedChanged += new System.EventHandler(this.chkAuslöser_valuechanged_CheckedChanged);
            // 
            // EventScript_Editor
            // 
            this.Controls.Add(this.chkAuslöser_valuechanged);
            this.Controls.Add(this.chkAuslöser_newrow);
            this.Controls.Add(this.chkAendertWerte);
            this.Controls.Add(this.chkExternVerfügbar);
            this.Controls.Add(this.chkZeile);
            this.Controls.Add(this.scriptEditor);
            this.Controls.Add(this.txbName);
            this.Controls.Add(this.capName);
            this.Name = "EventScript_Editor";
            this.Size = new System.Drawing.Size(1031, 459);
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
    }
}

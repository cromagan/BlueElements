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
            this.SuspendLayout();
            // 
            // txbName
            // 
            this.txbName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txbName.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbName.Location = new System.Drawing.Point(64, 16);
            this.txbName.Name = "txbName";
            this.txbName.Size = new System.Drawing.Size(952, 22);
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
            this.scriptEditor.Location = new System.Drawing.Point(8, 72);
            this.scriptEditor.Name = "scriptEditor";
            this.scriptEditor.Size = new System.Drawing.Size(1008, 376);
            this.scriptEditor.SkriptName = "";
            this.scriptEditor.TabIndex = 5;
            this.scriptEditor.TabStop = false;
            this.scriptEditor.Text = "Skript";
            // 
            // chkZeile
            // 
            this.chkZeile.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Checkbox | BlueControls.Enums.ButtonStyle.Text)));
            this.chkZeile.Location = new System.Drawing.Point(64, 40);
            this.chkZeile.Name = "chkZeile";
            this.chkZeile.QuickInfo = "Wenn das Skript Zellwerte der aktuellen Zeile ändern können soll,\r\nmuss dieses Hä" +
    "kchen gesetzt sein.";
            this.chkZeile.Size = new System.Drawing.Size(184, 24);
            this.chkZeile.TabIndex = 6;
            this.chkZeile.Text = "Zeilen-Skript";
            this.chkZeile.CheckedChanged += new System.EventHandler(this.chkZeile_CheckedChanged);
            // 
            // chkExternVerfügbar
            // 
            this.chkExternVerfügbar.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Checkbox | BlueControls.Enums.ButtonStyle.Text)));
            this.chkExternVerfügbar.Location = new System.Drawing.Point(248, 40);
            this.chkExternVerfügbar.Name = "chkExternVerfügbar";
            this.chkExternVerfügbar.QuickInfo = "Wenn das Skript über eine Menüleiste oder dem Kontextmenü\r\nwählbar sein soll, mus" +
    "s dieses Häkchen gesetzt sein.";
            this.chkExternVerfügbar.Size = new System.Drawing.Size(184, 24);
            this.chkExternVerfügbar.TabIndex = 7;
            this.chkExternVerfügbar.Text = "Extern verfügbar";
            this.chkExternVerfügbar.CheckedChanged += new System.EventHandler(this.chkExternVerfügbar_CheckedChanged);
            // 
            // EventScript_Editor
            // 
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
    }
}

using System.Diagnostics;
using BlueControls.Controls;

namespace BlueControls.Classes_Editor {
    internal partial class ImportScript_Editor {
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
            this.scriptEditor = new BlueControls.ScriptEditor();
            this.SuspendLayout();
            // 
            // txbName
            // 
            this.txbName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txbName.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbName.Location = new System.Drawing.Point(64, 24);
            this.txbName.Name = "txbName";
            this.txbName.Size = new System.Drawing.Size(952, 22);
            this.txbName.TabIndex = 4;
            this.txbName.TextChanged += new System.EventHandler(this.txbName_TextChanged);
            // 
            // capName
            // 
            this.capName.CausesValidation = false;
            this.capName.Location = new System.Drawing.Point(8, 24);
            this.capName.Name = "capName";
            this.capName.Size = new System.Drawing.Size(56, 22);
            this.capName.Text = "Name:";
            // 
            // scriptEditor
            // 
            this.scriptEditor.Location = new System.Drawing.Point(8, 48);
            this.scriptEditor.Name = "scriptEditor";
            this.scriptEditor.Size = new System.Drawing.Size(1008, 400);
            this.scriptEditor.TabIndex = 5;
            this.scriptEditor.TabStop = false;
            this.scriptEditor.Text = "Skript";
            this.scriptEditor.Changed += ScriptEditor_Changed;
            // 
            // ImportScript_Editor
            // 
            this.Controls.Add(this.scriptEditor);
            this.Controls.Add(this.txbName);
            this.Controls.Add(this.capName);
            this.Name = "ImportScript_Editor";
            this.Size = new System.Drawing.Size(1031, 459);
            this.ResumeLayout(false);

        }

  
        private TextBox txbName;
        private Caption capName;
        private ScriptEditor scriptEditor;
    }
}

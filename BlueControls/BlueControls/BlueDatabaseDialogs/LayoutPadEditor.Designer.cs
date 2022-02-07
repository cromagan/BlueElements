using System.Diagnostics;
using BlueControls.Controls;

namespace BlueControls.BlueDatabaseDialogs
{
    internal partial class LayoutPadEditor
    { 
        //Das Formular überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
        [DebuggerNonUserCode()]
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent()
        {
            this.tabDatei.SuspendLayout();
            this.grpDateiSystem.SuspendLayout();
            this.grpDesign.SuspendLayout();
            this.Ribbon.SuspendLayout();
            this.tabStart.SuspendLayout();
            this.grpAssistent.SuspendLayout();
            this.SuspendLayout();
            // 
            // Ribbon
            // 
            this.Ribbon.TabDefaultOrder = new string[] {
        "Datei",
        "Start",
        "Hintergrund",
        "Export"};
            // 
            // LayoutPadEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.ClientSize = new System.Drawing.Size(884, 361);
            this.Name = "LayoutPadEditor";
            this.tabDatei.ResumeLayout(false);
            this.grpDateiSystem.ResumeLayout(false);
            this.grpDesign.ResumeLayout(false);
            this.Ribbon.ResumeLayout(false);
            this.tabStart.ResumeLayout(false);
            this.grpAssistent.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        internal GroupBox grpLayoutSelection;
        internal Button btnLayoutUmbenennen;
        internal Button btnLayoutLöschen;
        internal Button btnLayoutHinzu;
        internal Caption capLayout;
        internal ComboBox cbxLayout;
        private TabControl tabRight;
        internal GroupBox grpExterneLayouts;
        private Button btnTextEditor;
        private Button btnLayoutVerzeichnis;
        private Button btnLayoutOeffnen;
        private  System.Windows.Forms.TabPage tabSkript;
        private ScriptEditorDatabase scriptEditor;
        internal Button btnCopyID;
    }
}

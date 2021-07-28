using System.Diagnostics;
using BlueControls.Controls;

namespace BlueControls.Classes_Editor
{
    internal partial class FilterItem_Editor
    {
        //Das Formular überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
        [DebuggerNonUserCode()]
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing )
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
            this.cbxColumns = new BlueControls.Controls.ComboBox();
            this.capSpalte = new BlueControls.Controls.Caption();
            this.btnFilterWahl = new BlueControls.Controls.Button();
            this.SuspendLayout();
            // 
            // cbxColumns
            // 
            this.cbxColumns.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxColumns.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxColumns.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxColumns.Location = new System.Drawing.Point(8, 40);
            this.cbxColumns.Name = "cbxColumns";
            this.cbxColumns.Size = new System.Drawing.Size(216, 24);
            this.cbxColumns.TabIndex = 0;
            this.cbxColumns.TextChanged += new System.EventHandler(this.cbxColumns_TextChanged);
            // 
            // capSpalte
            // 
            this.capSpalte.Location = new System.Drawing.Point(8, 16);
            this.capSpalte.Name = "capSpalte";
            this.capSpalte.Size = new System.Drawing.Size(120, 18);
            this.capSpalte.Text = "Filter der Spalte:";
            // 
            // btnFilterWahl
            // 
            this.btnFilterWahl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFilterWahl.ImageCode = "Trichter|16";
            this.btnFilterWahl.Location = new System.Drawing.Point(232, 40);
            this.btnFilterWahl.Name = "btnFilterWahl";
            this.btnFilterWahl.Size = new System.Drawing.Size(112, 24);
            this.btnFilterWahl.TabIndex = 1;
            this.btnFilterWahl.Text = "Filter wählen";
            this.btnFilterWahl.Click += new System.EventHandler(this.btnFilterWahl_Click);
            // 
            // FilterItem_Editor
            // 
            this.Controls.Add(this.cbxColumns);
            this.Controls.Add(this.btnFilterWahl);
            this.Controls.Add(this.capSpalte);
            this.Name = "FilterItem_Editor";
            this.Size = new System.Drawing.Size(354, 72);
            this.ResumeLayout(false);
        }
        internal ComboBox cbxColumns;
        internal Caption capSpalte;
        internal Button btnFilterWahl;
    }
}

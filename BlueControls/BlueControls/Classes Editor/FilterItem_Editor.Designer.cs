using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Controls;
using Button = BlueControls.Controls.Button;
using ComboBox = BlueControls.Controls.ComboBox;

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
            this.cbxColumns = new ComboBox();
            this.capSpalte = new Caption();
            this.btnFilterWahl = new Button();
            this.SuspendLayout();
            // 
            // cbxColumns
            // 
            this.cbxColumns.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) 
                                                      | AnchorStyles.Right)));
            this.cbxColumns.Cursor = Cursors.IBeam;
            this.cbxColumns.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cbxColumns.Location = new Point(8, 40);
            this.cbxColumns.Name = "cbxColumns";
            this.cbxColumns.Size = new Size(216, 24);
            this.cbxColumns.TabIndex = 0;
            this.cbxColumns.TextChanged += new EventHandler(this.cbxColumns_TextChanged);
            // 
            // capSpalte
            // 
            this.capSpalte.Location = new Point(8, 16);
            this.capSpalte.Name = "capSpalte";
            this.capSpalte.Size = new Size(120, 18);
            this.capSpalte.Text = "Filter der Spalte:";
            // 
            // btnFilterWahl
            // 
            this.btnFilterWahl.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Right)));
            this.btnFilterWahl.ImageCode = "Trichter|16";
            this.btnFilterWahl.Location = new Point(232, 40);
            this.btnFilterWahl.Name = "btnFilterWahl";
            this.btnFilterWahl.Size = new Size(112, 24);
            this.btnFilterWahl.TabIndex = 1;
            this.btnFilterWahl.Text = "Filter wählen";
            this.btnFilterWahl.Click += new EventHandler(this.btnFilterWahl_Click);
            // 
            // FilterItem_Editor
            // 
            this.Controls.Add(this.cbxColumns);
            this.Controls.Add(this.btnFilterWahl);
            this.Controls.Add(this.capSpalte);
            this.Name = "FilterItem_Editor";
            this.Size = new Size(354, 72);
            this.ResumeLayout(false);
        }
        internal ComboBox cbxColumns;
        internal Caption capSpalte;
        internal Button btnFilterWahl;
    }
}

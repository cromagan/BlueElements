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
            this.Col = new BlueControls.Controls.ComboBox();
            this.Caption1 = new BlueControls.Controls.Caption();
            this.FiltWahl = new BlueControls.Controls.Button();
            this.SuspendLayout();
            // 
            // Col
            // 
            this.Col.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Col.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.Col.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Col.Location = new System.Drawing.Point(8, 40);
            this.Col.Name = "Col";
            this.Col.Size = new System.Drawing.Size(216, 24);
            this.Col.TabIndex = 0;
            this.Col.TextChanged += new System.EventHandler(this.Col_TextChanged);
            // 
            // Caption1
            // 
            this.Caption1.Location = new System.Drawing.Point(8, 16);
            this.Caption1.Name = "Caption1";
            this.Caption1.Size = new System.Drawing.Size(120, 18);
            this.Caption1.Text = "Filter der Spalte:";
            // 
            // FiltWahl
            // 
            this.FiltWahl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.FiltWahl.ImageCode = "Trichter|16";
            this.FiltWahl.Location = new System.Drawing.Point(232, 40);
            this.FiltWahl.Name = "FiltWahl";
            this.FiltWahl.Size = new System.Drawing.Size(112, 24);
            this.FiltWahl.TabIndex = 1;
            this.FiltWahl.Text = "Filter wählen";
            this.FiltWahl.Click += new System.EventHandler(this.FiltWahl_Click);
            // 
            // FilterItem_Editor
            // 
            this.Controls.Add(this.Col);
            this.Controls.Add(this.FiltWahl);
            this.Controls.Add(this.Caption1);
            this.Name = "FilterItem_Editor";
            this.Size = new System.Drawing.Size(354, 72);
            this.ResumeLayout(false);

        }

        internal ComboBox Col;
        internal Caption Caption1;
        internal Button FiltWahl;
    }
}


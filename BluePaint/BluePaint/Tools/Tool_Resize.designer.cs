using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Controls;
using BlueDatabase.Enums;
using Button = BlueControls.Controls.Button;

namespace BluePaint
{
    public partial class Tool_Resize 
    {
        //UserControl überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
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
            this.btnDoResize = new Button();
            this.flxProzent = new FlexiControl();
            this.capInfo = new Caption();
            this.SuspendLayout();
            // 
            // btnDoResize
            // 
            this.btnDoResize.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Right)));
            this.btnDoResize.ImageCode = "Häkchen|16";
            this.btnDoResize.Location = new Point(160, 88);
            this.btnDoResize.Name = "btnDoResize";
            this.btnDoResize.Size = new Size(112, 32);
            this.btnDoResize.TabIndex = 9;
            this.btnDoResize.Text = "übernehmen";
            this.btnDoResize.Click += new EventHandler(this.btnDoResize_Click);
            // 
            // flxProzent
            // 
            this.flxProzent.Caption = "Skalieren auf:";
            this.flxProzent.CaptionPosition = ÜberschriftAnordnung.Links_neben_Dem_Feld;
            this.flxProzent.EditType = EditTypeFormula.Textfeld;
            this.flxProzent.Location = new Point(8, 8);
            this.flxProzent.Name = "flxProzent";
            this.flxProzent.Size = new Size(168, 32);
            this.flxProzent.Suffix = "%";
            this.flxProzent.TabIndex = 10;
            this.flxProzent.ValueChanged += new EventHandler(this.flxProzent_ValueChanged);
            // 
            // capInfo
            // 
            this.capInfo.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) 
                                                   | AnchorStyles.Right)));
            this.capInfo.Location = new Point(8, 48);
            this.capInfo.Name = "capInfo";
            this.capInfo.Size = new Size(264, 32);
            // 
            // Tool_Resize
            // 
            this.ClientSize = new Size(281, 385);
            this.Controls.Add(this.capInfo);
            this.Controls.Add(this.flxProzent);
            this.Controls.Add(this.btnDoResize);
            this.Name = "Tool_Resize";
            this.ResumeLayout(false);
        }
        internal Button btnDoResize;
        private FlexiControl flxProzent;
        private Caption capInfo;
    }
}
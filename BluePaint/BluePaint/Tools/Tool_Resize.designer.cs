using BlueControls.Controls;
using BlueBasics.Enums;
namespace BluePaint
{
    public partial class Tool_Resize 
    {
        //UserControl überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
        [System.Diagnostics.DebuggerNonUserCode()]
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
        [System.Diagnostics.DebuggerStepThrough()]
        private void InitializeComponent()
        {
            this.btnDoResize = new BlueControls.Controls.Button();
            this.flxProzent = new BlueControls.Controls.FlexiControl();
            this.capInfo = new BlueControls.Controls.Caption();
            this.SuspendLayout();
            // 
            // btnDoResize
            // 
            this.btnDoResize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDoResize.ImageCode = "Häkchen|16";
            this.btnDoResize.Location = new System.Drawing.Point(160, 88);
            this.btnDoResize.Name = "btnDoResize";
            this.btnDoResize.Size = new System.Drawing.Size(112, 32);
            this.btnDoResize.TabIndex = 9;
            this.btnDoResize.Text = "übernehmen";
            this.btnDoResize.Click += new System.EventHandler(this.btnDoResize_Click);
            // 
            // flxProzent
            // 
            this.flxProzent.Caption = "Skalieren auf:";
            this.flxProzent.CaptionPosition = BlueBasics.Enums.enÜberschriftAnordnung.Links_neben_Dem_Feld;
            this.flxProzent.EditType = BlueBasics.Enums.enEditTypeFormula.Textfeld;
            this.flxProzent.FileEncryptionKey = null;
            this.flxProzent.Format = BlueBasics.Enums.enDataFormat.Gleitkommazahl;
            this.flxProzent.Location = new System.Drawing.Point(8, 8);
            this.flxProzent.Name = "flxProzent";
            this.flxProzent.Size = new System.Drawing.Size(168, 32);
            this.flxProzent.Suffix = "%";
            this.flxProzent.TabIndex = 10;
            this.flxProzent.ValueChanged += new System.EventHandler(this.flxProzent_ValueChanged);
            // 
            // capInfo
            // 
            this.capInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.capInfo.Location = new System.Drawing.Point(8, 48);
            this.capInfo.Name = "capInfo";
            this.capInfo.Size = new System.Drawing.Size(264, 32);
            // 
            // Tool_Resize
            // 
            this.ClientSize = new System.Drawing.Size(281, 385);
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
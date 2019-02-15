using BlueControls.Controls;


namespace BluePaint
{


    public partial class Tool_Screenshot : GenericTool
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
            this.NeuerScreenshot = new Button();
            this.SuspendLayout();
            // 
            // NeuerScreenshot
            // 
            this.NeuerScreenshot.Location = new System.Drawing.Point(24, 48);
            this.NeuerScreenshot.Name = "NeuerScreenshot";
            this.NeuerScreenshot.Size = new System.Drawing.Size(128, 32);
            this.NeuerScreenshot.TabIndex = 4;
            this.NeuerScreenshot.Text = "Neuer Screenshot";
            this.NeuerScreenshot.Click += new System.EventHandler(this.NeuerScreenshot_Click);
            // 
            // Tool_Screenshot
            // 
            this.Controls.Add(this.NeuerScreenshot);
            this.Name = "Tool_Screenshot";
            this.Size = new System.Drawing.Size(173, 150);
            this.ResumeLayout(false);

        }

        internal Button NeuerScreenshot;
    }

}
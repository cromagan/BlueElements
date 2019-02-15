using BlueControls.Controls;


namespace BluePaint
{



    public partial class Tool_Spiegeln : GenericTool
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
            this.SpiegelnH = new Button();
            this.SpiegelnV = new Button();
            this.SuspendLayout();
            // 
            // SpiegelnH
            // 
            this.SpiegelnH.ImageCode = "SpiegelnHorizontal";
            this.SpiegelnH.Location = new System.Drawing.Point(16, 32);
            this.SpiegelnH.Name = "SpiegelnH";
            this.SpiegelnH.Size = new System.Drawing.Size(152, 48);
            this.SpiegelnH.TabIndex = 4;
            this.SpiegelnH.Text = "Horizontal spiegeln";
            this.SpiegelnH.Click += new System.EventHandler(this.SpiegelnH_Click);
            // 
            // SpiegelnV
            // 
            this.SpiegelnV.ImageCode = "SpiegelnVertikal";
            this.SpiegelnV.Location = new System.Drawing.Point(16, 88);
            this.SpiegelnV.Name = "SpiegelnV";
            this.SpiegelnV.Size = new System.Drawing.Size(152, 48);
            this.SpiegelnV.TabIndex = 5;
            this.SpiegelnV.Text = "Vertikal spiegeln";
            this.SpiegelnV.Click += new System.EventHandler(this.SpiegelnV_Click);
            // 
            // Tool_Spiegeln
            // 
            this.Controls.Add(this.SpiegelnV);
            this.Controls.Add(this.SpiegelnH);
            this.Name = "Tool_Spiegeln";
            this.Size = new System.Drawing.Size(295, 150);
            this.ResumeLayout(false);

        }

        internal Button SpiegelnH;
        internal Button SpiegelnV;
    }

}
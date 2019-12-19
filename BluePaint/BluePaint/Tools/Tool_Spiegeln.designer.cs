using BlueControls.Controls;


namespace BluePaint
{



    public partial class Tool_Spiegeln
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
            this.btnSpiegelnH = new BlueControls.Controls.Button();
            this.btnSpiegelnV = new BlueControls.Controls.Button();
            this.btnDrehenR = new BlueControls.Controls.Button();
            this.btnDrehenL = new BlueControls.Controls.Button();
            this.btnAusrichten = new BlueControls.Controls.Button();
            this.SuspendLayout();
            // 
            // btnSpiegelnH
            // 
            this.btnSpiegelnH.ImageCode = "SpiegelnHorizontal|24";
            this.btnSpiegelnH.Location = new System.Drawing.Point(16, 8);
            this.btnSpiegelnH.Name = "btnSpiegelnH";
            this.btnSpiegelnH.Size = new System.Drawing.Size(208, 48);
            this.btnSpiegelnH.TabIndex = 4;
            this.btnSpiegelnH.Text = "Horizontal spiegeln";
            this.btnSpiegelnH.Click += new System.EventHandler(this.SpiegelnH_Click);
            // 
            // btnSpiegelnV
            // 
            this.btnSpiegelnV.ImageCode = "SpiegelnVertikal|24";
            this.btnSpiegelnV.Location = new System.Drawing.Point(16, 64);
            this.btnSpiegelnV.Name = "btnSpiegelnV";
            this.btnSpiegelnV.Size = new System.Drawing.Size(208, 48);
            this.btnSpiegelnV.TabIndex = 5;
            this.btnSpiegelnV.Text = "Vertikal spiegeln";
            this.btnSpiegelnV.Click += new System.EventHandler(this.SpiegelnV_Click);
            // 
            // btnDrehenR
            // 
            this.btnDrehenR.ImageCode = "DrehenRechts|24";
            this.btnDrehenR.Location = new System.Drawing.Point(120, 128);
            this.btnDrehenR.Name = "btnDrehenR";
            this.btnDrehenR.Size = new System.Drawing.Size(104, 48);
            this.btnDrehenR.TabIndex = 6;
            this.btnDrehenR.Text = "drehen";
            this.btnDrehenR.Click += new System.EventHandler(this.btnDrehenR_Click);
            // 
            // btnDrehenL
            // 
            this.btnDrehenL.ImageCode = "DrehenLinks|24";
            this.btnDrehenL.Location = new System.Drawing.Point(16, 128);
            this.btnDrehenL.Name = "btnDrehenL";
            this.btnDrehenL.Size = new System.Drawing.Size(104, 48);
            this.btnDrehenL.TabIndex = 7;
            this.btnDrehenL.Text = "drehen";
            this.btnDrehenL.Click += new System.EventHandler(this.btnDrehenL_Click);
            // 
            // btnAusrichten
            // 
            this.btnAusrichten.Location = new System.Drawing.Point(16, 192);
            this.btnAusrichten.Name = "btnAusrichten";
            this.btnAusrichten.Size = new System.Drawing.Size(208, 48);
            this.btnAusrichten.TabIndex = 8;
            this.btnAusrichten.Text = "Ausrichten";
            this.btnAusrichten.Click += new System.EventHandler(this.btnAusrichten_Click);
            // 
            // Tool_Spiegeln
            // 
            this.Controls.Add(this.btnAusrichten);
            this.Controls.Add(this.btnDrehenL);
            this.Controls.Add(this.btnDrehenR);
            this.Controls.Add(this.btnSpiegelnV);
            this.Controls.Add(this.btnSpiegelnH);
            this.Name = "Tool_Spiegeln";
            this.Size = new System.Drawing.Size(248, 321);
            this.ResumeLayout(false);

        }

        internal Button btnSpiegelnH;
        internal Button btnSpiegelnV;
        internal Button btnDrehenR;
        internal Button btnDrehenL;
        internal Button btnAusrichten;
    }

}
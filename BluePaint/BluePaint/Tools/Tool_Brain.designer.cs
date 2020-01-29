using BlueControls.Controls;


namespace BluePaint
{


    public partial class Tool_Brain 
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
            this.btnLernen = new BlueControls.Controls.Button();
            this.btnAnwenden = new BlueControls.Controls.Button();
            this.txtPath = new BlueControls.Controls.TextBox();
            this.btnLernmaske = new BlueControls.Controls.Button();
            this.SuspendLayout();
            // 
            // btnLernen
            // 
            this.btnLernen.Location = new System.Drawing.Point(24, 24);
            this.btnLernen.Name = "btnLernen";
            this.btnLernen.Size = new System.Drawing.Size(232, 24);
            this.btnLernen.TabIndex = 0;
            this.btnLernen.Text = "Lernen";
            this.btnLernen.Click += new System.EventHandler(this.btnLernen_Click);
            // 
            // btnAnwenden
            // 
            this.btnAnwenden.Location = new System.Drawing.Point(24, 136);
            this.btnAnwenden.Name = "btnAnwenden";
            this.btnAnwenden.Size = new System.Drawing.Size(216, 40);
            this.btnAnwenden.TabIndex = 1;
            this.btnAnwenden.Text = "Anwenden";
            this.btnAnwenden.Click += new System.EventHandler(this.btnAnwenden_Click);
            // 
            // txtPath
            // 
            this.txtPath.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtPath.Location = new System.Drawing.Point(24, 56);
            this.txtPath.Name = "txtPath";
            this.txtPath.Size = new System.Drawing.Size(352, 24);
            this.txtPath.TabIndex = 2;
            this.txtPath.Text = "C:\\Users\\cp33\\OneDrive\\Desktop\\Learn\\";
            // 
            // btnLernmaske
            // 
            this.btnLernmaske.Location = new System.Drawing.Point(24, 224);
            this.btnLernmaske.Name = "btnLernmaske";
            this.btnLernmaske.Size = new System.Drawing.Size(216, 40);
            this.btnLernmaske.TabIndex = 3;
            this.btnLernmaske.Text = "Lernmaske anzeigen";
            this.btnLernmaske.Click += new System.EventHandler(this.btnLernmaske_Click);
            // 
            // Tool_Brain
            // 
            this.Controls.Add(this.btnLernmaske);
            this.Controls.Add(this.txtPath);
            this.Controls.Add(this.btnAnwenden);
            this.Controls.Add(this.btnLernen);
            this.Name = "Tool_Brain";
            this.Size = new System.Drawing.Size(377, 312);
            this.ResumeLayout(false);

        }

        private Button btnLernen;
        private Button btnAnwenden;
        private TextBox txtPath;
        private Button btnLernmaske;
    }

}
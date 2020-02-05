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
            this.tabControl1 = new BlueControls.Controls.TabControl();
            this.tabAnwenden = new BlueControls.Controls.TabPage();
            this.tabLernen = new BlueControls.Controls.TabPage();
            this.capFehlerrate = new BlueControls.Controls.Caption();
            this.btnStop = new BlueControls.Controls.Button();
            this.btnDrehen = new BlueControls.Controls.Button();
            this.tabControl1.SuspendLayout();
            this.tabAnwenden.SuspendLayout();
            this.tabLernen.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnLernen
            // 
            this.btnLernen.Location = new System.Drawing.Point(8, 16);
            this.btnLernen.Name = "btnLernen";
            this.btnLernen.Size = new System.Drawing.Size(232, 24);
            this.btnLernen.TabIndex = 0;
            this.btnLernen.Text = "Lernen";
            this.btnLernen.Click += new System.EventHandler(this.btnLernen_Click);
            // 
            // btnAnwenden
            // 
            this.btnAnwenden.Location = new System.Drawing.Point(8, 16);
            this.btnAnwenden.Name = "btnAnwenden";
            this.btnAnwenden.Size = new System.Drawing.Size(296, 48);
            this.btnAnwenden.TabIndex = 1;
            this.btnAnwenden.Text = "Anwenden";
            this.btnAnwenden.Click += new System.EventHandler(this.btnAnwenden_Click);
            // 
            // txtPath
            // 
            this.txtPath.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtPath.Location = new System.Drawing.Point(8, 40);
            this.txtPath.Name = "txtPath";
            this.txtPath.Size = new System.Drawing.Size(336, 24);
            this.txtPath.TabIndex = 2;
            this.txtPath.Text = "D:\\01_Data\\Dokumente\\Desktop\\Learn";
            // 
            // btnLernmaske
            // 
            this.btnLernmaske.Location = new System.Drawing.Point(24, 256);
            this.btnLernmaske.Name = "btnLernmaske";
            this.btnLernmaske.Size = new System.Drawing.Size(216, 40);
            this.btnLernmaske.TabIndex = 3;
            this.btnLernmaske.Text = "Lernmaske anzeigen";
            this.btnLernmaske.Click += new System.EventHandler(this.btnLernmaske_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabAnwenden);
            this.tabControl1.Controls.Add(this.tabLernen);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.HotTrack = true;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.Size = new System.Drawing.Size(361, 384);
            this.tabControl1.TabIndex = 4;
            // 
            // tabAnwenden
            // 
            this.tabAnwenden.Controls.Add(this.btnAnwenden);
            this.tabAnwenden.Location = new System.Drawing.Point(4, 25);
            this.tabAnwenden.Name = "tabAnwenden";
            this.tabAnwenden.Size = new System.Drawing.Size(353, 355);
            this.tabAnwenden.TabIndex = 0;
            this.tabAnwenden.Text = "Anwenden";
            // 
            // tabLernen
            // 
            this.tabLernen.Controls.Add(this.btnDrehen);
            this.tabLernen.Controls.Add(this.capFehlerrate);
            this.tabLernen.Controls.Add(this.btnStop);
            this.tabLernen.Controls.Add(this.btnLernmaske);
            this.tabLernen.Controls.Add(this.btnLernen);
            this.tabLernen.Controls.Add(this.txtPath);
            this.tabLernen.Location = new System.Drawing.Point(4, 25);
            this.tabLernen.Name = "tabLernen";
            this.tabLernen.Size = new System.Drawing.Size(353, 355);
            this.tabLernen.TabIndex = 1;
            this.tabLernen.Text = "Lernen";
            // 
            // capFehlerrate
            // 
            this.capFehlerrate.Location = new System.Drawing.Point(120, 144);
            this.capFehlerrate.Name = "capFehlerrate";
            this.capFehlerrate.Size = new System.Drawing.Size(216, 48);
            this.capFehlerrate.Text = "-";
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(8, 144);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(112, 40);
            this.btnStop.TabIndex = 4;
            this.btnStop.Text = "Stop";
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // btnDrehen
            // 
            this.btnDrehen.ButtonStyle = BlueControls.Enums.enButtonStyle.Checkbox_Text;
            this.btnDrehen.Checked = true;
            this.btnDrehen.Location = new System.Drawing.Point(40, 64);
            this.btnDrehen.Name = "btnDrehen";
            this.btnDrehen.Size = new System.Drawing.Size(152, 24);
            this.btnDrehen.TabIndex = 5;
            this.btnDrehen.Text = "Bilder auch drehen";
            // 
            // Tool_Brain
            // 
            this.ClientSize = new System.Drawing.Size(361, 384);
            this.Controls.Add(this.tabControl1);
            this.Name = "Tool_Brain";
            this.tabControl1.ResumeLayout(false);
            this.tabAnwenden.ResumeLayout(false);
            this.tabLernen.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private Button btnLernen;
        private Button btnAnwenden;
        private TextBox txtPath;
        private Button btnLernmaske;
        private TabControl tabControl1;
        private TabPage tabAnwenden;
        private TabPage tabLernen;
        private Button btnStop;
        private Caption capFehlerrate;
        private Button btnDrehen;
    }

}
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace BlueControls.Controls {
    partial class FormulaTimer {
        /// <summary> 
        /// Erforderliche Designervariable.
        /// </summary>
        private IContainer components = null;


        #region Vom Komponenten-Designer generierter Code
        /// <summary> 
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this.main = new System.Windows.Forms.Timer(this.components);
            this.capAuslösezeit = new BlueControls.Controls.Caption();
            this.capMessage = new BlueControls.Controls.Caption();
            this.capUhr = new BlueControls.Controls.Caption();
            this.SuspendLayout();
            // 
            // main
            // 
            this.main.Tick += new System.EventHandler(this.main_Tick);
            this.main.Interval = 1000;
            this.main.Enabled = true;
            // 
            // capAuslösezeit
            // 
            this.capAuslösezeit.CausesValidation = false;
            this.capAuslösezeit.Dock = System.Windows.Forms.DockStyle.Top;
            this.capAuslösezeit.Location = new System.Drawing.Point(72, 0);
            this.capAuslösezeit.Name = "capAuslösezeit";
            this.capAuslösezeit.Size = new System.Drawing.Size(462, 24);
            // 
            // capMessage
            // 
            this.capMessage.CausesValidation = false;
            this.capMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.capMessage.Location = new System.Drawing.Point(72, 24);
            this.capMessage.Name = "capMessage";
            this.capMessage.Size = new System.Drawing.Size(462, 52);
            this.capMessage.Text = "Initialisierung...";
            // 
            // capUhr
            // 
            this.capUhr.CausesValidation = false;
            this.capUhr.Dock = System.Windows.Forms.DockStyle.Left;
            this.capUhr.Location = new System.Drawing.Point(0, 0);
            this.capUhr.Name = "capUhr";
            this.capUhr.Size = new System.Drawing.Size(72, 76);
            this.capUhr.Text = "<imagecode=Uhr|64>";
            // 
            // FormulaTimer
            // 
            this.Controls.Add(this.capMessage);
            this.Controls.Add(this.capAuslösezeit);
            this.Controls.Add(this.capUhr);
            this.Name = "FormulaTimer";
            this.Size = new System.Drawing.Size(534, 76);
            this.ResumeLayout(false);

        }


        #endregion

        private Timer main;
        private Caption capAuslösezeit;
        private Caption capMessage;
        private Caption capUhr;
    }
}

// Licensed under AGPL-3.0; see License.md for disclaimer and details.

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
            this.capAuslösezeit = new BlueControls.Controls.Caption();
            this.capMessage = new BlueControls.Controls.Caption();
            this.capUhr = new BlueControls.Controls.Caption();
            this.panTop = new System.Windows.Forms.Panel();
            this.chkAktiv = new BlueControls.Controls.Button();
            this.capText = new BlueControls.Controls.Caption();
            this.panTop.SuspendLayout();
            this.SuspendLayout();
            // 
            // panTop
            // 
            this.panTop.AutoSize = true;
            this.panTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panTop.Location = new System.Drawing.Point(0, 0);
            this.panTop.Name = "panTop";
            this.panTop.Size = new System.Drawing.Size(534, 24);
            this.panTop.Visible = false;
            this.panTop.Controls.Add(this.capText);
            this.panTop.Controls.Add(this.chkAktiv);
            // 
            // chkAktiv
            // 
            this.chkAktiv.AutoSize = true;
            this.chkAktiv.ButtonStyle = ButtonStyle.Checkbox_Text;
            this.chkAktiv.Dock = System.Windows.Forms.DockStyle.Left;
            this.chkAktiv.Location = new System.Drawing.Point(0, 0);
            this.chkAktiv.Name = "chkAktiv";
            this.chkAktiv.Size = new System.Drawing.Size(80, 24);
            this.chkAktiv.Checked = true;
            // 
            // capText
            // 
            this.capText.CausesValidation = false;
            this.capText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.capText.Location = new System.Drawing.Point(80, 0);
            this.capText.Name = "capText";
            this.capText.Size = new System.Drawing.Size(454, 24);
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
            this.Controls.Add(this.panTop);
            this.Name = "FormulaTimer";
            this.Size = new System.Drawing.Size(534, 76);
            this.ResumeLayout(false);
            this.PerformLayout();

        }


        #endregion

        private Caption capAuslösezeit;
        private Caption capMessage;
        private Caption capText;
        private Caption capUhr;
        private BlueControls.Controls.Button chkAktiv;
        private System.Windows.Forms.Panel panTop;
    }
}

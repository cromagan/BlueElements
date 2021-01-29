using System;

namespace BlueControls.Controls {
    partial class EasyPicMulti {
        /// <summary> 
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Komponenten-Designer generierter Code

        /// <summary> 
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent() {
            this.pnlControls = new BlueControls.Controls.GroupBox();
            this.btnSchnittView = new BlueControls.Controls.Button();
            this.btnRight = new BlueControls.Controls.Button();
            this.btnLeft = new BlueControls.Controls.Button();
            this.zoompic = new BlueControls.Controls.ZoomPic();
            this.pnlControls.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlControls
            // 
            this.pnlControls.CausesValidation = false;
            this.pnlControls.Controls.Add(this.btnSchnittView);
            this.pnlControls.Controls.Add(this.btnRight);
            this.pnlControls.Controls.Add(this.btnLeft);
            this.pnlControls.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlControls.Location = new System.Drawing.Point(0, 0);
            this.pnlControls.Name = "pnlControls";
            this.pnlControls.Size = new System.Drawing.Size(0, 32);
            this.pnlControls.TabIndex = 2;
            this.pnlControls.TabStop = false;
            // 
            // btnSchnittView
            // 
            this.btnSchnittView.ImageCode = "Brille|32";
            this.btnSchnittView.Location = new System.Drawing.Point(77, 3);
            this.btnSchnittView.Name = "btnSchnittView";
            this.btnSchnittView.Size = new System.Drawing.Size(40, 26);
            this.btnSchnittView.TabIndex = 12;
            this.btnSchnittView.Click += new System.EventHandler(this.btnSchnittView_Click);
            // 
            // btnRight
            // 
            this.btnRight.ImageCode = "Pfeil_Rechts|16";
            this.btnRight.Location = new System.Drawing.Point(42, 3);
            this.btnRight.Name = "btnRight";
            this.btnRight.Size = new System.Drawing.Size(32, 26);
            this.btnRight.TabIndex = 6;
            this.btnRight.Click += new System.EventHandler(this.btnRight_Click);
            // 
            // btnLeft
            // 
            this.btnLeft.ImageCode = "Pfeil_Links|16";
            this.btnLeft.Location = new System.Drawing.Point(7, 3);
            this.btnLeft.Name = "btnLeft";
            this.btnLeft.Size = new System.Drawing.Size(32, 26);
            this.btnLeft.TabIndex = 2;
            this.btnLeft.Click += new System.EventHandler(this.btnLeft_Click);
            // 
            // zoompic
            // 
            this.zoompic.AlwaysSmooth = false;
            this.zoompic.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zoompic.Location = new System.Drawing.Point(0, 32);
            this.zoompic.Name = "zoompic";
            this.zoompic.Size = new System.Drawing.Size(0, 0);
            this.zoompic.TabIndex = 3;
            // 
            // EasyPicMulti
            // 
            this.Controls.Add(this.zoompic);
            this.Controls.Add(this.pnlControls);
            this.pnlControls.ResumeLayout(false);
            this.ResumeLayout(false);

        }



        #endregion
        private GroupBox pnlControls;
        private Button btnRight;
        private Button btnLeft;
        private Button btnSchnittView;
        private ZoomPic zoompic;
    }
}

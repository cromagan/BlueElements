
namespace BlueTools {
    partial class Form1 {
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

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this.chkMouseMove = new System.Windows.Forms.CheckBox();
            this.chkFNMappen = new System.Windows.Forms.CheckBox();
            this.tim = new System.Windows.Forms.Timer(this.components);
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lblInfo = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // chkMouseMove
            // 
            this.chkMouseMove.AutoSize = true;
            this.chkMouseMove.Checked = true;
            this.chkMouseMove.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkMouseMove.Location = new System.Drawing.Point(8, 16);
            this.chkMouseMove.Name = "chkMouseMove";
            this.chkMouseMove.Size = new System.Drawing.Size(157, 17);
            this.chkMouseMove.TabIndex = 0;
            this.chkMouseMove.Text = "Maus jede Minute bewegen";
            this.chkMouseMove.UseVisualStyleBackColor = true;
            this.chkMouseMove.CheckedChanged += new System.EventHandler(this.chkMouseMove_CheckedChanged);
            // 
            // chkFNMappen
            // 
            this.chkFNMappen.AutoSize = true;
            this.chkFNMappen.Location = new System.Drawing.Point(8, 40);
            this.chkFNMappen.Name = "chkFNMappen";
            this.chkFNMappen.Size = new System.Drawing.Size(160, 17);
            this.chkFNMappen.TabIndex = 1;
            this.chkFNMappen.Text = "FN-Taste mit STRG belegen";
            this.chkFNMappen.UseVisualStyleBackColor = true;
            this.chkFNMappen.CheckedChanged += new System.EventHandler(this.chkFNMappen_CheckedChanged);
            // 
            // tim
            // 
            this.tim.Interval = 60000;
            this.tim.Tick += new System.EventHandler(this.tim_Tick);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblInfo});
            this.statusStrip1.Location = new System.Drawing.Point(0, 66);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(181, 22);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // lblInfo
            // 
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(47, 17);
            this.lblInfo.Text = "Warte...";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(181, 88);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.chkFNMappen);
            this.Controls.Add(this.chkMouseMove);
            this.Name = "Form1";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "BlueTools";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chkMouseMove;
        private System.Windows.Forms.CheckBox chkFNMappen;
        private System.Windows.Forms.Timer tim;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel lblInfo;
    }
}


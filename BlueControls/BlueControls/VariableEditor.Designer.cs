namespace BlueControls {
    partial class VariableEditor {
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
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Komponenten-Designer generierter Code

        /// <summary> 
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent() {
            this.grpVariablen = new BlueControls.Controls.GroupBox();
            this.tableVariablen = new BlueControls.Controls.Table();
            this.filterVariablen = new BlueControls.BlueDatabaseDialogs.Filterleiste();
            this.grpVariablen.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpVariablen
            // 
            this.grpVariablen.Controls.Add(this.tableVariablen);
            this.grpVariablen.Controls.Add(this.filterVariablen);
            this.grpVariablen.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpVariablen.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.NormalBold;
            this.grpVariablen.Location = new System.Drawing.Point(0, 0);
            this.grpVariablen.Name = "grpVariablen";
            this.grpVariablen.Size = new System.Drawing.Size(502, 375);
            this.grpVariablen.TabIndex = 5;
            this.grpVariablen.TabStop = false;
            this.grpVariablen.Text = "Variablen";
            // 
            // tableVariablen
            // 
            this.tableVariablen.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableVariablen.DropMessages = false;
            this.tableVariablen.Location = new System.Drawing.Point(8, 64);
            this.tableVariablen.Name = "tableVariablen";
            this.tableVariablen.ShowWaitScreen = true;
            this.tableVariablen.Size = new System.Drawing.Size(483, 303);
            this.tableVariablen.TabIndex = 2;
            this.tableVariablen.Text = "tabVariablen";
            // 
            // filterVariablen
            // 
            this.filterVariablen.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.filterVariablen.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.filterVariablen.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.filterVariablen.Location = new System.Drawing.Point(8, 24);
            this.filterVariablen.Name = "filterVariablen";
            this.filterVariablen.Size = new System.Drawing.Size(483, 40);
            this.filterVariablen.TabIndex = 1;
            this.filterVariablen.TabStop = false;
            // 
            // VariableEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.grpVariablen);
            this.Name = "VariableEditor";
            this.Size = new System.Drawing.Size(502, 375);
            this.grpVariablen.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.GroupBox grpVariablen;
        private Controls.Table tableVariablen;
        private BlueDatabaseDialogs.Filterleiste filterVariablen;
    }
}

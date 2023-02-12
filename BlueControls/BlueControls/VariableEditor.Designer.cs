using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.BlueDatabaseDialogs;
using BlueControls.Controls;
using BlueControls.Enums;
using GroupBox = BlueControls.Controls.GroupBox;

namespace BlueControls {
    partial class VariableEditor {
        /// <summary> 
        /// Erforderliche Designervariable.
        /// </summary>
        private IContainer components = null;

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
            this.grpVariablen = new GroupBox();
            this.tableVariablen = new Table();
            this.filterVariablen = new Filterleiste();
            this.grpVariablen.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpVariablen
            // 
            this.grpVariablen.Controls.Add(this.tableVariablen);
            this.grpVariablen.Controls.Add(this.filterVariablen);
            this.grpVariablen.Dock = DockStyle.Fill;
            this.grpVariablen.GroupBoxStyle = GroupBoxStyle.NormalBold;
            this.grpVariablen.Location = new Point(0, 0);
            this.grpVariablen.Name = "grpVariablen";
            this.grpVariablen.Size = new Size(502, 375);
            this.grpVariablen.TabIndex = 5;
            this.grpVariablen.TabStop = false;
            this.grpVariablen.Text = "Variablen";
            // 
            // tableVariablen
            // 
            this.tableVariablen.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) 
                                                           | AnchorStyles.Left) 
                                                          | AnchorStyles.Right)));
            this.tableVariablen.DropMessages = false;
            this.tableVariablen.Location = new Point(8, 64);
            this.tableVariablen.Name = "tableVariablen";
            this.tableVariablen.ShowWaitScreen = true;
            this.tableVariablen.Size = new Size(483, 303);
            this.tableVariablen.TabIndex = 2;
            this.tableVariablen.Text = "tabVariablen";
            // 
            // filterVariablen
            // 
            this.filterVariablen.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) 
                                                           | AnchorStyles.Right)));
            this.filterVariablen.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.filterVariablen.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.filterVariablen.Location = new Point(8, 24);
            this.filterVariablen.Name = "filterVariablen";
            this.filterVariablen.Size = new Size(483, 40);
            this.filterVariablen.TabIndex = 1;
            this.filterVariablen.TabStop = false;
            // 
            // VariableEditor
            // 
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add(this.grpVariablen);
            this.Name = "VariableEditor";
            this.Size = new Size(502, 375);
            this.grpVariablen.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private GroupBox grpVariablen;
        private Table tableVariablen;
        private Filterleiste filterVariablen;
    }
}

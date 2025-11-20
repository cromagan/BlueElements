using System.ComponentModel;
using BlueControls.Controls;

namespace BlueControls {
    partial class Befehlsreferenz {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.lstCommands = new BlueControls.Controls.ListBox();
            this.txbComms = new BlueControls.Controls.TextBox();
            this.grpBefehle = new BlueControls.Controls.GroupBox();
            this.btnFilterDel = new BlueControls.Controls.Button();
            this.txbFilter = new BlueControls.Controls.TextBox();
            this.grpBefehle.SuspendLayout();
            this.SuspendLayout();
            // 
            // lstCommands
            // 
            this.lstCommands.AddAllowed = BlueControls.Enums.AddType.None;
            this.lstCommands.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstCommands.Location = new System.Drawing.Point(8, 56);
            this.lstCommands.Name = "lstCommands";
            this.lstCommands.Size = new System.Drawing.Size(288, 384);
            this.lstCommands.TabIndex = 3;
            this.lstCommands.ItemClicked += new System.EventHandler<BlueControls.EventArgs.AbstractListItemEventArgs>(this.lstCommands_ItemClicked);
            // 
            // txbComms
            // 
            this.txbComms.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbComms.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txbComms.Location = new System.Drawing.Point(304, 0);
            this.txbComms.Name = "txbComms";
            this.txbComms.Size = new System.Drawing.Size(496, 450);
            this.txbComms.TabIndex = 2;
            // 
            // grpBefehle
            // 
            this.grpBefehle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.grpBefehle.Controls.Add(this.btnFilterDel);
            this.grpBefehle.Controls.Add(this.txbFilter);
            this.grpBefehle.Controls.Add(this.lstCommands);
            this.grpBefehle.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpBefehle.Location = new System.Drawing.Point(0, 0);
            this.grpBefehle.Name = "grpBefehle";
            this.grpBefehle.Size = new System.Drawing.Size(304, 450);
            this.grpBefehle.TabIndex = 5;
            this.grpBefehle.TabStop = false;
            this.grpBefehle.Text = "Befehle";
            // 
            // btnFilterDel
            // 
            this.btnFilterDel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFilterDel.Enabled = false;
            this.btnFilterDel.ImageCode = "Trichter|16||1";
            this.btnFilterDel.Location = new System.Drawing.Point(256, 24);
            this.btnFilterDel.Name = "btnFilterDel";
            this.btnFilterDel.QuickInfo = "Filter löschen";
            this.btnFilterDel.Size = new System.Drawing.Size(40, 24);
            this.btnFilterDel.TabIndex = 5;
            this.btnFilterDel.Click += new System.EventHandler(this.btnFilterDel_Click);
            // 
            // txbFilter
            // 
            this.txbFilter.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbFilter.Location = new System.Drawing.Point(8, 24);
            this.txbFilter.Name = "txbFilter";
            this.txbFilter.QuickInfo = "Textfilter";
            this.txbFilter.Size = new System.Drawing.Size(240, 24);
            this.txbFilter.SpellCheckingEnabled = true;
            this.txbFilter.TabIndex = 4;
            this.txbFilter.TextChanged += new System.EventHandler(this.txbFilter_TextChanged);
            // 
            // Befehlsreferenz
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.txbComms);
            this.Controls.Add(this.grpBefehle);
            this.Name = "Befehlsreferenz";
            this.Text = "Befehlsübersicht";
            this.TopMost = true;
            this.grpBefehle.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private ListBox lstCommands;
        private TextBox txbComms;
        private GroupBox grpBefehle;
        private Button btnFilterDel;
        private TextBox txbFilter;
    }
}
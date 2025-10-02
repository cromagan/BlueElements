using BlueControls.Controls;
using System.ComponentModel;

namespace BlueControls.Forms {
    partial class Start {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing) {
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
            this.Forms = new BlueControls.Controls.ListBox();
            this.capApp = new BlueControls.Controls.Caption();
            this.pnlStatusBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // capStatusBar
            // 
            this.capStatusBar.Size = new System.Drawing.Size(257, 24);
            // 
            // pnlStatusBar
            // 
            this.pnlStatusBar.Location = new System.Drawing.Point(0, 343);
            this.pnlStatusBar.Size = new System.Drawing.Size(257, 24);
            // 
            // Forms
            // 
            this.Forms.AddAllowed = BlueControls.Enums.AddType.None;
            this.Forms.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Forms.AutoSort = true;
            this.Forms.Location = new System.Drawing.Point(8, 32);
            this.Forms.Name = "Forms";
            this.Forms.Size = new System.Drawing.Size(240, 304);
            this.Forms.TabIndex = 7;
            this.Forms.ItemClicked += new System.EventHandler<BlueControls.EventArgs.AbstractListItemEventArgs>(this.Forms_ItemClicked);
            // 
            // capApp
            // 
            this.capApp.CausesValidation = false;
            this.capApp.Location = new System.Drawing.Point(8, 8);
            this.capApp.Name = "capApp";
            this.capApp.Size = new System.Drawing.Size(208, 24);
            this.capApp.Text = "Applikation wählen:";
            // 
            // Start
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(257, 367);
            this.Controls.Add(this.capApp);
            this.Controls.Add(this.Forms);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "Start";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "BeCreative! - (c) Christian Peter";
            this.Controls.SetChildIndex(this.Forms, 0);
            this.Controls.SetChildIndex(this.capApp, 0);
            this.Controls.SetChildIndex(this.pnlStatusBar, 0);
            this.pnlStatusBar.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        protected ListBox Forms;
        protected Caption capApp;
    }
}
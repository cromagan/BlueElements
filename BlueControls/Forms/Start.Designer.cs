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
            Forms = new ListBox();
            capApp = new Caption();
            pnlStatusBar.SuspendLayout();
            SuspendLayout();
            // 
            // capStatusBar
            // 
            capStatusBar.Size = new System.Drawing.Size(379, 24);
            // 
            // pnlStatusBar
            // 
            pnlStatusBar.Location = new System.Drawing.Point(0, 343);
            pnlStatusBar.Size = new System.Drawing.Size(379, 24);
            // 
            // Forms
            // 
            Forms.AddAllowed = Enums.AddType.None;
            Forms.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            Forms.Appearance = Enums.ListBoxAppearance.ButtonList;
            Forms.CheckBehavior = Enums.CheckBehavior.NoSelection;
            Forms.Location = new System.Drawing.Point(8, 32);
            Forms.Name = "Forms";
            Forms.Size = new System.Drawing.Size(362, 304);
            Forms.TabIndex = 7;
            Forms.ItemClicked += Forms_ItemClicked;
            // 
            // capApp
            // 
            capApp.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            capApp.CausesValidation = false;
            capApp.Location = new System.Drawing.Point(8, 8);
            capApp.Name = "capApp";
            capApp.Size = new System.Drawing.Size(360, 24);
            capApp.Text = "<b>Applikation wählen:";
            // 
            // Start
            // 
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            ClientSize = new System.Drawing.Size(379, 367);
            Controls.Add(capApp);
            Controls.Add(Forms);
            GlobalMenuHeight = 0;
            Name = "Start";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "BeCreative! - (c) Christian Peter";
            Controls.SetChildIndex(Forms, 0);
            Controls.SetChildIndex(capApp, 0);
            Controls.SetChildIndex(pnlStatusBar, 0);
            pnlStatusBar.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        protected ListBox Forms;
        protected Caption capApp;
    }
}
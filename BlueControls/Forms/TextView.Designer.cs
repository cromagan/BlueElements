// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.ComponentModel;
using System.Windows.Forms;

namespace BlueControls.Forms {
    partial class TextView {
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
            grpToolbar = new BlueControls.Controls.GroupBox();
            btnCopy = new BlueControls.Controls.Button();
            btnSortDistinct = new BlueControls.Controls.Button();
            txbText = new BlueControls.Controls.TextBox();
            grpToolbar.SuspendLayout();
            SuspendLayout();
            // 
            // grpToolbar
            // 
            grpToolbar.BackColor = Color.FromArgb(255, 255, 255);
            grpToolbar.CausesValidation = false;
            grpToolbar.Controls.Add(btnCopy);
            grpToolbar.Controls.Add(btnSortDistinct);
            grpToolbar.Dock = DockStyle.Top;
            grpToolbar.GroupBoxStyle = GroupBoxStyle.RoundRect;
            grpToolbar.Location = new Point(0, 0);
            grpToolbar.Name = "grpToolbar";
            grpToolbar.Size = new Size(800, 48);
            grpToolbar.TabIndex = 0;
            grpToolbar.TabStop = false;
            // 
            // btnCopy
            // 
            btnCopy.ImageCode = "Kopieren|16";
            btnCopy.Location = new Point(8, 8);
            btnCopy.Name = "btnCopy";
            btnCopy.QuickInfo = "Den gesamten Inhalt in die<br>Zwischenablage kopieren.";
            btnCopy.Size = new Size(36, 32);
            btnCopy.TabIndex = 0;
            btnCopy.Click += btnCopy_Click;
            // 
            // btnSortDistinct
            // 
            btnSortDistinct.ImageCode = "AZ|16";
            btnSortDistinct.Location = new Point(48, 8);
            btnSortDistinct.Name = "btnSortDistinct";
            btnSortDistinct.QuickInfo = "Sortieren und doppelte Zeilen entfernen.";
            btnSortDistinct.Size = new Size(32, 32);
            btnSortDistinct.TabIndex = 1;
            btnSortDistinct.Click += btnSortDistinct_Click;
            // 
            // txbText
            // 
            txbText.Dock = DockStyle.Fill;
            txbText.Location = new Point(0, 48);
            txbText.MaxTextLength = 1000000;
            txbText.MultiLine = true;
            txbText.Name = "txbText";
            txbText.Size = new Size(800, 552);
            txbText.TabIndex = 1;
            // 
            // TextView
            // 
            ClientSize = new Size(800, 600);
            Controls.Add(txbText);
            Controls.Add(grpToolbar);
            Name = "TextView";
            Text = "Text";
            grpToolbar.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private BlueControls.Controls.GroupBox grpToolbar;
        private BlueControls.Controls.Button btnCopy;
        private BlueControls.Controls.Button btnSortDistinct;
        private BlueControls.Controls.TextBox txbText;
    }
}

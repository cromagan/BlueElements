// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.ComponentModel;

namespace BlueControls.Forms {
    partial class Test {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            try {
                if (disposing) {
                    components?.Dispose();
                }

                base.Dispose(disposing);
            }
            catch { }

        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            slideOutPanel1 = new BlueControls.Controls.SlideOutPanel();
            button2 = new BlueControls.Controls.Button();
            button1 = new BlueControls.Controls.Button();
            groupBox1 = new BlueControls.Controls.GroupBox();
            slideOutPanel2 = new BlueControls.Controls.SlideOutPanel();
            slideOutPanel1.SuspendLayout();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // slideOutPanel1
            // 
            slideOutPanel1.BackColor = Color.FromArgb(240, 240, 240);
            slideOutPanel1.Controls.Add(button2);
            slideOutPanel1.Controls.Add(button1);
            slideOutPanel1.Location = new Point(48, 40);
            slideOutPanel1.Name = "slideOutPanel1";
            slideOutPanel1.Size = new Size(384, 184);
            slideOutPanel1.Text = "slideOutPanel1";
            // 
            // button2
            // 
            button2.Location = new Point(240, 134);
            button2.Name = "button2";
            button2.Size = new Size(136, 22);
            button2.TabIndex = 1;
            button2.Text = "button2";
            // 
            // button1
            // 
            button1.Location = new Point(24, 46);
            button1.Name = "button1";
            button1.Size = new Size(128, 22);
            button1.TabIndex = 0;
            button1.Text = "button1";
            // 
            // groupBox1
            // 
            groupBox1.BackColor = Color.FromArgb(240, 240, 240);
            groupBox1.Controls.Add(slideOutPanel2);
            groupBox1.Location = new Point(56, 256);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(424, 136);
            groupBox1.TabIndex = 1;
            groupBox1.TabStop = false;
            groupBox1.Text = "groupBox1";
            // 
            // slideOutPanel2
            // 
            slideOutPanel2.BackColor = Color.FromArgb(244, 245, 246);
            slideOutPanel2.Location = new Point(104, 56);
            slideOutPanel2.Name = "slideOutPanel2";
            slideOutPanel2.Size = new Size(232, 64);
            slideOutPanel2.Text = "slideOutPanel2";
            // 
            // Test
            // 
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            ClientSize = new Size(613, 448);
            Controls.Add(groupBox1);
            Controls.Add(slideOutPanel1);
            Name = "Test";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Test";
            slideOutPanel1.ResumeLayout(false);
            groupBox1.ResumeLayout(false);
            ResumeLayout(false);
        }
        #endregion

        private Controls.SlideOutPanel slideOutPanel1;
        private Controls.Button button2;
        private Controls.Button button1;
        private Controls.GroupBox groupBox1;
        private Controls.SlideOutPanel slideOutPanel2;
    }
}

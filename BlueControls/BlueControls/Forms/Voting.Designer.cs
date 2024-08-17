using System.ComponentModel;

namespace BlueControls.Forms {
    partial class Voting {
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
            this.Pad1 = new BlueControls.Controls.CreativePad();
            this.Pad2 = new BlueControls.Controls.CreativePad();
            this.btn1 = new BlueControls.Controls.Button();
            this.btn2 = new BlueControls.Controls.Button();
            this.cbxStil = new BlueControls.Controls.ComboBox();
            this.SuspendLayout();
            // 
            // Pad1
            // 
            this.Pad1.Location = new System.Drawing.Point(544, 40);
            this.Pad1.Name = "Pad1";
            this.Pad1.Size = new System.Drawing.Size(528, 384);
            this.Pad1.TabIndex = 0;
            this.Pad1.Text = "Pad1";
            // 
            // Pad2
            // 
            this.Pad2.Location = new System.Drawing.Point(8, 40);
            this.Pad2.Name = "Pad2";
            this.Pad2.Size = new System.Drawing.Size(528, 384);
            this.Pad2.TabIndex = 1;
            this.Pad2.Text = "Pad2";
            // 
            // btn1
            // 
            this.btn1.ImageCode = "Herz|48";
            this.btn1.Location = new System.Drawing.Point(448, 432);
            this.btn1.Name = "btn1";
            this.btn1.Size = new System.Drawing.Size(80, 56);
            this.btn1.TabIndex = 2;
            this.btn1.Click += new System.EventHandler(this.btn1_Click);
            // 
            // btn2
            // 
            this.btn2.ImageCode = "Herz|48";
            this.btn2.Location = new System.Drawing.Point(552, 432);
            this.btn2.Name = "btn2";
            this.btn2.Size = new System.Drawing.Size(80, 56);
            this.btn2.TabIndex = 3;
            this.btn2.Click += new System.EventHandler(this.btn2_Click);
            // 
            // cbxStil
            // 
            this.cbxStil.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxStil.Location = new System.Drawing.Point(8, 8);
            this.cbxStil.Name = "cbxStil";
            this.cbxStil.Size = new System.Drawing.Size(272, 24);
            this.cbxStil.TabIndex = 4;
            this.cbxStil.TextChanged += new System.EventHandler(this.cbxStil_TextChanged);
            // 
            // Voting
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(1082, 500);
            this.Controls.Add(this.cbxStil);
            this.Controls.Add(this.btn2);
            this.Controls.Add(this.btn1);
            this.Controls.Add(this.Pad2);
            this.Controls.Add(this.Pad1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "Voting";
            this.Text = "Form";
            this.ResumeLayout(false);

        }
        #endregion

        private Controls.CreativePad Pad1;
        private Controls.CreativePad Pad2;
        private Controls.Button btn1;
        private Controls.Button btn2;
        private Controls.ComboBox cbxStil;
    }
}
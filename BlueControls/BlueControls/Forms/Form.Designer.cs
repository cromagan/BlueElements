using System.ComponentModel;

namespace BlueControls.Forms {
    partial class Form {
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
            this.SuspendLayout();
            // 
            // Form
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(263, 224);
            this.Name = "Form";
            this.Text = "Form";
            this.ResumeLayout(false);
        }
        #endregion

    }
}
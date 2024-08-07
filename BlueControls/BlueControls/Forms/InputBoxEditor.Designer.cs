using System.ComponentModel;
using BlueControls.Controls;

namespace BlueControls.Forms {
    partial class InputBoxEditor {
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
            this.SuspendLayout();
            // 
            // InputBoxEditor
            // 
            this.ClientSize = new System.Drawing.Size(182, 56);
            this.Name = "InputBoxEditor";
            this.ResumeLayout(false);

        }
        #endregion
    }
}
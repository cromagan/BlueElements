// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;
using System.ComponentModel;
using System.Windows.Forms;

namespace BlueControls.Forms {
    partial class ConnectedFormulaViewFloatingForm {
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
            CFormula = new ConnectedFormulaView();
            SuspendLayout();
            // 
            // CFormula
            // 
            CFormula.CausesValidation = false;
            CFormula.Dock = DockStyle.Fill;
            CFormula.FilenameForEditor = "";
            CFormula.GroupBoxStyle = GroupBoxStyle.Nothing;
            CFormula.Location = new Point(0, 0);
            CFormula.Name = "CFormula";
            CFormula.Page = null;
            CFormula.Size = new Size(496, 496);
            CFormula.TabIndex = 0;
            CFormula.TabStop = false;
            // 
            // ConnectedFormulaViewFloatingForm
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(496, 496);
            Controls.Add(CFormula);
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ConnectedFormulaViewFloatingForm";
            StartPosition = FormStartPosition.Manual;
            Text = "Formularansicht";
            TopMost = true;
            ResumeLayout(false);
        }

        #endregion

        private ConnectedFormulaView CFormula;
    }
}

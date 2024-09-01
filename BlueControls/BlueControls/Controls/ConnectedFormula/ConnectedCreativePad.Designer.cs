
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace BlueControls.Controls {
    partial class ConnectedCreativePad {
        /// <summary> 
        /// Erforderliche Designervariable.
        /// </summary>
        private IContainer components = null;



        #region Vom Komponenten-Designer generierter Code

        /// <summary> 
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this.pad = new BlueControls.Controls.CreativePad();

            this.SuspendLayout();
            // 
            // pad
            //
            this.pad.Dock = DockStyle.Fill;
            this.pad.Location = new System.Drawing.Point(0, 0);
            this.pad.Name = "pad";
            this.pad.Size = new System.Drawing.Size(128, 32);
            this.pad.TabIndex = 0;
            this.pad.Text = "";
            this.pad.Visible = true;
            // 
            // ConnectedFormulaView
            // 
            this.Controls.Add(this.pad);
            this.Name = "ConnectedFormulaView";
            this.Size = new System.Drawing.Size(507, 500);
            this.ResumeLayout(false);

        }

        #endregion


        private CreativePad pad;
    }
}

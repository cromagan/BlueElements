
using System.ComponentModel;
using System.Windows.Forms;

namespace BlueControls.Controls {
    partial class ConnectedFormulaButton {
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
            this.main = new BlueControls.Controls.Button();
            this.SuspendLayout();
            // 
            // main
            // 
            this.main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.main.Location = new System.Drawing.Point(0, 0);
            this.main.Name = "main";
            this.main.Size = new System.Drawing.Size(150, 150);
            this.main.TabIndex = 0;
            this.main.MouseUp += new System.Windows.Forms.MouseEventHandler(this.F_MouseUp);
            // 
            // ConnectedFormulaButton
            // 
            this.Controls.Add(this.main);
            this.Name = "ConnectedFormulaButton";
            this.ResumeLayout(false);

        }


        #endregion

        private Button main;
    }
}

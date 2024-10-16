using System.ComponentModel;
using System.Drawing;
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
            this.main = new Button();
            this.SuspendLayout();
            // 
            // main
            // 
            this.main.Dock = DockStyle.Fill;
            this.main.Location = new Point(0, 0);
            this.main.Name = "main";
            this.main.Size = new Size(150, 150);
            this.main.TabIndex = 0;
            this.main.MouseUp += new MouseEventHandler(this.F_MouseUp);
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

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace BlueControls.Controls {
    partial class FormulaTimer {
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
            this.main = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // main
            // 
            this.main.Tick += new System.EventHandler(this.main_Tick);
            // 
            // FormulaTimer
            // 
            this.Name = "FormulaTimer";
            this.ResumeLayout(false);

        }


        #endregion

        private Timer main;
    }
}


using System.ComponentModel;
using System.Windows.Forms;
using BlueControls.Controls;

namespace BlueControls.ConnectedFormula {
    partial class FlexiControlRowSelector {
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
            this.f = new FlexiControl();
            this.SuspendLayout();
            // 
            // f
            // 
            this.f.Dock = DockStyle.Fill;
            this.f.ValueChanged += F_ValueChanged;
            this.ResumeLayout(false);
        }



       #endregion

        private FlexiControl f;
    }
}

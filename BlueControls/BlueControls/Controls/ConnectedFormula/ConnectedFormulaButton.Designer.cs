using BlueControls.EventArgs;
using System;
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
            this.mainButton = new Button();
            this.SuspendLayout();
            // 
            // mainButton
            // 
            this.mainButton.Dock = DockStyle.Fill;
            this.mainButton.Location = new Point(0, 0);
            this.mainButton.Name = "main";
            this.mainButton.Size = new Size(150, 150);
            this.mainButton.TabIndex = 0;
            this.mainButton.ContextMenuInit += new EventHandler<ContextMenuInitEventArgs>(this.mainButton_ContextMenuInit);
            this.mainButton.MouseUp += new MouseEventHandler(this.mainButton_MouseUp);
            // 
            // ConnectedFormulaButton
            // 
            this.Controls.Add(this.mainButton);
            this.Name = "ConnectedFormulaButton";
            this.ResumeLayout(false);

        }

        #endregion

        private Button mainButton;
    }
}

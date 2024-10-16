using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace BlueControls.Controls {
    partial class ConnectedFormulaView {
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
            this.components = new Container();
            this.btnScript = new Button();
            this.updater = new Timer(this.components);
            this.SuspendLayout();
            // 
            // btnScript
            // 
            this.btnScript.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Right)));
            this.btnScript.ImageCode = "Kritisch|16";
            this.btnScript.Location = new Point(379, 0);
            this.btnScript.Name = "btnScript";
            this.btnScript.QuickInfo = "Admin verständigen, Skripte defekt";
            this.btnScript.Size = new Size(128, 32);
            this.btnScript.TabIndex = 0;
            this.btnScript.Text = "Skripte";
            this.btnScript.Visible = false;
            this.btnScript.Click += new EventHandler(this.btnSkript_Click);
            // 
            // updater
            // 
            this.updater.Interval = 2000;
            this.updater.Tick += new EventHandler(this.updater_Tick);
            // 
            // ConnectedFormulaView
            // 
            this.Controls.Add(this.btnScript);
            this.Name = "ConnectedFormulaView";
            this.Size = new Size(507, 500);
            this.ResumeLayout(false);

        }

        #endregion


        private Button btnScript;
        private Timer updater;
    }
}

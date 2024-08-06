
using System;
using System.ComponentModel;
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
            this.components = new System.ComponentModel.Container();
            this.btnScript = new BlueControls.Controls.Button();
            this.updater = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // btnScript
            // 
            this.btnScript.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnScript.ImageCode = "Kritisch|16";
            this.btnScript.Location = new System.Drawing.Point(379, 0);
            this.btnScript.Name = "btnScript";
            this.btnScript.QuickInfo = "Admin verständigen, Skripte defekt";
            this.btnScript.Size = new System.Drawing.Size(128, 32);
            this.btnScript.TabIndex = 0;
            this.btnScript.Text = "Skripte";
            this.btnScript.Visible = false;
            this.btnScript.Click += new System.EventHandler(this.btnSkript_Click);
            // 
            // updater
            // 
            this.updater.Interval = 2000;
            this.updater.Tick += new System.EventHandler(this.updater_Tick);
            // 
            // ConnectedFormulaView
            // 
            this.Controls.Add(this.btnScript);
            this.Name = "ConnectedFormulaView";
            this.Size = new System.Drawing.Size(507, 500);
            this.ResumeLayout(false);

        }

        #endregion


        private Button btnScript;
        private Timer updater;
    }
}

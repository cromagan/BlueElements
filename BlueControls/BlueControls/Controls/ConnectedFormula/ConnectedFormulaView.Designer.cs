
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
            this.components = new Container();
            this.btnScript = new Button();
            this.SuspendLayout();
            // 
            // btnScript
            // 
            this.btnScript.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.btnScript.ImageCode = "Kritisch|16";
            this.btnScript.Location = new System.Drawing.Point(372, 0);
            this.btnScript.Text = "Skripte";
            this.btnScript.Name = "btnScript";
            this.btnScript.QuickInfo = "Admin verständigen, Skripte defekt";
            this.btnScript.Size = new System.Drawing.Size(128, 32);
            this.btnScript.Visible = false;
            this.btnScript.Click += new EventHandler(this.btnSkript_Click);
            // 
            // ConnectedFormulaView
            // 
            this.Controls.Add(this.btnScript);
            this.Name = "ConnectedFormulaView";
            this.Size = new System.Drawing.Size(500, 500);
            this.ResumeLayout(false);

        }

        #endregion


        private Button btnScript;

    }
}

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
            this.btnDetach = new Button();
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
            // btnDetach
            // 
            this.btnDetach.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Right)));
            this.btnDetach.ImageCode = "Aufklappen|16";
            this.btnDetach.Location = new Point(343, 0);
            this.btnDetach.Name = "btnDetach";
            this.btnDetach.QuickInfo = "Ausklappen";
            this.btnDetach.Size = new Size(32, 32);
            this.btnDetach.TabIndex = 1;
            this.btnDetach.Visible = false;
            this.btnDetach.Click += new EventHandler(this.btnAufklappen_Click);
            // 
            // ConnectedFormulaView
            // 
            this.Controls.Add(this.btnDetach);
            this.Controls.Add(this.btnScript);
            this.Name = "ConnectedFormulaView";
            this.Size = new Size(507, 500);
            this.ResumeLayout(false);

        }

        #endregion


        private Button btnScript;
        private Button btnDetach;
    }
}
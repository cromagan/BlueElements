// Licensed under AGPL-3.0; see License.md for disclaimer and details.

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
            btnScript = new Button();
            btnDetach = new Button();
            btnEdit = new Button();
            SuspendLayout();
            // 
            // btnScript
            // 
            btnScript.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnScript.ImageCode = "Kritisch|16";
            btnScript.Location = new Point(368, 0);
            btnScript.Name = "btnScript";
            btnScript.QuickInfo = "Admin verständigen, Skripte defekt";
            btnScript.Size = new Size(128, 32);
            btnScript.TabIndex = 0;
            btnScript.Text = "Skripte";
            btnScript.Visible = false;
            btnScript.Click += btnSkript_Click;
            // 
            // btnDetach
            // 
            btnDetach.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnDetach.ImageCode = "Aufklappen|16";
            btnDetach.Location = new Point(472, 0);
            btnDetach.Name = "btnDetach";
            btnDetach.QuickInfo = "Ausklappen";
            btnDetach.Size = new Size(24, 24);
            btnDetach.TabIndex = 1;
            btnDetach.Visible = false;
            btnDetach.Click += btnAufklappen_Click;
            // 
            // btnEdit
            // 
            btnEdit.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnEdit.ImageCode = "Stift|16";
            btnEdit.Location = new Point(476, 476);
            btnEdit.Name = "btnEdit";
            btnEdit.QuickInfo = "Ansicht bearbeiten";
            btnEdit.Size = new Size(18, 18);
            btnEdit.TabIndex = 50;
            btnEdit.Visible = false;
            btnEdit.Click += btnEdit_Click;
            // 
            // ConnectedFormulaView
            // 
            Controls.Add(btnEdit);
            Controls.Add(btnDetach);
            Controls.Add(btnScript);
            Name = "ConnectedFormulaView";
            Size = new Size(496, 496);
            ResumeLayout(false);

        }

        #endregion


        private Button btnScript;
        private Button btnDetach;
        private Button btnEdit;
    }
}
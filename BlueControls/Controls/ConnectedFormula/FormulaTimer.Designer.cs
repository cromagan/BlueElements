// Licensed under AGPL-3.0; see License.md for disclaimer and details.

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
            capAuslösezeit = new Caption();
            capMessage = new Caption();
            chkAktiv = new Button();
            SuspendLayout();
            // 
            // capAuslösezeit
            // 
            capAuslösezeit.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            capAuslösezeit.CausesValidation = false;
            capAuslösezeit.Location = new Point(0, 48);
            capAuslösezeit.Name = "capAuslösezeit";
            capAuslösezeit.Size = new Size(536, 24);
            capAuslösezeit.Text = "00:00:00";
            // 
            // capMessage
            // 
            capMessage.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            capMessage.CausesValidation = false;
            capMessage.Location = new Point(0, 24);
            capMessage.Name = "capMessage";
            capMessage.Size = new Size(536, 24);
            capMessage.Text = "Initialisierung...";
            // 
            // chkAktiv
            // 
            chkAktiv.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            chkAktiv.ButtonStyle = ButtonStyle.Checkbox_Text;
            chkAktiv.Location = new Point(0, 0);
            chkAktiv.Name = "button1";
            chkAktiv.Size = new Size(536, 24);
            chkAktiv.TabIndex = 2;
            chkAktiv.Text = "Ich bin deaktivierbar!";
            // 
            // FormulaTimer
            // 
            Controls.Add(chkAktiv);
            Controls.Add(capMessage);
            Controls.Add(capAuslösezeit);
            Name = "FormulaTimer";
            Size = new Size(534, 85);
            ResumeLayout(false);
            PerformLayout();

        }


        #endregion

        private Caption capAuslösezeit;
        private Caption capMessage;
        private BlueControls.Controls.Button chkAktiv;
    }
}

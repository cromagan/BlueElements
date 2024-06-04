using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Enums;
using BlueControls.EventArgs;

namespace BlueControls.Controls {
    partial class TextGenerator {
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
            this.lstAuswahl = new ListBox();
            this.textBox1 = new TextBox();
            this.SuspendLayout();
            // 
            // lstAuswahl
            // 
            this.lstAuswahl.AddAllowed = AddType.None;
            this.lstAuswahl.AutoSort = true;
            this.lstAuswahl.CheckBehavior = CheckBehavior.MultiSelection;
            this.lstAuswahl.Dock = DockStyle.Left;
            this.lstAuswahl.Location = new Point(0, 48);
            this.lstAuswahl.Name = "lstAuswahl";
            this.lstAuswahl.Size = new Size(320, 471);
            this.lstAuswahl.TabIndex = 5;
            this.lstAuswahl.ItemClicked += new EventHandler<AbstractListItemEventArgs>(this.lstAuswahl_ItemClicked);
            // 
            // textBox1
            // 
            this.textBox1.Cursor = Cursors.IBeam;
            this.textBox1.Dock = DockStyle.Fill;
            this.textBox1.Enabled = false;
            this.textBox1.Location = new Point(320, 48);
            this.textBox1.MultiLine = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new Size(511, 471);
            this.textBox1.TabIndex = 6;
            // 
            // Textkonserven
            // 
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.lstAuswahl);
            this.Name = "Textkonserven";
            this.Size = new Size(831, 519);
            this.ResumeLayout(false);

        }

        #endregion

        private ListBox lstAuswahl;
        private TextBox textBox1;
    }
}

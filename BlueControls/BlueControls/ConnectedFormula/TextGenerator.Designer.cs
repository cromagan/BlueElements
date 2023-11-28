using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BlueBasics;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueDatabase;
using System.Collections.Generic;


namespace BlueControls.Controls {
    partial class TextGenerator {
        /// <summary> 
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;



        #region Vom Komponenten-Designer generierter Code

        /// <summary> 
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent() {
            this.lstAuswahl = new BlueControls.Controls.ListBox();
            this.textBox1 = new BlueControls.Controls.TextBox();
            this.SuspendLayout();
            // 
            // lstAuswahl
            // 
            this.lstAuswahl.AddAllowed = BlueControls.Enums.AddType.None;
            this.lstAuswahl.AutoSort = true;
            this.lstAuswahl.CheckBehavior = BlueControls.Enums.CheckBehavior.MultiSelection;
            this.lstAuswahl.Dock = System.Windows.Forms.DockStyle.Left;
            this.lstAuswahl.Location = new System.Drawing.Point(0, 48);
            this.lstAuswahl.Name = "lstAuswahl";
            this.lstAuswahl.Size = new System.Drawing.Size(320, 471);
            this.lstAuswahl.TabIndex = 5;
            this.lstAuswahl.ItemClicked += new System.EventHandler<BlueControls.EventArgs.AbstractListItemEventArgs>(this.lstAuswahl_ItemClicked);
            // 
            // textBox1
            // 
            this.textBox1.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox1.Enabled = false;
            this.textBox1.Location = new System.Drawing.Point(320, 48);
            this.textBox1.MultiLine = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(511, 471);
            this.textBox1.TabIndex = 6;
            // 
            // Textkonserven
            // 
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.lstAuswahl);
            this.Name = "Textkonserven";
            this.Size = new System.Drawing.Size(831, 519);
            this.ResumeLayout(false);

        }

        #endregion

        private BlueControls.Controls.ListBox lstAuswahl;
        private BlueControls.Controls.TextBox textBox1;
    }
}

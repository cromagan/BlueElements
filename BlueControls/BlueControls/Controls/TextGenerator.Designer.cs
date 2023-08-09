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

        /// <summary> 
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Komponenten-Designer generierter Code

        /// <summary> 
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent() {
            this.capModus = new BlueControls.Controls.Caption();
            this.cbxModus = new BlueControls.Controls.ComboBox();
            this.grpLeiste = new BlueControls.Controls.GroupBox();
            this.lstAuswahl = new BlueControls.Controls.ListBox();
            this.textBox1 = new BlueControls.Controls.TextBox();
            this.grpLeiste.SuspendLayout();
            this.SuspendLayout();
            // 
            // capModus
            // 
            this.capModus.CausesValidation = false;
            this.capModus.Location = new System.Drawing.Point(16, 16);
            this.capModus.Name = "capModus";
            this.capModus.Size = new System.Drawing.Size(64, 24);
            this.capModus.Text = "Modus:";
            // 
            // cbxModus
            // 
            this.cbxModus.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxModus.Location = new System.Drawing.Point(88, 16);
            this.cbxModus.Name = "cbxModus";
            this.cbxModus.Size = new System.Drawing.Size(328, 24);
            this.cbxModus.TabIndex = 1;
            this.cbxModus.TextChanged += new System.EventHandler(this.cbxModus_TextChanged);
            // 
            // grpLeiste
            // 
            this.grpLeiste.CausesValidation = false;
            this.grpLeiste.Controls.Add(this.cbxModus);
            this.grpLeiste.Controls.Add(this.capModus);
            this.grpLeiste.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpLeiste.Location = new System.Drawing.Point(0, 0);
            this.grpLeiste.Name = "grpLeiste";
            this.grpLeiste.Size = new System.Drawing.Size(831, 48);
            this.grpLeiste.TabIndex = 4;
            this.grpLeiste.TabStop = false;
            this.grpLeiste.Visible = true;
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
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.lstAuswahl);
            this.Controls.Add(this.grpLeiste);
            this.Name = "Textkonserven";
            this.Size = new System.Drawing.Size(831, 519);
            this.grpLeiste.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private BlueControls.Controls.Caption capModus;
        private BlueControls.Controls.ComboBox cbxModus;
        private BlueControls.Controls.GroupBox grpLeiste;
        private BlueControls.Controls.ListBox lstAuswahl;
        private BlueControls.Controls.TextBox textBox1;
    }
}

using System;
using System.Diagnostics;
using System.Drawing;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;

namespace BlueControls.Forms {
    public partial class RelationDiagram {
        //Das Formular überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
        [DebuggerNonUserCode()]
        protected override void Dispose(bool disposing) {
            try {
                if (disposing) {
                }
            } finally {
                base.Dispose(disposing);
            }
        }
        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent() {
            this.Hinzu = new BlueControls.Controls.Button();
            this.grpExtras = new BlueControls.Controls.GroupBox();
            this.btnTextExport = new BlueControls.Controls.Button();
            this.btnBilderExport = new BlueControls.Controls.Button();
            this.Ribbon.SuspendLayout();
            this.tabStart.SuspendLayout();
            this.grpKomponenteHinzufügen.SuspendLayout();
            this.grpDesign.SuspendLayout();
            this.grpAssistent.SuspendLayout();
            this.grpExtras.SuspendLayout();
            this.SuspendLayout();
            // 
            // Pad
            // 
            this.Pad.Size = new System.Drawing.Size(1290, 528);
            this.Pad.ContextMenuInit += new System.EventHandler<BlueControls.EventArgs.ContextMenuInitEventArgs>(this.Pad_ContextMenuInit);
            this.Pad.ContextMenuItemClicked += new System.EventHandler<BlueControls.EventArgs.ContextMenuItemClickedEventArgs>(this.Pad_ContextMenuItemClicked);
            // 
            // Ribbon
            // 
            this.Ribbon.SelectedIndex = 0;
            this.Ribbon.Size = new System.Drawing.Size(1290, 110);
            // 
            //  System.Windows.Forms.tabStart
            // 
            this.tabStart.Controls.Add(this.grpExtras);
            this.tabStart.Size = new System.Drawing.Size(1282, 81);
            this.tabStart.Controls.SetChildIndex(this.grpKomponenteHinzufügen, 0);
            this.tabStart.Controls.SetChildIndex(this.grpExtras, 0);
            // 
            // grpKomponenteHinzufügen
            // 
            this.grpKomponenteHinzufügen.Controls.Add(this.Hinzu);
            this.grpKomponenteHinzufügen.Size = new System.Drawing.Size(336, 81);
            this.grpKomponenteHinzufügen.Controls.SetChildIndex(this.Hinzu, 0);
            // 
            // ArbeitsbreichSetup
            // 
            this.btnArbeitsbreichSetup.ButtonStyle = BlueControls.Enums.enButtonStyle.Button;
            // 
            // Hinzu
            // 
            this.Hinzu.ImageCode = "PlusZeichen";
            this.Hinzu.Location = new System.Drawing.Point(264, 2);
            this.Hinzu.Name = "Hinzu";
            this.Hinzu.Size = new System.Drawing.Size(64, 66);
            this.Hinzu.TabIndex = 3;
            this.Hinzu.Text = "Eintrag hinzufügen";
            this.Hinzu.Click += new System.EventHandler(this.Hinzu_Click);
            // 
            // grpExtras
            // 
            this.grpExtras.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpExtras.Controls.Add(this.btnTextExport);
            this.grpExtras.Controls.Add(this.btnBilderExport);
            this.grpExtras.GroupBoxStyle = BlueControls.Enums.enGroupBoxStyle.RibbonBar;
            this.grpExtras.Location = new System.Drawing.Point(576, 0);
            this.grpExtras.Name = "grpExtras";
            this.grpExtras.Size = new System.Drawing.Size(392, 80);
            this.grpExtras.TabIndex = 2;
            this.grpExtras.TabStop = false;
            this.grpExtras.Text = "Extras";
            // 
            // btnTextExport
            // 
            this.btnTextExport.ButtonStyle = BlueControls.Enums.enButtonStyle.Button_Big_Borderless;
            this.btnTextExport.ImageCode = "Textdatei||||||||||Pfeil_Unten";
            this.btnTextExport.Location = new System.Drawing.Point(80, 2);
            this.btnTextExport.Name = "btnTextExport";
            this.btnTextExport.Size = new System.Drawing.Size(64, 66);
            this.btnTextExport.TabIndex = 1;
            this.btnTextExport.Text = "Text-Export";
            this.btnTextExport.Click += new System.EventHandler(this.btnTextExport_Click);
            // 
            // btnBilderExport
            // 
            this.btnBilderExport.ButtonStyle = BlueControls.Enums.enButtonStyle.Button_Big_Borderless;
            this.btnBilderExport.ImageCode = "Bild||||||||||Pfeil_Unten";
            this.btnBilderExport.Location = new System.Drawing.Point(8, 2);
            this.btnBilderExport.Name = "btnBilderExport";
            this.btnBilderExport.Size = new System.Drawing.Size(64, 66);
            this.btnBilderExport.TabIndex = 0;
            this.btnBilderExport.Text = "Bilder-Export";
            this.btnBilderExport.Click += new System.EventHandler(this.btnBilderExport_Click);
            // 
            // RelationDiagram
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1290, 638);
            this.Name = "RelationDiagram";
            this.Text = "Beziehungs-Editor";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Ribbon.ResumeLayout(false);
            this.tabStart.ResumeLayout(false);
            this.grpKomponenteHinzufügen.ResumeLayout(false);
            this.grpDesign.ResumeLayout(false);
            this.grpAssistent.ResumeLayout(false);
            this.grpExtras.ResumeLayout(false);
            this.ResumeLayout(false);
        }
        private Button Hinzu;
        private GroupBox grpExtras;
        private Button btnTextExport;
        private Button btnBilderExport;
    }
}
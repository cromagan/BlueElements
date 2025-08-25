using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Enums;
using BlueControls.EventArgs;
using Button = BlueControls.Controls.Button;
using GroupBox = BlueControls.Controls.GroupBox;

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
            this.Hinzu = new Button();
            this.grpExtras = new GroupBox();
            this.btnTextExport = new Button();
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
            this.Pad.Size = new Size(1290, 528);
            this.Pad.ContextMenuInit += new EventHandler<ContextMenuInitEventArgs>(this.Pad_ContextMenuInit);
            this.Pad.ContextMenuItemClicked += new EventHandler<ContextMenuItemClickedEventArgs>(this.Pad_ContextMenuItemClicked);
            // 
            // Ribbon
            // 
            this.Ribbon.SelectedIndex = 0;
            this.Ribbon.Size = new Size(1290, 110);
            // 
            //  System.Windows.Forms.tabStart
            // 
            this.tabStart.Controls.Add(this.grpExtras);
            this.tabStart.Size = new Size(1282, 81);
            this.tabStart.Controls.SetChildIndex(this.grpKomponenteHinzufügen, 0);
            this.tabStart.Controls.SetChildIndex(this.grpExtras, 0);
            // 
            // grpKomponenteHinzufügen
            // 
            this.grpKomponenteHinzufügen.Controls.Add(this.Hinzu);
            this.grpKomponenteHinzufügen.Size = new Size(336, 81);
            this.grpKomponenteHinzufügen.Controls.SetChildIndex(this.Hinzu, 0);
            // 
            // ArbeitsbreichSetup
            // 
            this.btnArbeitsbreichSetup.ButtonStyle = ButtonStyle.Button;
            // 
            // Hinzu
            // 
            this.Hinzu.ImageCode = "PlusZeichen";
            this.Hinzu.Location = new Point(264, 2);
            this.Hinzu.Name = "Hinzu";
            this.Hinzu.Size = new Size(64, 66);
            this.Hinzu.TabIndex = 3;
            this.Hinzu.Text = "Eintrag hinzufügen";
            this.Hinzu.Click += new EventHandler(this.Hinzu_Click);
            // 
            // grpExtras
            // 
            this.grpExtras.BackColor = Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpExtras.Controls.Add(this.btnTextExport);
            this.grpExtras.GroupBoxStyle = GroupBoxStyle.RibbonBar;
            this.grpExtras.Location = new Point(576, 0);
            this.grpExtras.Name = "grpExtras";
            this.grpExtras.Size = new Size(392, 80);
            this.grpExtras.TabIndex = 2;
            this.grpExtras.TabStop = false;
            this.grpExtras.Text = "Extras";
            // 
            // btnTextExport
            // 
            this.btnTextExport.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            this.btnTextExport.ImageCode = "Textdatei||||||||||Pfeil_Unten";
            this.btnTextExport.Location = new Point(80, 2);
            this.btnTextExport.Name = "btnTextExport";
            this.btnTextExport.Size = new Size(64, 66);
            this.btnTextExport.TabIndex = 1;
            this.btnTextExport.Text = "Text-Export";
            this.btnTextExport.Click += new EventHandler(this.btnTextExport_Click);
            // 
            // RelationDiagram
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new Size(1290, 638);
            this.Name = "RelationDiagram";
            this.Text = "Beziehungs-Editor";
            this.WindowState = FormWindowState.Maximized;
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
    }
}
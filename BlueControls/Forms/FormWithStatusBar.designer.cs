using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Controls;

namespace BlueControls.Forms {
    public partial class FormWithStatusBar {
        //Das Formular überschreibt den Deletevorgang, um die Komponentenliste zu bereinigen.
        [DebuggerNonUserCode()]
        protected override void Dispose(bool disposing) {
            if (disposing) {
            }
            base.Dispose(disposing);
        }
        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this.capStatusBar = new BlueControls.Controls.Caption();
            this.pnlStatusBar = new System.Windows.Forms.Panel();
            this.timMessageClearer = new System.Windows.Forms.Timer(this.components);
            this.btnNeuerModus = new BlueControls.Controls.Button();
            this.pnlStatusBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // capStatusBar
            // 
            this.capStatusBar.CausesValidation = false;
            this.capStatusBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.capStatusBar.Location = new System.Drawing.Point(0, 0);
            this.capStatusBar.Name = "capStatusBar";
            this.capStatusBar.Size = new System.Drawing.Size(287, 24);
            this.capStatusBar.Translate = false;
            // 
            // pnlStatusBar
            // 
            this.pnlStatusBar.Controls.Add(this.capStatusBar);
            this.pnlStatusBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlStatusBar.Location = new System.Drawing.Point(0, 148);
            this.pnlStatusBar.Name = "pnlStatusBar";
            this.pnlStatusBar.Size = new System.Drawing.Size(287, 24);
            this.pnlStatusBar.TabIndex = 96;
            // 
            // timMessageClearer
            // 
            this.timMessageClearer.Interval = 1000;
            this.timMessageClearer.Tick += new System.EventHandler(this.timMessageClearer_Tick);
            // 
            // btnNeuerModus
            // 
            this.btnNeuerModus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnNeuerModus.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnNeuerModus.ImageCode = "Stern|16";
            this.btnNeuerModus.Location = new System.Drawing.Point(232, 16);
            this.btnNeuerModus.Name = "btnNeuerModus";
            this.btnNeuerModus.Size = new System.Drawing.Size(48, 56);
            this.btnNeuerModus.TabIndex = 0;
            this.btnNeuerModus.Text = "Neuer Modus";
            this.btnNeuerModus.Click += new System.EventHandler(this.btnNeuerModus_Click);
            // 
            // FormWithStatusBar
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(287, 172);
            this.Controls.Add(this.btnNeuerModus);
            this.Controls.Add(this.pnlStatusBar);
            this.Name = "FormWithStatusBar";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "(c) Christian Peter";
            this.pnlStatusBar.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        protected Caption capStatusBar;
        protected Panel pnlStatusBar;
        private Timer timMessageClearer;
        private IContainer components;
        private Controls.Button btnNeuerModus;
    }
}
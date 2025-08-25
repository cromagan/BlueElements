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
            this.components = new Container();
            this.capStatusBar = new Caption();
            this.pnlStatusBar = new Panel();
            this.timMessageClearer = new Timer(this.components);
            this.pnlStatusBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // capStatusBar
            // 
            this.capStatusBar.CausesValidation = false;
            this.capStatusBar.Dock = DockStyle.Fill;
            this.capStatusBar.Location = new Point(0, 0);
            this.capStatusBar.Name = "capStatusBar";
            this.capStatusBar.Size = new Size(287, 24);
            this.capStatusBar.Translate = false;
            // 
            // pnlStatusBar
            // 
            this.pnlStatusBar.Controls.Add(this.capStatusBar);
            this.pnlStatusBar.Dock = DockStyle.Bottom;
            this.pnlStatusBar.Location = new Point(0, 148);
            this.pnlStatusBar.Name = "pnlStatusBar";
            this.pnlStatusBar.Size = new Size(287, 24);
            this.pnlStatusBar.TabIndex = 96;
            // 
            // timMessageClearer
            // 
            this.timMessageClearer.Interval = 1000;
            this.timMessageClearer.Tick += new EventHandler(this.timMessageClearer_Tick);
            // 
            // FormWithStatusBar
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new Size(287, 172);
            this.Controls.Add(this.pnlStatusBar);
            this.Name = "FormWithStatusBar";
            this.StartPosition = FormStartPosition.Manual;
            this.Text = "(c) Christian Peter";
            this.pnlStatusBar.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        protected Caption capStatusBar;
        protected Panel pnlStatusBar;
        private Timer timMessageClearer;
        private IContainer components;
    }
}
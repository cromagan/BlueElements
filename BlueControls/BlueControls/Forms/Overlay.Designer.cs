using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;

namespace BlueControls
{
    public partial class Overlay 
    {
        //Das Formular überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
        [DebuggerNonUserCode()]
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
        //Wird vom Windows Form-Designer benötigt.
        private IContainer components;
        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.Blinker = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // Blinker
            // 
            this.Blinker.Interval = 250;
            this.Blinker.Tick += new System.EventHandler(this.Blinker_Tick);
            // 
            // Overlay
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.Color.Magenta;
            this.ClientSize = new System.Drawing.Size(10, 10);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Overlay";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.TransparencyKey = System.Drawing.Color.Magenta;
            this.ResumeLayout(false);
        }
        internal System.Windows.Forms.Timer Blinker;
    }
}

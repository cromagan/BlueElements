using System;
using System.Diagnostics;
using System.Drawing;
using BlueControls.Controls;

namespace BluePaint
{
    public partial class Tool_Spiegeln
    {
        //UserControl überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
        [DebuggerNonUserCode()]
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent()
        {
            this.btnSpiegelnH = new Button();
            this.btnSpiegelnV = new Button();
            this.btnDrehenR = new Button();
            this.btnDrehenL = new Button();
            this.btnAusrichten = new Button();
            this.SuspendLayout();
            // 
            // btnSpiegelnH
            // 
            this.btnSpiegelnH.ImageCode = "SpiegelnHorizontal|24";
            this.btnSpiegelnH.Location = new Point(16, 8);
            this.btnSpiegelnH.Name = "btnSpiegelnH";
            this.btnSpiegelnH.Size = new Size(208, 48);
            this.btnSpiegelnH.TabIndex = 4;
            this.btnSpiegelnH.Text = "Horizontal spiegeln";
            this.btnSpiegelnH.Click += new EventHandler(this.SpiegelnH_Click);
            // 
            // btnSpiegelnV
            // 
            this.btnSpiegelnV.ImageCode = "SpiegelnVertikal|24";
            this.btnSpiegelnV.Location = new Point(16, 64);
            this.btnSpiegelnV.Name = "btnSpiegelnV";
            this.btnSpiegelnV.Size = new Size(208, 48);
            this.btnSpiegelnV.TabIndex = 5;
            this.btnSpiegelnV.Text = "Vertikal spiegeln";
            this.btnSpiegelnV.Click += new EventHandler(this.SpiegelnV_Click);
            // 
            // btnDrehenR
            // 
            this.btnDrehenR.ImageCode = "DrehenRechts|24";
            this.btnDrehenR.Location = new Point(120, 128);
            this.btnDrehenR.Name = "btnDrehenR";
            this.btnDrehenR.Size = new Size(104, 48);
            this.btnDrehenR.TabIndex = 6;
            this.btnDrehenR.Text = "drehen";
            this.btnDrehenR.Click += new EventHandler(this.btnDrehenR_Click);
            // 
            // btnDrehenL
            // 
            this.btnDrehenL.ImageCode = "DrehenLinks|24";
            this.btnDrehenL.Location = new Point(16, 128);
            this.btnDrehenL.Name = "btnDrehenL";
            this.btnDrehenL.Size = new Size(104, 48);
            this.btnDrehenL.TabIndex = 7;
            this.btnDrehenL.Text = "drehen";
            this.btnDrehenL.Click += new EventHandler(this.btnDrehenL_Click);
            // 
            // btnAusrichten
            // 
            this.btnAusrichten.Location = new Point(16, 192);
            this.btnAusrichten.Name = "btnAusrichten";
            this.btnAusrichten.Size = new Size(208, 48);
            this.btnAusrichten.TabIndex = 8;
            this.btnAusrichten.Text = "Ausrichten";
            this.btnAusrichten.Click += new EventHandler(this.btnAusrichten_Click);
            // 
            // Tool_Spiegeln
            // 
            this.Controls.Add(this.btnAusrichten);
            this.Controls.Add(this.btnDrehenL);
            this.Controls.Add(this.btnDrehenR);
            this.Controls.Add(this.btnSpiegelnV);
            this.Controls.Add(this.btnSpiegelnH);
            this.Name = "Tool_Spiegeln";
            this.Size = new Size(248, 321);
            this.ResumeLayout(false);
        }
        internal Button btnSpiegelnH;
        internal Button btnSpiegelnV;
        internal Button btnDrehenR;
        internal Button btnDrehenL;
        internal Button btnAusrichten;
    }
}
using System;
using System.Diagnostics;
using System.Drawing;
using BlueControls.Controls;

namespace BluePaint
{
    public partial class Tool_Bruchlinie : GenericTool
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
            this.Bruch_Rechts = new Button();
            this.Bruch_Links = new Button();
            this.Bruch_unten = new Button();
            this.Bruch_Oben = new Button();
            this.SuspendLayout();
            // 
            // Bruch_Rechts
            // 
            this.Bruch_Rechts.Location = new Point(152, 88);
            this.Bruch_Rechts.Name = "Bruch_Rechts";
            this.Bruch_Rechts.Size = new Size(128, 32);
            this.Bruch_Rechts.TabIndex = 7;
            this.Bruch_Rechts.Text = "rechts";
            this.Bruch_Rechts.Click += new EventHandler(this.Bruch_Click);
            // 
            // Bruch_Links
            // 
            this.Bruch_Links.Location = new Point(24, 88);
            this.Bruch_Links.Name = "Bruch_Links";
            this.Bruch_Links.Size = new Size(128, 32);
            this.Bruch_Links.TabIndex = 6;
            this.Bruch_Links.Text = "links";
            this.Bruch_Links.Click += new EventHandler(this.Bruch_Click);
            // 
            // Bruch_unten
            // 
            this.Bruch_unten.Location = new Point(88, 120);
            this.Bruch_unten.Name = "Bruch_unten";
            this.Bruch_unten.Size = new Size(128, 32);
            this.Bruch_unten.TabIndex = 5;
            this.Bruch_unten.Text = "unten";
            this.Bruch_unten.Click += new EventHandler(this.Bruch_Click);
            // 
            // Bruch_Oben
            // 
            this.Bruch_Oben.Location = new Point(88, 56);
            this.Bruch_Oben.Name = "Bruch_Oben";
            this.Bruch_Oben.Size = new Size(128, 32);
            this.Bruch_Oben.TabIndex = 4;
            this.Bruch_Oben.Text = "oben";
            this.Bruch_Oben.Click += new EventHandler(this.Bruch_Click);
            // 
            // Tool_Bruchlinie
            // 
            this.Controls.Add(this.Bruch_Rechts);
            this.Controls.Add(this.Bruch_Links);
            this.Controls.Add(this.Bruch_unten);
            this.Controls.Add(this.Bruch_Oben);
            this.Name = "Tool_Bruchlinie";
            this.Size = new Size(307, 350);
            this.ResumeLayout(false);
        }
        internal Button Bruch_Rechts;
        internal Button Bruch_Links;
        internal Button Bruch_unten;
        internal Button Bruch_Oben;
    }
}
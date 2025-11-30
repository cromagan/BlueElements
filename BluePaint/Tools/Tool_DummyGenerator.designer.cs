using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Controls;
using Button = BlueControls.Controls.Button;
using TextBox = BlueControls.Controls.TextBox;

namespace BluePaint
{
    public partial class Tool_DummyGenerator : GenericTool
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
            this.Erstellen = new Button();
            this.Caption1 = new Caption();
            this.Caption2 = new Caption();
            this.X = new TextBox();
            this.Y = new TextBox();
            this.Caption3 = new Caption();
            this.TXT = new TextBox();
            this.SuspendLayout();
            // 
            // Erstellen
            // 
            this.Erstellen.Location = new Point(88, 192);
            this.Erstellen.Name = "Erstellen";
            this.Erstellen.Size = new Size(128, 32);
            this.Erstellen.TabIndex = 4;
            this.Erstellen.Text = "Erstellen";
            this.Erstellen.Click += new EventHandler(this.Erstellen_Click);
            // 
            // Caption1
            // 
            this.Caption1.Location = new Point(40, 56);
            this.Caption1.Name = "Caption1";
            this.Caption1.Size = new Size(40, 24);
            this.Caption1.Text = "Breite:";
            // 
            // Caption2
            // 
            this.Caption2.Location = new Point(40, 88);
            this.Caption2.Name = "Caption2";
            this.Caption2.Size = new Size(40, 24);
            this.Caption2.Text = "Höhe:";
            // 
            // ControlX
            // 
            this.X.Cursor = Cursors.IBeam;
            this.X.Location = new Point(88, 56);
            this.X.Name = "X";
            this.X.Size = new Size(144, 24);
            this.X.Suffix = "Pixel";
            this.X.TabIndex = 6;
            // 
            // Y
            // 
            this.Y.Cursor = Cursors.IBeam;
            this.Y.Location = new Point(88, 88);
            this.Y.Name = "Y";
            this.Y.Size = new Size(144, 24);
            this.Y.Suffix = "Pixel";
            this.Y.TabIndex = 7;
            // 
            // Caption3
            // 
            this.Caption3.Location = new Point(40, 128);
            this.Caption3.Name = "Caption3";
            this.Caption3.Size = new Size(40, 24);
            this.Caption3.Text = "Text:";
            // 
            // TXT
            // 
            this.TXT.Cursor = Cursors.IBeam;
            this.TXT.Location = new Point(88, 128);
            this.TXT.Name = "TXT";
            this.TXT.Size = new Size(144, 24);
            this.TXT.TabIndex = 9;
            this.TXT.Text = "Dummy";
            // 
            // Tool_DummyGenerator
            // 
            this.Controls.Add(this.TXT);
            this.Controls.Add(this.Caption3);
            this.Controls.Add(this.Y);
            this.Controls.Add(this.X);
            this.Controls.Add(this.Caption2);
            this.Controls.Add(this.Caption1);
            this.Controls.Add(this.Erstellen);
            this.Name = "Tool_DummyGenerator";
            this.Size = new Size(368, 363);
            this.ResumeLayout(false);
        }
        internal Button Erstellen;
        internal Caption Caption1;
        internal Caption Caption2;
        internal TextBox X;
        internal TextBox Y;
        internal Caption Caption3;
        internal TextBox TXT;
    }
}
using BlueControls.Controls;
using BlueControls.Enums;
using System.Diagnostics;
using System.Drawing;

namespace BluePaint
{
    public partial class Tool_Paint : GenericTool
    {
        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent()
        {
            this.Stift = new Button();
            this.SuspendLayout();
            //
            //Stift
            //
            this.Stift.ButtonStyle = ButtonStyle.Optionbox_Text;
            this.Stift.Checked = true;
            this.Stift.Location = new Point(24, 64);
            this.Stift.Name = "Stift";
            this.Stift.Size = new Size(375, 56);
            this.Stift.TabIndex = 4;
            this.Stift.Text = "<b>Stift</b><br><i>Übermalen sie mit der Maus Bereiche mit schwarzer Farbe.";
            //
            //Tool_Paint
            //
            this.Controls.Add(this.Stift);
            this.Name = "Tool_Paint";
            this.Size = new Size(419, 274);
            this.Name = "Zeichnen";
            this.ResumeLayout(false);
        }
        internal Button Stift;
    }
}
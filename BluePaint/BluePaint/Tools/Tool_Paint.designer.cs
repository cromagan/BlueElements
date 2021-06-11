using BlueControls.Controls;
using BlueControls.Enums;
namespace BluePaint
{
    public partial class Tool_Paint : GenericTool
    {
        //UserControl überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
        [System.Diagnostics.DebuggerNonUserCode()]
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
        [System.Diagnostics.DebuggerStepThrough()]
        private void InitializeComponent()
        {
            this.Stift = new Button();
            this.SuspendLayout();
            //
            //Stift
            //
            this.Stift.ButtonStyle = enButtonStyle.Optionbox_Text;
            this.Stift.Checked = true;
            this.Stift.Location = new System.Drawing.Point(24, 64);
            this.Stift.Name = "Stift";
            this.Stift.Size = new System.Drawing.Size(375, 56);
            this.Stift.TabIndex = 4;
            this.Stift.Text = "<b>Stift</b><br><i>Übermalen sie mit der Maus Bereiche mit schwarzer Farbe.";
            //
            //Tool_Paint
            //
            this.Controls.Add(this.Stift);
            this.Name = "Tool_Paint";
            this.Size = new System.Drawing.Size(419, 274);
            this.Name = "Zeichnen";
            this.ResumeLayout(false);
        }
        internal Button Stift;
    }
}
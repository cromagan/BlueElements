using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace BlueControls
{
    public sealed partial class ScreenShot : Form
    {
        //Das Formular überschreibt den Deletevorgang, um die Komponentenliste zu bereinigen.
        [DebuggerNonUserCode()]
        protected override void Dispose(bool disposing)
        {
            if (disposing )
            {
            }
            base.Dispose(disposing);
        }
        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent()
        {
            this.SuspendLayout();
            //
            //frmScreenShot
            //
            this.AutoScaleMode = AutoScaleMode.None;
            this.ClientSize = new Size(292, 272);
            this.DoubleBuffered = true;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Name = "frmScreenShot";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.ResumeLayout(false);
        }
    }
}

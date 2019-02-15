using System.Diagnostics;
using System.Drawing;
using BlueControls.Controls;


namespace BlueControls.Classes_Editor
{
    internal partial class AbstractClassEditor : GroupBox
    {
        //UserControl überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
        [DebuggerNonUserCode()]
        protected override void Dispose(bool disposing)
        {
            try
            {
                //if (disposing && components != null)
                //{
                //	components.Dispose();
                //}
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
            this.SuspendLayout();
            //
            //AbstractClassEditor
            //
            this.Size = new Size(480, 218);
            this.Text = "";
            this.ResumeLayout(false);
        }

    }
}


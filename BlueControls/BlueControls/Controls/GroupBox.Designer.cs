using System.ComponentModel;
using System.Diagnostics;

namespace BlueControls.Controls
{
    public partial class GroupBox : GenericControl
    {
        //UserControl überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
        [DebuggerNonUserCode()]
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    if (_BitmapOfControl != null)
                    {
                        _BitmapOfControl.Dispose();
                    }
                    _BitmapOfControl = null;

                    if (components != null)
                    {
                        components.Dispose();
                    }
                }

            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        //Wird vom Windows Form-Designer benötigt.
        private IContainer components;

        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent()
        {
            components = new Container();
        }

    }
}

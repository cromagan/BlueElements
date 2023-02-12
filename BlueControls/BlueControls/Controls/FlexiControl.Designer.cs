using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace BlueControls.Controls
{
    public partial class FlexiControl 
    {
        //UserControl überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
        [DebuggerNonUserCode()]
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && components != null)
                {
                    _IdleTimer.Tick -= _IdleTimer_Tick;
                    _infoText = string.Empty;
                    //if (_BitmapOfControl != null) { _BitmapOfControl?.Dispose(); }
                    //DoInfoTextButton(); // Events entfernen!
                    RemoveAll(); // Events entfernen!
                   components?.Dispose();
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
            this.components = new Container();
            this._IdleTimer = new Timer(this.components);
            this.SuspendLayout();
            // 
            // _IdleTimer
            // 
            this._IdleTimer.Enabled = true;
            this._IdleTimer.Interval = 1000;
            this._IdleTimer.Tick += new EventHandler(this._IdleTimer_Tick);
            // 
            // FlexiControl
            // 
            this.Name = "FlexiControl";
            this.Size = new Size(100, 100);
            this.ResumeLayout(false);
        }
        protected Timer _IdleTimer;
    }
}

using System;
using System.ComponentModel;
using System.Diagnostics;
namespace BlueControls.Controls
{
    public partial class TextBox 
    {
        //UserControl überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
        [DebuggerNonUserCode()]
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && components != null)
                {
                    //if (_BitmapOfControl != null) { _BitmapOfControl.Dispose(); }
                    components.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
                //   Dictionary.Release()
                _Cursor_CharPos = 0;
                _MarkStart = 0;
                _MarkEnd = 0;
                _MouseValue = 0;
                _Cursor_Visible = false;
                _Suffix = null;
                _eTxt = null;
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
            this.Blinker = new System.Windows.Forms.Timer(this.components);
            this.SpellChecker = new BackgroundWorker();
            this.SuspendLayout();
            //
            //Blinker
            //
            Blinker.Interval = 500;
            Blinker.Tick += new EventHandler(Blinker_Tick);
            //
            //SpellChecker
            //
            SpellChecker.WorkerReportsProgress = true;
            SpellChecker.WorkerSupportsCancellation = true;
            SpellChecker.DoWork += new DoWorkEventHandler(SpellChecker_DoWork);
            SpellChecker.ProgressChanged += new ProgressChangedEventHandler(SpellChecker_ProgressChanged);
            SpellChecker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(SpellChecker_RunWorkerCompleted);
            //
            //TextBox
            //
            this.Cursor = System.Windows.Forms.Cursors.IBeam;
            Name = "TextBox";
            this.ResumeLayout(false);
        }
        internal System.Windows.Forms.Timer Blinker;
        internal BackgroundWorker SpellChecker;
    }
}

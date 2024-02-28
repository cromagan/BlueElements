using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;

namespace BlueControls.Controls
{
    public partial class TextBox 
    {

        //Wird vom Windows Form-Designer benötigt.
        private IContainer components;
        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent()
        {
            this.components = new Container();
            this.Blinker = new Timer(this.components);
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
            this.Cursor = Cursors.IBeam;
            Name = "TextBox";
            this.ResumeLayout(false);
        }
        internal Timer Blinker;
        internal BackgroundWorker SpellChecker;
    }
}

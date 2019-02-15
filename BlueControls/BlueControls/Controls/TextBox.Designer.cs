using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using BlueBasics.Enums;
using BlueControls.Enums;

namespace BlueControls.Controls
{
    public partial class TextBox : GenericControl
    {
        //UserControl überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
        [DebuggerNonUserCode()]
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && components != null)
                {
                    if (_BitmapOfControl != null) { _BitmapOfControl.Dispose(); }
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
            this.SliderY = new Slider();
            this.SpellChecker = new BackgroundWorker();
            this.SuspendLayout();
            //
            //Blinker
            //
            Blinker.Interval = 500;
            Blinker.Tick += new EventHandler(Blinker_Tick);
            //
            //SliderY
            //
            SliderY.Dock = System.Windows.Forms.DockStyle.Right;
            SliderY.LargeChange = 10.0D;
            SliderY.Location = new Point(132, 0);
            SliderY.Maximum = 100.0D;
            SliderY.Minimum = 0.0D;
            SliderY.MouseChange = 1.0D;
            SliderY.Name = "SliderY";
            SliderY.Orientation = enOrientation.Senkrecht;
            SliderY.Size = new Size(18, 150);
            SliderY.SmallChange = 48.0D;
            SliderY.TabIndex = 0;
            SliderY.TabStop = false;
            SliderY.Value = 0.0D;
            SliderY.Visible = false;
            SliderY.ValueChanged += new EventHandler(SliderY_ValueChange);
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
            this.Controls.Add(SliderY);
            this.Cursor = System.Windows.Forms.Cursors.IBeam;
            Name = "TextBox";
            this.ResumeLayout(false);
        }

        internal System.Windows.Forms.Timer Blinker;
        internal Slider SliderY;
        internal BackgroundWorker SpellChecker;
    }
}

using BlueBasics;
using BlueControls.Forms;
using System;
using System.Drawing;

namespace BlueControls.DialogBoxes
{
    public partial class QuickInfo : FloatingForm
    {


        public static string _shownTXT = string.Empty;
        public static string _AutoClosedTXT = string.Empty;

        private bool _Shown = false;


        private int Counter = 0;

        public QuickInfo()
        {
            InitializeComponent();
        }

        public QuickInfo(string Text)
        {
            InitializeComponent();


            capTXT.Text = Text;


            var He = Math.Min(capTXT.TextRequiredSize().Height, Convert.ToInt32(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Size.Height * 0.7));
            var Wi = Math.Min(capTXT.TextRequiredSize().Width, Convert.ToInt32(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Size.Width * 0.7));


            this.Size = new Size(Wi + capTXT.Left * 2, He + capTXT.Top * 2);

            this.Visible = false;

            timQI.Enabled = true;

        }


        public static void Show(string Text)
        {

            if (Text == _shownTXT) { return; }
            Close(false);

            if (Text == _AutoClosedTXT) { return; }




            _shownTXT = Text;
            if (string.IsNullOrEmpty(Text)) { return; }

            new QuickInfo(Text);
        }


        public new static void Close()
        {
            Close(false);
        }


        private static void Close(bool AutoClose)
        {

            if (AutoClose)
            {
                _AutoClosedTXT = _shownTXT;
            }
            else
            {
                _shownTXT = string.Empty;
                _AutoClosedTXT = string.Empty;
            }



            foreach (var ThisForm in AllBoxes)
            {
                if (!ThisForm.IsDisposed && ThisForm is QuickInfo QI)
                {
                    try
                    {
                        QI.timQI.Enabled = false;
                        ThisForm.Close();
                        Close(AutoClose);
                        return;
                    }
                    catch (Exception ex)
                    {
                        Develop.DebugPrint(ex);
                    }
                }
            }
        }



        private void timQI_Tick(object sender, System.EventArgs e)
        {
            Position_LocateToMouse();

            if (!_Shown)
            {
                _Shown = true;
                Show();
                timQI.Interval = 15;
            }


            Counter++;

            if (Counter * timQI.Interval > 10000)
            {
                timQI.Enabled = false;
                Close(true);
            }
        }
    }
}

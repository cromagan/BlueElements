using BluePaint.EventArgs;
using BlueControls;

namespace BluePaint
{
    public partial class Tool_Screenshot
    {

        public Tool_Screenshot()
        {
            InitializeComponent();
        }

        private void NeuerScreenshot_Click(object sender, System.EventArgs e)
        {

            DoScreenShot();
        }

        private void DoScreenShot()
        {
            OnHideMainWindow();

            BlueBasics.modAllgemein.Pause(1, true);

            var _Pic = ScreenShot.GrabArea(null, 0, 0).Pic;

            if (_Pic != null) { OnOverridePic(new BitmapEventArgs(_Pic)); }
            OnShowMainWindow();
        }

        public override void ToolFirstShown()
        {
            DoScreenShot();
        }
    }
}
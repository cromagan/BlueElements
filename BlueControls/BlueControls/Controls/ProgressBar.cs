using BlueControls.Enums;
using System;
using System.ComponentModel;
using System.Drawing;

namespace BlueControls.Controls
{
    public class ProgressBar : GenericControl
    {
        private int wProzent = 100;

        [DefaultValue(100)]
        public int Prozent
        {
            get
            {
                return wProzent;
            }
            set
            {
                if (value < 0)
                {
                    value = 0;
                }
                if (value > 100)
                {
                    value = 100;
                }

                if (wProzent == value)
                {
                    return;
                }
                wProzent = value;
                Invalidate();
            }
        }

        //Friend Overrides Sub PrepareForShowing()
        //    'Stop
        //End Sub
        protected override void InitializeSkin()
        {

        }

        // Private Sub EventDrawControl(GR as graphics, vState As enStates) Handles MyBase.DrawControl
        protected override void DrawControl(Graphics GR, enStates vState)
        {

            Skin.Draw_Back(GR, enDesign.Progressbar, vState, DisplayRectangle, this, true);

            if (wProzent > 0)
            {
                var r = new Rectangle(DisplayRectangle.X, DisplayRectangle.Y, Convert.ToInt32(Math.Truncate(DisplayRectangle.Width * wProzent / 100.0)), DisplayRectangle.Height);
                //r = New Rectangle(DisplayRectangle)
                //r.Width = CInt(r.Width * wProzent / 100)

                Skin.Draw_Back(GR, enDesign.Progressbar_Füller, vState, r, this, true);
                Skin.Draw_Border(GR, enDesign.Progressbar_Füller, vState, r);
            }


            Skin.Draw_Border(GR, enDesign.Progressbar, vState, DisplayRectangle);
        }



    }
}

using BlueBasics.Enums;
using BlueControls.Enums;
using BlueControls.Interfaces;
using System.ComponentModel;
using System.Drawing;

namespace BlueControls.Controls
{
    public class Line : GenericControl, IBackgroundNone
    {
        public Line()
        {

            // Dieser Aufruf ist für den Designer erforderlich.
            //  InitializeComponent()

            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
            SetNotFocusable();
            _MouseHighlight = false;
        }


        private enOrientation _Orientation = enOrientation.Waagerecht;


        [DefaultValue(enOrientation.Waagerecht)]
        public enOrientation Orientation
        {
            get
            {
                return _Orientation;
            }
            set
            {
                if (value == _Orientation)
                {
                    return;
                }
                _Orientation = value;
                CheckSize();
                Invalidate();

            }
        }

        [DefaultValue(0)]
        public new int TabIndex
        {
            get
            {
                return 0;
            }

            set
            {
                base.TabIndex = 0;
            }
        }

        [DefaultValue(false)]
        public new bool TabStop
        {
            get
            {
                return false;
            }
            set
            {
                base.TabStop = false;
            }
        }


        protected override void InitializeSkin()
        {

        }

        public void CheckSize()
        {
            if (_Orientation == enOrientation.Waagerecht)
            {
                if (Width < 10) { Width = 10; }
                Height = 2;
            }
            else
            {
                Width = 2;
                if (Height < 10) { Height = 10; }
            }


        }


        protected override void DrawControl(Graphics GR, enStates vState)
        {
            CheckSize();
            var DP = new Pen(SystemColors.ControlDark);
            var LP = new Pen(SystemColors.ControlLight);

            if (_Orientation == enOrientation.Waagerecht)
            {
                GR.DrawLine(DP, 0, 0, Width - 1, 0);
                GR.DrawLine(LP, 1, 1, Width, 1);
            }
            else
            {
                GR.DrawLine(DP, 0, 0, 0, Height - 1);
                GR.DrawLine(LP, 1, 1, 1, Height);
            }
        }
    }
}

using BlueControls.Controls;
using BlueControls.Enums;
using System;
using System.Drawing;

namespace BlueControls {
    public partial class Overlay : System.Windows.Forms.Form {

        private int Count;

        private readonly int Modus;

        private readonly GenericControl Control;

        public Overlay() {
            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();

            //// Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
            //SetStyle(System.Windows.Forms.ControlStyles.Selectable, false);
            //SetStyle(System.Windows.Forms.ControlStyles.StandardClick, false);
            //SetStyle(System.Windows.Forms.ControlStyles.StandardDoubleClick, false);

            Modus = 2;

            Control = null;

            var Radius = 10;

            Width = Radius * 2;
            Height = Radius * 2;

            Left = -Radius * 3;
            Top = -Radius * 3;
        }


        public Overlay(GenericControl OverControl) {

            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();

            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
            SetStyle(System.Windows.Forms.ControlStyles.Selectable, false);
            SetStyle(System.Windows.Forms.ControlStyles.StandardClick, false);
            SetStyle(System.Windows.Forms.ControlStyles.StandardDoubleClick, false);

            Modus = 1;
            Control = OverControl;
            SetControl();
        }


        public void SetControl() {
            Width = Control.Width;
            Height = Control.Height;

            var p = Control.PointToScreen(Point.Empty);

            Left = p.X;
            Top = p.Y;
        }

        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e) {
            if (IsDisposed) { return; }

            base.OnPaint(e);

            switch (Modus) {
                case 1:
                    Paint_RoterRahmenUmControlUndBlinken();
                    break;

                case 2:
                    Paint_Radius();
                    break;

                default:
                    Dispose();
                    break;
            }



        }


        public void Paint_Radius() {
            var g = CreateGraphics();
            g.Clear(Color.Magenta);
            g.DrawEllipse(new Pen(Color.Black, 3), 1, 1, Width - 3, Height - 3);
            g.DrawEllipse(Pens.Red, 1, 1, Width - 3, Height - 3);
            g.Dispose();
        }


        private void Paint_RoterRahmenUmControlUndBlinken() {
            SetControl();

            var g = CreateGraphics();

            g.Clear(Color.Magenta);

            g.DrawRectangle(Pens.Black, 0, 0, Width - 1, Height - 1);

            g.DrawRectangle(Pens.Red, 1, 1, Width - 3, Height - 3);
            g.DrawRectangle(Pens.Black, 2, 2, Width - 5, Height - 5);

            g.Dispose();

            Blinker.Enabled = true;

        }

        private void Blinker_Tick(object sender, System.EventArgs e) {

            if (Opacity > 0.5) {
                Opacity = 0.01;
            } else {
                Opacity = 1;
            }


            Count++;

            if (Count > 4) {
                Blinker.Enabled = false;
                Dispose();
            }
        }

        protected override System.Windows.Forms.CreateParams CreateParams {
            get {
                var oParam = base.CreateParams;
                oParam.ExStyle |= (int)enExStyle.EX_NOACTIVATE | (int)enExStyle.EX_TOOLWINDOW | (int)enExStyle.EX_TOPMOST;
                oParam.Parent = IntPtr.Zero;
                return oParam;
            }
        }



    }
}
using BlueControls.Controls;
using BlueControls.Enums;
using System;
using System.Drawing;

namespace BlueControls {

    public partial class Overlay : System.Windows.Forms.Form {

        #region Fields

        private readonly GenericControl? _control;
        private readonly int _modus;
        private int _count;

        #endregion

        #region Constructors

        public Overlay() {
            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();
            //// Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
            //SetStyle(System.Windows.Forms.ControlStyles.Selectable, false);
            //SetStyle(System.Windows.Forms.ControlStyles.StandardClick, false);
            //SetStyle(System.Windows.Forms.ControlStyles.StandardDoubleClick, false);
            _modus = 2;
            _control = null;
            const int radius = 10;
            Width = radius * 2;
            Height = radius * 2;
            Left = -radius * 3;
            Top = -radius * 3;
        }

        public Overlay(GenericControl overControl) {
            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();
            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
            SetStyle(System.Windows.Forms.ControlStyles.Selectable, false);
            SetStyle(System.Windows.Forms.ControlStyles.StandardClick, false);
            SetStyle(System.Windows.Forms.ControlStyles.StandardDoubleClick, false);
            _modus = 1;
            _control = overControl;
            SetControl();
        }

        #endregion

        #region Properties

        protected override System.Windows.Forms.CreateParams CreateParams {
            get {
                var oParam = base.CreateParams;
                oParam.ExStyle |= (int)ExStyle.EX_NOACTIVATE | (int)ExStyle.EX_TOOLWINDOW | (int)ExStyle.EX_TOPMOST;
                oParam.Parent = IntPtr.Zero;
                return oParam;
            }
        }

        #endregion

        #region Methods

        public void Paint_Radius() {
            var g = CreateGraphics();
            g.Clear(Color.Magenta);
            g.DrawEllipse(new Pen(Color.Black, 3), 1, 1, Width - 3, Height - 3);
            g.DrawEllipse(Pens.Red, 1, 1, Width - 3, Height - 3);
            g.Dispose();
        }

        public void SetControl() {
            Width = _control.Width;
            Height = _control.Height;
            var p = _control.PointToScreen(Point.Empty);
            Left = p.X;
            Top = p.Y;
        }

        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e) {
            if (IsDisposed) { return; }
            base.OnPaint(e);
            switch (_modus) {
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

        private void Blinker_Tick(object sender, System.EventArgs e) {
            Opacity = Opacity > 0.5 ? 0.01 : 1;
            _count++;
            if (_count > 4) {
                Blinker.Enabled = false;
                Dispose();
            }
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

        #endregion
    }
}
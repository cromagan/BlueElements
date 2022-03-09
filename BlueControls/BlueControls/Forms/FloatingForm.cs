﻿using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace BlueControls.Forms {

    public partial class FloatingForm : Form {

        #region Fields

        internal static List<FloatingForm> AllBoxes = new();
        private readonly Control? _connectedControl;

        #endregion

        #region Constructors

        public FloatingForm() : this(enDesign.Form_QuickInfo) { }

        protected FloatingForm(enDesign design) : base(design) {
            // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
            InitializeComponent();
            SetStyle(ControlStyles.Selectable, false);
            SetStyle(ControlStyles.StandardClick, false);
            SetStyle(ControlStyles.StandardDoubleClick, false);
            // Initialisierungen nach dem Aufruf InitializeComponent() hinzufügen
            SetStyle(ControlStyles.ResizeRedraw, false);
            SetStyle(ControlStyles.SupportsTransparentBackColor, false);
            SetStyle(ControlStyles.Opaque, false);
            //The next 3 styles are allefor double buffering
            // Bei FloatingForms muss immer der Hinetergunrd gezeichnet haben. wir wollen ja einen schönen Rahmen haben.
            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserPaint, true);
            //BackColor = Color.FromArgb(255, 0, 255);
            //TransparencyKey = Color.FromArgb(255, 0, 255);
            AllBoxes.Add(this);
        }

        protected FloatingForm(Control? connectedControl, enDesign design) : this(design) => _connectedControl = connectedControl;

        #endregion

        #region Properties

        /// <summary>
        /// Floating Forms sind immer Topmost, darf aber hier nicht gesetzt werden und wird über
        /// CreateParams gesteuert. Wenn TopMost true wäre, würde das Form den Focus bekommen.
        /// </summary>
        public new bool TopMost {
            get => false;
            set => base.TopMost = false;
        }

        protected override CreateParams CreateParams {
            get {
                var oParam = base.CreateParams;
                oParam.ExStyle |= (int)enExStyle.EX_NOACTIVATE | (int)enExStyle.EX_TOOLWINDOW | (int)enExStyle.EX_TOPMOST;
                oParam.Parent = IntPtr.Zero;
                return oParam;
            }
        }

        #endregion

        #region Methods

        public new void Close() {
            try {
                if (AllBoxes != null && AllBoxes.Contains(this)) { AllBoxes.Remove(this); }
                base.Close();
            } catch {
                Close();
            }
        }

        public void Position_CenterScreen(Point bestPosition) {
            var screenNr = Generic.PointOnScreenNr(bestPosition);
            CheckMaxSize(screenNr);
            StartPosition = FormStartPosition.CenterScreen;
            var xpos = Screen.AllScreens[screenNr].WorkingArea.Left + ((Screen.AllScreens[screenNr].WorkingArea.Width - Width) / 2.0);
            var ypos = Screen.AllScreens[screenNr].WorkingArea.Top + ((Screen.AllScreens[screenNr].WorkingArea.Height - Height) / 2.0);
            Position_SetWindowIntoScreen(screenNr, (int)xpos, (int)ypos);
        }

        public void Position_LocateToMouse() {
            var screenNr = Generic.PointOnScreenNr(Cursor.Position);
            CheckMaxSize(screenNr);
            var ypos = Cursor.Position.Y + 15;
            var xpos = Cursor.Position.X + 15;
            if (xpos + Width > Screen.AllScreens[screenNr].Bounds.Right) {
                xpos = Cursor.Position.X - 5 - Width;
            }
            if (ypos + Height > Screen.AllScreens[screenNr].Bounds.Bottom) {
                ypos = Cursor.Position.Y - 5 - Height;
            }
            Position_SetWindowIntoScreen(screenNr, xpos, ypos);
        }

        public void Position_LocateToPosition(Point bestPosition) {
            var screenNr = Generic.PointOnScreenNr(bestPosition);
            CheckMaxSize(screenNr);
            Position_SetWindowIntoScreen(screenNr, bestPosition.X, bestPosition.Y);
        }

        public void Position_SetWindowIntoScreen(int screenNr, int xpos, int ypos) {
            //  Dim ScreenNr As Integer = PointOnScreenNr(BestPosition)
            CheckMaxSize(screenNr);
            if (xpos < Screen.AllScreens[screenNr].WorkingArea.Left) { xpos = Screen.AllScreens[screenNr].WorkingArea.Left; }
            if (ypos < Screen.AllScreens[screenNr].WorkingArea.Top) { ypos = Screen.AllScreens[screenNr].WorkingArea.Top; }
            if (xpos + Width > Screen.AllScreens[screenNr].WorkingArea.Right) { xpos = Screen.AllScreens[screenNr].WorkingArea.Right - Width; }
            if (ypos + Height > Screen.AllScreens[screenNr].WorkingArea.Bottom) { ypos = Screen.AllScreens[screenNr].WorkingArea.Bottom - Height; }
            Location = new Point(xpos, ypos);
        }

        public new void Show() {
            try {
                WindowsRemoteControl.ShowWindow(Handle, (int)enSW.ShowNoActivate);
            } catch (ObjectDisposedException) {
                // kommt vor, wenn der Aufbau zu lange dauert. Ignorierbar.
            } catch (Exception ex) {
                Develop.DebugPrint(ex);
            }
        }

        public new void ShowDialog() => Develop.DebugPrint(enFehlerArt.Fehler, "FloatingForms können nur mit Show aufgerufen werden.");

        internal static void Close(object? connectedControl, enDesign design) {
            foreach (var thisForm in AllBoxes) {
                if (!thisForm.IsDisposed) {
                    if (connectedControl == null || connectedControl == thisForm._connectedControl) {
                        if (design == enDesign.Undefiniert || thisForm.Design == design) {
                            try {
                                thisForm.Close();
                                Close(connectedControl, design);
                                return;
                            } catch (Exception ex) {
                                Develop.DebugPrint(ex);
                            }
                        }
                    }
                }
            }
        }

        internal static void Close(enDesign design) => Close(null, design);

        internal static void Close(object connectedControl) => Close(connectedControl, enDesign.Undefiniert);

        internal static bool IsShowing(object connectedControl) => AllBoxes.Any(thisForm => !thisForm.IsDisposed && connectedControl == thisForm._connectedControl);

        protected override void OnPaint(PaintEventArgs? e) {
            // MyBase.OnPaint(e) - comment out - do not call  http://stackoverflow.com/questions/592538/how-to-create-a-transparent-control-which-works-when-on-top-of-other-controls
            if (IsClosed || IsDisposed) { return; }
            if (BackgroundImage == null || Width != BackgroundImage.Width || Height != BackgroundImage.Height) {
                BackgroundImage = new Bitmap(Width, Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            }
            var tmpgr = Graphics.FromImage(BackgroundImage);
            Skin.Draw_Back(tmpgr, Design, enStates.Standard, DisplayRectangle, null, false);
            Skin.Draw_Border(tmpgr, Design, enStates.Standard, DisplayRectangle);
        }

        private void CheckMaxSize(int screenNr) {
            Width = Math.Min(Width, (int)(Screen.AllScreens[screenNr].WorkingArea.Width * 0.9));
            Height = Math.Min(Height, (int)(Screen.AllScreens[screenNr].WorkingArea.Height * 0.9));
        }

        #endregion
    }
}
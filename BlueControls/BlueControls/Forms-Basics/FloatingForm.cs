using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace BlueControls.Forms
{
    public partial class FloatingForm : Form
    {

        internal static List<FloatingForm> AllBoxes = new List<FloatingForm>();

        private readonly System.Windows.Forms.Control _ConnectedControl = null;


        public FloatingForm()
        {


            // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
            InitializeComponent();
            SetStyles();
            AllBoxes.Add(this);

        }

        public FloatingForm(System.Windows.Forms.Control ConnectedControl)
        {


            // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
            InitializeComponent();
            _ConnectedControl = ConnectedControl;
            SetStyles();
            AllBoxes.Add(this);
        }


        private void SetStyles()
        {

            SetStyle(System.Windows.Forms.ControlStyles.Selectable, false);
            SetStyle(System.Windows.Forms.ControlStyles.StandardClick, false);
            SetStyle(System.Windows.Forms.ControlStyles.StandardDoubleClick, false);


            // Initialisierungen nach dem Aufruf InitializeComponent() hinzufügen
            SetStyle(System.Windows.Forms.ControlStyles.ResizeRedraw, false);
            SetStyle(System.Windows.Forms.ControlStyles.SupportsTransparentBackColor, false);
            SetStyle(System.Windows.Forms.ControlStyles.Opaque, false);


            //The next 3 styles are allefor double buffering
            // Bei FloatingForms muss immer der Hinetergunrd gezeichnet haben. wir wollen ja einen schönen Rahmen haben.
            SetStyle(System.Windows.Forms.ControlStyles.DoubleBuffer, true);
            SetStyle(System.Windows.Forms.ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(System.Windows.Forms.ControlStyles.UserPaint, true);


            //BackColor = Color.FromArgb(255, 0, 255);
            //TransparencyKey = Color.FromArgb(255, 0, 255);
        }



        /// <summary>
        /// Floating Forms sind immer Topmost, darf aber hier nicht gesetzt werden und wird über
        /// CreateParams gesteuert. Wenn TopMost true wäre, würde das Form den Focus bekommen.
        /// </summary>
        public new bool TopMost
        {
            get
            {
                return false;
            }
            set
            {
                base.TopMost = false;
            }
        }




        protected override System.Windows.Forms.CreateParams CreateParams
        {
            get
            {
                var oParam = base.CreateParams;


                oParam.ExStyle |= (int)enExStyle.EX_NOACTIVATE | (int)enExStyle.EX_TOOLWINDOW | (int)enExStyle.EX_TOPMOST;
                oParam.Parent = IntPtr.Zero;

                return oParam;
            }
        }


        private void CheckMaxSize(int ScreenNr)
        {

            Width = Math.Min(Width, (int)(System.Windows.Forms.Screen.AllScreens[ScreenNr].WorkingArea.Width * 0.9));
            Height = Math.Min(Height, (int)(System.Windows.Forms.Screen.AllScreens[ScreenNr].WorkingArea.Height * 0.9));
        }


        public void Position_CenterScreen(System.Drawing.Point BestPosition)
        {
            var ScreenNr = modAllgemein.PointOnScreenNr(BestPosition);
            CheckMaxSize(ScreenNr);

            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            var Xpos = System.Windows.Forms.Screen.AllScreens[ScreenNr].WorkingArea.Left + (System.Windows.Forms.Screen.AllScreens[ScreenNr].WorkingArea.Width - Width) / 2.0;
            var Ypos = System.Windows.Forms.Screen.AllScreens[ScreenNr].WorkingArea.Top + (System.Windows.Forms.Screen.AllScreens[ScreenNr].WorkingArea.Height - Height) / 2.0;
            Position_SetWindowIntoScreen(ScreenNr, (int)Xpos, (int)Ypos);
        }


        public void Position_LocateToPosition(System.Drawing.Point BestPosition)
        {
            var ScreenNr = modAllgemein.PointOnScreenNr(BestPosition);

            CheckMaxSize(ScreenNr);
            Position_SetWindowIntoScreen(ScreenNr, BestPosition.X, BestPosition.Y);
        }


        public void Position_LocateToMouse()
        {

            var ScreenNr = modAllgemein.PointOnScreenNr(System.Windows.Forms.Cursor.Position);

            CheckMaxSize(ScreenNr);
            var Ypos = System.Windows.Forms.Cursor.Position.Y + 15;
            var Xpos = System.Windows.Forms.Cursor.Position.X + 15;


            if (Xpos + Width > System.Windows.Forms.Screen.AllScreens[ScreenNr].Bounds.Right)
            {
                Xpos = System.Windows.Forms.Cursor.Position.X - 5 - Width;
            }

            if (Ypos + Height > System.Windows.Forms.Screen.AllScreens[ScreenNr].Bounds.Bottom)
            {
                Ypos = System.Windows.Forms.Cursor.Position.Y - 5 - Height;
            }


            Position_SetWindowIntoScreen(ScreenNr, Xpos, Ypos);
        }


        public void Position_SetWindowIntoScreen(int ScreenNr, int Xpos, int Ypos)
        {

            //  Dim ScreenNr As Integer = PointOnScreenNr(BestPosition)
            CheckMaxSize(ScreenNr);

            if (Xpos < System.Windows.Forms.Screen.AllScreens[ScreenNr].WorkingArea.Left) { Xpos = System.Windows.Forms.Screen.AllScreens[ScreenNr].WorkingArea.Left; }
            if (Ypos < System.Windows.Forms.Screen.AllScreens[ScreenNr].WorkingArea.Top) { Ypos = System.Windows.Forms.Screen.AllScreens[ScreenNr].WorkingArea.Top; }
            if (Xpos + Width > System.Windows.Forms.Screen.AllScreens[ScreenNr].WorkingArea.Right) { Xpos = System.Windows.Forms.Screen.AllScreens[ScreenNr].WorkingArea.Right - Width; }
            if (Ypos + Height > System.Windows.Forms.Screen.AllScreens[ScreenNr].WorkingArea.Bottom) { Ypos = System.Windows.Forms.Screen.AllScreens[ScreenNr].WorkingArea.Bottom - Height; }

            Location = new Point(Xpos, Ypos);
        }

        public new void ShowDialog()
        {
            Develop.DebugPrint(enFehlerArt.Fehler, "FloatingForms können nur mit Show aufgerufen werden.");
        }

        public new void Show()
        {
            try
            {
                modFernsteuerung.ShowWindow(Handle, (int)modFernsteuerung.enSW.ShowNoActivate);
            }
            catch (ObjectDisposedException _)
            {
                // kommt vor, wenn der Aufbau zu lange dauert. Ignorierbar.
            }
            catch (Exception ex)
            {
                Develop.DebugPrint(ex);
            }
        }


        public new void Close()
        {
            AllBoxes.Remove(this);
            base.Close();
        }

        internal static void Close(object ConnectedControl, enDesign Design)
        {

            foreach (var ThisForm in AllBoxes)
            {
                if (!ThisForm.IsDisposed)
                {

                    if (ConnectedControl == null || ConnectedControl == ThisForm._ConnectedControl)
                    {
                        if (Design == enDesign.Undefiniert || ((IDesignAble)ThisForm).Design == Design)
                        {
                            try
                            {
                                ThisForm.Close();
                                Close(ConnectedControl, Design);
                                return;
                            }
                            catch (Exception ex)
                            {
                                Develop.DebugPrint(ex);
                            }

                        }
                    }
                }
            }
        }


        internal static void Close(enDesign Design)
        {
            Close(null, Design);
        }


        internal static void Close(object ConnectedControl)
        {
            Close(ConnectedControl, enDesign.Undefiniert);
        }

        internal static bool IsShowing(object ConnectedControl)
        {
            foreach (var ThisForm in AllBoxes)
            {
                if (!ThisForm.IsDisposed && ConnectedControl == ThisForm._ConnectedControl) { return true; }
            }
            return false;
        }


        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            // MyBase.OnPaint(e) - comment out - do not call  http://stackoverflow.com/questions/592538/how-to-create-a-transparent-control-which-works-when-on-top-of-other-controls


            if (BackgroundImage == null || Width != BackgroundImage.Width || Height != BackgroundImage.Height)
            {
                BackgroundImage = new Bitmap(Width, Height);
            }

            var TMPGR = Graphics.FromImage(BackgroundImage);

            GenericControl.Skin.Draw_Back(TMPGR, Design, enStates.Standard, DisplayRectangle, null, false);
            GenericControl.Skin.Draw_Border(TMPGR, Design, enStates.Standard, DisplayRectangle);
        }


    }
}
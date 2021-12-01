// Authors:
// Christian Peter
//
// Copyright (c) 2021 Christian Peter
// https://github.com/cromagan/BlueElements
//
// License: GNU Affero General Public License v3.0
// https://github.com/cromagan/BlueElements/blob/master/LICENSE
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using BlueBasics;
using BlueControls.BlueDatabaseDialogs;
using BlueControls.Enums;
using BlueControls.Forms;
using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Imaging;

//Inherits UserControl ' -> Gibt Focus an Child!
//Inherits ContainerControl -> ?
//Inherits Panel '-> Alles ist ein Container!
//Inherits ScrollableControl - > keine Tastatur/Mouseabfragen

namespace BlueControls.Controls
{
    public class GenericControl : System.Windows.Forms.Control
    {
        #region Fields

        protected bool _MouseHighlight = true;

        private readonly bool _UseBackBitmap = false;

        private Bitmap _BitmapOfControl;

        private bool _GeneratingBitmapOfControl;

        private bool _MousePressing;

        private enPartentType _MyParentType = enPartentType.Unbekannt;

        // Dieser Codeblock ist im Interface IQuickInfo herauskopiert und muss überall Identisch sein.
        private string _QuickInfo = "";

        #endregion

        #region Constructors

        protected GenericControl() : this(false, false)
        {
        }

        protected GenericControl(bool doubleBuffer, bool useBackgroundBitmap) : base()
        {
            // Dieser Aufruf ist für den Designer erforderlich.
            // InitializeComponent()
            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
            SetStyle(System.Windows.Forms.ControlStyles.ContainerControl, false);
            SetStyle(System.Windows.Forms.ControlStyles.ResizeRedraw, false);
            SetStyle(System.Windows.Forms.ControlStyles.SupportsTransparentBackColor, false);
            SetStyle(System.Windows.Forms.ControlStyles.Opaque, true);
            //The next 3 styles are allefor double buffering
            SetStyle(System.Windows.Forms.ControlStyles.UserPaint, true);
            SetStyle(System.Windows.Forms.ControlStyles.AllPaintingInWmPaint, true);
            if (doubleBuffer)
            {
                SetDoubleBuffering();
            }
            _UseBackBitmap = useBackgroundBitmap;
            Translate = true;
        }

        #endregion

        #region Properties

        [DefaultValue(false)]
        public override bool AutoSize
        {
            get => false; //MyBase.AutoSize
            set => base.AutoSize = false;
        }

        [Category("Darstellung")]
        [DefaultValue("")]
        [Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
        [Description("QuickInfo des Steuerelementes - im extTXT-Format")]
        public string QuickInfo
        {
            get => _QuickInfo;
            set
            {
                if (_QuickInfo != value)
                {
                    Forms.QuickInfo.Close();
                    _QuickInfo = value;
                    OnQuickInfoChanged();
                }
            }
        }

        public virtual string QuickInfoText => _QuickInfo;

        [DefaultValue(true)]
        public bool Translate { get; set; }

        protected override bool ScaleChildren => false;

        #endregion

        #region Methods

        public static System.Windows.Forms.Form ParentForm(System.Windows.Forms.Control o)
        {
            if (o == null) { return null; }

            Develop.DebugPrint_Disposed(o.IsDisposed);

            do
            {
                switch (o)
                {
                    case Form frm:
                        return frm;

                    case null:
                        return null;

                    default:
                        o = o?.Parent; //Manchmal ist o null. MultiThreading?
                        break;
                }
            } while (true);
        }

        public static enPartentType Typ(System.Windows.Forms.Control control)
        {
            switch (control)
            {
                case null:
                    return enPartentType.Nothing;

                case GroupBox _:
                    {
                        if (control.Parent is System.Windows.Forms.TabPage TP)
                        {
                            if (TP.Parent == null) { return enPartentType.Unbekannt; }
                            if (TP.Parent is RibbonBar) { return enPartentType.RibbonGroupBox; }
                        }
                        return enPartentType.GroupBox;
                    }

                case LastFilesCombo _:
                    return enPartentType.LastFilesCombo;
                //Is = "BlueBasics.ComboBox"

                case ComboBox _ when ((ComboBox)control).ParentType() == enPartentType.RibbonPage:
                    return enPartentType.RibbonBarCombobox;

                case ComboBox _:
                    return enPartentType.ComboBox;
                // Is = "BlueBasics.TabControl"

                case RibbonBar _:
                    return enPartentType.RibbonControl;

                case TabControl _:
                    return enPartentType.TabControl;
                // Is = "BlueBasics.TabPage"

                case tabAdministration _:

                case System.Windows.Forms.TabPage _ when control.Parent is RibbonBar:
                    return enPartentType.RibbonPage;

                case System.Windows.Forms.TabPage _:
                    return enPartentType.TabPage;
                //Is = "BlueBasics.Slider"

                case Slider _:
                    return enPartentType.Slider;
                //Is = "FRMMSGBOX"

                case FloatingForm _:
                    return enPartentType.MsgBox;

                case DialogWithOkAndCancel _:
                    return enPartentType.MsgBox;

                case TextBox _:
                    return enPartentType.TextBox;

                case ListBox _:
                    return enPartentType.ListBox;

                case EasyPic _:
                    return enPartentType.EasyPic;

                case Button _:
                    return enPartentType.Button;

                case Line _:
                    return enPartentType.Line;

                case Caption _:
                    return enPartentType.Caption;

                case Formula _:
                    return enPartentType.Formula;

                case Form _:
                    return enPartentType.Form;

                case Table _:
                    return enPartentType.Table;

                case System.Windows.Forms.Panel _:
                    return enPartentType.Panel;

                case FlexiControlForCell _:
                    return enPartentType.FlexiControlForCell;

                case FlexiControl _:
                    return enPartentType.FlexiControl;

                default:
                    return enPartentType.Nothing;
            }
        }

        public Bitmap BitmapOfControl()
        {
            if (!_UseBackBitmap || _GeneratingBitmapOfControl) { return null; }
            _GeneratingBitmapOfControl = true;
            if (_BitmapOfControl == null) { Refresh(); }
            _GeneratingBitmapOfControl = false;
            return _BitmapOfControl;
        }

        public bool ContainsMouse() => ClientRectangle.Contains(PointToClient(System.Windows.Forms.Cursor.Position));

        public void DoQuickInfo()
        {
            if (string.IsNullOrEmpty(_QuickInfo) && string.IsNullOrEmpty(QuickInfoText))
            {
                Forms.QuickInfo.Close();
            }
            else
            {
                if (ContainsMouse())
                {
                    Forms.QuickInfo.Show(QuickInfoText);
                }
                else
                {
                    Forms.QuickInfo.Close();
                }
            }
        }

        public Point MousePos()
        {
            if (InvokeRequired)
            {
                return (Point)Invoke(new Func<Point>(() => MousePos()));
            }
            Develop.DebugPrint_Disposed(IsDisposed);
            return PointToClient(System.Windows.Forms.Cursor.Position);
        }

        // https://msdn.microsoft.com/de-de/library/ms229605(v=vs.110).aspx
        public void PerformAutoScale()
        {
            // NIX TUN!!!!
        }

        /// <summary>
        /// Veranlaßt, das das Control neu gezeichnet wird.
        /// </summary>
        /// <remarks></remarks>
        public override void Refresh()
        {
            if (IsDisposed) { return; }
            DoDraw(CreateGraphics());
        }

        public void Scale()
        {
            // NIX TUN!!!!
        }

        internal static bool AllEnabled(System.Windows.Forms.Control control)
        {
            Develop.DebugPrint_Disposed(control.IsDisposed);
            do
            {
                if (control == null) { return true; }
                if (control.IsDisposed) { return false; }
                if (!control.Enabled) { return false; }
                control = control.Parent;
            } while (true);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _BitmapOfControl?.Dispose();
                _BitmapOfControl = null;
            }
        }

        protected virtual void DrawControl(Graphics gr, enStates state) => Develop.DebugPrint_RoutineMussUeberschriebenWerden();

        //MyBase.ScaleChildren
        protected override Rectangle GetScaledBounds(Rectangle bounds, SizeF factor, System.Windows.Forms.BoundsSpecified specified) => bounds;

        protected bool MousePressing() => _MousePressing;

        protected override void OnEnabledChanged(System.EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnEnabledChanged(e)));
                return;
            }
            if (IsDisposed) { return; }
            base.OnEnabledChanged(e);
            Invalidate();
        }

        protected override void OnGotFocus(System.EventArgs e)
        {
            if (IsDisposed) { return; }
            if (!GetStyle(System.Windows.Forms.ControlStyles.Selectable))
            {
                Parent.SelectNextControl(this, true, true, true, true);
            }
            else
            {
                base.OnGotFocus(e);
                Invalidate();
            }
        }

        protected override void OnKeyDown(System.Windows.Forms.KeyEventArgs e)
        {
            if (IsDisposed) { return; }
            base.OnKeyDown(e);
        }

        protected override void OnKeyPress(System.Windows.Forms.KeyPressEventArgs e)
        {
            if (IsDisposed) { return; }
            base.OnKeyPress(e);
        }

        protected override void OnKeyUp(System.Windows.Forms.KeyEventArgs e)
        {
            if (IsDisposed) { return; }
            base.OnKeyUp(e);
        }

        protected override void OnLostFocus(System.EventArgs e)
        {
            if (IsDisposed) { return; }
            if (GetStyle(System.Windows.Forms.ControlStyles.Selectable))
            {
                //if (_MousePressing) { OnMouseUp(new System.Windows.Forms.MouseEventArgs(System.Windows.Forms.MouseButtons.None, 0, 0, 0, 0)); }
                _MouseHighlight = false;
                base.OnLostFocus(e);
                Invalidate();
            }
        }

        protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e)
        {
            lock (this)
            {
                if (IsDisposed) { return; }
                if (_MousePressing) { return; }
                _MousePressing = true;
                Forms.QuickInfo.Close();
                if (Enabled)
                {
                    if (GetStyle(System.Windows.Forms.ControlStyles.Selectable) && Focus()) { Focus(); }
                }
                base.OnMouseDown(e);
            }
        }

        protected override void OnMouseEnter(System.EventArgs e)
        {
            if (IsDisposed) { return; }
            base.OnMouseEnter(e);
            if (!string.IsNullOrEmpty(_QuickInfo) || !string.IsNullOrEmpty(QuickInfoText)) { Forms.QuickInfo.Show(QuickInfoText); }
        }

        protected override void OnMouseLeave(System.EventArgs e)
        {
            if (IsDisposed) { return; }
            base.OnMouseLeave(e);
            DoQuickInfo();
        }

        protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e)
        {
            lock (this)
            {
                if (IsDisposed) { return; }
                base.OnMouseMove(e);
                DoQuickInfo();
            }
        }

        protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
            if (IsDisposed) { return; }
            if (!_MousePressing) { return; }
            _MousePressing = false;
            base.OnMouseUp(e);
        }

        protected override void OnMouseWheel(System.Windows.Forms.MouseEventArgs e)
        {
            if (IsDisposed) { return; }
            _MousePressing = false;
            base.OnMouseWheel(e);
        }

        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e) =>
            // MyBase.OnPaint(e) - comment out - do not call  http://stackoverflow.com/questions/592538/how-to-create-a-transparent-control-which-works-when-on-top-of-other-controls
            DoDraw(e.Graphics);

        protected override void OnPaintBackground(System.Windows.Forms.PaintEventArgs pevent)
        {
            // do not allow the background to be painted
            // Um flimmern zu vermeiden!
        }

        protected virtual void OnQuickInfoChanged()
        {
            // Dummy, dass die angeleeiteten Controls reagieren können.
        }

        protected override void OnSizeChanged(System.EventArgs e)
        {
            if (IsDisposed) { return; }
            Invalidate();
            if (_BitmapOfControl != null)
            {
                if (_BitmapOfControl.Width < Width || _BitmapOfControl.Height < Height)
                {
                    _BitmapOfControl.Dispose();
                    _BitmapOfControl = null;
                }
            }
            base.OnSizeChanged(e);
        }

        protected System.Windows.Forms.Form ParentForm() => ParentForm(Parent);

        protected enPartentType ParentType()
        {
            if (Parent == null) { return enPartentType.Unbekannt; }
            if (_MyParentType != enPartentType.Unbekannt) { return _MyParentType; }
            _MyParentType = Typ(Parent);
            return _MyParentType;
        }

        protected override void ScaleControl(SizeF factor, System.Windows.Forms.BoundsSpecified specified)
        {
            factor = new SizeF(1, 1);
            base.ScaleControl(factor, specified);
        }

        protected void SetDoubleBuffering() => SetStyle(System.Windows.Forms.ControlStyles.DoubleBuffer, true);

        //MyBase.GetScaledBounds(bounds, factor, specified)

        protected void SetNotFocusable()
        {
            Develop.DebugPrint_Disposed(IsDisposed);
            TabStop = false;
            TabIndex = 0;
            CausesValidation = false;
            SetStyle(System.Windows.Forms.ControlStyles.Selectable, false);
            //SetStyle(System.Windows.Forms.ControlStyles.StandardClick, false);
            //SetStyle(System.Windows.Forms.ControlStyles.StandardDoubleClick, false);
        }

        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            try
            {
                //https://www.vb-paradise.de/allgemeines/tipps-tricks-und-tutorials/windows-forms/50038-wndproc-kleine-liste-aller-messages/
                if (m.Msg == (int)enWndProc.WM_ERASEBKGND) { return; }
                base.WndProc(ref m);
            }
            catch (Exception ex)
            {
                Develop.DebugPrint(ex);
            }
        }

        private void DoDraw(Graphics gr)
        {
            if (IsDisposed)
            {
                gr.Clear(Color.Red);
                return;
            }

            if (Develop.Exited || IsDisposed || !Visible) { return; }
            lock (this)
            {
                if (!Skin.inited)
                {
                    if (DesignMode)
                    {
                        Skin.LoadSkin();
                    }
                    else
                    {
                        return;
                    }
                }
                if (Width < 1 || Height < 1) { return; }
                try
                {
                    if (_UseBackBitmap)
                    {
                        if (_BitmapOfControl == null) { _BitmapOfControl = new Bitmap(ClientSize.Width, ClientSize.Height, PixelFormat.Format32bppPArgb); }
                        var TMPGR = Graphics.FromImage(_BitmapOfControl);
                        DrawControl(TMPGR, IsStatus());
                        if (_BitmapOfControl != null)
                        {
                            gr.DrawImage(_BitmapOfControl, 0, 0);
                        }
                        TMPGR.Dispose();
                    }
                    else
                    {
                        DrawControl(gr, IsStatus());
                    }
                }
                catch
                {
                    return;
                }
                // UmRandung für DesignMode ------------
                if (DesignMode)
                {
                    using Pen P = new(Color.FromArgb(128, 255, 0, 0));
                    gr.DrawRectangle(P, 0, 0, Width - 1, Height - 1);
                }
            }
        }

        private enStates IsStatus()
        {
            if (!Enabled) { return enStates.Standard_Disabled; }
            var S = enStates.Standard;
            if (_MouseHighlight && ContainsMouse()) { S |= enStates.Standard_MouseOver; }
            if (_MousePressing)
            {
                if (_MouseHighlight) { S |= enStates.Standard_MousePressed; }
                if (GetStyle(System.Windows.Forms.ControlStyles.Selectable) && CanFocus) { S |= enStates.Standard_HasFocus; }
            }
            else
            {
                if (GetStyle(System.Windows.Forms.ControlStyles.Selectable) && CanFocus && Focused) { S |= enStates.Standard_HasFocus; }
            }
            return S;
        }

        #endregion
    }
}
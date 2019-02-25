#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2019 Christian Peter
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
#endregion


using BlueBasics;
using BlueControls.DialogBoxes;
using BlueControls.Enums;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueDatabase;
using System;
using System.ComponentModel;
using System.Drawing;


//Inherits UserControl ' -> Gibt Focus an Child!
//Inherits ContainerControl -> ?
//Inherits Panel '-> Alles ist ein Container!
//Inherits ScrollableControl - > keine Tastatur/Mouseabfragen






namespace BlueControls.Controls
{
    public class GenericControl : System.Windows.Forms.Control
    {
        public static readonly clsSkin Skin = new clsSkin();
        protected RowItem tmpSkinRow;



        private enPartentType _MyParentType = enPartentType.Unbekannt;

        protected GenericControl()
        {

            // Dieser Aufruf ist für den Designer erforderlich.
            // InitializeComponent()

            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
            SetStyle(System.Windows.Forms.ControlStyles.ContainerControl, false);
            SetStyle(System.Windows.Forms.ControlStyles.ResizeRedraw, false);

            SetStyle(System.Windows.Forms.ControlStyles.SupportsTransparentBackColor, false);
            SetStyle(System.Windows.Forms.ControlStyles.Opaque, false);

            //The next 3 styles are allefor double buffering

            SetStyle(System.Windows.Forms.ControlStyles.UserPaint, true);


            InitializeSkin();
            Skin.SkinChanged += SkinChanged;

        }

        private void SkinChanged(object sender, System.EventArgs e)
        {
            InitializeSkin();
        }

        public void SetDoubleBuffering()
        {
            SetStyle(System.Windows.Forms.ControlStyles.DoubleBuffer, true);
            SetStyle(System.Windows.Forms.ControlStyles.AllPaintingInWmPaint, true);
        }


        #region  Standard-Variablen 
        private bool _MousePressing;
        protected bool _MouseHighlight = true;
        #endregion

        protected virtual void DrawControl(Graphics gr, enStates state)
        {
            Develop.DebugPrint_RoutineMussUeberschriebenWerden();
        }
        protected virtual void InitializeSkin()
        {
            Develop.DebugPrint_RoutineMussUeberschriebenWerden();
        }


        #region  AutoScale deaktivieren 
        // https://msdn.microsoft.com/de-de/library/ms229605(v=vs.110).aspx



        public void PerformAutoScale()
        {
            // NIX TUN!!!!
        }

        public void Scale()
        {
            // NIX TUN!!!!
        }



        protected override void ScaleControl(SizeF factor, System.Windows.Forms.BoundsSpecified specified)
        {
            factor = new SizeF(1, 1);
            base.ScaleControl(factor, specified);
        }

        protected override bool ScaleChildren
        {
            get
            {
                return false; //MyBase.ScaleChildren
            }
        }

        [DefaultValue(false)]
        public override bool AutoSize
        {
            get
            {
                return false; //MyBase.AutoSize
            }
            set
            {
                base.AutoSize = false;
            }
        }

        protected override Rectangle GetScaledBounds(Rectangle bounds, SizeF factor, System.Windows.Forms.BoundsSpecified specified)
        {
            return bounds; //MyBase.GetScaledBounds(bounds, factor, specified)
        }
        #endregion

        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            //https://www.vb-paradise.de/allgemeines/tipps-tricks-und-tutorials/windows-forms/50038-wndproc-kleine-liste-aller-messages/
            if (m.Msg == (int)enWndProc.WM_ERASEBKGND) { return; }

            base.WndProc(ref m);
        }


        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            // MyBase.OnPaint(e) - comment out - do not call  http://stackoverflow.com/questions/592538/how-to-create-a-transparent-control-which-works-when-on-top-of-other-controls
            DoDraw(e.Graphics, false);
        }






        /// <summary>
        /// Veranlaßt, das das Control neu gezeichnet wird.
        /// </summary>
        /// <remarks></remarks>
        public override void Refresh()
        {
            if (IsDisposed) { return; }
            DoDraw(CreateGraphics(), true);
        }


        protected override void OnEnabledChanged(System.EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnEnabledChanged(e)));
                return;
            }

            if (IsDisposed) { return; }
            tmpSkinRow = null;
            base.OnEnabledChanged(e);
            Invalidate();
        }


        protected override void OnPaintBackground(System.Windows.Forms.PaintEventArgs pevent)
        {
            // do not allow the background to be painted
            // Um flimmern zu vermeiden!
        }




        private void DoDraw(Graphics GR, bool IgnoreVisible)
        {
            if (IsDisposed) { return; }

            lock (this)
            {
                if (!Visible && !IgnoreVisible) { return; }

                if (!Skin.IsReady())
                {
                    if (DesignMode)
                    {
                        Skin.Skin = enSkin.Windows_10;
                    }
                    else
                    {
                        return;
                    }
                }


                if (Width < 1 || Height < 1) { return; }

                DrawControl(GR, IsStatus());


                // UmRandung für DesignMode ------------
                if (DesignMode)
                {
                    using (var P = new Pen(Color.FromArgb(128, 255, 0, 0)))
                    {
                        GR.DrawRectangle(P, 0, 0, Width - 1, Height - 1);
                    }
                }

                foreach (System.Windows.Forms.Control c in Controls)
                {
                    c.Invalidate();
                }

            }
        }




        public Point MousePos()
        {
            if (InvokeRequired)
            {
                return (Point)Invoke(new Action(() => MousePos()));
            }

            Develop.DebugPrint_Disposed(IsDisposed);
            return PointToClient(System.Windows.Forms.Cursor.Position);
        }

        internal bool MousePressing()
        {
            return _MousePressing;
        }

        public bool ContainsMouse()
        {
            return ClientRectangle.Contains(PointToClient(System.Windows.Forms.Cursor.Position));
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



        #region  Focus-Verwaltung 
        protected override void OnGotFocus(System.EventArgs e)
        {
            if (IsDisposed) { return; }

            if (!GetStyle(System.Windows.Forms.ControlStyles.Selectable))
            {
                Parent.SelectNextControl(this, true, true, true, true);
            }
            else
            {
                tmpSkinRow = null;
                base.OnGotFocus(e);
                Invalidate();
            }
        }

        protected override void OnLostFocus(System.EventArgs e)
        {
            if (IsDisposed) { return; }

            if (GetStyle(System.Windows.Forms.ControlStyles.Selectable))
            {
                if (_MousePressing) { OnMouseUp(new System.Windows.Forms.MouseEventArgs(System.Windows.Forms.MouseButtons.None, 0, 0, 0, 0)); }
                _MouseHighlight = false;
                tmpSkinRow = null;
                base.OnLostFocus(e);
                Invalidate();
            }
        }


        #endregion

        #region  Key-Verwaltung 
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

        #endregion

        #region  MousePos-Verwaltung 


        protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e)
        {

            lock (this)
            {
                if (IsDisposed) { return; }
                if (_MousePressing) { return; }

                _MousePressing = true;

                DialogBoxes.QuickInfo.Close();

                if (GetStyle(System.Windows.Forms.ControlStyles.Selectable) && Focus()) { Focus(); }
                tmpSkinRow = null;
                base.OnMouseDown(e);

            }
        }


        protected override void OnMouseLeave(System.EventArgs e)
        {
            if (IsDisposed) { return; }
            tmpSkinRow = null;
            base.OnMouseLeave(e);


            if (this is IQuickInfo tempVar)
            {
                if (!string.IsNullOrEmpty(tempVar.QuickInfo)) { DialogBoxes.QuickInfo.Close(); }
            }

        }



        protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e)
        {

            lock (this)
            {


                if (IsDisposed) { return; }

                if (this is IQuickInfo tempVar)
                {
                    if (!ContainsMouse())
                    {
                        if (!string.IsNullOrEmpty(tempVar.QuickInfo)) { DialogBoxes.QuickInfo.Close(); }
                    }
                    else
                    {
                        ShowQuickInfo();
                    }
                }

                base.OnMouseMove(e);

            }
        }

        protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
            if (IsDisposed) { return; }
            if (!_MousePressing) { return; }
            _MousePressing = false;
            tmpSkinRow = null;
            base.OnMouseUp(e);
        }


        protected override void OnMouseWheel(System.Windows.Forms.MouseEventArgs e)
        {
            if (IsDisposed) { return; }
            _MousePressing = false;
            base.OnMouseWheel(e);
        }

        protected override void OnMouseEnter(System.EventArgs e)
        {
            if (IsDisposed) { return; }
            tmpSkinRow = null;
            base.OnMouseEnter(e);
            ShowQuickInfo();
        }


        protected override void OnSizeChanged(System.EventArgs e)
        {
            if (IsDisposed) { return; }
            Invalidate();
            base.OnSizeChanged(e);
        }
        #endregion


        private void ShowQuickInfo()
        {
            if (IsDisposed) { return; }
            if (this is IQuickInfo tempVar)
            {
                if (string.IsNullOrEmpty(tempVar.QuickInfo)) { return; }
                DialogBoxes.QuickInfo.Show(tempVar.QuickInfo);
            }
        }


        public enum enPartentType
        {
            Unbekannt = -1,

            Nothing = 1,
            GroupBox = 2,
            RibbonBarCombobox = 3,
            Slider = 4,
            ComboBox = 5,
            //    RibbonBar = 6
            MsgBox = 7,

            TextBox = 8,
            ListBox = 9,
            EasyPic = 10,
            Button = 11,
            Line = 12,
            Caption = 13,

            TabPage = 14,
            TabControl = 15,
            Formula = 16,

            RibbonControl = 17,
            RibbonPage = 18,


            RibbonGroupBox = 19,

            LastFilesCombo = 20,

            Form = 21,

            Table = 22,

            Panel = 23,

            FlexiControl = 24,

            FlexiControlForCell = 25


        }


        public static enPartentType Typ(System.Windows.Forms.Control cControl)
        {

            switch (cControl)
            {
                case null:
                    return enPartentType.Nothing;
                case GroupBox _:
                    {
                        if (cControl.Parent is TabPage TP)
                        {

                            if (TP.Parent == null) { return enPartentType.Unbekannt; }

                            if (((TabControl)TP.Parent).IsRibbonBar)
                            {
                                return enPartentType.RibbonGroupBox;
                            }
                        }
                        return enPartentType.GroupBox;
                    }
                case LastFilesCombo _:
                    return enPartentType.LastFilesCombo;
                //Is = "BlueBasics.ComboBox"
                case ComboBox _ when ((ComboBox)cControl).ParentType() == enPartentType.RibbonPage:
                    return enPartentType.RibbonBarCombobox;
                case ComboBox _:
                    return enPartentType.ComboBox;
                // Is = "BlueBasics.TabControl"
                case TabControl _ when ((TabControl)cControl).IsRibbonBar:
                    return enPartentType.RibbonControl;
                case TabControl _:
                    return enPartentType.TabControl;
                // Is = "BlueBasics.TabPage"
                case TabPage _ when cControl.Parent != null && ((TabControl)cControl.Parent).IsRibbonBar:
                    return enPartentType.RibbonPage;
                case TabPage _:
                    return enPartentType.TabPage;
                //Is = "BlueBasics.Slider"
                case Slider _:
                    return enPartentType.Slider;
                //Is = "FRMMSGBOX"
                case Forms.FloatingForm _:
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

        protected enPartentType ParentType()
        {
            if (Parent == null) { return enPartentType.Unbekannt; }
            if (_MyParentType != enPartentType.Unbekannt) { return _MyParentType; }
            _MyParentType = Typ(Parent);
            return _MyParentType;
        }

        protected void SetNotFocusable()
        {
            Develop.DebugPrint_Disposed(IsDisposed);
            TabStop = false;
            TabIndex = 0;
            CausesValidation = false;
            SetStyle(System.Windows.Forms.ControlStyles.Selectable, false);
            SetStyle(System.Windows.Forms.ControlStyles.StandardClick, false);
            SetStyle(System.Windows.Forms.ControlStyles.StandardDoubleClick, false);
        }

        protected Form ParentForm()
        {
            Develop.DebugPrint_Disposed(IsDisposed);
            var o = Parent;

            do
            {
                switch (o)
                {
                    case null:
                        return null;
                    case Form frm:
                        return frm;
                    default:
                        o = o.Parent;
                        break;
                }
            } while (true);

        }

    }
}

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
using BlueBasics.Enums;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueControls.ItemCollection;
using BlueDatabase.EventArgs;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using static BlueBasics.Extensions;
using static BlueBasics.FileOperations;

namespace BlueControls.Controls
{
    [DefaultEvent("ImageChanged")]
    public sealed partial class EasyPic : IContextMenu, IBackgroundNone
    {



        private Bitmap _Bitmap = null;
        private int _MaxSize = -1;


        private int _Richt;

        //private int _X;
        //private int _Y;
        //private float _Zoom = 1;

        public EasyPic()
        {

            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();

            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
            SetNotFocusable();
        }



        public event EventHandler<ContextMenuInitEventArgs> ContextMenuInit;
        public event EventHandler<ContextMenuItemClickedEventArgs> ContextMenuItemClicked;
        public event EventHandler ImageChanged;
        public event EventHandler<DatabaseGiveBackEventArgs> ConnectedDatabase;


        [DefaultValue(-1)]
        public int MaxSize
        {
            get => _MaxSize;
            set
            {
                if (value == 0) { value = 1; }
                if (value < -1) { value = -1; }
                _MaxSize = value;
            }
        }


        [DefaultValue(enSorceType.Nichts)]
        public enSorceType SorceType { get; private set; }

        [DefaultValue("")]
        public string SorceName { get; private set; }

        [DefaultValue((Bitmap)null)]
        public Bitmap Bitmap
        {
            get
            {
                return _Bitmap;
            }
            private set
            {
                if (_Bitmap == null && value == null) { return; }
                _Bitmap = value;

                if (_Bitmap == null)
                {
                    SorceType = enSorceType.Nichts;
                }
                else
                {
                    SorceType = enSorceType.SetedByProperty;
                }

                SorceName = string.Empty;
                OnImageChanged();
                ZoomFitInvalidateAndCheckButtons();
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




        public void ContextMenu_Show(object sender, System.Windows.Forms.MouseEventArgs e)
        {

            var ThisContextMenu = new ItemCollectionList(enBlueListBoxAppearance.KontextMenu);

            if (_Bitmap != null)
            {
                ThisContextMenu.Add(new TextListItem("ExF", "Externes Fenster öffnen"));
                ThisContextMenu.Add(enContextMenuComands.Speichern);
            }

            var UserMenu = new ItemCollectionList(enBlueListBoxAppearance.KontextMenu);

            OnContextMenuInit(new ContextMenuInitEventArgs(null, UserMenu));

            if (ThisContextMenu.Count > 0 && UserMenu.Count > 0) { ThisContextMenu.Add(new LineListItem()); }
            if (UserMenu.Count > 0) { ThisContextMenu.AddRange(UserMenu); }

            if (ThisContextMenu.Count > 0)
            {
                ThisContextMenu.Add(new LineListItem());
                ThisContextMenu.Add(enContextMenuComands.Abbruch);

                var _ContextMenu = FloatingInputBoxListBoxStyle.Show(ThisContextMenu, null, this, Translate);
                _ContextMenu.ItemClicked += ContextMenuItemClickedInternalProcessig;
            }

        }







        private void OnImageChanged()
        {
            ImageChanged?.Invoke(this, System.EventArgs.Empty);
        }

        private void MakePic_Click(object sender, System.EventArgs e)
        {
            if (_Bitmap != null)
            {
                if (MessageBox.Show("Vorhandenes Bild überschreiben?", enImageCode.Warnung, "Ja", "Nein") != 0) { return; }
            }


            _Bitmap = ScreenShot.GrabArea(ParentForm(), _MaxSize, _MaxSize).Pic;
            SorceType = enSorceType.ScreenShot;
            SorceName = string.Empty;

            ZoomFitInvalidateAndCheckButtons();

            OnImageChanged();

        }

        private void DelP_Click(object sender, System.EventArgs e)
        {
            if (MessageBox.Show("Bild wirklich löschen?", enImageCode.Warnung, "Ja", "Nein") != 0) { return; }
            Clear();
        }

        private void Lade_Click(object sender, System.EventArgs e)
        {
            if (_Bitmap != null)
            {
                if (MessageBox.Show("Vorhandenes Bild überschreiben?", enImageCode.Warnung, "Ja", "Nein") != 0) { return; }
            }
            OpenDia.ShowDialog();
        }

        private void OpenDia_FileOk(object sender, CancelEventArgs e)
        {
            FromFile(OpenDia.FileName);
        }

        public void FromFile(string Filename)
        {

            if (!FileExists(Filename))
            {
                Clear();
                return;
            }

            var ix = (Bitmap)modAllgemein.Image_FromFile(Filename);

            var i = ix.Image_Clone();

            if (_MaxSize > 0)
            {
                _Bitmap = i.Resize(_MaxSize, _MaxSize, enSizeModes.Breite_oder_Höhe_Anpassen_OhneVergrößern, InterpolationMode.HighQualityBicubic, true);
                SorceType = enSorceType.LoadedFromDiskAndResized;
                SorceName = Filename;
            }
            else
            {
                _Bitmap = i;
                SorceType = enSorceType.LoadedFromDisk;
                SorceName = Filename;
            }

            ZoomFitInvalidateAndCheckButtons();

            OnImageChanged();
        }

        public void SetBitmap(Bitmap BMP)
        {
            _Bitmap = BMP;
            SorceType = enSorceType.SetedByProperty;
            SorceName = string.Empty;
            ZoomFitInvalidateAndCheckButtons();
            OnImageChanged();
        }

        private void ZoomFitInvalidateAndCheckButtons()
        {
            _Richt = -1;
            _PanelMover.Enabled = true;

            if (_Bitmap == null)
            {
                DelP.Enabled = false;

                Invalidate();
                return;
            }
            DelP.Enabled = true;
            Invalidate();
        }


        public void Clear()
        {
            if (_Bitmap != null || SorceType != enSorceType.Nichts || !string.IsNullOrEmpty(SorceName))
            {
                _Bitmap = null;
                SorceType = enSorceType.Nichts;
                SorceName = string.Empty;
                ZoomFitInvalidateAndCheckButtons();
                OnImageChanged();
            }
        }


        protected override void InitializeSkin()
        {

        }

        protected override void DrawControl(Graphics GR, enStates vState)
        {
            if (Convert.ToBoolean(vState & enStates.Standard_MouseOver)) { vState = vState ^ enStates.Standard_MouseOver; }
            if (Convert.ToBoolean(vState & enStates.Standard_MousePressed)) { vState = vState ^ enStates.Standard_MousePressed; }


            Skin.Draw_Back(GR, enDesign.EasyPic, vState, DisplayRectangle, this, true);




            if (_Bitmap != null)
            {
                GR.DrawImageInRectAspectRatio(_Bitmap, 1, 1, Width - 2, Height - 2);

            }

            Skin.Draw_Border(GR, enDesign.EasyPic, vState, DisplayRectangle);

        }


        protected override void OnEnabledChanged(System.EventArgs e)
        {
            base.OnEnabledChanged(e);

            if (!Enabled)
            {
                EditPanelFrame.Visible = false;
                _PanelMover.Enabled = false;
                _Richt = 0;
            }
        }

        protected override void OnMouseEnter(System.EventArgs e)
        {
            base.OnMouseEnter(e);

            var ed = new DatabaseGiveBackEventArgs();
            ed.Database = null;
            OnConnectedDatabase(ed);


            AusDatenbank.Enabled = ed.Database != null;

            _Richt = 1;
            _PanelMover.Enabled = true;
        }


        /// <summary>
        /// Ändert die Herkunft ab. 
        /// </summary>
        /// <param name="SorceName"></param>
        /// <param name="SorceType"></param>
        internal void ChangeSource(string SorceName, enSorceType SorceType, bool ThrowEvent)
        {
            this.SorceName = SorceName;
            this.SorceType = SorceType;
            if (ThrowEvent) { OnImageChanged(); }
        }

        private void OnConnectedDatabase(DatabaseGiveBackEventArgs e)
        {
            ConnectedDatabase?.Invoke(this, e);
        }

        protected override void OnMouseLeave(System.EventArgs e)
        {
            base.OnMouseLeave(e);
            _PanelMover.Enabled = true;
        }


        protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                ContextMenu_Show(this, e);
            }
        }

        private void ContextMenuItemClickedInternalProcessig(object sender, ContextMenuItemClickedEventArgs e)
        {

            FloatingInputBoxListBoxStyle.Close(this);

            switch (e.ClickedComand.Internal())
            {
                case "ExF":
                    var epv = new PictureView(_Bitmap);
                    epv.Show();
                    return;

                case "Speichern":
                    var SavOrt = new System.Windows.Forms.FolderBrowserDialog();
                    SavOrt.ShowDialog();

                    if (!PathExists(SavOrt.SelectedPath))
                    {
                        MessageBox.Show("Abbruch!", enImageCode.Warnung, "OK");
                        return;
                    }

                    var NDT = TempFile(SavOrt.SelectedPath + "\\Bild.png");

                    _Bitmap.Save(NDT, ImageFormat.Png);

                    modAllgemein.ExecuteFile(NDT);

                    return;

                case "abbruch":
                    break;

                default:
                    OnContextMenuItemClicked(e);
                    break;
            }
        }

        private void OnContextMenuItemClicked(ContextMenuItemClickedEventArgs e)
        {
            ContextMenuItemClicked?.Invoke(this, e);
        }

        private void EditPanel_Tick(object sender, System.EventArgs e)
        {

            if (_Richt == 0)
            {
                if (!EditPanelFrame.Visible)
                {
                    _PanelMover.Enabled = false;
                    return;
                }
            }

            if (_Richt >= 0)
            {
                if (!ContainsMouse()) { _Richt = -1; }
            }


            if (_Richt > 0)
            {
                if (!EditPanelFrame.Visible)
                {
                    EditPanelFrame.Top = -EditPanelFrame.Height;
                    EditPanelFrame.Visible = true;
                    return;
                }

                if (EditPanelFrame.Top >= 0)
                {
                    EditPanelFrame.Top = 0;
                    _Richt = 0;
                    return;
                }

                EditPanelFrame.Top += 4;
                return;
            }

            if (_Richt < 0)
            {
                if (EditPanelFrame.Top < -EditPanelFrame.Height)
                {
                    EditPanelFrame.Visible = false;
                    _Richt = 0;
                    return;
                }

                EditPanelFrame.Top -= 4;
            }


        }


        protected override void OnResize(System.EventArgs e)
        {
            base.OnResize(e);
            ZoomFitInvalidateAndCheckButtons();
        }

        private void OnContextMenuInit(ContextMenuInitEventArgs e)
        {
            ContextMenuInit?.Invoke(this, e);
        }


        ///// <summary>
        ///// Lädt das Bild aus einer Datenbank verknüpfung. Events werden ausgelöst.
        ///// </summary>
        ///// <param name="FullFileName"></param>
        //public void LoadFromDatabase(string FullFileName)
        //{

        //    _Bitmap = (Bitmap)modAllgemein.Image_FromFile(SorceName);
        //    SorceType = enSorceType.DatabaseInternal;
        //    SorceName = FullFileName.FileNameWithSuffix();
        //    OnImageChanged();
        //}

        ///// <summary>
        ///// Lädt das Bild nach. Der SourceName muss korrekt sein und der Typ muss SourceNameCorrectButImageNotLoaded sein.
        ///// </summary>
        ///// <param name="Column"></param>
        //public void LoadFromDatabase(ColumnItem Column, bool ThrowEvent)
        //{
        //    if (SorceType != enSorceType.SourceNameCorrectButImageNotLoaded) { Develop.DebugPrint_NichtImplementiert(); }


        //    if (string.IsNullOrEmpty(SorceName))
        //    {
        //        SorceType = enSorceType.Nichts;
        //        _Bitmap = null;
        //        if (ThrowEvent) { OnImageChanged(); }
        //        return;
        //    }


        //    var Filename = Column.BestFile(SorceName);

        //    _Bitmap = (Bitmap)modAllgemein.Image_FromFile(Filename);


        //    if (_Bitmap != null)
        //    {
        //        SorceType = enSorceType.DatabaseInternal;
        //    }
        //    else
        //    {
        //        SorceType = enSorceType.EntryWithoutPic;
        //    }

        //    ZoomFitInvalidateAndCheckButtons();
        //    if (ThrowEvent) { OnImageChanged(); }

        //}

        private void AusDatenbank_Click(object sender, System.EventArgs e)
        {

            var ed = new DatabaseGiveBackEventArgs();
            ed.Database = null;
            OnConnectedDatabase(ed);
            if (ed.Database == null) { return; }

            if (_Bitmap != null)
            {
                if (MessageBox.Show("Vorhandenes Bild überschreiben?", enImageCode.Warnung, "Ja", "Nein") != 0) { return; }
            }


            string n;

            var lLCase = ed.Database.AllConnectedFilesLCase();


            using (var x = new ItemSelect())
            {
                n = x.SelectOne_OfDataSystem(lLCase, ed.Database.FileEncryptionKey);
            }

            if (string.IsNullOrEmpty(n)) { return; }

            FromFile(n);
            OnImageChanged();

        }


    }
}

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.DialogBoxes;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueControls.ItemCollection.ItemCollectionList;
using BlueDatabase;
using BlueDatabase.EventArgs;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using static BlueBasics.FileOperations;

namespace BlueControls.Controls
{
    [DefaultEvent("ImageChanged")]
    public sealed partial class EasyPic : IContextMenu, IBackgroundNone
    {

        public enum enSorceType
        {

            Nichts = 0,

            DatabaseInternal = 1,
            Downloaded = 2,
            SetedByProperty = 3,
            ScreenShot = 4,

            LoadedFromDiskAndResized = 5,
            LoadedFromDisk = 6,

            EntryWithoutPic = 7,

            SourceNameCorrectButImageNotLoaded = 8

        }


        private int _MaxSize = -1;


        private int _Richt;

        private int _X;
        private int _Y;
        private float _Zoom = 1;

        public EasyPic()
        {

            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();

            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
            SetNotFocusable();
        }


        //Friend Overrides Sub PrepareForShowing()
        //    '    Stop
        //End Sub

        //Public Property EditBar() As Boolean
        //    Get
        //        Return vEditBar
        //    End Get
        //    Set(Value As Boolean)
        //        vEditBar = Value
        //        EditPanelFrame.Visible = vEditBar
        //        ZoomFitInvalidateAndCheckButtons()
        //    End Set
        //End Property
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


        public enSorceType SorceType { get; private set; }

        public string SorceName { get; private set; }

        [DefaultValue((Bitmap)null)]
        public Bitmap Bitmap
        {
            get;
            private set;
            //set
            //{
            //    if (value == null)
            //    {
            //        DeleteImage();
            //        return;
            //    }

            //    _Bitmap = value;


            //    SorceType = enSorceType.SetedByProperty;
            //    SorceName = "";
            //    ZoomFitInvalidateAndCheckButtons();
            //    OnImageChanged();

            //}
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


        public event EventHandler<ContextMenuInitEventArgs> ContextMenuInit;
        public event EventHandler<ContextMenuItemClickedEventArgs> ContextMenuItemClicked;


        public void ContextMenu_Show(object sender, System.Windows.Forms.MouseEventArgs e)
        {

            var ThisContextMenu = new ItemCollectionList(enBlueListBoxAppearance.KontextMenu);

            if (Bitmap != null)
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

                var _ContextMenu = FloatingInputBoxListBoxStyle.Show(ThisContextMenu, null, this);
                _ContextMenu.ItemClicked += ContextMenuItemClickedInternalProcessig;
            }

        }




        public event EventHandler ImageChanged;
        public event EventHandler<DatabaseGiveBackEventArgs> ConnectedDatabase;


        private void OnImageChanged()
        {
            ImageChanged?.Invoke(this, System.EventArgs.Empty);
        }

        private void MakePic_Click(object sender, System.EventArgs e)
        {
            if (Bitmap != null)
            {
                if (MessageBox.Show("Vorhandenes Bild überschreiben?", enImageCode.Warnung, "Ja", "Nein") != 0) { return; }
            }


            Bitmap = ScreenShot.GrabArea(ParentForm(), _MaxSize, _MaxSize).Pic;
            //  CameFrom = "Screenshot"

            SorceType = enSorceType.ScreenShot;
            SorceName = "";

            ZoomFitInvalidateAndCheckButtons();

            OnImageChanged();

        }

        private void DelP_Click(object sender, System.EventArgs e)
        {
            if (MessageBox.Show("Bild wirklich löschen?", enImageCode.Warnung, "Ja", "Nein") != 0) { return; }
            DeleteImage();
        }

        private void Lade_Click(object sender, System.EventArgs e)
        {
            if (Bitmap != null)
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
                DeleteImage();
                return;
            }

            var ix = (Bitmap)modAllgemein.Image_FromFile(Filename);

            var i = ix.Image_Clone();

            if (_MaxSize > 0)
            {
                Bitmap = i.Resize(_MaxSize, _MaxSize, enSizeModes.Breite_oder_Höhe_Anpassen_OhneVergrößern, InterpolationMode.HighQualityBicubic, true);
                // CameFrom = "LoadedAndResized;" & Filename
                SorceType = enSorceType.LoadedFromDiskAndResized;
                SorceName = Filename;
            }
            else
            {
                Bitmap = i;
                SorceType = enSorceType.LoadedFromDisk;
                SorceName = Filename;
                //CameFrom = "Loaded;" & Filename
            }

            ZoomFitInvalidateAndCheckButtons();

            OnImageChanged();
        }

        internal void SetBitmap(Bitmap BMP)
        {
            Bitmap = BMP;
            SorceType = enSorceType.SetedByProperty;
            SorceName = string.Empty;
        }

        private void ZoomFitInvalidateAndCheckButtons()
        {
            _Richt = -1;
            _PanelMover.Enabled = true;

            if (Bitmap == null)
            {
                _Zoom = 1;
                DelP.Enabled = false;

                Invalidate();
                return;
            }

            _Zoom = (float)Math.Min(Width / (double)Bitmap.Width, Height / (double)Bitmap.Height);
            DelP.Enabled = true;

            Invalidate();
        }


        public void DeleteImage()
        {
            if (Bitmap != null || SorceType != enSorceType.Nichts || !string.IsNullOrEmpty(SorceName))
            {
                Bitmap = null;
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

            if (Bitmap != null)
            {
                var DW = Convert.ToInt32(Math.Truncate(Bitmap.Width * _Zoom));
                var DH = Convert.ToInt32(Math.Truncate(Bitmap.Height * _Zoom));

                if (DW <= Width)
                {
                    _X = Convert.ToInt32(Math.Truncate((Width - DW) / 2.0));
                }
                else
                {
                    if (_X > 0) { _X = 0; }
                    if (_X < Width - DW) { _X = Width - DW; }
                }

                if (DH <= Height)
                {
                    _Y = Convert.ToInt32(Math.Truncate((Height - DH) / 2.0));
                }
                else
                {
                    if (_Y > 0) { _Y = 0; }
                    if (_Y < Height - DH) { _Y = Height - DH; }
                }
                GR.DrawImage(Bitmap, _X, _Y, DW, DH);
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
        /// Ändert die Herkunft ab. Es wird kein Event ausgelöst.
        /// </summary>
        /// <param name="SorceName"></param>
        /// <param name="SorceType"></param>
        internal void ChangeSource(string SorceName, enSorceType SorceType)
        {
            this.SorceName = SorceName;
            this.SorceType = SorceType;
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
                    var epv = new PictureView(Bitmap);
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

                    Bitmap.Save(NDT, ImageFormat.Png);

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


        /// <summary>
        /// Lädt das Bild aus einer Datenbank verknüpfung. Events werden ausgelöst.
        /// </summary>
        /// <param name="FullFileName"></param>
        public void LoadFromDatabase(string FullFileName)
        {

            Bitmap = (Bitmap)modAllgemein.Image_FromFile(SorceName);
            SorceType = enSorceType.DatabaseInternal;
            SorceName = FullFileName.FileNameWithSuffix();
            OnImageChanged();
        }

        /// <summary>
        /// Lädt das Bild nach. Der SourceName muss korrekt sein und der Typ muss SourceNameCorrectButImageNotLoaded sein. Es wird kein Event ausgelöst.
        /// </summary>
        /// <param name="Column"></param>
        public void LoadFromDatabase(ColumnItem Column)
        {
            if (SorceType != enSorceType.SourceNameCorrectButImageNotLoaded) { Develop.DebugPrint_NichtImplementiert(); }


            if (string.IsNullOrEmpty(SorceName))
            {
                SorceType = enSorceType.Nichts;
                Bitmap = null;
                return;
            }


            var Filename = Column.BestFile(SorceName);

            Bitmap = (Bitmap)modAllgemein.Image_FromFile(Filename);


            if (Bitmap != null)
            {
                SorceType = enSorceType.DatabaseInternal;
            }
            else
            {
                SorceType = enSorceType.EntryWithoutPic;
            }

            ZoomFitInvalidateAndCheckButtons();

        }

        private void AusDatenbank_Click(object sender, System.EventArgs e)
        {

            var ed = new DatabaseGiveBackEventArgs();
            ed.Database = null;
            OnConnectedDatabase(ed);
            if (ed.Database == null) { return; }

            if (Bitmap != null)
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

            LoadFromDatabase(n);

        }

        /// <summary>
        /// Entfernt das Bild, setzt den SourceName und markiert das Bild als "SourceNameCorrectButImageNotLoaded". ImageChanged wird ebenfalls ausgelöst.
        /// </summary>
        /// <param name="SourceName"></param>
        internal void SetSourceName(string SourceName)
        {
            SorceName = SourceName;
            SorceType = enSorceType.SourceNameCorrectButImageNotLoaded;
            Bitmap = null;
            ZoomFitInvalidateAndCheckButtons();
            OnImageChanged();

        }
    }
}

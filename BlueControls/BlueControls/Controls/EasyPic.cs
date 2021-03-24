#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2020 Christian Peter
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
using BlueBasics.EventArgs;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueControls.ItemCollection;
using BlueDatabase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using static BlueBasics.Extensions;
using static BlueBasics.FileOperations;

namespace BlueControls.Controls {
    [Designer(typeof(BasicDesigner))]
    [DefaultEvent("ImageChanged")]
    public sealed partial class EasyPic : GenericControl, IContextMenu, IBackgroundNone {

        #region Constructor
        public EasyPic() : base(false, false) {

            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();

            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
            SetNotFocusable();
        }

        #endregion

        private Bitmap _Bitmap = null;
        private int _MaxSize = -1;


        private int _Richt;




        public event EventHandler<ContextMenuInitEventArgs> ContextMenuInit;
        public event EventHandler<ContextMenuItemClickedEventArgs> ContextMenuItemClicked;
        public event EventHandler ImageChanged;
        public event EventHandler<MultiUserFileGiveBackEventArgs> ConnectedDatabase;


        [DefaultValue(-1)]
        public int MaxSize {
            get => _MaxSize;
            set {
                if (value < 1) { value = -1; }
                _MaxSize = value;
            }
        }


        [DefaultValue(enSorceType.Nichts)]
        public enSorceType SorceType { get; private set; }

        [DefaultValue("")]
        public string SorceName { get; private set; }

        [DefaultValue((Bitmap)null)]
        public Bitmap Bitmap {
            get {
                return _Bitmap;
            }
            private set {
                if (_Bitmap == null && value == null) { return; }
                _Bitmap = value;
                ZoomFitInvalidateAndCheckButtons();
            }
        }


        [DefaultValue(0)]
        public new int TabIndex {
            get {
                return 0;
            }

            set {
                base.TabIndex = 0;
            }
        }
        [DefaultValue(false)]
        public new bool TabStop {
            get {
                return false;
            }
            set {
                base.TabStop = false;
            }
        }




        public void GetContextMenuItems(System.Windows.Forms.MouseEventArgs e, ItemCollectionList Items, out object HotItem, List<string> Tags, ref bool Cancel, ref bool Translate) {

            HotItem = null;

            if (_Bitmap != null) {
                Items.Add("Externes Fenster öffnen", "ExF");
                Items.Add(enContextMenuComands.Speichern);
            }


        }







        private void OnImageChanged() {
            ImageChanged?.Invoke(this, System.EventArgs.Empty);
        }

        private void MakePic_Click(object sender, System.EventArgs e) {
            if (_Bitmap != null) {
                if (MessageBox.Show("Vorhandenes Bild überschreiben?", enImageCode.Warnung, "Ja", "Nein") != 0) { return; }
            }


            _Bitmap = ScreenShot.GrabArea(ParentForm(), _MaxSize, _MaxSize).Pic;
            SorceType = enSorceType.ScreenShot;
            SorceName = string.Empty;

            ZoomFitInvalidateAndCheckButtons();

            OnImageChanged();

        }

        private void DelP_Click(object sender, System.EventArgs e) {
            if (MessageBox.Show("Bild wirklich löschen?", enImageCode.Warnung, "Ja", "Nein") != 0) { return; }
            Clear();
        }

        private void Lade_Click(object sender, System.EventArgs e) {
            if (_Bitmap != null) {
                if (MessageBox.Show("Vorhandenes Bild überschreiben?", enImageCode.Warnung, "Ja", "Nein") != 0) { return; }
            }
            OpenDia.ShowDialog();
        }

        private void OpenDia_FileOk(object sender, CancelEventArgs e) {
            FromFile(OpenDia.FileName);
        }







        private void ZoomFitInvalidateAndCheckButtons() {
            _Richt = -1;
            _PanelMover.Enabled = true;

            if (_Bitmap == null) {
                DelP.Enabled = false;

                Invalidate();
                return;
            }
            DelP.Enabled = true;
            Invalidate();
        }





        #region " 3er Modifikatoren mit Eventauslösung "


        public void FromFile(string Filename) {

            if (!FileExists(Filename)) {
                //Develop.DebugPrint(enFehlerArt.Fehler, "Datei Existiert nicht: " + Filename);
                Clear();
                return;
            }

            var ix = (Bitmap)BitmapExt.Image_FromFile(Filename);

            var i = ix.Image_Clone();

            if (_MaxSize > 0) {
                _Bitmap = BitmapExt.Resize(i, _MaxSize, _MaxSize, enSizeModes.Breite_oder_Höhe_Anpassen_OhneVergrößern, InterpolationMode.HighQualityBicubic, true);
                SorceType = enSorceType.LoadedFromDiskAndResized;
                SorceName = Filename;
            } else {
                _Bitmap = i;
                SorceType = enSorceType.LoadedFromDisk;
                SorceName = Filename;
            }

            ZoomFitInvalidateAndCheckButtons();
            OnImageChanged();
        }

        public void SetBitmap(Bitmap BMP) {
            _Bitmap = BMP;
            SorceType = enSorceType.SetedByProperty;
            SorceName = string.Empty;
            ZoomFitInvalidateAndCheckButtons();
            OnImageChanged();
        }



        public void Clear() {
            if (_Bitmap != null || SorceType != enSorceType.Nichts || !string.IsNullOrEmpty(SorceName)) {
                _Bitmap = null;
                SorceType = enSorceType.Nichts;
                SorceName = string.Empty;
                ZoomFitInvalidateAndCheckButtons();
                OnImageChanged();
            }
        }
        #endregion


        protected override void DrawControl(Graphics GR, enStates vState) {
            if (Convert.ToBoolean(vState & enStates.Standard_MouseOver)) { vState ^= enStates.Standard_MouseOver; }
            if (Convert.ToBoolean(vState & enStates.Standard_MousePressed)) { vState ^= enStates.Standard_MousePressed; }


            Skin.Draw_Back(GR, enDesign.EasyPic, vState, DisplayRectangle, this, true);




            if (_Bitmap != null) {
                GR.DrawImageInRectAspectRatio(_Bitmap, 1, 1, Width - 2, Height - 2);

            }

            Skin.Draw_Border(GR, enDesign.EasyPic, vState, DisplayRectangle);

        }


        protected override void OnEnabledChanged(System.EventArgs e) {
            base.OnEnabledChanged(e);

            if (!Enabled) {
                EditPanelFrame.Visible = false;
                _PanelMover.Enabled = false;
                _Richt = 0;
            }
        }

        protected override void OnMouseEnter(System.EventArgs e) {
            base.OnMouseEnter(e);

            var ed = new MultiUserFileGiveBackEventArgs {
                File = null
            };
            OnConnectedDatabase(ed);


            AusDatenbank.Enabled = ed.File != null;

            _Richt = 1;
            _PanelMover.Enabled = true;
        }


        /// <summary>
        /// Ändert die Herkunft ab. 
        /// </summary>
        /// <param name="SorceName"></param>
        /// <param name="SorceType"></param>
        internal void ChangeSource(string SorceName, enSorceType SorceType, bool ThrowEvent) {
            this.SorceName = SorceName;
            this.SorceType = SorceType;
            if (ThrowEvent) { OnImageChanged(); }
        }

        private void OnConnectedDatabase(MultiUserFileGiveBackEventArgs e) {
            ConnectedDatabase?.Invoke(this, e);
        }

        protected override void OnMouseLeave(System.EventArgs e) {
            base.OnMouseLeave(e);
            _PanelMover.Enabled = true;
        }


        protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e) {
            base.OnMouseUp(e);
            if (e.Button == System.Windows.Forms.MouseButtons.Right) {
                FloatingInputBoxListBoxStyle.ContextMenuShow(this, e);
            }
        }

        public bool ContextMenuItemClickedInternalProcessig(object sender, ContextMenuItemClickedEventArgs e) {



            switch (e.ClickedComand) {
                case "ExF":
                    var epv = new PictureView(_Bitmap);
                    epv.Show();
                    return true;

                case "Speichern":
                    var SavOrt = new System.Windows.Forms.FolderBrowserDialog();
                    SavOrt.ShowDialog();

                    if (!PathExists(SavOrt.SelectedPath)) {
                        MessageBox.Show("Abbruch!", enImageCode.Warnung, "OK");
                        return true;
                    }

                    var NDT = TempFile(SavOrt.SelectedPath + "\\Bild.png");

                    _Bitmap.Save(NDT, ImageFormat.Png);

                    ExecuteFile(NDT);
                    return true;

            }

            return false;
        }

        public void OnContextMenuItemClicked(ContextMenuItemClickedEventArgs e) {
            ContextMenuItemClicked?.Invoke(this, e);
        }

        private void EditPanel_Tick(object sender, System.EventArgs e) {

            if (_Richt == 0) {
                if (!EditPanelFrame.Visible) {
                    _PanelMover.Enabled = false;
                    return;
                }
            }

            if (_Richt >= 0) {
                if (!ContainsMouse()) { _Richt = -1; }
            }


            if (_Richt > 0) {
                if (!EditPanelFrame.Visible) {
                    EditPanelFrame.Top = -EditPanelFrame.Height;
                    EditPanelFrame.Visible = true;
                    return;
                }

                if (EditPanelFrame.Top >= 0) {
                    EditPanelFrame.Top = 0;
                    _Richt = 0;
                    return;
                }

                EditPanelFrame.Top += 4;
                return;
            }

            if (_Richt < 0) {
                if (EditPanelFrame.Top < -EditPanelFrame.Height) {
                    EditPanelFrame.Visible = false;
                    _Richt = 0;
                    return;
                }

                EditPanelFrame.Top -= 4;
            }


        }


        protected override void OnResize(System.EventArgs e) {
            base.OnResize(e);
            ZoomFitInvalidateAndCheckButtons();
        }

        public void OnContextMenuInit(ContextMenuInitEventArgs e) {
            ContextMenuInit?.Invoke(this, e);
        }


        private void AusDatenbank_Click(object sender, System.EventArgs e) {

            var ed = new MultiUserFileGiveBackEventArgs {
                File = null
            };
            OnConnectedDatabase(ed);
            if (ed.File is Database DB) {
                if (_Bitmap != null) {
                    if (MessageBox.Show("Vorhandenes Bild überschreiben?", enImageCode.Warnung, "Ja", "Nein") != 0) { return; }
                }


                string n;

                var lLCase = DB.AllConnectedFilesLCase();


                using (var x = new ItemSelect()) {
                    n = x.SelectOne_OfDataSystem(lLCase, DB.FileEncryptionKey);
                }

                if (string.IsNullOrEmpty(n)) { return; }

                FromFile(n);
                OnImageChanged();
            }

        }


    }
}

// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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

#nullable enable

using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.EventArgs;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueDatabase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using BlueControls.ItemCollection.ItemCollectionList;
using static BlueBasics.BitmapExt;
using static BlueBasics.FileOperations;
using MessageBox = BlueControls.Forms.MessageBox;

namespace BlueControls.Controls {

    [Designer(typeof(BasicDesigner))]
    [DefaultEvent("ImageChanged")]
    public sealed partial class EasyPic : GenericControl, IContextMenu, IBackgroundNone {

        #region Fields

        private Bitmap? _bitmap;

        private int _maxSize = -1;

        private int _richt;

        #endregion

        #region Constructors

        public EasyPic() : base(false, false) {
            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();
            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
            SetNotFocusable();
        }

        #endregion

        #region Events

        public event EventHandler<MultiUserFileGiveBackEventArgs> ConnectedDatabase;

        public event EventHandler<ContextMenuInitEventArgs> ContextMenuInit;

        public event EventHandler<ContextMenuItemClickedEventArgs> ContextMenuItemClicked;

        public event EventHandler ImageChanged;

        #endregion

        #region Properties

        [DefaultValue((Bitmap?)null)]
        public Bitmap? Bitmap {
            get => _bitmap;
            private set {
                if (_bitmap == null && value == null) { return; }
                _bitmap = value;
                ZoomFitInvalidateAndCheckButtons();
            }
        }

        [DefaultValue(-1)]
        public int MaxSize {
            get => _maxSize;
            set {
                if (value < 1) { value = -1; }
                _maxSize = value;
            }
        }

        [DefaultValue("")]
        public string SorceName { get; private set; }

        [DefaultValue(SorceType.Nichts)]
        public SorceType SorceType { get; private set; }

        [DefaultValue(0)]
        public new int TabIndex {
            get => 0;
            set => base.TabIndex = 0;
        }

        [DefaultValue(false)]
        public new bool TabStop {
            get => false;
            set => base.TabStop = false;
        }

        #endregion

        #region Methods

        public void Clear() {
            if (_bitmap != null || SorceType != SorceType.Nichts || !string.IsNullOrEmpty(SorceName)) {
                _bitmap = null;
                SorceType = SorceType.Nichts;
                SorceName = string.Empty;
                ZoomFitInvalidateAndCheckButtons();
                OnImageChanged();
            }
        }

        public bool ContextMenuItemClickedInternalProcessig(object sender, ContextMenuItemClickedEventArgs e) {
            switch (e.ClickedComand) {
                case "ExF":
                    PictureView epv = new(_bitmap);
                    epv.Show();
                    return true;

                case "Speichern":
                    System.Windows.Forms.FolderBrowserDialog savOrt = new();
                    savOrt.ShowDialog();
                    if (!PathExists(savOrt.SelectedPath)) {
                        MessageBox.Show("Abbruch!", ImageCode.Warnung, "OK");
                        return true;
                    }
                    var ndt = TempFile(savOrt.SelectedPath + "\\Bild.png");
                    _bitmap.Save(ndt, ImageFormat.Png);
                    ExecuteFile(ndt);
                    return true;
            }
            return false;
        }

        public void FromFile(string filename) {
            if (!FileExists(filename)) {
                //Develop.DebugPrint(enFehlerArt.Fehler, "Datei Existiert nicht: " + Filename);
                Clear();
                return;
            }
            var ix = (Bitmap)Image_FromFile(filename);
            var i = Image_Clone(ix);
            if (_maxSize > 0) {
                _bitmap = BitmapExt.Resize(i, _maxSize, _maxSize, SizeModes.Breite_oder_Höhe_Anpassen_OhneVergrößern, InterpolationMode.HighQualityBicubic, true);
                SorceType = SorceType.LoadedFromDiskAndResized;
                SorceName = filename;
            } else {
                _bitmap = i;
                SorceType = SorceType.LoadedFromDisk;
                SorceName = filename;
            }
            ZoomFitInvalidateAndCheckButtons();
            OnImageChanged();
        }

        public void GetContextMenuItems(System.Windows.Forms.MouseEventArgs? e, ItemCollectionList? items, out object? hotItem, List<string> tags, ref bool cancel, ref bool translate) {
            hotItem = null;
            if (_bitmap != null) {
                items.Add("Externes Fenster öffnen", "ExF");
                items.Add(ContextMenuComands.Speichern);
            }
        }

        public void OnContextMenuInit(ContextMenuInitEventArgs e) => ContextMenuInit?.Invoke(this, e);

        public void OnContextMenuItemClicked(ContextMenuItemClickedEventArgs e) => ContextMenuItemClicked?.Invoke(this, e);

        public void SetBitmap(Bitmap? bmp) {
            _bitmap = bmp;
            SorceType = SorceType.SetedByProperty;
            SorceName = string.Empty;
            ZoomFitInvalidateAndCheckButtons();
            OnImageChanged();
        }

        /// <summary>
        /// Ändert die Herkunft ab.
        /// </summary>
        /// <param name="sorceName"></param>
        /// <param name="sorceType"></param>
        /// <param name="throwEvent"></param>
        internal void ChangeSource(string sorceName, SorceType sorceType, bool throwEvent) {
            SorceName = sorceName;
            SorceType = sorceType;
            if (throwEvent) { OnImageChanged(); }
        }

        protected override void DrawControl(Graphics gr, States vState) {
            if (Convert.ToBoolean(vState & States.Standard_MouseOver)) { vState ^= States.Standard_MouseOver; }
            if (Convert.ToBoolean(vState & States.Standard_MousePressed)) { vState ^= States.Standard_MousePressed; }
            Skin.Draw_Back(gr, Design.EasyPic, vState, DisplayRectangle, this, true);
            if (_bitmap != null) {
                gr.DrawImageInRectAspectRatio(_bitmap, 1, 1, Width - 2, Height - 2);
            }
            Skin.Draw_Border(gr, Design.EasyPic, vState, DisplayRectangle);
        }

        protected override void OnEnabledChanged(System.EventArgs e) {
            base.OnEnabledChanged(e);
            if (!Enabled) {
                EditPanelFrame.Visible = false;
                _PanelMover.Enabled = false;
                _richt = 0;
            }
        }

        protected override void OnMouseEnter(System.EventArgs e) {
            base.OnMouseEnter(e);
            MultiUserFileGiveBackEventArgs ed = new() {
                File = null
            };
            OnConnectedDatabase(ed);
            AusDatenbank.Enabled = ed.File != null;
            _richt = 1;
            _PanelMover.Enabled = true;
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

        protected override void OnResize(System.EventArgs e) {
            base.OnResize(e);
            ZoomFitInvalidateAndCheckButtons();
        }

        private void AusDatenbank_Click(object sender, System.EventArgs e) {
            MultiUserFileGiveBackEventArgs ed = new() {
                File = null
            };
            OnConnectedDatabase(ed);
            if (ed.File is Database db) {
                if (_bitmap != null) {
                    if (MessageBox.Show("Vorhandenes Bild überschreiben?", ImageCode.Warnung, "Ja", "Nein") != 0) { return; }
                }

                var n = ItemSelect.Show(db.AllConnectedFilesLCase(), db.FileEncryptionKey);

                if (string.IsNullOrEmpty(n)) { return; }
                FromFile(n);
                OnImageChanged();
            }
        }

        private void DelP_Click(object sender, System.EventArgs e) {
            if (MessageBox.Show("Bild wirklich löschen?", ImageCode.Warnung, "Ja", "Nein") != 0) { return; }
            Clear();
        }

        private void EditPanel_Tick(object sender, System.EventArgs e) {
            if (_richt == 0) {
                if (!EditPanelFrame.Visible) {
                    _PanelMover.Enabled = false;
                    return;
                }
            }
            if (_richt >= 0) {
                if (!ContainsMouse()) { _richt = -1; }
            }
            if (_richt > 0) {
                if (!EditPanelFrame.Visible) {
                    EditPanelFrame.Top = -EditPanelFrame.Height;
                    EditPanelFrame.Visible = true;
                    return;
                }
                if (EditPanelFrame.Top >= 0) {
                    EditPanelFrame.Top = 0;
                    _richt = 0;
                    return;
                }
                EditPanelFrame.Top += 4;
                return;
            }
            if (_richt < 0) {
                if (EditPanelFrame.Top < -EditPanelFrame.Height) {
                    EditPanelFrame.Visible = false;
                    _richt = 0;
                    return;
                }
                EditPanelFrame.Top -= 4;
            }
        }

        private void Lade_Click(object sender, System.EventArgs e) {
            if (_bitmap != null) {
                if (MessageBox.Show("Vorhandenes Bild überschreiben?", ImageCode.Warnung, "Ja", "Nein") != 0) { return; }
            }
            OpenDia.ShowDialog();
        }

        private void MakePic_Click(object sender, System.EventArgs e) {
            if (_bitmap != null) {
                if (MessageBox.Show("Vorhandenes Bild überschreiben?", ImageCode.Warnung, "Ja", "Nein") != 0) { return; }
            }
            _bitmap = ScreenShot.GrabArea(ParentForm());
            SorceType = SorceType.ScreenShot;
            SorceName = string.Empty;
            ZoomFitInvalidateAndCheckButtons();
            OnImageChanged();
        }

        private void OnConnectedDatabase(MultiUserFileGiveBackEventArgs e) => ConnectedDatabase?.Invoke(this, e);

        private void OnImageChanged() => ImageChanged?.Invoke(this, System.EventArgs.Empty);

        private void OpenDia_FileOk(object sender, CancelEventArgs e) => FromFile(OpenDia.FileName);

        private void ZoomFitInvalidateAndCheckButtons() {
            _richt = -1;
            _PanelMover.Enabled = true;
            if (_bitmap == null) {
                DelP.Enabled = false;
                Invalidate();
                return;
            }
            DelP.Enabled = true;
            Invalidate();
        }

        #endregion
    }
}
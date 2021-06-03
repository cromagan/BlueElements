#region BlueElements - a collection of useful tools, database and controls
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
#endregion

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using static BlueBasics.FileOperations;

namespace BlueControls.ItemCollection {
    public class DataListItem : BasicListItem {
        #region  Variablen-Deklarationen 

        private byte[] _bin;
        private string _caption;
        private List<string> _captiontmp = new();
        private int _padding;
        private readonly ListExt<QuickImage> _overlays = new();

        private string _filename;

        private readonly string _EncryptionKey;

        private int _captionlines = 2;

        private const int ConstMY = 15;

        #endregion

        #region  Event-Deklarationen + Delegaten 

        #endregion

        #region  Construktor + Initialize 

        public DataListItem(byte[] b, string internalname, string caption) : base(internalname) {

            _caption = caption;
            _captiontmp.Clear();

            _bin = b;
            _padding = 0;

            _overlays.Clear();
        }

        public DataListItem(string Filename, string internalname, string caption, string encryptionKey) : base(internalname) {

            _caption = caption;
            _captiontmp.Clear();

            _filename = Filename;
            _EncryptionKey = encryptionKey;

            _padding = 0;

            _overlays.Clear();
        }

        #endregion

        #region  Properties 

        public byte[] Bin {
            get {
                GetBin();
                return _bin;
            }
            set {
                _filename = string.Empty;
                _bin = value;
                //OnChanged();
            }
        }

        public string Caption {
            get => _caption;

            set {
                if (_caption == value) { return; }
                _caption = value;
                _captiontmp.Clear();
                //OnChanged();
            }
        }

        public int CaptionLines {
            get => _captionlines;
            set {
                if (value < 1) { value = 1; }

                if (_captionlines == value) { return; }
                _captionlines = value;
                _captiontmp.Clear();
                //OnChanged();
            }
        }

        public int Padding {
            get => _padding;

            set {
                if (_padding == value) { return; }
                _padding = value;
                //OnChanged();
            }
        }

        public List<QuickImage> Overlays => _overlays;

        public override string QuickInfo => string.Empty;

        #endregion

        protected override void DrawExplicit(Graphics GR, Rectangle PositionModified, enDesign itemdesign, enStates vState, bool DrawBorderAndBack, bool Translate) {

            if (DrawBorderAndBack) {
                Skin.Draw_Back(GR, itemdesign, vState, PositionModified, null, false);
            }
            var DCoordinates = PositionModified;
            DCoordinates.Inflate(-_padding, -_padding);
            var ScaledImagePosition = new RectangleF();
            var AreaOfWholeImage = new RectangleF();

            //GetBin();

            if (!string.IsNullOrEmpty(_caption) && _captiontmp.Count == 0) { _captiontmp = _caption.SplitByWidth(DCoordinates.Width, _captionlines, enDesign.Item_Listbox, enStates.Standard); }

            var binim = QuickImage.Get(_filename.FileType(), 48).BMP;

            if (_bin != null) {
                AreaOfWholeImage = new RectangleF(0, 0, binim.Width, binim.Height);
                var scale = (float)Math.Min((DCoordinates.Width - (_padding * 2)) / (double)binim.Width,
                                              (DCoordinates.Height - (_padding * 2) - (_captionlines * ConstMY)) / (double)binim.Height);

                ScaledImagePosition = new RectangleF(((DCoordinates.Width - (binim.Width * scale)) / 2) + DCoordinates.Left,
                                                     ((DCoordinates.Height - (binim.Height * scale)) / 2) + DCoordinates.Top - (_captionlines * ConstMY / 2),
                                                    binim.Width * scale,
                                                    binim.Height * scale);
            }

            var trp = DCoordinates.PointOf(enAlignment.Horizontal_Vertical_Center);

            ScaledImagePosition = new RectangleF(ScaledImagePosition.Left - trp.X, ScaledImagePosition.Top - trp.Y, ScaledImagePosition.Width, ScaledImagePosition.Height);

            GR.TranslateTransform(trp.X, trp.Y);
            if (_bin != null) { GR.DrawImage(binim, ScaledImagePosition, AreaOfWholeImage, GraphicsUnit.Pixel); }

            foreach (var thisQI in Overlays) {
                GR.DrawImage(thisQI.BMP, ScaledImagePosition.Left + 8, ScaledImagePosition.Top + 8);
            }

            if (!string.IsNullOrEmpty(_caption)) {

                var c = _captiontmp.Count;

                var Ausgl = (c - _captionlines) * ConstMY / 2;

                foreach (var ThisCap in _captiontmp) {
                    c--;
                    var s = Skin.FormatedText_NeededSize(ThisCap, null, Skin.GetBlueFont(enDesign.Item_Listbox, vState), 16);
                    var r = new Rectangle((int)(DCoordinates.Left + ((DCoordinates.Width - s.Width) / 2.0)), DCoordinates.Bottom - s.Height - 3, s.Width, s.Height);

                    r.X -= trp.X;
                    r.Y -= trp.Y;

                    r.Y = r.Y - (ConstMY * c) + Ausgl;

                    //r = new Rectangle(r.Left - trp.X, r.Top - trp.Y, r.Width, r.Height);
                    //GenericControl.Skin.Draw_Back(GR, enDesign.Item_Listbox_Unterschrift, vState, r, null, false);
                    //GenericControl.Skin.Draw_Border(GR, enDesign.Item_Listbox_Unterschrift, vState, r);
                    Skin.Draw_FormatedText(GR, ThisCap, enDesign.Item_Listbox, vState, null, enAlignment.Horizontal_Vertical_Center, r, null, false, false);

                }
            }

            GR.TranslateTransform(-trp.X, -trp.Y);
            GR.ResetTransform();

            if (DrawBorderAndBack) {
                Skin.Draw_Border(GR, itemdesign, vState, PositionModified);
            }
        }

        protected override Size ComputeSizeUntouchedForListBox() {
            return new Size(300, 300);
        }

        private void GetBin() {
            if (string.IsNullOrEmpty(_filename)) { return; }
            if (_bin != null) { return; }

            try {
                _bin = modConverter.FileToByte(_filename);
                if (FileExists(_filename)) {
                    if (!string.IsNullOrEmpty(_EncryptionKey)) {

                        _bin = modAllgemein.SimpleCrypt(_bin, _EncryptionKey, -1);
                    }
                }
            } catch (Exception ex) {
                Develop.DebugPrint(ex);
            }
        }

        public override int HeightForListBox(enBlueListBoxAppearance style, int columnWidth) {
            return style switch {
                enBlueListBoxAppearance.FileSystem => 110 + (_captionlines * ConstMY),
                _ => (int)(columnWidth * 0.8),
            };
        }

        protected override string GetCompareKey() {
            return Internal;
        }

        public override bool FilterMatch(string FilterText) {
            return base.FilterMatch(FilterText) || Caption.ToUpper().Contains(FilterText.ToUpper()) || _filename.ToUpper().Contains(FilterText.ToUpper());
        }
    }
}

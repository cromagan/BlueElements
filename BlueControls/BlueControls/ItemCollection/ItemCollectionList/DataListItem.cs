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

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using BlueControls.ItemCollection.ItemCollectionList;
using static BlueBasics.FileOperations;

namespace BlueControls.ItemCollection {

    public class DataListItem : BasicListItem {

        #region Fields

        private const int ConstMy = 15;
        private readonly string _encryptionKey;
        private readonly ListExt<QuickImage> _overlays = new();
        private byte[] _bin;
        private string _caption;
        private int _captionlines = 2;
        private List<string> _captiontmp = new();
        private string _filename;

        #endregion

        #region Constructors

        public DataListItem(byte[] b, string internalname, string caption) : base(internalname, true) {
            _caption = caption;
            _captiontmp.Clear();
            _bin = b;
            Padding = 0;
            _overlays.Clear();
        }

        public DataListItem(string filename, string internalname, string caption, string encryptionKey) : base(internalname, true) {
            _caption = caption;
            _captiontmp.Clear();
            _filename = filename;
            _encryptionKey = encryptionKey;
            Padding = 0;
            _overlays.Clear();
        }

        #endregion

        #region Properties

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

        public List<QuickImage> Overlays => _overlays;

        public int Padding { get; set; }

        public override string QuickInfo => string.Empty;

        #endregion

        #region Methods

        public override object Clone() {
            Develop.DebugPrint_NichtImplementiert();
            return null;
        }

        public override bool FilterMatch(string filterText) => base.FilterMatch(filterText) || Caption.ToUpper().Contains(filterText.ToUpper()) || _filename.ToUpper().Contains(filterText.ToUpper());

        public override int HeightForListBox(BlueListBoxAppearance style, int columnWidth) => style switch {
            BlueListBoxAppearance.FileSystem => 110 + (_captionlines * ConstMy),
            _ => (int)(columnWidth * 0.8)
        };

        protected override Size ComputeSizeUntouchedForListBox() => new(300, 300);

        protected override void DrawExplicit(Graphics gr, Rectangle positionModified, Design itemdesign, States state, bool drawBorderAndBack, bool translate) {
            if (drawBorderAndBack) { Skin.Draw_Back(gr, itemdesign, state, positionModified, null, false); }

            var drawingCoordinates = positionModified;
            drawingCoordinates.Inflate(-Padding, -Padding);
            var scaledImagePosition = RectangleF.Empty;
            var areaOfWholeImage = RectangleF.Empty;
            var bFont = Skin.GetBlueFont(itemdesign, state);

            if (!string.IsNullOrEmpty(_caption) && _captiontmp.Count == 0) { _captiontmp = bFont.SplitByWidth(_caption, drawingCoordinates.Width, _captionlines); }
            var binim = QuickImage.Get(_filename.FileType(), 48);
            if (_bin != null) {
                areaOfWholeImage = new RectangleF(0, 0, binim.Width, binim.Height);
                var scale = (float)Math.Min((drawingCoordinates.Width - (Padding * 2)) / (double)binim.Width,
                                              (drawingCoordinates.Height - (Padding * 2) - (_captionlines * ConstMy)) / (double)binim.Height);
                scaledImagePosition = new RectangleF(((drawingCoordinates.Width - (binim.Width * scale)) / 2) + drawingCoordinates.Left,
                                                     ((drawingCoordinates.Height - (binim.Height * scale)) / 2) + drawingCoordinates.Top - (_captionlines * ConstMy / 2),
                                                    binim.Width * scale,
                                                    binim.Height * scale);
            }
            var trp = drawingCoordinates.PointOf(Alignment.Horizontal_Vertical_Center);
            scaledImagePosition = new RectangleF(scaledImagePosition.Left - trp.X, scaledImagePosition.Top - trp.Y, scaledImagePosition.Width, scaledImagePosition.Height);
            gr.TranslateTransform(trp.X, trp.Y);
            if (_bin != null) { gr.DrawImage(binim, scaledImagePosition, areaOfWholeImage, GraphicsUnit.Pixel); }
            foreach (var thisQi in Overlays) {
                gr.DrawImage(thisQi, scaledImagePosition.Left + 8, scaledImagePosition.Top + 8);
            }
            if (!string.IsNullOrEmpty(_caption)) {
                var c = _captiontmp.Count;
                var ausgl = (c - _captionlines) * ConstMy / 2;
                foreach (var thisCap in _captiontmp) {
                    c--;
                    var s = Skin.FormatedText_NeededSize(thisCap, null, bFont, 16);
                    Rectangle r =
                        new((int)(drawingCoordinates.Left + ((drawingCoordinates.Width - s.Width) / 2.0)),
                            drawingCoordinates.Bottom - s.Height - 3, s.Width, s.Height);
                    r.X -= trp.X;
                    r.Y -= trp.Y;
                    r.Y = r.Y - (ConstMy * c) + ausgl;
                    //r = new Rectangle(r.Left - trp.X, r.Top - trp.Y, r.Width, r.Height);
                    //GenericControl.Skin.Draw_Back(GR, enDesign.Item_Listbox_Unterschrift, vState, r, null, false);
                    //GenericControl.Skin.Draw_Border(GR, enDesign.Item_Listbox_Unterschrift, vState, r);
                    Skin.Draw_FormatedText(gr, thisCap, Design.Item_Listbox, state, null, Alignment.Horizontal_Vertical_Center, r, null, false, false);
                }
            }
            gr.TranslateTransform(-trp.X, -trp.Y);
            gr.ResetTransform();
            if (drawBorderAndBack) {
                Skin.Draw_Border(gr, itemdesign, state, positionModified);
            }
        }

        protected override string GetCompareKey() => Internal;

        private void GetBin() {
            if (string.IsNullOrEmpty(_filename)) { return; }
            if (_bin != null) { return; }
            try {
                _bin = Converter.FileToByte(_filename);
                if (FileExists(_filename)) {
                    if (!string.IsNullOrEmpty(_encryptionKey)) {
                        _bin = Cryptography.SimpleCrypt(_bin, _encryptionKey, -1);
                    }
                }
            } catch (Exception ex) {
                Develop.DebugPrint(ex);
            }
        }

        #endregion
    }
}
// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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

using System;
using System.Collections.Generic;
using System.Drawing;
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Enums;
using static BlueBasics.IO;
using static BlueBasics.Extensions;

namespace BlueControls.ItemCollection.ItemCollectionList;

public class BitmapListItem : BasicListItem {

    #region Fields

    private const int ConstMy = 15;
    private Bitmap? _bitmap;

    private string _caption;

    private int _captionlines = 2;

    private List<string> _captiontmp = new();

    private string _imageFilename = string.Empty;

    #endregion

    #region Constructors

    public BitmapListItem(Bitmap? bmp, string internalname, string caption) : base(internalname, true) {
        _caption = caption;
        _captiontmp.Clear();
        _bitmap = bmp;
        Padding = 0;
        Overlays.Clear();
    }

    public BitmapListItem(string filename, string internalname, string caption) : base(internalname, true) {
        _caption = caption;
        _captiontmp.Clear();
        //_Bitmap = bmp;
        _imageFilename = filename;
        Padding = 0;
        Overlays.Clear();
        //_overlays.ListOrItemChanged += _overlays_ListOrItemChanged;
    }

    #endregion

    #region Properties

    public Bitmap? Bitmap {
        get {
            GetImage();
            return _bitmap;
        }
        set {
            _imageFilename = string.Empty;
            _bitmap = value;
            OnChanged();
        }
    }

    public string Caption {
        get => _caption;
        set {
            if (_caption == value) { return; }
            _caption = value;
            _captiontmp.Clear();
            OnChanged();
        }
    }

    public int CaptionLines {
        get => _captionlines;
        set {
            if (value < 1) { value = 1; }
            if (_captionlines == value) { return; }
            _captionlines = value;
            _captiontmp.Clear();
            OnChanged();
        }
    }

    public List<QuickImage> Overlays { get; } = new();

    public int Padding { get; set; }

    public override string QuickInfo => string.Empty;

    #endregion

    #region Methods

    public override object? Clone() {
        Develop.DebugPrint_NichtImplementiert();
        return null;
    }

    public override bool FilterMatch(string filterText) => base.FilterMatch(filterText) || Caption.ToUpper().Contains(filterText.ToUpper()) || (_imageFilename != null && _imageFilename.ToUpper().Contains(filterText.ToUpper()));

    public override int HeightForListBox(BlueListBoxAppearance style, int columnWidth, Design itemdesign) {
        if (style == BlueListBoxAppearance.FileSystem) {
            return 110 + (_captionlines * ConstMy);
        }

        if (_bitmap == null) { return (int)(columnWidth * 0.8); }

        var sc = (float)_bitmap.Height / _bitmap.Width;

        if (sc > 1) { sc = 1; }

        return (int)(sc * columnWidth);
    }

    public bool ImageLoaded() => _bitmap != null;

    protected override Size ComputeSizeUntouchedForListBox(Design itemdesign) {
        try {
            if (_bitmap == null) { return new Size(300, 300); }

            var sc = (float)_bitmap.Height / _bitmap.Width;

            if (sc > 1) { sc = 1; }

            return new Size(300, (int)(sc * 300));
        } catch {
            //... wird an anderer Stelle verwendet...
            return ComputeSizeUntouchedForListBox(itemdesign);
        }
    }

    protected override void DrawExplicit(Graphics gr, Rectangle positionModified, Design itemdesign, States state, bool drawBorderAndBack, bool translate) {
        if (drawBorderAndBack) { Skin.Draw_Back(gr, itemdesign, state, positionModified, null, false); }

        var drawingCoordinates = positionModified;
        drawingCoordinates.Inflate(-Padding, -Padding);
        var scaledImagePosition = RectangleF.Empty;
        var areaOfWholeImage = RectangleF.Empty;
        var bFont = Skin.GetBlueFont(itemdesign, state);
        GetImage();
        if (!string.IsNullOrEmpty(_caption) && _captiontmp.Count == 0) { _captiontmp = bFont.SplitByWidth(_caption, drawingCoordinates.Width, _captionlines); }

        //Point trp;

        bool ok;
        do {
            ok = true;
            if (_bitmap != null) {
                try {
                    areaOfWholeImage = new RectangleF(0, 0, _bitmap.Width, _bitmap.Height);
                    var scale = (float)Math.Min((drawingCoordinates.Width - (Padding * 2)) / (double)_bitmap.Width,
                        (drawingCoordinates.Height - (Padding * 2) - (_captionlines * ConstMy)) / (double)_bitmap.Height);
                    scaledImagePosition = new RectangleF(((drawingCoordinates.Width - (_bitmap.Width * scale)) / 2) + drawingCoordinates.Left,
                        ((drawingCoordinates.Height - (_bitmap.Height * scale)) / 2) + drawingCoordinates.Top - (_captionlines * ConstMy / 2),
                        _bitmap.Width * scale,
                        _bitmap.Height * scale);
                } catch {
                    ok = false;
                }
            }
        } while (!ok);

        var trp = drawingCoordinates.PointOf(Alignment.Horizontal_Vertical_Center);
        scaledImagePosition = scaledImagePosition with { X = scaledImagePosition.Left - trp.X, Y = scaledImagePosition.Top - trp.Y };

        gr.TranslateTransform(trp.X, trp.Y);

        bool ok2;
        do {
            ok2 = true;
            try {
                if (_bitmap != null) { gr.DrawImage(_bitmap, scaledImagePosition, areaOfWholeImage, GraphicsUnit.Pixel); }
                foreach (var thisQi in Overlays) {
                    gr.DrawImage(thisQi, scaledImagePosition.Left + 8, scaledImagePosition.Top + 8);
                }
            } catch {
                //Trotz lock kommt "Das Objekt wird an anderer Stelle verwendent.
                // Ein Bitmap?
                ok2 = false;
            }
        } while (!ok2);

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

    private void GetImage() {
        if (string.IsNullOrEmpty(_imageFilename)) { return; }
        if (_bitmap != null) { return; }
        try {
            if (FileExists(_imageFilename)) {
                _bitmap = Image_FromFile(_imageFilename) as Bitmap;
            }
        } catch (Exception ex) {
            Develop.DebugPrint("Fehler beim laden des Bildes", ex);
        }
    }

    #endregion
}
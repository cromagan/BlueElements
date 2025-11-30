// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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
using BlueControls.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using static BlueBasics.Extensions;
using static BlueBasics.IO;

namespace BlueControls.ItemCollectionList;

public class BitmapListItem : AbstractListItem {

    #region Fields

    private const int ConstMy = 15;
    private readonly int _captionlines = 2;
    private Bitmap? _bitmap;

    private string _caption;
    private List<string> _captiontmp = [];

    private string _imageFilename = string.Empty;

    #endregion

    #region Constructors

    public BitmapListItem(Bitmap? bmp, string keyName, string caption) : base(keyName, true) {
        _caption = caption;
        _captiontmp.Clear();
        _bitmap = bmp;
        Padding = 0;
        Overlays.Clear();
    }

    public BitmapListItem(string filename, string keyName, string caption) : base(keyName, true) {
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
            OnPropertyChanged();
        }
    }

    public string Caption {
        get => _caption;
        set {
            if (_caption == value) { return; }
            _caption = value;
            _captiontmp.Clear();
            OnPropertyChanged();
        }
    }

    //public int CaptionLines {
    //    get => _captionlines;
    //    set {
    //        if (value < 1) { value = 1; }
    //        if (_captionlines == value) { return; }
    //        _captionlines = value;
    //        _captiontmp.Clear();
    //        OnPropertyChanged(string propertyname);
    //    }
    //}

    public List<QuickImage> Overlays { get; } = [];

    public int Padding { get; set; }

    public override string QuickInfo => string.Empty;

    #endregion

    #region Methods

    public override bool FilterMatch(string filterText) => base.FilterMatch(filterText) || Caption.ToUpperInvariant().Contains(filterText.ToUpperInvariant()) || _imageFilename.ToUpperInvariant().Contains(filterText.ToUpperInvariant());

    public override int HeightInControl(ListBoxAppearance style, int columnWidth, Design itemdesign) {
        if (style == ListBoxAppearance.FileSystem) {
            return 110 + (_captionlines * ConstMy);
        }

        if (_bitmap == null) { return (int)(columnWidth * 0.8); }

        var sc = (float)_bitmap.Height / _bitmap.Width;

        if (sc > 1) { sc = 1; }

        return (int)(sc * columnWidth);
    }

    public bool ImageLoaded() => _bitmap != null;

    protected override Size ComputeUntrimmedCanvasSize(Design itemdesign) {
        try {
            if (_bitmap == null) { return new Size(300, 300); }

            var sc = (float)_bitmap.Height / _bitmap.Width;

            if (sc > 1) { sc = 1; }

            return new Size(300, (int)(sc * 300));
        } catch {
            //... wird an anderer Stelle verwendet...
            Develop.AbortAppIfStackOverflow();
            return ComputeUntrimmedCanvasSize(itemdesign);
        }
    }

    protected override void DrawExplicit(Graphics gr, Rectangle visibleArea, RectangleF positionInControl, Design itemdesign, States state, bool drawBorderAndBack, bool translate, float offsetX, float offsetY, float scale) {
        if (drawBorderAndBack) { Skin.Draw_Back(gr, itemdesign, state, positionInControl.ToRect(), null, false); }

        var drawingCoordinates = positionInControl;
        drawingCoordinates.Inflate(-Padding, -Padding);
        var scaledImagePosition = RectangleF.Empty;
        var areaOfWholeImage = RectangleF.Empty;
        var bFont = Skin.GetBlueFont(itemdesign, state);
        GetImage();
        if (!string.IsNullOrEmpty(_caption) && _captiontmp.Count == 0) { _captiontmp = BlueFont.SplitByWidth(bFont, _caption, drawingCoordinates.Width, _captionlines); }

        //Point trp;

        bool ok;
        do {
            ok = true;
            if (_bitmap != null) {
                try {
                    lock (_bitmap) {
                        areaOfWholeImage = new RectangleF(0, 0, _bitmap.Width, _bitmap.Height);
                        scale = (float)Math.Min((drawingCoordinates.Width - (Padding * 2)) / (double)_bitmap.Width,
                            (drawingCoordinates.Height - (Padding * 2) - (_captionlines * ConstMy)) / (double)_bitmap.Height);
                        scaledImagePosition = new RectangleF(((drawingCoordinates.Width - _bitmap.Width.CanvasToControl(scale)) / 2) + drawingCoordinates.Left,
                            ((drawingCoordinates.Height - _bitmap.Height.CanvasToControl(scale)) / 2) + drawingCoordinates.Top - (_captionlines * ConstMy / 2f),
                            _bitmap.Width.CanvasToControl(scale),
                            _bitmap.Height.CanvasToControl(scale));
                    }
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
                var s = bFont.FormatedText_NeededSize(thisCap, null, 16);
                var r = new Rectangle((int)(drawingCoordinates.Left + ((drawingCoordinates.Width - s.Width) / 2.0)),
                       (int)drawingCoordinates.Bottom - s.Height - 3, s.Width, s.Height);
                r.X -= (int)trp.X;
                r.Y -= (int)trp.Y;
                r.Y = r.Y - (ConstMy * c) + ausgl;
                //r = new Rectangle(r.Left - trp.ControlX, r.Top - trp.Y, r.Width, r.Height);
                //GenericControl.Skin.Draw_Back(GR, enDesign.Item_Listbox_Unterschrift, vState, r, null, false);
                //GenericControl.Skin.Draw_Border(GR, enDesign.Item_Listbox_Unterschrift, vState, r);
                Skin.Draw_FormatedText(gr, thisCap, null, Alignment.Horizontal_Vertical_Center, r, Design.Item_Listbox, state, null, false, false);
            }
        }
        gr.TranslateTransform(-trp.X, -trp.Y);
        gr.ResetTransform();
        if (drawBorderAndBack) {
            Skin.Draw_Border(gr, itemdesign, state, positionInControl.ToRect());
        }
    }

    protected override string GetCompareKey() => KeyName;

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
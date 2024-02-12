// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.ItemCollectionList;
using static BlueBasics.IO;
using static BlueBasics.Extensions;
using Orientation = BlueBasics.Enums.Orientation;
using static BlueBasics.Constants;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
public partial class ZoomPicWithPoints : ZoomPic {

    #region Fields

    public string Feedback = string.Empty;

    private static readonly Brush BrushRotTransp = new SolidBrush(Color.FromArgb(200, 255, 0, 0));
    private static readonly Pen PenRotTransp = new(Color.FromArgb(200, 255, 0, 0));
    private readonly List<PointM> _points = [];
    private Helpers _helper = Helpers.Ohne;
    private Orientation _mittelLinie = Orientation.Ohne;
    private bool _pointAdding;

    #endregion

    #region Constructors

    public ZoomPicWithPoints() : base() => InitializeComponent();

    #endregion

    #region Properties

    [DefaultValue(Helpers.Ohne)]
    public Helpers Helper {
        get => _helper;
        set {
            if (_helper == value) { return; }
            _helper = value;
            Invalidate();
        }
    }

    [DefaultValue((Orientation)(-1))]
    public Orientation Mittellinie {
        get => _mittelLinie;
        set {
            if (_mittelLinie == value) { return; }
            _mittelLinie = value;
            Invalidate();
        }
    }

    public List<string> Tags { get; } = [];

    #endregion

    #region Methods

    public static string FilenameTxt(string pathOfPicture) => pathOfPicture.FilePath() + pathOfPicture.FileNameWithoutSuffix() + ".txt";

    public static BitmapListItem GenerateBitmapListItem(string pathOfPicture) {
        var (bitmap, list) = LoadFromDisk(pathOfPicture);
        return GenerateBitmapListItem(bitmap, list);
    }

    public static BitmapListItem GenerateBitmapListItem(Bitmap? bmp, List<string> tags) {
        var filenamePng = tags.TagGet("ImageFile");
        BitmapListItem i = new(bmp, filenamePng, filenamePng.FileNameWithoutSuffix()) {
            Padding = 10,
            Tag = tags
        };
        return i;
    }

    public static Tuple<Bitmap?, List<string>> LoadFromDisk(string pathOfPicture) {
        Bitmap? bmp = null;
        List<string> tags = [];
        if (FileExists(pathOfPicture)) {
            bmp = (Bitmap?)Image_FromFile(pathOfPicture);
        }
        var ftxt = FilenameTxt(pathOfPicture);
        if (FileExists(ftxt)) {
            tags = File.ReadAllText(ftxt, Encoding.UTF8).SplitAndCutByCrToList();
        }
        tags.TagSet("ImageFile", pathOfPicture);
        return new Tuple<Bitmap?, List<string>>(bmp, tags);
    }

    public BitmapListItem GenerateBitmapListItem() {
        WritePointsInTags();
        return GenerateBitmapListItem(Bmp, Tags);
    }

    public PointM? GetPoint(string name) => _points.Get(name);

    public void LetUserAddAPoint(string pointName, Helpers helper, Orientation mittelline) {
        _mittelLinie = mittelline;
        _helper = helper;
        Feedback = pointName;
        _pointAdding = true;
        Invalidate();
    }

    public void LoadData(string pathOfPicture) {
        var (bitmap, list) = LoadFromDisk(pathOfPicture);
        Bmp = bitmap;
        Tags.Clear();
        Tags.AddRange(list);
        GeneratePointsFromTags();
        Invalidate();
    }

    public void PointClear() {
        _points.Clear();
        WritePointsInTags();
        Invalidate();
    }

    public void PointSet(string name, int x, int y) => PointSet(name, x, (float)y);

    public void PointSet(string name, float x, float y) {
        var p = _points.Get(name);
        if (p == null) {
            p = new PointM(name, x, y);
            _points.Add(p);
            WritePointsInTags();
            Invalidate();
            return;
        }
        if (Math.Abs(p.X - x) > DefaultTolerance || Math.Abs(p.Y - y) > DefaultTolerance) {
            p.X = x;
            p.Y = y;
            Invalidate();
        }
        WritePointsInTags();
    }

    public void SaveData() {
        WritePointsInTags();
        var path = Tags.TagGet("ImageFile");
        var pathtxt = FilenameTxt(path);
        try {
            Bmp?.Save(path, ImageFormat.Png);

            Tags.TagSet("Erstellt", Generic.UserName);
            Tags.TagSet("Datum", DateTime.UtcNow.ToString(Format_Date5, CultureInfo.InvariantCulture));
            Tags.WriteAllText(pathtxt, Win1252, false);
        } catch {
            Develop.DebugPrint("Fehler beim Speichern: " + pathtxt);
            _ = MessageBox.Show("Fehler beim Speichern");
        }
    }

    protected override RectangleF MaxBounds() {
        var r = base.MaxBounds();
        foreach (var thisP in _points) {
            r.X = Math.Min(r.X, thisP.X);
            r.Y = Math.Min(r.Y, thisP.Y);
            r.Width = Math.Max(r.Width, thisP.X - r.X);
            r.Height = Math.Max(r.Height, thisP.Y - r.Y);
        }
        return r;
    }

    //protected override void DrawControl(Graphics gr, enStates state)
    //{
    //    PrepareOverlay();
    //    base.DrawControl(gr, state);
    //}
    protected override void OnDoAdditionalDrawing(AdditionalDrawing e) {
        base.OnDoAdditionalDrawing(e);
        DrawMittelLinien(e);
        /// Punkte
        foreach (var thisPoint in _points) {
            if (_helper.HasFlag(Helpers.PointNames)) {
                thisPoint.Draw(e.G, e.Zoom, e.ShiftX, e.ShiftY, Design.Button_EckpunktSchieber, States.Standard);
            } else {
                thisPoint.Draw(e.G, e.Zoom, e.ShiftX, e.ShiftY, Design.Button_EckpunktSchieber, States.Standard);
            }
        }
    }

    protected override void OnImageMouseUp(MouseEventArgs1_1 e) {
        if (_pointAdding && !string.IsNullOrEmpty(Feedback)) {
            PointSet(Feedback, e.X, e.Y);
            _pointAdding = false;
        }
        base.OnImageMouseUp(e); // erst nachher, dass die MouseUpRoutine das Feedback nicht änddern kann
        //Feedback = string.Empty;
    }

    protected override void OnMouseDown(MouseEventArgs e) {
        base.OnMouseDown(e);
        Invalidate(); // Mousedown bereits in _MouseDown gespeichert
    }

    protected override void OnMouseLeave(System.EventArgs e) {
        base.OnMouseLeave(e);
        Invalidate();
    }

    protected override void OnMouseMove(MouseEventArgs e) {
        base.OnMouseMove(e);
        Invalidate();
    }

    //            return PathOfPicture.TrimEnd(".PNG").TrimEnd(".JPG").TrimEnd(".JPG") + ".txt";
    private void DrawMittelLinien(AdditionalDrawing eg) {
        if (Bmp == null || !Bmp.IsValid()) { return; }
        PositionEventArgs e = new(MousePos11.X, MousePos11.Y);
        OnOverwriteMouseImageData(e);
        ///// Punkte
        //foreach (var ThisPoint in points)
        //{
        //    ThisPoint.Draw(e.G, e.Zoom, e.MoveX, e.MoveY, enDesign.Button_EckpunktSchieber, enStates.Standard);
        //}
        //if (OverlayBmp == null || OverlayBmp.Width != Bmp.Width || OverlayBmp.Height != Bmp.Height)
        //{
        //    OverlayBmp = new Bitmap(Bmp.Width, Bmp.Height);
        //}
        //var TMPGR = Graphics.FromImage(Bmp);
        //TMPGR.Clear(Color.Transparent);
        // Mittellinie
        var picturePos = base.MaxBounds();
        if (_mittelLinie.HasFlag(Orientation.Waagerecht)) {
            var p1 = picturePos.PointOf(Alignment.VerticalCenter_Left).ZoomAndMove(eg.Zoom, eg.ShiftX, eg.ShiftY);
            var p2 = picturePos.PointOf(Alignment.VerticalCenter_Right).ZoomAndMove(eg.Zoom, eg.ShiftX, eg.ShiftY);
            eg.G.DrawLine(new Pen(Color.FromArgb(10, 0, 0, 0), 3), p1, p2);
            eg.G.DrawLine(new Pen(Color.FromArgb(220, 100, 255, 100)), p1, p2);
        }
        if (_mittelLinie.HasFlag(Orientation.Senkrecht)) {
            var p1 = picturePos.PointOf(Alignment.Top_HorizontalCenter).ZoomAndMove(eg.Zoom, eg.ShiftX, eg.ShiftY);
            var p2 = picturePos.PointOf(Alignment.Bottom_HorizontalCenter).ZoomAndMove(eg.Zoom, eg.ShiftX, eg.ShiftY);
            eg.G.DrawLine(new Pen(Color.FromArgb(10, 0, 0, 0), 3), p1, p2);
            eg.G.DrawLine(new Pen(Color.FromArgb(220, 100, 255, 100)), p1, p2);
        }
        if (MousePos11.IsEmpty) { return; }
        if (_helper.HasFlag(Helpers.HorizontalLine)) {
            var p1 = new PointM(0, e.Y).ZoomAndMove(eg);
            var p2 = new PointM(Bmp.Width, e.Y).ZoomAndMove(eg);
            eg.G.DrawLine(PenRotTransp, p1, p2);
        }
        if (_helper.HasFlag(Helpers.VerticalLine)) {
            var p1 = new PointM(e.X, 0).ZoomAndMove(eg);
            var p2 = new PointM(e.X, Bmp.Height).ZoomAndMove(eg);
            eg.G.DrawLine(PenRotTransp, p1, p2);
        }
        if (_helper.HasFlag(Helpers.SymetricalHorizontal)) {
            var h = Bmp.Width / 2;
            var x = Math.Abs(h - e.X);
            var p1 = new PointM(h - x, e.Y).ZoomAndMove(eg);
            var p2 = new PointM(h + x, e.Y).ZoomAndMove(eg);
            eg.G.DrawLine(PenRotTransp, p1, p2);
        }
        if (_helper.HasFlag(Helpers.MouseDownPoint)) {
            var m1 = new PointM(e.X, e.Y).ZoomAndMove(eg);
            eg.G.DrawEllipse(PenRotTransp, new RectangleF(m1.X - 3, m1.Y - 3, 6, 6));
            if (!MouseDownPos11.IsEmpty) {
                var md1 = new PointM(MouseDownPos11).ZoomAndMove(eg);
                var mc1 = new PointM(e.X, e.Y).ZoomAndMove(eg);
                eg.G.DrawEllipse(PenRotTransp, new RectangleF(md1.X - 3, md1.Y - 3, 6, 6));
                eg.G.DrawLine(PenRotTransp, mc1, md1);
            }
        }
        if (_helper.HasFlag(Helpers.FilledRectancle)) {
            if (!MouseDownPos11.IsEmpty) {
                var md1 = new PointM(MouseDownPos11).ZoomAndMove(eg);
                var mc1 = new PointM(e.X, e.Y).ZoomAndMove(eg);
                RectangleF r = new(Math.Min(md1.X, e.X), Math.Min(md1.Y, e.Y),
                    Math.Abs(md1.X - mc1.X) + 1, Math.Abs(md1.Y - mc1.Y) + 1);
                eg.G.FillRectangle(BrushRotTransp, r);
            }
        }
    }

    private void GeneratePointsFromTags() {
        var names = Tags.TagGet("AllPointNames").FromNonCritical().SplitAndCutBy("|");
        _points.Clear();
        foreach (var thisO in names) {
            var s = Tags.TagGet(thisO);
            _points.Add(new PointM(null, s));
        }
    }

    private void WritePointsInTags() {
        var old = Tags.TagGet("AllPointNames").FromNonCritical().SplitAndCutBy("|");
        foreach (var thisO in old) {
            Tags.TagSet(thisO, string.Empty);
        }
        var s = string.Empty;
        foreach (var thisP in _points) {
            s = s + thisP.KeyName + "|";
            Tags.TagSet(thisP.KeyName, thisP.ToString());
        }
        Tags.TagSet("AllPointNames", s.TrimEnd("|").ToNonCritical());
    }

    #endregion
}
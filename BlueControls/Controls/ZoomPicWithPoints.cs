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
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.ItemCollectionList;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using static BlueBasics.Constants;
using static BlueBasics.Extensions;
using static BlueBasics.IO;
using Orientation = BlueBasics.Enums.Orientation;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
public partial class ZoomPicWithPoints : ZoomPic {

    #region Fields

    /// <summary>
    /// Wenn eine Aktion ausgeführt wird, ein String, der den Aktionsnamen beinhaltet
    /// </summary>
    public string UserAction = string.Empty;

    private const int DrawSize = 20;
    private static readonly Brush BrushRotTransp = new SolidBrush(Color.FromArgb(200, 255, 0, 0));
    private static readonly Pen PenRotTransp = new(Color.FromArgb(200, 255, 0, 0));
    private readonly List<PointM> _points = [];
    private Helpers _helper = Helpers.None;
    private Orientation _mittelLinie = Orientation.Ohne;
    private bool _pointAdding;

    #endregion

    #region Constructors

    public ZoomPicWithPoints() : base() => InitializeComponent();

    #endregion

    #region Properties

    [DefaultValue(Helpers.None)]
    public Helpers Helper {
        get => _helper;
        set {
            if (_helper == value) { return; }
            _helper = value;
            Invalidate();
        }
    }

    [DefaultValue("")]
    public string InfoText {
        get;
        set {
            if (field == value) { return; }
            field = value;
            Invalidate();
        }
    } = string.Empty;

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
        // Used: Only BZL
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

        if (FileExists(pathOfPicture)) {
            bmp = (Bitmap?)Image_FromFile(pathOfPicture);
        }
        return new Tuple<Bitmap?, List<string>>(bmp, LoadTags(pathOfPicture));
    }

    public static List<string> LoadTags(string pathOfPicture) {
        List<string> tags = [];

        var ftxt = FilenameTxt(pathOfPicture);
        if (FileExists(ftxt)) {
            tags = ReadAllText(ftxt, Encoding.UTF8).SplitAndCutByCr().ToList();
        }
        tags.TagSet("ImageFile", pathOfPicture);
        return tags;
    }

    public BitmapListItem GenerateBitmapListItem() {
        // Used: Only BZL
        WritePointsInTags();
        return GenerateBitmapListItem(Bmp, Tags);
    }

    // Used: Only BZL
    public PointM? GetPoint(string name) => _points.GetByKey(name);

    public void LetUserAddAPoint(string pointName, Helpers helper, Orientation mittelline) {
        // Used: Only BZL
        _mittelLinie = mittelline;
        _helper = helper;
        UserAction = pointName;
        _pointAdding = true;
        Invalidate();
    }

    public void LoadData(string pathOfPicture) {
        // Used: Only BZL
        var (bitmap, tags) = LoadFromDisk(pathOfPicture);
        Bmp = bitmap;
        Tags.Clear();
        Tags.AddRange(tags);
        GeneratePointsFromTags();
        Invalidate();
    }

    public void PointClear() {
        // Used: Only BZL
        _points.Clear();
        WritePointsInTags();
        Invalidate();
    }

    public void PointSet(string name, int x, int y) => PointSet(name, x, (float)y);

    public void PointSet(string name, float x, float y) {
        var p = _points.GetByKey(name);
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
        // Used: Only BZL
        WritePointsInTags();
        var path = Tags.TagGet("ImageFile");
        var pathtxt = FilenameTxt(path);
        try {
            Bmp?.Save(path, ImageFormat.Png);

            Tags.TagSet("Erstellt", Generic.UserName);
            Tags.TagSet("Datum", DateTime.UtcNow.ToString5());
            Tags.WriteAllText(pathtxt, Win1252, false);
        } catch {
            Develop.DebugPrint("Fehler beim Speichern: " + pathtxt);
            MessageBox.Show("Fehler beim Speichern");
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
        DrawHelpers(e);
        // Punkte
        foreach (var thisPoint in _points) {
            if (_helper.HasFlag(Helpers.PointNames)) {
                thisPoint.Draw(e.G, e.Zoom, e.ShiftX, e.ShiftY, Design.Button_EckpunktSchieber, States.Standard);
            } else {
                thisPoint.Draw(e.G, e.Zoom, e.ShiftX, e.ShiftY, Design.Button_EckpunktSchieber, States.Standard);
            }
        }

        // Info Text
        if (!string.IsNullOrEmpty(InfoText)) {
            PrintInfoText(e.G);
        }

        // Magnifier
        if (_helper.HasFlag(Helpers.Magnifier) && Bmp != null && !MousePos11.IsEmpty) {
            BitmapExt.Magnify(Bmp, new Point(MousePos11.X, MousePos11.Y), e.G, false);
        }
    }

    protected override void OnImageMouseUp(MouseEventArgs1_1 e) {
        if (_pointAdding && !string.IsNullOrEmpty(UserAction)) {
            PointSet(UserAction, e.X, e.Y);
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
    private void DrawHelpers(AdditionalDrawing eg) {
        if (Bmp == null || !Bmp.IsValid()) { return; }

        var drawArea = AvailablePaintArea();
        eg.G.SetClip(drawArea);

        PositionEventArgs e = new(MousePos11.X, MousePos11.Y);
        OnOverwriteMouseImageData(e);

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

        if (MousePos11.IsEmpty) {
            eg.G.ResetClip();
            return;
        }

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
                RectangleF r = new(Math.Min(md1.X, e.X), Math.Min(md1.Y, e.Y), Math.Abs(md1.X - mc1.X) + 1, Math.Abs(md1.Y - mc1.Y) + 1);
                eg.G.FillRectangle(BrushRotTransp, r);
            }
        }

        // Rechteck zeichnen
        if (_helper.HasFlag(Helpers.DrawRectangle)) {
            if (!MouseDownPos11.IsEmpty) {
                var md1 = new PointM(MouseDownPos11).ZoomAndMove(eg);
                var mc1 = new PointM(e.X, e.Y).ZoomAndMove(eg);
                RectangleF r = new(Math.Min(md1.X, e.X), Math.Min(md1.Y, e.Y), Math.Abs(md1.X - mc1.X) + 1, Math.Abs(md1.Y - mc1.Y) + 1);
                eg.G.DrawRectangle(PenRotTransp, r.X, r.Y, r.Width, r.Height);
            }
        }

        // kleines Rechteck zeichnen
        if (_helper.HasFlag(Helpers.Draw20x10)) {
            if (!MousePos11.IsEmpty) {
                // Startpunkt des Rechtecks (oben links)
                var startPoint = new PointM(MousePos11.X - 10, MousePos11.Y - 5);
                // Skaliere und verschiebe den Startpunkt
                var scaledStart = startPoint.ZoomAndMove(eg);

                // Berechne die skalierten Dimensionen
                var scaledWidth = 20 * eg.Zoom;  // 20 Pixel Breite
                var scaledHeight = 10 * eg.Zoom;  // 10 Pixel Höhe

                // Zeichne das Rechteck mit den skalierten Werten
                eg.G.DrawRectangle(PenRotTransp,
                    scaledStart.X,
                    scaledStart.Y,
                    scaledWidth,
                    scaledHeight);
            }
        }

        eg.G.ResetClip();
    }

    private void GeneratePointsFromTags() {
        var names = Tags.TagGet("AllPointNames").FromNonCritical().SplitAndCutBy("|");
        _points.Clear();
        foreach (var thisO in names) {
            var s = Tags.TagGet(thisO);
            _points.Add(new PointM(null, s));
        }
    }

    private void PrintInfoText(Graphics gr) {
        if (string.IsNullOrEmpty(InfoText)) { return; }

        // Grundlegendes Setup
        using var bs = new SolidBrush(Color.FromArgb(150, 0, 0, 0));
        using var bf = new SolidBrush(Color.FromArgb(255, 255, 0, 0));
        using var fn = new Font("Arial", DrawSize, FontStyle.Bold);

        var drawArea = AvailablePaintArea();
        gr.SetClip(drawArea);

        // Hole alle Bildschirme
        var screens = Screen.AllScreens;

        foreach (var screen in screens) {
            // Konvertiere Bildschirmkoordinaten in Control-Koordinaten
            var screenBounds = screen.Bounds;
            var controlPoint = PointToClient(new Point(screenBounds.X, screenBounds.Y));

            // Berechne die Position für diesen Bildschirm
            var textSize = fn.MeasureString(InfoText);

            // Prüfe ob die Maus auf diesem Bildschirm ist
            var mouseScreenPoint = PointToScreen(new Point(MousePos11.X, MousePos11.Y));
            var mouseOnThisScreen = screen.Bounds.Contains(mouseScreenPoint);

            // Bestimme Y-Position unter Berücksichtigung der Scrollbars
            float yPos;
            if (mouseOnThisScreen) {
                var relativeMouseY = mouseScreenPoint.Y - screenBounds.Y;
                yPos = relativeMouseY < (screenBounds.Height / 2)
                    ? Math.Min(drawArea.Bottom - textSize.Height, controlPoint.Y + screenBounds.Height - textSize.Height)
                    : Math.Max(drawArea.Top, controlPoint.Y);
            } else {
                yPos = Math.Max(drawArea.Top, controlPoint.Y);
            }

            // Zeichne Hintergrund und Text innerhalb des verfügbaren Bereichs
            var rectBackground = new RectangleF(
                Math.Max(drawArea.Left, controlPoint.X),
                yPos - 5,
                Math.Min(screenBounds.Width, drawArea.Width),
                textSize.Height + 10);

            gr.FillRectangle(bs, rectBackground);
            BlueFont.DrawString(gr, InfoText, fn, bf,
                Math.Max(drawArea.Left + 2, controlPoint.X + 2),
                yPos);
        }

        gr.ResetClip();
    }

    private void WritePointsInTags() {
        var old = Tags.TagGet("AllPointNames").FromNonCritical().SplitAndCutBy("|");
        foreach (var thisO in old) {
            Tags.TagSet(thisO, string.Empty);
        }
        var s = string.Empty;
        foreach (var thisP in _points) {
            s = s + thisP.KeyName + "|";
            Tags.TagSet(thisP.KeyName, thisP.ParseableItems().FinishParseable());
        }
        Tags.TagSet("AllPointNames", s.TrimEnd("|").ToNonCritical());
    }

    #endregion
}
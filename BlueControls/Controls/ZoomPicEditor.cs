// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Classes;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Text;
using static BlueBasics.ClassesStatic.Constants;
using static BlueBasics.ClassesStatic.IO;
using static BlueBasics.Extensions;
using Orientation = BlueBasics.Enums.Orientation;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
public partial class ZoomPicEditor : ZoomPic {

    #region Fields

    private readonly List<PointM> _points = [];
    private bool _pointAdding;

    #endregion

    #region Constructors

    public ZoomPicEditor() : base() {
        InitializeComponent();
    }

    #endregion

    #region Properties

    public List<string> Tags { get; } = [];

    public string UserAction { get; set; } = string.Empty;

    #endregion

    #region Methods

    public static string FilenameTxt(string pathOfPicture) => pathOfPicture.FilePath() + pathOfPicture.FileNameWithoutSuffix() + ".txt";

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
            tags = [.. ReadAllText(ftxt, Encoding.UTF8).SplitAndCutByCr()];
        }
        tags.TagSet("ImageFile", pathOfPicture);
        return tags;
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

    public void SetData(Bitmap bitmap, List<string> tags) {
        Bmp = bitmap;
        Tags.Clear();
        Tags.AddRange(tags);
        GeneratePointsFromTags();
        Invalidate();
    }

    protected override RectangleF CalculateCanvasMaxBounds() {
        var a = base.CalculateCanvasMaxBounds();
        foreach (var thisP in _points) {
            a.X = Math.Min(a.X, thisP.X);
            a.Y = Math.Min(a.Y, thisP.Y);
            a.Width = Math.Max(a.Width, thisP.X - a.X);
            a.Height = Math.Max(a.Height, thisP.Y - a.Y);
        }
        return a;
    }

    protected override void OnDoAdditionalDrawing(AdditionalDrawingEventArgs e) {
        base.OnDoAdditionalDrawing(e);

        foreach (var thisPoint in _points) {
            thisPoint.Draw(e.Graphics, e.Zoom, e.OffsetX, e.OffsetY, Design.HandlePoint, States.Standard);
        }
    }

    protected override void OnImageMouseUp(TrimmedCanvasMouseEventArgs e) {
        if (_pointAdding && !string.IsNullOrEmpty(UserAction)) {
            PointSet(UserAction, e.CanvasX, e.CanvasY);
            _pointAdding = false;
        }
        base.OnImageMouseUp(e);
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
            Tags.TagSet(thisP.KeyName, thisP.ParseableItems().FinishParseable());
        }
        Tags.TagSet("AllPointNames", s.TrimEnd('|').ToNonCritical());
    }

    #endregion
}

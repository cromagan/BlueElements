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

    private UserInputMode _inputMode;

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

    public PointM? GetPoint(string name) {
        return null;
    }

    public void LetUserAddAPoint(string kn, UserInputMode inputmode, Helpers helper, Orientation orientation) {
        _mittelLinie = orientation;
        _helper = helper;
        UserAction = kn;
        _inputMode = inputmode;
        Invalidate();
    }

    public void LoadData(string pathOfPicture) {
        var (bitmap, tags) = LoadFromDisk(pathOfPicture);
        Bmp = bitmap;
        Tags.Clear();
        Tags.AddRange(tags);
        Invalidate();
    }

    public void LoadPoints() {
        var allPointNames = Tags.TagGet("AllPointNames").FromNonCritical().SplitAndCutBy("|");
        foreach (var pointName in allPointNames) {
            if (string.IsNullOrEmpty(pointName)) { continue; }
            var posStr = Tags.TagGet(pointName);
            if (string.IsNullOrEmpty(posStr)) { continue; }
            var parts = posStr.SplitAndCutBy("|");
            if (parts.Length >= 2 &&
                float.TryParse(parts[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out _) &&
                float.TryParse(parts[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out _)) {
            }
        }
    }

    public void PointClear() {
        WritePointsInTags();
        Invalidate();
    }

    public void PointSet(string name, float x, int y) => PointSet(name, x, (float)y);

    public void PointSet(string name, float x, float y) {
        WritePointsInTags();
        Invalidate();
    }

    public void SaveData() {
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
        Invalidate();
    }

    protected override void OnImageMouseUp(TrimmedCanvasMouseEventArgs e) {
        if (_inputMode == UserInputMode.Point && !string.IsNullOrEmpty(UserAction)) {
            PointSet(UserAction, e.CanvasX, e.CanvasY);
            _inputMode = UserInputMode.None;
        }
        base.OnImageMouseUp(e);
    }

    private void WritePointsInTags() {
        var old = Tags.TagGet("AllPointNames").FromNonCritical().SplitAndCutBy("|");
        foreach (var thisO in old) {
            Tags.TagRemove(thisO);
        }
        Tags.TagRemove("AllPointNames");

        if (Items == null) { return; }

        var pointNames = new List<string>();

        if (pointNames.Count > 0) {
            var encodedNames = new List<string>();
            foreach (var p in pointNames) { encodedNames.Add(p.ToNonCritical()); }
            Tags.TagSet("AllPointNames", string.Join("|", encodedNames));
        }
    }

    #endregion
}
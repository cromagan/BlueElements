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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.ItemCollection.ItemCollectionList;
using Microsoft.Win32;
using static BlueBasics.Generic;
using static BlueBasics.FileOperations;
using BlueControls.Enums;
using CryptoExplorer;
using static BlueBasics.BitmapExt;
using BlueControls.Interfaces;
using BlueScript.Variables;
using System.Drawing.Imaging;
using System.Windows.Data;
using MessageBox = BlueControls.Forms.MessageBox;

namespace BlueControls.Controls;

public partial class FileBrowser : GenericControl, IAcceptVariableList//UserControl //
{
    #region Fields

    public static readonly string CryptoErkennung = ".cyo";
    private string _ausschneiden = string.Empty;

    private bool _isLoading;
    private string _originalText = string.Empty;

    private string _sort = "Name";

    #endregion

    #region Constructors

    public FileBrowser() {
        InitializeComponent();
    }

    #endregion

    #region Events

    public event EventHandler FolderChanged;

    #endregion

    #region Properties

    [Browsable(false)]
    public bool Crypted {
        get => IsCyrpted(txbPfad.Text);
        set {
            if (IsCyrpted(txbPfad.Text) == value) { return; }

            var thumbPfad = txbPfad.Text.TrimEnd("\\") + "\\" + "Thumbs" + CryptoErkennung;

            var allF = Directory.GetFiles(txbPfad.Text.TrimEnd("\\") + "\\", "*", SearchOption.TopDirectoryOnly);
            foreach (var thisString in allF) {
                if (thisString.EndsWith(CryptoErkennung)) { File.Delete(thisString); }
            }

            if (value) {
                var l = new List<string>();
                l.Add("1.2");
                l.Add("Folder");
                //l.Add(_cryptMatrix);
                l.Save(CryoptErkennungsFile(txbPfad.Text.TrimEnd("\\") + "\\"), Constants.Win1252, false);

                Directory.CreateDirectory(thumbPfad);
            } else {
                DeleteDir(thumbPfad, true);
            }

            Reload();
        }
    }

    public string CryptMatrix { get; set; }

    public new bool Enabled {
        get => base.Enabled; set {
            base.Enabled = value;

            CheckButtons();
        }
    }

    public string OriginalText {
        get => _originalText;
        set {
            _originalText = value;
            CheckButtons();
        }
    }

    public string Pfad {
        get => txbPfad.Text;
        set {
            txbPfad.Text = value;
            txbPfad_Enter(null, null);
            CheckButtons();
        }
    }

    public string Sort {
        get => _sort;
        set {
            if (_sort == value) {
                return;
            }

            _sort = value;
            Reload();
        }
    }

    #endregion

    #region Methods

    public string GestStandardCommand(string extension) {
        if (!SubKeyExist(extension)) { return string.Empty; }
        var mainkey = Registry.ClassesRoot.OpenSubKey(extension);
        if (mainkey == null) { return string.Empty; }
        var type = mainkey.GetValue(""); // GetValue("") read the standard value of a key
        if (type == null) { return string.Empty; }
        mainkey = Registry.ClassesRoot.OpenSubKey(type.ToString());
        if (mainkey == null) { return string.Empty; }

        var shellKey = mainkey.OpenSubKey("shell");
        if (shellKey == null) { return string.Empty; }
        var shellComand = shellKey.GetValue("");

        if (shellComand == null) { return string.Empty; }

        var exekey = shellKey.OpenSubKey(shellComand.ToString());
        if (exekey == null) { return string.Empty; }
        exekey = exekey.OpenSubKey("command");
        if (exekey == null) { return string.Empty; }
        return exekey.GetValue("").ToString();
    }

    //public string GetDescription(string extension) {
    //    if (SubKeyExist(extension)) {
    //        var mainkey = Registry.ClassesRoot.OpenSubKey(extension);
    //        var type = mainkey.GetValue(""); // GetValue("") read the standard value of a key
    //        if (type == null) {
    //            return extension.Replace(".", string.Empty).ToUpper() + "-Datei";
    //        }

    //        mainkey = Registry.ClassesRoot.OpenSubKey(type.ToString());
    //        return mainkey.GetValue("").ToString();
    //    } else {
    //        return "Unknown Extension";
    //    }
    //}

    public void ParentPath() => btnZurück_Click(null, System.EventArgs.Empty);

    public bool ParseVariables(List<Variable>? list) {
        var ct = string.Empty;

        if (list != null) {
            ct = list.ReplaceInText(OriginalText);
        }

        Pfad = ct;
        return ct == OriginalText;
    }

    public void Reload() {
        ÖffnePfad(txbPfad.Text);
    }

    public bool UrlExists(string nt) {
        foreach (var thisItem in lsbFiles.Item) {
            var tags = (List<string>)(thisItem.Tag);

            if (tags.TagGet("URL").ToUpper() == nt.ToUpper()) { return true; }
        }

        return false;
    }

    protected override void DrawControl(Graphics gr, States state) {
        Skin.Draw_Back_Transparent(gr, ClientRectangle, this);
    }

    protected override void OnVisibleChanged(System.EventArgs e) {
        AbortThumbs();
        base.OnVisibleChanged(e);
    }

    private void AbortThumbs() {
        while (ThumbGenerator.IsBusy) {
            if (ThumbGenerator.IsBusy && !ThumbGenerator.CancellationPending) { ThumbGenerator.CancelAsync(); }
            Develop.DoEvents();
        }
    }

    private bool AddThis(string path, bool isFile) {
        if (isFile) {
            if (path.FilePath() == "C:\\") { return false; }
            if (path.EndsWith(CryptoErkennung)) { return false; }
            return true;
        }

        path = (path.Trim("\\") + "\\").ToLower();

        if (path.EndsWith("\\$getcurrent\\")) { return false; }
        if (path.EndsWith("\\$recycle.bin\\")) { return false; }
        if (path.EndsWith("\\$recycle\\")) { return false; }
        if (path.EndsWith("\\system volume information\\")) { return false; }
        if (path.EndsWith("\\thumbs" + CryptoErkennung.ToLower() + "\\")) { return false; }

        //if (Path.EndsWith("\\Dokumente und Einstellungen\\")) { return false; }

        return true;
    }

    private void btnAddScreenShot_Click(object sender, System.EventArgs e) {
        var i = ScreenShot.GrabArea(null);

        var dateiPng = TempFile(txbPfad.Text.TrimEnd("\\"), "Screenshot " + DateTime.Now.ToString(Constants.Format_Date4), "PNG");
        i.Save(dateiPng, ImageFormat.Png);
        i.Dispose();
        CollectGarbage();
        txbPfad_Enter(null, null);
    }

    private void btnExplorerÖffnen_Click(object sender, System.EventArgs e) {
        ExecuteFile(txbPfad.Text);
    }

    private void btnZurück_Click(object? sender, System.EventArgs e) {
        ÖffnePfad(txbPfad.Text.PathParent(1));
    }

    private string ChangeFile(string filename, bool shoudbeCrypted) {
        if (string.IsNullOrEmpty(CryptMatrix)) { return filename; }

        var richtung = 10;
        if (shoudbeCrypted) { richtung = -10; }

        if (!AddThis(filename, true)) { return filename; }

        var T = CryptedFileName(filename, richtung);

        if (T == filename) { return filename; }

        var tmpOriFile = filename;
        while (FileExists(T)) {
            tmpOriFile = tmpOriFile.FilePath() + "\\" + tmpOriFile.FileNameWithoutSuffix() + "_." + tmpOriFile.FileSuffix();

            T = CryptedFileName(tmpOriFile, richtung);
        }

        var I = new FileInfo(filename);

        var crt = File.GetCreationTime(filename);
        var cht = File.GetLastWriteTime(filename);
        var attribute = I.Attributes;

        File.Move(filename, T);

        if (T.FileNameWithSuffix().StartsWith("#")) {
            attribute = FileAttributes.Hidden;
        } else {
            if (attribute.HasFlag(FileAttributes.Hidden)) {
                attribute = I.Attributes & ~FileAttributes.Hidden;
            }
        }

        try {
            File.SetAttributes(T, attribute);
            File.SetCreationTime(T, crt);
            File.SetLastWriteTime(T, cht);
        } catch { }

        return T;
    }

    private void CheckButtons() {
        txbPfad.Enabled = Enabled && string.IsNullOrEmpty(OriginalText);
        lsbFiles.Enabled = Enabled;
        btnAddScreenShot.Enabled = PathExists(txbPfad.Text);
        btnExplorerÖffnen.Enabled = PathExists(txbPfad.Text);
        btnZurück.Enabled = PathExists(txbPfad.Text);
    }

    private string CryoptErkennungsFile(string ofPath) {
        var p = ofPath.Trim("\\") + "\\";
        p = p + ofPath.Trim("\\").FileNameWithSuffix() + CryptoErkennung;
        return p;
    }

    private string CryptedFileName(string fullFile, int richtung) {
        var ret = string.Empty;

        var fileName = fullFile.FileNameWithSuffix();

        if (richtung < 0 && fileName.StartsWith("#")) { return fullFile; }
        if (richtung > 0 && !fileName.StartsWith("#")) { return fullFile; }

        for (var z = 0; z < fileName.Length; z++) {
            int pos;
            if (richtung > 0) {
                pos = CryptMatrix.IndexOf(fileName[z]);
            } else {
                pos = CryptMatrix.LastIndexOf(fileName[z]);
            }

            if (pos >= 0) {
                ret = ret + CryptMatrix[pos + richtung];
            } else {
                ret = ret + fileName[z];
            }
        }

        if (richtung < 0) { return fullFile.FilePath() + "#" + ret; }
        return fullFile.FilePath() + ret.Substring(1);
    }

    private Bitmap? GetThumbnail(string cryptoFileOnDisk, string realFileName) {
        Bitmap? thumbnail = null;

        if (realFileName.FileSuffix() == "URL") {
            //var addres = GetUrlFileDestination(cryptoFileOnDisk);

            //// Let's use
            //string url = ((string.IsNullOrEmpty(Request.Params["site"])) ? "blog.yemrekeskin.com" : Request.Params["site"]);
            //int width = ((string.IsNullOrEmpty(Request.Params["width"])) ? 1000 : int.Parse(Request.Params["width"]));
            //int height = ((string.IsNullOrEmpty(Request.Params["height"])) ? 940 : int.Parse(Request.Params["height"]));
            //int capWidth = ((string.IsNullOrEmpty(Request.Params["capWidth"])) ? 900 : int.Parse(Request.Params["capWidth"]));
            //int capHeight = ((string.IsNullOrEmpty(Request.Params["capHeight"])) ? 800 : int.Parse(Request.Params["capHeight"]));

            //string address = "http://" + url;

            //WebsiteThumbImage test = new WebsiteThumbImage();
            //test.BrowserWidth = 1000;
            //test.BrowserHeight = 940;
            //test.ThumbWidth = 900;
            //test.ThumbHeight = 800;
            //test.Url = addres;
            //test.Generate();
            //thumbnail = WebsiteThumbImage.GenerateScreenshot(addres, 800, 600);

            //thumbnail = WebsiteThumbImage.WebsiteThumbnailImageGenerator.GetWebSiteThumbnail(address, 1000, 940, 900, 800);

            //Response.ContentType = "image/jpeg";
            //thumbnail.Save(Response.OutputStream, ImageFormat.Jpeg);
        } else {
            var f = ChangeFile(cryptoFileOnDisk, false);
            thumbnail = WindowsThumbnailProvider.GetThumbnail(f, 64, 64, ThumbnailOptions.BiggerSizeOk);
            ChangeFile(f, true);
        }

        return thumbnail;
    }

    private bool IsCyrpted(string path) {
        return FileExists(CryoptErkennungsFile(path));
    }

    private void lsbFiles_ContextMenuInit(object sender, ContextMenuInitEventArgs e) {
        if (e.HotItem == null) { return; }

        var it = ((BitmapListItem)e.HotItem);
        var tags = (List<string>)(it.Tag);
        var tmpCrypted = Crypted;

        e.UserMenu.Add(ContextMenuComands.Ausschneiden, !tags.TagGet("Folder").FromPlusMinus() && tmpCrypted);
        e.UserMenu.Add(ContextMenuComands.Einfügen, tags.TagGet("CryptetFolder").FromPlusMinus() && !string.IsNullOrEmpty(_ausschneiden));
        e.UserMenu.AddSeparator();
        e.UserMenu.Add(ContextMenuComands.Umbenennen, FileExists(it.Internal));
        e.UserMenu.Add(ContextMenuComands.Löschen, FileExists(it.Internal) && tmpCrypted);
        e.UserMenu.AddSeparator();
        e.UserMenu.Add("Screenshot als Vorschau", "Screenshot", QuickImage.Get(ImageCode.Bild), FileExists(it.Internal) && tmpCrypted);
        e.UserMenu.AddSeparator();

        e.UserMenu.Add("Im Explorer öffnen", "Explorer", QuickImage.Get(ImageCode.Ordner));
        //TextListItem I1 = new TextListItem("#Relation", "Neue Beziehung hinzufügen", QuickImage.Get(ImageCode.Herz), true);
    }

    private void lsbFiles_ContextMenuItemClicked(object sender, ContextMenuItemClickedEventArgs e) {
        var it = ((BitmapListItem)e.HotItem);
        var tags = (List<string>)(it.Tag);

        switch (e.ClickedComand) {
            case "Löschen":
                var I = new FileInfo(it.Internal);
                //var attribute = I.Attributes;

                bool ask = Convert.ToBoolean(I.Attributes & FileAttributes.ReadOnly);

                if (FileDialogs.DeleteFile(I.FullName, ask)) {
                    FileDialogs.DeleteFile(ThumbFile(I.FullName), false);
                    Reload();
                }
                break;

            case "Explorer":
                ExecuteFile(it.Internal);
                break;

            case "Umbenennen":

                var n = tags.TagGet("UncryptetName");

                var nn = InputBox.Show("Neuer Name:", n.FileNameWithoutSuffix(), VarType.Text);

                if (n.FileNameWithoutSuffix() == n) { return; }

                nn = n.FilePath() + nn + "." + n.FileSuffix();

                if (n != it.Internal) { nn = CryptedFileName(nn, -10); }

                RenameFile(it.Internal, nn, true);

                if (FileExists(ThumbFile(it.Internal))) {
                    RenameFile(ThumbFile(it.Internal), ThumbFile(nn), true);
                }

                Reload();
                break;

            case "Screenshot":
                var bmp = ScreenShot.GrabArea(ParentForm());

                if (bmp.GrabedArea().Width < 1) { return; }
                var th = ThumbFile(it.Internal);
                FileDialogs.DeleteFile(th, false);
                bmp.Save(th, ImageFormat.Png);
                Reload();

                break;

            case "Ausschneiden":
                _ausschneiden = it.Internal;
                break;

            case "Einfügen":
                var ziel = it.Internal + "\\" + _ausschneiden.FileNameWithSuffix();

                if (FileExists(ziel)) {
                    MessageBox.Show("Datei existiert am Zielort bereits, abbruch.", ImageCode.Information);
                    return;
                }

                RenameFile(_ausschneiden, ziel, true);

                if (FileExists(ThumbFile(_ausschneiden))) {
                    RenameFile(ThumbFile(_ausschneiden), ThumbFile(ziel), true);
                }
                _ausschneiden = string.Empty;
                Reload();

                break;

            default:
                Develop.DebugPrint(e);
                break;
        }
    }

    private void lsbFiles_ItemClicked(object sender, BasicListItemEventArgs e) {
        if (File.Exists(e.Item.Internal)) {
            var tags = (List<string>)e.Item.Tag;

            switch (tags.TagGet("Suffix")) {
                case "URL":
                    LaunchBrowser(tags.TagGet("URL"));
                    return;

                case "JPG":
                case "PNG":
                case "BMP":
                case "TIF":
                    var pc = Image_FromFile(e.Item.Internal);
                    if (pc != null) {
                        var d = new PictureView((Bitmap)pc);
                        d.WindowState = System.Windows.Forms.FormWindowState.Maximized;
                        d.Show();
                    }

                    return;

                case "TXT":
                    ExecuteFile("Notepad.exe", e.Item.Internal);
                    return;
            }

            string tmp;

            if (FileExists(CryoptErkennungsFile(txbPfad.Text.TrimEnd("\\") + "\\"))) {
                tmp = CryptedFileName(e.Item.Internal, 10);
            } else {
                tmp = e.Item.Internal;
            }

            var x = GestStandardCommand("." + tmp.FileSuffix());

            if (string.IsNullOrEmpty(x)) {
                //System.Diagnostics.Process processx = new System.Diagnostics.Process();
                //System.Diagnostics.ProcessStartInfo startInfox = new System.Diagnostics.ProcessStartInfo();
                //startInfox.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                //startInfox.FileName = @"C:\Windows\System32\dllhost.exe";
                //startInfox.Arguments = e.Item.Internal;
                //processx.StartInfo = startInfox;
                //processx.Start();
                return;
            }

            x = x.Replace("%1", e.Item.Internal);
            var process = new System.Diagnostics.Process();
            var startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/C \"" + x + "\"";
            process.StartInfo = startInfo;
            process.Start();

            return;
        }

        ÖffnePfad(e.Item.Internal);
    }

    private void ÖffnePfad(string newPath) {
        if (_isLoading) { return; }
        _isLoading = true;

        var dropChanged = newPath.ToLower() != txbPfad.Text.ToLower();

        AbortThumbs();

        txbPfad.Text = newPath.TrimEnd("\\") + "\\";

        lsbFiles.Item.Clear();

        if (!PathExists(newPath)) {
            _isLoading = false;
            return;
        }

        var tmpCyrptedCrypted = Crypted;

        var allF = Directory.GetFiles(newPath, "*", SearchOption.TopDirectoryOnly);

        foreach (var thisString in allF) {
            if (AddThis(thisString, true)) // Prüfung 1, ob .cyo
            {
                var fileName = ChangeFile(thisString, tmpCyrptedCrypted);
                string suffix;
                var uncryptetName = fileName;
                BitmapListItem p;
                if (tmpCyrptedCrypted) {
                    uncryptetName = CryptedFileName(fileName, 10);
                    suffix = uncryptetName.FileSuffix().ToUpper();
                    p = new BitmapListItem(QuickImage.Get(uncryptetName.FileType(), 64), fileName, uncryptetName.FileNameWithoutSuffix());
                } else {
                    suffix = fileName.FileSuffix().ToUpper();
                    p = new BitmapListItem(QuickImage.Get(fileName.FileType(), 64), fileName, fileName.FileNameWithoutSuffix());
                }

                if (AddThis(uncryptetName, true)) // Prüfung 2, ob der ungecryptete NAme auch wirklich rein darf
                {
                    p.Padding = 6;

                    switch (_sort) {
                        case "Größe":
                            p.UserDefCompareKey = new FileInfo(fileName).Length.ToString(Constants.Format_Integer10);
                            break;

                        case "Erstelldatum":
                            p.UserDefCompareKey = new FileInfo(fileName).CreationTime.ToString("yyyy/MM/dd HH:mm:ss.fff");
                            break;

                        default:
                            p.UserDefCompareKey = Constants.SecondSortChar + p.Caption.ToUpper();
                            break;
                    }

                    var tags = new List<string>();
                    tags.TagSet("Folder", bool.FalseString);

                    tags.TagSet("Suffix", suffix);

                    tags.TagSet("UncryptetName", uncryptetName);
                    if (suffix == "URL") { tags.TagSet("URL", GetUrlFileDestination(fileName)); }

                    p.Tag = tags;

                    lsbFiles.Item.Add(p);
                }
            }

            if (dropChanged) { OnFolderChanged(); }
        }

        var allD = Directory.GetDirectories(newPath, "*", SearchOption.TopDirectoryOnly);
        foreach (var thisString in allD) {
            if (AddThis(thisString, false)) {
                BitmapListItem p;
                var tags = new List<string>();

                if (IsCyrpted(thisString)) {
                    p = new BitmapListItem(QuickImage.Get("Ordner|64|||FF0000"), thisString, thisString.FileNameWithoutSuffix());
                    tags.TagSet("CryptetFolder", bool.TrueString);
                } else {
                    p = new BitmapListItem(QuickImage.Get("Ordner|64"), thisString, thisString.FileNameWithoutSuffix());
                    tags.TagSet("CryptetFolder", bool.FalseString);
                }

                tags.TagSet("Folder", bool.TrueString);
                p.Padding = 10;
                p.UserDefCompareKey = Constants.FirstSortChar + thisString.ToUpper();
                p.Tag = tags;
                lsbFiles.Item.Add(p);
            }
        }

        lsbFiles.Item.Sort();

        StartThumbs();
        _isLoading = false;
    }

    private void OnFolderChanged() {
        FolderChanged?.Invoke(this, System.EventArgs.Empty);
    }

    private void StartThumbs() {
        AbortThumbs();
        ThumbGenerator.RunWorkerAsync();
    }

    private bool SubKeyExist(string subkey) {
        // Check if a Subkey exist
        var myKey = Registry.ClassesRoot.OpenSubKey(subkey);
        if (myKey == null) {
            return false;
        } else {
            return true;
        }
    }

    private string ThumbFile(string thisString) {
        var p = thisString.FilePath();
        var d = thisString.FileNameWithSuffix();
        return p + "Thumbs" + CryptoErkennung + "\\" + d;
    }

    private void ThumbGenerator_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e) {
        //ThumbGenerator.SetApartmentState

        var newPath = txbPfad.Text.Trim("\\") + "\\";
        if (!PathExists(newPath)) { return; }

        var cryptet = IsCyrpted(newPath);

        var allF = Directory.GetFiles(newPath, "*", SearchOption.TopDirectoryOnly);

        if (ThumbGenerator.CancellationPending) { return; }

        foreach (var thisString in allF) {
            if (ThumbGenerator.CancellationPending) { return; }
            if (AddThis(thisString, true)) // Prüfung 1, ob .cyo und auch unverschlüsselte
            {
                var feedBack = new List<object?> { thisString }; // Zeile 1, Dateiname auf Festplatte, ZEile 2 das Bild selbst

                if (cryptet) {
                    var rEadable = CryptedFileName(thisString, 10);

                    if (AddThis(rEadable, true)) {
                        var th = ThumbFile(thisString);

                        if (!FileExists(th)) {
                            var thumbnail = GetThumbnail(thisString, rEadable);
                            if (!PathExists(th.FilePath())) {
                                Directory.CreateDirectory(th.FilePath());
                            }

                            if (thumbnail != null) {
                                thumbnail.Save(th, ImageFormat.Png);
                                feedBack.Add(thumbnail);
                            }
                        } else {
                            feedBack.Add(Image_FromFile(th));
                        }
                    }

                    if (feedBack.Count == 1) {
                        // Verschlüsselte Dateinamen....schwierig ein echtes Bild zu kriegen...
                        feedBack.Add(QuickImage.Get(rEadable.FileType(), 64));
                    }
                }

                if (feedBack.Count == 1) {
                    feedBack.Add(WindowsThumbnailProvider.GetThumbnail(thisString, 64, 64, ThumbnailOptions.BiggerSizeOk));
                }

                ThumbGenerator.ReportProgress(0, feedBack);
            }
        }
    }

    private void ThumbGenerator_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e) {
        var gb = (List<object>)e.UserState;
        var file = (string)gb[0];

        var item = lsbFiles.Item[file];
        if (item == null) {
            return;
        }

        var bItem = (BitmapListItem)item;

        if (gb.Count == 2) {
            if (gb[1] is Bitmap bmp) {
                bItem.Bitmap = bmp;
                return;
            }
            if (gb[1] is BitmapExt bmp2) {
                bItem.Bitmap = bmp2;
                return;
            }
        }

        bItem.Bitmap = (Bitmap)Image_FromFile(ThumbFile(file));
    }

    private void txbPfad_Enter(object? sender, System.EventArgs? e) {
        ÖffnePfad(txbPfad.Text);
    }

    #endregion
}
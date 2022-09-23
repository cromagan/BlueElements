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
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueControls.ItemCollection.ItemCollectionList;
using BlueScript.Variables;
using CryptoExplorer;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using static BlueBasics.FileOperations;
using static BlueBasics.Generic;
using MessageBox = BlueControls.Forms.MessageBox;

namespace BlueControls.Controls;

public partial class FileBrowser : GenericControl, IAcceptVariableList//UserControl //
{
    #region Fields

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
        get => IsDisposed ? string.Empty : txbPfad.Text;
        set {
            if (IsDisposed) { return; }
            if (value != txbPfad.Text) {
                txbPfad.Text = value;
                txbPfad_Enter(null, null);
                CheckButtons();
            }
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
        if (IsDisposed) { return false; }

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
            return true;
        }

        path = (path.Trim("\\") + "\\").ToLower();

        if (path.EndsWith("\\$getcurrent\\")) { return false; }
        if (path.EndsWith("\\$recycle.bin\\")) { return false; }
        if (path.EndsWith("\\$recycle\\")) { return false; }
        if (path.EndsWith("\\system volume information\\")) { return false; }

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

    private void CheckButtons() {
        txbPfad.Enabled = Enabled && string.IsNullOrEmpty(OriginalText);
        lsbFiles.Enabled = Enabled;
        btnAddScreenShot.Enabled = PathExists(txbPfad.Text);
        btnExplorerÖffnen.Enabled = PathExists(txbPfad.Text);
        btnZurück.Enabled = PathExists(txbPfad.Text);
    }

    private void lsbFiles_ContextMenuInit(object sender, ContextMenuInitEventArgs e) {
        if (e.HotItem == null) { return; }

        var it = ((BitmapListItem)e.HotItem);
        var tags = (List<string>)(it.Tag);

        e.UserMenu.Add(ContextMenuComands.Ausschneiden, !tags.TagGet("Folder").FromPlusMinus());
        e.UserMenu.Add(ContextMenuComands.Einfügen, tags.TagGet("Folder").FromPlusMinus() && !string.IsNullOrEmpty(_ausschneiden));
        e.UserMenu.AddSeparator();
        e.UserMenu.Add(ContextMenuComands.Umbenennen, FileExists(it.Internal));
        e.UserMenu.Add(ContextMenuComands.Löschen, FileExists(it.Internal));
        e.UserMenu.AddSeparator();
        //e.UserMenu.Add("Screenshot als Vorschau", "Screenshot", QuickImage.Get(ImageCode.Bild), FileExists(it.Internal));
        //e.UserMenu.AddSeparator();

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

                var ask = Convert.ToBoolean(I.Attributes & FileAttributes.ReadOnly);

                if (FileDialogs.DeleteFile(I.FullName, ask)) {
                    //FileDialogs.DeleteFile(ThumbFile(I.FullName), false);
                    Reload();
                }
                break;

            case "Explorer":
                ExecuteFile(it.Internal);
                break;

            case "Umbenennen":
                var n = it.Internal;

                var nn = InputBox.Show("Neuer Name:", n.FileNameWithoutSuffix(), VarType.Text);

                if (n.FileNameWithoutSuffix() == n) { return; }

                nn = n.FilePath() + nn + "." + n.FileSuffix();

                RenameFile(it.Internal, nn, true);

                Reload();
                break;

            //case "Screenshot":
            //    var bmp = ScreenShot.GrabArea(ParentForm());

            //    if (bmp.GrabedArea().Width < 1) { return; }
            //    var th = ThumbFile(it.Internal);
            //    FileDialogs.DeleteFile(th, false);
            //    bmp.Save(th, ImageFormat.Png);
            //    Reload();

            //    break;

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

                //if (FileExists(ThumbFile(_ausschneiden))) {
                //    RenameFile(ThumbFile(_ausschneiden), ThumbFile(ziel), true);
                //}
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

            switch (e.Item.Internal.FileType()) {
                case FileFormat.Link:
                    LaunchBrowser(tags.TagGet("URL"));
                    return;

                case FileFormat.Image:
                    var l = new List<string>();

                    foreach (var thisS in lsbFiles.Item) {
                        if (thisS.Internal.FileType() == FileFormat.Image && FileExists(thisS.Internal)) {
                            l.Add(thisS.Internal);
                        }
                    }

                    var no = l.IndexOf(e.Item.Internal);
                    if (no > -1) {
                        var d = new PictureView(l, true, e.Item.Internal.FilePath(), no);
                        d.WindowState = System.Windows.Forms.FormWindowState.Maximized;
                        d.Show();
                    }

                    return;

                case FileFormat.Textdocument:
                    ExecuteFile("Notepad.exe", e.Item.Internal);
                    return;
            }

            //if (FileExists(CryoptErkennungsFile(txbPfad.Text.TrimEnd("\\") + "\\"))) {
            //    tmp = CryptedFileName(e.Item.Internal, 10);
            //} else {
            var tmp = e.Item.Internal;
            //}

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
        if (IsDisposed) { return; }
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

        var allF = Directory.GetFiles(newPath, "*", SearchOption.TopDirectoryOnly);

        foreach (var fileName in allF) {
            if (AddThis(fileName, true)) {
                //var suffix = fileName.FileSuffix().ToUpper();
                var p = new BitmapListItem(QuickImage.Get(fileName.FileType(), 64), fileName, fileName.FileNameWithoutSuffix());

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
                //tags.TagSet("Suffix", suffix);
                p.Tag = tags;
                lsbFiles.Item.Add(p);
            }

            if (dropChanged) { OnFolderChanged(); }
        }

        var allD = Directory.GetDirectories(newPath, "*", SearchOption.TopDirectoryOnly);
        foreach (var thisString in allD) {
            if (AddThis(thisString, false)) {
                var tags = new List<string>();

                //if (IsCyrpted(thisString)) {
                //    p = new BitmapListItem(QuickImage.Get("Ordner|64|||FF0000"), thisString, thisString.FileNameWithoutSuffix());
                //    tags.TagSet("CryptetFolder", bool.TrueString);
                //} else {
                var p = new BitmapListItem(QuickImage.Get("Ordner|64"), thisString, thisString.FileNameWithoutSuffix());
                //tags.TagSet("CryptetFolder", bool.FalseString);
                //}

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
        }

        return true;
    }

    private void ThumbGenerator_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e) {
        //ThumbGenerator.SetApartmentState

        var newPath = txbPfad.Text.Trim("\\") + "\\";
        if (!PathExists(newPath)) { return; }

        var allF = Directory.GetFiles(newPath, "*", SearchOption.TopDirectoryOnly);

        if (ThumbGenerator.CancellationPending) { return; }

        foreach (var thisString in allF) {
            if (ThumbGenerator.CancellationPending) { return; }
            if (AddThis(thisString, true)) // Prüfung 1, ob .cyo und auch unverschlüsselte
            {
                var feedBack = new List<object?> { thisString }; // Zeile 1, Dateiname auf Festplatte, ZEile 2 das Bild selbst

                //if (cryptet) {
                //    var rEadable = CryptedFileName(thisString, 10);

                //    if (AddThis(rEadable, true)) {
                //        var th = ThumbFile(thisString);

                //        if (!FileExists(th)) {
                //            var thumbnail = GetThumbnail(thisString, rEadable);
                //            if (!PathExists(th.FilePath())) {
                //                Directory.CreateDirectory(th.FilePath());
                //            }

                //            if (thumbnail != null) {
                //                thumbnail.Save(th, ImageFormat.Png);
                //                feedBack.Add(thumbnail);
                //            }
                //        } else {
                //            feedBack.Add(Image_FromFile(th));
                //        }
                //    }

                //    if (feedBack.Count == 1) {
                //        // Verschlüsselte Dateinamen....schwierig ein echtes Bild zu kriegen...
                //        feedBack.Add(QuickImage.Get(rEadable.FileType(), 64));
                //    }
                //}

                feedBack.Add(WindowsThumbnailProvider.GetThumbnail(thisString, 64, 64, ThumbnailOptions.BiggerSizeOk));

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

        bItem.Bitmap = null;//  (Bitmap)Image_FromFile(ThumbFile(file));
    }

    private void txbPfad_Enter(object? sender, System.EventArgs? e) {
        if (IsDisposed) { return; }
        ÖffnePfad(txbPfad.Text);
    }

    #endregion
}
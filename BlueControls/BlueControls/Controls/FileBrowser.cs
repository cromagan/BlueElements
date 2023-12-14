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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionList;
using BlueDatabase;
using BlueScript.Variables;
using Microsoft.Win32;
using static BlueBasics.Generic;
using static BlueBasics.IO;
using MessageBox = BlueControls.Forms.MessageBox;

namespace BlueControls.Controls;

public partial class FileBrowser : GenericControl, IControlAcceptSomething   //UserControl //
{
    #region Fields

    private string _lastcheck = string.Empty;
    private string _originalText = string.Empty;
    private string _sort = "Name";
    private string _todel = string.Empty;
    private FileSystemWatcher? _watcher;

    #endregion

    #region Constructors

    public FileBrowser() => InitializeComponent();

    #endregion

    #region Events

    public event EventHandler? FolderChanged;

    #endregion

    #region Properties

    public bool CreateDir { get; set; }
    public bool DeleteDir { get; set; }

    public new bool Enabled {
        get => base.Enabled; set {
            base.Enabled = value;
            CheckButtons(DirectoryExists(txbPfad.Text));
        }
    }

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public FilterCollection? FilterInput { get; set; }

    public bool FilterManualSeted { get; set; } = false;

    public string OriginalText {
        get => _originalText;
        set {
            _originalText = value;
            CheckButtons(DirectoryExists(txbPfad.Text));
        }
    }

    public List<IControlSendSomething> Parents { get; } = [];

    public string Pfad {
        get => IsDisposed ? string.Empty : txbPfad.Text;
        set {
            if (IsDisposed) { return; }
            if (value != txbPfad.Text) {
                txbPfad.Text = value;
                txbPfad_Enter(null, null);
                CheckButtons(false); // Macht der Worker wieder heile
            }
        }
    }

    public string Sort {
        get => _sort;
        set {
            if (_sort == value) { return; }
            _sort = value;
            Reload();
        }
    }

    #endregion

    #region Methods

    public void FilterInput_Changed(object sender, System.EventArgs e) {
        FilterInput = this.FilterOfSender();
        Invalidate();

        var row = FilterInput?.RowSingleOrNull;
        row?.CheckRowDataIfNeeded();
        ParseVariables(row?.LastCheckedEventArgs?.Variables);
        CreateWatcher();
    }

    public void FilterInput_Changing(object sender, System.EventArgs e) { RemoveWatcher(); }

    public string GestStandardCommand(string extension) {
        if (!SubKeyExist(extension)) { return string.Empty; }
        var mainkey = Registry.ClassesRoot.OpenSubKey(extension);
        var type = mainkey?.GetValue(""); // GetValue("") read the standard value of a key
        if (type == null) { return string.Empty; }
        mainkey = Registry.ClassesRoot.OpenSubKey(type.ToString());

        var shellKey = mainkey?.OpenSubKey("shell");
        if (shellKey == null) { return string.Empty; }

        var shellCommand = shellKey.GetValue(string.Empty);
        if (shellCommand == null) { return string.Empty; }

        var exekey = shellKey.OpenSubKey(shellCommand.ToString());
        if (exekey == null) { return string.Empty; }

        exekey = exekey.OpenSubKey("command");
        if (exekey == null) { return string.Empty; }

        return exekey.GetValue("").ToString();
    }

    public bool ParseVariables(VariableCollection? list) {
        if (IsDisposed) { return false; }

        var ct = string.Empty;

        if (list != null) {
            ct = list.ReplaceInText(OriginalText);
        }

        Pfad = ct;
        return ct == OriginalText;
    }

    public void Reload() => ÖffnePfad(txbPfad.Text);

    protected override void DrawControl(Graphics gr, States state) => Skin.Draw_Back_Transparent(gr, ClientRectangle, this);

    protected override void OnVisibleChanged(System.EventArgs e) {
        if (ThumbGenerator.IsBusy && !ThumbGenerator.CancellationPending) { ThumbGenerator.CancelAsync(); }
        lsbFiles.Item.Clear();
        _lastcheck = string.Empty;
        base.OnVisibleChanged(e);
    }

    private static bool AddThis(FileInfo fi) {
        if (fi.Attributes.HasFlag(FileAttributes.System)) { return false; }
        //if (fi.Attributes.HasFlag(FileAttributes.Hidden)) { return false; }

        if (fi.Exists) {
            if (fi.DirectoryName == null || fi.DirectoryName.FilePath() == "C:\\") { return false; }
            return true;
        }

        if (fi.DirectoryName != null) {
            var tmp = (fi.DirectoryName.Trim("\\") + "\\").ToLower();

            if (tmp.EndsWith("\\$getcurrent\\")) { return false; }
            if (tmp.EndsWith("\\$recycle.bin\\")) { return false; }
            if (tmp.EndsWith("\\$recycle\\")) { return false; }
            if (tmp.EndsWith("\\system volume information\\")) { return false; }
        }

        //if (Path.EndsWith("\\Dokumente und Einstellungen\\")) { return false; }

        return true;
    }

    private static bool SubKeyExist(string subkey) {
        // Check if a Subkey exist
        var myKey = Registry.ClassesRoot.OpenSubKey(subkey);
        return myKey != null;
    }

    private void btnAddScreenShot_Click(object sender, System.EventArgs e) {
        var i = ScreenShot.GrabArea(ParentForm());

        var dateiPng = TempFile(txbPfad.Text.TrimEnd("\\"), "Screenshot " + DateTime.Now.ToString(Constants.Format_Date4, CultureInfo.InvariantCulture), "PNG");
        i.Save(dateiPng, ImageFormat.Png);
        i.Dispose();
        CollectGarbage();
        txbPfad_Enter(null, null);
    }

    private void btnExplorerÖffnen_Click(object sender, System.EventArgs e) => ExecuteFile(txbPfad.Text);

    private void btnZurück_Click(object? sender, System.EventArgs e) => ÖffnePfad(txbPfad.Text.PathParent(1));

    private void CheckButtons(bool pfadexists) {
        txbPfad.Enabled = Enabled && string.IsNullOrEmpty(OriginalText);
        lsbFiles.Enabled = Enabled;
        btnAddScreenShot.Enabled = pfadexists;
        btnExplorerÖffnen.Enabled = pfadexists;
        btnZurück.Enabled = pfadexists;
    }

    private void chkFolder_Tick(object sender, System.EventArgs e) {
        if (IsDisposed) {
            chkFolder.Enabled = false;
            return;
        }

        if (ThumbGenerator.IsBusy) { return; }

        if (_lastcheck == txbPfad.Text) { return; }

        ThumbGenerator.RunWorkerAsync();
    }

    private void CreateWatcher() {
        if (!string.IsNullOrEmpty(txbPfad.Text) && DirectoryExists(txbPfad.Text)) {
            _watcher = new FileSystemWatcher(txbPfad.Text);
            _watcher.Changed += Watcher_Changed;
            _watcher.Created += Watcher_Created;
            _watcher.Deleted += Watcher_Deleted;
            _watcher.Renamed += Watcher_Renamed;
            _watcher.Error += Watcher_Error;
            _watcher.EnableRaisingEvents = true;
        }
    }

    private DragDropEffects CurrentState(DragEventArgs e) {
        if (!DirectoryExists(txbPfad.Text)) {
            return DragDropEffects.None;
        }

        if (!CanWriteInDirectory(txbPfad.Text)) {
            return DragDropEffects.None;
        }

        if ((ModifierKeys & Keys.Shift) == Keys.Shift && e.AllowedEffect.HasFlag(DragDropEffects.Move)) {
            return DragDropEffects.Move;
        }

        if (e.AllowedEffect.HasFlag(DragDropEffects.Copy)) {
            return DragDropEffects.Copy;
        }

        return DragDropEffects.None;
    }

    private void lsbFiles_ContextMenuInit(object sender, ContextMenuInitEventArgs e) {
        if (e.HotItem is not BitmapListItem it) { return; }
        //if (it.Tag is not List<string> tags) { return; }

        //_ = e.UserMenu.Add(ContextMenuCommands.Ausschneiden, !tags.TagGet("Folder").FromPlusMinus());
        //_ = e.UserMenu.Add(ContextMenuCommands.Einfügen, tags.TagGet("Folder").FromPlusMinus() && !string.IsNullOrEmpty(_ausschneiden));
        //_ = e.UserMenu.AddSeparator();
        _ = e.UserMenu.Add(ContextMenuCommands.Umbenennen, FileExists(it.KeyName));
        _ = e.UserMenu.Add(ContextMenuCommands.Löschen, FileExists(it.KeyName));
        _ = e.UserMenu.AddSeparator();
        _ = e.UserMenu.Add("Im Explorer öffnen", "Explorer", QuickImage.Get(ImageCode.Ordner));
    }

    private void lsbFiles_ContextMenuItemClicked(object sender, ContextMenuItemClickedEventArgs e) {
        if (e.HotItem is not BitmapListItem it) { return; }

        switch (e.ClickedCommand) {
            case "Löschen":
                var I = new FileInfo(it.KeyName);
                var silent = !I.Attributes.HasFlag(FileAttributes.ReadOnly);
                if (FileDialogs.DeleteFile(I.FullName, !silent)) {
                    Reload();
                }
                break;

            case "Explorer":
                _ = ExecuteFile(it.KeyName);
                break;

            case "Umbenennen":
                var n = it.KeyName;

                var nn = InputBox.Show("Neuer Name:", n.FileNameWithoutSuffix(), FormatHolder.Text);

                if (n.FileNameWithoutSuffix() == n) { return; }

                nn = n.FilePath() + nn + "." + n.FileSuffix();

                _ = MoveFile(it.KeyName, nn, true);

                Reload();
                break;

            //case "Ausschneiden":
            //    _ausschneiden = it.KeyName;
            //    break;

            //case "Einfügen":
            //    var ziel = it.KeyName + "\\" + _ausschneiden.FileNameWithSuffix();

            //    if (FileExists(ziel)) {
            //        _ = MessageBox.Show("Datei existiert am Zielort bereits, abbruch.", ImageCode.Information);
            //        return;
            //    }

            //    _ = MoveFile(_ausschneiden, ziel, true);

            //    //if (FileExists(ThumbFile(_ausschneiden))) {
            //    //    RenameFile(ThumbFile(_ausschneiden), ThumbFile(ziel), true);
            //    //}
            //    _ausschneiden = string.Empty;
            //    Reload();

            //    break;

            default:
                Develop.DebugPrint(e.ClickedCommand);
                break;
        }
    }

    private void lsbFiles_DragDrop(object sender, DragEventArgs e) {
        var tmp = CurrentState(e);

        if (tmp == DragDropEffects.None) { return; }

        List<string> files;

        try {
            var dropped = ((string[])e.Data.GetData(DataFormats.FileDrop));
            files = dropped.ToList();
        } catch {
            MessageBox.Show("Fehler bei Drag/Drop,<br>nichts wurde verändert.", ImageCode.Warnung, "Ok");
            return;
        }

        foreach (var thisfile in files) {
            if (!FileExists(thisfile)) {
                MessageBox.Show("Fehler bei Drag/Drop,<br>nichts wurde verändert.", ImageCode.Warnung, "Ok");
                return;
            }
        }

        foreach (var thisfile in files) {
            var f = TempFile(txbPfad.Text, thisfile.FileNameWithoutSuffix(), thisfile.FileSuffix());

            if (tmp == DragDropEffects.Copy) {
                _ = CopyFile(thisfile, f, true);
            }

            if (tmp == DragDropEffects.Move) {
                _ = MoveFile(thisfile, f, true);
            }
        }
    }

    private void lsbFiles_DragEnter(object sender, DragEventArgs e) {
        var tmp = CurrentState(e);

        if (e.AllowedEffect.HasFlag(tmp)) {
            e.Effect = tmp;
            return;
        }

        e.Effect = DragDropEffects.None;
    }

    private void lsbFiles_ItemClicked(object sender, AbstractListItemEventArgs e) {
        if (File.Exists(e.Item.KeyName)) {
            switch (e.Item.KeyName.FileType()) {
                case FileFormat.Link:
                    if (e.Item.Tag is not List<string> tags) { return; }
                    LaunchBrowser(tags.TagGet("URL"));
                    return;

                case FileFormat.Image:
                    var l = new List<string>();
                    foreach (var thisS in lsbFiles.Item) {
                        if (thisS.KeyName.FileType() == FileFormat.Image && FileExists(thisS.KeyName)) {
                            l.Add(thisS.KeyName);
                        }
                    }

                    var no = l.IndexOf(e.Item.KeyName);
                    if (no > -1) {
                        var d = new PictureView(l, true, e.Item.KeyName.FilePath(), no) {
                            WindowState = FormWindowState.Maximized
                        };
                        d.Show();
                    }

                    return;

                case FileFormat.Textdocument:
                    _ = ExecuteFile("Notepad.exe", e.Item.KeyName);
                    return;
            }

            var tmp = e.Item.KeyName;

            var x = GestStandardCommand("." + tmp.FileSuffix());

            if (string.IsNullOrEmpty(x)) { return; }

            x = x.Replace("%1", e.Item.KeyName);
            var process = new Process();
            var startInfo = new ProcessStartInfo {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "cmd.exe",
                Arguments = "/C \"" + x + "\""
            };
            process.StartInfo = startInfo;
            _ = process.Start();

            return;
        }

        ÖffnePfad(e.Item.KeyName);
    }

    private void ÖffnePfad(string newPath) {
        if (IsDisposed) { return; }

        RemoveWatcher();
        if (ThumbGenerator.IsBusy && !ThumbGenerator.CancellationPending) { ThumbGenerator.CancelAsync(); }

        newPath = newPath.TrimEnd("\\") + "\\";
        var dropChanged = !String.Equals(newPath, txbPfad.Text, StringComparison.OrdinalIgnoreCase);
        txbPfad.Text = newPath;
        lsbFiles.Item.Clear();

        if (dropChanged) { OnFolderChanged(); }
        _lastcheck = string.Empty;
    }

    private void OnFolderChanged() => FolderChanged?.Invoke(this, System.EventArgs.Empty);

    private void RemoveWatcher() {
        if (_watcher != null) {
            _watcher.EnableRaisingEvents = false;
            _watcher.Changed -= Watcher_Changed;
            _watcher.Created -= Watcher_Created;
            _watcher.Deleted -= Watcher_Deleted;
            _watcher.Renamed -= Watcher_Renamed;
            _watcher.Error -= Watcher_Error;
            _watcher?.Dispose();
            _watcher = null;
        }
    }

    private void ThumbGenerator_DoWork(object sender, DoWorkEventArgs e) {
        var newPath = txbPfad.Text.Trim("\\") + "\\";

        if (_lastcheck == newPath) { return; }
        if (!newPath.IsFormat(FormatHolder.Filepath)) { return; }

        _lastcheck = newPath;

        if (!Visible) { return; }

        #region  Buttons ausschalten

        var feedBack = new List<object?> { "Buttons&Watcher", false };
        ThumbGenerator.ReportProgress(0, feedBack);

        #endregion

        #region  Leeres Verzeichniss löschen

        if (DeleteDir && Enabled) {
            if (DirectoryExists(_todel)) {
                var emd = Directory.GetDirectories(_todel, "*", SearchOption.TopDirectoryOnly);
                if (emd.Length == 0) {
                    var emf = Directory.GetFiles(_todel, "*", SearchOption.TopDirectoryOnly);
                    if (emf.Length == 0) {
                        _ = DeleteDir(_todel, false);
                    }
                }
            }
        }

        #endregion

        _todel = _lastcheck;

        #region Verzeichniss erstellen oder raus hier

        if (!DirectoryExists(newPath)) {
            if (CreateDir && Enabled) {
                _ = Directory.CreateDirectory(newPath);
            } else {
                return;
            }
            if (!DirectoryExists(newPath)) { return; }
        }

        #endregion

        var allF = Directory.GetFiles(newPath, "*", SearchOption.TopDirectoryOnly);

        if (ThumbGenerator.CancellationPending || _lastcheck != newPath) { return; }

        #region  Buttons und Watcher einschalten

        feedBack = ["Buttons&Watcher", true];
        ThumbGenerator.ReportProgress(0, feedBack);

        #endregion

        #region  Datei Objekte selbst flott erstellen

        foreach (var fileName in allF) {
            var fi = new FileInfo(fileName);

            if (AddThis(fi)) {
                if (ThumbGenerator.CancellationPending || _lastcheck != newPath) { return; }

                var p = new BitmapListItem(QuickImage.Get(fileName.FileType(), 64), fileName, fileName.FileNameWithoutSuffix()) {
                    Padding = 6
                };

                switch (_sort) {
                    case "Größe":
                        p.UserDefCompareKey = fi.Length.ToString(Constants.Format_Integer10);
                        break;

                    case "Erstelldatum":
                        p.UserDefCompareKey = fi.CreationTime.ToString("yyyy/MM/dd HH:mm:ss.fff");
                        break;

                    default:
                        p.UserDefCompareKey = Constants.SecondSortChar + p.Caption.ToUpper();
                        break;
                }

                var tags = new List<string>();
                tags.TagSet("Folder", bool.FalseString);
                p.Tag = tags;

                feedBack = ["Add", p];
                ThumbGenerator.ReportProgress(1, feedBack);
            }
        }

        #endregion

        #region Verzeichniss objekte Flott erstellen

        var allD = Directory.GetDirectories(newPath, "*", SearchOption.TopDirectoryOnly);
        foreach (var thisString in allD) {
            var fi = new FileInfo(thisString);
            if (AddThis(fi)) {
                var tags = new List<string>();
                var p = new BitmapListItem(QuickImage.Get("Ordner|64"), thisString, thisString.FileNameWithoutSuffix());
                tags.TagSet("Folder", bool.TrueString);
                p.Padding = 10;
                p.UserDefCompareKey = Constants.FirstSortChar + thisString.ToUpper();
                p.Tag = tags;
                feedBack = ["Add", p];
                ThumbGenerator.ReportProgress(1, feedBack);
            }
        }

        #endregion

        #region  Vorschau Bilder erstellen

        foreach (var thisString in allF) {
            if (ThumbGenerator.CancellationPending || _lastcheck != newPath) { return; }
            var fi = new FileInfo(thisString);
            if (AddThis(fi)) {
                feedBack = ["Image", thisString, WindowsThumbnailProvider.GetThumbnail(thisString, 64, 64, ThumbnailOptions.BiggerSizeOk)];
                if (ThumbGenerator.CancellationPending || _lastcheck != newPath) { return; }
                ThumbGenerator.ReportProgress(2, feedBack);
            }
        }

        #endregion
    }

    private void ThumbGenerator_ProgressChanged(object sender, ProgressChangedEventArgs e) {
        var gb = (List<object>)e.UserState;

        var com = ((string)gb[0]).ToLower();

        switch (com) {
            case "buttons&watcher":

                if ((bool)gb[1]) {
                    CheckButtons(true);
                    CreateWatcher();
                } else {
                    CheckButtons(false);
                    RemoveWatcher();
                }

                break;

            case "add":
                var ob = (BitmapListItem)gb[1];
                lsbFiles.Item.Add(ob);
                return;

            case "image":
                var file = (string)gb[1];
                var item = lsbFiles.Item[file];
                if (item == null) { return; }
                var bItem = (BitmapListItem)item;
                if (gb[2] is Bitmap bmp) {
                    bItem.Bitmap = bmp;
                    return;
                }
                bItem.Bitmap = null;
                break;
        }
    }

    private void ThumbGenerator_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) { }

    private void txbPfad_Enter(object? sender, System.EventArgs? e) {
        if (IsDisposed) { return; }
        ÖffnePfad(txbPfad.Text);
    }

    private void Watcher_Changed(object sender, FileSystemEventArgs e) {
        var fi = new FileInfo(e.Name);
        if (!AddThis(fi)) { return; }

        //if (e.Name.Equals("Thumbs.db", StringComparison.OrdinalIgnoreCase)) { return; }
        //if(e.ChangeType == WatcherChangeTypes.Changed) { return; }
        txbPfad_Enter(null, System.EventArgs.Empty);
    }

    private void Watcher_Created(object sender, FileSystemEventArgs e) => txbPfad_Enter(null, System.EventArgs.Empty);

    private void Watcher_Deleted(object sender, FileSystemEventArgs e) => txbPfad_Enter(null, System.EventArgs.Empty);

    /// <summary>
    /// Im Verzeichnis wurden zu viele Änderungen gleichzeitig vorgenommen...
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Watcher_Error(object sender, ErrorEventArgs e) => txbPfad_Enter(null, System.EventArgs.Empty);

    private void Watcher_Renamed(object sender, RenamedEventArgs e) => txbPfad_Enter(null, System.EventArgs.Empty);

    #endregion
}
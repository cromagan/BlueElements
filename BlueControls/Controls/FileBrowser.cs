// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
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
using BlueBasics.Classes;
using BlueBasics.ClassesStatic;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.Classes;
using BlueControls.Classes.ItemCollectionList;
using BlueControls.Controls.ConnectedFormula;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Windows.Forms;
using static BlueBasics.ClassesStatic.Generic;
using static BlueBasics.ClassesStatic.IO;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Controls;

public sealed partial class FileBrowser : GenericControlReciver   //UserControl //
{
    #region Fields

    private string _directory = string.Empty;
    private string _filter = "*";
    private string _todel = string.Empty;
    private System.IO.FileSystemWatcher? _watcher;
    private string _workinDir = string.Empty;

    #endregion

    #region Constructors

    public FileBrowser() : base(false, false, false) => InitializeComponent();

    #endregion

    #region Events

    public event EventHandler<AbstractListItemEventArgs>? ItemClicked;

    #endregion

    #region Properties

    [DefaultValue(true)]
    public bool AllowDragDrop { get; set; } = true;

    [DefaultValue(true)]
    public bool AllowEdit { get; set; } = true;

    [DefaultValue(true)]
    public bool AllowScreenshots { get; set; } = true;

    [DefaultValue(false)]
    public bool CreateDir { get; set; } = false;

    [DefaultValue(false)]
    public bool DeleteDir { get; set; } = false;

    public string Directory {
        get => IsDisposed ? string.Empty : _directory;
        set {
            if (IsDisposed) { return; }
            value = value.TrimEnd("\\") + "\\";

            if (value == "\\") { value = string.Empty; }

            if (value == _directory) { return; }

            _directory = value;
            txbPfad.Text = _directory;

            ReloadDirectory();
        }
    }

    public string DirectoryMin {
        get => IsDisposed ? string.Empty : field;
        set {
            if (IsDisposed) { return; }
            value = value.ToLowerInvariant().TrimEnd("\\") + "\\";

            if (value == "\\") { value = string.Empty; }

            //if (value == _directoryMin) { return; }

            field = value;
        }
    } = string.Empty;

    [DefaultValue(true)]
    public bool DoDefaultHandling { get; set; } = true;

    public new bool Enabled {
        get => base.Enabled; set {
            base.Enabled = value;
            CheckButtons(false);
        }
    }

    public string Filter {
        get => IsDisposed ? string.Empty : _filter;
        set {
            if (IsDisposed) { return; }

            if (string.IsNullOrEmpty(value)) { value = "*"; }

            value = value.ToLowerInvariant();
            if (value == _filter) { return; }

            _filter = value;

            ReloadDirectory();
        }
    }

    public string Sort {
        get;
        set {
            if (field == value) { return; }
            field = value;
            ReloadDirectory();
        }
    } = "Name";

    public string Var_Directory {
        get;
        set {
            if (field == value) { return; }
            field = value;
            ReloadDirectory();
        }
    } = string.Empty;

    public string Var_DirectoryMin {
        get;
        set {
            if (field == value) { return; }
            field = value;
            ReloadDirectory();
        }
    } = string.Empty;

    #endregion

    #region Methods

    public static string GetStandardCommand(string extension) {
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
        return exekey == null ? string.Empty : exekey.GetValue("").ToString();
    }

    /// <summary>
    /// Verwendete Ressourcen bereinigen.
    /// </summary>
    /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
    protected override void Dispose(bool disposing) {
        if (disposing) {
            ReloadDirectory();
        }

        base.Dispose(disposing);
    }

    protected override void DrawControl(Graphics gr, States state) {
        if (IsDisposed) { return; }
        base.DrawControl(gr, state);
        Skin.Draw_Back_Transparent(gr, ClientRectangle, this);
    }

    protected override void HandleChangesNow() {
        base.HandleChangesNow();
        if (IsDisposed) { return; }
        if (RowsInputChangedHandled && FilterInputChangedHandled) { return; }

        DoInputFilter(null, false);
        RowsInputChangedHandled = true;

        if (Parents.Count > 0) {
            var tmpDirectory = string.Empty;
            var tmpDirectoryMin = string.Empty;

            if (RowSingleOrNull()?.CheckRow().PrepareFormulaFeedback.Variables is { } list) {
                tmpDirectory = list.ReplaceInText(Var_Directory);
                tmpDirectoryMin = list.ReplaceInText(Var_DirectoryMin);
            }

            Directory = tmpDirectory;
            DirectoryMin = tmpDirectoryMin;
        }
    }

    protected override void OnVisibleChanged(System.EventArgs e) {
        base.OnVisibleChanged(e);
        ReloadDirectory();
    }

    private static bool AddThis(System.IO.FileInfo? fi) {
        if (fi == null) { return false; }
        if (fi.Attributes.HasFlag(System.IO.FileAttributes.System)) { return false; }
        //if (fi.Attributes.HasFlag(FileAttributes.Hidden)) { return false; }

        if (fi.Exists) {
            return fi.DirectoryName != null && fi.DirectoryName.FilePath() != "C:\\";
        }

        if (fi.DirectoryName != null) {
            var tmp = (fi.DirectoryName.Trim("\\") + "\\").ToLowerInvariant();

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
        if (!AllowScreenshots) { return; }

        var i = ScreenShot.GrabArea(ParentForm());

        if (i.Area is not { } bmp) { return; }

        var dateiPng = TempFile(_directory.TrimEnd("\\"), "Screenshot " + DateTime.Now.ToString4(), "PNG");
        bmp.Save(dateiPng, ImageFormat.Png);

        CollectGarbage();

        ReloadDirectory();
    }

    private void btnExplorerÖffnen_Click(object sender, System.EventArgs e) => ExecuteFile(_directory);

    private void btnZurück_Click(object? sender, System.EventArgs e) => Directory = Directory.PathParent(1);

    private void CheckButtons(bool pfadexists) {
        txbPfad.Enabled = string.IsNullOrEmpty(DirectoryMin) && Enabled && string.IsNullOrEmpty(Var_Directory);
        lsbFiles.Enabled = Enabled && pfadexists;
        btnAddScreenShot.Enabled = AllowScreenshots && Enabled && pfadexists;

        btnExplorerÖffnen.Enabled = pfadexists && Enabled;

        if (string.IsNullOrEmpty(DirectoryMin)) {
            btnZurück.Enabled = Enabled && pfadexists;
        } else {
            if (Directory.Equals(DirectoryMin, StringComparison.OrdinalIgnoreCase)) {
                btnZurück.Enabled = false;
            } else {
                btnZurück.Enabled = Enabled && pfadexists;
            }
        }
    }

    private string CheckCode() {
        if (IsDisposed) { return string.Empty; }

        try {
            if (InvokeRequired) {
                return (string)Invoke(new Func<string>(CheckCode));
            }

            return _directory + "?" + Visible + "?" + _filter + "?" + Sort + "?" + FilterInputChangedHandled + "?" + RowsInputChangedHandled;
        } catch {
            // Manchmal verworfen
            return CheckCode();
        }
    }

    private void chkFolder_Tick(object sender, System.EventArgs e) {
        if (IsDisposed) {
            chkFolder.Enabled = false;
            return;
        }

        if (ThumbGenerator.IsBusy || !Visible) { return; }

        if (ThumbGenerator.CancellationPending) { return; }

        if (_workinDir == CheckCode()) { return; }

        ThumbGenerator.RunWorkerAsync();
    }

    private void Contextmenu_Delete(object sender, ObjectEventArgs e) {
        if (e.Data is not BitmapListItem it) { return; }
        if (!AllowEdit) { return; }

        var I = GetFileInfo(it.KeyName);
        if (I == null) {
            ReloadDirectory();
            return;
        }

        var silent = !I.Attributes.HasFlag(System.IO.FileAttributes.ReadOnly);
        if (FileDialogs.DeleteFile(I.FullName, !silent)) {
            ReloadDirectory();
        }
    }

    private void Contextmenu_OpenExplorer(object sender, ObjectEventArgs e) {
        if (e.Data is not BitmapListItem it) { return; }
        if (!AllowEdit) { return; }

        ExecuteFile(it.KeyName);
    }

    private void Contextmenu_Rename(object sender, ObjectEventArgs e) {
        if (e.Data is not BitmapListItem it) { return; }
        if (!AllowEdit) { return; }

        var n = it.KeyName;

        var nn = InputBox.Show("Neuer Name:", n.FileNameWithoutSuffix(), FormatHolder.Text);

        if (n.FileNameWithoutSuffix() == n) { return; }

        nn = n.FilePath() + nn + "." + n.FileSuffix();

        MoveFile(it.KeyName, nn, true);

        ReloadDirectory();
    }

    private void CreateWatcher() {
        if (Disposing || IsDisposed) { return; }

        if (InvokeRequired) {
            try {
                Invoke(new Action(CreateWatcher));
                return;
            } catch {
                // Kann dank Multitasking disposed sein
                Develop.AbortAppIfStackOverflow();
                CreateWatcher();
                return;
            }
        }

        if (!string.IsNullOrEmpty(_directory) && DirectoryExists(_directory)) {
            _watcher = new System.IO.FileSystemWatcher(_directory) {
                InternalBufferSize = 64 * 1024,
                IncludeSubdirectories = false,
                EnableRaisingEvents = true,
            };

            _watcher.Changed += Watcher_Changed;
            _watcher.Created += Watcher_Created;
            _watcher.Deleted += Watcher_Deleted;
            _watcher.Renamed += Watcher_Renamed;
            _watcher.Error += Watcher_Error;
        }
    }

    private DragDropEffects CurrentState(DragEventArgs e) {
        if (!AllowDragDrop) { return DragDropEffects.None; }
        if (!DirectoryExists(_directory)) { return DragDropEffects.None; }

        var opr = CanWriteInDirectory(_directory);
        if (!string.IsNullOrEmpty(opr)) { return DragDropEffects.None; }

        if ((ModifierKeys & Keys.Shift) == Keys.Shift && e.AllowedEffect.HasFlag(DragDropEffects.Move)) { return DragDropEffects.Move; }

        return e.AllowedEffect.HasFlag(DragDropEffects.Copy) ? DragDropEffects.Copy : DragDropEffects.None;
    }

    private void lsbFiles_ContextMenuInit(object sender, ContextMenuInitEventArgs e) {
        if (e.HotItem is not BitmapListItem it) { return; }
        if (!AllowEdit) { return; }

        e.ContextMenu.Add(ItemOf("Umbenennen", QuickImage.Get(ImageCode.Stift), Contextmenu_Rename, e.HotItem, FileExists(it.KeyName), string.Empty));
        e.ContextMenu.Add(ItemOf("Löschen", QuickImage.Get(ImageCode.Kreuz), Contextmenu_Delete, e.HotItem, FileExists(it.KeyName), string.Empty));
        e.ContextMenu.Add(Separator());
        e.ContextMenu.Add(ItemOf("Im Explorer öffnen", QuickImage.Get(ImageCode.Ordner), Contextmenu_OpenExplorer, e.HotItem, true, string.Empty));
    }

    private void lsbFiles_DragDrop(object sender, DragEventArgs e) {
        if (!AllowDragDrop) { return; }

        var tmp = CurrentState(e);

        if (tmp == DragDropEffects.None) { return; }

        List<string> files;

        try {
            var dropped = (string[])e.Data.GetData(DataFormats.FileDrop);
            files = [.. dropped];
        } catch {
            Forms.MessageBox.Show("Fehler bei Drag/Drop,<br>nichts wurde verändert.", ImageCode.Warnung, "Ok");
            return;
        }

        foreach (var thisfile in files) {
            if (!FileExists(thisfile)) {
                Forms.MessageBox.Show("Fehler bei Drag/Drop,<br>nichts wurde verändert.", ImageCode.Warnung, "Ok");
                return;
            }
        }

        foreach (var thisfile in files) {
            var f = TempFile(_directory, thisfile.FileNameWithoutSuffix(), thisfile.FileSuffix());

            if (tmp == DragDropEffects.Copy) {
                FileCopy(thisfile, f, true);
            }

            if (tmp == DragDropEffects.Move) {
                MoveFile(thisfile, f, true);
            }
        }
    }

    private void lsbFiles_DragEnter(object sender, DragEventArgs e) {
        if (!AllowDragDrop) { e.Effect = DragDropEffects.None; return; }

        var tmp = CurrentState(e);

        if (e.AllowedEffect.HasFlag(tmp)) {
            e.Effect = tmp;
            return;
        }

        e.Effect = DragDropEffects.None;
    }

    private void lsbFiles_ItemClicked(object sender, AbstractListItemEventArgs e) {
        OnItemClicked(e);

        if (!DoDefaultHandling) { return; }

        if (FileExists(e.Item.KeyName)) {
            switch (e.Item.KeyName.FileType()) {
                case FileFormat.Link:
                    if (e.Item.Tag is not List<string> tags) { return; }
                    LaunchBrowser(tags.TagGet("URL"));
                    return;

                case FileFormat.Image:
                    var l = new List<string>();
                    foreach (var thisS in lsbFiles.Items) {
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
                    ExecuteFile("Notepad.exe", e.Item.KeyName);
                    return;
            }

            var tmp = e.Item.KeyName;

            var x = GetStandardCommand("." + tmp.FileSuffix());

            if (string.IsNullOrEmpty(x)) { return; }

            x = x.Replace("%1", e.Item.KeyName);
            var process = new Process();
            var startInfo = new ProcessStartInfo {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "cmd.exe",
                Arguments = "/C \"" + x + "\""
            };
            process.StartInfo = startInfo;
            process.Start();

            return;
        }

        Directory = e.Item.KeyName;
    }

    private void OnItemClicked(AbstractListItemEventArgs e) => ItemClicked?.Invoke(this, e);

    private void ReloadDirectory() {
        if (InvokeRequired) {
            try {
                Invoke(new Action(ReloadDirectory));
                return;
            } catch {
                // Kann dank Multitasking disposed sein
                Develop.AbortAppIfStackOverflow();
                ReloadDirectory();
                return;
            }
        }

        try {
            if (_watcher != null) {
                _watcher.EnableRaisingEvents = false;
                _watcher.Changed -= Watcher_Changed;
                _watcher.Created -= Watcher_Created;
                _watcher.Deleted -= Watcher_Deleted;
                _watcher.Renamed -= Watcher_Renamed;
                _watcher.Error -= Watcher_Error;

                var watcherToDispose = _watcher;
                _watcher = null;

                Task.Run(watcherToDispose.Dispose);
            }
        } catch { }

        if (ThumbGenerator is { IsBusy: true, CancellationPending: false }) { ThumbGenerator.CancelAsync(); }
        lsbFiles.ItemClear();

        _workinDir = string.Empty;
        Invalidate();
    }

    private void ThumbGenerator_DoWork(object sender, DoWorkEventArgs e) {
        if (IsDisposed) { return; }

        var newCheckCode = CheckCode();

        if (newCheckCode == _workinDir) { return; }

        _workinDir = newCheckCode;

        var dir = _directory;

        if (!dir.IsFormat(FormatHolder.Filepath)) { return; }

        if (!Visible) { return; }

        #region  Buttons ausschalten

        List<object?> feedBack = ["Buttons&Watcher", false];
        ThumbGenerator.ReportProgress(0, feedBack);

        #endregion

        #region  Leeres Verzeichnis löschen

        if (DeleteDir && Enabled && _todel != dir) {
            if (DirectoryExists(_todel)) {
                var emd = GetDirectories(_todel, "*", System.IO.SearchOption.TopDirectoryOnly);
                if (emd.Length == 0) {
                    var emf = GetFiles(_todel, "*", System.IO.SearchOption.TopDirectoryOnly);
                    if (emf.Length == 0) {
                        DeleteDir(_todel, false);
                    }
                }
            }
        }

        #endregion

        _todel = dir;

        #region Verzeichnis erstellen oder raus hier

        if (!DirectoryExists(dir)) {
            if (CreateDir && Enabled && RowsInputChangedHandled && FilterInputChangedHandled &&
                !string.IsNullOrEmpty(dir) && dir.IsFormat(FormatHolder.Filepath)) {
                CreateDirectory(dir);
            } else {
                return;
            }
            if (!DirectoryExists(dir)) { return; }
        }

        #endregion

        var allF = GetFiles(dir, _filter, System.IO.SearchOption.TopDirectoryOnly);

        if (ThumbGenerator.CancellationPending || newCheckCode != CheckCode()) { return; }

        #region  Buttons und Watcher einschalten

        feedBack = ["Buttons&Watcher", true];
        ThumbGenerator.ReportProgress(0, feedBack);

        #endregion

        #region  Datei Objekte selbst flott erstellen

        foreach (var fileName in allF) {
            var fi = GetFileInfo(fileName);

            if (AddThis(fi)) {
                if (ThumbGenerator.CancellationPending || newCheckCode != CheckCode()) { return; }

                var bli = new BitmapListItem(QuickImage.Get(fileName.FileType(), 64), fileName, fileName.FileNameWithoutSuffix(), string.Empty) {
                    Padding = 6
                };

                switch (Sort) {
                    case "Größe":
                        bli.UserDefCompareKey = ((int)fi.Length).ToString10();
                        break;

                    case "Erstelldatum":
                        bli.UserDefCompareKey = fi.CreationTime.ToString9();
                        break;

                    default:
                        bli.UserDefCompareKey = Constants.SecondSortChar + bli.Caption.ToUpperInvariant();
                        break;
                }

                var tags = new List<string>();
                tags.TagSet("Folder", bool.FalseString);
                bli.Tag = tags;

                feedBack = ["Add", bli];
                ThumbGenerator.ReportProgress(1, feedBack);
            }
        }

        #endregion

        #region Verzeichnis objekte Flott erstellen

        var allD = GetDirectories(dir, "*", System.IO.SearchOption.TopDirectoryOnly);
        foreach (var thisString in allD) {
            var fi = GetFileInfo(thisString);
            if (AddThis(fi)) {
                var tags = new List<string>();
                var bli = new BitmapListItem(QuickImage.Get("Ordner|64"), thisString, thisString.FileNameWithoutSuffix(), string.Empty);
                tags.TagSet("Folder", bool.TrueString);
                bli.Padding = 10;
                bli.UserDefCompareKey = Constants.FirstSortChar + thisString.ToUpperInvariant();
                bli.Tag = tags;
                feedBack = ["Add", bli];
                ThumbGenerator.ReportProgress(1, feedBack);
            }
        }

        #endregion

        #region  Vorschau Bilder erstellen

        foreach (var thisString in allF) {
            if (ThumbGenerator.CancellationPending || newCheckCode != CheckCode()) { return; }
            var fi = GetFileInfo(thisString);
            if (AddThis(fi)) {
                feedBack = ["Image", thisString, WindowsThumbnailProvider.GetThumbnail(thisString, 64, 64, ThumbnailOptions.BiggerSizeOk)];
                if (ThumbGenerator.CancellationPending || newCheckCode != CheckCode()) { return; }
                ThumbGenerator.ReportProgress(2, feedBack);
            }
        }

        #endregion
    }

    private void ThumbGenerator_ProgressChanged(object sender, ProgressChangedEventArgs e) {
        if (IsDisposed) { return; }

        var gb = (List<object>)e.UserState;

        var com = ((string)gb[0]).ToLowerInvariant();

        switch (com) {
            case "buttons&watcher":

                if ((bool)gb[1]) {
                    CheckButtons(true);
                    CreateWatcher();
                } else {
                    CheckButtons(false);
                }

                break;

            case "add":
                var ob = (BitmapListItem)gb[1];
                if (_workinDir != CheckCode()) { return; }

                lsbFiles.Remove(ob.KeyName);

                lsbFiles.ItemAdd(ob);
                return;

            case "image":
                var file = (string)gb[1];
                var item = lsbFiles[file];
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

    private void txbPfad_Enter(object? sender, System.EventArgs? e) {
        if (IsDisposed) { return; }
        Directory = txbPfad.Text;
    }

    private void Watcher_Changed(object sender, System.IO.FileSystemEventArgs e) {
        var fi = GetFileInfo(e.Name);
        AddThis(fi);

        //if (e.Name.Equals("Thumbs.db", StringComparison.OrdinalIgnoreCase)) { return; }
        //if(e.ChangeType == WatcherChangeTypes.Changed) { return; }
    }

    private void Watcher_Created(object sender, System.IO.FileSystemEventArgs e) => ReloadDirectory();

    private void Watcher_Deleted(object sender, System.IO.FileSystemEventArgs e) => ReloadDirectory();

    /// <summary>
    /// Im Verzeichnis wurden zu viele Änderungen gleichzeitig vorgenommen...
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Watcher_Error(object sender, System.IO.ErrorEventArgs e) => ReloadDirectory();

    private void Watcher_Renamed(object sender, System.IO.RenamedEventArgs e) => ReloadDirectory();

    #endregion
}
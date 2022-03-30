using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.ItemCollection.ItemCollectionList;

using BlueBasics.Enums;

using Microsoft.Win32;

using System;
using System.Collections.Generic;

using System.Diagnostics;

using System.Drawing;
using System.IO;

using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

using BlueBasics;
using BlueControls.EventArgs;

using BlueControls;
using static BlueBasics.Generic;
using static BlueBasics.FileOperations;
using BlueControls.Enums;
using BlueControls.ItemCollection;

using BlueControls.Forms;
using BlueControls.ItemCollection.ItemCollectionList;

using CryptoExplorer;
using static BlueBasics.BitmapExt;

namespace BlueControls.Controls {

    public partial class FileBrowser : UserControl {

        #region Fields

        private string _ausschneiden = string.Empty;

        private bool _isLoading = false;

        #endregion

        #region Constructors

        public FileBrowser() {
            InitializeComponent();
        }

        #endregion

        #region Properties

        public string Sorierung { get; set; }

        #endregion

        #region Methods

        public string GestStandardCommand(string extension) {
            if (!SubKeyExist(extension)) { return string.Empty; }
            var mainkey = Registry.ClassesRoot.OpenSubKey(extension);
            var type = mainkey.GetValue(""); // GetValue("") read the standard value of a key
            if (type == null) { return string.Empty; }
            mainkey = Registry.ClassesRoot.OpenSubKey(type.ToString());
            if (mainkey == null) { return string.Empty; }

            var shellKey = mainkey.OpenSubKey("shell");
            var shellComand = shellKey.GetValue("");

            if (shellComand == null) { return string.Empty; }

            var exekey = shellKey.OpenSubKey(shellComand.ToString());
            exekey = exekey.OpenSubKey("command");

            return exekey.GetValue("").ToString();
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

        //private string ChangeFile(string filename, bool shoudbeCrypted) {
        //    var richtung = 10;
        //    if (shoudbeCrypted) { richtung = -10; }

        //    if (!AddThis(filename, true)) { return filename; }

        //    var T = CryptedFileName(filename, richtung);

        //    if (T == filename) { return filename; }

        //    //DeleteFile("C:\\bootmgr");

        //    var tmpOriFile = filename;
        //    while (FileExists(T)) {
        //        tmpOriFile = tmpOriFile.FilePath() + "\\" + tmpOriFile.FileNameWithoutSuffix() + "_." + tmpOriFile.FileSuffix();

        //        T = CryptedFileName(tmpOriFile, richtung);
        //    }

        //    var I = new FileInfo(filename);
        //    var attribute = I.Attributes;
        //    var crt = File.GetCreationTime(filename);
        //    var cht = File.GetLastWriteTime(filename);

        //    File.Move(filename, T);

        //    if (T.FileNameWithSuffix().StartsWith("#")) {
        //        attribute = I.Attributes | FileAttributes.Hidden;
        //    } else {
        //        attribute = I.Attributes & ~FileAttributes.Hidden;
        //    }
        //    File.SetAttributes(T, attribute);
        //    File.SetCreationTime(T, crt);
        //    File.SetLastWriteTime(T, cht);
        //    return T;
        //}

        private void lsbFiles_ContextMenuInit(object sender, ContextMenuInitEventArgs e) {
            if (e.HotItem == null) { return; }

            var it = ((BitmapListItem)e.HotItem);
            var tags = (List<string>)(it.Tag);

            e.UserMenu.Add(ContextMenuComands.Ausschneiden, !tags.TagGet("Folder").FromPlusMinus());
            e.UserMenu.Add(ContextMenuComands.Einfügen, tags.TagGet("CryptetFolder").FromPlusMinus() && !string.IsNullOrEmpty(_ausschneiden));
            e.UserMenu.AddSeparator();
            e.UserMenu.Add(ContextMenuComands.Umbenennen, FileExists(it.Internal));
            e.UserMenu.Add(ContextMenuComands.Löschen, FileExists(it.Internal));
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
                    var attribute = I.Attributes;

                    var ask = false;

                    if (Convert.ToBoolean(I.Attributes & FileAttributes.ReadOnly)) { ask = true; }

                    if (FileDialogs.DeleteFile(I.FullName, ask)) {
                        ÖffnePfad(txbPfad.Text);
                    }
                    break;

                case "Explorer":
                    ExecuteFile(it.Internal);
                    break;

                case "Umbenennen":

                    //var n = tags.TagGet("UncryptetName");

                    //var nn = InputBox.Show("Neuer Name:", n.FileNameWithoutSuffix(), VarType.Text);

                    //if (n.FileNameWithoutSuffix() == n) { return; }

                    //nn = n.FilePath() + nn + "." + n.FileSuffix();

                    //if (n != it.Internal) { nn = CryptedFileName(nn, -10); }

                    //RenameFile(it.Internal, nn, true);

                    //if (FileExists(ThumbFile(it.Internal))) {
                    //    RenameFile(ThumbFile(it.Internal), ThumbFile(nn), true);
                    //}

                    //ÖffnePfad(txbPfad.Text);
                    break;

                case "Ausschneiden":
                    _ausschneiden = it.Internal;
                    break;

                case "Einfügen":
                    var ziel = it.Internal + "\\" + _ausschneiden.FileNameWithSuffix();

                    if (FileExists(ziel)) {
                        Forms.MessageBox.Show("Datei existiert am Zielort bereits, abbruch.", ImageCode.Information);
                        return;
                    }

                    RenameFile(_ausschneiden, ziel, true);

                    _ausschneiden = string.Empty;
                    ÖffnePfad(txbPfad.Text);

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
                        var d = new PictureView((Bitmap)pc);
                        d.WindowState = FormWindowState.Maximized;
                        d.Show();
                        return;

                    case "TXT":
                        ExecuteFile("Notepad.exe", e.Item.Internal);
                        return;
                }

                var tmp = e.Item.Internal;

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
                var process = new Process();
                var startInfo = new ProcessStartInfo();
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
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

            AbortThumbs();

            txbPfad.Text = newPath.TrimEnd("\\") + "\\";

            lsbFiles.Item.Clear();

            if (!PathExists(newPath)) {
                _isLoading = false;
                return;
            }

            var allF = Directory.GetFiles(newPath, "*", SearchOption.TopDirectoryOnly);

            foreach (var thisString in allF) {
                if (AddThis(thisString, true)) // Prüfung 1, ob .cyo
                {
                    BitmapListItem p;

                    var suffix = thisString.FileSuffix().ToUpper();
                    p = new BitmapListItem(QuickImage.Get(thisString.FileType(), 64), thisString, thisString.FileNameWithoutSuffix());

                    if (AddThis(thisString, true)) // Prüfung 2, ob der ungecryptete NAme auch wirklich rein darf
                    {
                        p.Padding = 6;

                        switch (Sorierung.ToLower()) {
                            case "größe":
                                p.UserDefCompareKey = new FileInfo(thisString).Length.ToString(Constants.Format_Integer10);
                                break;

                            case "erstelldatum":
                                p.UserDefCompareKey = new FileInfo(thisString).CreationTime.ToString("yyyy/MM/dd HH:mm:ss.fff");
                                break;

                            default:
                                p.UserDefCompareKey = Constants.SecondSortChar + p.Caption.ToUpper();
                                break;
                        }

                        var tags = new List<string>();
                        tags.TagSet("Folder", bool.FalseString);

                        tags.TagSet("Suffix", suffix);

                        tags.TagSet("UncryptetName", thisString);
                        if (suffix == "URL") { tags.TagSet("URL", GetUrlFileDestination(thisString)); }

                        p.Tag = tags;

                        lsbFiles.Item.Add(p);
                    }
                }
            }

            var allD = Directory.GetDirectories(newPath, "*", SearchOption.TopDirectoryOnly);
            foreach (var thisString in allD) {
                if (AddThis(thisString, false)) {
                    BitmapListItem p = null;
                    var tags = new List<string>();

                    p = new BitmapListItem(QuickImage.Get("Ordner|64"), thisString, thisString.FileNameWithoutSuffix());
                    tags.TagSet("CryptetFolder", bool.FalseString);

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

        private void ThumbGenerator_DoWork(object sender, DoWorkEventArgs e) {
            //ThumbGenerator.SetApartmentState

            var newPath = txbPfad.Text.Trim("\\") + "\\";
            if (!PathExists(newPath)) { return; }

            string rEadable;

            var allF = Directory.GetFiles(newPath, "*", SearchOption.TopDirectoryOnly);

            if (ThumbGenerator.CancellationPending) { return; }

            foreach (var thisString in allF) {
                if (ThumbGenerator.CancellationPending) { return; }
                if (AddThis(thisString, true)) // Prüfung 1, ob .cyo und auch unverschlüsselte
                {
                    var feedBack = new List<object>(); // Zeile 1, Dateiname auf Festplatte, ZEile 2 das Bild selbst
                    feedBack.Add(thisString);

                    feedBack.Add(WindowsThumbnailProvider.GetThumbnail(thisString, 64, 64, ThumbnailOptions.BiggerSizeOk));
                    ThumbGenerator.ReportProgress(0, feedBack);
                }
            }
        }

        private void ThumbGenerator_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            var gb = (List<object>)e.UserState;
            var file = (string)gb[0];

            var item = lsbFiles.Item[file];
            if (item == null) { return; }
            var bItem = (BitmapListItem)item;

            if (gb.Count == 2) {
                bItem.Bitmap = (Bitmap)gb[1];
                return;
            }

            //      bItem.Bitmap = (Bitmap)Image_FromFile(ThumbFile(file));
        }

        private void txbPfad_Enter(object sender, System.EventArgs e) {
            ÖffnePfad(txbPfad.Text);
        }

        #endregion
    }
}
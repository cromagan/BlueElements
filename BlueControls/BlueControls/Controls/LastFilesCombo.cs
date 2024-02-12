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

using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using BlueBasics;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.ItemCollectionList;
using static BlueBasics.IO;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
[DefaultEvent("ItemClicked")]
public sealed class LastFilesCombo : ComboBox {

    #region Fields

    private bool _loaded;
    private int _maxCount = 20;
    private bool _mustExists = true;
    private List<string> _settings = [];
    private string _settingsfilename = string.Empty;

    #endregion

    #region Constructors

    public LastFilesCombo() : base() => SetLastFilesStyle();

    #endregion

    #region Properties

    /// <summary>
    /// Wohin die Datei gespeichtert werden soll, welche Dateien zuletzt benutzt wurden.
    /// </summary>
    ///
    [DefaultValue("")]
    public string Filename {
        get => _settingsfilename;
        set {
            if (_settingsfilename == value) { return; }
            _settingsfilename = value;
            LoadSettingsFromDisk();
            GenerateMenu();
        }
    }

    [DefaultValue(20)]
    public int MaxCount {
        get => _maxCount;
        set {
            if (_maxCount == value) { return; }
            _maxCount = value;
            GenerateMenu();
        }
    }

    [DefaultValue(true)]
    public bool MustExist {
        get => _mustExists;
        set {
            if (_mustExists == value) { return; }
            _mustExists = value;
            GenerateMenu();
        }
    }

    #endregion

    #region Methods

    public void AddFileName(string? fileName, string additionalText) {
        if (fileName != null) {
            var s = fileName + "|" + additionalText;
            s = s.Replace("\r\n", ";");
            s = s.Replace("\r", ";");
            s = s.Replace("\n", ";");
            if (!_mustExists || FileExists(fileName)) {
                if (!_loaded) { LoadSettingsFromDisk(); }
                if (_settings.Count > 0) { _settings.RemoveString(fileName, false); }
                if (_settings.Count > 0) { _settings.RemoveString(s, false); }
                _settings.Add(s);

                SaveSettingsToDisk();
            }
        }
        GenerateMenu();
    }

    protected override void DrawControl(Graphics gr, States state) {
        SetLastFilesStyle();
        base.DrawControl(gr, state);
    }

    protected override void OnHandleCreated(System.EventArgs e) {
        base.OnHandleCreated(e);
        if (!_loaded) { LoadSettingsFromDisk(); }
        GenerateMenu();
    }

    protected override void OnItemClicked(AbstractListItemEventArgs e) {
        base.OnItemClicked(e);

        if (e.Item.Tag is not List<string> t) { return; }

        AddFileName(e.Item.KeyName, t[0]);
    }

    private void GenerateMenu() {
        var nr = -1;
        var vis = false;
        Item.Clear();
        for (var z = _settings.Count - 1; z >= 0; z--) {
            var x = _settings[z].SplitAndCutBy("|");
            if (x.GetUpperBound(0) >= 0 && !string.IsNullOrEmpty(x[0]) && Item[x[0]] is null) {
                if (!_mustExists || FileExists(x[0])) {
                    nr++;
                    if (nr < MaxCount) {
                        vis = true;
                        var show = (nr + 1).ToString(Constants.Format_Integer3) + ": ";
                        if (_mustExists) {
                            show += x[0].FileNameWithSuffix();
                        } else {
                            show += x[0];
                        }
                        if (x.GetUpperBound(0) > 0 && !string.IsNullOrEmpty(x[1])) {
                            show = show + " - " + x[1];
                        }
                        TextListItem it = new(show, x[0], null, false, true,
                            nr.ToString(Constants.Format_Integer3));
                        List<string> t = [x.GetUpperBound(0) > 0 && !string.IsNullOrEmpty(x[1]) ? x[1] : string.Empty];
                        it.Tag = t;
                        Item.Add(it);
                    }
                }
            }
        }
        Enabled = vis;
    }

    private void LoadSettingsFromDisk() {
        _settings = [];

        if (FileExists(SettingsFileName())) {
            var t = File.ReadAllText(SettingsFileName(), Encoding.UTF8);
            t = t.RemoveChars("\n");
            _settings.AddRange(t.SplitAndCutByCr());
            _loaded = true;
        }
    }

    private void SaveSettingsToDisk() {
        var pf = SettingsFileName().FilePath();

        if (string.IsNullOrEmpty(pf)) { return; }

        if (!DirectoryExists(pf)) {
            _ = Directory.CreateDirectory(pf);
        }

        if (CanWriteInDirectory(pf)) {
            _settings.WriteAllText(SettingsFileName(), Encoding.UTF8, false);
        }
    }

    private void SetLastFilesStyle() {
        if (DrawStyle == ComboboxStyle.TextBox) {
            DrawStyle = ComboboxStyle.Button;
        }
        if (string.IsNullOrEmpty(ImageCode)) { ImageCode = "Ordner"; }
        if (string.IsNullOrEmpty(Text)) { Text = "zuletzt geöffnete Dateien"; }
    }

    private string SettingsFileName() => !string.IsNullOrEmpty(_settingsfilename) ? _settingsfilename.CheckFile() : Application.StartupPath + "\\" + Name + "-Files.laf";

    #endregion
}
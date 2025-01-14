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

using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using BlueBasics;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionList;
using static BlueBasics.IO;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
[DefaultEvent("ItemClicked")]
public sealed class LastFilesCombo : ComboBox, IHasSettings {

    #region Fields

    private int _maxCount = 20;
    private bool _mustExists = true;

    private string _settingsManualFilename = string.Empty;

    #endregion

    #region Constructors

    public LastFilesCombo() : base() => SetLastFilesStyle();

    #endregion

    #region Properties

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

    public List<string> Settings { get; } = [];
    public bool SettingsLoaded { get; set; } = false;

    /// <summary>
    /// Wohin die Datei gespeichtert werden soll, welche Dateien zuletzt benutzt wurden.
    /// </summary>
    ///
    [DefaultValue("")]
    public string SettingsManualFilename {
        get => _settingsManualFilename;
        set {
            if (_settingsManualFilename == value) { return; }
            _settingsManualFilename = value;
            this.LoadSettingsFromDisk(true);
            GenerateMenu();
        }
    }

    #endregion

    #region Methods

    public void AddFileName(string? fileName, string additionalText) {
        if (fileName != null) {
            var s = fileName + "|" + additionalText;

            if (!_mustExists || FileExists(fileName)) {
                this.SettingsAdd(s);
            }
        }
        GenerateMenu();
    }

    protected override void DrawControl(Graphics gr, States state) {
        SetLastFilesStyle();
        base.DrawControl(gr, state);
    }

    protected override void OnCreateControl() {
        base.OnCreateControl();
        CheckBack();
        this.LoadSettingsFromDisk(false);
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
        ItemClear();
        for (var z = Settings.Count - 1; z >= 0; z--) {
            var x = Settings[z].SplitAndCutBy("|");
            if (x.GetUpperBound(0) >= 0 && !string.IsNullOrEmpty(x[0]) && base[x[0]] is null) {
                if (!_mustExists || FileExists(x[0])) {
                    nr++;
                    if (nr < MaxCount) {
                        vis = true;
                        var show = (nr + 1).ToStringInt3() + ": ";
                        if (_mustExists) {
                            show += x[0].FileNameWithSuffix();
                        } else {
                            show += x[0];
                        }
                        if (x.GetUpperBound(0) > 0 && !string.IsNullOrEmpty(x[1])) {
                            show = show + " - " + x[1];
                        }
                        TextListItem it = new(show, x[0], null, false, true,
                            nr.ToStringInt3());
                        List<string> t = [x.GetUpperBound(0) > 0 && !string.IsNullOrEmpty(x[1]) ? x[1] : string.Empty];
                        it.Tag = t;
                        ItemAdd(it);
                    }
                }
            }
        }
        Enabled = vis;
    }

    private void SetLastFilesStyle() {
        if (DrawStyle == ComboboxStyle.TextBox) {
            DrawStyle = ComboboxStyle.Button;
        }
        if (string.IsNullOrEmpty(ImageCode)) { ImageCode = "Ordner"; }
        if (string.IsNullOrEmpty(Text)) { Text = "zuletzt geöffnete Dateien"; }
    }

    #endregion
}
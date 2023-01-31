// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.ItemCollection.ItemCollectionList;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using static BlueBasics.IO;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
[DefaultEvent("ItemClicked")]
public sealed class LastFilesCombo : ComboBox {

    #region Fields

    private string _filename = string.Empty;
    private List<string> _lastD = new();
    private int _maxCount = 20;
    private bool _mustExists = true;

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
        get => _filename;
        set {
            if (_filename == value) { return; }
            _filename = value;
            LoadFromDisk();
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
                if (_lastD.Count > 0) { _lastD.RemoveString(fileName, false); }
                if (_lastD.Count > 0) { _lastD.RemoveString(s, false); }
                _lastD.Add(s);

                if (CanWriteInDirectory(SaveFileName().FilePath())) {
                    _lastD.Save(SaveFileName(), System.Text.Encoding.UTF8, false);
                }
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
        LoadFromDisk();
        GenerateMenu();
    }

    protected override void OnItemClicked(BasicListItemEventArgs e) {
        base.OnItemClicked(e);
        var t = (List<string>)e.Item.Tag;
        AddFileName(e.Item.Internal, t[0]);
    }

    private void GenerateMenu() {
        var nr = -1;
        var vis = false;
        Item.Clear();
        for (var z = _lastD.Count - 1; z >= 0; z--) {
            var x = _lastD[z].SplitAndCutBy("|");
            if (x != null && x.GetUpperBound(0) >= 0 && !string.IsNullOrEmpty(x[0]) && Item[x[0]] is null) {
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
                        List<string> t = new();
                        if (x.GetUpperBound(0) > 0 && !string.IsNullOrEmpty(x[1])) {
                            t.Add(x[1]);
                        } else {
                            t.Add(string.Empty);
                        }
                        it.Tag = t;
                        Item.Add(it);
                    }
                }
            }
        }
        Enabled = vis;
    }

    private void LoadFromDisk() {
        _lastD = new List<string?>();
        if (FileExists(SaveFileName())) {
            var t = File.ReadAllText(SaveFileName(), System.Text.Encoding.UTF8);
            t = t.RemoveChars("\n");
            _lastD.AddRange(t.SplitAndCutByCr());
        }
    }

    private string SaveFileName() => !string.IsNullOrEmpty(_filename) ? _filename.CheckFile() : System.Windows.Forms.Application.StartupPath + "\\" + Name + "-Files.laf";

    private void SetLastFilesStyle() {
        if (DrawStyle == ComboboxStyle.TextBox) {
            DrawStyle = ComboboxStyle.Button;
        }
        if (string.IsNullOrEmpty(ImageCode)) { ImageCode = "Ordner"; }
        if (string.IsNullOrEmpty(Text)) { Text = "zuletzt geöffnete Dateien"; }
    }

    #endregion
}
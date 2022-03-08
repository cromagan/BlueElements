﻿// Authors:
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

using BlueBasics;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.EventArgs;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using BlueControls.ItemCollection.ItemCollectionList;
using static BlueBasics.FileOperations;

namespace BlueControls.Controls {

    [Designer(typeof(BasicDesigner))]
    [DefaultEvent("ItemClicked")]
    public sealed class LastFilesCombo : ComboBox {

        #region Fields

        public string _filename = string.Empty;
        public int _maxCount = 20;
        public bool _mustExists = true;
        private List<string?> LastD = new();

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

        public void AddFileName(string fileName, string additionalText) {
            var s = fileName + "|" + additionalText;
            s = s.Replace("\r\n", ";");
            s = s.Replace("\r", ";");
            s = s.Replace("\n", ";");
            if (!_mustExists || FileExists(fileName)) {
                if (LastD.Count > 0) { LastD.RemoveString(fileName, false); }
                if (LastD.Count > 0) { LastD.RemoveString(s, false); }
                LastD.Add(s);

                if (CanWriteInDirectory(SaveFileName().FilePath())) {
                    LastD.Save(SaveFileName(), System.Text.Encoding.UTF8, false);
                }
            }
            GenerateMenu();
        }

        protected override void DrawControl(Graphics gr, enStates state) {
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
            var NR = -1;
            var Vis = false;
            Item.Clear();
            for (var Z = LastD.Count - 1; Z >= 0; Z--) {
                var x = LastD[Z].SplitAndCutBy("|");
                if (x != null && x.GetUpperBound(0) >= 0 && !string.IsNullOrEmpty(x[0]) && Item[x[0]] is null) {
                    if (!_mustExists || FileExists(x[0])) {
                        NR++;
                        if (NR < MaxCount) {
                            Vis = true;
                            var show = (NR + 1).ToString(Constants.Format_Integer3) + ": ";
                            if (_mustExists) {
                                show += x[0].FileNameWithSuffix();
                            } else {
                                show += x[0];
                            }
                            if (x.GetUpperBound(0) > 0 && !string.IsNullOrEmpty(x[1])) {
                                show = show + " - " + x[1];
                            }
                            TextListItem it = new(show, x[0], null, false, true,
                                NR.ToString(Constants.Format_Integer3));
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
            Enabled = Vis;
        }

        private void LoadFromDisk() {
            LastD = new List<string?>();
            if (FileExists(SaveFileName())) {
                var t = File.ReadAllText(SaveFileName(), System.Text.Encoding.UTF8);
                t = t.RemoveChars("\n");
                LastD.AddRange(t.SplitAndCutByCr());
            }
        }

        private string SaveFileName() => !string.IsNullOrEmpty(_filename) ? _filename.CheckFile() : System.Windows.Forms.Application.StartupPath + "\\" + Name + "-Files.laf";

        private void SetLastFilesStyle() {
            if (DrawStyle == enComboboxStyle.TextBox) {
                DrawStyle = enComboboxStyle.Button;
            }
            if (string.IsNullOrEmpty(ImageCode)) { ImageCode = "Ordner"; }
            if (string.IsNullOrEmpty(Text)) { Text = "zuletzt geöffnete Dateien"; }
        }

        #endregion
    }
}
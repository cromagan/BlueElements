#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2021 Christian Peter
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
#endregion

using BlueBasics;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.ItemCollection;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using static BlueBasics.FileOperations;

namespace BlueControls.Controls {
    [Designer(typeof(BasicDesigner))]
    [DefaultEvent("ItemClicked")]
    public sealed class LastFilesCombo : ComboBox {

        private List<string> LastD = new();

        public string _filename = string.Empty;
        public bool _mustExists = true;
        public int _maxCount = 20;
        //public string _specialcommand = string.Empty;

        #region Constructor
        public LastFilesCombo() : base() => SetLastFilesStyle();

        #endregion

        #region  Events 
        //public event System.EventHandler SpecialCommandClicked;
        #endregion

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

        ///// <summary>
        ///// Wenn an erster Stelle ein besonderer Befehl stehen soll. Das Event SpecialCommandClicked wird anstelle ItemClicked ausgelöst.
        ///// </summary>
        ///// 
        //[DefaultValue("")]
        //public string SpecialCommand
        //{
        //    get
        //    {
        //        return _specialcommand;
        //    }
        //    set
        //    {
        //        if (_specialcommand == value) { return; }
        //        _specialcommand = value;
        //        GenerateMenu();
        //    }
        //}

        [DefaultValue(true)]
        public bool MustExist {
            get => _mustExists;
            set {
                if (_mustExists == value) { return; }
                _mustExists = value;
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

        private void GenerateMenu() {

            var NR = -1;
            var Vis = false;

            Item.Clear();

            for (var Z = LastD.Count - 1; Z >= 0; Z--) {

                var x = LastD[Z].SplitBy("|");

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

                            var it = new TextListItem(show, x[0], null, false, true, NR.ToString(Constants.Format_Integer3));

                            var t = new List<string>();

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

        public void AddFileName(string FileName, string AdditionalText) {

            var s = FileName + "|" + AdditionalText;

            s = s.Replace("\r\n", ";");
            s = s.Replace("\r", ";");
            s = s.Replace("\n", ";");

            if (!_mustExists || FileExists(FileName)) {
                if (LastD.Count > 0) { LastD.RemoveString(FileName, false); }
                if (LastD.Count > 0) { LastD.RemoveString(s, false); }
                LastD.Add(s);

                LastD.Save(SaveFile(), false, System.Text.Encoding.GetEncoding(1252));
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

        private string SaveFile() => !string.IsNullOrEmpty(_filename) ? _filename : System.Windows.Forms.Application.StartupPath + Name + "-Files.laf";

        private void LoadFromDisk() {
            LastD = new List<string>();

            if (FileExists(SaveFile())) {
                var t = File.ReadAllText(SaveFile(), Constants.Win1252);
                t = t.RemoveChars("\n");
                LastD.AddRange(t.SplitByCR());
            }
        }

        private void SetLastFilesStyle() {
            if (DrawStyle == enComboboxStyle.TextBox) {
                DrawStyle = enComboboxStyle.Button;
            }
            if (string.IsNullOrEmpty(ImageCode)) { ImageCode = "Ordner"; }
            if (string.IsNullOrEmpty(Text)) { Text = "zuletzt geöffnete Dateien"; }
        }

        protected override void OnItemClicked(BasicListItemEventArgs e) {

            //if (!string.IsNullOrEmpty(_specialcommand) && e.Item.Internal == "#SPECIAL#")
            //{
            //    OnSpecialCommandClicked();
            //    return;
            //}

            base.OnItemClicked(e);

            var t = (List<string>)e.Item.Tag;

            AddFileName(e.Item.Internal, t[0]);
        }

        //private void OnSpecialCommandClicked()
        //{
        //    SpecialCommandClicked?.Invoke(this, System.EventArgs.Empty);
        //}

    }
}

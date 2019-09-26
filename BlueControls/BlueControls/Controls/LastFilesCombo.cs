#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2019 Christian Peter
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
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.ItemCollection;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using static BlueBasics.FileOperations;

namespace BlueControls.Controls
{
    public sealed class LastFilesCombo : ComboBox
    {

        private List<string> LastD = new List<string>();

        public string _filename = string.Empty;
        public bool _mustExists = true;
        public int _maxCount = 20;
        public string _specialcommand = string.Empty;


        #region  Events 
        public event System.EventHandler SpecialCommandClicked;
        #endregion


        /// <summary>
        /// Wohin die Datei gespeichtert werden soll, welche Dateien zuletzt benutzt wurden.
        /// </summary>
        /// 
        [DefaultValue("")]
        public string Filename
        {
            get
            {
                return _filename;
            }
            set
            {
                if (_filename == value) { return; }
                _filename = value;
                LoadFromDisk();
                GenerateMenu();
            }
        }

        /// <summary>
        /// Wenn an erster Stelle ein besonderer Befehl stehen soll. Das Event SpecialCommandClicked wird anstelle ItemClicked ausgelöst.
        /// </summary>
        /// 
        [DefaultValue("")]
        public string SpecialCommand
        {
            get
            {
                return _specialcommand;
            }
            set
            {
                if (_specialcommand == value) { return; }
                _specialcommand = value;
                GenerateMenu();
            }
        }


        [DefaultValue(true)]
        public bool MustExist
        {
            get
            {
                return _mustExists;
            }
            set
            {
                if (_mustExists == value) { return; }
                _mustExists = value;
                GenerateMenu();

            }
        }

        [DefaultValue(20)]
        public int MaxCount
        {
            get
            {
                return _maxCount;
            }
            set
            {
                if (_maxCount == value) { return; }
                _maxCount = value;
                GenerateMenu();

            }
        }


        public LastFilesCombo()
        {
            SetLastFilesStyle();
        }




        private void GenerateMenu()
        {

            var NR = 0;


            var Vis = false;


            Item.Clear();


            if (!string.IsNullOrEmpty(_specialcommand))
            {
                Item.Add(new TextListItem("#SPECIAL#", _specialcommand, QuickImage.Get(ImageCode)));
            }



            NR = -1;

            for (var Z = LastD.Count - 1; Z >= 0; Z--)
            {

                var x = LastD[Z].SplitBy("|");

                if (!_mustExists || FileExists(x[0]))
                {
                    NR += 1;

                    if (NR < MaxCount)
                    {
                        Vis = true;
                        var show = (NR + 1).ToString(Constants.Format_Integer3) + ": ";

                        if (_mustExists)
                        {
                            show = show + x[0].FileNameWithSuffix();
                        }
                        else
                        {
                            show = show + x[0];
                        }

                        if (x.GetUpperBound(0) > 0 && !string.IsNullOrEmpty(x[1]))
                        {
                            show = show + " - " + x[1];
                        }

                        var it = new TextListItem(false, x[0], show, NR.ToString(Constants.Format_Integer3));
                        if (x.GetUpperBound(0) > 0 && !string.IsNullOrEmpty(x[1]))
                        {
                            it.Tags.Add(x[1]);
                        }
                        else
                        {
                            it.Tags.Add(string.Empty);
                        }

                        Item.Add(it);
                    }
                }
            }



            Enabled = Vis;
        }


        public void AddFileName(string FileName, string AdditionalText)
        {

            var s = FileName + "|" + AdditionalText;


            s = s.Replace("\r\n", ";");
            s = s.Replace("\r", ";");
            s = s.Replace("\n", ";");

            if (!_mustExists || FileExists(FileName))
            {
                if (LastD.Count > 0) { LastD.RemoveString(FileName, false); }
                if (LastD.Count > 0) { LastD.RemoveString(s, false); }
                LastD.Add(s);
                LastD.Save(SaveFile(), false);
            }
            GenerateMenu();
        }


        protected override void DrawControl(Graphics gr, enStates state)
        {
            SetLastFilesStyle();
            base.DrawControl(gr, state);
        }

        protected override void OnHandleCreated(System.EventArgs e)
        {
            base.OnHandleCreated(e);
            LoadFromDisk();
            GenerateMenu();
        }


        private string SaveFile()
        {
            if (!string.IsNullOrEmpty(_filename)) { return _filename; }

            return System.Windows.Forms.Application.StartupPath + "\\" + Name + "-Files.laf";
        }


        private void LoadFromDisk()
        {
            LastD = new List<string>();

            if (FileExists(SaveFile()))
            {
                var t = modAllgemein.LoadFromDisk(SaveFile());
                t = t.RemoveChars("\n");
                LastD.AddRange(t.SplitByCR());
            }
        }


        private void SetLastFilesStyle()
        {
            if (DrawStyle == enComboboxStyle.TextBox)
            {
                DrawStyle = enComboboxStyle.Button;
            }
            if (string.IsNullOrEmpty(ImageCode)) { ImageCode = "Ordner"; }
            if (string.IsNullOrEmpty(Text)) { Text = "zuletzt geöffnete Dateien"; }
        }



        protected override void OnItemClicked(BasicListItemEventArgs e)
        {

            if (!string.IsNullOrEmpty(_specialcommand) && e.Item.Internal() == "#SPECIAL#")
            {
                OnSpecialCommandClicked();
                return;
            }

            base.OnItemClicked(e);
            AddFileName(e.Item.Internal(), e.Item.Tags[0]);
        }


        private void OnSpecialCommandClicked()
        {
            SpecialCommandClicked?.Invoke(this, System.EventArgs.Empty);
        }


    }
}

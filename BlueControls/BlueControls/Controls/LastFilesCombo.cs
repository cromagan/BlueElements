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
            NR = -1;

            for (var Z = LastD.Count - 1; Z >= 0; Z--)
            {
                if (!_mustExists || FileExists(LastD[Z]))
                {
                    NR += 1;

                    if (NR < MaxCount)
                    {
                        Vis = true;


                        var show = (NR + 1).ToString(Constants.Format_Integer3) + ": ";


                        var x = (LastD[Z] + "|").SplitBy("|");

                        if (_mustExists)
                        {
                            show = show + x[0].FileNameWithSuffix();
                        }
                        else
                        {
                            show = show + x[0];
                        }

                        if (!string.IsNullOrEmpty(x[1]))
                        {
                            show = show + " - " + x[1];
                        }
                        
                        var it = new TextListItem(false, x[0], show, NR.ToString(Constants.Format_Integer3));
                        it.Tags.Add(x[1]);
                        Item.Add(it);
                    }
                }
            }



            Enabled = Vis;
        }


        public void AddFileName(string FileName, string AdditionalText)
        {

            var s = FileName + "|" + AdditionalText;

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
            DrawStyle = enComboboxStyle.RibbonBar;

            if (string.IsNullOrEmpty(ImageCode)) { ImageCode = "Ordner"; }
            if (string.IsNullOrEmpty(Text)) { Text = "zuletzt geöffnete Dateien"; }
        }



        protected override void OnItemClicked(BasicListItemEventArgs e)
        {
            base.OnItemClicked(e);
            AddFileName(e.Item.Internal(), e.Item.Tags[0]);
        }
    }
}

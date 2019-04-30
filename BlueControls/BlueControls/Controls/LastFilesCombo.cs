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
using BlueControls.ItemCollection;
using System.Collections.Generic;
using System.Drawing;
using static BlueBasics.FileOperations;

namespace BlueControls.Controls
{
    public sealed class LastFilesCombo : ComboBox
    {
        private int MaxCount = 20;
        private List<string> LastD = new List<string>();
        private string SaveFile;



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

            for (var Z = LastD.Count - 1 ; Z >= 0 ; Z--)
            {
                if (FileExists(LastD[Z]))
                {
                    NR += 1;

                    if (NR < MaxCount)
                    {
                        Vis = true;
                        Item.Add(new TextListItem(false, LastD[Z], (NR + 1).ToString(Constants.Format_Integer2) + ": " + LastD[Z].FileNameWithSuffix(), NR.ToString(Constants.Format_Integer3)));
                    }
                }
            }



            Enabled = Vis;
        }


        public void AddFileName(string FileName)
        {
            if (FileExists(FileName))
            {
                if (LastD.Count > 0)
                {
                    LastD.RemoveString(FileName, false);
                }
                LastD.Add(FileName);
                LastD.Save(SaveFile, false);
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

            MaxCount = 20;
            SaveFile = System.Windows.Forms.Application.StartupPath + "\\" + Name + "-Files.laf";
            LastD = new List<string>();

            if (FileExists(SaveFile))
            {
                var t = modAllgemein.LoadFromDisk(SaveFile);
                t = t.RemoveChars("\n");
                LastD.AddRange(t.SplitByCR());
            }

            GenerateMenu();
        }








        private void SetLastFilesStyle()
        {
            DrawStyle = enComboboxStyle.RibbonBar;
            ImageCode = "Ordner";
            Text = "zuletzt geöffnete Dateien";
        }

    }
}

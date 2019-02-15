using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Enums;
using BlueControls.ItemCollection.ItemCollectionList;
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
                        Item.Add(new TextListItem(false, LastD[Z], (NR + 1).Nummer(2) + ": " + LastD[Z].FileNameWithSuffix(), NR.Nummer(3)));
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


        protected override void DrawControl(Graphics GR, enStates vState)
        {
            SetLastFilesStyle();
            base.DrawControl(GR, vState);
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

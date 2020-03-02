#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2020 Christian Peter
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

using System.Drawing;
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Forms;
using BlueControls.EventArgs;
using BlueControls.ItemCollection;
using BlueDatabase;
using static BlueBasics.FileOperations;
using System.ComponentModel;

namespace BlueControls.BlueDatabaseDialogs
{

    internal partial class LayoutDesigner
    {
        public readonly Database Database;

        private string _LoadedLayout = string.Empty;
        private string _AdditionalLayoutPath = "";


        public LayoutDesigner(Database cDatabase, string cvAdditionalLayoutPath)
        {

            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();

            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
            Database = cDatabase;
            _AdditionalLayoutPath = cvAdditionalLayoutPath;


            BefülleSpaltenDropdown();

            BefülleRestlicheDropDowns();

            befülleLayoutDropdown();

            AndereSpalteGewählt();
            CheckButtons();
        }

        protected override void OnFormClosing(System.Windows.Forms.FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            SaveCurrentLayout();
        }

        private void CheckButtons()
        {

            if (Layout1.Item.Count > 0)
            {
                Layout1.Enabled = true;
            }
            else
            {
                Layout1.Enabled = false;
                Layout1.Text = string.Empty;
                _LoadedLayout = string.Empty;
            }

            if (Database != null)
            {
                Hinzu.Enabled = true;
                if (Database.Layouts.Count == 0)
                {
                    _LoadedLayout = string.Empty;
                }
            }
            else
            {
                Hinzu.Enabled = true;
                _LoadedLayout = string.Empty;
            }


            if (Layout1.Text.FileSuffix().ToUpper() != "BCR" && FileExists(Layout1.Text))
            {
                _LoadedLayout = string.Empty;
                LayBearb.Enabled = true;
                LayOpen.Enabled = true;
                Page_Control.Enabled = false;
                Abma.Enabled = true;
                Base64.Enabled = true;
                Area_Drucken.Enabled = false;
            }
            else
            {
                LayBearb.Enabled = false;
                LayOpen.Enabled = false;
                Abma.Enabled = false;
                Base64.Enabled = false;
            }

            if (!string.IsNullOrEmpty(_LoadedLayout))
            {
                Page_Control.Enabled = true;

                grpDateiSystem.Enabled = true;


                weg.Enabled = true;
                NamB.Enabled = true;
                Area_Drucken.Enabled = true;
            }
            else
            {
                Area_Drucken.Enabled = false;
                Page_Control.Enabled = false;

                grpDateiSystem.Enabled = false;


                weg.Enabled = false;
                NamB.Enabled = false;
            }
        }

        private void Hinzu_Click(object sender, System.EventArgs e)
        {
            SaveCurrentLayout();

            var ex = InputBox.Show("Geben sie den Namen<br>des neuen Layouts ein:", "", enDataFormat.Text);
            if (string.IsNullOrEmpty(ex)) { return; }
            var c = new CreativePad();

            c.Item.Caption = ex;
            Database.Layouts.Add(c.ToString());


            befülleLayoutDropdown();


            _LoadedLayout = string.Empty;

            LoadLayout(c.Item.ID);

            CheckButtons();
        }

        private void weg_Click(object sender, System.EventArgs e)
        {
            SaveCurrentLayout();


            if (string.IsNullOrEmpty(_LoadedLayout)) { return; }


            if (MessageBox.Show("Layout <b>'" + Pad.Item.Caption + "'</b><br>wirklich löschen?", enImageCode.Warnung, "Ja", "Nein") != 0) { return; }

            Pad.Item.Clear();
            var ind = Database.LayoutIDToIndex(_LoadedLayout);

            Database.Layouts.RemoveAt(ind);
            _LoadedLayout = string.Empty;


            befülleLayoutDropdown();

            Layout1.Text = string.Empty;

            CheckButtons();

        }

        private void NamB_Click(object sender, System.EventArgs e)
        {
            SaveCurrentLayout();
            if (string.IsNullOrEmpty(_LoadedLayout)) { return; }

            var ex = InputBox.Show("Namen des Layouts ändern:", Pad.Item.Caption, enDataFormat.Text);
            if (string.IsNullOrEmpty(ex)) { return; }
            Pad.Item.Caption = ex;

            SaveCurrentLayout();
            befülleLayoutDropdown();
            CheckButtons();

        }

        private void Layout1_ItemClicked(object sender, BasicListItemEventArgs e)
        {
            LoadLayout(e.Item.Internal);
        }



        internal void LoadLayout(string fileOrLayoutID)
        {
            SaveCurrentLayout();


            Layout1.Text = fileOrLayoutID;

            if (!fileOrLayoutID.StartsWith("#"))
            {

                if (fileOrLayoutID.FileSuffix().ToUpper() == "BCR")
                {
                    Pad.Enabled = true;
                    _LoadedLayout = fileOrLayoutID;
                    var l = modAllgemein.LoadFromDisk(fileOrLayoutID);
                    Pad.Item = new ItemCollectionPad(l, string.Empty);
                    ItemChanged();

                }
                else
                {
                    _LoadedLayout = string.Empty;
                    Pad.Item.Clear();
                    Pad.Item.SheetSizeInMM = SizeF.Empty;
                    var x = new TextPadItem(Pad.Item, "x", "Nicht editierbares Layout aus dem Dateisystem");
                    Pad.Item.Add(x);
                    x.Stil = Enums.PadStyles.Style_Überschrift_Haupt;
                    x.SetCoordinates(new RectangleDF(0, 0, 1000, 400));
                    ItemChanged();
                    Pad.Enabled = false;
                }
            }
            else
            {
                Pad.Enabled = true;
                _LoadedLayout = fileOrLayoutID;
                var ind = Database.LayoutIDToIndex(_LoadedLayout);
                Pad.Item = new ItemCollectionPad(Database.Layouts[ind], string.Empty);
                ItemChanged();
            }


        }


        public override void ItemChanged()
        {
            base.ItemChanged();

            CheckButtons();
            GenerateText();

        }


        private void SaveCurrentLayout()
        {
            if (Database == null) { return; }
            if (string.IsNullOrEmpty(_LoadedLayout)) { return; }

            var newl = Pad.Item.ToString();


            if (_LoadedLayout.StartsWith("#"))
            {
                var ind = Database.LayoutIDToIndex(_LoadedLayout);
                if (Database.Layouts[ind] == newl) { return; }
                Database.Layouts[ind] = newl;
            }
            else if (_LoadedLayout.FileSuffix().ToUpper() == "BCR")
            {
                modAllgemein.SaveToDisk(_LoadedLayout, newl, false);
            }


            Pad.ZoomFit();
        }




        private void LeerLösch_Click(object sender, System.EventArgs e)
        {
            GenerateText();
        }

        private void Leer_TextChanged(object sender, System.EventArgs e)
        {
            GenerateText();
        }

        private void GenerateText()
        {

            ColumnItem c = null;
            if (Database != null) { c = Database.Column[Spaltx.Text]; }
            var i = Spaltx.Item[Spaltx.Text];


            if (i == null) { return; }

            string Nam;

            if (c != null)
            {
                Nam = c.Name;
            }
            else
            {
                Nam = i.Internal;
            }



            string T = null;

            if (!string.IsNullOrEmpty(Nam) && txt.Enabled && FomAx.SelectedIndex == 0)
            {
                T = "//TS/000" + Nam;

                // Auf die Reihenfolge kommt es an!
                if (LeerLösch.Checked) { T = T + "/100"; }
                if (Spezialvormat.Checked) { T = T + "/107"; }
                if (Abkürz.Checked) { T = T + "/110"; }
                if (HtmlSonderzeichen.Checked) { T = T + "/108"; }
                if (!string.IsNullOrEmpty(Leer.Text)) { T = T + "/101" + Leer.Text.EleminateSlash().ToNonCritical(); }
                if (!string.IsNullOrEmpty(Vortext.Text)) { T = T + "/103" + Vortext.Text.EleminateSlash().ToNonCritical(); }
                if (!string.IsNullOrEmpty(Nachtext.Text)) { T = T + "/104" + Nachtext.Text.EleminateSlash().ToNonCritical(); }
                if (ZeilUmbruch.Enabled && !string.IsNullOrEmpty(ZeilUmbruch.Text)) { T = T + "/102" + ZeilUmbruch.Text.EleminateSlash().ToNonCritical(); }
                if (ÜberschriftS.Checked) { T = T + "/109"; }

                T = T + "/E";
                Code.Text = T;

            }
            else if (!string.IsNullOrEmpty(Nam) && Pic.Enabled && FomAx.SelectedIndex == 1)
            {
                if (Abma.Enabled)
                {
                    T = "//TS/001" + Nam;
                    if (int.Parse(Wi.Text) > 5) { T = T + "/200" + int.Parse(Wi.Text); }
                    if (int.Parse(He.Text) > 5) { T = T + "/201" + int.Parse(He.Text); }
                    if (Mxx.Checked) { T = T + "/210"; }
                    if (ExactMi.Checked) { T = T + "/211"; }
                    if (GroMi.Checked) { T = T + "/212"; }
                    if (Base64.Checked) { T = T + "/220"; }
                }
                else
                {
                    T = "//TS/000" + Nam;
                }


                T = T + "/E";
                Code.Text = T;

            }
            else if (FomAx.SelectedIndex == 2)
            {
                if (BefE2.Item.Checked().Count == 1) { Code.Text = BefE2.Item.Checked()[0].Internal; }
            }
            else
            {
                Code.Text = "";
            }
        }

        private void BefE2_Item_CheckedChanged(object sender, System.EventArgs e)
        {
            GenerateText();
        }

        public void AndereSpalteGewählt()
        {

            ColumnItem c = null;
            if (Database != null) { c = Database.Column[Spaltx.Text]; }
            var i = Spaltx.Item[Spaltx.Text];

            var TextB = false;
            var PicB = false;
            var MultiL = false;
            var LeerM = false;
            var Nam = "UNBEKANNT";

            if (c != null)
            {
                Nam = c.Name;
                TextB = c.Format.TextboxEditPossible();

                //if (c.Format == enDataFormat.Relation) { TextB = true; }

                if (TextB)
                {
                    MultiL = c.MultiLine;
                    LeerM = true;
                }
                else
                {
                    if (c.Format == enDataFormat.Link_To_Filesystem)
                    {
                        if (c.MultiLine == false) { PicB = true; }
                    }
                }



            }
            else
            {
                switch (i)
                {
                    case TextListItem _:
                        Nam = i.Internal;
                        TextB = true; //Not String.IsNullOrEmpty(.Text)
                        //  If TextB Then
                        //  MultiL = False
                        MultiL = true;
                        LeerM = true;
                        //End If
                        break;
                    case BitmapListItem _:
                        PicB = true;
                        TextB = false;
                        break;
                }
            }


            if (TextB)
            {
                txt.Enabled = true;
                if (FomAx.SelectedIndex == 1) { FomAx.SelectedIndex = 0; }
                Pic.Enabled = false;
            }
            else if (PicB)
            {
                Pic.Enabled = true;
                if (FomAx.SelectedIndex == 0) { FomAx.SelectedIndex = 1; }
                txt.Enabled = false;
            }
            else
            {
                FomAx.SelectedIndex = 2;
                Pic.Enabled = false;
                txt.Enabled = false;
            }

            if (MultiL)
            {
                ZeilUmbruch.Enabled = true;
                ZeilCap.Enabled = true;
            }
            else
            {
                ZeilUmbruch.Enabled = false;
                ZeilUmbruch.Text = "";
                ZeilCap.Enabled = false;
            }


            BefE2.Item.Clear();
            BefE2.Item.Add(new TextListItem("//AS/300/AE", "Kopf Ende, Körper beginnt", QuickImage.Get("Pfeil_Rechts|16")));
            BefE2.Item.Add(new TextListItem("//AS/301/AE", "Körper Ende, Fuß beginnt", QuickImage.Get("Pfeil_Rechts|16")));
            BefE2.Item.Add(new TextListItem("//XS/302", "Bereinigung Start", enImageCode.Pinsel));
            BefE2.Item.Add(new TextListItem("/XE", "Bereinigung Ende", enImageCode.Pinsel));

            if (LeerM)
            {
                BefE2.Item.Add(new LineListItem());
                BefE2.Item.Add(new TextListItem("//AS/003" + Nam.ToUpper() + "/310", "Wenn der Inhalt Nicht Leer ist, dann...", QuickImage.Get("Gänsefüßchen|16|12")));
                BefE2.Item.Add(new TextListItem("/AE", "...Ende der Abfrage", QuickImage.Get("Gänsefüßchen|16|12|6")));
            }


            GenerateText();
        }


        private void Spalt_ItemClicked(object sender, BasicListItemEventArgs e)
        {
            AndereSpalteGewählt();
        }



        private void BefülleSpaltenDropdown()
        {
            Spaltx.Item.Clear();

            if (Database != null)
            {
                Spaltx.Item.AddRange(Database.Column, true, true);
                Spaltx.Item.Sort();
            }


            if (Spaltx.Item.Count > 0)
            {
                Spaltx.Enabled = true;
                Spaltx.Text = Spaltx.Item[0].Internal;
            }
            else
            {
                Spaltx.Enabled = false;
            }
        }

        private void BefülleRestlicheDropDowns()
        {
            Leer.Item.Clear();
            Leer.Item.Add(new TextListItem("k. A."));
            Leer.Item.Add(new TextListItem("-"));


            ZeilUmbruch.Item.Clear();
            ZeilUmbruch.Item.Add(new TextListItem(", "));
            ZeilUmbruch.Item.Add(new TextListItem("/"));
            ZeilUmbruch.Item.Add(new TextListItem(" ", "Leerzeichen"));
            ZeilUmbruch.Item.Add(new TextListItem("<br>", "<br> (HTML-Format)"));
            ZeilUmbruch.Item.Add(new TextListItem("<w:br/>", "<w:br/> (Word XML-Format)"));
            ZeilUmbruch.Item.Add(new TextListItem("\r\n", "Zeilenumbruch (CR & LF [Carriage Return und Line Feed])"));


        }



        private void Clip_Click(object sender, System.EventArgs e)
        {
            System.Windows.Forms.Clipboard.SetDataObject(Code.Text, true);

            Notification.Show("Der gewählte Platzhalter<br>befindet sich nun in der<br>Zwischenablage und kann<br>nun in einer anderen Anwendung<br>verwendet werden.", enImageCode.Clipboard);

        }

        private void LayBearb_Click(object sender, System.EventArgs e)
        {
            modAllgemein.ExecuteFile("notepad.exe", Layout1.Text, false);
        }

        private void LayOpen_Click(object sender, System.EventArgs e)
        {
            modAllgemein.ExecuteFile(Layout1.Text, null, false);
        }

        private void LayVZ_Click(object sender, System.EventArgs e)
        {

            if (string.IsNullOrEmpty(_AdditionalLayoutPath) && Database != null) { _AdditionalLayoutPath = Database.Filename.FilePath() + "Layouts\\"; }

            modAllgemein.ExecuteFile(_AdditionalLayoutPath);
        }

        private void befülleLayoutDropdown()
        {
            if (Database != null)
            {
                Layout1.Item.Clear();
                Layout1.Item.AddLayoutsOf(Database, true, _AdditionalLayoutPath);
            }
        }

        private void LoadTab_FileOk(object sender, CancelEventArgs e)
        {
            LoadFile(LoadTab.FileName);
            Ribbon.SelectedIndex = 1;
        }

        private void SaveTab_FileOk(object sender, CancelEventArgs e)
        {

            var t = Pad.Item.ToString();
            modAllgemein.SaveToDisk(SaveTab.FileName, t, false);

        }

        private void btnOeffnen_Click(object sender, System.EventArgs e)
        {
            LoadTab.ShowDialog();
        }


        private void btnSpeichern_Click(object sender, System.EventArgs e)
        {

            SaveTab.ShowDialog();
        }

        private void btnNeu_Click(object sender, System.EventArgs e)
        {
            Pad.Item.Clear();
            Pad.ZoomFit();
            Ribbon.SelectedIndex = 1;
        }

        private void btnLastFiles_ItemClicked(object sender, BasicListItemEventArgs e)
        {
            LoadFile(e.Item.Internal);
        }


        private void LoadFile(string fileName)
        {
            Pad.Item.Clear();
            var t = modAllgemein.LoadFromDisk(fileName);

            Pad.Item = new ItemCollectionPad(t, _LoadedLayout);
            ItemChanged();

        }
    }
}
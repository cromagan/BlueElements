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


using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using BlueBasics;
using BlueControls.BlueDatabaseDialogs;
using BlueControls.Controls;
using BlueControls.EventArgs;
using BlueControls.ItemCollection;
using BlueDatabase;
using static BlueBasics.FileOperations;

namespace BlueControls.Forms
{
    public sealed partial class ExportDialog
    {


        private string _AdditionalLayoutPath = "";
        private List<RowItem> Liste;
        private readonly Database Database;
        private string ZielPfad = "";

        private string SaveTo = "";

        private int ItemNrForPrint;


        public ExportDialog(Database db, List<RowItem> ListetItems, bool CanChangeItems)
        {

            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();

            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
            Database = db;
            Liste = ListetItems;

            Init(string.Empty, CanChangeItems, string.Empty);
        }


        public ExportDialog(string vAdditionalLayoutPath, string AutosaveFile)
        {

            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();

            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
            Database = null;
            Liste = null;

            //   Vars = Variablen

            Init(vAdditionalLayoutPath, false, AutosaveFile);
        }


        public void Init(string vAdditionalLayoutPath, bool CanChangeItems, string AutosaveFile)
        {

            _AdditionalLayoutPath = vAdditionalLayoutPath;

            if (!string.IsNullOrEmpty(AutosaveFile))
            {
                ZielPfad = AutosaveFile.FilePath();
                SaveTo = AutosaveFile;
            }
            else
            {
                ZielPfad = Path.GetTempPath();
            }


            try
            {
                if (!string.IsNullOrEmpty(_AdditionalLayoutPath) && !PathExists(_AdditionalLayoutPath))
                {
                    Directory.CreateDirectory(_AdditionalLayoutPath);
                }
                if (!PathExists(ZielPfad))
                {
                    Directory.CreateDirectory(ZielPfad);
                }
            }
            catch (Exception)
            {

            }


            FrmDrucken_EinWahl.Enabled = CanChangeItems;

        }



        private void EinWahl_Click(object sender, System.EventArgs e)
        {
            using (var fr = new TableView(Database))
            {
                Liste = fr.GetFilteredItems();
            }


            FrmDrucken_Zusammen.Checked = false;
            FrmDrucken_Einzeln.Checked = false;

            ShowAndCheckTabEinträgeWählen();

            //   LayoutAusSysSpalteHolen("")

            // InfoUndHauptButtons()
        }





        protected override void OnShown(System.EventArgs e)
        {
            base.OnShown(e);
            ShowExportAktion();
        }



        public void ShowAndCheckLayoutWahl()
        {
            Tabs.TabPages[0].Enabled = false;
            Tabs.TabPages[1].Enabled = true;
            Tabs.TabPages[2].Enabled = false;
            Tabs.TabPages[3].Enabled = false;
            Tabs.TabPages[4].Enabled = false;
            Tabs.SelectedIndex = 1;


            if (FrmDrucken_Layout1.Item.Count == 0)
            {
                BefülleLayoutDropdowns();
            }



            Weiter2.Enabled = !string.IsNullOrEmpty(FrmDrucken_Layout1.Text);


            if (FrmDrucken_Layout1.Text.IsLong())
            {

                TmpPad.ShowInPrintMode = true;
                TmpPad.GenerateFromRow(int.Parse(FrmDrucken_Layout1.Text), Liste[0], false);
                TmpPad.ZoomFit();
            }

        }


        public void ShowAndCheckTabEinträgeWählen()
        {

            Tabs.TabPages[0].Enabled = false;
            Tabs.TabPages[1].Enabled = false;
            Tabs.TabPages[2].Enabled = true;
            Tabs.TabPages[3].Enabled = false;
            Tabs.TabPages[4].Enabled = false;
            Tabs.SelectedIndex = 2;

            WeiterEinträge.Enabled = Convert.ToBoolean(Liste.Count > 0);


            var MultiMöglich = true;
            //  Dim EinzelMöglich As Boolean = True


            if (Liste == null || Liste.Count == 0)
            {
                FrmDrucken_Info.Text = "Bitte wählen sie die Einträge für den Export.";

            }
            else
            {
                if (Liste.Count == 1)
                {
                    FrmDrucken_Info.Text = "Es ist genau ein Eintrag gewählt:<br> <b>-" + Liste[0].CellFirstString().Replace("\r\n", " ");
                    MultiMöglich = false;

                }
                else
                {
                    FrmDrucken_Info.Text = "Es sind <b>" + Liste.Count + "</b> Einträge gewählt.";

                }
            }


            if (Speichern.Checked && FrmDrucken_Layout1.Text.IsLong())
            {
                MultiMöglich = false;
            }


            if (MultiMöglich)
            {
                FrmDrucken_Zusammen.Enabled = true;
            }
            else
            {
                FrmDrucken_Einzeln.Checked = true;
                FrmDrucken_Zusammen.Enabled = false;
            }
        }



        private void BefülleLayoutDropdowns()
        {
            if (Database != null)
            {
                FrmDrucken_Layout1.Item.AddRange(Database, Speichern.Checked, _AdditionalLayoutPath);
            }
        }


        //Private Sub Drucken_Click(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles FrmDrucken_Drucken.Click
        //    GenerateLayout()
        //End Sub

        private void LayoutEditor_Click(object sender, System.EventArgs e)
        {

            Enabled = false;

            var n = FrmDrucken_Layout1.Text;

            FrmDrucken_Layout1.Text = string.Empty;
            tabAdministration.OpenLayoutEditor(Database, _AdditionalLayoutPath, n);
            FrmDrucken_Layout1.Item.Clear();
            BefülleLayoutDropdowns();

            if (FrmDrucken_Layout1.Item[n] != null)
            {
                FrmDrucken_Layout1.Text = n;
            }

            Enabled = true;
        }

        private void Button1_Click(object sender, System.EventArgs e)
        {
            modAllgemein.ExecuteFile(ZielPfad);
        }

        private void WeiterEinträge_Click(object sender, System.EventArgs e)
        {


            if (Drucken.Checked)
            {

                ShowDruckenEnde();
            }
            else
            {


                ShowSpeichernEnde();

                var l = new List<string>();

                if (FrmDrucken_Layout1.Text.IsLong())
                {
                    l = Export.SaveAsBitmap(Liste, int.Parse(FrmDrucken_Layout1.Text), ZielPfad, CreativePad.GenerateLayoutFromRow);
                }
                else
                {
                    l = Export.GenerateLayout_FileSystem(Liste, FrmDrucken_Layout1.Text, SaveTo, FrmDrucken_Zusammen.Checked, ZielPfad);
                }


                Exported.Item.AddRange(l);





            }


        }

        private void Weiter2_Click(object sender, System.EventArgs e)
        {
            ShowAndCheckTabEinträgeWählen();
        }



        public void ShowExportAktion()
        {
            Tabs.TabPages[0].Enabled = true;
            Tabs.TabPages[1].Enabled = false;
            Tabs.TabPages[2].Enabled = false;
            Tabs.TabPages[3].Enabled = false;
            Tabs.TabPages[4].Enabled = false;
            Tabs.SelectedIndex = 0;


            WeiterAktion.Enabled = Speichern.Checked || Drucken.Checked;
        }

        public void ShowDruckenEnde()
        {
            Tabs.TabPages[0].Enabled = false;
            Tabs.TabPages[1].Enabled = false;
            Tabs.TabPages[2].Enabled = false;
            Tabs.TabPages[3].Enabled = false;
            Tabs.TabPages[4].Enabled = true;
            Tabs.SelectedIndex = 4;
            GeneratePrintPad(0);


        }

        public void ShowSpeichernEnde()
        {
            Tabs.TabPages[0].Enabled = false;
            Tabs.TabPages[1].Enabled = false;
            Tabs.TabPages[2].Enabled = false;
            Tabs.TabPages[3].Enabled = true;
            Tabs.TabPages[4].Enabled = false;
            Tabs.SelectedIndex = 3;
        }




        private void FrmDrucken_Layout1_TextChanged(object sender, System.EventArgs e)
        {
            ShowAndCheckLayoutWahl();
        }

        private void Speichern_CheckedChanged(object sender, System.EventArgs e)
        {
            ShowExportAktion();
        }

        private void WeiterAktion_Click(object sender, System.EventArgs e)
        {
            ShowAndCheckLayoutWahl();
        }

        private void Exported_Item_Click(object sender, BasicListItemEventArgs e)
        {
            modAllgemein.ExecuteFile(e.Item.Internal());
        }

        private void FrmDrucken_Drucken_Click(object sender, System.EventArgs e)
        {
            Close();
        }

        private void Button_PageSetup_Click(object sender, System.EventArgs e)
        {
            PrintPad.ShowPrinterPageSetup();


            PrintPad.CopyPrinterSettingsToWorkingArea();

            GeneratePrintPad(0);
        }

        private void Druckenxxx_Click(object sender, System.EventArgs e)
        {
            ItemNrForPrint = 0;
            PrintPad.Print();
            // Den Rest mach 'PrintPad.PrintPage'
        }


        private void PrintPad_PrintPage(object sender, PrintPageEventArgs e)
        {
            ItemNrForPrint = GeneratePrintPad(ItemNrForPrint);


            e.HasMorePages = Convert.ToBoolean(ItemNrForPrint < Liste.Count);

            //If ItenNrForPrint >= Liste.Count Then
            //    e.HasMorePages = False
            //    'MessageBox.Show("Alle Druckaufträge abgeschickt", enImageCode.Information, "OK")
            //    'Exit Sub
            //End If


        }



        private void Vorschau_Click(object sender, System.EventArgs e)
        {
            PrintPad.ShowPrintPreview();
        }


        //Shared Sub Print(Row As RowItem)
        //    If Row Is Nothing Then
        //        MessageBox.Show("Kein Eintrag gewählt.", enImageCode.Information, "OK")
        //        Exit Sub
        //    End If

        //    If Row.Database.Layouts.Count = 0 Then
        //        MessageBox.Show("Kein druckbaren Layouts vorhanden.", enImageCode.Information, "OK")
        //        Exit Sub
        //    End If

        //    'Dim x As String = Row.Cell(Row.Database.Column.SysLastUsedLayout).String

        //    'If x.IsLong Then
        //    '    GenerateLayout_Internal(Row, Integer.Parse(x), True, False, String.Empty)
        //    'Else
        //    MessageBox.Show("Bitte erweitertes Drucken<br> und richtiges Layout wählen.", enImageCode.Information, "OK")
        //    '    GenerateLayout_Internal(Row, 0, True, False, String.Empty)
        //    '  End If
        //End Sub



        public int GeneratePrintPad(int StartNr)
        {
            PrintPad.ShowInPrintMode = false;
            PrintPad.Item.Clear();
            modAllgemein.CollectGarbage();




            var tmp = new CreativePad();

            tmp.GenerateFromRow(int.Parse(FrmDrucken_Layout1.Text), Liste[0], false);

            var OneItem = tmp.MaxBounds();

            PrintPad.SheetStyle = tmp.SheetStyle;
            PrintPad.SheetStyleScale = tmp.SheetStyleScale;

            tmp.Dispose();


            var DruckB = PrintPad.DruckbereichRect();


            var tempVar = (int)(Math.Floor(DruckB.Width / (double)OneItem.Width));
            for (var x = 0 ; x < tempVar ; x++)
            {
                var tempVar2 = (int)(Math.Floor(DruckB.Height / (double)OneItem.Height));
                for (var y = 0 ; y < tempVar2 ; y++)
                {


                    var It = new ChildPadItem();
                    It.PadInternal.GenerateFromRow(int.Parse(FrmDrucken_Layout1.Text), Liste[StartNr], false);

                    //Dim it As New RowFormulaPadItem(Liste(StartNr), Integer.Parse(FrmDrucken_Layout1.Text))
                    PrintPad.Item.Add(It);
                    It.SetCoordinates(new RectangleDF(DruckB.Left + x * OneItem.Width, DruckB.Top + y * OneItem.Height, OneItem.Width, OneItem.Height));


                    StartNr += 1;

                    if (FrmDrucken_Einzeln.Checked || StartNr >= Liste.Count)
                    {
                        break;
                    }
                }

                if (FrmDrucken_Einzeln.Checked || StartNr >= Liste.Count)
                {
                    break;
                }
            }

            PrintPad.ZoomFit();


            return StartNr;

        }


    }
}
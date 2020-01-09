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

using System;
using System.Collections.Generic;
using BlueBasics.Enums;
using static BlueBasics.Extensions;
using BlueControls.Forms;
using BlueDatabase;

namespace BlueControls.BlueDatabaseDialogs
{
    public sealed partial class Import
    {

        private readonly Database _Database;
        private readonly string OriTXT = "";


        public Import(Database Database, string TXT)
        {

            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();

            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
            OriTXT = TXT.Replace("\r\n", "\r").Trim("\r");

            var Ein = new List<string>();

            Ein.AddRange(OriTXT.SplitByCR());
            Eintr.Text = Ein.Count + " zum Importieren bereit.";

            _Database = Database;
        }



        private void Fertig_Click(object sender, System.EventArgs e)
        {

            var TR = string.Empty;

            if (TabStopp.Checked)
            {
                TR = "\t";
            }
            else if (Semikolon.Checked)
            {
                TR = ";";
            }
            else if (Komma.Checked)
            {
                TR = ",";
            }
            else if (Leerzeichen.Checked)
            {
                TR = " ";
            }
            else if (Andere.Checked)
            {
                TR = aTXT.Text;
            }

            if (string.IsNullOrEmpty(TR))
            {
                MessageBox.Show("Bitte Trennzeichen angeben.", enImageCode.Information, "OK");
                return;
            }

            DoImport(_Database, OriTXT, SpalteZuordnen.Checked, ZeilenZuorden.Checked, TR, Aufa.Checked, AnfTre.Checked, false);

            Close();
        }

        private void Cancel_Click(object sender, System.EventArgs e)
        {
            Close();
        }


        public static void DoImport(Database _Database, string TXT, bool SpalteZuordnenx, bool ZeileZuordnen, string ColumnSplitChar, bool EliminateMultipleSplitter, bool EleminateSplitterAtStart, bool SilentMode)
        {
            //DebugPrint_InvokeRequired(InvokeRequired, false);

            // Vorbereitung des Textes -----------------------------
            TXT = TXT.Replace("\r\n", "\r").Trim("\r");

            var ein = TXT.SplitByCR();
            var Zeil = new List<string[]>();
            var neuZ = 0;


            for (var z = 0; z <= ein.GetUpperBound(0); z++)
            {
                if (EliminateMultipleSplitter)
                {
                    ein[z] = ein[z].Replace(ColumnSplitChar + ColumnSplitChar, ColumnSplitChar);
                }
                if (EleminateSplitterAtStart)
                {
                    ein[z] = ein[z].TrimStart(ColumnSplitChar);
                }
                ein[z] = ein[z].TrimEnd(ColumnSplitChar);
                Zeil.Add(ein[z].SplitBy(ColumnSplitChar));
            }

            if (Zeil.Count == 0)
            {
                if (!SilentMode)
                {
                    MessageBox.Show("Import kann nicht ausgeführt werden.", enImageCode.Information, "Ok");
                }
                return;
            }


            var columns = new List<ColumnItem>();
            RowItem row = null;
            var StartZ = 0;

            // -------------------------------------
            // --- Spalten-Reihenfolge ermitteln ---
            // -------------------------------------
            if (SpalteZuordnenx)
            {
                StartZ = 1;


                for (var SpaltNo = 0; SpaltNo < Zeil[0].GetUpperBound(0) + 1; SpaltNo++)
                {
                    if (string.IsNullOrEmpty(Zeil[0][SpaltNo]))
                    {
                        if (!SilentMode) { MessageBox.Show("Abbruch,<br>leerer Spaltenname.", enImageCode.Information, "Ok"); }
                        return;
                    }

                    Zeil[0][SpaltNo] = Zeil[0][SpaltNo].Replace(" ", "_");
                    var Col = _Database.Column[Zeil[0][SpaltNo]];
                    if (Col == null)
                    {
                        Col = new ColumnItem(_Database, Zeil[0][SpaltNo], true);
                        Col.Caption = Zeil[0][SpaltNo];
                        Col.Format = enDataFormat.Text;
                    }
                    columns.Add(Col);

                }
            }
            else
            {
                foreach (var thisColumn in _Database.Column)
                {
                    if (thisColumn != null && string.IsNullOrEmpty(thisColumn.Identifier)) { columns.Add(thisColumn); }
                }

                while (columns.Count < Zeil[0].GetUpperBound(0) + 1)
                {
                    var newc = new ColumnItem(_Database, true);
                    newc.Caption = newc.Name;
                    newc.Format = enDataFormat.Text;
                    newc.MultiLine = true;
                    columns.Add(newc);
                }

            }


            // -------------------------------------
            // --- Importieren ---
            // -------------------------------------


            Progressbar P = null;

            if (!SilentMode) { P = Progressbar.Show("Importiere...", Zeil.Count - 1); }

            for (var ZeilNo = StartZ; ZeilNo < Zeil.Count; ZeilNo++)
            {
                P?.Update(ZeilNo);


                var tempVar2 = Math.Min(Zeil[ZeilNo].GetUpperBound(0) + 1, columns.Count);
                row = null;
                for (var SpaltNo = 0; SpaltNo < tempVar2; SpaltNo++)
                {

                    if (SpaltNo == 0)
                    {
                        row = null;
                        if (ZeileZuordnen && !string.IsNullOrEmpty(Zeil[ZeilNo][SpaltNo])) { row = _Database.Row[Zeil[ZeilNo][SpaltNo]]; }
                        if (row == null && !string.IsNullOrEmpty(Zeil[ZeilNo][SpaltNo]))
                        {
                            row = _Database.Row.Add(Zeil[ZeilNo][SpaltNo]);
                            neuZ += 1;
                        }
                    }
                    else
                    {
                        if (row != null)
                        {
                            row.CellSet(columns[SpaltNo], Zeil[ZeilNo][SpaltNo].SplitBy("|").JoinWithCr());
                        }
                    }

                }
            }

            P?.Close();


            if (!SilentMode)
            {
                BlueControls.Forms.MessageBox.Show("<b>Import abgeschlossen.</b>\r\n" + neuZ.ToString() + " neue Zeilen erstellt.");
            }

        }




    }
}
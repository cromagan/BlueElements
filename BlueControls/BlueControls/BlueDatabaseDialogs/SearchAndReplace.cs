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
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.DialogBoxes;
using BlueDatabase;
using BlueDatabase.EventArgs;

namespace BlueControls.BlueDatabaseDialogs
{

    internal sealed partial class SearchAndReplace
    {

        private readonly Table _BlueTable;
        private bool IsWorking;

        public SearchAndReplace(Table table)
        {

            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();

            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
            _BlueTable = table;
            _BlueTable.CursorPosChanged += CursorPosChanged;
            CursorPosChanged(_BlueTable, new CellEventArgs(_BlueTable.CursorPosColumn(), _BlueTable.CursorPosRow()));

        }




        private void CursorPosChanged(object sender, CellEventArgs e)
        {

            if (e.Column == null)
            {
                NurinAktuellerSpalte.Text = "Nur in der <b>aktuell gewählten Spalte</b> ersetzen.";
            }
            else
            {
                NurinAktuellerSpalte.Text = "Nur in Spalte <b>'" + e.Column.ReadableText() + "'</b> ersetzen.";
            }

            Checkbuttons();
        }

        private void ers_Click(object sender, System.EventArgs e)
        {

            if (IsWorking) { return; }

            var SuchText = Alt.Text.Replace(";cr;", "\r").Replace(";tab;", "\t");
            var ErsetzText = Neu.Text.Replace(";cr;", "\r").Replace(";tab;", "\t");

            _BlueTable.Database.OnConnectedControlsStopAllWorking(new DatabaseStoppedEventArgs());


            var sp = new List<ColumnItem>();
            var ro = new List<RowItem>();

            if (NurinAktuellerSpalte.Checked)
            {
                sp.Add(_BlueTable.CursorPosColumn());
            }
            else
            {
                foreach (var ThisColumn in _BlueTable.Database.Column)
                {
                    if (ThisColumn != null && ThisColumn.Format.CanBeChangedByRules()) { sp.Add(ThisColumn); }
                }
            }


            foreach (var ThisRow in _BlueTable.Database.Row)
            {
                if (ThisRow != null)
                {
                    if (!AktuelleFilterung.Checked || ThisRow.MatchesTo(_BlueTable.Filter))
                    {
                        if (!AbgeschlosseZellen.Checked || !ThisRow.CellGetBoolean(_BlueTable.Database.Column.SysLocked)) { ro.Add(ThisRow); }
                    }
                }
            }


            var count = 0;
            var GeändeterText = "";

            var co = 0;

            var P = Progressbar.Show("Ersetze...", ro.Count);

            foreach (var ThisRow in ro)
            {
                var RowChanged = false;
                co += 1;
                P.Update(co);


                foreach (var Thiscolumn in sp)
                {

                    var trifft = false;
                    var OriginalText = ThisRow.CellGetString(Thiscolumn);

                    if (SucheNach.Checked)
                    {
                        trifft = OriginalText.Contains(SuchText);

                    }
                    else if (SucheExact.Checked)
                    {
                        trifft = Convert.ToBoolean(OriginalText == SuchText);

                    }
                    else if (InhaltEgal.Checked)
                    {
                        trifft = true;
                    }


                    if (trifft)
                    {
                        if (ErsetzeMit.Checked)
                        {
                            GeändeterText = OriginalText.Replace(SuchText, ErsetzText);
                        }
                        else if (ErsetzeKomplett.Checked)
                        {
                            GeändeterText = ErsetzText;
                        }
                        else if (FügeHinzu.Checked)
                        {
                            var tmp = new List<string>(OriginalText.SplitByCR());
                            tmp.Add(ErsetzText);
                            GeändeterText = tmp.SortedDistinctList().JoinWithCr();
                        }


                        if (GeändeterText != OriginalText)
                        {
                            RowChanged = true;
                            count += 1;
                            ThisRow.CellSet(Thiscolumn, GeändeterText);
                        }
                    }


                }

                if (RowChanged) { ThisRow.DoAutomatic(false, true); }
            }

            P?.Close();
            MessageBox.Show(count + " Ersetzungen vorgenommen.", enImageCode.Information, "OK");


            IsWorking = false;

        }



        private void Alt_TextChange(object sender, System.EventArgs e)
        {
            Checkbuttons();
        }

        private void Something_CheckedChanged(object sender, System.EventArgs e)
        {
            Checkbuttons();
        }


        private void Checkbuttons()
        {
            var CanDo = true;
            if (_BlueTable == null) { return; }


            if (!_BlueTable.Database.IsAdministrator()) { CanDo = false; }

            if (SucheNach.Checked)
            {
                if (string.IsNullOrEmpty(Alt.Text)) { CanDo = false; }
                Alt.Enabled = true;
                ErsetzeMit.Enabled = true;
                NurinAktuellerSpalte.Enabled = true;

            }
            else if (SucheExact.Checked)
            {
                Alt.Enabled = true;
                ErsetzeMit.Enabled = false;
                if (ErsetzeMit.Checked) { CanDo = false; }
                NurinAktuellerSpalte.Enabled = true;


            }
            else if (InhaltEgal.Checked)
            {
                Alt.Enabled = false;
                ErsetzeMit.Enabled = false;

                NurinAktuellerSpalte.Enabled = false; // Zu Riskant
                if (!NurinAktuellerSpalte.Checked) { CanDo = false; }// Zu Riskant

                if (ErsetzeMit.Checked) { CanDo = false; }

            }
            else
            {
                CanDo = false;
            }



            if (ErsetzeMit.Checked)
            {


            }
            else if (ErsetzeKomplett.Checked)
            {
                NurinAktuellerSpalte.Enabled = false; // Zu Riskant
                if (!NurinAktuellerSpalte.Checked) { CanDo = false; }// Zu Riskant

            }
            else if (FügeHinzu.Checked)
            {
                if (string.IsNullOrEmpty(Neu.Text)) { CanDo = false; }
            }
            else
            {
                CanDo = true;
            }


            if (NurinAktuellerSpalte.Checked)
            {


                if (_BlueTable.CursorPosColumn() == null)
                {
                    CanDo = false;
                }
                else
                {
                    if (!_BlueTable.CursorPosColumn().Format.CanBeChangedByRules()) { CanDo = false; }
                }
            }


            if (Alt.Text == Neu.Text)
            {
                if (!InhaltEgal.Checked) { CanDo = false; }
                if (!ErsetzeKomplett.Checked) { CanDo = false; }
            }

            ers.Enabled = CanDo;
        }

        protected override void OnFormClosing(System.Windows.Forms.FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            _BlueTable.CursorPosChanged -= CursorPosChanged;
        }

    }
}

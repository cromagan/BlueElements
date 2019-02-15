using System;
using System.Collections.Generic;
using BlueBasics.Enums;
using static BlueBasics.Extensions;
using BlueControls.DialogBoxes;
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


            for (var z = 0 ; z <= ein.GetUpperBound(0) ; z++)
            {
                if (EliminateMultipleSplitter)
                {
                    ein[z] = ein[z].Replace(ColumnSplitChar + ColumnSplitChar, ColumnSplitChar);
                }
                if (EliminateMultipleSplitter)
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


            var colsx = new List<ColumnItem>();
            RowItem R = null;
            var StartZ = 0;

            // -------------------------------------
            // --- Spalten-Reihenfolge ermitteln ---
            // -------------------------------------
            if (SpalteZuordnenx)
            {
                StartZ = 1;


                for (var SpaltNo = 0 ; SpaltNo < Zeil[0].GetUpperBound(0)+1 ; SpaltNo++)
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
                        Col = _Database.Column.Add(Zeil[0][SpaltNo]);
                        Col.Caption = Zeil[0][SpaltNo];
                        Col.Format = enDataFormat.Text;
                    }
                    colsx.Add(Col);

                }
            }
            else
            {
                foreach (var thisColumn in _Database.Column)
                {
                    if (thisColumn != null && string.IsNullOrEmpty(thisColumn.Identifier)) { colsx.Add(thisColumn); }
                }

                while(colsx.Count < Zeil[0].GetUpperBound(0)+1)
                {
                    var newc = _Database.Column.Add(_Database.Column.Freename("NEUE_SPALTE"));
                    newc.Format = enDataFormat.Text;
                    newc.MultiLine = true;
                    colsx.Add(newc);
                }

            }


            // -------------------------------------
            // --- Importieren ---
            // -------------------------------------


            Progressbar P = null;

            if (!SilentMode) { P = Progressbar.Show("Importiere...", Zeil.Count - 1); }

            for (var ZeilNo = StartZ ; ZeilNo < Zeil.Count ; ZeilNo++)
            {
                P?.Update(ZeilNo);


                 var tempVar2 = Math.Min(Zeil[ZeilNo].GetUpperBound(0) + 1, colsx.Count);
                for (var SpaltNo = 0 ; SpaltNo < tempVar2 ; SpaltNo++)
                {

                    if (SpaltNo == 0)
                    {
                        R = null;
                        if (ZeileZuordnen && !string.IsNullOrEmpty(Zeil[ZeilNo][SpaltNo])) { R = _Database.Row[Zeil[ZeilNo][SpaltNo]]; }
                        if (R == null)
                        {
                            R = _Database.Row.Add(Zeil[ZeilNo][SpaltNo]);
                            neuZ += 1;
                        }
                    }
                    else
                    {

                        R.CellSet(colsx[SpaltNo], Zeil[ZeilNo][SpaltNo].SplitBy("|").JoinWithCr());
                    }

                }
            }

            P?.Close();


            if (!SilentMode)
            {
                BlueControls.DialogBoxes.MessageBox.Show("<b>Import abgeschlossen.</b>\r\n" + neuZ.ToString() + " neue Zeilen erstellt.");
            }

        }




    }
}
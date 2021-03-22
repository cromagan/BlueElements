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

using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueDatabase.Enums;
using System;
using System.Collections.Generic;

namespace BlueDatabase {

    public sealed class RuleItem_Old : IParseable, IReadableTextWithChanging, ICompareKey {

        #region  Variablen-Deklarationen 
        public readonly Database Database;
        #endregion


        #region  Event-Deklarationen + Delegaten 
        public event EventHandler Changed;
        #endregion


        #region  Construktor + Initialize 


        private void Initialize() {
            if (Actions.Count > 0) { Actions.Clear(); }
            Actions.Changed += Actions_ListOrItemChanged;
        }


        public RuleItem_Old(Database database, string CodeToParse) {
            Database = database;
            Parse(CodeToParse);
        }

        public RuleItem_Old(Database database) {
            Database = database;
            Initialize();
        }

        public RuleItem_Old(ColumnItem column) {
            Database = column.Database;
            Initialize();
        }


        #endregion


        #region  Properties 

        public bool IsParsing { get; private set; }

        public ListExt<RuleActionItem> Actions { get; } = new ListExt<RuleActionItem>();


        #endregion


        //public bool IsNullOrEmpty() {
        //    if (!IsOk()) { return true; }
        //    return false;
        //}


        public override string ToString() {

            string Result = "";

            //   if (!string.IsNullOrEmpty(_SystemKey)) { Result = Result + "SK=" + _SystemKey.ToNonCritical() + ", "; }


            foreach (RuleActionItem ThisAction in Actions) {
                if (ThisAction != null) {
                    Result = Result + ", Aktion=" + ThisAction;
                }
            }


            return "{" + Result.TrimStart(", ") + "}";
        }


        public void Parse(string ToParse) {
            IsParsing = true;
            Actions.ThrowEvents = false;

            Initialize();

            foreach (KeyValuePair<string, string> pair in ToParse.GetAllTags()) {
                switch (pair.Key) {
                    case "sk": // TODO: alt 28.03.2019 , löschen
                               //  _SystemKey = pair.Value.FromNonCritical();
                        break;

                    case "aktion":
                        Actions.Add(new RuleActionItem(this, pair.Value));
                        break;
                    default:
                        Develop.DebugPrint(enFehlerArt.Fehler, "Tag unbekannt: " + pair.Key);
                        break;
                }
            }

            Actions.ThrowEvents = true;
            IsParsing = false;
        }




        //public string ErrorReason() {
        //    //var VorschlagsRegel = false;

        //    //   if (!string.IsNullOrEmpty(_SystemKey)) { return string.Empty; }

        //    var Dann = AnzahlDanns();

        //    if (Dann == 0) { return "Es ist keine 'Dann-Aktion' vorhanden."; }


        //    foreach (var ThisAction in Actions) {
        //        if (ThisAction != null) {
        //            //if (!ThisAction.IsOk()) { return "Eine Aktion ist fehlerhaft."; }

        //            //switch (ThisAction.Action)
        //            //{

        //            //    case enAction.Erhält_den_Focus:
        //            //        VorschlagsRegel = true;
        //            //        break;
        //            //}
        //        }
        //    }

        //    //if (VorschlagsRegel)
        //    //{
        //    //    if (Dann != 1) { return "Eine Regel für eine neue Zeile oder einen Vorschlag benötigt genau eine 'Dann-Aktion'."; }

        //    //    foreach (var ThisAction in Actions)
        //    //    {
        //    //        if (ThisAction != null && !ThisAction.IsBedingung() && ThisAction.Action != enAction.Mache_einen_Vorschlag) { return "Nur 'Mache einen Vorschlag' bei neuen Focus-Aktionen möglich."; }
        //    //    }
        //    //}

        //    return string.Empty;
        //}


        //public bool IsOk() {
        //    return string.IsNullOrEmpty(ErrorReason());
        //}

        public int AnzahlWenns() {
            int W = 0;

            if (Actions.Count == 0) { return 0; }

            foreach (RuleActionItem ThisAction in Actions) {
                if (ThisAction != null && ThisAction.IsBedingung()) { W++; }
            }

            return W;
        }

        public int AnzahlDanns() {
            int W = 0;
            if (Actions.Count == 0) { return 0; }

            foreach (RuleActionItem ThisAction in Actions) {
                if (ThisAction != null && !ThisAction.IsBedingung()) { W++; }
            }


            return W;
        }


        public QuickImage SymbolForReadableText() {

            //if (!IsOk()) { return QuickImage.Get(enImageCode.Kritisch); }

            if (AnzahlWenns() > 1) { return null; }

            foreach (RuleActionItem ThisAction in Actions) {
                if (ThisAction != null) {
                    if (ThisAction.IsBedingung()) { return ThisAction.SymbolForReadableText(); }
                }
            }


            return QuickImage.Get(enImageCode.Häkchen);
        }


        public string ReadableText() {
            // if (!IsOk()) { return "Fehlerhafte Regel wird ignoriert: " + ErrorReason(); }


            List<string>[] Txts = new List<string>[2];
            Txts[0] = new List<string>();
            Txts[1] = new List<string>();

            string hasAnmerkung = string.Empty;


            foreach (RuleActionItem ThisAction in Actions) {
                if (ThisAction != null) {

                    if (ThisAction.Action == enAction.Anmerkung) {
                        hasAnmerkung = ThisAction.ReadableText() + "  ";

                    } else if (ThisAction.IsBedingung()) {
                        Txts[0].Add(ThisAction.ReadableText());
                    } else {
                        Txts[1].Add(ThisAction.ReadableText());
                    }
                }
            }


            string[] GiveB = new string[2];


            for (int z = 0; z <= 1; z++) {
                switch (Txts[z].Count) {
                    case 1:
                        GiveB[z] = Txts[z][0];
                        break;
                    case 0:
                        GiveB[z] = "";
                        break;
                    default: {
                        GiveB[z] = Txts[z][0];
                        int tempVar = Txts[z].Count - 2;
                        for (int z1 = 1; z1 <= tempVar; z1++) {
                            GiveB[z] = GiveB[z] + ", " + Txts[z][z1];
                        }
                        GiveB[z] = GiveB[z] + " und " + Txts[z][Txts[z].Count - 1];
                        break;
                    }
                }
            }


            if (!string.IsNullOrEmpty(GiveB[0]) && !string.IsNullOrEmpty(GiveB[1])) {
                return hasAnmerkung + "Wenn " + GiveB[0] + ", dann " + GiveB[1] + ".";
            }

            return hasAnmerkung + "Mache immer: " + GiveB[1];
        }


        //public bool TrifftZu(RowItem vRow) {
        //    foreach (var ThisAction in Actions) {
        //        if (ThisAction != null && ThisAction.IsBedingung() && !ThisAction.TrifftZu(vRow)) { return false; }
        //    }
        //    return true;
        //}


        //public string Execute() {

        //    var Meldung = string.Empty;


        //    //foreach (var ThisAction in Actions) {
        //    //    if (ThisAction != null) {
        //    //        if (!ThisAction.IsBedingung()) {
        //    //            if (string.IsNullOrEmpty(Meldung)) {
        //    //                Meldung = ThisAction.Execute(vRow);
        //    //            }
        //    //            else {
        //    //                ThisAction.Execute(vRow);
        //    //            }
        //    //        }
        //    //    }
        //    //}

        //    //if (string.IsNullOrEmpty(Meldung)) { return string.Empty; }
        //    ////if (FeedbackMode == enControlAccesMode.NoFeedBack)
        //    ////{
        //    ////    return "Dummy-Fehler mit Dummy Spalte '#Spalte:" + Database.Column[0].Name + "'";
        //    ////}


        //    var e = new List<string>();

        //    foreach (var ThisAction in Actions) {
        //        if (ThisAction != null && ThisAction.IsBedingung()) { e.Add(ThisAction.TrifftZuText(vRow)); }
        //    }

        //    e = e.SortedDistinctList();


        //    var m = "";
        //    if (Meldung == "*") {
        //        if (e.Count == 0) { return " Diese Zeile wird immer als fehlerhaft markiert."; }
        //    }
        //    else {
        //        m = Meldung + "<DELETE>";
        //    }


        //    for (var z = 0; z < e.Count; z++) {
        //        if (z == 0) {
        //            m = m + e[z].Substring(0, 1).ToUpper() + e[z].Substring(1);
        //        }
        //        else {
        //            m = m + "<br><b> - O D E R - </b><br>" + e[z];
        //        }
        //    }


        //    return m + ".";
        //}


        //public int CompareTo(object obj) {
        //    if (obj is RuleItem RLI) {
        //        return CompareKey().CompareTo(RLI.CompareKey());
        //    }
        //    else {
        //        Develop.DebugPrint(enFehlerArt.Fehler, "Falscher Objecttyp!");
        //        return 0;
        //    }
        //}


        public string CompareKey() {


            int MaxColumnIndex = -1;
            int MaxCode = -1;

            foreach (RuleActionItem ThisAction in Actions) {
                if (ThisAction != null) {
                    foreach (ColumnItem ThisColumnItem in ThisAction.ColumnsAllUsed()) {
                        if (ThisColumnItem != null) {
                            MaxColumnIndex = Math.Max(ThisColumnItem.Index(), MaxColumnIndex);
                        }
                    }
                }


                int Co = -1;
                switch (ThisAction.Action) {
                    case 0:
                        Co = 1;
                        break; // Neue Action
                    case enAction.Anmerkung:
                        Co = 2;
                        break;
                    //case enAction.Ist_der_Nutzer: Co = 3; break;
                    case enAction.Setze_Fehlerhaft:
                        Co = 4;
                        break;
                    case enAction.Sperre_die_Zelle:
                        Co = 5;
                        break;
                    case enAction.Wert_Setzen:
                        Co = 10;
                        break;
                    case enAction.Wert_Dazu:
                        Co = 20;
                        break;
                    case enAction.Wert_Weg:
                        Co = 30;
                        break;


                    case enAction.Berechne:
                        Co = 37;
                        break;


                    case enAction.Ist:
                        Co = 100;
                        break;
                    case enAction.Ist_Nicht:
                        Co = 101;
                        break;
                    case enAction.Enthält:
                        Co = 102;
                        break;
                    case enAction.Enthält_Zeichenkette:
                        Co = 103;
                        break;
                    case enAction.Enthält_NICHT_Zeichenkette:
                        Co = 104;
                        break;
                    case enAction.Formatfehler_des_Zelleninhaltes:
                        Co = 105;
                        break;
                    case enAction.Enthält_ungültige_Zeichen:
                        Co = 106;
                        break;
                    case enAction.Unsichtbare_Zeichen_am_Ende_Enthält:
                        Co = 107;
                        break;
                    case enAction.Auf_eine_existierende_Datei_verweist:
                        Co = 108;
                        break;
                    case enAction.Auf_einen_existierenden_Pfad_verweist:
                        Co = 109;
                        break;
                    case enAction.Berechnung_ist_True:
                        Co = 110;
                        break;
                    case enAction.Substring:
                        Co = 115;
                        break;

                    default:
                        Co = 0;
                        Develop.DebugPrint(ThisAction.Action);
                        break;
                }

                MaxCode = Math.Max(MaxCode, Co);

            }

            // um wirklich die Reihenfolge immer gleich zu halen auch ein ToString
            return (MaxColumnIndex + 1).ToString(Constants.Format_Integer3) + (MaxCode + 1).ToString(Constants.Format_Integer3) + ToString();


        }


        //public void Repair() {

        //    if (!IsOk()) { return; }

        //    Actions.RemoveNullOrEmpty();
        //    Actions.Sort();

        //}



        //public object Clone() {

        //    return new RuleItem(Database, ToString());
        //}


        //internal bool Contains(ColumnItem newName) {
        //    // Wichtig, daß Rechenformeln richtiggestellt werden
        //    if (Database != newName.Database) { Develop.DebugPrint(enFehlerArt.Fehler, "Datenbanken inkonsitent"); }


        //    foreach (var Thisaction in Actions) {
        //        if (Thisaction.Contains(newName)) { return true; }
        //    }
        //    return false;
        //}

        //internal void RenameColumn(string oldName, ColumnItem newName) {
        //    // Wichtig, daß Rechenformeln richtiggestellt werden
        //    if (Database != newName.Database) { Develop.DebugPrint(enFehlerArt.Fehler, "Datenbanken inkonsitent"); }


        //    foreach (var Thisaction in Actions) {
        //        Thisaction.RenameColumn(oldName, newName);
        //    }
        //}

        //internal bool BlockEditing(ColumnItem Column, RowItem Row) {
        //    foreach (var ThisAction in Actions) {
        //        if (ThisAction != null) {
        //            if (ThisAction.Action == enAction.Sperre_die_Zelle && ThisAction.Columns.Contains(Column)) {
        //                var Match = TrifftZu(Row);
        //                if (Match) { return true; }
        //            }
        //        }
        //    }
        //    return false;
        //}

        internal bool WillAlwaysCellOverride(ColumnItem Column) {

            if (AnzahlWenns() > 0) { return false; }

            foreach (RuleActionItem ThisAction in Actions) {
                if (ThisAction != null) {
                    //if (ThisAction.Action != enAction.KopiereAndereSpalten)
                    //{
                    if (ThisAction.Columns.Contains(Column)) { return true; }
                    //}
                }
            }
            return false;
        }

        internal void Actions_ListOrItemChanged(object sender, System.EventArgs e) {
            OnChanged();
        }



        public void OnChanged() {
            if (IsParsing) { Develop.DebugPrint(enFehlerArt.Warnung, "Falscher Parsing Zugriff!"); return; }
            Changed?.Invoke(this, System.EventArgs.Empty);
        }

        internal string ToScript() {


            List<ColumnItem> c = new List<ColumnItem>();

            string txt = "\r\n\r\n// --------------------------------------------------------\r\n";
            txt += "// " + ReadableText();
            txt += "\r\n// --------------------------------------------------------\r\n";


            string a = string.Empty;
            string e = string.Empty;

            foreach (RuleActionItem thisAktion in Actions) {

                (string anfang, string ende) = thisAktion.ToScript(c);

                a += anfang + "\r\n";

                if (!string.IsNullOrEmpty(ende)) {
                    e = ende + "\r\n" + e;
                }


            }




            return txt + a + e.TrimStart("\r\n");


        }
    }
}
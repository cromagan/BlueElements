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

namespace BlueDatabase
{
    public sealed class RuleActionItem : IParseable, IReadableTextWithChanging, IComparable, ICompareKey, ICloneable
    {
        #region  Variablen-Deklarationen 

        public readonly RuleItem_Old Rule;
        private enAction _Action;
        private string _Text;

        #endregion


        #region  Event-Deklarationen + Delegaten 
        public event EventHandler Changed;
        #endregion


        #region  Construktor + Initialize 


        private void Initialize()
        {
            _Action = 0;
            _Text = string.Empty;
            Columns.Clear(); // = New List(Of ColumnItem)
            Columns.Changed += Columns_ListOrItemChanged;
        }


        public RuleActionItem(RuleItem_Old RL, enAction Action, string Text, ColumnItem Column)
        {
            Rule = RL;
            Initialize();
            _Action = Action;
            _Text = Text;
            if (Column != null) { Columns.Add(Column); }
        }

        public RuleActionItem(RuleItem_Old RL, string Code)
        {
            Rule = RL;
            Parse(Code);
        }

        #endregion

        internal void Columns_ListOrItemChanged(object sender, System.EventArgs e)
        {
            OnChanged();
        }


        #region  Properties 

        public bool IsParsing { get; private set; }

        public enAction Action
        {
            get => _Action;
            set
            {
                if (_Action == value) { return; }
                _Action = value;
                OnChanged();
            }
        }


        public string Text
        {
            get => _Text;
            set
            {
                if (_Text == value) { return; }
                _Text = value;
                OnChanged();
            }
        }


        public ListExt<ColumnItem> Columns { get; } = new ListExt<ColumnItem>();

        #endregion


        //public bool IsNullOrEmpty() {
        //    if (!IsOk()) { return true; }
        //    return false;
        //}









        public int CompareTo(object obj)
        {
            if (obj is RuleActionItem RAI)
            {
                // hierist es egal, ob es ein DoAlways ist oder nicht. Es sollen nur Bedingugen VOR Aktionen kommen
                return CompareKey().CompareTo(RAI.CompareKey());
            }
            else
            {
                Develop.DebugPrint(enFehlerArt.Fehler, "Falscher Objecttyp!");
                return 0;
            }


        }


        public string CompareKey()
        {
            var MaxColumnIndex = -1;
            var Co = -1;


            foreach (var ThisColumnItem in ColumnsAllUsed())
            {
                if (ThisColumnItem != null)
                {
                    MaxColumnIndex = Math.Max(ThisColumnItem.Index(), MaxColumnIndex);
                }
            }

            switch (_Action)
            {
                case 0:
                    Co = 0;
                    break;// Neue Action
                case enAction.Anmerkung:
                    Co = 5;
                    break;

                //Bedingungen
                //case enAction.Erhält_den_Focus: Co = 8; break;
                //case enAction.Ist_der_Nutzer: Co = 10; break;
                case enAction.Ist:
                    Co = 20;
                    break;
                case enAction.Ist_Nicht:
                    Co = 30;
                    break;
                case enAction.Enthält:
                    Co = 40;
                    break;
                case enAction.Enthält_Zeichenkette:
                    Co = 50;
                    break;
                case enAction.Enthält_NICHT_Zeichenkette:
                    Co = 55;
                    break;
                case enAction.Formatfehler_des_Zelleninhaltes:
                    Co = 60;
                    break;
                case enAction.Enthält_ungültige_Zeichen:
                    Co = 70;
                    break;
                case enAction.Unsichtbare_Zeichen_am_Ende_Enthält:
                    Co = 80;
                    break;
                case enAction.Auf_eine_existierende_Datei_verweist:
                    Co = 90;
                    break;
                case enAction.Auf_einen_existierenden_Pfad_verweist:
                    Co = 100;
                    break;
                case enAction.Berechnung_ist_True:
                    Co = 110;
                    break;
                //case enAction.Ist_Jünger_Als: Co = 120; break;

                //Während der Bearbeitung
                //case enAction.Mache_einen_Vorschlag: Co = 190; break;

                // Einfache Aktionen
                case enAction.Wert_Setzen:
                    Co = 200;
                    break;
                case enAction.Wert_Dazu:
                    Co = 210;
                    break;
                case enAction.Wert_Weg:
                    Co = 220;
                    break;

                // Interaktion mit andern Zellen / Datenbanken
                case enAction.Berechne:
                    Co = 250;
                    break;
                case enAction.Substring:
                    Co = 260;
                    break;
                //case enAction.LinkedCell: Co = 270; break;
                //case enAction.SortiereIntelligent: Co = 290; break;
                //  case enAction.KopiereAndereSpalten: Co = 295; break;

                // Finaler Abschluss
                case enAction.Sperre_die_Zelle:
                    Co = 300;
                    break;
                case enAction.Setze_Fehlerhaft:
                    Co = 310;
                    break;



                case enAction.Skript:
                    Co = 400;
                    break;
                default:
                    Develop.DebugPrint(_Action);
                    break;
            }



            // um wirklich die Reihenfolge immer gleich zu halen auch ein ToString
            return (Co + 1).ToString(Constants.Format_Integer3) + (MaxColumnIndex + 1).ToString(Constants.Format_Integer3) + ToString();
        }

        public override string ToString()
        {

            var Result = "{Action=" + (int)_Action;

            foreach (var t in Columns)
            {
                if (t != null)
                {
                    Result = Result + ", " + t.ParsableColumnKey();
                }
            }

            if (!string.IsNullOrEmpty(_Text))
            {
                Result = Result + ", Text=" + _Text.ToNonCritical();
            }


            return Result + "}";
        }

        public void Parse(string ToParse)
        {
            IsParsing = true;
            Columns.ThrowEvents = false;
            Initialize();
            foreach (var pair in ToParse.GetAllTags())
            {
                switch (pair.Key)
                {
                    case "identifier": //TODO: Identifier entferneen, altlast. 06.09.2019
                        if (pair.Value != "Action") { Develop.DebugPrint(enFehlerArt.Fehler, "Identifier fehlerhaft: " + pair.Value); }
                        break;

                    case "action":
                        _Action = (enAction)int.Parse(pair.Value);
                        break;

                    case "column":
                    case "columnname": // Columname wichtg, wegen CopyLayout
                        Columns.Add(Rule.Database.Column[pair.Value]);
                        break;

                    case "columnkey":
                        Columns.Add(Rule.Database.Column.SearchByKey(int.Parse(pair.Value)));
                        break;

                    case "text":
                        _Text = pair.Value.FromNonCritical();
                        break;

                    default:
                        Develop.DebugPrint(enFehlerArt.Fehler, "Tag unbekannt: " + pair.Key);
                        break;
                }
            }

            Columns.ThrowEvents = true;
            IsParsing = false;
        }

        internal (string anfang, string ende) ToScript(List<ColumnItem> c)
        {

            var txtList = Text.SplitByCRToList();
            var txtJoinedKomma = "\"" + txtList.JoinWith("\", \"").Trim(", \"") + "\"";
            //var txtListString = "{\"" + txtJoinedKomma + "}";
            var txtJoined = txtList.JoinWith(";").Trim(";");


            var ct = "Exception(\"Nicht oder unvollständig konvertiert!\");\r\n// Benutzte Spalten:";

            foreach (var thisc in Columns)
            {
                ct = ct + " " + thisc.Name;
                c.Add(thisc);
            }
            ct = ct + "\r\n// Original-Text: " + txtJoined + "\r\n";


            var cnl = string.Empty;

            foreach (var thisc in c)
            {
                cnl = cnl + thisc.Name + ", ";
            }
            cnl = cnl.Trim(", ");


            switch (_Action)
            {
                case enAction.Ist:

                    if (txtList is null || txtList.Count == 0)
                    {
                        return ("if (IsNullOrEmpty(" + Columns[0].Name + ")) {", "}");
                    }

                    if (txtList.Count > 1)
                    {
                        //var tmpn = "tmpList_" + Columns[0].Name;
                        //var tmpt = "var " + tmpn + " = " + txtListString + ";";
                        return ("if (Contains(" + Columns[0].Name + ", false, " + txtJoinedKomma + ") { ", "} ");
                    }



                    if (Columns[0].Format.IsZahl())
                    {
                        return ("if (" + Columns[0].Name + " == " + Text + ") {", "}");
                    }

                    if (Columns[0].Format == enDataFormat.Bit)
                    {

                        if (Text == "+")
                        {
                            return ("if (" + Columns[0].Name + ") {", "}");
                        }
                        if (Text == "-")
                        {
                            return ("if (!" + Columns[0].Name + ") {", "}");
                        }

                        return (ct, "");
                    }


                    return ("if (" + Columns[0].Name + " == \"" + Text + "\") {", "}");


                case enAction.Setze_Fehlerhaft:

                    if (string.IsNullOrEmpty(Text))
                    {
                        return ("SetError(\"Allgemeiner Fehler.\",  " + cnl + "); // TODO: Text korrigieren", "");
                    }

                    return ("SetError(\"" + Text.Replace("\r\n", "").Replace("\r", "") + "\",  " + cnl + ");", "");

                case enAction.Ist_Nicht:
                    if (txtList is null || txtList.Count == 0)
                    {
                        return ("if (!IsNullOrEmpty(" + Columns[0].Name + ")) {", "}");
                    }

                    if (txtList is null || txtList.Count == 1)
                    {
                        return ("if (" + Columns[0].Name + " != \"" + txtList[0] + "\") {", "}");
                    }

                    return (ct, "");


                case enAction.Wert_Setzen:
                    if (Columns.Count == 0) { return (ct, ""); }
                    var bigt = string.Empty;



                    foreach (var thisc in Columns)
                    {
                        var thist = string.Empty;

                        if (thisc.Format.IsZahl() || thisc.Format == enDataFormat.FarbeInteger)
                        {
                            thist = thisc.Name + " = " + txtJoined + ";";
                        }

                        if (thisc.MultiLine)
                        {
                            thist = thisc.Name + " = " + txtList + ";";
                        }


                        if (thisc.Format == enDataFormat.Text || thisc.Format == enDataFormat.LinkedCell || thisc.Format == enDataFormat.BildCode || thisc.Format == enDataFormat.Datum_und_Uhrzeit)
                        {
                            thist = thisc.Name + " = \"" + txtJoined + "\";";
                        }

                        if (thisc.Format == enDataFormat.Bit)
                        {
                            if (Text == "+") { thist = thisc.Name + " = true;"; }
                            if (Text == "-") { thist = thisc.Name + " = false;"; }

                        }
                        if (!string.IsNullOrEmpty(thist))
                        {
                            bigt = bigt + thist + "\r\n";
                        }
                        else
                        {
                            bigt = bigt + ct;
                        }

                    }

                    return (bigt, "");

                case enAction.Substring:
                case enAction.Berechne:
                    if (Columns.Count != 1) { return (ct, ""); }
                    return (ct + Columns[0].Name + " = " + txtJoined.Replace("&", " + ").Replace(";", "") + ";", "");


                case enAction.Unsichtbare_Zeichen_am_Ende_Enthält:
                    return ("if (EndsWith(" + Columns[0].Name + ", false, \" \", \"\\r\", \"\\n\", \"\\t\")) {", "}");

                case enAction.Enthält:
                case enAction.Enthält_Zeichenkette:
                    var s = "if (";
                    foreach (var thisc in Columns)
                    {
                        s = s + "Contains(" + Columns[0].Name + ", false, " + txtJoinedKomma + ") ||";
                    }
                    s = s.TrimEnd(" ||") + ") {";
                    return (s, "}");

                case enAction.Enthält_NICHT_Zeichenkette:
                    var s2 = "if (";
                    foreach (var thisc in Columns)
                    {
                        s2 = s2 + "!Contains(" + Columns[0].Name + ", false, " + txtJoinedKomma + ") &&";
                    }
                    s2 = s2.TrimEnd(" &&") + ") {";
                    return (s2, "}");


                case enAction.Wert_Dazu:
                    if (Columns.Count != 1) { return (ct, ""); }
                    return ("Add(" + Columns[0].Name + ", " + txtJoinedKomma + ");", "");

                case enAction.Wert_Weg:
                    if (Columns.Count != 1) { return (ct, ""); }
                    return ("Remove(" + Columns[0].Name + ", false,  " + txtJoinedKomma + ");", "");


                case enAction.Anmerkung:
                    return ("// #### " + txtJoined + " ####", "");

                case enAction.Formatfehler_des_Zelleninhaltes:
                case enAction.Enthält_ungültige_Zeichen:
                case enAction.Berechnung_ist_True:
                    return (ct + "if (false) { // Deaktivierter Inhalt", "}");

                case enAction.Sperre_die_Zelle:
                    return (ct, "");

                default:
                    return (ct, "");


            }


        }





        //private static double? MatheErgebnis(string Formel, RowItem Row) {

        //    // Formel Vorbereiten ----------------
        //    Formel = Formel.ToUpper();
        //    Formel = Formel.RemoveChars(" \r\n");

        //    Formel = Row.ReplaceVariablesForMath(Formel);

        //    Formel = Formel.ToUpper();
        //    Formel = Formel.RemoveChars(" \r\n");

        //    return modErgebnis.Ergebnis(Formel);
        //}




        public QuickImage SymbolForReadableText()
        {
            //if (!IsOk()) { return QuickImage.Get(enImageCode.Kritisch); }


            switch (_Action)
            {
                case enAction.Ist:
                    if (string.IsNullOrEmpty(_Text))
                    {
                        return QuickImage.Get(enImageCode.Datei, 16, "00FF00", "");
                    }
                    else
                    {
                        return QuickImage.Get(enImageCode.Textdatei, 16, "0000FF", "");
                    }

                case enAction.Ist_Nicht:
                    if (string.IsNullOrEmpty(_Text))
                    {
                        return QuickImage.Get("Datei|16||1|00FF00");
                    }
                    else
                    {
                        return QuickImage.Get("Textdatei|16||1|0000FF");
                    }

                case enAction.Formatfehler_des_Zelleninhaltes:
                    return QuickImage.Get("Formel|16||1");
                case enAction.Enthält_ungültige_Zeichen:
                    return QuickImage.Get("TasteProzent|16||1");
                case enAction.Enthält:
                    return QuickImage.Get("Pfeil_Rechts|16");
                case enAction.Enthält_Zeichenkette:
                    return QuickImage.Get(enImageCode.Undo);
                case enAction.Enthält_NICHT_Zeichenkette:
                    return QuickImage.Get("Undo|16||1");
                case enAction.Setze_Fehlerhaft:
                    return QuickImage.Get(enImageCode.Warnung);
                case enAction.Wert_Setzen:
                    return QuickImage.Get(enImageCode.PlusZeichen);
                case enAction.Wert_Dazu:
                    return QuickImage.Get(enImageCode.PlusZeichen, 16, "0000FF", "");
                case enAction.Wert_Weg:
                    return QuickImage.Get(enImageCode.MinusZeichen, 16, "0000FF", "");
                case enAction.Berechne:
                    return QuickImage.Get(enImageCode.Formel);
                case enAction.Substring:
                    return QuickImage.Get(enImageCode.TasteABC);
                case enAction.Auf_eine_existierende_Datei_verweist:
                    return QuickImage.Get(enImageCode.Ordner, 16, "00FF00", "");
                case enAction.Auf_einen_existierenden_Pfad_verweist:
                    return QuickImage.Get(enImageCode.Ordner);
                //case enAction.Erhält_den_Focus:
                //    return QuickImage.Get(enImageCode.Gänsefüßchen);
                //case enAction.Mache_einen_Vorschlag:
                //    return QuickImage.Get(enImageCode.Sonne);
                case enAction.Unsichtbare_Zeichen_am_Ende_Enthält:
                    return QuickImage.Get(enImageCode.Blitz);
                case enAction.Sperre_die_Zelle:
                    return QuickImage.Get("Stift|16||1");
                //case enAction.Ist_der_Nutzer:
                //    return QuickImage.Get(enImageCode.Person);
                case enAction.Berechnung_ist_True:
                    return QuickImage.Get(enImageCode.Binärdaten);
                case enAction.Anmerkung:
                    return QuickImage.Get(enImageCode.Information);
                //case enAction.LinkedCell:
                //    return QuickImage.Get(enImageCode.Fernglas);
                //case enAction.Ist_Jünger_Als:
                //    return QuickImage.Get(enImageCode.Uhr);
                //case enAction.SortiereIntelligent:
                //    return QuickImage.Get(enImageCode.Lupe);
                //case enAction.KopiereAndereSpalten:
                //    return QuickImage.Get(enImageCode.Clipboard);

                case enAction.Skript:
                    return QuickImage.Get(enImageCode.Skript);

                default:
                    if (_Action > 0) { Develop.DebugPrint(_Action); }
                    return QuickImage.Get(enImageCode.Kritisch);
            }
        }


        //public static void MaximalText(Database IrgendeineDatebank, enAction Action, ref string rtext, ref QuickImage rSym) {
        //    var tmpa = new RuleActionItem(null, Action, string.Empty, null); // HIER NOTHING! WEil sonst eine Änderun ausgelöst wird, wo keine ist

        //    tmpa.Columns.Clear(); // = New List(Of ColumnItem)
        //    tmpa._Text = "TEXT";


        //    if (!tmpa.IsOk()) {
        //        tmpa._Text = "TEXT";
        //        tmpa.Columns.Clear(); // = New List(Of ColumnItem)
        //        tmpa.Columns.Add(IrgendeineDatebank.Column[0]);
        //    }

        //    if (!tmpa.IsOk()) {
        //        tmpa._Text = "TEXT\rTEXT2"; // Für Analyse
        //    }


        //    if (!tmpa.IsOk()) {
        //        tmpa._Text = "TEXT\rTEXT2\rTEXT3"; // Einheit, Spalte, Einheitenspalte
        //    }

        //    if (!tmpa.IsOk()) {
        //        tmpa._Text = "";
        //    }

        //    if (!tmpa.IsOk()) {
        //        tmpa._Text = "";
        //        tmpa.Columns.Clear();
        //    }


        //    if (!tmpa.IsOk()) {
        //        tmpa._Text = "2";
        //        tmpa.Columns.Clear();
        //        tmpa.Columns.Add(IrgendeineDatebank.Column[0]);
        //    }

        //    if (!tmpa.IsOk()) {
        //        tmpa._Text = "";
        //        tmpa.Columns.Clear();
        //        tmpa.Columns.Add(IrgendeineDatebank.Column[0]);
        //        tmpa.Columns.Add(IrgendeineDatebank.Column[1]);
        //    }



        //    if (!tmpa.IsOk()) {
        //        tmpa._Text = "2";
        //        tmpa.Columns.Clear();
        //    }


        //    if (tmpa.IsOk()) {
        //        rtext = "..." + tmpa.ReadableText();

        //        rtext = rtext.Replace("'" + IrgendeineDatebank.Column[0].ReadableText() + "'", "'SPALTE'");
        //        rtext = rtext.Replace("'" + IrgendeineDatebank.Column[1].ReadableText() + "'", "'SPALTE'");
        //        //rtext = rtext.Replace(" 2 Kom", " ?? Kom");
        //        //rtext = rtext.Replace(" 2 Stu", " ?? Stu");
        //        rSym = tmpa.SymbolForReadableText();

        //    } else {
        //        Develop.DebugPrint(enFehlerArt.Fehler, "Unbekannte Action: " + Action);

        //    }


        //}



        //public static enNeededColumns NeededColumns(string Action) {

        //    if (string.IsNullOrEmpty(Action)) { return enNeededColumns.None; }
        //    if (!Action.IsLong()) { return enNeededColumns.None; }

        //    var x = (enAction)int.Parse(Action);
        //    if (x.ToString() == ((int)x).ToString()) { return enNeededColumns.None; }

        //    return NeededColumns(x);
        //}

        //public static enNeededText NeededText(string Action) {
        //    if (string.IsNullOrEmpty(Action)) { return enNeededText.None; }
        //    if (!Action.IsLong()) { return enNeededText.None; }

        //    var x = (enAction)int.Parse(Action);
        //    if (x.ToString() == ((int)x).ToString()) {
        //        return enNeededText.None;
        //    }

        //    return NeededText(x);
        //}

        //public static enNeededColumns NeededColumns(enAction Action) {


        //    switch (Action) {
        //        case enAction.Ist:
        //            return enNeededColumns.OneOrMore;
        //        //case enAction.Erhält_den_Focus:
        //        //    return enNeededColumns.OnlyOne;
        //        //case enAction.Mache_einen_Vorschlag:
        //        //    return enNeededColumns.OneOrMore;
        //        case enAction.Ist_Nicht:
        //            return enNeededColumns.OneOrMore;
        //        case enAction.Enthält_ungültige_Zeichen:
        //            return enNeededColumns.OneOrMore;
        //        case enAction.Formatfehler_des_Zelleninhaltes:
        //            return enNeededColumns.OneOrMore;
        //        //    Case Is = enAction.Zeile_wird_initialisiertx : Return enNeededColumns.None
        //        case enAction.Auf_eine_existierende_Datei_verweist:
        //            return enNeededColumns.OnlyOne;
        //        case enAction.Auf_einen_existierenden_Pfad_verweist:
        //            return enNeededColumns.OnlyOne;
        //        case enAction.Enthält:
        //            return enNeededColumns.OneOrMore;
        //        case enAction.Enthält_Zeichenkette:
        //            return enNeededColumns.OneOrMore;
        //        case enAction.Enthält_NICHT_Zeichenkette:
        //            return enNeededColumns.OnlyOne;
        //        case enAction.Setze_Fehlerhaft:
        //            return enNeededColumns.None;
        //        case enAction.Wert_Setzen:
        //            return enNeededColumns.OneOrMore;
        //        case enAction.Sperre_die_Zelle:
        //            return enNeededColumns.OneOrMore;
        //        case enAction.Berechne:
        //            return enNeededColumns.OnlyOne;
        //        case enAction.Substring:
        //            return enNeededColumns.OnlyOne;
        //        case enAction.Wert_Dazu:
        //            return enNeededColumns.OneOrMore;
        //        case enAction.Wert_Weg:
        //            return enNeededColumns.OneOrMore;
        //        case enAction.Anmerkung:
        //            return enNeededColumns.DoesNotMatter;
        //        //case enAction.Ist_der_Nutzer:
        //        //    return enNeededColumns.None;
        //        case enAction.Berechnung_ist_True:
        //            return enNeededColumns.None;
        //        case enAction.Unsichtbare_Zeichen_am_Ende_Enthält:
        //            return enNeededColumns.OneOrMore;
        //        //case enAction.Ist_Jünger_Als:
        //        //    return enNeededColumns.None;
        //        //case enAction.SortiereIntelligent:
        //        //    return enNeededColumns.MoreThanOne;
        //        //case enAction.KopiereAndereSpalten:
        //        //    return enNeededColumns.TwoColumns;
        //        case enAction.Skript:
        //            return enNeededColumns.None;


        //        default:
        //            Develop.DebugPrint(Action);

        //            break;
        //    }

        //    return enNeededColumns.None;
        //}

        //public static enNeededText NeededText(enAction Action) {


        //    switch (Action) {
        //        case enAction.Ist:
        //            return enNeededText.DoesNotMatter;
        //        //case enAction.Erhält_den_Focus:
        //        //    return enNeededText.None;
        //        //case enAction.Mache_einen_Vorschlag:
        //        //    return enNeededText.None;
        //        case enAction.Ist_Nicht:
        //            return enNeededText.MaxOneLine;
        //        case enAction.Enthält_ungültige_Zeichen:
        //            return enNeededText.None;
        //        case enAction.Formatfehler_des_Zelleninhaltes:
        //            return enNeededText.None;
        //        case enAction.Auf_eine_existierende_Datei_verweist:
        //            return enNeededText.None;
        //        case enAction.Auf_einen_existierenden_Pfad_verweist:
        //            return enNeededText.None;
        //        case enAction.Enthält:
        //            return enNeededText.OneOrMore;
        //        case enAction.Enthält_Zeichenkette:
        //            return enNeededText.OneOrMore;
        //        case enAction.Enthält_NICHT_Zeichenkette:
        //            return enNeededText.OneOrMore;
        //        case enAction.Unsichtbare_Zeichen_am_Ende_Enthält:
        //            return enNeededText.None;
        //        case enAction.Setze_Fehlerhaft:
        //            return enNeededText.DoesNotMatter;
        //        case enAction.Wert_Setzen:
        //            return enNeededText.DoesNotMatter;
        //        case enAction.Sperre_die_Zelle:
        //            return enNeededText.None;
        //        case enAction.Berechnung_ist_True:
        //            return enNeededText.Rechenformel1;
        //        case enAction.Berechne:
        //            return enNeededText.Rechenformel1;
        //        case enAction.Substring:
        //            return enNeededText.Rechenformel2;
        //        case enAction.Wert_Dazu:
        //            return enNeededText.OneOrMore;
        //        case enAction.Wert_Weg:
        //            return enNeededText.OneOrMore;
        //        //case enAction.Ist_der_Nutzer:
        //        //    return enNeededText.OneOrMore;
        //        case enAction.Anmerkung:
        //            return enNeededText.DoesNotMatter;

        //        case enAction.Skript:
        //            return enNeededText.DoesNotMatter;
        //        //case enAction.Ist_Jünger_Als:
        //        //    return enNeededText.OneIntegerValue;
        //        //case enAction.SortiereIntelligent:
        //        //    return enNeededText.None;
        //        //case enAction.KopiereAndereSpalten:
        //        //    return enNeededText.None;



        //        default:
        //            Develop.DebugPrint(Action);

        //            break;
        //    }

        //    return enNeededText.None;
        //}


        //public static string HelpTextinHTML(Database IrgedeineDatenbank, enAction Action) {


        //    //Dim x As enAction = CType(CInt(Rule_Aktion.Text), enAction)
        //    //If x.ToString = Microsoft.VisualBasic.Val(x).ToString Then Exit Sub


        //    var t = string.Empty;
        //    QuickImage s = null;
        //    MaximalText(IrgedeineDatenbank, Action, ref t, ref s);

        //    t = t.Trim('.');
        //    t = t.Trim(' ');

        //    var r = "<b><u>Information</b></u><br><br>";


        //    if ((int)Action < 1000) {
        //        r = r + "<b>Bedingung:<br>Wenn </b>" + t.Trim('.') + "<b>, dann </b>...<br>";
        //        r += "<i> - Bedingungen können nicht alleine stehen, sie benötigen immer eine auszuführende Aktion<br> - Bedingungen prüfen nur und ändern nichts</i><br>";
        //    } else {
        //        r = r + "<b>Aktion:<br>Wenn </b>..., <b> dann </b>" + t.Trim('.') + ".<br>";
        //        r += "<i> - Aktion können alleine stehen, evtl. ist dadurch eine manuelle Zellenbearbeitung nicht mehr möglich<br> - Aktionen haben immer Auswirkungen auf eine oder mehrere Zellen</i><br>";
        //    }

        //    r += "<br><br><b>Beschreibung:</b><br>";


        //    var MehrereTexteReichtEiner = false;
        //    var EineSpalteWahrUmWahrzusein = false;

        //    var MehrTexteAllegeamacht = false;
        //    var MehrSpaltenAllegeamacht = false;


        //    switch (Action) {
        //        case enAction.Anmerkung:
        //            r += "Anmerkung ist ohne Funktion und nur für Notizen gedacht.";
        //            break;

        //        case enAction.Ist:
        //            r = r + "Diese Bedingung ist <i>WAHR</i>, wenn der hier angegebene Text exakt so <b>in der Zelle</b> steht.<br>" +
        //                    "Wird hier kein Text eingegeben, ist <i>WAHR</i>, wenn die Zelle komplett leer ist.";
        //            EineSpalteWahrUmWahrzusein = true;
        //            break;

        //        case enAction.Ist_Nicht:
        //            r = r + "Diese Bedingung ist <i>WAHR</i>, wenn der hier angegebene Text vom Zelleninhalt abweicht.<br>" +
        //                    "Wird hier kein Text eingegeben, ist <i>WAHR</i>, wenn die Zelle befüllt ist.";
        //            EineSpalteWahrUmWahrzusein = true;
        //            break;

        //        case enAction.Enthält:
        //            r += "Diese Bedingung ist <i>WAHR</i>, wenn der hier angegebene Text exakt so <b>in einer Zeile der Zelle</b> steht.";
        //            MehrereTexteReichtEiner = true;
        //            EineSpalteWahrUmWahrzusein = true;
        //            break;

        //        case enAction.Enthält_Zeichenkette:
        //            r += "Diese Bedingung ist <i>WAHR</i>, wenn der hier angegebene Text exakt so <b>irgendwo in der Zelle</b> steht.<br>Gross/Kleinschreibung wird ignoriert.";
        //            EineSpalteWahrUmWahrzusein = true;
        //            break;

        //        case enAction.Enthält_NICHT_Zeichenkette:
        //            r += "Diese Bedingung ist <i>WAHR</i>, wenn der hier angegebene Text exakt so  <b>nicht in der Zelle</b> steht.<br>Gross/Kleinschreibung wird ignoriert.";
        //            MehrereTexteReichtEiner = true;
        //            break;


        //        case enAction.Formatfehler_des_Zelleninhaltes:
        //            r = r + "Diese Bedingung ist <i>WAHR</i>, wenn der <b>Inhalt der Zelle</b> nicht mit dem geforderten<br>" +
        //                    "Format der jeweiligen Spalte übereinstimmt.";
        //            EineSpalteWahrUmWahrzusein = true;
        //            break;

        //        case enAction.Enthält_ungültige_Zeichen:
        //            r = r + "Diese Bedingung ist <i>WAHR</i>, wenn <b>im Inhalt der Zelle</b> mindestens ein Zeichen enthalten ist,<br>" +
        //                    "die laut Definiton der jeweiligen Spalte nicht erlaubt ist.";
        //            EineSpalteWahrUmWahrzusein = true;
        //            break;

        //        case enAction.Setze_Fehlerhaft:
        //            r = r + "Zeilen sind standardmässig fehlerfrei. Durch diese Aktion wird die Zeile als fehlerhaft markiert.<br>" +
        //                    "Wird ein Text angegeben, wird dieser als Fehlermeldung zurückgegen.<br>" +
        //                    "Wird kein Text angegeben, wird ein Standard-Fehler-Text angezeigt.";
        //            break;

        //        case enAction.Auf_eine_existierende_Datei_verweist:
        //            r += "Diese Bedingung ist <i>WAHR</i>, wenn der <b>Inhalt der Zelle</b> auf eine existierende Datei im Windows-Dateisystem verweist.";
        //            break;

        //        case enAction.Auf_einen_existierenden_Pfad_verweist:
        //            r = r + "Diese Bedingung ist <i>WAHR</i>, wenn der <b>Inhalt der Zelle</b> auf einen existierenden Dateipfad im Windows-Dateisystem verweist.<br>" +
        //                    "Dateinamen oder Dateinamen mit Dateipfaden sind nicht <i>WAHR</i>.";
        //            break;

        //        //case enAction.Erhält_den_Focus:
        //        //    r = r + "Diese Bedingung ist <i>WAHR</i>, wenn das Bearbeitungsfeld in der Formularansicht den Fokus erhält.<br>" +
        //        //            "Nur in Kombination mit <b>Vorschlag machen</b> erlaubt.";
        //        //    break;

        //        case enAction.Unsichtbare_Zeichen_am_Ende_Enthält:
        //            r = r + "Diese Bedingung ist <i>WAHR</i>, wenn am Ende des <b>Inhalts der Zelle</b> Leerzeichen oder Zeilenumbruchcodes sind. <br>" +
        //                    "Damit können unötige Zeichen abgefagen werden.";
        //            EineSpalteWahrUmWahrzusein = true;
        //            break;

        //        //case enAction.Ist_der_Nutzer:
        //        //    r = r + "Diese Bedingung ist <i>WAHR</i>, wenn der Benutzer zu einer Benutzergruppe gehört, die im Text definiert ist.<br>" +
        //        //            "Hier trifft die Benutzergruppe #Administrator nicht automatisch und muss separat angegeben werden.<br>" +
        //        //            "Einzelne Nutzer können direkt mit #USER:Max Mustermann angesprochen werden.";
        //        //    MehrereTexteReichtEiner = true;
        //        //    break;


        //        //case enAction.Ist_Jünger_Als:
        //        //    r = r + "Diese Bedingung ist <i>WAHR</i>, das Erstelldatum der Zeile jünger ist, als die im Textfeld angegebene Stunden-Zahl.";
        //        //    break;


        //        case enAction.Wert_Dazu:
        //            r += "Diese Aktion fügt neue Werte hinzu, ohne die alten zu löschen. Anschließende wird sortiert und eventuell doppelt Werte entfernt.";
        //            MehrTexteAllegeamacht = true;
        //            MehrSpaltenAllegeamacht = true;
        //            break;

        //        case enAction.Wert_Weg:
        //            r += "Diese Aktion entfernt die hier angegeben Werte hinzu, ohne die alten zu löschen. Anschließende wird sortiert und eventuell doppelte Werte entfernt.";
        //            MehrTexteAllegeamacht = true;
        //            MehrSpaltenAllegeamacht = true;
        //            break;

        //        case enAction.Wert_Setzen:
        //            r += "Diese Aktion überschreibt den Text in der Zelle mit dem hier angegeben Text.";
        //            MehrSpaltenAllegeamacht = true;
        //            break;

        //        //case enAction.Mache_einen_Vorschlag:
        //        //    r = r + "Diese Aktion benötigt die vorherige Bedingung <b>erhält den Focus</b>.<br>" +
        //        //            "Die hier angegebenen Spalten werden verglichen, ob diese bereits in einer Zeile der Datenbank vorkommen.<br>" +
        //        //            "Falls ja, wird der Wert, der in dieser Zeile enthalten ist, in das Feld im Formular geschrieben.";
        //        //    break;

        //        case enAction.Berechne:
        //            r = r + "Diese Aktion berechnet einen Wert. Dazu muss hier im Textfeld eine Formel eingegeben werden.<br>" +
        //                    "Inhalte anderer Spalten der gleichen Zeile können ebenfalls angesprochen werden.<br>" +
        //                    "Dazu wird der interne Spaltenname als Variable benutzt. <i><u>Beispiel:</u> &amp;SPALTENNAME;</i><br>" +
        //                    "Unterstützt werden Operatoren (< > =), Funktionen (Int, IFF, Min, Max), Klammern, Addition, Subtraktion, Muliplikation und Divison.<br>" +
        //                    "Kann die Formel nicht berechnet werden, wird die Zeile als fehlerhaft markiert.<br>" +
        //                    "<i><u>Beispiel:</u> ( &amp;Spalte1; + 10,5 )  / ( &amp;Spalte2; - &amp;Spalte3; ) * &amp;Spalte4;</i>";
        //            break;

        //        case enAction.Berechnung_ist_True:
        //            r = r + "Diese Bedingung berechent das Ergebnis. Wenn am Ende der Wert -1 (=True) steht, trifft die Bedingung zu.<br>" +
        //                    "Inhalte anderer Spalten der gleichen Zeile können ebenfalls angesprochen werden.<br>" +
        //                    "Dazu wird der interne Spaltenname als Variable benutzt. <i><u>Beispiel:</u> &amp;SPALTENNAME;</i><br>" +
        //                    "Unterstützt werden Operatoren (< > =), Funktionen (Int, IFF, Min, Max), Klammern, Addition, Subtraktion, Muliplikation und Divison.<br>" +
        //                    "Kann die Formel nicht berechnet werden, wird die Zeile als fehlerhaft markiert.<br>" +
        //                    "<i><u>Beispiel:</u> ( &amp;Spalte1; + 10,5 )  / ( &amp;Spalte2; - &amp;Spalte3; ) * &amp;Spalte4;</i>";
        //            break;

        //        case enAction.Substring:
        //            //r = r + "Diese Aktion setzt einen Text mit Inhalten aus anderen Zellen zusammen.<br>" +
        //            //        "Dazu wird der interne Spaltenname als Variable benutzt. <i><u>Beispiel:</u> &amp;SPALTENNAME;</i><br>" +
        //            //        "Zusätzlich können auch nur Teilbereiche benutzt werden <i><u>Beispiel:</u> &amp;SPALTENNAME(L,1);</i><br>" +
        //            //        "Es sind die Buchstaben L für links, M für Mitte und R für rechts möglich:<br>" +
        //            //        "&amp;SPALTENNAME(L,3) = Die ersten 3 Zeichen<br>" +
        //            //        "&amp;SPALTENNAME(R,3) = Die letzten 5 Zeichen<br>" +
        //            //        "&amp;SPALTENNAME(M,3,5) = Ab dem dritten Zeichen 5 Zeichen<br>" +
        //            //        "Diese Berechnung erzeugt keine Fehler.";
        //            r = r + "Diese Aktion setzt einen Text mit Inhalten aus anderen Zellen zusammen.<br>" +
        //                    "Dazu wird der interne Spaltenname als Variable benutzt. <i><u>Beispiel:</u> &amp;SPALTENNAME;</i><br>" +
        //                    "Zusätzlich können auch nur Teilbereiche benutzt werden <i><u>Beispiel:</u> &amp;SPALTENNAME(L,1);</i><br>" +
        //                    "&amp;SPALTENNAME(L,3) = Die ersten 3 Zeichen<br>" +
        //                    "Diese Berechnung erzeugt keine Fehler.";
        //            break;

        //        case enAction.Sperre_die_Zelle:
        //            r += "Diese Aktion sperrt die <b>manuelle</b> Bearbeitung für dieser Zelle.";
        //            MehrSpaltenAllegeamacht = true;
        //            break;

        //        case enAction.Skript:
        //            r += " ";
        //            //MehrSpaltenAllegeamacht = true;
        //            break;

        //        //case enAction.SortiereIntelligent:
        //        //    r = r + "Diese Aktion sortiert die Einträge der gewwählten Spalten automatisch in die richtigen Spalten ein.";
        //        //    break;


        //        //case enAction.KopiereAndereSpalten:
        //        //    r = r + "Diese Aktion benötigt eine Spalte vom Typ 'Relation' und eine normale Spalte. Alle mittels 'Relations' miteinander verknüpften Zeilen, erhalten den gleichen Wert der zweiten Spalte.";
        //        //    break;




        //        default:
        //            Develop.DebugPrint(Action);

        //            break;
        //    }

        //    if (MehrereTexteReichtEiner) {
        //        r += "<br>Werden hier mehrere Textzeilen eingegeben, wird jede Textzeile für sich geprüft. Um <i>WAHR</i> zu sein, reicht es, wenn <b>eine</b> Textzeile zutrifft.";
        //    }

        //    if (EineSpalteWahrUmWahrzusein) {
        //        r += "<br>Werden hier mehrere Spalten ausgewählt, reicht es wenn <b>eine</b> der Spalten <i>WAHR</i> zurückgibt, um <i>WAHR</i> zu sein.";
        //    }

        //    if (MehrTexteAllegeamacht) {
        //        r += "<br>Werden hier mehrere Textzeilen eingegeben, wird jede Textzeile für sich abgearbeitet.";
        //    }

        //    if (MehrSpaltenAllegeamacht) {
        //        r += "<br>Werden hier mehrere Spalten ausgewählt, werden alle abgearbeitet.";
        //    }

        //    return r;
        //}


        //public string ErrorReason() {
        //    if (_Action == enAction.Sperre_die_Zelle) { return string.Empty; }

        //    foreach (var t in Columns) {
        //        if (t == null) { return " Veraltete Spalten sind angewählt"; }

        //        if (IsBedingung()) {
        //            if (!t.Format.CanBeCheckedByRules()) { return "Aus der Spalte '" + t.ReadableText() + "' kann generell nicht gelesen werden"; }
        //        } else {
        //            if (!t.Format.CanBeChangedByRules()) { return "Die Spalte '" + t.ReadableText() + "' kann generell nicht beschrieben werden"; }
        //        }
        //    }

        //    if (_Action == 0) { return "Keine Aktion gewählt"; }

        //    switch (NeededColumns(_Action)) {
        //        case enNeededColumns.DoesNotMatter:
        //            break;
        //        case enNeededColumns.None:
        //            if (Columns.Count > 0) { return "Es darf keine Spalte angewählt werden"; }
        //            break;
        //        case enNeededColumns.MoreThanOne:
        //            if (Columns.Count < 2) { return "Es müssen mindestens zwei Spalten angewählt werden"; }
        //            break;
        //        case enNeededColumns.OneOrMore:
        //            if (Columns.Count == 0) { return "Es muss mindestens eine Spalte angewählt werden"; }
        //            break;
        //        case enNeededColumns.OnlyOne:
        //            if (Columns.Count != 1) { return "Es muss genau eine Spalte angewählt werden"; }
        //            break;
        //        case enNeededColumns.TwoColumns:
        //            if (Columns.Count != 2) { return "Es müssen genau zwei Spalten angewählt werden"; }
        //            break;
        //        default:
        //            Develop.DebugPrint(NeededColumns(_Action));
        //            break;
        //    }


        //    switch (NeededText(_Action)) {
        //        case enNeededText.DoesNotMatter:
        //            break;
        //        case enNeededText.None:
        //            if (!string.IsNullOrEmpty(_Text)) { return "Es darf kein Text eingegeben werden"; }
        //            break;

        //        case enNeededText.MaxOneLine:
        //            if (_Text.Contains("\r")) { return "Der Text darf maximal eine Zeile haben"; }
        //            break;

        //        case enNeededText.OneOrMore:
        //            if (string.IsNullOrEmpty(_Text)) { return "Es muss ein Text eingegeben werden"; }
        //            break;

        //        case enNeededText.Rechenformel1:
        //            if (string.IsNullOrEmpty(_Text)) { return "Es muss eine Rechenformel im Textfeld eingegeben werden"; }
        //            break;


        //        case enNeededText.Rechenformel2:
        //            if (string.IsNullOrEmpty(_Text)) { return "Es muss ein Text-Baukasten im Textfeld eingegeben werden"; }
        //            break;

        //        case enNeededText.OneIntegerValue:
        //            if (string.IsNullOrEmpty(_Text)) { return "Es muss eine Zahl eingegeben werden"; }
        //            if (!_Text.IsFormat(enDataFormat.Ganzzahl)) { return "Es muss eine Zahl eingegeben werden"; }
        //            break;

        //        default: {
        //                Develop.DebugPrint(NeededText(_Action));
        //                break;
        //            }
        //    }


        //    return string.Empty;
        //}


        public string ReadableText()
        {

            var ColsOder = "";
            var ColsUnd = "";

            //var dd = ErrorReason();
            //if (!string.IsNullOrEmpty(dd)) { return "Aktion fehlerhaft: " + dd; }


            for (var z = 0; z < Columns.Count; z++)
            {
                if (z == Columns.Count - 2)
                {
                    ColsOder = ColsOder + "'" + Columns[z].ReadableText() + "' oder ";
                    ColsUnd = ColsUnd + "'" + Columns[z].ReadableText() + "' und ";
                }
                else
                {
                    ColsUnd = ColsUnd + "'" + Columns[z].ReadableText() + "', ";
                    ColsOder = ColsOder + "'" + Columns[z].ReadableText() + "', ";
                }

                //  COls += 1
            }


            ColsOder = ColsOder.TrimEnd(", ");
            ColsUnd = ColsUnd.TrimEnd(", ");


            _Text = _Text.Replace("\r\n", "\r"); // sollte eh schon richtig sein...


            switch (_Action)
            {

                case enAction.Anmerkung:
                    return " ##### ANMERKUNG ##### ";

                case enAction.Ist:
                    if (string.IsNullOrEmpty(_Text)) { return "die Zelle in " + ColsOder + " leer ist"; }
                    return "die gesamte Zelle in " + ColsOder + " genau der Text '" + _Text.Replace("\r", "' oder '") + "' ist";

                //case enAction.Erhält_den_Focus:
                //    return "die Zelle in " + ColsOder + " den Focus erhält";

                case enAction.Berechnung_ist_True:
                    return "die mathematische Berechnung der Formel '" + _Text + "' WAHR ist";

                //case enAction.Mache_einen_Vorschlag:
                //    return "mache einen Vorschlag, bezugnehmend auf " + ColsUnd + "";


                case enAction.Ist_Nicht:
                    if (string.IsNullOrEmpty(_Text)) { return "die Zelle in " + ColsOder + " befüllt ist"; }
                    return "die Zelle in " + ColsOder + " NICHT der Text '" + _Text.Replace("\r", "' oder '") + "' ist";

                case enAction.Enthält_ungültige_Zeichen:
                    return "die Zelle in " + ColsOder + " nicht erlaubte Zeichen enthält";

                case enAction.Formatfehler_des_Zelleninhaltes:
                    return "die Zelle in " + ColsOder + " vom Format abweicht";


                case enAction.Auf_eine_existierende_Datei_verweist:
                    return "die Zelle in  " + ColsOder + " auf eine existierende Datei verweist";

                case enAction.Auf_einen_existierenden_Pfad_verweist:
                    return "die Zelle in  " + ColsOder + " auf eine existierenden Dateipfad verweist";

                case enAction.Enthält:
                    return "einer der Werte in der Zelle " + ColsOder + " genau der Text '" + _Text.Replace("\r", "' oder '") + "' ist";

                case enAction.Enthält_Zeichenkette:
                    return "die Zelle in " + ColsOder + " die Zeichenkette '" + _Text.Replace("\r", "' oder '") + "' enthält";

                case enAction.Enthält_NICHT_Zeichenkette:
                    return "die Zelle in " + ColsOder + " NICHT die Zeichenkette '" + _Text.Replace("\r", "' oder '") + "' enthält";

                //case enAction.Ist_der_Nutzer:
                //    return "einer der Nutzer '" + _Text.Replace("\r", "' oder '") + "' ist";

                case enAction.Unsichtbare_Zeichen_am_Ende_Enthält:
                    return "die Zelle in  " + ColsOder + " am Ende Leerzeichen oder Enter-Codes enthält";

                case enAction.Setze_Fehlerhaft:
                    if (string.IsNullOrEmpty(_Text)) { return "markiere die Zeile als fehlerhaft"; }
                    return "markiere die Zeile als fehlerhaft, mit speziellen Fehlertext";

                case enAction.Wert_Setzen:
                    if (string.IsNullOrEmpty(_Text)) { return "lösche den Text in der Zelle " + ColsUnd; }
                    return "ändere den Wert in der Zelle " + ColsUnd + " in '" + _Text.Replace("\r", "'/'") + "' ab";

                case enAction.Berechne:
                    return "berechne die Zelle in " + ColsUnd + " mit einer Formel";

                case enAction.Substring:
                    return "ändere den Text der Zelle in " + ColsUnd + " mit einer Formel";

                //case enAction.SortiereIntelligent:
                //    return "Teile die Inhalte von " + ColsUnd + " intelligent auf";


                case enAction.Wert_Dazu:
                    return "füge in der Zelle " + ColsUnd + " '" + _Text.Replace("\r", "' und '") + "' hinzu";

                case enAction.Wert_Weg:
                    return "entferne in der Zelle " + ColsUnd + " '" + _Text.Replace("\r", "' und '") + "'";


                case enAction.Sperre_die_Zelle:
                    return "sperre die Bearbeitung der Zelle " + ColsUnd;



                case enAction.Skript:
                    return "#### Skript ####";


                //case enAction.Ist_Jünger_Als:
                //    return "die Zeile jünger als " + _Text + " Stunde(n) ist";


                //case enAction.LinkedCell:
                //    return "ändere die ID der verlinkten Datenbank in " + ColsUnd;

                //case enAction.KopiereAndereSpalten:
                //    return "halte die Inhalte der verknüpften Spalten von " + ColsUnd + " gleich";

                case 0:
                    // Neue Aktion
                    break;
                default:
                    Develop.DebugPrint(_Action);
                    break;
            }
            return string.Empty;
        }

        //public string TrifftZuText(RowItem vRow) {

        //    string[] w;
        //    if (string.IsNullOrEmpty(_Text)) {
        //        w = new string[1]; //MUSS mindestens eines haben, daß auf Leere geprüft werden kann
        //        w[0] = "";
        //    }
        //    else {
        //        w = _Text.SplitByCR();
        //    }


        //    var t = "";

        //    if (Columns.Count > 0) {
        //        foreach (var t1 in Columns) {
        //            t += TrifftZuText(vRow, t1, w);
        //            if (!string.IsNullOrEmpty(t)) { break; }
        //        }
        //    }
        //    else {
        //        t = t + TrifftZuText(vRow, null, w) + "\r";
        //    }

        //    do {
        //        if (!t.Contains("\r\r")) { break; }
        //        t = t.Replace("\r\r", "\r");
        //    } while (true);


        //    t = t.TrimCr();
        //    t = t.Replace("\r", " und ");
        //    return t;
        //}

        //public bool TrifftZu(RowItem vRow) {
        //    if (!IsBedingung()) { return true; }


        //    string[] w;
        //    if (string.IsNullOrEmpty(_Text)) {
        //        w = new string[1]; //MUSS mindestens eines haben, daß auf Leere geprüft werden kann
        //        w[0] = string.Empty;
        //    }
        //    else {
        //        w = _Text.SplitByCR();
        //    }

        //    for (var wtz = 0; wtz <= w.GetUpperBound(0); wtz++) {
        //        if (Columns.Count > 0) {
        //            foreach (var t in Columns) {
        //                if (TrifftZu(vRow, t, w[wtz])) { return true; }
        //            }
        //        }
        //        else {
        //            if (TrifftZu(vRow, null, w[wtz])) { return true; }
        //        }
        //    }

        //    return false;
        //}

        //private bool TrifftZu(RowItem Row, ColumnItem Column, string OneValue) {

        //    switch (_Action) {
        //        case enAction.Anmerkung:
        //            break;

        //        case enAction.Ist:
        //            if (Column == null) { return false; }
        //            return OneValue.ToUpper() == Column.Database.Cell.GetString(Column, Row).ToUpper();

        //        case enAction.Ist_Nicht:
        //            if (Column == null) { return false; }
        //            return OneValue.ToUpper() != Column.Database.Cell.GetString(Column, Row).ToUpper();

        //        case enAction.Enthält:
        //            if (Column == null) { return false; }
        //            return Column.Database.Cell.GetList(Column, Row).Contains(OneValue, false);

        //        case enAction.Enthält_Zeichenkette:
        //            if (Column == null) { return false; }
        //            return Column.Database.Cell.GetString(Column, Row).ToUpper().Contains(OneValue.ToUpper());

        //        case enAction.Enthält_NICHT_Zeichenkette:
        //            if (Column == null) { return false; }
        //            return !Column.Database.Cell.GetString(Column, Row).ToUpper().Contains(OneValue.ToUpper());

        //        case enAction.Unsichtbare_Zeichen_am_Ende_Enthält:
        //            if (Column == null) { return false; }
        //            var TMP = Column.Database.Cell.GetString(Column, Row);


        //            if (!string.IsNullOrEmpty(TMP)) {
        //                switch (TMP.Substring(TMP.Length - 1, 1)) {
        //                    case " ":
        //                        return true;
        //                    case "\r":
        //                        return true;
        //                    case "\n":
        //                        return true;
        //                }
        //            }


        //            if (TMP.Contains(" \r")) { return true; }
        //            if (TMP.Contains(" \n")) { return true; }

        //            break;


        //        //case enAction.Ist_Jünger_Als:
        //        //    return DateTime.Now.Subtract(Row.CellGetDateTime(Row.Database.Column.SysRowCreateDate())).TotalHours < int.Parse(OneValue);




        //        case enAction.Enthält_ungültige_Zeichen:
        //            if (Column == null) { return false; }

        //            if (!Row.CellIsNullOrEmpty(Column)) {
        //                string tmp;
        //                if (!string.IsNullOrEmpty(Column.AllowedChars)) {
        //                    tmp = Column.AllowedChars;
        //                }
        //                else {
        //                    tmp = Column.Format.AllowedChars();
        //                }
        //                if (Column.MultiLine) {
        //                    tmp += "\r";
        //                }
        //                return !Column.Database.Cell.GetString(Column, Row).ContainsOnlyChars(tmp);
        //            }
        //            break;

        //        case enAction.Formatfehler_des_Zelleninhaltes:
        //            if (Column == null) { return false; }
        //            if (!Row.CellIsNullOrEmpty(Column)) {
        //                if (!Column.Database.Cell.GetString(Column, Row).IsFormat(Column.Format, true)) { return true; }
        //            }
        //            break;

        //        case enAction.Auf_eine_existierende_Datei_verweist:
        //            if (Column == null) { return false; }
        //            return FileExists(Column.Database.Cell.GetString(Column, Row));


        //        case enAction.Auf_einen_existierenden_Pfad_verweist:
        //            if (Column == null) { return false; }
        //            return PathExists(Column.Database.Cell.GetString(Column, Row));

        //        //case enAction.Ist_der_Nutzer:
        //        //    return Row.Database.PermissionCheckWithoutAdmin(OneValue, Row);

        //        //case enAction.Erhält_den_Focus:
        //        //    return Convert.ToBoolean(ColumnFocus == Column);

        //        case enAction.Berechnung_ist_True:
        //            return Convert.ToBoolean(MatheErgebnis(_Text, Row) == -1);


        //        default:
        //            Develop.DebugPrint(_Action);
        //            break;
        //    }

        //    return false;
        //}

        //private string TrifftZuText(RowItem Row, ColumnItem Column, string[] AllValue) {

        //    // Es werden nur Bedingungen abgefragt, weil es nur Sinn macht, einen Fehlertext generieren zu lassen.

        //    for (var z = 0; z <= AllValue.GetUpperBound(0); z++) {
        //        if (TrifftZu(Row, Column, AllValue[z])) {

        //            switch (_Action) {
        //                case enAction.Enthält:
        //                    var ex = Column.Database.Cell.GetList(Column, Row);
        //                    if (ex.Contains(AllValue[z], false)) { return "'" + AllValue[z] + "' in '#Spalte:" + Column.Name + "' ist nicht erlaubt"; }
        //                    break;

        //                case enAction.Ist:
        //                    if (AllValue.GetUpperBound(0) == 0 && string.IsNullOrEmpty(AllValue[0])) { return "'#Spalte:" + Column.Name + "' muss befüllt werden"; }
        //                    return "der Wert in '#Spalte:" + Column.Name + "' ist ungültig";

        //                case enAction.Ist_Nicht:
        //                    if (AllValue.GetUpperBound(z) == 0 && string.IsNullOrEmpty(AllValue[0])) { return "'#Spalte:" + Column.Name + "' muss leer sein"; }
        //                    return "der Wert in '#Spalte:" + Column.Name + "' ist ungültig";

        //                case enAction.Berechnung_ist_True:
        //                    return "eine Berechnung trifft zu";

        //                case enAction.Formatfehler_des_Zelleninhaltes:
        //                    return "der Wert in '#Spalte:" + Column.Name + "' entspricht nicht dem erwarteten Format";

        //                case enAction.Unsichtbare_Zeichen_am_Ende_Enthält:
        //                    return "am Ende einer Zeile der Spalte '#Spalte:" + Column.Name + "' sind unsichtbare Zeichen";


        //                case enAction.Enthält_ungültige_Zeichen:
        //                    return "der Wert in '#Spalte:" + Column.Name + "' enthält nicht erlaubte Zeichen";

        //                case enAction.Enthält_Zeichenkette:
        //                    return "der Wert in '#Spalte:" + Column.Name + "' enthält eine unerlaubte Zeichenkette";

        //                case enAction.Enthält_NICHT_Zeichenkette:
        //                    return "beim Wert in '#Spalte:" + Column.Name + "' fehlt eine Zeichenkette";

        //                    //case enAction.Ist_der_Nutzer:
        //                    //    return "sie gehören zu einer bestimmten Benutzergruppe";

        //                    //case enAction.Ist_Jünger_Als:
        //                    //    return "die Zeile ist zu jung";

        //            }
        //            Develop.DebugPrint(_Action);
        //            return "Fehler in der Spalte '#Spalte:" + Column.Name + "'";

        //        }

        //    }

        //    return string.Empty;
        //}

        public bool IsBedingung()
        {
            return (int)_Action > 1 && (int)_Action < 1000;
        }


        public object Clone()
        {
            return (RuleActionItem)MemberwiseClone();
        }

        internal List<ColumnItem> ColumnsAllUsed()
        {
            var l = new List<ColumnItem>();


            if (_Action == 0) { return l; }

            l.AddRange(Columns);


            switch (_Action)
            {
                case enAction.Ist:
                case enAction.Ist_Nicht:
                case enAction.Anmerkung:
                case enAction.Enthält_ungültige_Zeichen:
                case enAction.Auf_eine_existierende_Datei_verweist:
                case enAction.Auf_einen_existierenden_Pfad_verweist:
                case enAction.Enthält:
                //case enAction.Erhält_den_Focus:
                //case enAction.Mache_einen_Vorschlag:
                case enAction.Unsichtbare_Zeichen_am_Ende_Enthält:
                case enAction.Setze_Fehlerhaft:
                case enAction.Enthält_Zeichenkette:
                case enAction.Enthält_NICHT_Zeichenkette:
                case enAction.Wert_Dazu:
                case enAction.Wert_Weg:
                case enAction.Wert_Setzen:
                case enAction.Sperre_die_Zelle:
                case enAction.Formatfehler_des_Zelleninhaltes:
                    //case enAction.Ist_der_Nutzer:
                    //case enAction.Ist_Jünger_Als:
                    //case enAction.SortiereIntelligent:
                    //case enAction.KopiereAndereSpalten:
                    break;

                case enAction.Berechne:
                case enAction.Berechnung_ist_True:
                    if (!string.IsNullOrEmpty(_Text))
                    {
                        foreach (var thisColumnItem in Rule.Database.Column)
                        {
                            if (thisColumnItem != null)
                            {
                                if (_Text.ToUpper().Contains("&" + thisColumnItem.Name.ToUpper() + ";"))
                                {
                                    l.Add(thisColumnItem);
                                }

                            }
                        }
                    }
                    break;

                case enAction.Substring:
                    if (!string.IsNullOrEmpty(_Text) && Columns.Count > 0)
                    {
                        foreach (var thisColumnItem in Rule.Database.Column)
                        {
                            if (thisColumnItem != null)
                            {
                                if (_Text.ToUpper().Contains("&" + thisColumnItem.Name.ToUpper() + ";")) { l.Add(thisColumnItem); }
                                if (_Text.ToUpper().Contains("&" + thisColumnItem.Name.ToUpper() + "(")) { l.Add(thisColumnItem); }

                            }
                        }
                    }
                    break;

                //case enAction.LinkedCell:
                //    if (!string.IsNullOrEmpty(_Text))
                //    {
                //        var o = _Text.SplitByCR();

                //        if (o.Length == 3)
                //        {
                //            if (!int.TryParse(o[0], out var RowKey)) { l.Add(Columns[0].Database.Column.SearchByKey(RowKey)); }
                //            if (!int.TryParse(o[1], out var ColKey)) { l.Add(Columns[0].Database.Column.SearchByKey(ColKey)); }
                //        }


                //    }
                //    break;

                case enAction.Skript:
                    break;



                default:
                    Develop.DebugPrint(_Action);
                    break;
            }

            return l;
        }



        public void OnChanged()
        {
            if (IsParsing) { Develop.DebugPrint(enFehlerArt.Warnung, "Falscher Parsing Zugriff!"); return; }
            Changed?.Invoke(this, System.EventArgs.Empty);
        }



        //public bool IsOk() {
        //    return string.IsNullOrEmpty(ErrorReason());
        //}
    }
}
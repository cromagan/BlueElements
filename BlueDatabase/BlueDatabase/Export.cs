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
using BlueDatabase.Enums;
using BlueDatabase.EventArgs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using static BlueBasics.FileOperations;

namespace BlueDatabase {



    public static class Export {



        public static List<string> SaveAsBitmap(List<RowItem> Row, string LayoutID, string Path) {
            var l = new List<string>();

            foreach (var ThisRow in Row) {
                var FN = TempFile(Path, ThisRow.CellFirstString(), "PNG");
                ThisRow.Database.OnGenerateLayoutInternal(new GenerateLayoutInternalEventargs(ThisRow, LayoutID, FN));
                l.Add(FN);
            }

            return l;
        }



        public static void SaveAsBitmap(RowItem Row, string LayoutID, string Filename) {
            Row.Database.OnGenerateLayoutInternal(new GenerateLayoutInternalEventargs(Row, LayoutID, Filename));
        }








        public static string ParseVariable(string Text, string VariableName, string Valuex, enValueType IsValueType, enValueType AcceptdValueType) {

            var x = 0;

            if (string.IsNullOrEmpty(Text)) { return Text; }
            if (string.IsNullOrEmpty(VariableName)) { return Text; }
            if (Text.Length < VariableName.Length + 4) { return Text; }
            ColumnItem Col = null;


            do {
                var TMP = Text.ToUpper().IndexOf("//TS/000" + VariableName.ToUpper() + "/", x);

                if (TMP < 0) { return Text; }


                if (AcceptdValueType != IsValueType) {
                    Develop.DebugPrint("Inkompatible Typen");
                    return Text;
                }


                x = TMP;

                var e = Text.ToUpper().IndexOf("/E", x + 1);

                if (e < 0) { return Text; }

                var TX = Valuex;

                var Ges = Text.Substring(x, e - x + 2);
                var ToParse = Text.Substring(x + 8 + VariableName.Length, e - x - 8 - VariableName.Length) + "/END";

                ToParse = ToParse.Trim('/');

                var c = ToParse.Split('/');
                var z = -1;
                var Ended = false;
                do {
                    z++;
                    if (z > c.GetUpperBound(0) || c[z].Length < 3) {
                        TX = "/FehlerTS/";
                        break;
                    }


                    c[z] = c[z].FromNonCritical().FromNonCritical().GenerateSlash();


                    var tempVar2 = 0;
                    var tempVar3 = 0;
                    Bitmap tempVar4 = null;
                    string tempVar5 = null;
                    DoSingleCode(c[z], ref TX, null, ref Col, ref tempVar2, ref tempVar3, ref tempVar4, ref tempVar5, Ges, ref Ended);


                } while (!Ended);


                Text = Text.Replace(Ges, TX);
                x = 0;



            } while (true);
        }




        private static void DoSingleCode(string CodeNr, ref string tx, RowItem row, ref ColumnItem column, ref int Wi, ref int He, ref Bitmap I, ref string BT, string Code, ref bool Ended) {

            switch (CodeNr.Substring(0, 3)) {
                case "000":// Spaltenname für Textersetzung
                    if (CodeNr.Substring(0, 3) == "000") // Spaltenname für Textersetzung
                    {

                        if (row != null) {
                            column = row.Database.Column[CodeNr.Substring(3)];
                            if (column == null || !column.ExportableTextformatForLayout()) {
                                tx = "/FehlerTS/" + Code;
                                Ended = true;
                                return;
                            }

                            tx = row.CellGetString(column);
                        }

                        if (!string.IsNullOrEmpty(tx)) {
                            tx = tx.Trim();
                            tx = tx.Replace("\r\n", "\r");
                            tx = tx.TrimCr();


                            //if (fo == enDataFormat.Relation)
                            //{

                            //    var tmp = tx.SplitByCR();
                            //    for (var rz = 0 ; rz <= tmp.GetUpperBound(0) ; rz++)
                            //    {
                            //        tmp[rz] = new clsRelation(Col, vRow, tmp[rz]).ReadableText();
                            //    }
                            //    tx = string.Join("\r", tmp);
                            //}
                        }
                    }

                    break;

                case "001": // Spaltenname für Bild


                    if (row != null) {
                        column = row.Database.Column[CodeNr.Substring(3)];
                        if (column == null) {
                            tx = "/FehlerTS/" + Code;
                            Ended = true;
                            return;
                        }
                        tx = row.CellGetString(column);
                        switch (column.Format) {
                            //case enDataFormat.Binärdaten_Bild:
                            //    I = row.CellGetBitmap(column);
                            //    //break; case Is = enDataFormat.Link_To_BlueDataSystem
                            //    //    I = Nothing
                            //    //    vRow.Database.DataSystem.File_Load(tx, I)

                            //    break;
                            case enDataFormat.Link_To_Filesystem:
                                I = (Bitmap)BitmapExt.Image_FromFile(column.BestFile(row.CellGetString(column), false));


                                break;
                        }
                    }

                    if (I == null) { I = QuickImage.Get(enImageCode.Warnung, 32).BMP; }

                    break;

                case "003": // Spaltenname für Bedingugnen
                    if (row != null) {
                        column = row.Database.Column[CodeNr.Substring(3)];
                        if (column == null || !column.ExportableTextformatForLayout()) {
                            tx = "/FehlerTS/" + Code;
                            Ended = true;
                            return;
                        }
                        BT = row.CellGetString(column);
                    }

                    break;

                case "100": // Wenn leer, Nix
                    if (string.IsNullOrEmpty(tx)) {
                        tx = string.Empty;
                        Ended = true;
                    }


                    break;

                case "101": // Ersetze Leere Zelle mit
                    if (string.IsNullOrEmpty(tx)) {
                        tx = CodeNr.Substring(3);
                    }
                    break;

                case "102": // Ersetze Zeilenumbrüche mit
                    tx = tx.Replace("\r", CodeNr.Substring(3));
                    break;

                case "103": // Vortext
                    var ts = tx.SplitByCR();
                    for (var tz = 0; tz <= ts.GetUpperBound(0); tz++) {
                        ts[tz] = CodeNr.Substring(3) + ts[tz];
                    }
                    tx = string.Join("\r", ts);
                    break;

                case "104": // Nachtext
                    var ts2 = tx.SplitByCR();
                    for (var tz = 0; tz <= ts2.GetUpperBound(0); tz++) {
                        ts2[tz] = ts2[tz] + CodeNr.Substring(3);
                    }
                    tx = string.Join("\r", ts2);
                    break;

                case "105":
                    if (!string.IsNullOrEmpty(tx)) {
                        tx = tx.Replace("<H3>", CodeNr.Substring(3) + "<H3>");
                        tx = tx.Replace("<H2>", CodeNr.Substring(3) + "<H2>");
                        tx = tx.Replace("<H1>", CodeNr.Substring(3) + "<H1>");
                    }
                    break;

                case "106":
                    if (!string.IsNullOrEmpty(tx)) {
                        tx = tx.Replace("<H4>", "<H4>" + CodeNr.Substring(3));
                        //Tx = Tx.Replace("<H2>", "<H2>" & cczz.Substring(3))
                        //Tx = Tx.Replace("<H1>", "<H1>" & cczz.Substring(3))
                    }

                    break;

                case "108": // & -Zeichen -> "&amp

                    if (!string.IsNullOrEmpty(tx)) {

                        tx = tx.CreateHtmlCodes(false);

                        //tx = tx.Replace("&", "&amp;");
                        //tx = tx.Replace("<", "&lt;");
                        //tx = tx.Replace(">", "&gt;");
                    }
                    break;

                case "109": // & -Zeichen -> "&amp

                    if (!string.IsNullOrEmpty(tx)) {
                        tx = tx.Replace("<H2>", "<H1>");
                        tx = tx.Replace("<H3>", "<H2>");
                    }

                    break;

                case "200": // Bildbreite

                    Wi = int.Parse(CodeNr.Substring(3));



                    break;
                case "201": // Bildhöhe

                    He = int.Parse(CodeNr.Substring(3));


                    break;
                case "210": // Maximale Größe
                    Wi = Math.Max(4, Wi);
                    He = Math.Max(4, He);
                    I = BitmapExt.Resize(I, Wi, He, enSizeModes.Breite_oder_Höhe_Anpassen_MitVergrößern, InterpolationMode.HighQualityBicubic, true);

                    break;

                case "211": // Exacte Größe, Weißer Rand
                    Wi = Math.Max(4, Wi);
                    He = Math.Max(4, He);
                    I = BitmapExt.Resize(I, Wi, He, enSizeModes.EmptySpace, InterpolationMode.HighQualityBicubic, true);
                    break;

                case "212":// Exacte Größe, Auffüllen
                    Wi = Math.Max(4, Wi);
                    He = Math.Max(4, He);
                    I = BitmapExt.Resize(I, Wi, He, enSizeModes.BildAbschneiden, InterpolationMode.HighQualityBicubic, true);

                    break;

                case "220":
                    tx = modConverter.BitmapToBase64(I, ImageFormat.Jpeg);
                    break;

                case "302": // Bereinigung

                    tx = CleanUpLayout(Code.Substring(3, Code.Length - 7));
                    Ended = true;
                    return;

                case "310": // Wenn nichtleer, dann!

                    if (string.IsNullOrEmpty(BT)) {
                        tx = string.Empty;
                        Ended = true;
                        return;
                    }
                    var x = Code.IndexOf("/310");
                    tx = Code.Substring(x + 4, Code.Length - x - 8);
                    Ended = true;
                    return;

                case "107":

                    var ts3 = tx.SplitByCR();
                    for (var tz = 0; tz <= ts3.GetUpperBound(0); tz++) {
                        ts3[tz] = CellItem.ValueReadable(column, ts3[tz], enShortenStyle.HTML, enBildTextVerhalten.Nur_Text, true);
                    }
                    tx = string.Join("\r", ts3);
                    break;

                case "110":

                    string[] A = { "es", "er", "em", "en", "e", "" };

                    if (!string.IsNullOrEmpty(tx)) {

                        tx = tx.HTMLSpecialToNormalChar();

                        tx = tx.Replace("Sekunden", "Sek.");
                        tx = tx.Replace("Sekunde", "Sek.");
                        tx = tx.Replace("Minuten", "Min.");
                        tx = tx.Replace("Minute", "Min.");
                        tx = tx.Replace("Stunden", "Std.");
                        tx = tx.Replace("Stunde", "Std.");
                        tx = tx.Replace(" und ", " & ");
                        tx = tx.Replace(" oder ", " o. ");
                        tx = tx.Replace("Zum Beispiel", "Z. B.");
                        tx = tx.Replace("zum Beispiel", "z. B.");
                        tx = tx.Replace("Keine Angaben", "K. A.");
                        tx = tx.Replace("keine Angaben", "k. A.");
                        tx = tx.Replace("Keine Angabe", "K. A.");
                        tx = tx.Replace("keine Angabe", "k. A.");
                        //Tx = Tx.Replace("Etwa ", "Ca. ") ' und mit etwas Glück = und mit ca. Glück :-(((
                        //Tx = Tx.Replace("etwa ", "ca. ")
                        tx = tx.Replace("Circa", "Ca.");
                        tx = tx.Replace("circa", "ca.");
                        tx = tx.Replace("Stücke", "St.");
                        tx = tx.Replace("Stück", "St.");
                        tx = tx.Replace("St.n", "Stücken");
                        tx = tx.Replace("St.chen", "Stückchen");
                        tx = tx.Replace("Kilogramm", "kg");
                        //  tx = tx.Replace(" Kilo", " kg")
                        tx = tx.Replace("Gramm", "g");
                        tx = tx.Replace("Päckchen", "P.");
                        tx = tx.Replace("Packung", "P.");
                        tx = tx.Replace("Esslöffel", "EL");
                        tx = tx.Replace("Eßlöffel", "EL");
                        tx = tx.Replace("Teelöffel", "TL");
                        tx = tx.Replace("Messerspitze", "Msp.");
                        tx = tx.Replace("Portionen", "Port.");
                        tx = tx.Replace("Portion", "Port.");
                        tx = tx.Replace("ein halbes ", "1/2 ", RegexOptions.IgnoreCase);
                        tx = tx.Replace("eine halbe ", "1/2 ", RegexOptions.IgnoreCase);
                        tx = tx.Replace("ein halber ", "1/2 ", RegexOptions.IgnoreCase);
                        tx = tx.Replace("ein drittel ", "1/3 ", RegexOptions.IgnoreCase);
                        tx = tx.Replace("zwei drittel ", "2/3 ", RegexOptions.IgnoreCase);
                        tx = tx.Replace("eine drittel ", "1/3 ", RegexOptions.IgnoreCase);
                        tx = tx.Replace("ein achtel ", "1/8 ", RegexOptions.IgnoreCase);
                        tx = tx.Replace("eine achtel ", "1/8 ", RegexOptions.IgnoreCase);
                        tx = tx.Replace("Stufe ", "St. ");
                        tx = tx.Replace("Liter", "l ", RegexOptions.IgnoreCase);

                        tx = tx.Replace("ein EL", "1 EL", RegexOptions.IgnoreCase);
                        tx = tx.Replace("ein TL", "1 TL", RegexOptions.IgnoreCase);

                        tx = tx.Replace("zwei EL", "2 EL", RegexOptions.IgnoreCase);
                        tx = tx.Replace("zwei TL", "2 TL", RegexOptions.IgnoreCase);


                        for (var t = 0; t <= A.GetUpperBound(0); t++) {
                            tx = tx.Replace("gerieben" + A[t], "ger.");
                            //tx = tx.Replace("groß" + A[t], "gr.");
                            //tx = tx.Replace("klein" + A[t], "kl.");
                            tx = tx.Replace("gekocht" + A[t], "gek.");
                            tx = tx.Replace("tiefgekühlt" + A[t], "TK");
                        }

                        tx = tx.Replace("Tiefkühl", "TK-");

                        tx = tx.CreateHtmlCodes(true);

                    }



                    break;
                case "END":

                    //  If SternDone AndAlso Not String.IsNullOrEmpty(Tx) Then Tx = Tx.Replace(cw2, "")
                    //Return tx
                    Ended = true;
                    return;


                default:

                    tx = "/Fehler/ " + Code;
                    Ended = true;
                    return;
            }
        }


        private static string CleanUpLayout(string t2) {

            string t1;
            t2 = t2.FromNonCritical();

            do {

                t1 = t2;

                t2 = t2.Replace("  ", " ");
                t2 = t2.Replace(">\r\n<", "><");
                t2 = t2.Replace("> <", "><");
                t2 = t2.Replace("\r\r", "\r", RegexOptions.IgnoreCase);
                t2 = t2.Replace("<w:br/><w:br/>", "<w:br/>", RegexOptions.IgnoreCase);
                t2 = t2.Replace(", ,", ",");
                t2 = t2.Replace(",,", ",");
                t2 = t2.Replace(", ,", ",");
                t2 = t2.Replace("/ /", "/");
                t2 = t2.Replace("//", "/");
                t2 = t2.Replace("; ;", ";");
                t2 = t2.Replace(";;", ";");
                t2 = t2.Replace("- -", "-");
                t2 = t2.Replace("--", "-");
                t2 = t2.Replace(", <w:br/>", "<w:br/>", RegexOptions.IgnoreCase);

                t2 = t2.Trim();
                t2 = t2.Trim("<w:br/>");
                t2 = t2.Trim(',');
                t2 = t2.Trim(';');
                t2 = t2.Trim('/');

            } while (t2 != t1);

            return t2.ToNonCritical();
        }



        public static string DoLayoutCode(string Welcher, string tmpBody, RowItem vRow, string EndCode, bool ToNonCriticalText) {
            Welcher = Welcher.ToUpper();
            EndCode = EndCode.ToUpper();

            do {
                var stx = tmpBody.ToUpper().IndexOf("//" + Welcher.ToUpper() + "/");
                if (stx < 0) {
                    return tmpBody;
                }
                var enx = tmpBody.ToUpper().IndexOf("/" + EndCode.ToUpper(), stx + 4);
                if (enx < 0) {
                    return tmpBody;
                }
                var T1 = tmpBody.Substring(stx, enx - stx + 1 + EndCode.Length);
                var T2 = GenerateLayoutString(T1, vRow, Welcher);


                T2 = T2.FromNonCritical(); // Sicherhethalber, daß der Text auf jeden Fall lesbar ist

                // Es kann vorkommen, daß ein Base64 Bild GENAU die nötigen Steuercodes hat!!!!
                if (T2 == "/FehlerTS/") {
                    return tmpBody;
                }


                if (ToNonCriticalText) {
                    T2 = T2.ToNonCritical();
                }


                tmpBody = tmpBody.Replace(T1, T2);
                //  If Not tmpBody.Contains("{Type=BlueBasics.TextItem, InternalName=05.03.2015 12:31:071741881, DPI=300,") Then Stop

            } while (true);
        }


        private static string GenerateLayoutString(string Code, RowItem vRow, string Art) {

            Bitmap I = null;
            var Wi = 8;
            var He = 8;


            Code = Code.TrimStart("//" + Art + "/");
            Code = Code.TrimEnd("XE");
            Code = Code.TrimEnd("AE");
            Code = Code.TrimEnd('E') + "END";
            var c = Code.Split('/');
            var z = -1;
            var Tx = "";
            var BT = "";
            var Ended = false;



            //http://de.selfhtml.org/html/referenz/zeichen.htm#benannte_iso8859_1
            ColumnItem Col = null;
            do {
                z++;
                if (z > c.GetUpperBound(0) || c[z].Length < 3) { return "/FehlerTS/"; }


                c[z] = c[z].FromNonCritical().FromNonCritical().GenerateSlash();

                DoSingleCode(c[z], ref Tx, vRow, ref Col, ref Wi, ref He, ref I, ref BT, Code, ref Ended);

                if (Ended) { return Tx; }


            } while (true);

        }









        public static List<string> SaveAs(RowItem Row, string Layout, string DestinationFile) {

            var l = new List<RowItem>
            {
                Row
            };

            return GenerateLayout_FileSystem(l, Layout, DestinationFile, false, string.Empty);
        }

        //Shared Sub SaveAsBitmap(Row As RowItem)
        //    If Row Is Nothing Then
        //        MessageBox.Show("Kein Eintrag gewählt.", enImageCode.Information, "OK")
        //        Exit Sub
        //    End If

        //    If Row.Database.Layouts.Count = 0 Then
        //        MessageBox.Show("Kein Layouts vorhanden.", enImageCode.Information, "OK")
        //        Exit Sub
        //    End If

        //    'Dim x As String = Row.Cell(Row.Database.Column.SysLastUsedLayout).String

        //    'If x.IsLong Then
        //    '    GenerateLayout_Internal(Row, Integer.Parse(x), False, True, String.Empty)
        //    'Else
        //    GenerateLayout_Internal(Row, 0, False, True, String.Empty)
        //    '   End If
        //End Sub








        public static List<string> GenerateLayout_FileSystem(List<RowItem> Liste, string Lad, string OptionalFileName, bool EineGrosseDatei, string ZielPfad) {

            string sav = null;

            var l = new List<string>();

            if (Liste == null) { return l; }


            if (Liste.Count == 1 || EineGrosseDatei) {
                if (!string.IsNullOrEmpty(OptionalFileName)) {
                    sav = TempFile(OptionalFileName.FilePath(), OptionalFileName.FileNameWithoutSuffix(), Lad.FileSuffix());
                } else {
                    sav = TempFile(ZielPfad, Liste[0].CellFirstString(), Lad.FileSuffix());
                }
                CreateLayout(Liste, Lad, sav);
                l.Add(sav);
            } else {
                foreach (var ThisRow in Liste) {
                    if (!string.IsNullOrEmpty(OptionalFileName)) {
                        sav = TempFile(OptionalFileName.FilePath(), OptionalFileName.FileNameWithoutSuffix(), Lad.FileSuffix());
                    } else {
                        sav = TempFile(ZielPfad, ThisRow.CellFirstString(), Lad.FileSuffix());
                    }
                    CreateLayout(ThisRow, Lad, sav);
                    l.Add(sav);
                }


                //    If OpenIt Then ExecuteFile(ZielPfad)

            }

            //If Not String.IsNullOrEmpty(sav) Then
            //    If FileExists(sav) Then
            //        If OpenIt Then ExecuteFile(sav)
            //    Else
            //        MessageBox.Show("Datei konnte nicht erzeugt werden.", enImageCode.Information, "OK")
            //    End If
            //End If

            return l;
        }






        public static string CreateLayout(RowItem Row, string LoadedFile, bool ToNonCriticalText) {
            if (string.IsNullOrEmpty(LoadedFile)) {
                return string.Empty;
            }


            if (LoadedFile.Contains("BlueBasics")) {
                Develop.DebugPrint(enFehlerArt.Fehler, "Nur für externe Elemente erlaubt!");
                return string.Empty;
            }


            var TMPList = new List<RowItem>
            {
                Row
            };
            return InternalCreateLayout(TMPList, LoadedFile, string.Empty, ToNonCriticalText);
        }


        public static void CreateLayout(RowItem Row, string LoadFile, string SaveFile) {
            if (!FileExists(LoadFile)) { return; }

            var TMPList = new List<RowItem>
            {
                Row
            };
            InternalCreateLayout(TMPList, LoadFromDisk(LoadFile), SaveFile, false);
        }

        public static void CreateLayout(List<RowItem> Rows, string LoadFile, string SaveFile) {
            if (!FileExists(LoadFile)) { return; }
            InternalCreateLayout(Rows, LoadFromDisk(LoadFile), SaveFile, false);
        }


        private static string InternalCreateLayout(List<RowItem> Rows, string FileLoaded, string SaveFile, bool ToNonCriticalText) {


            string tmpSave = null;
            var Head = "";
            var Body = FileLoaded;
            var Foot = "";
            var stx = FileLoaded.ToUpper().IndexOf("//AS/300/AE");
            var enx = FileLoaded.ToUpper().IndexOf("//AS/301/AE");

            if (stx > -1 && enx > stx) {
                Head = FileLoaded.Substring(0, stx);
                Body = FileLoaded.Substring(stx + 11, enx - stx - 11);
                Foot = FileLoaded.Substring(enx + 11);
            }

            tmpSave = Head;

            if (Rows != null) {


                foreach (var ThisRow in Rows) // As Integer = 0 To Rows.GetUpperBound(0)
                {
                    if (ThisRow != null) {
                        var tmpBody = Body;
                        tmpBody = DoLayoutCode("AS", tmpBody, ThisRow, "AE", ToNonCriticalText); // Anfangsbedingungen
                        tmpBody = DoLayoutCode("TS", tmpBody, ThisRow, "E", ToNonCriticalText); // Textbedingungen (Endcode NUR e, weil Pics sonst den zweiten Buchstaben IMMER löschen!
                        tmpBody = DoLayoutCode("XS", tmpBody, ThisRow, "XE", ToNonCriticalText); // Endbedingungen
                        tmpSave += tmpBody;
                    }
                }

                //Else
                //    tmpBody = Body
                //    tmpBody = DoLayoutCode("AS", tmpBody, Nothing, Items, "AE", ToNonCriticalText) ' Anfangsbedingungen
                //    tmpBody = DoLayoutCode("TS", tmpBody, Nothing, Items, "E", ToNonCriticalText) ' Textbedingungen (Endcode NUR e, weil Pics sonst den zweiten Buchstaben IMMER löschen!
                //    tmpBody = DoLayoutCode("XS", tmpBody, Nothing, Items, "XE", ToNonCriticalText) ' Endbedingungen
                //    tmpSave = tmpSave & tmpBody
            }


            tmpSave += Foot;
            if (!string.IsNullOrEmpty(SaveFile)) // Dateien ohne SUfiix-Angabe könenn nicht gespeichert werden
            {
                SaveToDisk(SaveFile, tmpSave, false);
            }

            return tmpSave;
        }




    }


}

// Authors:
// Christian Peter
//
// Copyright (c) 2021 Christian Peter
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

using BlueBasics;
using BlueDatabase.EventArgs;
using System.Collections.Generic;
using System.IO;
using static BlueBasics.FileOperations;

namespace BlueDatabase {

    public static class Export {

        #region Methods

        //public static string CreateLayout(RowItem Row, string LoadedFile, bool ToNonCriticalText) {
        //    if (string.IsNullOrEmpty(LoadedFile)) {
        //        return string.Empty;
        //    }
        //    if (LoadedFile.Contains("BlueBasics")) {
        //        Develop.DebugPrint(enFehlerArt.Fehler, "Nur für externe Elemente erlaubt!");
        //        return string.Empty;
        //    }
        //    var TMPList = new List<RowItem>
        //    {
        //        Row
        //    };
        //    return InternalCreateLayout(TMPList, LoadedFile, string.Empty, ToNonCriticalText);
        //}
        public static void CreateLayout(RowItem Row, string LoadFile, string SaveFile) {
            if (!FileExists(LoadFile)) { return; }
            List<RowItem> TMPList = new()
            {
                Row
            };
            InternalCreateLayout(TMPList, File.ReadAllText(LoadFile, Constants.Win1252), SaveFile, false);
        }

        public static void CreateLayout(List<RowItem> Rows, string LoadFile, string SaveFile) {
            if (!FileExists(LoadFile)) { return; }
            InternalCreateLayout(Rows, File.ReadAllText(LoadFile, Constants.Win1252), SaveFile, false);
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
            List<string> l = new();
            if (Liste == null) { return l; }
            string sav;
            if (Liste.Count == 1 || EineGrosseDatei) {
                sav = !string.IsNullOrEmpty(OptionalFileName)
                    ? TempFile(OptionalFileName.FilePath(), OptionalFileName.FileNameWithoutSuffix(), Lad.FileSuffix())
                    : TempFile(ZielPfad, Liste[0].CellFirstString(), Lad.FileSuffix());
                CreateLayout(Liste, Lad, sav);
                l.Add(sav);
            } else {
                foreach (var ThisRow in Liste) {
                    sav = !string.IsNullOrEmpty(OptionalFileName)
                        ? TempFile(OptionalFileName.FilePath(), OptionalFileName.FileNameWithoutSuffix(), Lad.FileSuffix())
                        : TempFile(ZielPfad, ThisRow.CellFirstString(), Lad.FileSuffix());
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

        //public static object ParseVariable(string platzhaltertxt, string variablename, object value) {
        //    var kennungstart = 0;
        //    if (string.IsNullOrEmpty(platzhaltertxt)) { return platzhaltertxt; }
        //    if (string.IsNullOrEmpty(variablename)) { return platzhaltertxt; }
        //    if (platzhaltertxt.Length < variablename.Length + 4) { return platzhaltertxt; }
        //    ColumnItem Col = null;
        //    do {
        //        var tmpKennungstart = platzhaltertxt.ToUpper().IndexOf("//TS/000" + variablename.ToUpper() + "/", kennungstart);
        //        if (tmpKennungstart < 0) { return platzhaltertxt; }
        //        kennungstart = tmpKennungstart;
        //        var kennungende = platzhaltertxt.ToUpper().IndexOf("/E", kennungstart + 1);
        //        if (kennungende < 0) { return platzhaltertxt; }
        //        var obj = value;
        //        var Ges = platzhaltertxt.Substring(kennungstart, kennungende - kennungstart + 2);
        //        var ToParse = platzhaltertxt.Substring(kennungstart + 8 + variablename.Length, kennungende - kennungstart - 8 - variablename.Length) + "/END";
        //        ToParse = ToParse.Trim('/');
        //        var codes = ToParse.Split('/');
        //        var current = -1;
        //        var Ended = false;
        //        do {
        //            current++;
        //            if (current > codes.GetUpperBound(0) || codes[current].Length < 3) {
        //                obj = "/FehlerTS/";
        //                break;
        //            }
        //            codes[current] = codes[current].FromNonCritical().FromNonCritical().GenerateSlash();
        //            var tempVar2 = 0;
        //            var tempVar3 = 0;
        //            string tempVar5 = null;
        //            DoSingleCode(codes[current], ref obj, null, ref Col, ref tempVar2, ref tempVar3, ref tempVar5, Ges, ref Ended);
        //        } while (!Ended);
        //        if (obj is string ttt) {
        //            platzhaltertxt = platzhaltertxt.Replace(Ges, ttt);
        //        }
        //        if (obj is Bitmap bmp) {
        //            // Dann ist sozusagen kein Text mehr vorhanden, also das Bild zurückgeben
        //            return bmp;
        //        }
        //        kennungstart = 0;
        //    } while (true);
        //}
        //private static void DoSingleCode(string CodeNr, ref object value, RowItem row, ref ColumnItem column, ref int Wi, ref int He, ref string BT, string Code, ref bool Ended) {
        //    var TXT = string.Empty;
        //    var TextReturn = false;
        //    Bitmap BMP = null;
        //    if (value is string s) {
        //        TXT = s;
        //        TextReturn = true;
        //    }
        //    if (value is Bitmap s2) {
        //        BMP = s2;
        //        TextReturn = false;
        //    }
        //    switch (CodeNr.Substring(0, 3)) {
        //        case "000":// Spaltenname für Textersetzung
        //            if (CodeNr.Substring(0, 3) == "000") // Spaltenname für Textersetzung
        //            {
        //                if (row != null) {
        //                    column = row.Database.Column[CodeNr.Substring(3)];
        //                    if (column == null || !column.ExportableTextformatForLayout()) {
        //                        TXT = "/FehlerTS/" + Code;
        //                        Ended = true;
        //                        break;
        //                    } else {
        //                        TXT = row.CellGetString(column);
        //                    }
        //                }
        //                if (!string.IsNullOrEmpty(TXT)) {
        //                    TXT = TXT.Trim();
        //                    TXT = TXT.Replace("\r\n", "\r");
        //                    TXT = TXT.TrimCr();
        //                }
        //            }
        //            break;
        //        case "001": // Spaltenname für Bild
        //            TextReturn = false;
        //            if (row != null) {
        //                column = row.Database.Column[CodeNr.Substring(3)];
        //                if (column == null) {
        //                    value = "/FehlerTS/" + Code;
        //                    Ended = true;
        //                    break;
        //                } else {
        //                    TXT = row.CellGetString(column);
        //                    switch (column.Format) {
        //                        case enDataFormat.Link_To_Filesystem:
        //                            BMP = (Bitmap)BitmapExt.Image_FromFile(column.BestFile(row.CellGetString(column), false));
        //                            break;
        //                    }
        //                }
        //            }
        //            if (BMP == null) { BMP = QuickImage.Get(enImageCode.Warnung, 32).BMP; }
        //            break;
        //        case "003": // Spaltenname für Bedingugnen
        //            if (row != null) {
        //                column = row.Database.Column[CodeNr.Substring(3)];
        //                if (column == null || !column.ExportableTextformatForLayout()) {
        //                    TXT = "/FehlerTS/" + Code;
        //                    Ended = true;
        //                    break;
        //                }
        //                BT = row.CellGetString(column);
        //            }
        //            break;
        //        case "100": // Wenn leer, Nix
        //            if (string.IsNullOrEmpty(TXT)) {
        //                TXT = string.Empty;
        //                Ended = true;
        //            }
        //            break;
        //        case "101": // Ersetze Leere Zelle mit
        //            if (string.IsNullOrEmpty(TXT)) {
        //                TXT = CodeNr.Substring(3);
        //            }
        //            break;
        //        case "102": // Ersetze Zeilenumbrüche mit
        //            TXT = TXT.Replace("\r", CodeNr.Substring(3));
        //            break;
        //        case "103": // Vortext
        //            var ts = TXT.SplitAndCutByCR();
        //            for (var tz = 0; tz <= ts.GetUpperBound(0); tz++) {
        //                ts[tz] = CodeNr.Substring(3) + ts[tz];
        //            }
        //            TXT = string.Join("\r", ts);
        //            break;
        //        case "104": // Nachtext
        //            var ts2 = TXT.SplitAndCutByCR();
        //            for (var tz = 0; tz <= ts2.GetUpperBound(0); tz++) {
        //                ts2[tz] = ts2[tz] + CodeNr.Substring(3);
        //            }
        //            TXT = string.Join("\r", ts2);
        //            break;
        //        case "105":
        //            if (!string.IsNullOrEmpty(TXT)) {
        //                TXT = TXT.Replace("<H3>", CodeNr.Substring(3) + "<H3>");
        //                TXT = TXT.Replace("<H2>", CodeNr.Substring(3) + "<H2>");
        //                TXT = TXT.Replace("<H1>", CodeNr.Substring(3) + "<H1>");
        //            }
        //            break;
        //        case "106":
        //            if (!string.IsNullOrEmpty(TXT)) {
        //                TXT = TXT.Replace("<H4>", "<H4>" + CodeNr.Substring(3));
        //                //Tx = Tx.Replace("<H2>", "<H2>" & cczz.Substring(3))
        //                //Tx = Tx.Replace("<H1>", "<H1>" & cczz.Substring(3))
        //            }
        //            break;
        //        case "108": // & -Zeichen -> "&amp
        //            if (!string.IsNullOrEmpty(TXT)) {
        //                TXT = TXT.CreateHtmlCodes(false);
        //                //tx = tx.Replace("&", "&amp;");
        //                //tx = tx.Replace("<", "&lt;");
        //                //tx = tx.Replace(">", "&gt;");
        //            }
        //            break;
        //        case "109": // & -Zeichen -> "&amp
        //            if (!string.IsNullOrEmpty(TXT)) {
        //                TXT = TXT.Replace("<H2>", "<H1>");
        //                TXT = TXT.Replace("<H3>", "<H2>");
        //            }
        //            break;
        //        case "200": // Bildbreite
        //            Wi = int.Parse(CodeNr.Substring(3));
        //            break;
        //        case "201": // Bildhöhe
        //            He = int.Parse(CodeNr.Substring(3));
        //            break;
        //        case "210": // Maximale Größe
        //            TextReturn = false;
        //            Wi = Math.Max(4, Wi);
        //            He = Math.Max(4, He);
        //            BMP = BitmapExt.Resize(BMP, Wi, He, enSizeModes.Breite_oder_Höhe_Anpassen_MitVergrößern, InterpolationMode.HighQualityBicubic, true);
        //            break;
        //        case "211": // Exacte Größe, Weißer Rand
        //            TextReturn = false;
        //            Wi = Math.Max(4, Wi);
        //            He = Math.Max(4, He);
        //            BMP = BitmapExt.Resize(BMP, Wi, He, enSizeModes.EmptySpace, InterpolationMode.HighQualityBicubic, true);
        //            break;
        //        case "212":// Exacte Größe, Auffüllen
        //            TextReturn = false;
        //            Wi = Math.Max(4, Wi);
        //            He = Math.Max(4, He);
        //            BMP = BitmapExt.Resize(BMP, Wi, He, enSizeModes.BildAbschneiden, InterpolationMode.HighQualityBicubic, true);
        //            break;
        //        case "220":
        //            TXT = modConverter.BitmapToBase64(BMP, ImageFormat.Jpeg);
        //            TextReturn = true;
        //            break;
        //        case "302": // Bereinigung
        //            TXT = CleanUpLayout(Code.Substring(3, Code.Length - 7));
        //            Ended = true;
        //            break;
        //        case "310": // Wenn nichtleer, dann!
        //            if (string.IsNullOrEmpty(BT)) {
        //                TXT = string.Empty;
        //                Ended = true;
        //                break;
        //            }
        //            var x = Code.IndexOf("/310");
        //            TXT = Code.Substring(x + 4, Code.Length - x - 8);
        //            Ended = true;
        //            break;
        //        case "107":
        //            q
        //            var ts3 = TXT.SplitAndCutByCR();
        //            for (var tz = 0; tz <= ts3.GetUpperBound(0); tz++) {
        //                ts3[tz] = CellItem.ValueReadable(column, ts3[tz], enShortenStyle.HTML, enBildTextVerhalten.Nur_Text, true);
        //            }
        //            TXT = string.Join("\r", ts3);
        //            break;
        //        case "110":
        //            string[] A = { "es", "er", "em", "en", "e", "" };
        //            if (!string.IsNullOrEmpty(TXT)) {
        //                TXT = TXT.HTMLSpecialToNormalChar();
        //                TXT = TXT.Replace("Sekunden", "Sek.");
        //                TXT = TXT.Replace("Sekunde", "Sek.");
        //                TXT = TXT.Replace("Minuten", "Min.");
        //                TXT = TXT.Replace("Minute", "Min.");
        //                TXT = TXT.Replace("Stunden", "Std.");
        //                TXT = TXT.Replace("Stunde", "Std.");
        //                TXT = TXT.Replace(" und ", " & ");
        //                TXT = TXT.Replace(" oder ", " o. ");
        //                TXT = TXT.Replace("Zum Beispiel", "Z. B.");
        //                TXT = TXT.Replace("zum Beispiel", "z. B.");
        //                TXT = TXT.Replace("Keine Angaben", "K. A.");
        //                TXT = TXT.Replace("keine Angaben", "k. A.");
        //                TXT = TXT.Replace("Keine Angabe", "K. A.");
        //                TXT = TXT.Replace("keine Angabe", "k. A.");
        //                //Tx = Tx.Replace("Etwa ", "Ca. ") ' und mit etwas Glück = und mit ca. Glück :-(((
        //                //Tx = Tx.Replace("etwa ", "ca. ")
        //                TXT = TXT.Replace("Circa", "Ca.");
        //                TXT = TXT.Replace("circa", "ca.");
        //                TXT = TXT.Replace("Stücke", "St.");
        //                TXT = TXT.Replace("Stück", "St.");
        //                TXT = TXT.Replace("St.n", "Stücken");
        //                TXT = TXT.Replace("St.chen", "Stückchen");
        //                TXT = TXT.Replace("Kilogramm", "kg");
        //                //  tx = tx.Replace(" Kilo", " kg")
        //                TXT = TXT.Replace("Gramm", "g");
        //                TXT = TXT.Replace("Päckchen", "P.");
        //                TXT = TXT.Replace("Packung", "P.");
        //                TXT = TXT.Replace("Esslöffel", "EL");
        //                TXT = TXT.Replace("Eßlöffel", "EL");
        //                TXT = TXT.Replace("Teelöffel", "TL");
        //                TXT = TXT.Replace("Messerspitze", "Msp.");
        //                TXT = TXT.Replace("Portionen", "Port.");
        //                TXT = TXT.Replace("Portion", "Port.");
        //                TXT = TXT.Replace("ein halbes ", "1/2 ", RegexOptions.IgnoreCase);
        //                TXT = TXT.Replace("eine halbe ", "1/2 ", RegexOptions.IgnoreCase);
        //                TXT = TXT.Replace("ein halber ", "1/2 ", RegexOptions.IgnoreCase);
        //                TXT = TXT.Replace("ein drittel ", "1/3 ", RegexOptions.IgnoreCase);
        //                TXT = TXT.Replace("zwei drittel ", "2/3 ", RegexOptions.IgnoreCase);
        //                TXT = TXT.Replace("eine drittel ", "1/3 ", RegexOptions.IgnoreCase);
        //                TXT = TXT.Replace("ein achtel ", "1/8 ", RegexOptions.IgnoreCase);
        //                TXT = TXT.Replace("eine achtel ", "1/8 ", RegexOptions.IgnoreCase);
        //                TXT = TXT.Replace("Stufe ", "St. ");
        //                TXT = TXT.Replace("Liter", "l ", RegexOptions.IgnoreCase);
        //                TXT = TXT.Replace("ein EL", "1 EL", RegexOptions.IgnoreCase);
        //                TXT = TXT.Replace("ein TL", "1 TL", RegexOptions.IgnoreCase);
        //                TXT = TXT.Replace("zwei EL", "2 EL", RegexOptions.IgnoreCase);
        //                TXT = TXT.Replace("zwei TL", "2 TL", RegexOptions.IgnoreCase);
        //                for (var t = 0; t <= A.GetUpperBound(0); t++) {
        //                    TXT = TXT.Replace("gerieben" + A[t], "ger.");
        //                    //tx = tx.Replace("groß" + A[t], "gr.");
        //                    //tx = tx.Replace("klein" + A[t], "kl.");
        //                    TXT = TXT.Replace("gekocht" + A[t], "gek.");
        //                    TXT = TXT.Replace("tiefgekühlt" + A[t], "TK");
        //                }
        //                TXT = TXT.Replace("Tiefkühl", "TK-");
        //                TXT = TXT.CreateHtmlCodes(true);
        //            }
        //            break;
        //        case "END":
        //            Ended = true;
        //            break;
        //        default:
        //            value = "/Fehler/ " + Code;
        //            Ended = true;
        //            break;
        //    }
        //    if (TextReturn) {
        //        value = TXT;
        //    } else {
        //        value = BMP;
        //    }
        //}
        //private static string CleanUpLayout(string t2) {
        //    string t1;
        //    t2 = t2.FromNonCritical();
        //    do {
        //        t1 = t2;
        //        t2 = t2.Replace("  ", " ");
        //        t2 = t2.Replace(">\r\n<", "><");
        //        t2 = t2.Replace("> <", "><");
        //        t2 = t2.Replace("\r\r", "\r", RegexOptions.IgnoreCase);
        //        t2 = t2.Replace("<w:br/><w:br/>", "<w:br/>", RegexOptions.IgnoreCase);
        //        t2 = t2.Replace(", ,", ",");
        //        t2 = t2.Replace(",,", ",");
        //        t2 = t2.Replace(", ,", ",");
        //        t2 = t2.Replace("/ /", "/");
        //        t2 = t2.Replace("//", "/");
        //        t2 = t2.Replace("; ;", ";");
        //        t2 = t2.Replace(";;", ";");
        //        t2 = t2.Replace("- -", "-");
        //        t2 = t2.Replace("--", "-");
        //        t2 = t2.Replace(", <w:br/>", "<w:br/>", RegexOptions.IgnoreCase);
        //        t2 = t2.Trim();
        //        t2 = t2.Trim("<w:br/>");
        //        t2 = t2.Trim(',');
        //        t2 = t2.Trim(';');
        //        t2 = t2.Trim('/');
        //    } while (t2 != t1);
        //    return t2.ToNonCritical();
        //}
        //public static string DoLayoutCode(string Welcher, string tmpBody, RowItem vRow, string EndCode, bool ToNonCriticalText) {
        //    Welcher = Welcher.ToUpper();
        //    EndCode = EndCode.ToUpper();
        //    do {
        //        var stx = tmpBody.ToUpper().IndexOf("//" + Welcher.ToUpper() + "/");
        //        if (stx < 0) {
        //            return tmpBody;
        //        }
        //        var enx = tmpBody.ToUpper().IndexOf("/" + EndCode.ToUpper(), stx + 4);
        //        if (enx < 0) {
        //            return tmpBody;
        //        }
        //        var T1 = tmpBody.Substring(stx, enx - stx + 1 + EndCode.Length);
        //        var T2 = GenerateLayoutString(T1, vRow, Welcher);
        //        T2 = T2.FromNonCritical(); // Sicherhethalber, daß der Text auf jeden Fall lesbar ist
        //        // Es kann vorkommen, daß ein Base64 Bild GENAU die nötigen Steuercodes hat!!!!
        //        if (T2 == "/FehlerTS/") {
        //            return tmpBody;
        //        }
        //        if (ToNonCriticalText) {
        //            T2 = T2.ToNonCritical();
        //        }
        //        tmpBody = tmpBody.Replace(T1, T2);
        //        //  If Not tmpBody.Contains("{Type=BlueBasics.TextItem, InternalName=05.03.2015 12:31:071741881, DPI=300,") Then Stop
        //    } while (true);
        //}
        //private static string GenerateLayoutString(string Code, RowItem vRow, string Art) {
        //    var Wi = 8;
        //    var He = 8;
        //    Code = Code.TrimStart("//" + Art + "/");
        //    Code = Code.TrimEnd("XE");
        //    Code = Code.TrimEnd("AE");
        //    Code = Code.TrimEnd('E') + "END";
        //    var codes = Code.Split('/');
        //    var z = -1;
        //    object Tx = string.Empty;
        //    var BT = "";
        //    var Ended = false;
        //    //http://de.selfhtml.org/html/referenz/zeichen.htm#benannte_iso8859_1
        //    ColumnItem Col = null;
        //    do {
        //        z++;
        //        if (z > codes.GetUpperBound(0) || codes[z].Length < 3) { return "/FehlerTS/"; }
        //        codes[z] = codes[z].FromNonCritical().FromNonCritical().GenerateSlash();
        //        DoSingleCode(codes[z], ref Tx, vRow, ref Col, ref Wi, ref He, ref BT, Code, ref Ended);
        //        if (Ended) {
        //            if (Tx is string txt) {
        //                return txt;
        //            }
        //            return "/FehlerTS/";
        //        }
        //    } while (true);
        //}
        public static List<string> SaveAs(RowItem Row, string Layout, string DestinationFile) {
            List<RowItem> l = new()
            {
                Row
            };
            return GenerateLayout_FileSystem(l, Layout, DestinationFile, false, string.Empty);
        }

        public static List<string> SaveAsBitmap(List<RowItem> Row, string LayoutID, string Path) {
            List<string> l = new();
            foreach (var ThisRow in Row) {
                var FN = TempFile(Path, ThisRow.CellFirstString(), "PNG");
                ThisRow.Database.OnGenerateLayoutInternal(new GenerateLayoutInternalEventargs(ThisRow, LayoutID, FN));
                l.Add(FN);
            }
            return l;
        }

        public static void SaveAsBitmap(RowItem Row, string LayoutID, string Filename) => Row.Database.OnGenerateLayoutInternal(new GenerateLayoutInternalEventargs(Row, LayoutID, Filename));

        private static string InternalCreateLayout(List<RowItem> Rows, string FileLoaded, string SaveFile, bool ToNonCriticalText) {
            var Head = "";
            var Foot = "";
            var stx = FileLoaded.ToUpper().IndexOf("//AS/300/AE");
            var enx = FileLoaded.ToUpper().IndexOf("//AS/301/AE");
            if (stx > -1 && enx > stx) {
                Head = FileLoaded.Substring(0, stx);
                _ = FileLoaded.Substring(stx + 11, enx - stx - 11);
                Foot = FileLoaded.Substring(enx + 11);
            }
            var tmpSave = Head;
            if (Rows != null) {
                foreach (var ThisRow in Rows) // As Integer = 0 To Rows.GetUpperBound(0)
                {
                    if (ThisRow != null) {
                        Develop.DebugPrint_NichtImplementiert();
                        //var tmpBody = Body;
                        //tmpBody = DoLayoutCode("AS", tmpBody, ThisRow, "AE", ToNonCriticalText); // Anfangsbedingungen
                        //tmpBody = DoLayoutCode("TS", tmpBody, ThisRow, "E", ToNonCriticalText); // Textbedingungen (Endcode NUR e, weil Pics sonst den zweiten Buchstaben IMMER löschen!
                        //tmpBody = DoLayoutCode("XS", tmpBody, ThisRow, "XE", ToNonCriticalText); // Endbedingungen
                        //tmpSave += tmpBody;
                    }
                }
            }
            tmpSave += Foot;
            if (!string.IsNullOrEmpty(SaveFile)) // Dateien ohne SUfiix-Angabe könenn nicht gespeichert werden
            {
                WriteAllText(SaveFile, tmpSave, Constants.Win1252, false);
            }
            return tmpSave;
        }

        #endregion
    }
}
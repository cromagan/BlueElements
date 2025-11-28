// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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

#nullable enable

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.ItemCollectionPad;
using BlueTable;
using System;
using System.Collections.Generic;
using System.Linq;
using static BlueBasics.IO;

namespace BlueControls;

public static class Export {
    //public static string CreateLayout(RowItem Row, string LoadedFile, bool ToNonCriticalText) {
    //    if (string.IsNullOrEmpty(LoadedFile)) {
    //        return string.Empty;
    //    }
    //    if (LoadedFile.Contains("BlueBasics")) {
    //        Develop.DebugPrint(ErrorType.Error, "Nur für externe Elemente erlaubt!");
    //        return string.Empty;
    //    }
    //    var TMPList = new List<RowItem>
    //    {
    //        Row
    //    };
    //    return InternalCreateLayout(TMPList, LoadedFile, string.Empty, ToNonCriticalText);
    //}

    #region Methods

    public static string CreateLayoutBMP(RowItem row, string loadFile, string saveFile) {
        if (!FileExists(loadFile)) { return "Datei nicht gefunden."; }

        if (loadFile.FileType() != FileFormat.BlueCreativeFile) { return "Datei hat das falsche Format."; }

        using var l = new ItemCollectionPadItem(loadFile);
        l.ForPrinting = true;

        if (!l.Any()) { return "Layout nicht gefunden oder fehlerhaft."; }

        l.ResetVariables();
        var scx = l.ReplaceVariables(row);

        if (scx.Failed) { return "Generierung fehlgeschlagen"; }

        var bmp = l.ToBitmap(1);

        if (bmp == null) { return "Generierung fehlgeschlagen"; }

        bmp.Save(saveFile, System.Drawing.Imaging.ImageFormat.Png);
        bmp.Dispose();
        Generic.CollectGarbage();

        return string.Empty;
    }

    public static (List<string>? files, string error) GenerateLayout_FileSystem(List<RowItem>? liste, string lad, string optionalFileName, bool eineGrosseDatei, string zielPfad) {
        if (liste == null) { return (null, "Keine Zeilen angegeben"); }

        List<string> l = [];

        var fehler = string.Empty;

        if (eineGrosseDatei) {
            var sav = !string.IsNullOrEmpty(optionalFileName)
                 ? TempFile(optionalFileName.FilePath(), optionalFileName.FileNameWithoutSuffix(), lad.FileSuffix())
                 : TempFile(zielPfad, liste[0].CellFirstString(), lad.FileSuffix());

            fehler = CreateLayout(liste, lad, sav);
            l.Add(sav);
        } else {
            foreach (var thisRow in liste) {
                var sav = !string.IsNullOrEmpty(optionalFileName)
                     ? TempFile(optionalFileName.FilePath(), optionalFileName.FileNameWithoutSuffix(), lad.FileSuffix())
                     : TempFile(zielPfad, thisRow.CellFirstString(), lad.FileSuffix());

                if (lad.FileType() == FileFormat.BlueCreativeFile) {
                    sav = TempFile(sav.FilePath(), sav.FileNameWithoutSuffix(), "png");
                    fehler = CreateLayoutBMP(thisRow, lad, sav);
                } else {
                    fehler = CreateLayout(thisRow, lad, sav);
                }

                l.Add(sav);
            }
        }

        if (!string.IsNullOrEmpty(fehler)) {
            Forms.MessageBox.Show(fehler);
        }
        return (l, fehler);
    }

    private static string CreateLayout(RowItem row, string loadFile, string saveFile) => CreateLayout([row], loadFile, saveFile);

    private static string CreateLayout(List<RowItem> rows, string loadFile, string saveFile) {
        if (!FileExists(loadFile)) { return "Datei nicht gefunden."; }

        return InternalCreateLayout(rows, ReadAllText(loadFile, Constants.Win1252), saveFile);
    }

    //Shared Sub SaveAsBitmap(Row As RowItem)
    //    If Row Is Nothing Then
    //        BlueControls.Forms.MessageBox.Show("Kein Eintrag gewählt.", ImageCode.Information, "OK")
    //        Exit Sub
    //    End If
    //    If Row.Table.Layouts.Count = 0 Then
    //        BlueControls.Forms.MessageBox.Show("Kein Layouts vorhanden.", ImageCode.Information, "OK")
    //        Exit Sub
    //    End If
    //    'Dim x As String = Row.Cell(Row.Table.Column.SysLastUsedLayout).String
    //    'If x.IsLong Then
    //    '    GenerateLayout_Internal(Row, Integer.Parse(x), False, True, String.Empty)
    //    'Else
    //    GenerateLayout_Internal(Row, 0, False, True, String.Empty)
    //    '   End If
    //End Sub
    //public static object ParseVariable(string platzhaltertxt, string variablename, object value) {
    //    var kennungstart = 0;
    //    if (string.IsNullOrEmpty(platzhaltertxt)) { return platzhaltertxt; }
    //    if (string.IsNullOrEmpty(variablename)) { return platzhaltertxt; }
    //    if (platzhaltertxt.Length < variablename.Length + 4) { return platzhaltertxt; }
    //    ColumnItem Col = null;
    //    do {
    //        var tmpKennungstart = platzhaltertxt.ToUpperInvariant().IndexOf("//TS/000" + variablename.ToUpperInvariant() + "/", kennungstart);
    //        if (tmpKennungstart < 0) { return platzhaltertxt; }
    //        kennungstart = tmpKennungstart;
    //        var kennungende = platzhaltertxt.ToUpperInvariant().IndexOf("/E", kennungstart + 1);
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
    //    Bitmap Bmp = null;
    //    if (value is string s) {
    //        TXT = s;
    //        TextReturn = true;
    //    }
    //    if (value is Bitmap s2) {
    //        Bmp = s2;
    //        TextReturn = false;
    //    }
    //    switch (CodeNr.Substring(0, 3)) {
    //        case "000":// Spaltenname für Textersetzung
    //            if (CodeNr.Substring(0, 3) == "000") // Spaltenname für Textersetzung
    //            {
    //                if (row != null && !row.IsDisposed) {
    //                    column = row.Table?.Column[CodeNr.Substring(3)];
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
    //            if (row != null && !row.IsDisposed) {
    //                column = row.Table?.Column[CodeNr.Substring(3)];
    //                if (Column  ==null || Column .IsDisposed) {
    //                    value = "/FehlerTS/" + Code;
    //                    Ended = true;
    //                    break;
    //                } else {
    //                    TXT = row.CellGetString(column);
    //                    switch (column.Format) {
    //                        case DataFormat.Link_To_Filesystem:
    //                            Bmp = (Bitmap)BitmapExt.Image_FromFile(column.BestFile(row.CellGetString(column), false));
    //                            break;
    //                    }
    //                }
    //            }
    //            if (Bmp == null) { Bmp = QuickImage.Get(ImageCode.Warnung, 32).Bmp; }
    //            break;
    //        case "003": // Spaltenname für Bedingugnen
    //            if (row != null && !row.IsDisposed) {
    //                column = row.Table?.Column[CodeNr.Substring(3)];
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
    //            var ts = TXT.SplitAndCutByCr();
    //            for (var tz = 0; tz <= ts.GetUpperBound(0); tz++) {
    //                ts[tz] = CodeNr.Substring(3) + ts[tz];
    //            }
    //            TXT = string.Join("\r", ts);
    //            break;
    //        case "104": // Nachtext
    //            var ts2 = TXT.SplitAndCutByCr();
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
    //            Wi = IntParse(CodeNr.Substring(3));
    //            break;
    //        case "201": // Bildhöhe
    //            He = IntParse(CodeNr.Substring(3));
    //            break;
    //        case "210": // Maximale Größe
    //            TextReturn = false;
    //            Wi = Math.Max(4, Wi);
    //            He = Math.Max(4, He);
    //            Bmp = BitmapExt.Resize(Bmp, Wi, He, enSizeModes.Breite_oder_Höhe_Anpassen_MitVergrößern, InterpolationMode.HighQualityBicubic, true);
    //            break;
    //        case "211": // Exacte Größe, Weißer Rand
    //            TextReturn = false;
    //            Wi = Math.Max(4, Wi);
    //            He = Math.Max(4, He);
    //            Bmp = BitmapExt.Resize(Bmp, Wi, He, enSizeModes.EmptySpace, InterpolationMode.HighQualityBicubic, true);
    //            break;
    //        case "212":// Exacte Größe, Auffüllen
    //            TextReturn = false;
    //            Wi = Math.Max(4, Wi);
    //            He = Math.Max(4, He);
    //            Bmp = BitmapExt.Resize(Bmp, Wi, He, enSizeModes.BildAbschneiden, InterpolationMode.HighQualityBicubic, true);
    //            break;
    //        case "220":
    //            TXT = modConverter.BitmapToBase64(Bmp, ImageFormat.Jpeg);
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
    //            var ts3 = TXT.SplitAndCutByCr();
    //            for (var tz = 0; tz <= ts3.GetUpperBound(0); tz++) {
    //                ts3[tz] = CellItem.ValueReadable(column, ts3[tz], ShortenStyle.HTML, BildTextVerhalten.Nur_Text, true);
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
    //        value = Bmp;
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
    //    Welcher = Welcher.ToUpperInvariant();
    //    EndCode = EndCode.ToUpperInvariant();
    //    do {
    //        var stx = tmpBody.ToUpperInvariant().IndexOf("//" + Welcher.ToUpperInvariant() + "/");
    //        if (stx < 0) {
    //            return tmpBody;
    //        }
    //        var enx = tmpBody.ToUpperInvariant().IndexOf("/" + EndCode.ToUpperInvariant(), stx + 4);
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
    //        //  If Not tmpBody.Contains("{Type=BlueBasics.TextItem, InternalName=05.03.2015 12:31:071741881, Dpi=300,") Then Stop
    //    } while (true);
    //}
    //private static string GenerateLayoutString(string toParse, RowItem vRow, string Art) {
    //    var Wi = 8;
    //    var He = 8;
    //    Code = Code.TrimStart("//" + Art + "/");
    //    Code = Code.TrimEnd("XE");
    //    Code = Code.TrimEnd("AE");
    //    Code = Code.TrimEnd('E') + "END";
    //    var codes = Code.Split('/');
    //    var z = -1;
    //    object Tx = string.Empty;
    //    var BT = string.Empty;
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
    //public static void SaveAsBitmap(RowItem row, string layoutId, string filename) => row.Table.OnGenerateLayoutInternal(new GenerateLayoutInternalEventArgs(row, layoutId, filename));

    private static string InternalCreateLayout(List<RowItem> rows, string fileLoaded, string saveFileName) {
        var head = string.Empty;
        var foot = string.Empty;
        var body = fileLoaded;
        var stx = fileLoaded.ToUpperInvariant().IndexOf("//AS/300/AE", StringComparison.Ordinal);
        var enx = fileLoaded.ToUpperInvariant().IndexOf("//AS/301/AE", StringComparison.Ordinal);
        if (stx > -1 && enx > stx) {
            head = fileLoaded.Substring(0, stx);
            body = fileLoaded.Substring(stx + 11, enx - stx - 11);
            foot = fileLoaded.Substring(enx + 11);
        }

        var f = string.Empty;
        var onemled = string.Empty;

        var tmpSave = head;

        foreach (var thisRow in rows) // As Integer = 0 To Rows.GetUpperBound(0)
        {
            if (thisRow is { IsDisposed: false }) {
                var tmpBody = body;

                thisRow.CheckRow(); // Nichtspeicherbare Spalten
                var script = thisRow.ExecuteScript(ScriptEventTypes.export, string.Empty, true, 0, null, true, false);

                if (script.Failed) {
                    f = f + thisRow.CellFirstString() + "\r\n";
                    onemled = script.ProtocolText;
                }

                if (script.Variables != null) {
                    foreach (var thisV in script.Variables) {
                        tmpBody = thisV.ReplaceInText(tmpBody);
                    }
                }

                tmpSave += tmpBody;
            }
        }

        tmpSave += foot;
        if (!string.IsNullOrEmpty(saveFileName)) // Dateien ohne Suffix-Angabe können nicht gespeichert werden
        {
            WriteAllText(saveFileName, tmpSave, Constants.Win1252, false);
        }

        return !string.IsNullOrEmpty(f) ? "Fehler bei:\r\n" + f + "\r\nDie Meldung des letzten Eintrages:\r\n" + onemled : string.Empty;
    }

    #endregion
}
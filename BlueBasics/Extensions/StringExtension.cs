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
using BlueBasics.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BlueBasics
{
    public static partial class Extensions
    {

        public static bool FromPlusMinus(this string Value)
        {

            if (string.IsNullOrEmpty(Value)) { return false; }

            switch (Value.ToUpper())
            {
                case "+": return true;
                case "-": return false;
                case "WAHR": return true;
                case "FALSCH": return false;
                case "TRUE": return true;
                case "FALSE": return false;
                case "": return false;
                default:
                    Develop.DebugPrint(enFehlerArt.Warnung, "'" + Value + "' unbekannt!");
                    return false;
            }
        }

        public static string RemoveHTMLCodes(this string html)
        {
            return Regex.Replace(html, "<.*?>", "");
        }

        public static string Reverse(this string TXT)
        {
            var charArray = TXT.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        public static byte[] ToByte(this string TXT)
        {
            return Encoding.Default.GetBytes(TXT);
        }

        public static List<byte> ToByteList(this string TXT)
        {
            var x = new List<byte>();
            x.AddRange(Encoding.Default.GetBytes(TXT));
            return x;
        }

        public static bool isPossibleLineBreak(this char value)
        {
            const string TR = " ?!%/\\}])-.,;_°~|\r\n\t";
            // Kein Doppelpunkt, weil auch 3:50 Uhr möglich ist
            return TR.Contains(value.ToString());
        }


        public static bool isWordSeperator(this char value)
        {
            const string TR = "~|=<>+`´\r\n\t";

            if (char.IsPunctuation(value)) { return true; }
            if (char.IsSeparator(value)) { return true; }

            return TR.Contains(value.ToString());

        }

        public static string Insert(this string TXT, string InsertTxt, string AfterTXT, string WhenNotContais)
        {
            if (string.IsNullOrEmpty(AfterTXT)) { return TXT; }
            if (string.IsNullOrEmpty(InsertTxt)) { return TXT; }
            if (string.IsNullOrEmpty(TXT)) { return TXT; }
            if (!TXT.Contains(AfterTXT)) { return TXT; }

            var Pos = -1;

            do
            {
                Pos++;
                var InsterPos = Pos + AfterTXT.Length;
                if (InsterPos > TXT.Length) { break; }


                if (TXT.Substring(Pos, AfterTXT.Length) == AfterTXT)
                {
                    if (InsterPos == TXT.Length || !WhenNotContais.Contains(TXT.Substring(InsterPos, 1)))
                    {
                        TXT = TXT.Insert(InsterPos, InsertTxt);
                        Pos += InsertTxt.Length;
                        //Stop
                    }
                }

            } while (true);
            return TXT;
        }



        public static bool IsHTMLColorCode(this string col)
        {
            if (string.IsNullOrEmpty(col)) { return false; }
            if (col.Length != 6 && col.Length != 8) { return false; }

            return col.ContainsOnlyChars(Constants.Char_Numerals + "abcdefABCDEF");

        }

        public static bool IsNumeral(this string TXT)
        {
            if (TXT.IsFormat(enDataFormat.Ganzzahl)) { return true; }
            if (TXT.IsFormat(enDataFormat.Gleitkommazahl)) { return true; }
            return false;
        }

        public static bool IsLong(this string TXT)
        {
            if (TXT is null) { return false; }
            return TXT.IsFormat(enDataFormat.Ganzzahl);
        }


        public static bool IsDouble(this string TXT)
        {
            return TXT.IsFormat(enDataFormat.Gleitkommazahl);
        }





        public static bool ContainsChars(this string TXT, string Chars)
        {
            return Chars.Where((t, z) => TXT.Contains(Chars.Substring(z, 1))).Any();
        }

        public static bool ContainsOnlyChars(this string TXT, string Chars)
        {
            return !TXT.Where((t, z) => !Chars.Contains(TXT.Substring(z, 1))).Any();
        }

        /// <summary>
        /// Teilt einen String, der geparsed werden kann in seine Bestandteile auf.
        /// </summary>
        /// <param name="Value">Ein String, der mit { beginnt. Z.B. {Wert=100, Wert2=150}</param>
        /// <returns>Gibt immer eine List zurück.</returns>
        public static List<KeyValuePair<string, string>> GetAllTags(this string Value)
        {

            var Result = new List<KeyValuePair<string, string>>();

            if (string.IsNullOrEmpty(Value) || Value.Length < 3) { return Result; }
            if (Value.Substring(0, 1) != "{") { return Result; }

            var Beg = 0;

            do
            {
                Beg++;
                if (Beg > Value.Length) { break; }
                var T = Value.ParseTag(Beg);
                var V = Value.ParseValue(T, Beg);
                if (!string.IsNullOrEmpty(T) && !string.IsNullOrEmpty(V))
                {
                    Result.Add(new KeyValuePair<string, string>(T, V));
                }

                Beg = Beg + T.Length + V.Length + 2;
            } while (true);

            return Result;
        }


        public static string ParseTag(this string TXT, int StartIndex)
        {

            var IG = TXT.IndexOf("=", StartIndex);
            if (IG < 1) { return string.Empty; }

            var BG = IG - 1;
            do
            {
                if (BG < 1) { break; }
                if (" ,{".Contains(TXT.Substring(BG, 1))) { break; }
                BG--;
            } while (true);

            return TXT.Substring(BG + 1, IG - BG - 1).ToLower();
        }


        public static string ParseValue(this string TXT, string OffTag, int StartIndex)
        {

            if (string.IsNullOrEmpty(TXT)) { return string.Empty; }
            if (string.IsNullOrEmpty(OffTag)) { return string.Empty; }

            var FirstCharAfterEquals = TXT.ToLower().IndexOf(OffTag.ToLower() + "=", StartIndex);

            if (FirstCharAfterEquals < 0) { return string.Empty; }
            FirstCharAfterEquals = FirstCharAfterEquals + OffTag.Length + 1;

            //while (TXT.Substring(FirstCharAfterEquals, 1) == " ")
            //{
            //    FirstCharAfterEquals++;
            //}

            var OpenBraketCount = 0;
            var CurrentChar = FirstCharAfterEquals - 1; // Wird im ersten Step wieder erhöht

            var ExitDo = false;

            while (!ExitDo && CurrentChar < TXT.Length - 1)
            {

                CurrentChar++;

                switch (TXT.Substring(CurrentChar, 1))
                {
                    case " ":
                        if (TXT.Substring(CurrentChar - 1, 1) == "," && OpenBraketCount == 0)
                        {
                            CurrentChar -= 2;
                            ExitDo = true;
                        }
                        break;

                    case "{":
                        OpenBraketCount++;
                        break;

                    case "}":
                        if (OpenBraketCount == 0)
                        {
                            CurrentChar--;
                            ExitDo = true;
                        }
                        else
                        {
                            OpenBraketCount--;
                        }
                        break;
                }

            }

            if (OpenBraketCount != 0) { Develop.DebugPrint(enFehlerArt.Fehler, "Parse Fehler: " + TXT); }
            return TXT.Substring(FirstCharAfterEquals, CurrentChar - FirstCharAfterEquals + 1);
        }



        public static string StarkeVereinfachung(this string TXT, string AdditinalAllowed)
        {
            TXT = TXT.ToLower().ReduceToChars(Constants.Char_Numerals + Constants.Char_Buchstaben + AdditinalAllowed);

            TXT = TXT.Replace("ä", "ae");
            TXT = TXT.Replace("ö", "oe");
            TXT = TXT.Replace("ü", "ue");

            TXT = TXT.Replace("á", "a");
            TXT = TXT.Replace("ó", "o");
            TXT = TXT.Replace("ú", "u");
            TXT = TXT.Replace("í", "i");
            TXT = TXT.Replace("é", "e");

            TXT = TXT.Replace("à", "a");
            TXT = TXT.Replace("ò", "o");
            TXT = TXT.Replace("ù", "u");
            TXT = TXT.Replace("ì", "i");
            TXT = TXT.Replace("è", "e");

            TXT = TXT.Replace("â", "a");
            TXT = TXT.Replace("ô", "o");
            TXT = TXT.Replace("û", "u");
            TXT = TXT.Replace("î", "i");
            TXT = TXT.Replace("ê", "e");

            TXT = TXT.Replace("ž", "z");

            TXT = TXT.Replace("ß", "ss");

            return TXT;
        }


        /// <summary>
        /// Löscht alle Zeichen - außder dem erlaubten - aus dem String. Gross- und Kleinschreibung wird unterschieden. 
        /// "RemoveChars" macht das Gegenteil
        /// </summary>
        /// <param name="TXT">Der zu bereinigende Text</param>
        /// <param name="Chars">Die noch erlaubten Zeichen</param>
        /// <returns>Der bereinigte Text mit nur noch den erlaubten Zeichen</returns>
        /// <remarks></remarks>
        public static string ReduceToChars(this string TXT, string Chars)
        {
            var p = 0;

            while (p < TXT.Length)
            {

                if (!Chars.Contains(TXT.Substring(p, 1)))
                {
                    TXT = TXT.Replace(TXT.Substring(p, 1), string.Empty);
                }
                else
                {
                    p++;
                }
            }
            return TXT;
        }

        /// <summary>
        /// Löscht alle angegebnen Zeichen aus dem String. Gross- und Kleinschreibung wird unterschieden. 
        /// "ReduceToChars" macht das Gegenteil
        /// </summary>
        /// <param name="TXT">Der zu bereinigende Text</param>
        /// <param name="Chars">Die zu entfernenden Zeichen</param>
        /// <returns>der bereinigte Text ohne die unerwünschten Zeichen</returns>
        /// <remarks></remarks>
        public static string RemoveChars(this string TXT, string Chars)
        {
            for (var z = 0; z < Chars.Length; z++)
            {
                TXT = TXT.Replace(Chars.Substring(z, 1), string.Empty);
            }

            return TXT;
        }



        public static string EleminateSlash(this string txt)
        {
            return txt.Replace("/", "[Slash]");
        }

        public static string GenerateSlash(this string txt)
        {
            return txt.Replace("[Slash]", "/");
        }


        public static string HTMLSpecialToNormalChar(this string TXT)
        {
            // http://sonderzeichentabelle.de/
            // http://www.htmlhelp.com/reference/html40/entities/special.html


            TXT = TXT.Replace("&amp;", "&"); // Wichtig! An erster Stelle! ä-> &auml; -> &amp;auml;
            TXT = TXT.Replace("<H7>", ((char)1007).ToString());
            TXT = TXT.Replace("<H4>", ((char)1004).ToString());
            TXT = TXT.Replace("<H3>", ((char)1003).ToString());

            TXT = TXT.Replace("&auml;", "ä");
            TXT = TXT.Replace("&ouml;", "ö");
            TXT = TXT.Replace("&uuml;", "ü");
            TXT = TXT.Replace("&Auml;", "Ä");
            TXT = TXT.Replace("&Ouml;", "Ö");
            TXT = TXT.Replace("&Uuml;", "Ü");
            TXT = TXT.Replace("&szlig;", "ß");

            TXT = TXT.Replace("&Eacute;", "É");
            TXT = TXT.Replace("&eacute;", "é");


            TXT = TXT.Replace("&euro;", "€");

            TXT = TXT.Replace("&Oslash;", "Ø");
            TXT = TXT.Replace("&oslash;", "ø");
            TXT = TXT.Replace("&bull;", "•");


            TXT = TXT.Replace("<br>", "\r");

            TXT = TXT.Replace("<", ((char)1020).ToString());
            TXT = TXT.Replace(">", ((char)1021).ToString());

            TXT = TXT.Replace("&lt;", "<");
            TXT = TXT.Replace("&gt;", ">");
            TXT = TXT.Replace("&quot;", "\"");

            return TXT;

        }





        public static string CreateHtmlCodes(this string TXT)
        {
            // http://sonderzeichentabelle.de/
            // http://www.htmlhelp.com/reference/html40/entities/special.html


            TXT = TXT.Replace("&", "&amp;"); // Wichtig! An erster Stelle! ä-> &auml; -> &amp;auml;

            TXT = TXT.Replace("ä", "&auml;");
            TXT = TXT.Replace("ö", "&ouml;");
            TXT = TXT.Replace("ü", "&uuml;");
            TXT = TXT.Replace("Ä", "&Auml;");
            TXT = TXT.Replace("Ö", "&Ouml;");
            TXT = TXT.Replace("Ü", "&Uuml;");
            TXT = TXT.Replace("ß", "&szlig;");

            TXT = TXT.Replace("É", "&Eacute;");
            TXT = TXT.Replace("é", "&eacute;");


            TXT = TXT.Replace("€", "&euro;");

            TXT = TXT.Replace("Ø", "&Oslash;");
            TXT = TXT.Replace("ø", "&oslash;");
            TXT = TXT.Replace("•", "&bull;");

            TXT = TXT.Replace("<", "&lt;");
            TXT = TXT.Replace(">", "&gt;");
            TXT = TXT.Replace("\"", "&quot;");



            //TXT = TXT.Replace("&lt;br&gt;", "<br>", False)
            //TXT = TXT.Replace("&lt;b&gt;", "<b>", False)
            //TXT = TXT.Replace("&lt;/b&gt;", "</b>", False)
            //TXT = TXT.Replace("&lt;u&gt;", "<u>", False)
            //TXT = TXT.Replace("&lt;/u&gt;", "</u>", False)
            //TXT = TXT.Replace("&lt;i&gt;", "<i>", False)
            //TXT = TXT.Replace("&lt;/i&gt;", "</i>", False)



            TXT = TXT.Replace("\r\n", "<br>", RegexOptions.IgnoreCase);
            TXT = TXT.Replace("\r", "<br>", RegexOptions.IgnoreCase);
            TXT = TXT.Replace("\n", "<br>", RegexOptions.IgnoreCase);

            TXT = TXT.Replace(((char)1007).ToString(), "<H7>");
            TXT = TXT.Replace(((char)1004).ToString(), "<H4>");
            TXT = TXT.Replace(((char)1003).ToString(), "<H3>");


            TXT = TXT.Replace(((char)1020).ToString(), "<");
            TXT = TXT.Replace(((char)1021).ToString(), ">");

            return TXT;

        }


        public static string ToHTMLText(this string txt)
        {
            txt = txt.Replace("&", "&amp;");
            txt = txt.Replace("<", "&lt;");
            txt = txt.Replace(">", "&gt;");
            //s
            //txt = txt.Replace("~~&lt;", "<<>");
            //txt = txt.Replace("~~&gt;", "<>>");
            txt = txt.Replace("\r\n", "<br>");
            txt = txt.Replace("\r", "<br>");
            return txt;
        }

        public static string ConvertFromHtmlToRich(this string txt)
        {
            txt = txt.Replace("\r\n", "<br>");
            txt = txt.Replace("<<>", "&lt;");
            txt = txt.Replace("<>>", "&gt;");
            return txt;
        }



        public static string ToNonCritical(this string txt)
        {
            //http://www.theasciicode.com.ar/ascii-printable-characters/braces-curly-brackets-opening-ascii-code-123.html
            if (string.IsNullOrEmpty(txt)) { return string.Empty; }

            txt = txt.Replace("[", "[Z]");
            txt = txt.Replace(";", "[A]");
            txt = txt.Replace("<", "[B]");
            txt = txt.Replace(">", "[C]");
            txt = txt.Replace("\r\n", "[D]");
            txt = txt.Replace("\r", "[E]");
            txt = txt.Replace("\n", "[F]");
            txt = txt.Replace("|", "[G]");
            txt = txt.Replace("}", "[H]");
            txt = txt.Replace("{", "[I]");
            txt = txt.Replace("=", "[J]");
            txt = txt.Replace(",", "[K]");
            txt = txt.Replace("&", "[L]");
            return txt;
        }


        public static string FromNonCritical(this string txt)
        {
            //http://www.theasciicode.com.ar/ascii-printable-characters/braces-curly-brackets-opening-ascii-code-123.html


            if (string.IsNullOrEmpty(txt)) { return string.Empty; }
            if (txt.Length < 3) { return txt; }
            if (!txt.Contains(";") && !txt.Contains("[")) { return txt; }

            txt = txt.Replace("[A]", ";");
            txt = txt.Replace("[B]", "<");
            txt = txt.Replace("[C]", ">");
            txt = txt.Replace("[D]", "\r\n");
            txt = txt.Replace("[E]", "\r");
            txt = txt.Replace("[F]", "\n");
            txt = txt.Replace("[G]", "|");
            txt = txt.Replace("[H]", "}");
            txt = txt.Replace("[I]", "{");
            txt = txt.Replace("[J]", "=");
            txt = txt.Replace("[K]", ",");
            txt = txt.Replace("[L]", "&");
            txt = txt.Replace("[M]", "/");
            txt = txt.Replace("[Z]", "[");
            return txt;
        }











        public static enFileFormat FileType(this string Filename)
        {

            string Suffix;


            if (string.IsNullOrEmpty(Filename)) { return enFileFormat.Unknown; }


            if (Filename.IndexOf(".") > 1 && Filename.IndexOf(".") < Filename.Length)
            {
                Suffix = Filename.FileSuffix();
            }
            else
            {
                Suffix = Filename;
            }

            switch (Suffix)
            {
                case "DOC":
                case "DOCX":
                case "RTF":
                case "ODT":
                    return enFileFormat.WordKind;

                case "TXT":
                case "INI":
                case "INFO":
                    return enFileFormat.Textdocument;

                case "XLS":
                case "CSV":
                case "XLA":
                case "XLSX":
                case "XLSM":
                case "ODS":
                    return enFileFormat.ExcelKind;

                case "PPT":
                case "PPS":
                case "PPA":
                    return enFileFormat.PowerPointKind;

                case "MSG":
                case "EML":
                    return enFileFormat.EMail;

                case "PDF":
                    return enFileFormat.Pdf;

                case "HTM":
                case "HTML":
                    return enFileFormat.HTML;

                case "JPG":
                case "JPEG":
                case "BMP":
                case "TIFF":
                case "TIF":
                case "GIF":
                case "PNG":
                    return enFileFormat.Image;

                case "ICO":
                    return enFileFormat.Icon;

                case "ZIP":
                case "RAR":
                case "7Z":
                    return enFileFormat.CompressedArchive;

                case "AVI":
                case "DIVX":
                case "MPG":
                case "MPEG":
                case "WMV":
                case "FLV":
                case "MP4":
                case "MKV":
                case "M4V":
                    return enFileFormat.Movie;

                case "EXE":
                case "BAT":
                case "SCR":
                    return enFileFormat.Executable;

                case "CHM":
                    return enFileFormat.HelpFile;

                case "XML":
                    return enFileFormat.XMLFile;

                case "VCF":
                    return enFileFormat.Visitenkarte;

                case "MP3":
                case "WAV":
                case "AAC":
                    return enFileFormat.Sound;


                case "B4A":
                case "BAS":
                case "CS":
                    //case "DLL":
                    return enFileFormat.ProgrammingCode;

                case "DB":
                case "MDB":
                    return enFileFormat.Database;

                case "LNK":
                case "URL":
                    return enFileFormat.Link;

                default:
                    return enFileFormat.Unknown;
            }
        }




        public static string PathParent(this string Pfad, int AnzahlParents)
        {
            for (var z = 1; z <= AnzahlParents; z++)
            {
                Pfad = Pfad.PathParent();
            }

            return Pfad;
        }

        public static string PathParent(this string Pfad)
        {
            var z = Pfad.Length;
            Pfad = Pfad.CheckPath();

            do
            {
                z--;
                if (z <= 1) { return string.Empty; }
                if (Pfad.Substring(z - 1, 1) == "\\") { return Pfad.Substring(0, z); }
            } while (true);

        }




        public static string Folder(this string Pathx)
        {


            if (string.IsNullOrEmpty(Pathx)) { return string.Empty; }
            // Kann vorkommen, wenn ein Benutzer einen Pfad
            // per Hand eingeben darf

            Pathx = Pathx.Replace("/", "\\").TrimEnd('\\');
            if (!Pathx.Contains("\\")) { return Pathx; }


            var z = Pathx.Length;
            if (z < 2) { return string.Empty; }

            do
            {
                z--;
                if (Pathx.Substring(z, 1) == "\\") { return Pathx.Substring(z + 1); }
                if (z < 1) { return string.Empty; }
            } while (true);

        }




        public static string CheckPath(this string Pfad)
        {
            if (string.IsNullOrEmpty(Pfad)) { return string.Empty; }  // Kann vorkommen, wenn ein Benutzer einen Pfad per Hand eingeben darf

            if (Pfad.Length > 6 && Pfad.Substring(0, 7).ToLower() == "http://") { return Pfad; }
            if (Pfad.Length > 7 && Pfad.Substring(0, 8).ToLower() == "https://") { return Pfad; }
            if (Pfad.Contains("/")) { Pfad = Pfad.Replace("/", "\\"); }
            if (Pfad.Substring(Pfad.Length - 1) != "\\") { Pfad += "\\"; }

            if (Pfad.IndexOf("\\\\", 1) > 0) { Develop.DebugPrint("Achtung, Doppelslash: " + Pfad); }

            if (Pfad.Substring(0, 1) == "\\" && Pfad.Substring(0, 2) != "\\\\") { Develop.DebugPrint("Achtung, Doppelslash: " + Pfad); }

            return Pfad;
        }



        public static string FilePath(this string Name)
        {

            if (string.IsNullOrEmpty(Name)) { return string.Empty; }
            //    Return Path.GetDirectoryName(Name) & "\" ' <---- Versagt ab 260 Zeichen


            Name = Name.Replace("/", "\\");

            var z = Name.LastIndexOf("\\");
            if (z < 0) { return string.Empty; }
            return Name.Substring(0, z + 1);
        }



        public static string FileSuffix(this string Name)
        {
            if (string.IsNullOrEmpty(Name)) { return string.Empty; }
            if (!Name.Contains(".")) { return string.Empty; }


            var l = Path.GetExtension(Name);
            if (string.IsNullOrEmpty(l)) { return string.Empty; }

            return l.Substring(1).ToUpper();
        }


        public static string FileNameWithSuffix(this string Name)
        {
            if (string.IsNullOrEmpty(Name)) { return string.Empty; }
            return Path.GetFileName(Name);
        }

        /// <summary>
        /// Gibt den Dateinamen ohne Suffix zurück.
        /// </summary>
        /// <param name="Name">Der ganze Pfad der Datei.</param>
        /// <returns>Dateiname ohne Suffix</returns>
        /// <remarks></remarks>
        public static string FileNameWithoutSuffix(this string Name)
        {
            if (string.IsNullOrEmpty(Name)) { return string.Empty; }
            return Path.GetFileNameWithoutExtension(Name);
        }

        public static string Trim(this string TXT, string Was)
        {
            if (string.IsNullOrEmpty(TXT)) { return string.Empty; }
            TXT = TXT.TrimEnd(Was);
            if (string.IsNullOrEmpty(TXT)) { return string.Empty; }
            return TXT.TrimStart(Was);
        }


        public static string TrimEnd(this string TXT, string Was)
        {

            if (string.IsNullOrEmpty(TXT)) { return string.Empty; }

            if (Was.Length < 1) { Develop.DebugPrint(enFehlerArt.Fehler, "Trimmen nicht möglich mit: " + Was); }


            Was = Was.ToUpper();

            do
            {
                if (TXT.Length < Was.Length || TXT.Substring(TXT.Length - Was.Length).ToUpper() != Was) { break; }
                TXT = TXT.Remove(TXT.Length - Was.Length);
            } while (true);

            return TXT;
        }

        public static string Replace(this string TXT, string Alt, string Neu, RegexOptions options)
        {


            if (options != RegexOptions.IgnoreCase) { Develop.DebugPrint(enFehlerArt.Fehler, "Regex option nicht erlaubt."); }

            if (string.IsNullOrEmpty(Alt)) { Develop.DebugPrint(enFehlerArt.Fehler, "ALT is Empty"); }


            var OldPos = 0;

            do
            {
                if (string.IsNullOrEmpty(TXT)) { return TXT; }

                var Posx = TXT.ToUpper().IndexOf(Alt.ToUpper(), OldPos);
                if (Posx >= 0)
                {
                    TXT = TXT.Substring(0, Posx) + Neu + TXT.Substring(Posx + Alt.Length);
                    OldPos = Posx + Neu.Length;
                }
                else
                {
                    return TXT;
                }
            } while (true);

        }

        public static bool ContainsWord(this string input, string value, RegexOptions options)
        {
            return (input.IndexOfWord(value, 0, options) >= 0);
        }


        public static List<string> AllWords(this string input)
        {

            input = " " + input + " ";
            var position = 0;
            var LastSeperator = 0;
            var l = new List<string>();

            do
            {

                position++;

                if (position >= input.Length) { return l; }

                if (input[position].isWordSeperator())
                {
                    if (position > LastSeperator + 1)
                    {
                        l.Add(input.Substring(LastSeperator+1, position - LastSeperator-1));
                    }
                    LastSeperator = position;

                }


            } while (true);


        }

        public static int IndexOfWord(this string input, string value, int startIndex, RegexOptions options)
        {
            if (options != RegexOptions.IgnoreCase) { Develop.DebugPrint(enFehlerArt.Fehler, "Regex option nicht erlaubt."); }

            value = value.ToUpper();
            input = " " + input.ToUpper() + " ";
            startIndex++;


            do
            {
                if (startIndex > input.Length - 1) { return -1; }

                startIndex = input.IndexOf(value, startIndex);

                if (startIndex < 0) { return -1; }

                if (startIndex > 0 && startIndex < input.Length - value.Length)
                {
                    if (input[startIndex - 1].isWordSeperator() && input[startIndex + value.Length].isWordSeperator())
                    {
                        return startIndex - 1; // -1, weil ein Leereichen hinzugefügt wurde.
                    }
                    startIndex += value.Length;

                }
            }
            while (true);
        }



        public static string ReplaceWord(this string input, string Alt, string replacement, RegexOptions options)
        {
            //return Regex.Replace(input, "\\b" + Alt + "\\b", replacement);

            if (options != RegexOptions.IgnoreCase) { Develop.DebugPrint(enFehlerArt.Fehler, "Regex option nicht erlaubt."); }

            if (replacement.ToUpper().Contains(Alt.ToUpper()))
            {
                var du = "@DUMMY@";
                input = ReplaceWord(input, Alt, du, options);
                input = ReplaceWord(input, du, replacement, options);
                return input;
            }



            do
            {
                var start = IndexOfWord(input, Alt, 0, options);
                if (start < 0) { return input; }
                input = input.Substring(0, start) + replacement + input.Substring(start + Alt.Length);
            }
            while (true);
        }


        public static string TrimCr(this string TXT)
        {
            if (string.IsNullOrEmpty(TXT)) { return string.Empty; }
            return TXT.Trim("\r");
        }




        public static string TrimStart(this string TXT, string Was)
        {
            if (string.IsNullOrEmpty(TXT)) { return string.Empty; }

            if (Was.Length < 1) { Develop.DebugPrint(enFehlerArt.Fehler, "Trimmen nicht möglich mit: " + Was); }


            Was = Was.ToUpper();

            do
            {
                if (TXT.Length < Was.Length || TXT.Substring(0, Was.Length).ToUpper() != Was) { break; }
                TXT = TXT.Remove(0, Was.Length);
            } while (true);

            return TXT;
        }

        /// <summary>
        /// Trennt den Text mittels dem Trennzeichen. Sind ein oder mehrere Trennzeichen am Ende, werden Leeren Felder diese nicht zurückgegeben.
        /// </summary>
        /// <param name="TextToSplit"></param>
        /// <param name="Trennzeichen"></param>
        /// <returns></returns>
        public static string[] SplitBy(this string TextToSplit, string Trennzeichen)
        {
            var w = new string[0];
            if (string.IsNullOrEmpty(TextToSplit)) { return w; }

            TextToSplit = TextToSplit.TrimEnd(Trennzeichen);

            w = TextToSplit.Split(new[] { Trennzeichen }, StringSplitOptions.None);

            if (w.Length == 1 && string.IsNullOrEmpty(w[0])) { w = new string[0]; }

            return w;

        }


        public static List<string> SplitByCRToList(this string TextToSplit)
        {
            var w = new List<string>();
            if (string.IsNullOrEmpty(TextToSplit)) { return w; }



            TextToSplit = TextToSplit.Replace("\r\n", "\r");
            TextToSplit = TextToSplit.Replace("\n", "\r");


            w.AddRange(TextToSplit.SplitBy("\r"));

            return w;
        }




        public static string[] SplitByCR(this string TextToSplit)
        {
            var w = new string[0];
            if (string.IsNullOrEmpty(TextToSplit)) { return w; }
            TextToSplit = TextToSplit.Replace("\r\n", "\r");
            TextToSplit = TextToSplit.Replace("\n", "\r");

            return TextToSplit.SplitBy("\r");
        }

        public static int CountString(this string Text, string value)
        {
            var Anz = 0;
            for (var z = 0; z <= Text.Length - value.Length; z++)
            {
                if (Text.Substring(z, value.Length) == value) { Anz++; }
            }
            return Anz;
        }



        /// <summary>
        /// Reduziert den Text auf ein Minimum, Trennzeichen ist \r\n
        /// </summary>
        /// <param name="vText">Beispiel: Hund frißt Knochen, Hund vergräbt knochen.</param>
        /// <param name="vSearch">Beispiel: Hund * Kochen.</param>
        /// <returns>Beispiel: frißt \r\n vergräbt</returns>
        /// <remarks></remarks>
        public static List<string> ReduceToMulti(this string vText, string vSearch)
        {
            if (vSearch.CountString("*") != 1) { return null; }

            var e = vSearch.Split('*');

            if (e.Length != 2) { return null; }

            var txt = new List<string>();

            var enx = 0;

            do
            {

                var bgx = vText.ToUpper().IndexOf(e[0].ToUpper(), enx);
                if (bgx < 0) { break; }

                enx = vText.ToUpper().IndexOf(e[1].ToUpper(), bgx + e[0].Length);
                if (bgx + e[0].Length > enx) { break; }


                txt.Add(vText.Substring(bgx + e[0].Length, enx - bgx - e[0].Length));


            } while (true);

            return txt;

        }


        public static string SetLenght(this string s, int Anzahl)
        {
            if (s.Length == Anzahl) { return s; }
            if (s.Length < Anzahl) { return s.PadRight(Anzahl); }
            return s.Substring(0, Anzahl);
        }
    }
}

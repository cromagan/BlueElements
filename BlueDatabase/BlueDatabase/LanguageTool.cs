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

namespace BlueDatabase
{
    public static class LanguageTool
    {

        public static Database Translation = null;
        private static string German = string.Empty;
        private static string English = string.Empty;

        private static readonly object[] EmptyArgs = new object[0];


        /// <summary>
        /// Fügt Präfix und Suffix hinzu und ersteztt den Text nach dem gewünschten Stil.
        /// </summary>
        /// <param name="newTXT"></param>
        /// <param name="Column"></param>
        /// <param name="Style"></param>
        /// <returns></returns>
        public static string ColumnReplace(string newTXT, ColumnItem Column, enShortenStyle Style, enImageNotFound compactView)
        {
            if (!compactView && !string.IsNullOrEmpty(newTXT))
            {
                if (!string.IsNullOrEmpty(Column.Prefix)) { newTXT = DoTranslate(Column.Prefix, true) + " " + newTXT; }
                if (!string.IsNullOrEmpty(Column.Suffix)) { newTXT = newTXT + " " + DoTranslate(Column.Suffix, true); }
            }

            if (Translation != null) { return ColumnReplaceTranslated(newTXT, Column); }

            if (Style == enShortenStyle.Unreplaced || Column.OpticalReplace.Count == 0) { return newTXT; }

            var OT = newTXT;

            foreach (var ThisString in Column.OpticalReplace)
            {
                var x = ThisString.SplitBy("|");
                if (x.Length == 2)
                {
                    if (string.IsNullOrEmpty(x[0]))
                    {
                        if (string.IsNullOrEmpty(newTXT)) { newTXT = x[1]; }
                    }
                    else
                    {
                        newTXT = newTXT.Replace(x[0], x[1]);
                    }
                }
                if (x.Length == 1 && !ThisString.StartsWith("|")) { newTXT = newTXT.Replace(x[0], string.Empty); }
            }

            if (Style == enShortenStyle.Replaced || Style == enShortenStyle.HTML || OT == newTXT) { return newTXT; }
            return OT + " (" + newTXT + ")";
        }

        private static string ColumnReplaceTranslated(string newTXT, ColumnItem column)
        {
            switch (column.Format)
            {
                case enDataFormat.Ganzzahl:
                case enDataFormat.Gleitkommazahl:
                case enDataFormat.Datum_und_Uhrzeit:
                case enDataFormat.FarbeInteger:
                case enDataFormat.Schrift:
                case enDataFormat.Text_mit_Formatierung:
                case enDataFormat.Link_To_Filesystem:
                    return newTXT;
                default:
                    return DoTranslate(newTXT, false);
            }

        }




        public static string DoTranslate(string txt) => DoTranslate(txt, true, EmptyArgs);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="txt"></param>
        /// <param name="mustTranslate">TRUE erstellt einen Eintrag in der Englisch-Datenbank, falls nicht vorhanden.</param>
        /// <returns></returns>
        public static string DoTranslate(string txt, bool mustTranslate, params object[] args)
        {

            if (Translation == null) { return string.Format(txt, args); }
            if (string.IsNullOrEmpty(txt)) { return string.Empty; }

            if (German == txt) { return string.Format(English, args); }

            German = txt;

            //if (txt.ContainsChars(Constants.Char_Numerals)) { English = German; return string.Format(English, args); }
            //if (txt.ToLower().Contains("imagecode")) { English = German; return string.Format(English, args); }

            var addend = string.Empty;

            if (txt.EndsWith(":"))
            {
                txt = txt.TrimEnd(":");
                addend = ":";
            }

            txt = txt.Replace("\r\n", "\r");

            var r = Translation.Row[txt];
            if (r == null)
            {
                if (Translation.ReadOnly) { English = German; return string.Format(English, args); }
                if (!mustTranslate) { English = German; return string.Format(English, args); }
                r = Translation.Row.Add(txt);
            }


            var t = r.CellGetString("Translation");
            if (string.IsNullOrEmpty(t)) { English = German; return string.Format(English, args); }
            English = t + addend;

            return string.Format(English, args);
        }

    }
}

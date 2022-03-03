// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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
using BlueDatabase.Enums;

namespace BlueDatabase {

    public static class LanguageTool {

        #region Fields

        public static Database? Translation = null;
        private static readonly object[] EmptyArgs = new object[0];
        private static string English = string.Empty;
        private static string German = string.Empty;

        #endregion

        #region Methods

        /// <summary>
        /// Fügt Präfix und Suffix hinzu und ersetzt den Text nach dem gewünschten Stil.
        /// </summary>
        /// <param name="txt"></param>
        /// <param name="column"></param>
        /// <param name="style"></param>
        /// <returns></returns>
        public static string ColumnReplace(string txt, ColumnItem column, enShortenStyle style) {
            if (!string.IsNullOrEmpty(txt)) {
                if (!string.IsNullOrEmpty(column.Prefix)) { txt = DoTranslate(column.Prefix, true) + " " + txt; }
                if (!string.IsNullOrEmpty(column.Suffix)) { txt = txt + " " + DoTranslate(column.Suffix, true); }
            }
            if (Translation != null) { return ColumnReplaceTranslated(txt, column); }
            if (style == enShortenStyle.Unreplaced || column.OpticalReplace.Count == 0) { return txt; }
            var OT = txt;
            foreach (var ThisString in column.OpticalReplace) {
                var x = ThisString.SplitAndCutBy("|");
                if (x.Length == 2) {
                    if (string.IsNullOrEmpty(x[0])) {
                        if (string.IsNullOrEmpty(txt)) { txt = x[1]; }
                    } else {
                        txt = txt.Replace(x[0], x[1]);
                    }
                }
                if (x.Length == 1 && !ThisString.StartsWith("|")) { txt = txt.Replace(x[0], string.Empty); }
            }
            return style == enShortenStyle.Replaced || style == enShortenStyle.HTML || OT == txt ? txt : OT + " (" + txt + ")";
        }

        public static string DoTranslate(string txt) => DoTranslate(txt, true, EmptyArgs);

        /// <summary>
        ///
        /// </summary>
        /// <param name="txt"></param>
        /// <param name="mustTranslate">TRUE erstellt einen Eintrag in der Englisch-Datenbank, falls nicht vorhanden.</param>
        /// <returns></returns>
        public static string DoTranslate(string txt, bool mustTranslate, params object[] args) {
            try {
                if (Translation == null) {
                    return args.GetUpperBound(0) < 0 ? txt : string.Format(txt, args);
                }
                if (string.IsNullOrEmpty(txt)) { return string.Empty; }
                if (German == txt) { return args.GetUpperBound(0) < 0 ? English : string.Format(English, args); }
                German = txt;
                //if (txt.ContainsChars(Constants.Char_Numerals)) { English = German; return string.Format(English, args); }
                //if (txt.ToLower().Contains("imagecode")) { English = German; return string.Format(English, args); }
                var addend = string.Empty;
                if (txt.EndsWith(":")) {
                    txt = txt.TrimEnd(":");
                    addend = ":";
                }
                txt = txt.Replace("\r\n", "\r");
                var r = Translation.Row[txt];
                if (r == null) {
                    if (Translation.ReadOnly) { English = German; return args.GetUpperBound(0) < 0 ? English : string.Format(English, args); }
                    if (!mustTranslate) { English = German; return args.GetUpperBound(0) < 0 ? English : string.Format(English, args); }
                    r = Translation.Row.Add(txt);
                }
                var t = r.CellGetString("Translation");
                if (string.IsNullOrEmpty(t)) { English = German; return args.GetUpperBound(0) < 0 ? English : string.Format(English, args); }
                English = t + addend;
                return args.GetUpperBound(0) < 0 ? English : string.Format(English, args);
            } catch {
                return txt;
            }
        }

        private static string ColumnReplaceTranslated(string newTXT, ColumnItem column) => column.Translate == enTranslationType.Übersetzen ? DoTranslate(newTXT, false) : newTXT;

        #endregion
    }
}
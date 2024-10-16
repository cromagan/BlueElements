// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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

using System.Collections.ObjectModel;
using BlueBasics;
using BlueBasics.Enums;
using BlueDatabase.Enums;

namespace BlueDatabase;

public static class LanguageTool {

    #region Fields

    public static Database? Translation = null;
    private static readonly object?[] EmptyArgs = [];
    private static string _english = string.Empty;
    private static string _german = string.Empty;

    #endregion

    #region Methods

    public static string DoTranslate(string txt) => DoTranslate(txt, true, EmptyArgs);

    /// <summary>
    ///
    /// </summary>
    /// <param name="txt"></param>
    /// <param name="mustTranslate">TRUE erstellt einen Eintrag in der Englisch-Datenbank, falls nicht vorhanden.</param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static string DoTranslate(string txt, bool mustTranslate, params object?[] args) {
        try {
            if (Translation == null) {
                return args.GetUpperBound(0) < 0 ? txt : string.Format(txt, args);
            }
            if (string.IsNullOrEmpty(txt)) { return string.Empty; }
            if (_german == txt) { return args.GetUpperBound(0) < 0 ? _english : string.Format(_english, args); }
            _german = txt;
            //if (txt.ContainsChars(Constants.Char_Numerals)) { English = German; return string.Format(English, args); }
            //if (txt.ToLowerInvariant().Contains("imagecode")) { English = German; return string.Format(English, args); }
            var addend = string.Empty;
            if (txt.EndsWith(":")) {
                txt = txt.TrimEnd(":");
                addend = ":";
            }
            txt = txt.Replace("\r\n", "\r");
            var r = Translation.Row[txt];
            if (r is not { IsDisposed: false }) {
                var m = Translation.EditableErrorReason(EditableErrorReasonType.EditAcut);
                if (!string.IsNullOrEmpty(m)) { _english = _german; return args.GetUpperBound(0) < 0 ? _english : string.Format(_english, args); }
                if (!mustTranslate) { _english = _german; return args.GetUpperBound(0) < 0 ? _english : string.Format(_english, args); }
                r = Translation.Row.GenerateAndAdd(txt, null, "Missing translation");
                if (r is not { IsDisposed: false }) { return args.GetUpperBound(0) < 0 ? txt : string.Format(txt, args); }
            }
            var t = r.CellGetString("Translation");
            if (string.IsNullOrEmpty(t)) { _english = _german; return args.GetUpperBound(0) < 0 ? _english : string.Format(_english, args); }
            _english = t + addend;
            return args.GetUpperBound(0) < 0 ? _english : string.Format(_english, args);
        } catch {
            return txt;
        }
    }

    /// <summary>
    /// Fügt Präfix und Suffix hinzu und ersetzt den Text nach dem gewünschten Stil.
    /// </summary>
    /// <param name="txt"></param>
    /// <param name="style"></param>
    /// <param name="prefix"></param>
    /// <param name="suffix"></param>
    /// <param name="doOpticalTranslation"></param>
    /// <param name="opticalReplace"></param>
    /// <returns></returns>
    public static string PrepaireText(string txt, ShortenStyle style, string prefix, string suffix, TranslationType doOpticalTranslation, ReadOnlyCollection<string>? opticalReplace) {
        if (!string.IsNullOrEmpty(txt)) {
            if (Translation != null && doOpticalTranslation == TranslationType.Übersetzen) {
                txt = DoTranslate(txt, true);
                if (!string.IsNullOrEmpty(prefix)) { prefix = DoTranslate(prefix, true); }
                if (!string.IsNullOrEmpty(suffix)) { suffix = DoTranslate(suffix, true); }
            }
            if (!string.IsNullOrEmpty(prefix)) { txt = $"{prefix} {txt}"; }
            if (!string.IsNullOrEmpty(suffix)) { txt = $"{txt} {suffix}"; }
        }

        if (opticalReplace == null || style == ShortenStyle.Unreplaced || opticalReplace.Count == 0) { return txt; }

        var ot = txt;
        foreach (var thisString in opticalReplace) {
            var x = thisString.SplitBy("|");

            if (x.Length == 2) {
                if (string.IsNullOrEmpty(x[0])) {
                    if (string.IsNullOrEmpty(txt)) { txt = x[1]; }
                } else {
                    txt = txt.Replace(x[0], x[1]);
                }
            }
        }

        if (style is ShortenStyle.Replaced or ShortenStyle.HTML || ot.Equals(txt)) { return txt; }

        return $"{ot} ({txt})";
    }

    #endregion
}
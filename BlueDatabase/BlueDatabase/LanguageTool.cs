﻿// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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

using System;
using BlueBasics;
using BlueBasics.Enums;
using BlueDatabase.Enums;
using BlueDatabase.Interfaces;

namespace BlueDatabase;

public static class LanguageTool {

    #region Fields

    public static DatabaseAbstract? Translation = null;
    private static readonly object?[] EmptyArgs = Array.Empty<object>();
    private static string _english = string.Empty;
    private static string _german = string.Empty;

    #endregion

    /// <summary>
    /// Fügt Präfix und Suffix hinzu und ersetzt den Text nach dem gewünschten Stil.
    /// </summary>
    /// <param name="txt"></param>
    /// <param name="column"></param>
    /// <param name="style"></param>
    /// <returns></returns>

    #region Methods

    public static string ColumnReplace(string txt, ColumnItem? column, ShortenStyle style) {
        if (column == null || column.IsDisposed) { return txt; }

        if (!string.IsNullOrEmpty(txt)) {
            if (!string.IsNullOrEmpty(column.Prefix)) { txt = DoTranslate(column.Prefix, true) + " " + txt; }
            if (!string.IsNullOrEmpty(column.Suffix)) { txt = txt + " " + DoTranslate(column.Suffix, true); }
        }
        if (Translation != null) { return ColumnReplaceTranslated(txt, column); }
        if (style == ShortenStyle.Unreplaced || column.OpticalReplace.Count == 0) { return txt; }
        var ot = txt;
        foreach (var thisString in column.OpticalReplace) {
            var x = thisString.SplitBy("|");

            if (x.Length == 2) { 


            //if (!string.IsNullOrEmpty(x[0]) && !string.IsNullOrEmpty(x[1]) && x[0] == txt) { txt = x[1]; }

                if (string.IsNullOrEmpty(x[0])) {
                    if (string.IsNullOrEmpty(txt)) { txt = x[1]; }
                } else {
                    txt = txt.Replace(x[0], x[1]);
                }
            }
            //if (x.Length == 1 && !thisString.StartsWith("|")) { txt = txt.Replace(x[0], string.Empty); }
        }


        if (style is ShortenStyle.Replaced or ShortenStyle.HTML || ot == txt) {
            return txt;
        }

        return ot + " (" + txt + ")";
    }

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
            //if (txt.ToLower().Contains("imagecode")) { English = German; return string.Format(English, args); }
            var addend = string.Empty;
            if (txt.EndsWith(":")) {
                txt = txt.TrimEnd(":");
                addend = ":";
            }
            txt = txt.Replace("\r\n", "\r");
            var r = Translation.Row[txt];
            if (r == null || r.IsDisposed) {
                var m = Translation.EditableErrorReason(EditableErrorReasonType.EditAcut);
                if (!string.IsNullOrEmpty(m)) { _english = _german; return args.GetUpperBound(0) < 0 ? _english : string.Format(_english, args); }
                if (!mustTranslate) { _english = _german; return args.GetUpperBound(0) < 0 ? _english : string.Format(_english, args); }
                r = Translation.Row.GenerateAndAdd(txt, "Missing translation");
                if (r == null || r.IsDisposed) { return args.GetUpperBound(0) < 0 ? txt : string.Format(txt, args); }
            }
            var t = r.CellGetString("Translation");
            if (string.IsNullOrEmpty(t)) { _english = _german; return args.GetUpperBound(0) < 0 ? _english : string.Format(_english, args); }
            _english = t + addend;
            return args.GetUpperBound(0) < 0 ? _english : string.Format(_english, args);
        } catch {
            return txt;
        }
    }

    private static string ColumnReplaceTranslated(string newTxt, IColumnInputFormat column) => column.DoOpticalTranslation == TranslationType.Übersetzen ? DoTranslate(newTxt, false) : newTxt;

    #endregion
}
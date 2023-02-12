// Authors:
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
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Extended_Text;
using BlueControls.Forms;
using BlueDatabase;
using BlueDatabase.Enums;

namespace BlueControls;

internal static class Dictionary {

    #region Fields

    public static bool IsSpellChecking;
    internal static readonly object LockSpellChecking = new();
    private static DatabaseAbstract? _dictWords;

    #endregion

    #region Methods

    public static bool DictionaryRunning(bool trytoinit) {
        if (trytoinit && _dictWords == null) { Init(); }
        return _dictWords != null;
    }

    public static bool IsWordOk(string word) {
        if (!DictionaryRunning(false)) { return true; }
        if (string.IsNullOrEmpty(word)) { return true; }
        if (word.Length == 1) { return true; }
        if (word.IsNumeral()) { return true; }
        if (Constants.Char_Numerals.Contains(word.Substring(0, 1))) { return true; }// z.B. 00 oder 1b oder 2L
        if (word != word.ToLower() && word != word.ToUpper() && word != word.Substring(0, 1).ToUpper() + word.Substring(1).ToLower()) { return false; }
        if (word == word.ToLower()) {
            // Wenn ein Wort klein geschrieben ist, mu0ß auch das kleingeschriebene in der Datenbank sein!
            return _dictWords.Row[new FilterItem(_dictWords.Column.First, FilterType.Istgleich, word)] != null;
        }
        return _dictWords.Row[word] != null;
    }

    public static bool IsWriteable() => _dictWords is Database db && !string.IsNullOrEmpty(db.Filename);

    public static List<string>? SimilarTo(string word) {
        if (IsWordOk(word)) { return null; }
        List<string> l = new();
        foreach (var thisRowItem in _dictWords.Row) {
            if (thisRowItem != null) {
                var w = thisRowItem.CellFirstString();
                var di = Generic.LevenshteinDistance(word.ToLower(), w.ToLower());
                if (di < word.Length / 2.0 || di < w.Length / 2.0) {
                    l.Add(di.ToString(Constants.Format_Integer5) + w);
                }
            }
        }
        if (l.Count == 0) { return null; }
        l.Sort();
        List<string> l2 = new();
        foreach (var thisstring in l) {
            l2.Add(thisstring.Substring(5));
            if (l2.Count == 10) { return l2; }
        }
        return l2;
    }

    public static void SpellCheckingAll(ExtText etxt, bool allOk) {
        var can = Monitor.TryEnter(LockSpellChecking);
        if (can) { Monitor.Exit(LockSpellChecking); }
        if (!can || IsSpellChecking) {
            MessageBox.Show("Die Rechtschreibprüfung steht<br>nicht zur Verfügung.", ImageCode.Information, "OK");
            return;
        }
        lock (LockSpellChecking) {
            var pos = 0;
            var woEnd = -1;
            IsSpellChecking = true;
            do {
                pos = Math.Max(woEnd + 1, pos + 1);
                if (pos >= etxt.Count) { break; }
                var woStart = etxt.WordStart(pos);
                if (woStart > -1) {
                    woEnd = etxt.WordEnd(pos);
                    var wort = etxt.Word(pos);
                    if (!IsWordOk(wort)) {
                        var butt = wort.ToLower() != wort
                            ? MessageBox.Show("<b>" + wort + "</b>", ImageCode.Stift, "'" + wort + "' aufnehmen", "'" + wort.ToLower() + "' aufnehmen", "Ignorieren", "Beenden")
                            : allOk ? 1 : MessageBox.Show("<b>" + wort + "</b>", ImageCode.Stift, "'" + wort + "' aufnehmen", "Ignorieren", "Beenden") + 1;
                        switch (butt) {
                            case 0:
                                WordAdd(wort);
                                break;

                            case 1:
                                WordAdd(wort.ToLower());
                                break;

                            case 2:
                                //  woEnd = woStart - 1
                                break;

                            case 3:
                                IsSpellChecking = false;
                                return;
                        }
                    }
                }
            } while (true);
            IsSpellChecking = false;
        }
    }

    public static void WordAdd(string wort) {
        if (_dictWords.Row[wort] != null) { _ = _dictWords.Row.Remove(_dictWords.Row[wort], "Remove Dictionary word for Upadte"); }
        _ = _dictWords.Row.GenerateAndAdd(wort, "Add Word (after deleting)");
    }

    private static void Init() {
        var tmp = DatabaseAbstract.LoadResource(Assembly.GetAssembly(typeof(Skin)), "Deutsch.MDB", "Dictionary", true, false);
        if (tmp is DatabaseAbstract DBD) { _dictWords = DBD; }
    }

    #endregion
}
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

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Extended_Text;
using BlueControls.Forms;
using BlueTable;
using BlueTable.Enums;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace BlueControls;

internal static class Dictionary {

    #region Fields

    public static bool IsSpellChecking;
    internal static readonly object LockSpellChecking = new();
    private static Table? _dictWords;

    #endregion

    #region Methods

    public static bool DictionaryRunning(bool trytoinit) {
        if (trytoinit && _dictWords == null) { Init(); }
        return _dictWords != null;
    }

    /// <summary>
    /// Ist das Dictionary nicht geladen, wird True zurück gegeben
    /// </summary>
    /// <param name="word"></param>
    /// <returns></returns>
    public static bool IsWordOk(string word) {
        if (!DictionaryRunning(false) || _dictWords?.Column.First is not { IsDisposed: false } fc) { return true; }
        if (string.IsNullOrEmpty(word)) { return true; }
        if (word.Length == 1) { return true; }
        if (word.IsNumeral()) { return true; }
        if (Constants.Char_Numerals.Contains(word.Substring(0, 1))) { return true; }// z.B. 00 oder 1b oder 2L

        //if (word != word.ToLowerInvariant() &&
        //    word != word.ToUpperInvariant() &&
        //    word != word.ToTitleCase()) { return false; }  //GmbH

        if (word == word.ToLowerInvariant() && word != word.ToUpperInvariant() && word != word.ToTitleCase()) {
            // Wenn ein Wort klein geschrieben ist
            // nicht GROSS gescrieben
            // oder nicht am Worftanfang
            // muss es genau so in der Tabelle sein!
            return _dictWords.Row[new FilterItem(fc, FilterType.Istgleich, word)] != null;
        }

        return _dictWords.Row[word] != null;
    }

    public static bool IsWriteable() => _dictWords is TableFile { IsDisposed: false } tbf && !tbf.CanSaveMainChunk().Failed;

    public static List<string>? SimilarTo(string word) {
        if (IsWordOk(word) || _dictWords == null) { return null; }
        List<string> l = [];
        foreach (var thisRowItem in _dictWords.Row) {
            if (thisRowItem != null) {
                var w = thisRowItem.CellFirstString();
                var di = Generic.LevenshteinDistance(word.ToLowerInvariant(), w.ToLowerInvariant());
                if (di < word.Length / 2.0 || di < w.Length / 2.0) {
                    l.Add(di.ToString5() + w);
                }
            }
        }
        if (l.Count == 0) { return null; }
        l.Sort();
        List<string> l2 = [];
        foreach (var thisstring in l) {
            l2.Add(thisstring.Substring(5));
            if (l2.Count == 10) { return l2; }
        }
        return l2;
    }

    public static void SpellCheckingAll(ExtText etxt, bool allOk) {
        var can = Monitor.TryEnter(LockSpellChecking);

        if (!can || IsSpellChecking) {
            if (can) { Monitor.Exit(LockSpellChecking); }
            MessageBox.Show("Die Rechtschreibprüfung steht<br>nicht zur Verfügung.", ImageCode.Information, "OK");
            return;
        }

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
                    var butt = wort.ToLowerInvariant() != wort
                        ? MessageBox.Show("<b>" + wort + "</b>", ImageCode.Stift, "'" + wort + "' aufnehmen", "'" + wort.ToLowerInvariant() + "' aufnehmen", "Ignorieren", "Beenden")
                        : allOk ? 1 : MessageBox.Show("<b>" + wort + "</b>", ImageCode.Stift, "'" + wort + "' aufnehmen", "Ignorieren", "Beenden") + 1;
                    switch (butt) {
                        case 0:
                            WordAdd(wort);
                            break;

                        case 1:
                            WordAdd(wort.ToLowerInvariant());
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
        Monitor.Exit(LockSpellChecking);
    }

    public static void WordAdd(string wort) {
        if (_dictWords == null) { return; }
        if (_dictWords.Row[wort] != null) { RowCollection.Remove(_dictWords.Row[wort], "Remove Dictionary word for Upadte"); }
        _dictWords.Row.GenerateAndAdd(wort, "Add Word (after deleting)");
    }

    private static void Init() {
        var tmp = Table.LoadResource(Assembly.GetAssembly(typeof(Skin)), "Deutsch.BDB", "Dictionary", false, false);
        if (tmp is { IsDisposed: false } tb) { _dictWords = tb; }
    }

    #endregion
}
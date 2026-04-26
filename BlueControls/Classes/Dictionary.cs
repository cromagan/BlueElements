// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
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
using BlueBasics.ClassesStatic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;

namespace BlueControls.Classes;

internal static class Dictionary {

    #region Fields

    internal static readonly object _lockSpellChecking = new();
    private static Dictionary<string, string>? _dictWords;
    private static bool _initFailed;
    private static bool _loadedFromStream;

    #endregion

    #region Methods

    public static bool DictionaryRunning(bool trytoinit) {
        if (trytoinit && _dictWords == null && !_initFailed) { Init(); }
        return _dictWords != null;
    }

    /// <summary>
    /// Prüft, ob ein Wort korrekt geschrieben ist.
    /// </summary>
    public static bool IsWordOk(string word, IReadOnlySet<string>? additionalWords) {
        if (!DictionaryRunning(false) || _dictWords == null) { return true; }
        if (string.IsNullOrEmpty(word)) { return true; }
        if (word.Length == 1) { return true; }
        if (Constants.Char_Numerals.Contains(word[0])) { return true; } // z.B. 00 oder 1b oder 2L

        if (_dictWords.TryGetValue(word, out var stored)) {
            // Klein geschriebene Wörter müssen exakt im Wörterbuch stehen
            if (word.Equals(word, StringComparison.OrdinalIgnoreCase)) { return stored == word; }
            // Groß/title-case Wörter: case-insensitive Match reicht
            return true;
        }

        if (additionalWords != null) {
            return additionalWords.Contains(word) || additionalWords.Contains(word.ToLowerInvariant());
        }

        return false;
    }

    public static bool ContainsWord(string word) {
        if (!DictionaryRunning(true) || _dictWords == null) { return false; }
        if (string.IsNullOrEmpty(word)) { return false; }
        return _dictWords.ContainsKey(word) || _dictWords.ContainsKey(word.ToLowerInvariant());
    }

    public static bool IsWriteable() => !_loadedFromStream && DictionaryRunning(false);

    public static List<string>? SimilarTo(string word, IReadOnlySet<string>? additionalWords) {
        if (IsWordOk(word, additionalWords)) { return null; }

        List<string> l = [];
        var wordLow = word.ToLowerInvariant();

        if (_dictWords != null) {
            foreach (var w in _dictWords.Values) {
                var di = Generic.LevenshteinDistance(wordLow, w.ToLowerInvariant());
                if (di < word.Length / 2.0 || di < w.Length / 2.0) {
                    l.Add(di.ToString5() + w);
                }
            }
        }

        if (additionalWords != null) {
            foreach (var w in additionalWords) {
                var di = Generic.LevenshteinDistance(wordLow, w.ToLowerInvariant());
                if (di < word.Length / 2.0 || di < w.Length / 2.0) {
                    l.Add(di.ToString5() + w);
                }
            }
        }

        if (l.Count == 0) { return null; }
        l.Sort();
        List<string> l2 = [];
        foreach (var thisstring in l) {
            l2.Add(thisstring[5..]);
            if (l2.Count == 10) { return l2; }
        }
        return l2;
    }

    public static void WordAdd(string wort) {
        if (_dictWords == null || _loadedFromStream) { return; }
        if (_dictWords.ContainsKey(wort)) { return; }
        _dictWords[wort] = wort;
        SaveDictFile();
    }

    private static string DictFilePath() => Path.Combine(Develop.AppPath(), "Deutsch.bin");

    private static void Init() {
        try {
            var dictFile = DictFilePath();

            // Zuerst versuchen, die Datei von der Festplatte zu laden (enthält Benutzereinträge)
            if (IO.FileExists(dictFile)) {
                var content = IO.ReadAllText(dictFile, System.Text.Encoding.UTF8);
                LoadFromText(content);
                return;
            }

            // Sonst aus der eingebetteten Ressource laden
            var assembly = Assembly.GetAssembly(typeof(Dictionary));
            var stream = Generic.GetEmmbedResource(assembly, "Deutsch.bin");

            if (stream != null) {
                using var reader = new StreamReader(stream, System.Text.Encoding.UTF8);
                LoadFromText(reader.ReadToEnd());
                _loadedFromStream = true;
                return;
            }

            _initFailed = true;
        } catch (Exception) {
            _initFailed = true;
        }
    }

    private static void LoadFromText(string content) {
        var words = content.SplitAndCutByCr();

        _dictWords = new Dictionary<string, string>(words.Length, StringComparer.OrdinalIgnoreCase);
        foreach (var w in words) { _dictWords[w] = w; }
    }

    private static void SaveDictFile() {
        if (_dictWords == null || _loadedFromStream) { return; }
        try {
            var sorted = _dictWords.Values.OrderBy(x => x, StringComparer.OrdinalIgnoreCase);
            IO.WriteAllText(DictFilePath(), string.Join("\r\n", sorted), System.Text.Encoding.UTF8, false);
        } catch {
            _initFailed = true;
        }
    }

    #endregion
}
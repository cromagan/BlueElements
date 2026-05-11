// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.IO;
using System.Reflection;

namespace BlueControls.Classes;

internal static class Dictionary {

    #region Fields

    internal static readonly object _lockSpellChecking = new();
    private static Dictionary<string, string>? _dictWords;
    private static bool _initFailed;

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
#pragma warning disable CA1862 // "StringComparison"-Methodenüberladungen verwenden, um Zeichenfolgenvergleiche ohne Beachtung der Groß-/Kleinschreibung durchzuführen
            if (stored != stored.ToLowerInvariant()) { return stored == word; }            // Groß/title-case Wörter: case-insensitive Match reicht
#pragma warning restore CA1862 // "StringComparison"-Methodenüberladungen verwenden, um Zeichenfolgenvergleiche ohne Beachtung der Groß-/Kleinschreibung durchzuführen
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

    private static void Init() {
        try {
            var assembly = Assembly.GetAssembly(typeof(Dictionary));
            if (assembly == null) {
                _initFailed = true;
                return;
            }

            using var stream = Generic.GetEmmbedResource(assembly, "Deutsch.bin");
            if (stream == null) {
                _initFailed = true;
                return;
            }
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            var data = ms.ToArray();
            var unzipped = data.UnzipIt();
            if (unzipped is null) {
                _initFailed = true;
                return;
            }

            _dictWords = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            // Wir arbeiten direkt auf dem byte-Array (Span)
            ReadOnlySpan<byte> span = unzipped;
            int start = 0;

            for (int i = 0; i <= span.Length; i++) {
                // Prüfe auf Zeilenumbruch (13 = \r, 10 = \n) oder Ende des Arrays
                if (i == span.Length || span[i] == 13 || span[i] == 10) {
                    int length = i - start;
                    if (length > 0) {
                        var word = System.Text.Encoding.UTF8.GetString(span.Slice(start, length));

                        _dictWords[word] = word;
                    }
                    start = i + 1;
                }
            }
        } catch {
            _initFailed = true;
        }
    }

    #endregion
}
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

using BlueBasics.ClassesStatic;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static BlueBasics.ClassesStatic.IO;

namespace BlueBasics;

public static partial class Extensions {

    #region Methods

    public static bool AddIfNotExists<T>(this ICollection<T> l, ICollection<T>? values) {
        if (values is not { Count: not 0 }) { return false; }
        return values.Count(l.AddIfNotExists) > 0;
    }

    public static bool AddIfNotExists<T>(this ICollection<T> l, T value) {
        if (l.Contains(value)) { return false; }
        l.Add(value);
        return true;
    }

    public static void AddRange<T>(this ConcurrentBag<T> @this, IEnumerable<T> toAdd) => toAdd.AsParallel().ForAll(@this.Add);

    public static List<T> Clone<T>(this ICollection<T> l) => [.. l];

    public static List<T> CloneWithClones<T>(this ICollection<T>? l) where T : ICloneable {
        var l2 = new List<T>();

        if (l == null) { return l2; }

        foreach (var item in l) {
            var it = item?.Clone();
            if (it != null) {
                l2.Add((T)it);
            }
        }

        return l2;
    }

    public static string FinishParseable(this ICollection<string> col) => "{" + col.JoinWith(", ") + "}";

    public static List<string> FromNonCritical(this ICollection<string> col) {
        var l = new List<string>();
        if (col.Count == 0) { return l; }

        foreach (var thiss in col) {
            l.Add(thiss.FromNonCritical());
        }

        return l;
    }

    public static T? GetByKey<T>(this IEnumerable<T?>? items, string? name) where T : IHasKeyName {
        if (name is not { } || string.IsNullOrEmpty(name)) { return default; }

        if (items == null || !items.Any() || items.First() is not { } i) { return default; }

        if (i.KeyIsCaseSensitive) {
            return items.FirstOrDefault(thisp =>
                thisp != null && string.Equals(thisp.KeyName, name, StringComparison.Ordinal));
        } else {
            return items.FirstOrDefault(thisp =>
                thisp != null && string.Equals(thisp.KeyName, name, StringComparison.OrdinalIgnoreCase));
        }
    }

    public static int IndexOf<T>(this IEnumerable<T?>? items, string name) where T : IHasKeyName {
        if (string.IsNullOrEmpty(name) || items == null) { return -1; }

        for (var z = 0; z < items.Count(); z++) {
            var thisp = items.ElementAt(z);
            if (thisp != null && string.Equals(thisp.KeyName, name, StringComparison.OrdinalIgnoreCase)) {
                return z;
            }
        }

        return -1;
    }

    public static bool IsDifferentTo<T>(this IEnumerable<T>? list1, IEnumerable<T>? list2) =>
                    // https://docs.microsoft.com/en-us/dotnet/api/system.linq.enumerable.sequenceequal?redirectedfrom=MSDN&view=netcore-3.1#System_Linq_Enumerable_SequenceEqual__1_System_Collections_Generic_IEnumerable___0__System_Collections_Generic_IEnumerable___0__
                    list1 != list2 && (list1 is null || list2 is null || !list1.SequenceEqual(list2));

    public static void Load(this List<string> l, string filename, Encoding code) {
        var t = ReadAllText(filename, code);
        l.Clear();
        l.AddRange(t.SplitAndCutByCr());
    }

    public static string Parseable(this ICollection<string> col, string baseToString) {
        if (string.IsNullOrEmpty(baseToString) ||
            !baseToString.StartsWith("{") ||
            !baseToString.EndsWith("}")) {
            Develop.DebugPrint(ErrorType.Error, "Basestringfehler!");
        }

        if (col.Count == 0) { return baseToString; } // Sonst gibts zusätzliche Kommas...

        return baseToString.Substring(0, baseToString.Length - 1) + ", " + col.JoinWith(", ") + "}";
    }

    public static void Remove<T>(this List<T> items, string name) where T : class, IHasKeyName {
        if (string.IsNullOrEmpty(name)) { return; }

        items.RemoveAll(item => item != null && string.Equals(item.KeyName, name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    ///  Falls der Dateityp String ist, WIRD zwischen Gross und Kleinschreibung unterschieden! Dafür kann RemoveString benutzt werden.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="l"></param>
    /// <param name="value"></param>
    public static void Remove<T>(this IList<T> l, T value) where T : IComparable {
        do { } while (l.Remove(value));
    }

    public static void RemoveAll(this IList list) {
        while (list.Count > 0) {
            list.RemoveAt(list.Count - 1);
        }
    }

    public static bool RemoveNull<T>(this IList<T>? l) {
        if (l is not { Count: not 0 }) { return false; }
        var did = false;
        var z = 0;
        while (z < l.Count) {
            var x = l[z];
            if (x == null || x.Equals(default(T))) {
                l.RemoveAt(z);
                did = true;
            } else {
                z++;
            }
        }
        return did;
    }

    public static bool RemoveNullOrEmpty<T>(this IList<T>? l) where T : ICanBeEmpty {
        if (l is not { Count: not 0 }) { return false; }
        var did = false;
        var z = 0;
        while (z < l.Count) {
            if (l[z] == null || l[z].IsNullOrEmpty()) {
                l.RemoveAt(z);
                did = true;
            } else {
                z++;
            }
        }
        return did;
    }

    public static void RemoveNullOrEmpty(this IList<string?> l) {
        if (l.Count == 0) { return; }

        var z = 0;
        while (z < l.Count) {
            if (string.IsNullOrEmpty(l[z])) {
                l.RemoveAt(z);
            } else {
                z++;
            }
        }
    }

    public static void RemoveString(this IList<string>? l, string value, bool caseSensitive) {
        if (l is not { Count: not 0 }) { return; }

        var cas = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
        var z = 0;
        while (z < l.Count) {
            if (string.Equals(l[z], value, cas)) {
                l.RemoveAt(z);
            } else {
                z++;
            }
        }
    }

    public static void RemoveString(this IList<string>? l, List<string>? value, bool caseSensitive) {
        if (l == null || value == null) { return; }

        foreach (var t in value) {
            l.RemoveString(t, caseSensitive);
        }
    }

    public static void Shuffle<T>(this IList<T> list) {
        for (var i1 = 0; i1 < list.Count; i1++) {
            var i2 = Constants.GlobalRnd.Next(i1, list.Count);
            if (i1 != i2) {
                var v1 = list[i1];
                var v2 = list[i2];
                // Generic.Swap(ref tempVar, ref tempVar2);
                list[i1] = v2;
                list[i2] = v1;
            }
        }
    }

    /// <summary>
    /// Sortiert die Liste alphabetisch und gibt diese ohne doppelten Einträgen und ohne Leeren zurück.
    /// </summary>
    /// <param name="arr"></param>
    /// <returns></returns>
    public static List<string> SortedDistinctList(this IEnumerable<string>? arr) {
        if (arr == null) { return []; }

        var arr2 = arr.Distinct().ToList();
        arr2.Remove(string.Empty);
        arr2.Sort();
        return arr2;
    }

    public static void SplitAndCutByCr(this List<string> list, string textToSplit) {
        List<string> l = [.. textToSplit.SplitAndCutByCr()];
        if (!list.IsDifferentTo(l)) { return; }
        if (list.Count > 0) { list.Clear(); }
        list.AddRange(l);
    }

    public static void SplitAndCutByCr_QuickSortAndRemoveDouble(this List<string> list, string textToSplit) {
        List<string> l = [.. textToSplit.SplitAndCutByCr()];
        l = l.SortedDistinctList();

        if (!list.IsDifferentTo(l)) { return; }

        if (list.Count > 0) { list.Clear(); }
        list.AddRange(l);
    }

    public static string TagGet(this IEnumerable<string>? list, string tagName) {
        if (list == null) { return string.Empty; }
        var uTagName = tagName.ToUpperInvariant().Trim();
        foreach (var thisString in list) {
            if (thisString.StartsWith(uTagName, StringComparison.OrdinalIgnoreCase)) {
                if (thisString.StartsWith(uTagName + ": ", StringComparison.OrdinalIgnoreCase)) { return thisString.Substring(uTagName.Length + 2); }
                if (thisString.StartsWith(uTagName + ":", StringComparison.OrdinalIgnoreCase)) { return thisString.Substring(uTagName.Length + 1); }
                if (thisString.StartsWith(uTagName + " = ", StringComparison.OrdinalIgnoreCase)) { return thisString.Substring(uTagName.Length + 3); }
                if (thisString.StartsWith(uTagName + "=", StringComparison.OrdinalIgnoreCase)) { return thisString.Substring(uTagName.Length + 1); }
            }
        }
        return string.Empty;
    }

    public static List<string> TagGetAll(this IEnumerable<string>? list, string tagName) {
        List<string> l = [];
        if (list == null) { return l; }
        var uTagName = tagName.ToUpperInvariant();
        foreach (var thisString in list) {
            if (thisString.StartsWith(uTagName, StringComparison.OrdinalIgnoreCase)) {
                if (thisString.StartsWith(uTagName + ": ", StringComparison.OrdinalIgnoreCase)) {
                    l.Add(thisString.Substring(uTagName.Length + 2));
                } else {
                    if (thisString.StartsWith(uTagName + ":", StringComparison.OrdinalIgnoreCase)) { l.Add(thisString.Substring(uTagName.Length + 1)); }
                }
                if (thisString.StartsWith(uTagName + " = ", StringComparison.OrdinalIgnoreCase)) {
                    l.Add(thisString.Substring(uTagName.Length + 3));
                } else {
                    if (thisString.StartsWith(uTagName + "=", StringComparison.OrdinalIgnoreCase)) { l.Add(thisString.Substring(uTagName.Length + 1)); }
                }
            }
        }
        return l;
    }

    /// <summary>
    /// Führt bei allem Typen ein ToString aus und addiert diese mittels \r. Enthält ein ToString ein \r, dann wird abgebrochen.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="removeEmpty"></param>
    /// <returns></returns>
    public static string ToString<T>(this IEnumerable<T> items, bool removeEmpty) where T : IStringable? {
        var sb = new StringBuilder();

        foreach (var thisItem in items.ToList()) {
            var itemString = thisItem?.ParseableItems().FinishParseable() ?? string.Empty;

            if (itemString.Contains("\r")) { Develop.DebugPrint(ErrorType.Error, "List.Tostring hat einen Zeilenumbruch gefunden."); }

            if (!removeEmpty || !string.IsNullOrEmpty(itemString)) {
                if (sb.Length > 0) { sb.Append("\r"); }
                sb.Append(itemString);
            }
        }
        return sb.ToString();
    }

    public static bool WriteAllText(this IEnumerable<string> l, string filename, Encoding endcoding, bool executeAfter) {
        CreateDirectory(filename.FilePath());
        var t = string.Join("\r\n", l);
        return IO.WriteAllText(filename, t, endcoding, executeAfter);
    }

    #endregion
}
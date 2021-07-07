// Authors:
// Christian Peter
//
// Copyright (c) 2021 Christian Peter
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

using BlueBasics.Enums;
using BlueBasics.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using static BlueBasics.FileOperations;
using static BlueBasics.modConverter;

namespace BlueBasics {

    public static class ListOfExtension {

        #region Methods

        public static bool AddIfNotExists<T>(this List<T> l, BindingList<T> values) {
            if (values == null || values.Count == 0) { return false; }
            var ok1 = false;
            foreach (var thivalue in values) {
                if (l.AddIfNotExists(thivalue)) {
                    ok1 = true;
                }
            }
            return ok1;
        }

        public static bool AddIfNotExists<T>(this List<T> l, List<T> values) {
            if (values == null || values.Count == 0) { return false; }
            var ok1 = false;
            foreach (var thivalue in values) {
                if (l.AddIfNotExists(thivalue)) {
                    ok1 = true;
                }
            }
            return ok1;
        }

        public static bool AddIfNotExists<T>(this List<T> l, T value) {
            if (!l.Contains(value)) {
                l.Add(value);
                return true;
            }
            return false;
        }

        public static bool IsDifferentTo<T>(this List<T> list1, List<T> list2) =>
                    // https://docs.microsoft.com/en-us/dotnet/api/system.linq.enumerable.sequenceequal?redirectedfrom=MSDN&view=netcore-3.1#System_Linq_Enumerable_SequenceEqual__1_System_Collections_Generic_IEnumerable___0__System_Collections_Generic_IEnumerable___0__
                    list1 != list2 && (list1 is null || list2 is null || !list1.SequenceEqual(list2));

        public static void Load(this List<string> l, string filename, System.Text.Encoding code) {
            var t = File.ReadAllText(filename, code);
            l.Clear();
            l.AddRange(t.SplitByCR());
        }

        /// <summary>
        ///  Falls der Dateityp String ist, WIRD zwischen Gross und Kleinschreibung unterschieden! Dafür kann RemoveString benutzt werden.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="l"></param>
        /// <param name="value"></param>
        public static void Remove<T>(this List<T> l, T value) where T : IComparable {
            do { } while (l.Remove(value));
        }

        public static bool RemoveNull<T>(this List<T> l) {
            if (l == null || l.Count == 0) { return false; }
            var Did = false;
            var z = 0;
            while (z < l.Count) {
                if (l[z] == null || l[z].Equals(default(T))) {
                    l.RemoveAt(z);
                    Did = true;
                } else {
                    z++;
                }
            }
            return Did;
        }

        public static bool RemoveNullOrEmpty<T>(this List<T> l) where T : ICanBeEmpty {
            if (l == null || l.Count == 0) { return false; }
            var Did = false;
            var z = 0;
            while (z < l.Count) {
                if (l[z] == null || l[z].IsNullOrEmpty()) {
                    l.RemoveAt(z);
                    Did = true;
                } else {
                    z++;
                }
            }
            return Did;
        }

        ///// <summary>
        ///// Falls der Dateityp String ist, WIRD zwischen Gross und Kleinschreibung unterschieden!
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="L"></param>
        ///// <param name="Value"></param>
        // public static void Remove<T>(this List<T> L, List<T> Value) where T : IComparable
        // {
        //    foreach (var Item in Value)
        //    {
        //        L.Remove(Item);
        //    }
        // }
        public static void RemoveNullOrEmpty(this List<string> l) {
            var z = 0;
            while (z < l.Count) {
                if (string.IsNullOrEmpty(l[z])) {
                    l.RemoveAt(z);
                } else {
                    z++;
                }
            }
        }

        public static void RemoveString(this List<string> l, string value, bool caseSensitive) {
            var cas = StringComparison.OrdinalIgnoreCase;
            if (!caseSensitive) { cas = StringComparison.Ordinal; }
            var z = 0;
            while (z < l.Count) {
                if (string.Equals(l[z], value, cas)) {
                    l.RemoveAt(z);
                } else {
                    z++;
                }
            }
        }

        public static void RemoveString(this List<string> l, List<string> value, bool caseSensitive) {
            foreach (var t in value) {
                l.RemoveString(t, caseSensitive);
            }
        }

        public static void RemoveString(this List<string> l, string[] value, bool caseSensitive) {
            for (var z = 0; z <= value.GetUpperBound(0); z++) {
                l.RemoveString(value[z], caseSensitive);
            }
        }

        public static void Save(this List<string> l, string dateiName, System.Text.Encoding code, bool executeAfter) {
            var t = l.JoinWith("\r\n").TrimEnd("\r\n");
            if (!PathExists(dateiName.FilePath())) {
                Directory.CreateDirectory(dateiName.FilePath());
            }
            WriteAllText(dateiName, t, code, executeAfter);
        }

        // public static bool IsDifferentTo<T>(this List<T> List1, List<T> List2) where T : IParseable
        // {
        //    if (List1.Count != List2.Count) { return true; }
        // return List1.Where((t, Count) => t.ToString() != List2[Count].ToString()).Any();
        // }
        // public static bool IsDifferentTo(this List<string> List1, List<string> List2)
        // {
        //    if (List1.Count != List2.Count) { return true; }
        // return List1.Where((t, Count) => t != List2[Count]).Any();
        // }
        // public static bool IsDifferentTo(this List<string> List1, BindingList<string> List2)
        // {
        //    if (List1.Count != List2.Count) { return true; }
        // return List1.Where((t, Count) => t != List2[Count]).Any();
        // }
        public static void Shuffle<T>(this IList<T> list) {
            for (var i1 = 0; i1 < list.Count; i1++) {
                var i2 = Constants.GlobalRND.Next(i1, list.Count);
                if (i1 != i2) {
                    var v1 = list[i1];
                    var v2 = list[i2];
                    // modAllgemein.Swap(ref tempVar, ref tempVar2);
                    list[i1] = v2;
                    list[i2] = v1;
                }
            }
        }

        public static void SplitByCR(this List<string> list, string textToSplit) {
            List<string> l = new();
            l.AddRange(textToSplit.SplitByCR());
            if (!list.IsDifferentTo(l)) { return; }
            if (list.Count > 0) { list.Clear(); }
            list.AddRange(l);
        }

        public static void SplitByCR_QuickSortAndRemoveDouble(this List<string> list, string textToSplit) {
            List<string> l = new();
            l.AddRange(textToSplit.SplitByCR());
            l = l.SortedDistinctList();
            if (!list.IsDifferentTo(l)) { return; }
            if (list.Count > 0) { list.Clear(); }
            list.AddRange(l);
        }

        public static string TagGet(this ICollection<string> _String, string tagName) {
            if (_String == null) { return string.Empty; }
            var uTagName = tagName.ToUpper().Trim();
            foreach (var ThisString in _String) {
                if (ThisString.ToUpper().StartsWith(uTagName)) {
                    if (ThisString.ToUpper().StartsWith(uTagName + ": ")) { return ThisString.Substring(uTagName.Length + 2); }
                    if (ThisString.ToUpper().StartsWith(uTagName + ":")) { return ThisString.Substring(uTagName.Length + 1); }
                    if (ThisString.ToUpper().StartsWith(uTagName + " = ")) { return ThisString.Substring(uTagName.Length + 3); }
                    if (ThisString.ToUpper().StartsWith(uTagName + "=")) { return ThisString.Substring(uTagName.Length + 1); }
                }
            }
            return string.Empty;
        }

        public static List<string> TagGetAll(this ICollection<string> _String, string tagName) {
            List<string> l = new();
            if (_String == null) { return l; }
            var uTagName = tagName.ToUpper();
            foreach (var ThisString in _String) {
                if (ThisString.ToUpper().StartsWith(uTagName)) {
                    if (ThisString.ToUpper().StartsWith(uTagName + ": ")) {
                        l.Add(ThisString.Substring(uTagName.Length + 2));
                    } else {
                        if (ThisString.ToUpper().StartsWith(uTagName + ":")) { l.Add(ThisString.Substring(uTagName.Length + 1)); }
                    }
                    if (ThisString.ToUpper().StartsWith(uTagName + " = ")) {
                        l.Add(ThisString.Substring(uTagName.Length + 3));
                    } else {
                        if (ThisString.ToUpper().StartsWith(uTagName + "=")) { l.Add(ThisString.Substring(uTagName.Length + 1)); }
                    }
                }
            }
            return l;
        }

        public static double TagGetDouble(this ICollection<string> _String, string tagName) => DoubleParse(TagGet(_String, tagName));

        public static int TagGetInt(this ICollection<string> _String, string tagName) => IntParse(TagGet(_String, tagName));

        public static void TagSet(this ICollection<string> _String, string tagNamex, string value) {
            var uTagName = tagNamex.ToUpper() + ":";
            var Found = -1;
            for (var z = 0; z < _String.Count; z++) {
                if (_String.ElementAtOrDefault(z)?.Length > uTagName.Length + 1 && _String.ElementAtOrDefault(z)?.Substring(0, uTagName.Length + 1).ToUpper() == uTagName + " ") {
                    Found = z;
                    break;
                }
                if (_String.ElementAtOrDefault(z)?.Length > uTagName.Length && _String.ElementAtOrDefault(z)?.Substring(0, uTagName.Length).ToUpper() == uTagName) {
                    Found = z;
                    break;
                }
            }
            var n = tagNamex + ": " + value;
            if (Found >= 0) {
                if (_String.ElementAtOrDefault(Found) == n) {
                    return;
                }
                _String.Remove(_String.ElementAtOrDefault(Found));
            }
            _String.Add(n);
        }

        /// <summary>
        /// Führt bei allem Typen ein ToString aus und addiert diese mittels \r. Enthält ein ToString ein \r, dann wird abgebrochen.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="l"></param>
        /// <param name="removeEmpty"></param>
        /// <returns></returns>
        public static string ToString<T>(this List<T> l, bool removeEmpty) where T : IParseable {
            // Remove Empty sollte eigentlich selbstverständlich seih. Ist nur als Dummy drinnen, dass der Interpreter zwischen der Internen und Extension unterscheiden kann.
            var tmp = string.Empty;
            foreach (var Item in l) {
                var tmp2 = string.Empty;
                if (Item != null) { tmp2 = Item.ToString(); }
                if (tmp2.Contains("\r")) { Develop.DebugPrint(enFehlerArt.Fehler, "List.Tostring hat einen Zeilenumbruch gefunden."); }
                if (!removeEmpty || !string.IsNullOrEmpty(tmp2)) {
                    tmp = tmp + tmp2 + "\r";
                }
            }
            return tmp.TrimCr();
        }

        #endregion
    }
}
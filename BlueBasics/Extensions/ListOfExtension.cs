﻿// Authors:
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

#nullable enable

using BlueBasics.Enums;
using BlueBasics.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using static BlueBasics.FileOperations;

namespace BlueBasics;

public static partial class Extensions {

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
        var ok1 = values.Count(thivalue => l.AddIfNotExists(thivalue)) > 0;
        return ok1;
    }

    public static bool AddIfNotExists<T>(this List<T> l, T value) {
        if (l.Contains(value)) {
            return false;
        }

        l.Add(value);
        return true;
    }

    public static void CloneFrom<T>(this List<T> list1, List<T> list2) where T : IComparable {
        if (!list1.IsDifferentTo(list2)) { return; }

        //if (list2 == null) { list1 = null; return; }

        if (list1 == null || list2 == null) {
            Develop.DebugPrint("Null-Listen nicht unterstützt.");
            return;
        }

        for (var z = 0; z < list2.Count; z++) {
            if (z >= list1.Count) { list1.Add(list2[z]); }

            if (list2[z] != null && !list1[z].Equals(list2[z])) { list1[z] = list2[z]; }

            if (list1[z] != null && list2[z] == null) { list1[z] = default; }
        }

        while (list1.Count > list2.Count) {
            list1.RemoveAt(list2.Count);
        }
    }

    public static bool IsDifferentTo<T>(this List<T>? list1, List<T>? list2) =>
        // https://docs.microsoft.com/en-us/dotnet/api/system.linq.enumerable.sequenceequal?redirectedfrom=MSDN&view=netcore-3.1#System_Linq_Enumerable_SequenceEqual__1_System_Collections_Generic_IEnumerable___0__System_Collections_Generic_IEnumerable___0__
        list1 != list2 && (list1 is null || list2 is null || !list1.SequenceEqual(list2));

    public static void Load(this List<string> l, string filename, System.Text.Encoding code) {
        var t = File.ReadAllText(filename, code);
        l.Clear();
        l.AddRange(t.SplitAndCutByCr());
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

    public static bool RemoveNull<T>(this List<T>? l) {
        if (l == null || l.Count == 0) { return false; }
        var did = false;
        var z = 0;
        while (z < l.Count) {
            if (l[z] == null || l[z].Equals(default(T))) {
                l.RemoveAt(z);
                did = true;
            } else {
                z++;
            }
        }
        return did;
    }

    public static bool RemoveNullOrEmpty<T>(this List<T> l) where T : ICanBeEmpty {
        if (l == null || l.Count == 0) { return false; }
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
    public static void RemoveNullOrEmpty(this List<string?>? l) {
        var z = 0;
        while (z < l.Count) {
            if (string.IsNullOrEmpty(l[z])) {
                l.RemoveAt(z);
            } else {
                z++;
            }
        }
    }

    public static void RemoveString(this List<string?>? l, string value, bool caseSensitive) {
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

    public static void RemoveString(this List<string>? l, List<string>? value, bool caseSensitive) {
        if (l == null || value == null) { return; }

        foreach (var t in value) {
            l!.RemoveString(t, caseSensitive);
        }
    }

    public static void RemoveString(this List<string>? l, string[]? value, bool caseSensitive) {
        if (l == null || value == null) { return; }

        for (var z = 0; z <= value.GetUpperBound(0); z++) {
            l!.RemoveString(value[z], caseSensitive);
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
                // Generic.Swap(ref tempVar, ref tempVar2);
                list[i1] = v2;
                list[i2] = v1;
            }
        }
    }

    public static void SplitAndCutByCr(this List<string?> list, string textToSplit) {
        List<string?> l = new();
        l.AddRange(textToSplit.SplitAndCutByCr());
        if (!list.IsDifferentTo(l)) { return; }
        if (list.Count > 0) { list.Clear(); }
        list.AddRange(l);
    }

    public static void SplitAndCutByCr_QuickSortAndRemoveDouble(this List<string> list, string textToSplit) {
        List<string?> l = new();
        l.AddRange(textToSplit.SplitAndCutByCr());
        l = l.SortedDistinctList();

        if (!list.IsDifferentTo(l)) { return; }

        if (list.Count > 0) { list.Clear(); }
        list.AddRange(l);
    }

    public static string TagGet(this ICollection<string>? list, string tagName) {
        if (list == null) { return string.Empty; }
        var uTagName = tagName.ToUpper().Trim();
        foreach (var thisString in list) {
            if (thisString.ToUpper().StartsWith(uTagName)) {
                if (thisString.ToUpper().StartsWith(uTagName + ": ")) { return thisString.Substring(uTagName.Length + 2); }
                if (thisString.ToUpper().StartsWith(uTagName + ":")) { return thisString.Substring(uTagName.Length + 1); }
                if (thisString.ToUpper().StartsWith(uTagName + " = ")) { return thisString.Substring(uTagName.Length + 3); }
                if (thisString.ToUpper().StartsWith(uTagName + "=")) { return thisString.Substring(uTagName.Length + 1); }
            }
        }
        return string.Empty;
    }

    public static List<string> TagGetAll(this ICollection<string>? list, string tagName) {
        List<string> l = new();
        if (list == null) { return l; }
        var uTagName = tagName.ToUpper();
        foreach (var thisString in list) {
            if (thisString.ToUpper().StartsWith(uTagName)) {
                if (thisString.ToUpper().StartsWith(uTagName + ": ")) {
                    l.Add(thisString.Substring(uTagName.Length + 2));
                } else {
                    if (thisString.ToUpper().StartsWith(uTagName + ":")) { l.Add(thisString.Substring(uTagName.Length + 1)); }
                }
                if (thisString.ToUpper().StartsWith(uTagName + " = ")) {
                    l.Add(thisString.Substring(uTagName.Length + 3));
                } else {
                    if (thisString.ToUpper().StartsWith(uTagName + "=")) { l.Add(thisString.Substring(uTagName.Length + 1)); }
                }
            }
        }
        return l;
    }

    //public static double TagGetDouble(this ICollection<string>? col, string tagname) => DoubleParse(TagGet(col, tagname));

    //public static int TagGetInt(this ICollection<string>? col, string tagname) => IntParse(TagGet(col, tagname));

    public static void TagRemove(this ICollection<string> col, string tagname) {
        var found = col.TagGetPosition(tagname);
        if (found >= 0) {
            col.Remove(col.ElementAtOrDefault(found));
        }
    }

    public static void TagSet(this ICollection<string> col, string tagname, string value) {
        var found = col.TagGetPosition(tagname);
        var n = tagname + ": " + value;

        if (found >= 0) {
            if (col.ElementAtOrDefault(found) == n) { return; }
            col.Remove(col.ElementAtOrDefault(found));
        }

        col.Add(n);
    }

    /// <summary>
    /// Führt bei allem Typen ein ToString aus und addiert diese mittels \r. Enthält ein ToString ein \r, dann wird abgebrochen.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="l"></param>
    /// <param name="removeEmpty"></param>
    /// <returns></returns>
    public static string ToString<T>(this List<T> l, bool removeEmpty) where T : IStringable? {
        // Remove Empty sollte eigentlich selbstverständlich seih. Ist nur als Dummy drinnen, dass der Interpreter zwischen der Internen und Extension unterscheiden kann.
        var tmp = string.Empty;
        foreach (var item in l) {
            var tmp2 = string.Empty;
            if (item != null) { tmp2 = item.ToString(); }
            if (tmp2.Contains("\r")) { Develop.DebugPrint(FehlerArt.Fehler, "List.Tostring hat einen Zeilenumbruch gefunden."); }
            if (!removeEmpty || !string.IsNullOrEmpty(tmp2)) {
                tmp = tmp + tmp2 + "\r";
            }
        }
        return tmp.TrimCr();
    }

    private static int TagGetPosition(this ICollection<string> col, string tagname) {
        var uTagName = tagname.ToUpper() + ":";

        for (var z = 0; z < col.Count; z++) {
            if (col.ElementAtOrDefault(z)?.Length > uTagName.Length + 1 && col.ElementAtOrDefault(z)?.Substring(0, uTagName.Length + 1).ToUpper() == uTagName + " ") {
                return z;
            }
            if (col.ElementAtOrDefault(z)?.Length > uTagName.Length && col.ElementAtOrDefault(z)?.Substring(0, uTagName.Length).ToUpper() == uTagName) {
                return z;
            }
        }

        return -1;
    }

    #endregion
}
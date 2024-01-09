﻿// Authors:
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

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using static BlueBasics.IO;
using static BlueBasics.Converter;

#nullable enable

namespace BlueBasics;

public static partial class Extensions {

    #region Methods

    public static bool AddIfNotExists<T>(this ICollection<T> l, ICollection<T>? values) {
        if (values == null || values.Count == 0) { return false; }
        var ok1 = values.Count(l.AddIfNotExists) > 0;
        return ok1;
    }

    public static bool AddIfNotExists<T>(this ICollection<T> l, T value) {
        if (l.Contains(value)) {
            return false;
        }

        l.Add(value);
        return true;
    }

    public static void AddRange<T>(this ConcurrentBag<T> @this, IEnumerable<T> toAdd) => toAdd.AsParallel().ForAll(@this.Add);

    public static List<T> Clone<T>(this ICollection<T> l) {
        var l2 = new List<T>();
        l2.AddRange(l);
        return l2;
    }

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

    public static List<string> FromNonCritical(this ICollection<string> col) {
        var l = new List<string>();
        if (col.Count == 0) { return l; }

        foreach (var thiss in col) {
            l.Add(thiss.FromNonCritical());
        }

        return l;
    }

    public static T? Get<T>(this IEnumerable<T?>? items, string name) where T : IHasKeyName {
        if (string.IsNullOrEmpty(name)) { return default; }

        if (items != null) {
            return items.FirstOrDefault(thisp =>
                thisp != null && string.Equals(thisp.KeyName, name, StringComparison.OrdinalIgnoreCase));
        }

        return default;
    }

    public static bool IsDifferentTo<T>(this IEnumerable<T>? list1, IEnumerable<T>? list2) =>
                // https://docs.microsoft.com/en-us/dotnet/api/system.linq.enumerable.sequenceequal?redirectedfrom=MSDN&view=netcore-3.1#System_Linq_Enumerable_SequenceEqual__1_System_Collections_Generic_IEnumerable___0__System_Collections_Generic_IEnumerable___0__
                list1 != list2 && (list1 is null || list2 is null || !list1.SequenceEqual(list2));

    public static void Load(this List<string> l, string filename, Encoding code) {
        var t = File.ReadAllText(filename, code);
        l.Clear();
        l.AddRange(t.SplitAndCutByCr());
    }

    public static string Parseable(this ICollection<string> col, string baseToString) {
        if (string.IsNullOrEmpty(baseToString) ||
            !baseToString.StartsWith("{") ||
            !baseToString.EndsWith("}")) {
            Develop.DebugPrint(FehlerArt.Fehler, "Basestringfehler!");
        }

        return baseToString.Substring(0, baseToString.Length - 1) + ", " + col.JoinWith(", ") + "}";
    }

    public static string Parseable(this ICollection<string> col) => "{" + col.JoinWith(", ") + "}";

    public static void ParseableAdd(this ICollection<string> col, string tagname, string? value) {
        if (value == null || string.IsNullOrEmpty(value)) { return; }
        col.Add(tagname + "=" + value.ToNonCriticalWithQuote());
    }

    public static void ParseableAdd<T>(this ICollection<string> col, string tagname, string nameofeveryItem, ICollection<T>? value) where T : IStringable? {
        if (value == null) { return; }

        if (value is IDisposableExtended d && d.IsDisposed) { return; }

        if (value is IStringable) {
            Develop.DebugPrint(FehlerArt.Fehler, "Stringable Collection nicht möglich!");
        }

        if (value.Count < 1) { return; }

        var txt = tagname + "={";

        foreach (var item in value) {
            var tmp2 = string.Empty;
            if (item != null) { tmp2 = item.ToString(); }
            if (!string.IsNullOrEmpty(tmp2)) {
                txt = txt + nameofeveryItem + "=" + tmp2.ToNonCritical() + ", ";
            }
        }

        txt = txt.TrimEnd(", ") + "}";

        col.Add(txt);
    }

    public static void ParseableAdd(this ICollection<string> col, string tagname, DateTime? value) {
        if (value == null) { return; }
        col.Add(tagname + "=" + ((DateTime)value).ToString(Constants.Format_Date5, CultureInfo.InvariantCulture));
    }

    public static void ParseableAdd(this ICollection<string> col, string tagname, DateTime? value, string format) {
        if (value == null) { return; }
        col.Add(tagname + "=" + ((DateTime)value).ToString(format, CultureInfo.InvariantCulture));
    }

    public static void ParseableAdd(this ICollection<string> col, string tagname, Color value) => col.Add(tagname + "=" + value.ToHtmlCode());

    public static void ParseableAdd(this ICollection<string> col, string tagname, Bitmap? value) {
        if (value == null) { return; }
        col.Add(tagname + "=" + BitmapToBase64(value, ImageFormat.Png));
    }

    public static void ParseableAdd(this ICollection<string> col, string tagname, SizeF value) => col.Add(tagname + "=" + value.ToString().ToNonCritical());

    public static void ParseableAdd(this ICollection<string> col, string tagname, float value) => col.Add(tagname + "=" + value.ToString(CultureInfo.InvariantCulture).Replace(",", ".").ToNonCritical());

    public static void ParseableAdd(this ICollection<string> col, string tagname, IHasKeyName? value) {
        if (value == null) { return; }

        if (value is IDisposableExtended d && d.IsDisposed) { return; }

        var v = value.KeyName;
        if (string.IsNullOrEmpty(v)) { return; }

        col.Add(tagname + "=" + v.ToNonCritical());
    }

    /// <summary>
    /// Fügt nur die Key-Namen in die Liste hinzu, getrennt mit |
    /// </summary>
    /// <param name="col"></param>
    /// <param name="tagname"></param>
    /// <param name="value"></param>
    public static void ParseableAdd(this ICollection<string> col, string tagname, IEnumerable<IHasKeyName>? value) {
        if (value == null || !value.Any()) { return; }
        ParseableAdd(col, tagname, value.ToListOfString());
    }

    /// <summary>
    /// Fügt die Einträge der Liste hinzu, getrennt mit |
    /// </summary>
    /// <param name="col"></param>
    /// <param name="tagname"></param>
    /// <param name="value"></param>
    public static void ParseableAdd(this ICollection<string> col, string tagname, ICollection<string>? value) {
        if (value == null || value.Count == 0) { return; }

        var l = new StringBuilder();

        foreach (var thisString in value) {
            _ = l.Append(thisString.ToNonCritical());
            _ = l.Append("|");
        }

        if (l.Length > 0) { l.Remove(l.Length - 1, 1); } // Letzten | abschneiden

        col.Add(tagname + "=" + l);
    }

    public static void ParseableAdd<T>(this ICollection<string> col, string tagname, T? value) where T : Enum {
        if (value == null) { return; }

        var underlyingType = Enum.GetUnderlyingType(typeof(T));

        if (underlyingType == typeof(int)) {
            col.Add(tagname + "=" + ((int)(object)value));
            return;
        }

        if (underlyingType == typeof(byte)) {
            col.Add(tagname + "=" + ((byte)(object)value));
            return;
        }

        Develop.DebugPrint(FehlerArt.Fehler, "Parseable unbekannt!");
    }

    public static void ParseableAdd(this ICollection<string> col, string tagname, IStringable? value) {
        if (value == null) { return; }

        if (value is IDisposableExtended d && d.IsDisposed) { return; }

        col.Add(tagname + "=" + value.ToString().ToNonCritical());
    }

    public static void ParseableAdd(this ICollection<string> col, string tagname, bool value) => col.Add(tagname + "=" + value.ToPlusMinus());

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
        if (l == null || l.Count == 0) { return false; }
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
        if (l == null || l.Count == 0) { return; }

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
        List<string> l = [];
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

    public static void TagRemove(this ICollection<string> col, string tagname) {
        var found = col.TagGetPosition(tagname);
        if (found >= 0) {
            _ = col.Remove(col.ElementAtOrDefault(found));
        }
    }

    //public static int TagGetInt(this ICollection<string>? col, string tagname) => IntParse(TagGet(col, tagname));
    public static void TagSet(this List<string> col, string tagname, string value) {
        var found = col.TagGetPosition(tagname);
        var n = tagname + ": " + value;

        if (found >= 0) {
            if (col.ElementAtOrDefault(found) == n) { return; }
            _ = col.Remove(col.ElementAtOrDefault(found));
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
    public static string ToString<T>(this ICollection<T> l, bool removeEmpty) where T : IStringable? {
        // Remove Empty sollte eigentlich selbstverständlich sein. Ist nur als Dummy drinnen, dass der Interpreter zwischen der Internen und Extension unterscheiden kann.
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

    public static bool WriteAllText(this ICollection<string> l, string filename, Encoding endcoding, bool executeAfter) {
        var t = l.JoinWith("\r\n").TrimEnd("\r\n");
        if (!DirectoryExists(filename.FilePath())) {
            _ = Directory.CreateDirectory(filename.FilePath());
        }
        return IO.WriteAllText(filename, t, endcoding, executeAfter);
    }

    /// <summary>
    /// Cloned eine Liste mit Clonen drinnen.
    /// </summary>
    /// <returns></returns>
    private static int TagGetPosition(this ICollection<string>? col, string tagname) {
        if (col == null) { return -1; }

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
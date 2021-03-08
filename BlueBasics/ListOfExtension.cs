#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2020 Christian Peter
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
#endregion

using BlueBasics.Enums;
using BlueBasics.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using static BlueBasics.modConverter;
using static BlueBasics.FileOperations;

namespace BlueBasics
{
    public static class ListOfExtension
    {

        /// <summary>
        /// Führt bei allem Typen ein ToString aus und addiert diese mittels \r. Enthält ein ToString ein \r, dann wird abgebrochen.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="RemoveEmpty"></param>
        /// <returns></returns>
        public static string ToString<T>(this List<T> L, bool RemoveEmpty) where T : IParseable
        {
            //Remove Empty sollte eigentlich selbstverständlich seih. Ist nur als Dummy drinnen, dass der Interpreter zwischen der Internen und Extension unterscheiden kann.

            var tmp = string.Empty;

            foreach (var Item in L)
            {

                var tmp2 = string.Empty;

                if (Item != null) { tmp2 = Item.ToString(); }

                if (tmp2.Contains("\r")) { Develop.DebugPrint(enFehlerArt.Fehler, "List.Tostring hat einen Zeilenumbruch gefunden."); }

                if (RemoveEmpty == false || string.IsNullOrEmpty(tmp2) == false)
                {
                    tmp = tmp + tmp2 + "\r";
                }
            }

            return tmp.TrimCr();
        }


        public static void SplitByCR(this List<string> List, string TextToSplit)
        {
            var l = new List<string>();
            l.AddRange(TextToSplit.SplitByCR());

            if (!List.IsDifferentTo(l)) { return; }

            if (List.Count > 0) { List.Clear(); }
            List.AddRange(l);
        }


        public static void SplitByCR_QuickSortAndRemoveDouble(this List<string> List, string TextToSplit)
        {

            var l = new List<string>();
            l.AddRange(TextToSplit.SplitByCR());
            l = l.SortedDistinctList();

            if (!List.IsDifferentTo(l)) { return; }



            if (List.Count > 0) { List.Clear(); }
            List.AddRange(l);
        }

        public static bool RemoveNullOrEmpty<T>(this List<T> L) where T : ICanBeEmpty
        {

            if (L == null || L.Count == 0) { return false; }

            var Did = false;

            var z = 0;


            while (z < L.Count)
            {

                if (L[z] == null || L[z].IsNullOrEmpty())
                {
                    L.RemoveAt(z);
                    Did = true;
                }
                else
                {
                    z++;
                }
            }

            return Did;

        }

        public static bool IsDifferentTo<T>(this List<T> List1, List<T> List2)
        {
            //https://docs.microsoft.com/en-us/dotnet/api/system.linq.enumerable.sequenceequal?redirectedfrom=MSDN&view=netcore-3.1#System_Linq_Enumerable_SequenceEqual__1_System_Collections_Generic_IEnumerable___0__System_Collections_Generic_IEnumerable___0__
            if (List1 == List2) { return false; }
            if (List1 is null || List2 is null) { return true; }
            return !List1.SequenceEqual(List2);
        }



        //public static bool IsDifferentTo<T>(this List<T> List1, List<T> List2) where T : IParseable
        //{
        //    if (List1.Count != List2.Count) { return true; }

        //    return List1.Where((t, Count) => t.ToString() != List2[Count].ToString()).Any();
        //}


        //public static bool IsDifferentTo(this List<string> List1, List<string> List2)
        //{
        //    if (List1.Count != List2.Count) { return true; }

        //    return List1.Where((t, Count) => t != List2[Count]).Any();
        //}
        //public static bool IsDifferentTo(this List<string> List1, BindingList<string> List2)
        //{
        //    if (List1.Count != List2.Count) { return true; }

        //    return List1.Where((t, Count) => t != List2[Count]).Any();
        //}





        public static void Shuffle<T>(this IList<T> list)
        {
            for (var i1 = 0; i1 < list.Count; i1++)
            {
                var i2 = Constants.GlobalRND.Next(i1, list.Count);
                if (i1 != i2)
                {
                    var v1 = list[i1];
                    var v2 = list[i2];
                    // modAllgemein.Swap(ref tempVar, ref tempVar2);
                    list[i1] = v2;
                    list[i2] = v1;
                }
            }
        }


        ///// <summary>
        ///// Falls der Dateityp String ist, WIRD zwischen Gross und Kleinschreibung unterschieden!
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="L"></param>
        ///// <param name="Value"></param>
        //public static void Remove<T>(this List<T> L, List<T> Value) where T : IComparable
        //{
        //    foreach (var Item in Value)
        //    {
        //        L.Remove(Item);
        //    }
        //}

        public static void RemoveNullOrEmpty(this List<string> L)
        {
            var z = 0;

            while (z < L.Count)
            {

                if (string.IsNullOrEmpty(L[z]))
                {
                    L.RemoveAt(z);
                }
                else
                {
                    z++;
                }
            }


        }

        public static bool RemoveNull<T>(this List<T> L)
        {

            if (L == null || L.Count == 0) { return false; }

            var Did = false;

            var z = 0;

            while (z < L.Count)
            {
                if (L[z] == null || L[z].Equals(default(T)))
                {
                    L.RemoveAt(z);
                    Did = true;
                }
                else
                {
                    z++;
                }
            }

            return Did;

        }


        public static void Load(this List<string> L, string Filename)
        {
            var t = LoadFromDisk(Filename);

            L.Clear();
            L.AddRange(t.SplitByCR());

        }


        public static void Save(this List<string> L, string DateiName, bool ExecuteAfter)
        {
            var t = L.JoinWith("\r\n").TrimEnd("\r\n");

            if (!FileOperations.PathExists(DateiName.FilePath()))
            {
                System.IO.Directory.CreateDirectory(DateiName.FilePath());
            }

            SaveToDisk(DateiName, t, ExecuteAfter);
        }


        /// <summary>
        ///  Falls der Dateityp String ist, WIRD zwischen Gross und Kleinschreibung unterschieden! Dafür kann RemoveString benutzt werden.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="L"></param>
        /// <param name="Value"></param>
        public static void Remove<T>(this List<T> L, T Value) where T : IComparable
        {
            do { } while (L.Remove(Value));
        }


        public static void RemoveString(this List<string> L, string Value, bool CaseSensitive)
        {

            if (CaseSensitive) { Develop.DebugPrint(enFehlerArt.Fehler, "CaseSensitive = True"); }

            var z = 0;

            while (z < L.Count)
            {

                if (L[z].ToUpper() == Value.ToUpper())
                {
                    L.RemoveAt(z);
                }
                else
                {
                    z++;
                }

            }

        }


        public static void RemoveString(this List<string> L, List<string> Value, bool CaseSensitive)
        {
            foreach (var t in Value)
            {
                L.RemoveString(t, CaseSensitive);
            }
        }


        public static void RemoveString(this List<string> L, string[] Value, bool CaseSensitive)
        {

            for (var z = 0; z <= Value.GetUpperBound(0); z++)
            {
                L.RemoveString(Value[z], CaseSensitive);
            }

        }



        public static List<string> TagGetAll(this ICollection<string> _String, string TagName)
        {
            var l = new List<string>();

            if (_String == null) { return l; }

            var uTagName = TagName.ToUpper();

            foreach (var ThisString in _String)
            {
                if (ThisString.ToUpper().StartsWith(uTagName))
                {
                    if (ThisString.ToUpper().StartsWith(uTagName + ": "))
                    {
                        l.Add(ThisString.Substring(uTagName.Length + 2));
                    }
                    else
                    {
                        if (ThisString.ToUpper().StartsWith(uTagName + ":")) { l.Add(ThisString.Substring(uTagName.Length + 1)); }
                    }

                    if (ThisString.ToUpper().StartsWith(uTagName + " = "))
                    {
                        l.Add(ThisString.Substring(uTagName.Length + 3));
                    }
                    else
                    {
                        if (ThisString.ToUpper().StartsWith(uTagName + "=")) { l.Add(ThisString.Substring(uTagName.Length + 1)); }
                    }
                }
            }
            return l;
        }

        public static int TagGetInt(this ICollection<string> _String, string TagName)
        {

            return IntParse(TagGet(_String, TagName));
        }

        public static decimal TagGetDecimal(this ICollection<string> _String, string TagName)
        {

            return DecimalParse(TagGet(_String, TagName));
        }

        public static double TagGetDouble(this ICollection<string> _String, string TagName)
        {

            return DoubleParse(TagGet(_String, TagName));
        }


        public static string TagGet(this ICollection<string> _String, string TagName)
        {
            if (_String == null) { return string.Empty; }

            var uTagName = TagName.ToUpper().Trim();

            foreach (var ThisString in _String)
            {
                if (ThisString.ToUpper().StartsWith(uTagName))
                {
                    if (ThisString.ToUpper().StartsWith(uTagName + ": ")) { return ThisString.Substring(uTagName.Length + 2); }
                    if (ThisString.ToUpper().StartsWith(uTagName + ":")) { return ThisString.Substring(uTagName.Length + 1); }
                    if (ThisString.ToUpper().StartsWith(uTagName + " = ")) { return ThisString.Substring(uTagName.Length + 3); }
                    if (ThisString.ToUpper().StartsWith(uTagName + "=")) { return ThisString.Substring(uTagName.Length + 1); }
                }
            }
            return string.Empty;
        }


        public static void TagSet(this ICollection<string> _String, string TagNamex, string Value)
        {

            var uTagName = TagNamex.ToUpper() + ":";
            var Found = -1;

            for (var z = 0; z < _String.Count; z++)
            {
                if (_String.ElementAtOrDefault(z).Length > uTagName.Length + 1 && _String.ElementAtOrDefault(z).Substring(0, uTagName.Length + 1).ToUpper() == uTagName + " ")
                {
                    Found = z;
                    break;
                }
                if (_String.ElementAtOrDefault(z).Length > uTagName.Length && _String.ElementAtOrDefault(z).Substring(0, uTagName.Length).ToUpper() == uTagName)
                {
                    Found = z;
                    break;
                }
            }

            var n = TagNamex + ": " + Value;

            if (Found >= 0)
            {
                if (_String.ElementAtOrDefault(Found) == n)
                {
                    return;
                }
                _String.Remove(_String.ElementAtOrDefault(Found));
            }


            _String.Add(n);
        }




        public static bool AddIfNotExists<T>(this List<T> L, BindingList<T> Values)
        {
            if (Values == null || Values.Count == 0) { return false; }

            var ok1 = false;

            foreach (var thivalue in Values)
            {
                if (L.AddIfNotExists(thivalue))
                {
                    ok1 = true;
                }
            }
            return ok1;
        }


        public static bool AddIfNotExists<T>(this List<T> L, List<T> Values)
        {
            if (Values == null || Values.Count == 0) { return false; }

            var ok1 = false;

            foreach (var thivalue in Values)
            {
                if (L.AddIfNotExists(thivalue))
                {
                    ok1 = true;
                }
            }
            return ok1;
        }

        public static bool AddIfNotExists<T>(this List<T> L, T Value)
        {

            if (!L.Contains(Value))
            {
                L.Add(Value);
                return true;
            }

            return false;

        }


    }
}
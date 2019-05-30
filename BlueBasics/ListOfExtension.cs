﻿#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2019 Christian Peter
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
                    z += 1;
                }
            }

            return Did;

        }


        public static bool IsDifferentTo<T>(this List<T> List1, List<T> List2) where T : IParseable
        {
            if (List1.Count != List2.Count) { return true; }

            return List1.Where((t, Count) => t.ToString() != List2[Count].ToString()).Any();
        }


        public static bool IsDifferentTo(this List<string> List1, List<string> List2)
        {
            if (List1.Count != List2.Count) { return true; }

            return List1.Where((t, Count) => t != List2[Count]).Any();
        }
        public static bool IsDifferentTo(this List<string> List1, BindingList<string> List2)
        {
            if (List1.Count != List2.Count) { return true; }

            return List1.Where((t, Count) => t != List2[Count]).Any();
        }





        public static void Shuffle<T>(this IList<T> list)
        {
            for (var i = 0 ; i < list.Count ; i++)
            {
                var index = Constants.GlobalRND.Next(i, list.Count);
                if (i != index)
                {
                    var tempVar = list[i];
                    var tempVar2 = list[index];
                    modAllgemein.Swap(ref tempVar, ref tempVar2);
                    list[index] = tempVar2;
                    list[i] = tempVar;
                }
            }
        }


        /// <summary>
        /// Falls der Dateityp String ist, WIRD zwischen Gross und Kleinschreibung unterschieden!
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="L"></param>
        /// <param name="Value"></param>
        public static void Remove<T>(this List<T> L, List<T> Value) where T : IComparable
        {
            foreach (var Item in Value)
            {
                L.Remove(Item);
            }
        }


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
                    z += 1;
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
                    z += 1;
                }
            }

            return Did;

        }


        public static void Load(this List<string> L, string Filename)
        {
            var t = modAllgemein.LoadFromDisk(Filename);

            L.Clear();
            L.AddRange(t.SplitByCR());

        }


        public static void Save(this List<string> L, string DateiName, bool ExecuteAfter)
        {
            var t = L.JoinWith("\r\n").TrimEnd("\r\n");
            modAllgemein.SaveToDiskx(DateiName, t, ExecuteAfter);
        }


        /// <summary>
        ///  Falls der Dateityp String ist, WIRD zwischen Gross und Kleinschreibung unterschieden!
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
                    z += 1;
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

            for (var z = 0 ; z <= Value.GetUpperBound(0) ; z++)
            {
                L.RemoveString(Value[z], CaseSensitive);
            }

        }



        public static string TagGet(this ICollection<string> _String, string TagName)
        {


            if (_String == null) { return string.Empty; }

            var uTagName = TagName.ToUpper();

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

            for (var z = 0 ; z < _String.Count ; z++)
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
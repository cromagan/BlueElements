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

using System.Collections.Concurrent;
using System.Collections.Generic;
using BlueBasics.Enums;
using BlueBasics.Interfaces;

namespace BlueBasics
{
    public static partial class Extensions
    {

        public static bool RemoveNullOrEmpty<T>(this ConcurrentDictionary<int, T> L) where T : ICanBeEmpty
        {

            if (L == null || L.Count == 0) { return false; }

            var remo = new List<int>();

            foreach (var pair in L)
            {
                if (pair.Value == null || pair.Value.IsNullOrEmpty()) { remo.Add(pair.Key); }
            }

            if (remo.Count == 0) { return false; }
            var dummy = default(T);

            foreach (var ThisInteger in remo)
            {
                if (!L.TryRemove(ThisInteger, out dummy))
                {
                    Develop.DebugPrint(enFehlerArt.Fehler, "Remove failed: " + ThisInteger);
                }
            }

            return true;

        }


    }
}
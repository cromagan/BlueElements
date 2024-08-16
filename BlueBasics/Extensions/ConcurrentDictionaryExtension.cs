// Authors:
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

#nullable enable

using BlueBasics.Enums;
using BlueBasics.Interfaces;
using System.Collections.Concurrent;
using System.Linq;

namespace BlueBasics;

public static partial class Extensions {

    #region Methods

    public static bool RemoveNullOrEmpty<T>(this ConcurrentDictionary<string, T>? l) where T : ICanBeEmpty? {
        if (l is not { Count: not 0 }) { return false; }
        var remo = (from pair in l where pair.Value == null || pair.Value.IsNullOrEmpty() select pair.Key).ToList();
        if (remo.Count == 0) { return false; }

        foreach (var thisInteger in remo) {
            if (!l.TryRemove(thisInteger, out _)) {
                Develop.DebugPrint(FehlerArt.Fehler, "Remove failed: " + thisInteger);
            }
        }

        return true;
    }

    #endregion
}
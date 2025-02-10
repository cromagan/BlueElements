// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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

using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using BlueBasics.Interfaces;

namespace BlueBasics;

public static partial class Extensions {

    #region Methods

    public static bool RemoveNullOrEmpty<T>(this ConcurrentDictionary<string, T>? l) where T : ICanBeEmpty? {
        if (l is not { Count: not 0 }) { return false; }

        var snapshot = l.ToArray();
        var remo = snapshot
            .Where(pair => pair.Value == null || pair.Value.IsNullOrEmpty())
            .Select(pair => pair.Key)
            .ToList();

        if (remo.Count == 0) { return false; }

        var removedAny = false;

        foreach (var key in remo) {
            while (l.ContainsKey(key)) {
                if (l.TryRemove(key, out _)) {
                    removedAny = true;
                    break;
                }
                Thread.Sleep(1);
            }
        }

        return removedAny;
    }

    #endregion
}
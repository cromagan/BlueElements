// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Interfaces;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace BlueBasics;

public static partial class Extensions {

    #region Methods

    public static bool RemoveNullOrEmpty<T>(this ConcurrentDictionary<string, T>? l) where T : ICanBeEmpty? {
        //TODO: Unused
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
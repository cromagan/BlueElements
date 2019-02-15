using System.Collections.Concurrent;
using System.Collections.Generic;
using BlueBasics.Enums;
using BlueBasics.Interfaces;

namespace BlueBasics
{
    public static partial class Extensions
    {

        public static bool RemoveNullOrEmpty<t>(this ConcurrentDictionary<int, t> L) where t : ICanBeEmpty
        {

            if (L == null || L.Count == 0) { return false; }

            var remo = new List<int>();

            foreach (var pair in L)
            {
                if (pair.Value == null || pair.Value.IsNullOrEmpty()) { remo.Add(pair.Key); }
            }

            if (remo.Count == 0) { return false; }
            var dummy = default(t);

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
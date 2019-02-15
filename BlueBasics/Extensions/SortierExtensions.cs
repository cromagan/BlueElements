using System.Collections.Generic;
using System.Linq;

namespace BlueBasics
{
    public static partial class Extensions
    {
        public static List<string> SortedDistinctList(this List<string> arr)
        {
            var arr2 = arr.Distinct().ToList();
            if (arr2.Contains(string.Empty)) { arr2.Remove(string.Empty); }
            arr2.Sort();
            return arr2;

        }
    }
}
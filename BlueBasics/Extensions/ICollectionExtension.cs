using BlueBasics.Enums;
using System.Collections.Generic;
using System.Linq;

namespace BlueBasics
{
    public static partial class Extensions
    {



        public static bool Contains(this ICollection<string> IC, string SearchKeyword, bool CaseSensitive)
        {
            if (CaseSensitive) { Develop.DebugPrint(enFehlerArt.Fehler, "CaseSensitive = True"); }

            return IC.Any(Item => Item.ToUpper() == SearchKeyword.ToUpper());
        }

        public static string JoinWithCr(this ICollection<string> IC)
        {
            if (IC == null || IC.Count == 0) { return string.Empty; }

            return IC.JoinWith("\r");
        }


        public static string JoinWith(this ICollection<string> IC, string JoinChar)
        {
            return string.Join(JoinChar, IC.ToArray()).TrimEnd(JoinChar);
        }



    }
}
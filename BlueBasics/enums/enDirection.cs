using System;

namespace BlueBasics.Enums
{
    [Flags]
    public enum enDirection : byte
    {
        Nichts = 0,
        Oben = 1,
        Unten = 2,
        Links = 4,
        Rechts = 8,

        Oben_Links = Oben | Links,
        Oben_Rechts = Oben | Rechts,
        Unten_Links = Unten | Links,
        Unten_Rechts = Unten | Rechts
    }
}
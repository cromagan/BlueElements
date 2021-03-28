using System;

namespace BlueControls.Enums
{
    [Flags]
    public enum enAutoRelationMode
    {
        None = 0,
        DirektVerbindungen = 1,
        Waagerecht = 2,
        Senkrecht = 4,
        NurBeziehungenErhalten = 8,
        WaagerechtSenkrecht = Waagerecht + Senkrecht,

        DirektVerbindungen_Erhalten = DirektVerbindungen | NurBeziehungenErhalten,
        //WaagerechtSenkrecht_Erhalten = WaagerechtSenkrecht | NurBeziehungenErhalten,


        Alle = WaagerechtSenkrecht | DirektVerbindungen,

        Alle_Erhalten = Alle + NurBeziehungenErhalten
    }
}
using System;

namespace BlueControls.Enums
{
    [Flags]
    public enum enSteuerelementVerhalten
    {


        Text_Abschneiden = 0,
        Steuerelement_Anpassen = 1,
        Scrollen_ohne_Textumbruch = 2,
        Scrollen_mit_Textumbruch = 4


        // Scrollen_mit_Textumbruch = Scrollen_mit_Textumbruch
        // Scrollen = Scrollen_ohne_Textumbruch
    }
}
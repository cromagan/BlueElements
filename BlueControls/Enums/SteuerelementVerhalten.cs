// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System;

namespace BlueControls.Enums;

[Flags]
public enum SteuerelementVerhalten {
    Text_Abschneiden = 0,
    Steuerelement_Anpassen = 1,
    Scrollen_ohne_Textumbruch = 2,
    Scrollen_mit_Textumbruch = 4
    // Scrollen_mit_Textumbruch = Scrollen_mit_Textumbruch
    // Scrollen = Scrollen_ohne_Textumbruch
}
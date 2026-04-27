// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System;

namespace BlueTable.Enums;

[Flags]
public enum ButtonArgs {
    Nie = 0,
    Egal = 1,
    Keine_Zeile = 2,
    Genau_eine_Zeile = 4,
    Eine_oder_mehr_Zeilen = 8
}
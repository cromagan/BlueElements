// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueTable.Enums;

[Flags]
public enum ButtonArgs {
    Nie = 0,
    Egal = 1,
    Keine_Zeile = 2,
    Genau_eine_Zeile = 4,
    Eine_oder_mehr_Zeilen = 8
}
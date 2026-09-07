// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueBasics.Enums;

[Flags]
public enum Direction : byte {
    None = 0,
    Oben = 1,
    Unten = 2,
    Links = 4,
    Rechts = 8
}
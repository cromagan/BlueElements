// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueBasics.Enums;

[Flags]
public enum Direction : byte {
    None = 0,
    Oben = 1,
    Unten = 2,
    Links = 4,
    Rechts = 8
}
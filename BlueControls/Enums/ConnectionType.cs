// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueControls.Enums;

[Flags]
public enum ConnectionType {
    Auto = 0,

    Top = 1,
    Bottom = 2,
    Left = 4,
    Right = 8
}
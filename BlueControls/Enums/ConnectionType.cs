// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.Enums;

[Flags]
public enum ConnectionType {
    Auto = 0,

    Top = 1,
    Bottom = 2,
    Left = 4,
    Right = 8
}
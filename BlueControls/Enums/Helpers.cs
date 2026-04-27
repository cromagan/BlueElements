// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System;

namespace BlueControls.Enums;

[Flags]
public enum Helpers {
    // Sollten die Rountnen benötigt werden, nach
    // ZoomPic.DrawHelpers
    // verschieben

    None = 0,
    SmallCircle_unused = 1 << 0,   // 1
    SymetricalHorizontal = 1 << 1,   // 2
    SymetricalVertical_unused = 1 << 2,   // 4
    MouseDownPoint = 1 << 3,   // 8
    HorizontalLine = 1 << 4,   // 16
    VerticalLine = 1 << 5,   // 32
    DrawToPoint_unused = 1 << 6,   // 64
    FilledRectancle = 1 << 7,   // 128
    PointNames = 1 << 8,   // 256
    Magnifier = 1 << 9,   // 512
    DrawRectangle = 1 << 10,  // 1024
    Draw20x10 = 1 << 11,  // 2048
}
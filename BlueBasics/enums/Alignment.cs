// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Windows.Forms;

namespace BlueBasics.Enums;

[Flags]
public enum Alignment {

    // Undefiniert = CByte(TextFormatFlags.none)
    Left = TextFormatFlags.Left,

    Right = TextFormatFlags.Right,
    HorizontalCenter = TextFormatFlags.HorizontalCenter,
    Top = TextFormatFlags.Top,
    Bottom = TextFormatFlags.Bottom,
    VerticalCenter = TextFormatFlags.VerticalCenter,

    // DehnenLR = 64
    // DehnenOU = 128
    // DehnenLROU = DehnenLR Or DehnenOU
    Top_Left = Left | Top,

    Top_HorizontalCenter = HorizontalCenter | Top,
    Top_Right = Right | Top,
    Bottom_Left = Left | Bottom,
    Bottom_HorizontalCenter = HorizontalCenter | Bottom,
    Bottom_Right = Right | Bottom,
    VerticalCenter_Left = Left | VerticalCenter,
    Horizontal_Vertical_Center = HorizontalCenter | VerticalCenter,
    VerticalCenter_Right = Right | VerticalCenter
}
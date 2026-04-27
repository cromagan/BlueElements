// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueTable.Enums;

[Flags]
public enum ColumnBackgroundStyle : long {
    None = 0,
    Brighten = 1L << 0,
    Darken = 1L << 1,
    PopIn = 1L << 2,
    PopOut = 1L << 3,
    //// ... bis ...
    //Flag64 = 1L << 63
}
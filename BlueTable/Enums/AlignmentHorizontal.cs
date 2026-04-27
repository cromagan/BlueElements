// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System;
using System.Windows.Forms;

namespace BlueTable.Enums;

[Flags]
public enum AlignmentHorizontal {

    //Keine_Präferenz = -1,
    Links = TextFormatFlags.Left,

    Rechts = TextFormatFlags.Right,
    Zentriert = TextFormatFlags.HorizontalCenter
}
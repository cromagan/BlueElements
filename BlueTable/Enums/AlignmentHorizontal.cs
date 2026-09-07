// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Windows.Forms;

namespace BlueTable.Enums;

[Flags]
public enum AlignmentHorizontal {

    //Keine_Präferenz = -1,
    Links = TextFormatFlags.Left,

    Rechts = TextFormatFlags.Right,
    Zentriert = TextFormatFlags.HorizontalCenter
}
// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueTable.Enums;

/// <summary>
/// In welchem Datenformat die Daten gelesen werden.
/// </summary>
public enum Routinen : byte {
    //[Obsolete("Wird zukünftig entfernt werden", false)]
    //Column = 0,

    //[Obsolete("Wird zukünftig entfernt werden", false)]
    //DatenAllgemein = 1,

    //[Obsolete("Wird zukünftig entfernt werden", false)]
    //CellFormat = 2,

    DatenAllgemeinUTF8 = 3,

    //[Obsolete("Wird zukünftig entfernt werden", false)]
    //CellFormatUTF8 = 4,

    //[Obsolete("Wird zukünftig entfernt werden", false)]
    //ColumnUTF8 = 5,

    //[Obsolete("Wird zukünftig entfernt werden", false)]
    //CellFormatUTF8_V400 = 6,

    //ColumnUTF8_V400 = 7,

    [Obsolete("Wird zukünftig entfernt werden", false)]
    CellFormatUTF8_V401 = 8,

    ColumnUTF8_V401 = 9,

    [Obsolete("Wird zukünftig entfernt werden", false)]
    CellFormatUTF8_V402 = 10, // Ohne ColumKey, mit RowKey

    CellFormatUTF8_V403 = 11 // Mit ColumKey und RowKey
}
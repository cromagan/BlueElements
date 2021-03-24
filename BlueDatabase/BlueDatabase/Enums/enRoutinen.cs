
using System;

namespace BlueDatabase.Enums {

    /// <summary>
    /// In welchem Datenformat die Daten gelesen werden.
    /// </summary>
    public enum enRoutinen : byte {
        [Obsolete]
        Column = 0,
        [Obsolete]
        DatenAllgemein = 1,
        [Obsolete]
        CellFormat = 2,
        DatenAllgemeinUTF8 = 3,
        CellFormatUTF8 = 4,
        ColumnUTF8 =5
    }

}

namespace BlueDatabase.Enums {

    /// <summary>
    /// In welchem Datenformat die Daten gelesen werden.
    /// </summary>
    public enum enRoutinen : byte {
        Column = 0,
        DatenAllgemein = 1,
        CellFormat = 2,
        DatenAllgemeinUTF8 = 3,
        CellFormatUTF8 = 4,
        ColumnUTF8 =5
    }

}
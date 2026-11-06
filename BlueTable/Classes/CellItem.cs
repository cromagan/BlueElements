// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueTable.Classes;

/// <summary>
/// Diese Klasse enthält nur das Aussehen und gibt keinerlei Events ab.
/// </summary>
public class CellItem {

    #region Constructors

    public CellItem(string value) => Value = value;

    public CellItem() { }

    #endregion

    #region Properties

    public string Value { get; set; } = string.Empty;

    #endregion
}

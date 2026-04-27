// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueTable.Classes;

/// <summary>
/// Diese Klasse enthält nur das Aussehen und gibt keinerlei Events ab.
/// </summary>
public class CellItem {

    #region Constructors

    public CellItem(string value) => Value = value;

    #endregion

    #region Properties

    //public Color BackColor { get; set; }
    //public Color FontColor { get; set; }
    //public bool Editable { get; set; }
    //public byte Symbol { get; set; }
    public string Value { get; set; }

    #endregion
}
// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.Enums;

public enum GroupBoxStyle {

    /// <summary>
    /// Klassischer Rahmen, Beschriftung oben links.
    /// </summary>
    Normal = 1,

    /// <summary>
    /// Flacher Rahmen für Ribbon-Leisten, Beschriftung unten mittig.
    /// </summary>
    RibbonBar = 2,

    /// <summary>
    /// Kein Rahmen, transparenter Hintergrund.
    /// </summary>
    Nothing = 3,

    /// <summary>
    /// Klassischer Rahmen mit kräftigem Rahmen und fetter Beschriftung.
    /// </summary>
    NormalBold = 4,

    /// <summary>
    /// Abgerundetes Rechteck mit dünnem Rahmen.
    /// </summary>
    RoundRect = 5,

    /// <summary>
    /// Rahmen im Stil eines geöffneten Dropdown-Menüs (Form_SelectBox_Dropdown).
    /// </summary>
    DropdownMenu = 6
}

// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.Enums;

/// <summary>
/// Bestimmt das Verhalten des Dropdown-Bereichs einer <see cref="Controls.ComboBox"/>.
/// Entspricht <see cref="System.Windows.Forms.ComboBoxStyle"/>, erweitert um
/// <see cref="ClickableMenu"/> für reine Aktionsmenüs ohne Auswahlzustand.
/// </summary>
public enum DropDownMode {

    /// <summary>
    /// Der Textbereich ist editierbar. Der Benutzer kann einen beliebigen Wert
    /// eingeben oder einen Eintrag aus der Liste auswählen.
    /// </summary>
    DropDown = 0,

    /// <summary>
    /// Der Textbereich ist nicht editierbar. Der Benutzer kann nur einen
    /// Eintrag aus der Liste auswählen.
    /// </summary>
    DropDownList = 1,

    /// <summary>
    /// Die Dropdown-Liste verhält sich wie ein reines Aktionsmenü: Die Elemente
    /// sind klickbar, werden aber nie als ausgewählt markiert. In der Listbox
    /// wird <see cref="CheckBehavior.NoSelection"/> verwendet.
    /// </summary>
    ClickableMenu = 2
}

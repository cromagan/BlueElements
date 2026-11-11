// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueControls.Enums;

public enum CheckBehavior {

    /// <summary>
    /// Alles Einträge, die angezeigt werden, zählen als ausgewählt.
    /// </summary>
    AllSelected = 0,

    /// <summary>
    /// Erlaubt das Auswählen eines oder keines Eintrages. Sozusagen Cancel erlaubt.
    /// </summary>
    SingleSelection = 1,

    /// <summary>
    /// Erlaubt das Auswählen einer beliebigen Anzahl an Einträgen. Sozusagen Cancel erlaubt.
    /// </summary>
    MultiSelection = 2,

    /// <summary>
    /// Es muss genau ein Eintrag gewählt sein. Der Benutzer kann diesen nicht abwählen, systemintern ist das erlaubt.
    /// </summary>
    UserLockedSingleSelection = 3,

    /// <summary>
    /// Es können keine Einträge gewählt werden. Es können nur Werte angeklickt werden.
    /// </summary>
    NoSelection = 4
}
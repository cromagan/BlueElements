// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System;

namespace BlueControls.Enums;

[Flags]
public enum MarkState {
    None = 0,

    /// <summary>
    /// Bei Rechtschreibfehlern
    /// </summary>
    Ringelchen = 1,

    /// <summary>
    /// Felder im Creativepad
    /// </summary>
    Field = 2,

    /// <summary>
    /// Verknüpfungen, der eigene Name
    /// </summary>
    MyOwn = 4,

    /// <summary>
    /// Verknüpfungen, ein erkannter Link
    /// </summary>
    Other = 8,

    /// <summary>
    /// Zellverknüpfung (CellLink)
    /// </summary>
    CellLink = 16
}
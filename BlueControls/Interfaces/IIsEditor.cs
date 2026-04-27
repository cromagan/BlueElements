// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.Interfaces;

/// <summary>
/// Definiet das Objekt als Editor für eine Klasse des Typs IEditable, das in EditorFor definiert wird.
/// Benötigt einen parameterlosen Konstruktor.
/// Für ein Grundgerüs kann EditorEasy verwendet werden - Objekte dieses Types können als UserControl verwendet werden,
/// oder automatisch eine Form geöffnet werden.
/// InputBoxEditorExtension.Edit kann dabei benutzt werden.
/// </summary>
public interface IIsEditor {

    #region Properties

    /// <summary>
    /// Gibt den Typ des IEditable an, den dieser Editor bearbeiten kann.
    /// </summary>
    Type? EditorFor { get; }

    IEditable? ToEdit { set; }

    #endregion
}
// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.Interfaces;

/// <summary>
/// Definiert das Objekt als Editor für eine Klasse, deren Typ in EditorFor definiert wird.
/// Benötigt einen parameterlosen Konstruktor.
/// Für ein Grundgerüst kann EditorEasy verwendet werden - Objekte dieses Types können als UserControl verwendet werden,
/// oder automatisch eine Form geöffnet werden.
/// InputBoxEditorExtension.Edit kann dabei benutzt werden.
/// Das zu bearbeitende Objekt sollte idealerweise IEditable implementieren (für IsNowEditable-Prüfung und CaptionForEditor),
/// ist aber nicht zwingend erforderlich.
/// </summary>
public interface IIsEditor {

    #region Fields

    public static readonly AssemblyAwareCache<IIsEditor> AllEditors = new();

    #endregion

    #region Properties

    /// <summary>
    /// Gibt den Typ des zu bearbeitenden Objekts an. Muss nicht zwingend IEditable implementieren.
    /// </summary>
    Type? EditorFor { get; }

    object? ToEdit { set; }

    #endregion
}
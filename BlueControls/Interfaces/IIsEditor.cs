// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Editoren;

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

    object? InputItem { get; set; }

    EditorMode Mode { get; set; }

    /// <summary>
    /// Gibt an, ob das InputItem bereits in die Editor-Oberfläche geladen wurde.
    /// EditorEasy lädt das Item verzögert (erst beim Sichtbarwerden). Solange das
    /// aussteht, kann OutputItem nichts Neues erzeugen und gibt das Original zurück.
    /// Direkte Implementierungen arbeiten live und geben standardmäßig true zurück.
    /// </summary>
    bool IsInputItemLoaded => true;

    public object? OutputItem {
        get {
            switch (Mode) {
                case EditorMode.OnlyShow:
                    return null;

                case EditorMode.EditItem:
                    return InputItem;

                default:
                    if (!IsInputItemLoaded) { return InputItem; }
                    return CreateNewItem();
            }
        }
    }

    EditorMode SupportedModes { get; }

    #endregion

    #region Methods

    object? CreateNewItem();

    #endregion
}
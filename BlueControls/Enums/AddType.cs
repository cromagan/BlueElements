// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.Enums;

[Flags]
public enum AddType {

    /// <summary>
    /// Hinzu-Button wird nicht angezeigt.
    /// </summary>
    None = 0,

    /// <summary>
    /// Hinzu-Button wird mit Texteingabefeld angezeigt.
    /// Der Text wird beim Klick an Controls.ListBox.AddClicked übergeben.
    /// </summary>
    Text = 1,

    /// <summary>
    /// Hinzu-Button wird angezeigt und öffnet ein Dropdown mit Vorschlägen.
    /// Ist Text ebenfalls gesetzt, wird eine ComboBox verwendet.
    /// </summary>
    Suggestions = 2,

    /// <summary>
    /// Kombination aus Text und Suggestions.
    /// </summary>
    TextAndSuggestions = Text | Suggestions
}
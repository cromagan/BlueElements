// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueBasics.Interfaces;

/// <summary>
/// Pendant zu <see cref="IStringable" /> für die neuen JSON-basierten Serialisierungs-Routinen.
/// Die Implementierung liefert ein veränderbares <see cref="JsonObject" />, das den kompletten
/// Zustand des Objekts enthält. Sub-Objekte (die ihrerseits <see cref="IJsonStringable" /> oder
/// <see cref="IJsonParseable" /> implementieren) werden als verschachtelte JSON-Strukturen
/// eingebettet - kein String-Encoding via ToNonCritical/FromNonCritical mehr nötig.
/// </summary>
/// <remarks>
/// Gewollt ist <see cref="JsonObject" /> und nicht <see cref="System.Text.Json.JsonElement" />,
/// weil <see cref="JsonObject" /> mutable ist: Subklassen können das Ergebnis von
/// <c>base.ParseableJson()</c> direkt erweitern, und Einbetten als Sub-Objekt funktioniert ohne
/// Konvertierung, da <see cref="JsonObject" /> selbst ein <see cref="JsonNode" /> ist.
/// </remarks>
public interface IJsonStringable {

    #region Methods

    /// <summary>
    /// Serialisiert den aktuellen Zustand in ein neues, veränderbares
    /// <see cref="JsonObject" />. Jeder Aufruf liefert eine frische Instanz,
    /// sodass der Aufrufer sie ohne Side-Effects mutieren darf.
    /// </summary>
    JsonObject ParseableJson();

    #endregion
}
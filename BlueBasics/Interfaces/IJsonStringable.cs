// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueBasics.Interfaces;

/// <summary>
/// Pendant zu IStringable für die neuen JSON-basierten Serialisierungs-Routinen.
/// Die Implementierung liefert ein veränderbares JsonObject, das den kompletten
/// Zustand des Objekts enthält. Sub-Objekte (die ihrerseits IJsonStringable oder
/// IJsonParseable implementieren) werden als verschachtelte JSON-Strukturen
/// eingebettet - kein String-Encoding via ToNonCritical/FromNonCritical mehr nötig.
/// </summary>
/// <remarks>
/// Gewollt ist JsonObject und nicht JsonElement,
/// weil JsonObject mutable ist: Subklassen können das Ergebnis von
/// <c>base.ParseableJson()</c> direkt erweitern, und Einbetten als Sub-Objekt funktioniert ohne
/// Konvertierung, da JsonObject selbst ein JsonNode ist.
/// </remarks>
public interface IJsonStringable {

    #region Methods

    /// <summary>
    /// Serialisiert den aktuellen Zustand in ein neues, veränderbares
    /// JsonObject. Jeder Aufruf liefert eine frische Instanz,
    /// sodass der Aufrufer sie ohne Side-Effects mutieren darf.
    /// </summary>
    JsonObject ParseableJson();

    #endregion
}
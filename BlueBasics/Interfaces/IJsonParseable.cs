// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using static BlueBasics.ClassesStatic.Develop;

namespace BlueBasics.Interfaces;

/// <summary>
/// Statische Erweiterungsmethoden für <see cref="IJsonParseable" />.
/// Pendant zur <see cref="ParseableExtension" /> für die klassische
/// String-basierte Serialisierung.
/// </summary>
public static class JsonParseableExtension {

    #region Methods

    /// <summary>
    /// Deserialisiert das übergebene <see cref="JsonElement" /> in das Zielobjekt,
    /// indem für jede Property <see cref="IJsonParseable.ParseThisJson" /> aufgerufen wird.
    /// Der Key wird dabei - wie beim klassischen Parser - auf Kleinschreibung normalisiert.
    /// Am Ende wird <see cref="IJsonParseable.ParseFinishedJson" /> aufgerufen.
    /// </summary>
    /// <returns><c>true</c>, wenn der Parse-Vorgang gestartet wurde (auch bei einzelnen unbekannten Keys).
    /// <c>false</c>, wenn das Element kein JSON-Objekt ist.</returns>
    public static bool ParseJson(this IJsonParseable parsable, JsonElement toParse) {
        if (toParse.ValueKind != JsonValueKind.Object) { return false; }

        foreach (var pair in toParse.EnumerateObject()) {
            var accepted = parsable.ParseThisJson(pair.Name.ToLowerInvariant(), pair.Value);

            if (!accepted) {
                DebugPrint(ErrorType.Info, $"Nicht geparster Key: '{pair.Name}' in {toParse.GetRawText()[..Math.Min(toParse.GetRawText().Length, 200)]}");
            }
        }

        parsable.ParseFinishedJson(toParse);
        return true;
    }

    #endregion
}

/// <summary>
/// Pendant zu <see cref="IParseable" /> für die neuen JSON-basierten Serialisierungs-Routinen.
/// Wird parallel zu <see cref="IParseable" /> implementiert - beide Schnittstellen können
/// koexistieren, ohne sich gegenseitig zu beeinflussen. Die klassischen Routinen
/// (<see cref="IParseable.ParseThis" /> usw.) bleiben unangetastet.
/// </summary>
public interface IJsonParseable : IJsonStringable {

    #region Methods

    /// <summary>
    /// Wird nach dem Parsen aller Keys aufgerufen und ermöglicht
    /// abschließende Validierungen und Reparaturen. Pendant zu
    /// <see cref="IParseable.ParseFinished" />.
    /// </summary>
    /// <param name="parsed">Das komplette JSON-Element, das ursprünglich übergeben wurde.</param>
    void ParseFinishedJson(JsonElement parsed);

    /// <summary>
    /// Verarbeitet ein einzelnes Key-Value-Paar. Pendant zu
    /// <see cref="IParseable.ParseThis" />.
    /// </summary>
    /// <param name="key">Der Key in Kleinschreibung konvertiert.</param>
    /// <param name="value">Der Wert als <see cref="JsonElement" />. Der ValueKind
    /// gibt Aufschluss über den Typ (String, Number, Object, Array, ...).</param>
    /// <returns><c>true</c>, wenn der Key erkannt und verarbeitet wurde. Sonst <c>false</c>.</returns>
    bool ParseThisJson(string key, JsonElement value);

    #endregion
}
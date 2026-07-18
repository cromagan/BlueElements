// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueBasics.Interfaces;

/// <summary>
/// Pendant zu <see cref="IParseable" /> für die neuen JSON-basierten Serialisierungs-Routinen.
/// Wird parallel zu <see cref="IParseable" /> implementiert - beide Schnittstellen können
/// koexistieren, ohne sich gegenseitig zu beeinflussen. Die klassischen Routinen
/// (<see cref="IParseable.ParseThis" /> usw.) bleiben unangetastet.
/// </summary>
public interface IJsonParseable : IJsonStringable {

    #region Events

    event EventHandler<JsonPathChangedEventArgs>? PropertyChangedExt;

    #endregion

    #region Methods

    /// <summary>
    /// Sucht ein direktes Sub-Item anhand des Container-Namens und des KeyNames.
    /// Container-Namen sind z.B. <c>Items</c>, <c>Points</c>, <c>JointPoints</c>.
    /// Gibt <c>null</c> zurück, wenn kein passendes Sub-Item gefunden wurde.
    /// Wird von <see cref="JsonParseableExtension.ApplyPartialJson" /> für den
    /// Pfadabstieg genutzt.
    /// </summary>
    IJsonParseable? GetSubItemByKey(string containerName, string key);

    /// <summary>
    /// Löst das <see cref="PropertyChangedExt" />-Event aus. Wird von Property-Settern
    /// in Implementierern aufgerufen, um eine inkrementelle Speicherung auszulösen.
    /// Implementierer delegieren typischerweise an
    /// <see cref="JsonParseableExtension.BuildSubItemEventArgs" />.
    /// </summary>
    /// <param name="relativePath">Pfad relativ zu diesem Objekt, z.B.
    /// <c>Rotation</c> oder <c>Items[btnSubmit].Rotation</c>.</param>
    /// <param name="value">Der neue Wert. Entweder ein primitives Objekt
    /// (int, string, bool, ...), das unter dem letzten Pfad-Segment als Key
    /// eingetragen wird — oder ein bestehendes <see cref="JsonObject" />
    /// (für Bubbling aus Sub-Containern).</param>
    void OnPropertyChangedExt(string relativePath, object? value);

    /// <summary>
    /// Wird nach dem Parsen aller Keys aufgerufen und ermöglicht
    /// abschließende Validierungen und Reparaturen. Pendant zu
    /// <see cref="IParseable.ParseFinished" />.
    /// </summary>
    /// <param name="parsed">Das komplette JSON-Element, das ursprünglich übergeben wurde.</param>
    void ParseFinishedJson(JsonElement parsed);

    /// <summary>
    /// Übernimmt den Zustand aus dem übergebenen <paramref name="json" />.
    /// Pendant zu <see cref="IJsonStringable.ParseableJson" /> — selbe Spiegel-Richtung:
    /// die Implementierung liest ihre eigenen Keys direkt (z.B.
    /// <c>_drehwinkel = json.GetInt("rotation");</c>) und reicht das gleiche
    /// <paramref name="json" /> anschließend an <c>base.ParseJson(json)</c> weiter.
    /// Damit funktioniert auch ein Partial-Update (<c>{ "rotation": 45 }</c>)
    /// automatisch — alle nicht vorhandenen Keys werden schlicht übersprungen.
    /// Keys sind stets kleingeschrieben.
    /// </summary>
    void ParseJson(JsonObject json);

    #endregion
}

/// <summary>
/// Statische Erweiterungsmethoden für <see cref="IJsonParseable" />.
/// Pendant zur <see cref="ParseableExtension" /> für die klassische
/// String-basierte Serialisierung.
/// </summary>
public static class JsonParseableExtension {

    #region Methods

    /// <summary>
    /// Löst einen key-basierten Pfad wie <c>Items[btnSubmit].Rotation</c> ausgehend von
    /// <paramref name="root" /> auf und wendet <paramref name="value" /> über
    /// <see cref="IJsonParseable.ParseJson" /> auf das Blatt an.
    /// Pfad-Segmente: Container-Name, optionaler [<c>KeyName</c>], Punkt als Trenner.
    /// Am Blatt (kein weiterer Punkt im Pfad) wird der letzte Token als Property-Name
    /// zusammen mit dem Wert in ein <see cref="JsonObject" /> verpackt und an
    /// <see cref="IJsonParseable.ParseJson" /> übergeben.
    /// </summary>
    public static void ApplyPartialJson(this IJsonParseable root, string path, JsonElement value) {
        if (string.IsNullOrEmpty(path)) { return; }

        var bracketPos = path.IndexOf('[');

        if (bracketPos < 0) {
            var leafName = path;
            var dotPos = path.LastIndexOf('.');
            if (dotPos >= 0) { leafName = path[(dotPos + 1)..]; }

            var partial = new JsonObject { [leafName.ToLowerInvariant()] = value.ToJsonNode() };
            root.ParseJson(partial);
            return;
        }

        var containerName = path[..bracketPos];
        var keyEnd = path.IndexOf(']', bracketPos);
        if (keyEnd < 0) { return; }

        var key = path[(bracketPos + 1)..keyEnd];

        var dotAfterKey = path.IndexOf('.', keyEnd);
        if (dotAfterKey < 0 || dotAfterKey >= path.Length - 1) { return; }

        var rest = path[(dotAfterKey + 1)..];
        var child = root.GetSubItemByKey(containerName, key);

        if (child is null) { return; }

        child.ApplyPartialJson(rest, value);
    }

    /// <summary>
    /// Wandelt einen beliebigen Wert in einen <see cref="JsonNode" /> um.
    /// Primitive Typen werden direkt konvertiert, alles andere über
    /// <see cref="JsonSerializer" /> serialisiert.
    /// </summary>
    public static JsonNode? AsJsonNode(object? value) {
        return value switch {
            null => null,
            JsonNode jn => jn,
            string s => JsonValue.Create(s),
            int i => JsonValue.Create(i),
            long l => JsonValue.Create(l),
            float f => JsonValue.Create(f),
            double d => JsonValue.Create(d),
            bool b => JsonValue.Create(b),
            DateTime dt => JsonValue.Create(dt.ToString("o")),
            _ => JsonSerializer.SerializeToNode(value)
        };
    }

    /// <summary>
    /// Erzeugt die <see cref="JsonPathChangedEventArgs" /> für einen
    /// <see cref="IJsonParseable.OnPropertyChangedExt" />-Aufruf. Die Logik ist
    /// in der Extension zentralisiert, sodass Implementierer von
    /// <see cref="IJsonParseable" /> nur noch ein Einzeiler bleiben.
    /// <para>
    /// Wert-Regel:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Ist <paramref name="value" /> bereits ein <see cref="JsonObject" />
    ///     (Bubbling aus einem Sub-Container), wird es unverändert übernommen.</description></item>
    ///   <item><description>Sonst wird der letzte Pfad-Abschnitt als Key verwendet und der Wert
    ///     als <see cref="JsonNode" /> eingetragen, z.B. <c>"rotation"</c> + <c>45</c>
    ///     ergibt <c>{"rotation": 45}</c>.</description></item>
    /// </list>
    /// </summary>
    public static JsonPathChangedEventArgs BuildSubItemEventArgs(this IJsonParseable sender, string relativePath, object? value) {
        var sourceKey = (sender as IHasKeyName)?.KeyName ?? sender.GetType().Name;
        return new JsonPathChangedEventArgs(relativePath, BuildPartialJson(relativePath, value), sourceKey);
    }

    /// <summary>
    /// Deserialisiert das übergebene <see cref="JsonElement" /> in das Zielobjekt,
    /// indem es in ein <see cref="JsonObject" /> konvertiert (Keys auf
    /// Kleinschreibung normalisiert) und einmalig an
    /// <see cref="IJsonParseable.ParseJson" /> übergeben wird.
    /// Am Ende wird <see cref="IJsonParseable.ParseFinishedJson" /> aufgerufen.
    /// </summary>
    public static void ParseJson(this IJsonParseable parsable, JsonElement toParse) {
        if (toParse.ValueKind != JsonValueKind.Object) { return; }

        var jo = new JsonObject();
        foreach (var pair in toParse.EnumerateObject()) {
            jo[pair.Name.ToLowerInvariant()] = pair.Value.ToJsonNode();
        }

        parsable.ParseJson(jo);
        parsable.ParseFinishedJson(toParse);
    }

    /// <summary>
    /// Pendant zu <see cref="ParseJson(IJsonParseable, JsonElement)" /> für
    /// Aufrufer, die bereits ein <see cref="JsonObject" /> haben. Ruft
    /// <see cref="IJsonParseable.ParseJson" /> direkt auf und reicht das
    /// Objekt (konvertiert) an <see cref="IJsonParseable.ParseFinishedJson" /> weiter.
    /// </summary>
    public static void ParseJson(this IJsonParseable parsable, JsonObject toParse) {
        parsable.ParseJson(toParse);
        parsable.ParseFinishedJson(toParse.ToJsonElement());
    }

    /// <summary>
    /// Verpackt <paramref name="value" /> in ein <see cref="JsonObject" />:
    /// bereits bestehende JsonObjects (Bubbling) werden 1:1 übernommen,
    /// alle anderen Werte unter dem letzten Segment von
    /// <paramref name="relativePath" /> als Key eingetragen.
    /// <para>
    /// Internal statt Extension auf <see cref="IJsonParseable" />, weil auch
    /// <c>ParseableItem</c> (das formal nicht IJsonParseable implementiert)
    /// die gleiche Logik nutzen soll.
    /// </para>
    /// </summary>
    internal static JsonObject BuildPartialJson(string relativePath, object? value) {
        if (value is JsonObject jo) { return jo; }

        var leaf = relativePath;
        var dot = relativePath.LastIndexOf('.');
        if (dot >= 0 && dot < relativePath.Length - 1) {
            leaf = relativePath[(dot + 1)..];
        }

        return new JsonObject { [leaf] = AsJsonNode(value) };
    }

    #endregion
}

/// <summary>
/// Event-Argumente für <see cref="IJsonParseable.PropertyChangedExt" />.
/// Beschreibt eine einzelne Property-Änderung im Sub-Baum.
/// </summary>
public class JsonPathChangedEventArgs(string relativePath, JsonObject partial, string sourceKey) : System.EventArgs {

    #region Properties

    /// <summary>
    /// Partielles JSON für genau diese Property, z.B. <c>{ "rotation": 45 }</c>.
    /// Wird direkt ins Append-Log serialisiert.
    /// </summary>
    public JsonObject Partial { get; init; } = partial;

    /// <summary>
    /// Relativer Pfad vom Auslöser zum Abonnenten, z.B. <c>Rotation</c>
    /// oder <c>Points[LO].X</c> (bereits von Containern präfixt).
    /// </summary>
    public string RelativePath { get; init; } = relativePath;

    /// <summary>
    /// KeyName des ursprünglichen Auslösers (des Objekts, dessen Property geändert wurde).
    /// </summary>
    public string SourceKey { get; init; } = sourceKey;

    #endregion
}
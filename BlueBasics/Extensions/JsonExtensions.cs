// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueBasics;

public static partial class Extensions {

    #region Methods

    /// <summary>
    /// Interpretiert das aktuelle <see cref="JsonElement" /> selbst als Padding-Objekt
    /// (Felder <c>left</c>, <c>top</c>, <c>right</c>, <c>bottom</c>).
    /// </summary>
    public static System.Windows.Forms.Padding AsPadding(this JsonElement json) =>
        json.ValueKind != JsonValueKind.Object ? System.Windows.Forms.Padding.Empty
            : new System.Windows.Forms.Padding(json.GetInt("left"), json.GetInt("top"), json.GetInt("right"), json.GetInt("bottom"));

    /// <summary>
    /// Interpretiert das aktuelle <see cref="JsonElement" /> selbst als SizeF-Objekt
    /// (Felder <c>width</c>, <c>height</c>).
    /// </summary>
    public static SizeF AsSizeF(this JsonElement json) =>
        json.ValueKind != JsonValueKind.Object ? SizeF.Empty : new SizeF(json.GetFloat("width"), json.GetFloat("height"));

    public static bool GetBool(this JsonElement json, string key, bool defaultValue = false) {
        if (json.TryGetProperty(key, out var elem) && elem.ValueKind is JsonValueKind.True or JsonValueKind.False) { return elem.GetBoolean(); }
        return defaultValue;
    }

    public static bool GetBool(this JsonObject json, string key, bool defaultValue = false) {
        if (json[key] is JsonValue v && v.TryGetValue(out bool f)) { return f; }
        return defaultValue;
    }

    public static T GetEnum<T>(this JsonElement json, string key) where T : struct, Enum {
        if (json.TryGetProperty(key, out var elem) && elem.ValueKind == JsonValueKind.String && Enum.TryParse<T>(elem.GetString(), out var result)) { return result; }
        return default;
    }

    /// <summary>
    /// Liest einen Enum-Wert unter <paramref name="key" />. Akzeptiert sowohl den
    /// numerischen Wert (<see cref="JsonValueKind.Number" />) als auch die
    /// String-Repräsentation. Bei fehlendem oder ungültigem Key wird
    /// <c>default(T)</c> geliefert.
    /// </summary>
    public static T GetEnum<T>(this JsonObject json, string key) where T : struct, Enum {
        switch (json[key]) {
            case JsonValue v when v.TryGetValue(out int i): return (T)(object)i;
            case JsonValue v when v.TryGetValue(out string? s) && Enum.TryParse<T>(s, out var result): return result;
        }
        return default;
    }

    public static float GetFloat(this JsonElement json, string key, float defaultValue = 0f) {
        if (json.TryGetProperty(key, out var elem) && elem.ValueKind == JsonValueKind.Number) { return elem.GetSingle(); }
        return defaultValue;
    }

    public static float GetFloat(this JsonElement? json, string key, float defaultValue = 0f) => json.HasValue ? json.Value.GetFloat(key, defaultValue) : defaultValue;

    public static float GetFloat(this JsonObject json, string key, float defaultValue = 0f) {
        if (json[key] is JsonValue v && v.TryGetValue(out float f)) { return f; }
        return defaultValue;
    }

    public static int GetInt(this JsonElement json, string key, int defaultValue = 0) {
        // ValueKind == Number schließt Floats nicht aus - GetInt32() wuerfe dann eine
        // InvalidOperationException. TryGetInt32 prueft den tatsaechlichen Integral-Wert.
        if (json.TryGetProperty(key, out var elem) && elem.ValueKind == JsonValueKind.Number && elem.TryGetInt32(out var i)) { return i; }
        return defaultValue;
    }

    public static int GetInt(this JsonElement? json, string key, int defaultValue = 0) => json.HasValue ? json.Value.GetInt(key, defaultValue) : defaultValue;

    public static int GetInt(this JsonObject json, string key, int defaultValue = 0) {
        if (json[key] is JsonValue v && v.TryGetValue(out int i)) { return i; }
        return defaultValue;
    }

    public static JsonElement? GetJson(this JsonElement json, string key) => json.TryGetProperty(key, out var elem) ? elem : null;

    public static JsonElement? GetJson(this JsonElement? json, string key) => json.HasValue ? json.Value.GetJson(key) : null;

    public static JsonNode? GetJson(this JsonObject json, string key) => json[key];

    /// <summary>
    /// Liest ein vom <see cref="SetPadding" /> geschriebenes verschachteltes Objekt
    /// unter <paramref name="key" />. Fehlt der Key oder ist kein Objekt, wird <see cref="System.Windows.Forms.Padding.Empty" /> geliefert.
    /// </summary>
    public static System.Windows.Forms.Padding GetPadding(this JsonElement json, string key) {
        if (!json.TryGetProperty(key, out var elem) || elem.ValueKind != JsonValueKind.Object) { return System.Windows.Forms.Padding.Empty; }
        return new System.Windows.Forms.Padding(elem.GetInt("left"), elem.GetInt("top"), elem.GetInt("right"), elem.GetInt("bottom"));
    }

    /// <summary>
    /// Liest ein vom <see cref="SetSizeF" /> geschriebenes verschachteltes Objekt
    /// unter <paramref name="key" />. Fehlt der Key oder ist kein Objekt, wird <see cref="SizeF.Empty" /> geliefert.
    /// </summary>
    public static SizeF GetSizeF(this JsonElement json, string key) {
        if (!json.TryGetProperty(key, out var elem) || elem.ValueKind != JsonValueKind.Object) { return SizeF.Empty; }
        return new SizeF(elem.GetFloat("width"), elem.GetFloat("height"));
    }

    public static string GetString(this JsonElement json, string key, string defaultValue = "") {
        if (json.TryGetProperty(key, out var elem) && elem.ValueKind == JsonValueKind.String) { return elem.GetString() ?? defaultValue; }
        return defaultValue;
    }

    public static string GetString(this JsonElement? json, string key, string defaultValue = "") => json.HasValue ? json.Value.GetString(key, defaultValue) : defaultValue;

    public static string GetString(this JsonObject json, string key, string defaultValue = "") {
        if (json[key] is JsonValue v && v.TryGetValue(out string? s)) { return s ?? defaultValue; }
        return defaultValue;
    }

    /// <summary>
    /// Liest ein JSON-Array von Strings unter dem angegebenen Key und gibt es als Liste zurück.
    /// Fehlt der Key oder ist er kein Array, wird eine leere Liste geliefert.
    /// Null-Elemente innerhalb des Arrays werden als leerer String interpretiert.
    /// </summary>
    public static List<string> GetStringList(this JsonElement json, string key) {
        if (!json.TryGetProperty(key, out var elem) || elem.ValueKind != JsonValueKind.Array) { return []; }

        List<string> result = new(elem.GetArrayLength());
        foreach (var item in elem.EnumerateArray()) {
            result.Add(item.ValueKind == JsonValueKind.String ? item.GetString() ?? string.Empty : string.Empty);
        }
        return result;
    }

    public static bool IsArray(this JsonElement json) => json.ValueKind == JsonValueKind.Array;

    public static bool IsObject(this JsonElement json) => json.ValueKind == JsonValueKind.Object;

    public static bool IsObject(this JsonElement? json) => json.HasValue && json.Value.IsObject();

    public static JsonObject Set(this JsonObject json, string key, JsonNode? value) {
        json[key] = value;
        return json;
    }

    /// <summary>
    /// Erzeugt aus einer Sequenz von <see cref="IJsonStringable" />-Objekten ein
    /// <see cref="JsonArray" /> (jedes Element via <see cref="IJsonStringable.ParseableJson" />)
    /// und weist es unter <paramref name="key" /> zu. Bei leerer Quelle erfolgt keine
    /// Zuweisung, das <paramref name="json" />-Objekt bleibt unverändert.
    /// </summary>
    public static JsonObject SetArrayIfNotEmpty<T>(this JsonObject json, string key, IEnumerable<T> items) where T : IJsonStringable {
        if (!items.Any()) { return json; }
        JsonArray array = [];
        foreach (var item in items) { array.Add(item.ParseableJson()); }
        json[key] = array;
        return json;
    }

    /// <summary>
    /// Erzeugt aus einer Sequenz von Strings ein <see cref="JsonArray" /> und weist
    /// es unter <paramref name="key" /> zu. Bei leerer Quelle erfolgt keine Zuweisung,
    /// das <paramref name="json" />-Objekt bleibt unverändert.
    /// </summary>
    public static JsonObject SetArrayIfNotEmpty(this JsonObject json, string key, IEnumerable<string> items) {
        if (!items.Any()) { return json; }
        JsonArray array = [];
        foreach (var item in items) { array.Add(item); }
        json[key] = array;
        return json;
    }

    /// <summary>
    /// Serialisiert ein <see cref="System.Windows.Forms.Padding" /> unter <paramref name="key" /> als
    /// verschachteltes Objekt mit den Feldern <c>left</c>, <c>top</c>, <c>right</c>, <c>bottom</c>.
    /// </summary>
    public static JsonObject SetPadding(this JsonObject json, string key, System.Windows.Forms.Padding padding) {
        json[key] = new JsonObject()
            .Set("left", padding.Left)
            .Set("top", padding.Top)
            .Set("right", padding.Right)
            .Set("bottom", padding.Bottom);
        return json;
    }

    /// <summary>
    /// Serialisiert ein <see cref="SizeF" /> unter <paramref name="key" /> als
    /// verschachteltes Objekt mit den Feldern <c>width</c> und <c>height</c>.
    /// </summary>
    public static JsonObject SetSizeF(this JsonObject json, string key, SizeF size) {
        json[key] = new JsonObject()
            .Set("width", size.Width)
            .Set("height", size.Height);
        return json;
    }

    /// <summary>
    /// Konvertiert ein <see cref="JsonElement" /> (z. B. aus <see cref="System.Text.Json.JsonDocument" />
    /// oder <see cref="System.Text.Json.JsonElement.Clone" />) in einen <see cref="JsonNode" />,
    /// sodass es direkt in ein übergeordnetes <see cref="JsonObject" /> / <see cref="JsonArray" />
    /// eingebettet werden kann.
    /// Ein <see cref="JsonValueKind.Undefined" />- oder <see cref="JsonValueKind.Null" />-Element
    /// liefert <c>null</c>, da <see cref="JsonElement.GetRawText" /> bei Undefined werfen würde.
    /// </summary>
    public static JsonNode? ToJsonNode(this JsonElement element) =>
        element.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null ? null : JsonNode.Parse(element.GetRawText());

    /// <summary>
    /// Konvertiert einen <see cref="JsonNode" /> (z.B. <see cref="JsonObject" />,
    /// <see cref="JsonArray" />) in ein <see cref="JsonElement" />, sodass er an
    /// APIs übergeben werden kann, die auf <see cref="JsonElement" /> arbeiten
    /// (z.B. die <c>AsPadding</c>-/<c>AsSizeF</c>-Erweiterungen). <c>null</c> liefert
    /// <see cref="JsonValueKind.Undefined" />.
    /// </summary>
    public static JsonElement ToJsonElement(this JsonNode? node) =>
        node is null ? default : JsonDocument.Parse(node.ToJsonString()).RootElement.Clone();

    #endregion
}
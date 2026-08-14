// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Classes;

namespace BlueBasics;

public static partial class Extensions {

    #region Methods

    /// <summary>
    /// Liest ein von <see cref="Set" /> geschriebenes Base64-PNG unter
    /// <paramref name="key" />. Fehlt der Key, ist er kein String oder leer, wird
    /// <paramref name="defaultValue" /> geliefert - so bleiben Partial-Updates
    /// möglich, ohne bestehende Bilder zu überschreiben.
    /// </summary>
    public static Bitmap? GetBitmap(this JsonObject json, string key, Bitmap? defaultValue = null) {
        if (json[key] is JsonValue v && v.TryGetValue(out string? s) && s is { Length: > 0 }) {
            return Base64ToBitmap(s);
        }
        return defaultValue;
    }

    public static bool GetBool(this JsonObject json, string key, bool defaultValue = false) {
        if (json[key] is JsonValue v && v.TryGetValue(out bool f)) { return f; }
        return defaultValue;
    }

    /// <summary>
    /// Liest einen ARGB-Farbwert (als int) unter <paramref name="key" /> und
    /// konvertiert ihn in ein <see cref="Color" />. Negative Werte gelten als
    /// "nicht gesetzt" (Sentinel). Fehlt der Key, ist er keine Zahl oder ist der
    /// Wert negativ, wird <paramref name="defaultValue" /> geliefert - so bleiben
    /// Partial-Updates möglich, ohne bestehende Farben zu überschreiben.
    /// </summary>
    public static Color GetColor(this JsonObject json, string key, Color defaultValue) {
        if (json[key] is JsonValue v && v.TryGetValue(out int i) && i >= 0) { return Color.FromArgb(i); }
        return defaultValue;
    }

    public static double GetDouble(this JsonObject json, string key, double defaultValue = 0d) {
        if (json[key] is JsonValue v && v.TryGetValue(out double d)) { return d; }
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
    public static T GetEnum<T>(this JsonObject json, string key) where T : struct, Enum => json.GetEnum(key, default(T));

    /// <summary>
    /// Liest einen Enum-Wert unter <paramref name="key" />. Akzeptiert sowohl den
    /// numerischen Wert (<see cref="JsonValueKind.Number" />) als auch die
    /// String-Repräsentation. Bei fehlendem oder ungültigem Key wird
    /// <paramref name="defaultValue" /> geliefert - so bleiben Partial-Updates
    /// möglich, ohne bestehende Felder zu überschreiben.
    /// </summary>
    public static T GetEnum<T>(this JsonObject json, string key, T defaultValue) where T : struct, Enum {
        switch (json[key]) {
            case JsonValue v when v.TryGetValue(out int i):
                return (T)Enum.ToObject(typeof(T), i);

            case JsonValue v when v.TryGetValue(out string? s) && Enum.TryParse<T>(s, out var result):
                return result;
        }
        return defaultValue;
    }

    public static float GetFloat(this JsonElement json, string key, float defaultValue = 0f) {
        if (json.TryGetProperty(key, out var elem) && elem.ValueKind == JsonValueKind.Number) { return elem.GetSingle(); }
        return defaultValue;
    }

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

    public static int GetInt(this JsonObject json, string key, int defaultValue = 0) {
        if (json[key] is JsonValue v && v.TryGetValue(out int i)) { return i; }
        return defaultValue;
    }

    public static JsonElement? GetJson(this JsonElement json, string key) => json.TryGetProperty(key, out var elem) ? elem : null;

    public static JsonNode? GetJson(this JsonObject json, string key) => json[key];

    /// <summary>
    /// Liest ein JSON-Array von <see cref="IJsonParseable" />-Objekten unter dem
    /// angegebenen <paramref name="key" /> und erzeugt über
    /// <see cref="ParseableItem.NewByParsingJson{T}" /> die passenden Instanzen.
    /// Elemente, die kein JSON-Objekt sind oder nicht geparsed werden konnten,
    /// werden übersprungen. Fehlt der Key oder ist er kein Array, wird eine leere
    /// Liste geliefert. Bei <paramref name="sort" /> = <c>true</c> wird die
    /// Ergebnisliste vor der Rückgabe sortiert.
    /// </summary>
    public static List<T> GetList<T>(this JsonObject json, string key, bool sort) where T : ParseableItem, IJsonParseable {
        List<T> result = [];
        if (json[key] is JsonArray arr) {
            foreach (var item in arr) {
                if (item is not JsonObject jo) { continue; }
                if (ParseableItem.NewByParsingJson<T>(jo) is { } created) { result.Add(created); }
            }
            if (sort) { result.Sort(); }
        }
        return result;
    }

    /// <summary>
    /// Liest ein JSON-Array von Strings unter dem angegebenen Key und gibt es als
    /// Liste zurück. Fehlt der Key oder ist er kein Array, wird
    /// <paramref name="defaultValue" /> geliefert.
    /// Null-Elemente innerhalb des Arrays werden als leerer String interpretiert.
    /// </summary>
    public static List<string> GetListString(this JsonObject json, string key, List<string>? defaultValue) =>
        json[key] is JsonArray arr ? arr.ToStringList() : defaultValue ?? [];

    /// <summary>
    /// Liest ein vom <see cref="Set" /> geschriebenes verschachteltes Objekt
    /// unter <paramref name="key" />. Fehlt der Key oder ist kein Objekt, wird
    /// <paramref name="defaultValue" /> geliefert.
    /// </summary>
    public static System.Windows.Forms.Padding GetPadding(this JsonObject json, string key, System.Windows.Forms.Padding defaultValue) {
        if (json[key] is not JsonObject jo) { return defaultValue; }
        return new System.Windows.Forms.Padding(jo.GetInt("left"), jo.GetInt("top"), jo.GetInt("right"), jo.GetInt("bottom"));
    }

    /// <summary>
    /// Liest ein vom <see cref="Set" /> geschriebenes verschachteltes Objekt
    /// unter <paramref name="key" />. Fehlt der Key oder ist kein Objekt, wird
    /// <paramref name="defaultValue" /> geliefert.
    /// </summary>
    public static SizeF GetSizeF(this JsonObject json, string key, SizeF defaultValue) {
        if (json[key] is not JsonObject jo) { return defaultValue; }
        return new SizeF(jo.GetFloat("width"), jo.GetFloat("height"));
    }

    public static string GetString(this JsonElement json, string key, string defaultValue = "") {
        if (json.TryGetProperty(key, out var elem) && elem.ValueKind == JsonValueKind.String) { return elem.GetString() ?? defaultValue; }
        return defaultValue;
    }

    public static string GetString(this JsonObject json, string key, string defaultValue = "") {
        if (json[key] is JsonValue v && v.TryGetValue(out string? s)) { return s ?? defaultValue; }
        return defaultValue;
    }

    /// <summary>
    /// Liest ein JSON-Array von Strings unter dem angegebenen Key und gibt es als
    /// Liste zurück. Fehlt der Key oder ist er kein Array, wird eine leere Liste
    /// geliefert. Null-Elemente innerhalb des Arrays werden als leerer String
    /// interpretiert.
    /// </summary>
    public static List<string> GetStringList(this JsonObject json, string key) =>
        json[key] is JsonArray arr ? arr.ToStringList() : [];

    public static bool IsArray(this JsonElement json) => json.ValueKind == JsonValueKind.Array;

    public static bool IsObject(this JsonElement json) => json.ValueKind == JsonValueKind.Object;

    public static void Set(this JsonObject json, string key, JsonNode? value) => json[key] = value;

    /// <summary>
    /// Serialisiert ein <see cref="Bitmap" /> unter <paramref name="key" /> als
    /// Base64-kodiertes PNG. Bei <c>null</c> wird der Key nicht gesetzt, sodass
    /// das Ziel-JSON unverändert bleibt (kein <c>null</c>-Eintrag).
    /// </summary>
    public static void Set(this JsonObject json, string key, Bitmap? bmp) {
        if (bmp is null) { return; }
        json[key] = BitmapToBase64(bmp, ImageFormat.Png);
    }

    /// <summary>
    /// Serialisiert ein <see cref="DateTime" /> unter <paramref name="key" /> als
    /// ISO-8601-Roundtrip-Format (<c>"o"</c>), sodass der Wert verlustfrei
    /// zurückgelesen werden kann - kompatibel zu den Get-Routen, die
    /// <see cref="JsonNode.GetValue{T}" /> bzw.
    /// <see cref="DateTimeParse(string)" /> verwenden.
    /// </summary>
    public static void Set(this JsonObject json, string key, DateTime value) => json[key] = value.ToString("o", CultureInfo.InvariantCulture);

    /// <summary>
    /// Serialisiert ein <see cref="System.Windows.Forms.Padding" /> unter <paramref name="key" /> als
    /// verschachteltes Objekt mit den Feldern <c>left</c>, <c>top</c>, <c>right</c>, <c>bottom</c>.
    /// </summary>
    public static void Set(this JsonObject json, string key, System.Windows.Forms.Padding padding) {
        JsonObject jo = new();
        jo.Set("left", padding.Left);
        jo.Set("top", padding.Top);
        jo.Set("right", padding.Right);
        jo.Set("bottom", padding.Bottom);
        json.Set(key, jo);
    }

    /// <summary>
    /// Serialisiert ein <see cref="SizeF" /> unter <paramref name="key" /> als
    /// verschachteltes Objekt mit den Feldern <c>width</c> und <c>height</c>.
    /// </summary>
    public static void Set(this JsonObject json, string key, SizeF size) {
        JsonObject jo = new();
        jo.Set("width", size.Width);
        jo.Set("height", size.Height);
        json.Set(key, jo);
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
    /// Konvertiert ein <see cref="JsonElement" /> (z. B. aus <see cref="JsonDocument" />
    /// oder <see cref="JsonElement.Clone" />) in einen <see cref="JsonNode" />,
    /// sodass es direkt in ein übergeordnetes <see cref="JsonObject" /> / <see cref="JsonArray" />
    /// eingebettet werden kann.
    /// Ein <see cref="JsonValueKind.Undefined" />- oder <see cref="JsonValueKind.Null" />-Element
    /// liefert <c>null</c>, da <see cref="JsonElement.GetRawText" /> bei Undefined werfen würde.
    /// </summary>
    public static JsonNode? ToJsonNode(this JsonElement element) =>
        element.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null ? null : JsonNode.Parse(element.GetRawText());

    /// <summary>
    /// Konvertiert ein <see cref="JsonArray" /> in eine Liste von Strings.
    /// Elemente, die keine String-<see cref="JsonValue" /> sind, werden als
    /// leerer String interpretiert.
    /// </summary>
    public static List<string> ToStringList(this JsonArray arr) {
        List<string> result = new(arr.Count);
        foreach (var item in arr) {
            result.Add(item is JsonValue v && v.TryGetValue(out string? s) ? s ?? string.Empty : string.Empty);
        }
        return result;
    }

    #endregion
}
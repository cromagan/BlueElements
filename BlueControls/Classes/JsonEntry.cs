// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Text;

namespace BlueControls.Classes;

public sealed class JsonEntry : IHasKeyName {

    #region Constructors

    public JsonEntry(string name, JsonObject? data) {
        KeyName = name;
        JsonData = data;
        Modified = DateTime.Now;
    }

    #endregion

    #region Properties

    public JsonObject? JsonData { get; set; }
    public string KeyName { get; set; }
    public DateTime Modified { get; set; }

    #endregion

    #region Methods

    public static JsonEntry? Parse(JsonElement element) {
        if (!element.IsObject()) { return null; }

        var name = element.GetString("name");
        if (string.IsNullOrEmpty(name)) { return null; }

        // ToJsonNode liefert null bei Undefined/Null, sonst den geparsten Knoten.
        // Da das Daten-Feld ein JSON-Objekt sein soll, casten wir direkt auf JsonObject.
        // Clone entfällt: JsonObject ist mutable und unabhängig vom JsonDocument.
        var data = element.GetJson("data");
        return new JsonEntry(name, data?.ToJsonNode() as JsonObject);
    }

    #endregion
}

// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Text.Json.Nodes;

namespace BlueDeskSnap.Snap;

/// <summary>
/// Gespeicherte Einstellungen eines Quality-Plugins innerhalb eines Snaps - gleiche Struktur wie die TableView-Views: Name, Daten und Änderungszeit.
/// </summary>
public sealed class QualityPluginState : ParseableItem, IJsonParseable, IHasKeyName {

    #region Constructors

    public QualityPluginState() { }

    public QualityPluginState(string keyName, JsonObject? settings) {
        KeyName = keyName;
        Settings = settings;
        Modified = DateTime.Now;
    }

    #endregion

    #region Properties

    public static string ClassId => "QualityPluginState";

    public string KeyName { get; set; } = string.Empty;

    /// <summary>
    /// Die Einstellungen des Plugins zum Erfassungszeitpunkt.
    /// </summary>
    public JsonObject? Settings { get; set; }

    public DateTime Modified { get; set; } = DateTime.Now;

    #endregion

    #region Methods

    /// <summary>
    /// Serialisiert den Zustand als JsonObject; Bestandteil des WindowSnapshot-JSON.
    /// </summary>
    public JsonObject ParseableJson() {
        JsonObject json = new();
        json.Set("type", MyClassId);
        json.Set("name", KeyName);
        json.Set("data", Settings?.DeepClone());
        json.Set("modified", Modified);
        return json;
    }

    public void ParseFinishedJson(JsonObject parsed) { }

    public void ParseJson(JsonObject json) {
        KeyName = json.GetString("name", KeyName);
        Settings = json.GetJson("data") as JsonObject;
        if (DateTimeTryParse(json.GetString("modified"), out var modified)) { Modified = modified; }
    }

    public override List<string> ParseableItems() {
        List<string> result = [.. base.ParseableItems()];
        result.ParseableAdd("Name", KeyName);
        return result;
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "type":
            case "classid": // Wurde bereits abgefragt, dadurch ist erst die Routine aufgerufen worden
                return value.ToNonCritical() == MyClassId;
            case "name":
                KeyName = value.FromNonCritical();
                return true;
        }

        return false;
    }

    #endregion
}

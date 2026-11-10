// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Text.Json.Nodes;

namespace BlueDeskSnap.Snap;

/// <summary>
/// Ein Desktop-Snapshot: alle sichtbaren Fenster mit deren Geometrie und Dokumenten.
/// </summary>
public sealed class WindowSnapshot : ParseableItem, IJsonParseable {

    #region Properties

    public static string ClassId => "WindowSnapshot";

    /// <summary>
    /// KeyNames der Quality-Plugins, die beim Erfassen aktiviert waren.
    /// </summary>
    public List<string> ActivePlugins { get; } = [];

    public DateTime Created { get; set; }

    /// <summary>
    /// Eindeutiger Schlüssel des Snaps - stabil über Umbenennen und Aktualisieren hinweg.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Einstellungen aller Quality-Plugins zum Erfassungszeitpunkt - gleiche Struktur wie die TableView-Views.
    /// </summary>
    public List<QualityPluginState> PluginSettings { get; } = [];

    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Die Fenster in Z-Ordnung (vorne zuerst).
    /// </summary>
    public List<WindowInfo> Windows { get; } = [];

    #endregion

    #region Methods

    /// <summary>
    /// Serialisiert den kompletten Snapshot als JsonObject inklusive Fenster und aktivierter Quality-Plugins.
    /// </summary>
    public JsonObject ParseableJson() {
        JsonObject json = new();
        json.Set("type", MyClassId);
        json.Set("id", Id);
        json.Set("name", Name);
        json.Set("created", Created);
        json.Set("activeplugins", ActivePlugins);
        json.SetArrayIfNotEmpty("pluginsettings", PluginSettings);
        json.SetArrayIfNotEmpty("windows", Windows);
        return json;
    }

    public override List<string> ParseableItems() {
        List<string> result = [.. base.ParseableItems()];
        result.ParseableAdd("Name", Name);
        result.ParseableAdd("Created", Created);
        result.ParseableAdd("Window", Windows.Cast<IStringable>());
        return result;
    }

    public void ParseFinishedJson(JsonObject parsed) { }

    public void ParseJson(JsonObject json) {
        Id = json.GetString("id", Id);
        Name = json.GetString("name", Name);
        if (DateTimeTryParse(json.GetString("created"), out var created)) { Created = created; }
        ActivePlugins.Clear();
        ActivePlugins.AddRange(json.GetStringList("activeplugins"));
        PluginSettings.Clear();
        PluginSettings.AddRange(json.GetList<QualityPluginState>("pluginsettings", false));
        Windows.Clear();
        Windows.AddRange(json.GetList<WindowInfo>("windows", false));
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "type":
            case "classid": // Wurde bereits abgefragt, dadurch ist erst die Routine aufgerufen worden
                return value.ToNonCritical() == MyClassId;
            case "id":
                Id = value.FromNonCritical();
                return true;
            case "name":
                Name = value.FromNonCritical();
                return true;
            case "created":
                Created = DateTimeParse(value);
                return true;
            case "folder": // Früher lag jeder Snap in einem eigenen Ordner, der Schlüssel wird nur noch konsumiert
                return true;
            case "screenshot": // Früher wurde ein Screenshot gespeichert, der Schlüssel wird nur noch konsumiert
                return true;
            case "window":
                if (NewByParsing<WindowInfo>(value.FromNonCritical()) is { } wi) { Windows.Add(wi); }
                return true;
        }

        return false;
    }

    #endregion
}

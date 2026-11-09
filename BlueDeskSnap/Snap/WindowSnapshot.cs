// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Text.Json.Nodes;

namespace BlueDeskSnap.Snap;

/// <summary>
/// Ein Desktop-Snapshot: Screenshot-Datei plus alle sichtbaren Fenster mit deren Geometrie und Dokumenten.
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
    /// Der Ordner, in dem Snapshot.json und der Screenshot liegen.
    /// </summary>
    public string Folder { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string ScreenshotFile { get; set; } = string.Empty;

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
        json.Set("name", Name);
        json.Set("created", Created);
        json.Set("folder", Folder);
        json.Set("screenshot", ScreenshotFile);
        json.Set("activeplugins", ActivePlugins);
        json.SetArrayIfNotEmpty("windows", Windows);
        return json;
    }

    public override List<string> ParseableItems() {
        List<string> result = [.. base.ParseableItems()];
        result.ParseableAdd("Name", Name);
        result.ParseableAdd("Created", Created);
        result.ParseableAdd("Folder", Folder);
        result.ParseableAdd("Screenshot", ScreenshotFile);
        result.ParseableAdd("Window", Windows.Cast<IStringable>());
        return result;
    }

    public void ParseFinishedJson(JsonObject parsed) { }

    public void ParseJson(JsonObject json) {
        Name = json.GetString("name", Name);
        if (DateTimeTryParse(json.GetString("created"), out var created)) { Created = created; }
        Folder = json.GetString("folder", Folder);
        ScreenshotFile = json.GetString("screenshot", ScreenshotFile);
        ActivePlugins.Clear();
        ActivePlugins.AddRange(json.GetStringList("activeplugins"));
        Windows.Clear();
        Windows.AddRange(json.GetList<WindowInfo>("windows", false));
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "type":
            case "classid": // Wurde bereits abgefragt, dadurch ist erst die Routine aufgerufen worden
                return value.ToNonCritical() == MyClassId;
            case "name":
                Name = value.FromNonCritical();
                return true;
            case "created":
                Created = DateTimeParse(value);
                return true;
            case "folder":
                Folder = value.FromNonCritical();
                return true;
            case "screenshot":
                ScreenshotFile = value.FromNonCritical();
                return true;
            case "window":
                if (NewByParsing<WindowInfo>(value.FromNonCritical()) is { } wi) { Windows.Add(wi); }
                return true;
        }

        return false;
    }

    #endregion
}

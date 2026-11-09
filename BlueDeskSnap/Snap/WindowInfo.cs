// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Text.Json.Nodes;
using FormWindowState = System.Windows.Forms.FormWindowState;

namespace BlueDeskSnap.Snap;

/// <summary>
/// Gesicherte Daten eines einzelnen Fensters: Prozess, Titel, Klasse, Geometrie, Zustand und offene Dokumente.
/// </summary>
public sealed class WindowInfo : ParseableItem, IJsonParseable {

    #region Properties

    public static string ClassId => "WindowInfo";

    public Rectangle Bounds { get; set; } = Rectangle.Empty;

    public string ClassName { get; set; } = string.Empty;

    /// <summary>
    /// Die beim Snapshot ermittelten Dateien/URLs des Fensters (sortiert nach dem zuständigen Plugin).
    /// </summary>
    public List<string> Documents { get; } = [];

    public string PluginKeyName { get; set; } = string.Empty;

    public string ProcessName { get; set; } = string.Empty;

    public FormWindowState State { get; set; } = FormWindowState.Normal;

    public string Title { get; set; } = string.Empty;

    #endregion

    #region Methods

    /// <summary>
    /// Serialisiert den Zustand als JsonObject; Bestandteil des WindowSnapshot-JSON.
    /// </summary>
    public JsonObject ParseableJson() {
        JsonObject json = new();
        json.Set("type", MyClassId);
        json.Set("process", ProcessName);
        json.Set("title", Title);
        json.Set("classname", ClassName);
        json.Set("plugin", PluginKeyName);
        json.Set("state", (int)State);
        json.Set("bounds", Bounds);
        json.Set("documents", Documents);
        return json;
    }

    public override List<string> ParseableItems() {
        List<string> result = [.. base.ParseableItems()];
        result.ParseableAdd("Process", ProcessName);
        result.ParseableAdd("Title", Title);
        result.ParseableAdd("ClassName", ClassName);
        result.ParseableAdd("Plugin", PluginKeyName);
        result.ParseableAdd("State", State);
        result.ParseableAdd("Bounds", BoundsText());
        result.ParseableAdd("Doc", Documents, true);
        return result;
    }

    public void ParseFinishedJson(JsonObject parsed) { }

    public void ParseJson(JsonObject json) {
        ProcessName = json.GetString("process", ProcessName);
        Title = json.GetString("title", Title);
        ClassName = json.GetString("classname", ClassName);
        PluginKeyName = json.GetString("plugin", PluginKeyName);
        State = (FormWindowState)json.GetInt("state", (int)State);
        Bounds = json.GetRectangle("bounds", Bounds);
        Documents.Clear();
        Documents.AddRange(json.GetStringList("documents"));
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "type":
            case "classid": // Wurde bereits abgefragt, dadurch ist erst die Routine aufgerufen worden
                return value.ToNonCritical() == MyClassId;
            case "process":
                ProcessName = value.FromNonCritical();
                return true;
            case "title":
                Title = value.FromNonCritical();
                return true;
            case "classname":
                ClassName = value.FromNonCritical();
                return true;
            case "plugin":
                PluginKeyName = value.FromNonCritical();
                return true;
            case "state":
                State = (FormWindowState)IntParse(value);
                return true;
            case "bounds":
                Bounds = BoundsParse(value);
                return true;
            case "doc":
                foreach (var doc in value.Split('|', StringSplitOptions.RemoveEmptyEntries)) {
                    Documents.Add(doc.FromNonCritical());
                }
                return true;
        }

        return false;
    }

    private static Rectangle BoundsParse(string value) {
        var parts = value.Split(';');
        if (parts.Length != 4) { return Rectangle.Empty; }

        return new Rectangle(IntParse(parts[0]), IntParse(parts[1]), IntParse(parts[2]), IntParse(parts[3]));
    }

    private string BoundsText() => $"{Bounds.Left};{Bounds.Top};{Bounds.Width};{Bounds.Height}";

    #endregion
}

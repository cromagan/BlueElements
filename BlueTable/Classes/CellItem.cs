// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueTable.Classes;

/// <summary>
/// Diese Klasse enthält nur das Aussehen und gibt keinerlei Events ab.
/// </summary>
public class CellItem : IJsonParseable {

    #region Constructors

    public CellItem(string value) => Value = value;

    public CellItem() { }

    #endregion

    #region Events

    public event EventHandler<JsonPathChangedEventArgs>? PropertyChangedExt;

    #endregion

    #region Properties

    public string Value { get; set; } = string.Empty;

    #endregion

    #region Methods

    public IJsonParseable? GetSubItemByKey(string containerName, string key) => null;

    public void OnPropertyChangedExt(string relativePath, object? value) {
        if (string.IsNullOrEmpty(relativePath)) { return; }
        PropertyChangedExt?.Invoke(this, this.BuildSubItemEventArgs(relativePath, value));
    }

    public JsonObject ParseableJson() {
        var json = new JsonObject();
        json.Set("value", Value);
        return json;
    }

    public void ParseFinishedJson(JsonElement parsed) { }

    public void ParseJson(JsonObject json) {
        Value = json.GetString("value", Value);
    }

    #endregion
}
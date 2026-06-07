// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Text;
using System.Text.Json;

namespace BlueControls.Classes;

public sealed class JsonEntry : IHasKeyName {

    #region Constructors

    public JsonEntry(string name, JsonElement data) {
        KeyName = name;
        JsonData = data;
        Modified = DateTime.Now;
    }

    #endregion

    #region Properties

    public JsonElement JsonData { get; set; }
    public string KeyName { get; set; }
    public DateTime Modified { get; set; }

    #endregion

    #region Methods

    public static JsonEntry? Parse(JsonElement element) {
        if (!element.IsObject()) { return null; }

        var name = element.GetString("name");
        if (string.IsNullOrEmpty(name)) { return null; }

        var data = element.GetJson("data");
        return new JsonEntry(name, data ?? default);
    }

    #endregion
}
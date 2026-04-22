// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
// https://github.com/cromagan/BlueElements
//
// License: GNU Affero General Public License v3.0
// https://github.com/cromagan/BlueElements/blob/master/LICENSE
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace BlueBasics;

public static partial class Extensions {

    #region Methods

    public static JsonElement? GetJson(this JsonElement json, string key) => json.TryGetProperty(key, out var elem) ? elem : null;

    public static JsonElement? GetJson(this JsonElement? json, string key) => json.HasValue ? json.Value.GetJson(key) : null;

    public static T GetEnum<T>(this JsonElement json, string key) where T : struct, Enum {
        if (json.TryGetProperty(key, out var elem) && elem.ValueKind == JsonValueKind.String && Enum.TryParse<T>(elem.GetString(), out var result)) { return result; }
        return default;
    }

    public static float GetFloat(this JsonElement json, string key, float defaultValue = 0f) {
        if (json.TryGetProperty(key, out var elem) && elem.ValueKind == JsonValueKind.Number) { return elem.GetSingle(); }
        return defaultValue;
    }

    public static float GetFloat(this JsonElement? json, string key, float defaultValue = 0f) => json.HasValue ? json.Value.GetFloat(key, defaultValue) : defaultValue;

    public static int GetInt(this JsonElement json, string key, int defaultValue = 0) {
        if (json.TryGetProperty(key, out var elem) && elem.ValueKind == JsonValueKind.Number) { return elem.GetInt32(); }
        return defaultValue;
    }

    public static int GetInt(this JsonElement? json, string key, int defaultValue = 0) => json.HasValue ? json.Value.GetInt(key, defaultValue) : defaultValue;

    public static string GetString(this JsonElement json, string key, string defaultValue = "") {
        if (json.TryGetProperty(key, out var elem) && elem.ValueKind == JsonValueKind.String) { return elem.GetString() ?? defaultValue; }
        return defaultValue;
    }

    public static string GetString(this JsonElement? json, string key, string defaultValue = "") => json.HasValue ? json.Value.GetString(key, defaultValue) : defaultValue;

    public static bool IsArray(this JsonElement json) => json.ValueKind == JsonValueKind.Array;

    public static bool IsObject(this JsonElement json) => json.ValueKind == JsonValueKind.Object;

    public static bool IsObject(this JsonElement? json) => json.HasValue && json.Value.IsObject();

    public static JsonObject Set(this JsonObject json, string key, JsonNode? value) {
        json[key] = value;
        return json;
    }

    #endregion
}

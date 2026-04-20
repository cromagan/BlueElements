using System;
using System.Collections.Generic;
using System.Text.Json;

namespace BlueBasics.ClassesStatic;

public static class JsonHelper {

    public static T GetEnumProperty<T>(Dictionary<string, JsonElement> props, string key) where T : struct, Enum {
        if (props.TryGetValue(key, out var elem) && elem.ValueKind == JsonValueKind.String) {
            var value = elem.GetString();
            if (!string.IsNullOrEmpty(value) && Enum.TryParse<T>(value, out var result)) {
                return result;
            }
        }
        return default;
    }

    public static string GetJsonProperty(Dictionary<string, JsonElement> props, string key, string defaultValue) {
        if (props.TryGetValue(key, out var elem) && elem.ValueKind == JsonValueKind.String) {
            return elem.GetString() ?? defaultValue;
        }
        return defaultValue;
    }

    public static int GetJsonProperty(Dictionary<string, JsonElement> props, string key, int defaultValue) {
        if (props.TryGetValue(key, out var elem) && elem.ValueKind == JsonValueKind.Number) {
            return elem.GetInt32();
        }
        return defaultValue;
    }

    public static bool GetJsonProperty(Dictionary<string, JsonElement> props, string key, bool defaultValue) {
        if (props.TryGetValue(key, out var elem) && elem.ValueKind == JsonValueKind.True || elem.ValueKind == JsonValueKind.False) {
            return elem.GetBoolean();
        }
        return defaultValue;
    }

    public static Dictionary<string, JsonElement> ToDictionary(JsonElement element) {
        var result = new Dictionary<string, JsonElement>();
        if (element.ValueKind != JsonValueKind.Object) { return result; }
        foreach (var prop in element.EnumerateObject()) {
            result[prop.Name] = prop.Value;
        }
        return result;
    }
}

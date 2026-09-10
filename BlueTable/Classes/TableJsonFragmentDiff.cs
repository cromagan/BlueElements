// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueTable.Classes;

/// <summary>
/// Berechnet granulare Pfad/Wert-Änderungen für JSON-Fragmente (MTBLJ).
/// Vergleicht den vorherigen und den neuen Blob-Wert eines TableDataType, indem
/// beide Seiten durch die identische Parse-/Serialize-Pipeline geschickt werden,
/// und liefert je geändertem Wert eine Zeile im Format {"path":..., "value":...}.
/// Sind Strukturen nicht vergleichbar (Items hinzugefügt/entfernt), wird eine
/// Komplett-Sync-Zeile der jeweiligen Liste geliefert. Das binäre Format
/// (.bdb/.mbdb) bleibt von dieser Klasse unberührt.
/// </summary>
public static class TableJsonFragmentDiff {

    #region Methods

    /// <summary>
    /// Vergleicht den vorherigen mit dem neuen Wert und liefert die granularen
    /// Pfad/Wert-Zeilen für das JSON-Fragment. Sind beide Stände gleich, ist die
    /// Liste leer.
    /// </summary>
    public static List<(string Path, JsonNode? Value)> GetChanges(Table table, TableDataType type, string previousValue, string newValue) {
        List<(string Path, JsonNode? Value)> result = [];

        switch (type) {
            case TableDataType.EventScript:
                DiffEventScripts(table, previousValue, newValue, result);
                break;

            case TableDataType.SortDefinition:
                DiffSortDefinition(table, previousValue, newValue, result);
                break;

            case TableDataType.ColumnArrangement:
                DiffColumnArrangements(table, previousValue, newValue, result);
                break;

            case TableDataType.UniqueValues:
                DiffUniqueValues(table, previousValue, newValue, result);
                break;

            case TableDataType.TableVariables:
                DiffVariables(previousValue, newValue, result);
                break;
        }

        // Sicherheitsnetz: Der Wert hat sich geändert, aber der Diff findet keinen
        // unterscheidbaren Wert (z.B. nicht serialisierte Details). Dann die
        // komplette Liste synchronisieren, damit nichts verloren geht.
        if (result.Count == 0 && !string.Equals(previousValue, newValue, StringComparison.Ordinal)) {
            result.Add(GetFullSync(table, type, newValue));
        }

        return result;
    }

    /// <summary>
    /// True, wenn Änderungen dieses Typs als granulare Pfad/Wert-Zeilen gespeichert
    /// werden. Alle anderen Typen nutzen weiterhin UndoItem-Zeilen.
    /// </summary>
    public static bool IsGranularType(TableDataType type) => type is TableDataType.EventScript
                                                                  or TableDataType.SortDefinition
                                                                  or TableDataType.ColumnArrangement
                                                                  or TableDataType.UniqueValues
                                                                  or TableDataType.TableVariables;

    private static void DiffColumnArrangements(Table table, string previousValue, string newValue, List<(string Path, JsonNode? Value)> result) {
        var oldItems = ParseArrangements(table, previousValue);
        var newItems = ParseArrangements(table, newValue);

        if (oldItems.Count != newItems.Count) {
            result.Add(("columnarrangements", ArrangementsToJsonArray(newItems)));
            return;
        }

        for (var i = 0; i < newItems.Count; i++) {
            var oldJson = oldItems[i].ParseableJson();
            var newJson = newItems[i].ParseableJson();

            var oldColumns = ColumnsByName(oldJson);
            var newColumns = ColumnsByName(newJson);

            if (!SameKeys(oldColumns.Keys, newColumns.Keys)) {
                // Struktur der Ansicht unterschiedlich - komplette Liste synchronisieren.
                result.Add(("columnarrangements", ArrangementsToJsonArray(newItems)));
                return;
            }

            foreach (var prop in newJson) {
                if (prop.Key == "columns") { continue; }
                if (!JsonNode.DeepEquals(prop.Value, oldJson[prop.Key])) {
                    result.Add(($"columnarrangements[{i}].{prop.Key}", prop.Value?.DeepClone() ?? JsonValue.Create(string.Empty)));
                }
            }

            foreach (var (colKey, colJson) in newColumns) {
                var oldColJson = oldColumns[colKey];
                foreach (var prop in colJson) {
                    if (!JsonNode.DeepEquals(prop.Value, oldColJson[prop.Key])) {
                        result.Add(($"columnarrangements[{i}].columns[{colKey}].{prop.Key}", prop.Value?.DeepClone() ?? JsonValue.Create(string.Empty)));
                    }
                }
            }
        }
    }

    private static void DiffEventScripts(Table table, string previousValue, string newValue, List<(string Path, JsonNode? Value)> result) {
        var oldItems = ParseScripts(table, previousValue);
        var newItems = ParseScripts(table, newValue);

        if (!SameKeys(oldItems.Keys, newItems.Keys)) {
            result.Add(("eventscript", ItemsToJsonArray(newItems.Values)));
            return;
        }

        foreach (var (key, newJson) in newItems) {
            var oldJson = oldItems[key];
            foreach (var prop in newJson) {
                if (!JsonNode.DeepEquals(prop.Value, oldJson[prop.Key])) {
                    result.Add(($"eventscript[{key}].{prop.Key}", prop.Value?.DeepClone() ?? JsonValue.Create(string.Empty)));
                }
            }
        }
    }

    private static void DiffSortDefinition(Table table, string previousValue, string newValue, List<(string Path, JsonNode? Value)> result) {
        var newJson = ParseSortDefinition(table, newValue);
        if (newJson is null) { return; }

        var oldJson = ParseSortDefinition(table, previousValue);
        if (JsonNode.DeepEquals(oldJson, newJson)) { return; }

        result.Add(("sortdefinition", newJson));
    }

    private static void DiffUniqueValues(Table table, string previousValue, string newValue, List<(string Path, JsonNode? Value)> result) {
        var oldItems = ParseUniqueValues(table, previousValue);
        var newItems = ParseUniqueValues(table, newValue);

        if (!SameKeys(oldItems.Keys, newItems.Keys)) {
            result.Add(("uniquevalues", ItemsToJsonArray(newItems.Values)));
            return;
        }

        foreach (var (key, newJson) in newItems) {
            if (!JsonNode.DeepEquals(oldItems[key], newJson)) {
                result.Add(($"uniquevalues[{key}]", newJson.DeepClone()));
            }
        }
    }

    private static void DiffVariables(string previousValue, string newValue, List<(string Path, JsonNode? Value)> result) {
        var oldItems = ParseVariables(previousValue);
        var newItems = ParseVariables(newValue);

        if (!SameKeys(oldItems.Keys, newItems.Keys)) {
            var obj = new JsonObject { ["variables"] = ItemsToJsonArray(newItems.Values) };
            result.Add(("variables", obj));
            return;
        }

        foreach (var (key, newJson) in newItems) {
            if (!JsonNode.DeepEquals(oldItems[key], newJson)) {
                result.Add(($"variables[{key}]", newJson.DeepClone()));
            }
        }
    }

    private static JsonArray ArrangementsToJsonArray(List<ColumnViewCollection> items) {
        JsonArray array = [];
        foreach (var item in items) { array.Add(item.ParseableJson()); }
        return array;
    }

    private static Dictionary<string, JsonObject> ColumnsByName(JsonObject arrangementJson) {
        Dictionary<string, JsonObject> result = new(StringComparer.OrdinalIgnoreCase);
        if (arrangementJson["columns"] is not JsonArray arr) { return result; }

        foreach (var item in arr) {
            if (item is not JsonObject jo) { continue; }
            var key = jo.GetString("columnname", string.Empty);
            if (key is not { Length: > 0 }) { continue; }
            result.TryAdd(key, jo);
        }

        return result;
    }

    private static (string Path, JsonNode? Value) GetFullSync(Table table, TableDataType type, string newValue) => type switch {
        TableDataType.EventScript => ("eventscript", ItemsToJsonArray(ParseScripts(table, newValue).Values)),
        TableDataType.SortDefinition => ("sortdefinition", ParseSortDefinition(table, newValue)),
        TableDataType.ColumnArrangement => ("columnarrangements", ArrangementsToJsonArray(ParseArrangements(table, newValue))),
        TableDataType.UniqueValues => ("uniquevalues", ItemsToJsonArray(ParseUniqueValues(table, newValue).Values)),
        _ => ("variables", new JsonObject { ["variables"] = ItemsToJsonArray(ParseVariables(newValue).Values) })
    };

    private static JsonArray ItemsToJsonArray(IEnumerable<JsonObject> items) {
        JsonArray array = [];
        foreach (var item in items) { array.Add(item.DeepClone()); }
        return array;
    }

    private static List<ColumnViewCollection> ParseArrangements(Table table, string value) {
        List<ColumnViewCollection> result = [];
        if (string.IsNullOrWhiteSpace(value)) { return result; }

        foreach (var t in value.SplitAndCutByCr()) {
            if (string.IsNullOrWhiteSpace(t)) { continue; }
            result.Add(new ColumnViewCollection(table, t));
        }

        return result;
    }

    private static Dictionary<string, JsonObject> ParseScripts(Table table, string value) {
        Dictionary<string, JsonObject> result = new(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(value)) { return result; }

        foreach (var t in value.SplitAndCutByCr()) {
            if (string.IsNullOrWhiteSpace(t)) { continue; }
            var item = new TableScriptDescription(table, t);
            result.TryAdd(item.KeyName, item.ParseableJson());
        }

        return result;
    }

    private static JsonObject? ParseSortDefinition(Table table, string value) {
        if (string.IsNullOrWhiteSpace(value)) { return null; }
        return new RowSortDefinition(table, value).ParseableJson();
    }

    private static Dictionary<string, JsonObject> ParseUniqueValues(Table table, string value) {
        Dictionary<string, JsonObject> result = new(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(value)) { return result; }

        foreach (var t in value.SplitAndCutByCr()) {
            if (string.IsNullOrWhiteSpace(t)) { continue; }
            var item = new UniqueValueDefinition(table, t);
            result.TryAdd(item.KeyName, item.ParseableJson());
        }

        return result;
    }

    private static Dictionary<string, JsonObject> ParseVariables(string value) {
        Dictionary<string, JsonObject> result = new(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(value)) { return result; }

        foreach (var v in VariableCollection.ParseVariable(value, true)) {
            result.TryAdd(v.KeyName, v.ParseableJson());
        }

        return result;
    }

    private static bool SameKeys(Dictionary<string, JsonObject>.KeyCollection oldKeys, Dictionary<string, JsonObject>.KeyCollection newKeys) =>
        oldKeys.Count == newKeys.Count && oldKeys.All(newKeys.Contains);

    #endregion
}

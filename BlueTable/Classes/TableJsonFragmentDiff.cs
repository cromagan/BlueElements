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

            // Bei Umbenennung der Ansicht index-basierte Pfade verwenden: Die
            // Fremd-Instanz kennt den neuen Namen erst nach dem Einspielen der
            // Name-Zeile — Folgelines mit dem neuen Namen würden ins Leere laufen.
            var viewKey = string.Equals(oldItems[i].KeyName, newItems[i].KeyName, StringComparison.OrdinalIgnoreCase)
                ? ArrangementKey(newItems[i], i)
                : i.ToString1();

            var oldColumns = ColumnsByName(oldJson);
            var newColumns = ColumnsByName(newJson);

            if (!SameKeys(oldColumns.Keys, newColumns.Keys) || !SameColumnOrder(oldJson, newJson)) {
                // Spalten dieser Ansicht hinzugefügt/entfernt/umsortiert —
                // nur das Spalten-Array dieser Ansicht synchronisieren statt der
                // ganzen Ansicht. ParseJson ersetzt die Spaltenliste damit
                // vollständig (inkl. Reihenfolge und Spalten-Details).
                result.Add(($"columnarrangements[{viewKey}].columns", newJson["columns"]?.DeepClone() ?? new JsonArray()));
                continue;
            }

            // Schlüsselbestand einer einzelnen Ansichtsspalte geändert (z. B.
            // wegfallendes "isexpanded" beim Aufklappen): Der neue Stand ist
            // nicht prop-weise ausdrückbar — Spaltenliste synchronisieren.
            var columnKeysDiverged = newColumns.Any(c => !SameJsonKeys(oldColumns[c.Key], c.Value));

            if (columnKeysDiverged) {
                result.Add(($"columnarrangements[{viewKey}].columns", newJson["columns"]?.DeepClone() ?? new JsonArray()));
                continue;
            }

            foreach (var prop in newJson) {
                if (prop.Key == "columns") { continue; }
                if (JsonNode.DeepEquals(prop.Value, oldJson[prop.Key])) { continue; }
                result.Add(($"columnarrangements[{viewKey}].{prop.Key}", prop.Value?.DeepClone() ?? JsonValue.Create(string.Empty)));
            }

            // Verschollene Array-Keys explizit leer senden (z. B.
            // "permissiongroups"), damit das Einspielen den alten Stand nicht erhält.
            foreach (var oldProp in oldJson) {
                if (oldProp.Value is JsonArray && newJson[oldProp.Key] is null) {
                    result.Add(($"columnarrangements[{viewKey}].{oldProp.Key}", new JsonArray()));
                }
            }

            foreach (var (colKey, colJson) in newColumns) {
                var oldColJson = oldColumns[colKey];
                foreach (var prop in colJson) {
                    if (JsonNode.DeepEquals(prop.Value, oldColJson[prop.Key])) { continue; }
                    result.Add(($"columnarrangements[{viewKey}].columns[{colKey}].{prop.Key}", prop.Value?.DeepClone() ?? JsonValue.Create(string.Empty)));
                }
            }
        }
    }

    /// <summary>
    /// Pfad-Key einer Ansicht: der Ansichtsname, wenn vorhanden (robust gegen
    /// unterschiedliche Ansicht-Reihenfolgen bei Multi-User), sonst der Index.
    /// </summary>
    private static string ArrangementKey(ColumnViewCollection view, int index) =>
        view.KeyName is { Length: > 0 } name ? name : index.ToString1();

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
                if (string.Equals(prop.Key, "savedvariables", StringComparison.OrdinalIgnoreCase)) {
                    DiffSavedVariables(key, oldJson, prop.Value, result);
                    continue;
                }

                if (JsonNode.DeepEquals(prop.Value, oldJson[prop.Key])) { continue; }
                result.Add(($"eventscript[{key}].{prop.Key}", prop.Value?.DeepClone() ?? JsonValue.Create(string.Empty)));
            }

            // Verschollene Array-Keys explizit leer senden (z. B. "usergroups"),
            // damit das Einspielen den alten Stand nicht erhält.
            foreach (var oldProp in oldJson) {
                if (oldProp.Value is JsonArray && newJson[oldProp.Key] is null) {
                    result.Add(($"eventscript[{key}].{oldProp.Key}", new JsonArray()));
                }
            }
        }
    }

    /// <summary>
    /// Vergleicht die gespeicherten Variablen eines Skripts verschachtelt:
    /// Der ReadOnly-Flag und jedes Variablen-Attribut erhalten eine eigene
    /// Pfad-Zeile. Nur wenn Variablen hinzugefügt/entfernt wurden oder der
    /// Schlüsselbestand einzelner Variablen wechselt, wird ein größerer
    /// Ausschnitt (Variablen-Array bzw. das komplette Objekt) synchronisiert.
    /// </summary>
    private static void DiffSavedVariables(string scriptKey, JsonObject oldScriptJson, JsonNode? newSavedNode, List<(string Path, JsonNode? Value)> result) {
        var prefix = $"eventscript[{scriptKey}].savedvariables";

        if (newSavedNode is not JsonObject newSaved || oldScriptJson["savedvariables"] is not JsonObject oldSaved) {
            result.Add((prefix, newSavedNode?.DeepClone() ?? new JsonObject()));
            return;
        }

        if (!JsonNode.DeepEquals(newSaved["readonly"], oldSaved["readonly"])) {
            result.Add(($"{prefix}.readonly", newSaved["readonly"]?.DeepClone() ?? JsonValue.Create(false)));
        }

        var oldVars = VariablesByName(oldSaved);
        var newVars = VariablesByName(newSaved);

        if (!SameKeys(oldVars.Keys, newVars.Keys)) {
            result.Add(($"{prefix}.variables", VariablesToJsonArray(newSaved)));
            return;
        }

        foreach (var (varKey, newVarJson) in newVars) {
            var oldVarJson = oldVars[varKey];

            // Schlüsselbestand der Variable geändert (z. B. Typwechsel mit
            // anderem Wert-Key) — nur dann die komplette Variable senden.
            if (!SameJsonKeys(oldVarJson, newVarJson)) {
                result.Add(($"{prefix}.variables[{varKey}]", newVarJson.DeepClone()));
                continue;
            }

            foreach (var prop in newVarJson) {
                if (JsonNode.DeepEquals(prop.Value, oldVarJson[prop.Key])) { continue; }
                result.Add(($"{prefix}.variables[{varKey}].{prop.Key}", prop.Value?.DeepClone() ?? JsonValue.Create(string.Empty)));
            }
        }

        // "variables" als Ganzes verschwunden (leerer Variablenstand): explizit
        // leer senden, sonst erhält das Einspielen den alten Variablenstand.
        if (newSaved["variables"] is null && oldSaved["variables"] is not null) {
            result.Add(($"{prefix}.variables", new JsonArray()));
        }
    }

    private static void DiffSortDefinition(Table table, string previousValue, string newValue, List<(string Path, JsonNode? Value)> result) {
        var newJson = ParseSortDefinition(table, newValue);
        if (newJson is null) { return; }

        var oldJson = ParseSortDefinition(table, previousValue);

        if (oldJson is null) {
            // Kein vorheriger Stand — komplette Definition als Partial-Objekt der Tabelle.
            result.Add(("sortdefinition", newJson));
            return;
        }

        foreach (var prop in newJson) {
            if (!JsonNode.DeepEquals(prop.Value, oldJson[prop.Key])) {
                // Key "x" wird beim Einspielen ignoriert — Table.GetSubItemByKey
                // liefert für den Container SortDefinition stets die eine Instanz.
                result.Add(($"sortdefinition[x].{prop.Key}", prop.Value?.DeepClone() ?? JsonValue.Create(string.Empty)));
            }
        }

        // Verschollene Keys explizit leeren: ParseJson überspringt fehlende Keys,
        // ein weggefallenes "columns" würde den alten Stand sonst erhalten.
        if (newJson["columns"] is null && oldJson["columns"] is not null) {
            result.Add(("sortdefinition[x].columns", new JsonArray()));
        }
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

        // Ein Typwechsel kann am bestehenden Variablen-Objekt nicht ausgedrückt
        // werden (der Typ ist beim Einspielen fest) — dann die komplette Liste.
        var typeChanged = newItems.Any(v => !string.Equals(
            oldItems[v.Key].GetString("type", string.Empty),
            v.Value.GetString("type", string.Empty),
            StringComparison.OrdinalIgnoreCase));

        if (typeChanged) {
            var obj = new JsonObject { ["variables"] = ItemsToJsonArray(newItems.Values) };
            result.Add(("variables", obj));
            return;
        }

        foreach (var (key, newJson) in newItems) {
            var oldJson = oldItems[key];
            foreach (var prop in newJson) {
                if (JsonNode.DeepEquals(prop.Value, oldJson[prop.Key])) { continue; }
                result.Add(($"variables[{key}].{prop.Key}", prop.Value?.DeepClone() ?? JsonValue.Create(string.Empty)));
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

    /// <summary>
    /// True, wenn beide JSON-Objekte denselben Schlüsselbestand haben. Ein
    /// unterscheidlicher Bestand ist nicht prop-weise übertragbar.
    /// </summary>
    private static bool SameJsonKeys(JsonObject oldJson, JsonObject newJson) {
        if (oldJson.Count != newJson.Count) { return false; }

        foreach (var prop in oldJson) {
            if (newJson[prop.Key] is null) { return false; }
        }

        return true;
    }

    /// <summary>
    /// Löst die Variablen eines savedvariables-JSON über ihren Schlüssel auf.
    /// </summary>
    private static Dictionary<string, JsonObject> VariablesByName(JsonObject savedJson) {
        Dictionary<string, JsonObject> result = new(StringComparer.OrdinalIgnoreCase);
        if (savedJson["variables"] is not JsonArray arr) { return result; }

        foreach (var item in arr) {
            if (item is not JsonObject jo) { continue; }
            var key = jo.GetString("key", string.Empty);
            if (key is not { Length: > 0 }) { continue; }
            result.TryAdd(key, jo);
        }

        return result;
    }

    /// <summary>
    /// Kopiert das Variablen-Array eines savedvariables-JSON (leer, wenn
    /// nicht vorhanden) — für die Komplett-Sync-Zeile der Variablen.
    /// </summary>
    private static JsonArray VariablesToJsonArray(JsonObject savedJson) {
        JsonArray array = [];
        if (savedJson["variables"] is not JsonArray arr) { return array; }

        foreach (var item in arr) {
            array.Add(item?.DeepClone());
        }

        return array;
    }

    private static (string Path, JsonNode? Value) GetFullSync(Table table, TableDataType type, string newValue) => type switch {
        TableDataType.EventScript => ("eventscript", ItemsToJsonArray(ParseScripts(table, newValue).Values)),
        // Eine geleerte/strukturlose Sortierung muss als anwendbares Objekt ankommen —
        // null würde beim Fremd-Replay als No-Op durchlaufen und den alten Stand erhalten.
        TableDataType.SortDefinition => ("sortdefinition", ParseSortDefinition(table, newValue) ?? new JsonObject { ["reverse"] = false }),
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

    /// <summary>
    /// Vergleicht die Reihenfolge der Spaltennamen im "columns"-Array beider Stände.
    /// Die Reihenfolge steckt in keinem einzelnen Property und würde sonst beim
    /// Einspielen prop-weiser Zeilen verloren gehen.
    /// </summary>
    private static bool SameColumnOrder(JsonObject oldJson, JsonObject newJson) {
        if (oldJson["columns"] is not JsonArray oldArr) { return newJson["columns"] is not JsonArray; }
        if (newJson["columns"] is not JsonArray newArr) { return false; }

        if (oldArr.Count != newArr.Count) { return false; }

        for (var i = 0; i < oldArr.Count; i++) {
            var o = oldArr[i]?.AsObject().GetString("columnname", string.Empty);
            var n = newArr[i]?.AsObject().GetString("columnname", string.Empty);

            if (!string.Equals(o, n, StringComparison.OrdinalIgnoreCase)) { return false; }
        }

        return true;
    }

    #endregion
}

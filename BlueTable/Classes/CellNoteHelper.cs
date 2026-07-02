// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueTable.Classes;

public static class CellNoteHelper {

    #region Fields

    private const string KeySymbol = "Symbol";
    private const string KeyText = "Text";

    #endregion

    #region Methods

    /// <summary>
    /// Liefert die Notiz für <paramref name="column" /> in <paramref name="row" />,
    /// oder <c>null</c>, wenn keine vorhanden ist.
    /// </summary>
    public static (NoteSymbols Symbol, string Text)? GetNoteData(ColumnItem column, RowItem row) {
        if (GetNoteColumn(column) is not { } noteColumn) { return null; }

        var cellValue = row.CellGetString(noteColumn);
        if (cellValue is not { Length: > 0 }) { return null; }

        foreach (var (keyName, symbol, text) in ParseAllNotes(cellValue)) {
            if (string.Equals(keyName, column.KeyName, StringComparison.OrdinalIgnoreCase)) {
                return (symbol, text);
            }
        }
        return null;
    }

    /// <summary>
    /// Zerlegt den Rohwert der Notiz-Spalte in einzelne Einträge. Das Format ist
    /// ein JSON-Objekt, das jeder Column-KeyName auf ein { Symbol, Text }-Objekt
    /// abbildet. Zeilenumbrüche im Text werden durch die JSON-Kodierung erhalten.
    /// Beschädigte oder leere Werte liefern eine leere Liste.
    /// </summary>
    public static List<(string KeyName, NoteSymbols Symbol, string Text)> ParseAllNotes(string content) {
        var result = new List<(string, NoteSymbols, string)>();
        if (content is not { Length: > 0 }) { return result; }

        JsonNode? root;
        try {
            root = JsonNode.Parse(content);
        } catch (JsonException) {
            return result;
        }

        if (root is not JsonObject obj) { return result; }

        foreach (var kvp in obj) {
            if (string.IsNullOrEmpty(kvp.Key)) { continue; }
            if (kvp.Value is not JsonObject entry) { continue; }

            var symbol = ParseSymbol(entry.GetString(KeySymbol));
            var text = entry.GetString(KeyText);
            if (string.IsNullOrEmpty(text)) { continue; }

            result.Add((kvp.Key, symbol, text));
        }

        return result;
    }

    public static void RemoveNote(ColumnItem column, RowItem row) {
        if (GetNoteColumn(column) is not { } noteColumn) { return; }

        var cellValue = row.CellGetString(noteColumn);
        if (cellValue is not { Length: > 0 }) { return; }

        var entries = ParseAllNotes(cellValue);
        var removed = entries.RemoveAll(e => string.Equals(e.KeyName, column.KeyName, StringComparison.OrdinalIgnoreCase));
        if (removed == 0) { return; }

        row.CellSet(noteColumn, BuildJson(entries), "Notiz entfernt");
    }

    public static void SetNote(ColumnItem column, RowItem row, NoteSymbols symbol, string text) {
        if (GetNoteColumn(column) is not { } noteColumn) { return; }

        if (string.IsNullOrEmpty(text)) {
            RemoveNote(column, row);
            return;
        }

        var cellValue = row.CellGetString(noteColumn);
        var entries = ParseAllNotes(cellValue);

        var replaced = false;
        for (var i = 0; i < entries.Count; i++) {
            if (string.Equals(entries[i].KeyName, column.KeyName, StringComparison.OrdinalIgnoreCase)) {
                entries[i] = (entries[i].KeyName, symbol, text);
                replaced = true;
                break;
            }
        }

        if (!replaced) { entries.Add((column.KeyName, symbol, text)); }

        row.CellSet(noteColumn, BuildJson(entries), "Notiz setzen");
    }

    private static string BuildJson(List<(string KeyName, NoteSymbols Symbol, string Text)> entries) {
        if (entries.Count == 0) { return string.Empty; }

        JsonObject obj = new();
        foreach (var (keyName, symbol, text) in entries) {
            obj[keyName] = new JsonObject()
                .Set(KeySymbol, symbol.ToString())
                .Set(KeyText, text);
        }
        return obj.ToJsonString();
    }

    private static ColumnItem? GetNoteColumn(ColumnItem column) {
        if (column.Table is not { IsDisposed: false } tb) { return null; }
        return tb.Column.SysCellNote is { IsDisposed: false } nc ? nc : null;
    }

    private static NoteSymbols ParseSymbol(string value) {
        if (Enum.TryParse<NoteSymbols>(value, true, out var result)) { return result; }

        switch (value) {
            case "Kritisch": return NoteSymbols.Critical;
            case "Warnung": return NoteSymbols.Warning;
            case "Häkchen": return NoteSymbols.Ok;
            case "Stift": return NoteSymbols.Pencil;
            default: return NoteSymbols.Pencil;
        }
    }

    #endregion
}

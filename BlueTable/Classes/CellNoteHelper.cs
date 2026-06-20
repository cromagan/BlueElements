// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueTable.Classes;

public static class CellNoteHelper {

    #region Methods

    public static (NoteSymbols Symbol, string Text)? GetNoteData(ColumnItem column, RowItem row) {
        if (GetNoteColumn(column) is not { } noteColumn) { return null; }

        var cellValue = row.CellGetString(noteColumn);
        if (cellValue is not { Length: > 0 }) { return null; }

        var data = "\r" + cellValue;
        var (idx, contentStart, end) = FindEntryBounds(data, column.KeyName);
        if (idx < 0) { return null; }

        var entry = end < 0 ? data[contentStart..] : data[contentStart..end];
        var parts = entry.Split('|');
        return parts.Length < 2 ? null : (ParseSymbol(parts[0]), parts[1]);
    }

    public static void RemoveNote(ColumnItem column, RowItem row) {
        if (GetNoteColumn(column) is not { } noteColumn) { return; }

        var cellValue = row.CellGetString(noteColumn);
        if (cellValue is not { Length: > 0 }) { return; }

        var data = "\r" + cellValue;
        var (idx, _, end) = FindEntryBounds(data, column.KeyName);
        if (idx < 0) { return; }

        var remaining = end < 0 ? data[..idx] : data[..idx] + data[end..];
        row.CellSet(noteColumn, remaining.Length > 0 ? remaining[1..] : string.Empty, "Notiz entfernt");
    }

    public static void SetNote(ColumnItem column, RowItem row, NoteSymbols symbol, string text) {
        if (GetNoteColumn(column) is not { } noteColumn) { return; }

        if (string.IsNullOrEmpty(text)) {
            RemoveNote(column, row);
            return;
        }

        var cleanText = CleanForStorage(text);
        var cleanSymbol = CleanForStorage(symbol.ToString());
        var newEntry = $"{column.KeyName}|{cleanSymbol}|{cleanText}";

        var cellValue = row.CellGetString(noteColumn);
        var data = "\r" + cellValue;
        var (idx, _, end) = FindEntryBounds(data, column.KeyName);

        string newValue;
        if (idx < 0) {
            newValue = string.IsNullOrEmpty(cellValue) ? newEntry : cellValue + "\r" + newEntry;
        } else {
            var remaining = end < 0 ? data[..idx] : data[..idx] + data[end..];
            newValue = remaining.Length > 0 ? remaining[1..] + "\r" + newEntry : newEntry;
        }

        row.CellSet(noteColumn, newValue, "Notiz setzen");
    }

    private static ColumnItem? GetNoteColumn(ColumnItem column) {
        if (column.Table is not { IsDisposed: false } tb) { return null; }
        return tb.Column.SysCellNote is { IsDisposed: false } nc ? nc : null;
    }

    /// <summary>
    /// Sucht den Notiz-Eintrag für <paramref name="columnKeyName"/> in <paramref name="data"/>
    /// (das mit führendem \r vorbereitete cellValue). Liefert (-1, -1, -1) falls kein Eintrag
    /// existiert. Der zweite Wert (ContentStart) zeigt hinter das "key|"-Präfix, der dritte
    /// Wert (End) auf das nächste \r oder -1, wenn der Eintrag bis zum Ende reicht.
    /// </summary>
    private static (int Idx, int ContentStart, int End) FindEntryBounds(string data, string columnKeyName) {
        var search = $"\r{columnKeyName}|";
        var idx = data.IndexOf(search, StringComparison.OrdinalIgnoreCase);
        if (idx < 0) { return (-1, -1, -1); }
        var contentStart = idx + search.Length;
        return (idx, contentStart, data.IndexOf('\r', contentStart));
    }

    private static string CleanForStorage(string value) {
        if (string.IsNullOrEmpty(value)) { return value; }
        return value
            .Replace("\r", "")
            .Replace("\n", "")
            .Replace("|", "");
    }

    private static NoteSymbols ParseSymbol(string value) {
        if (Enum.TryParse<NoteSymbols>(value, true, out var result)) { return result; }
        return value switch {
            "Kritisch" => NoteSymbols.Critical,
            "Warnung" => NoteSymbols.Warning,
            "Häkchen" => NoteSymbols.Ok,
            "Stift" => NoteSymbols.Pencil,
            _ => NoteSymbols.Pencil
        };
    }

    #endregion
}

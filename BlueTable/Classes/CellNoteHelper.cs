// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueTable.Classes;

public static class CellNoteHelper {

    #region Methods

    public static (string Symbol, string Text)? GetNoteData(ColumnItem column, RowItem row) {
        if (column.Table is not { IsDisposed: false } tb) { return null; }
        if (tb.Column.SysCellNote is not { IsDisposed: false } noteColumn) { return null; }

        var cellValue = row.CellGetString(noteColumn);
        if (string.IsNullOrEmpty(cellValue)) { return null; }

        var search = $"\r{column.KeyName}|";
        var idx = ("\r" + cellValue).IndexOf(search, StringComparison.OrdinalIgnoreCase);
        if (idx < 0) { return null; }

        var start = idx + search.Length;
        var end = ("\r" + cellValue).IndexOf('\r', start);
        var entry = end < 0 ? ("\r" + cellValue)[start..] : ("\r" + cellValue)[start..end];

        var parts = entry.Split('|');
        if (parts.Length < 2) { return null; }

        return (parts[0], parts[1]);
    }

    public static void RemoveNote(ColumnItem column, RowItem row) {
        if (column.Table is not { IsDisposed: false } tb) { return; }
        if (tb.Column.SysCellNote is not { IsDisposed: false } noteColumn) { return; }

        var cellValue = row.CellGetString(noteColumn);
        if (string.IsNullOrEmpty(cellValue)) { return; }

        var search = $"\r{column.KeyName}|";
        var data = "\r" + cellValue;
        var idx = data.IndexOf(search, StringComparison.OrdinalIgnoreCase);
        if (idx < 0) { return; }

        var end = data.IndexOf('\r', idx + search.Length);
        var remaining = end < 0 ? data[..idx] : data[..idx] + data[end..];
        row.CellSet(noteColumn, remaining.Length > 0 ? remaining[1..] : string.Empty, "Notiz entfernt");
    }

    public static void SetNote(ColumnItem column, RowItem row, string symbol, string text) {
        if (column.Table is not { IsDisposed: false } tb) { return; }
        if (tb.Column.SysCellNote is not { IsDisposed: false } noteColumn) { return; }

        if (string.IsNullOrEmpty(text)) {
            RemoveNote(column, row);
            return;
        }

        var cleanText = CleanForStorage(text);
        var cleanSymbol = CleanForStorage(symbol);
        var newEntry = $"{column.KeyName}|{cleanSymbol}|{cleanText}";

        var cellValue = row.CellGetString(noteColumn);
        var data = "\r" + cellValue;
        var search = $"\r{column.KeyName}|";
        var idx = data.IndexOf(search, StringComparison.OrdinalIgnoreCase);

        string newValue;
        if (idx < 0) {
            newValue = string.IsNullOrEmpty(cellValue) ? newEntry : cellValue + "\r" + newEntry;
        } else {
            var end = data.IndexOf('\r', idx + search.Length);
            var remaining = end < 0 ? data[..idx] : data[..idx] + data[end..];
            newValue = remaining.Length > 0 ? remaining[1..] + "\r" + newEntry : newEntry;
        }

        row.CellSet(noteColumn, newValue, "Notiz setzen");
    }

    private static string CleanForStorage(string value) {
        if (string.IsNullOrEmpty(value)) { return value; }
        return value
            .Replace("\r", "")
            .Replace("\n", "")
            .Replace("|", "");
    }

    #endregion
}

// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueTable.Classes;

public static class CellNoteHelper {

    #region Methods

    public static (string Symbol, string Text)? GetNoteData(ColumnItem column, RowItem row) {
        if (column.Table is not { IsDisposed: false } tb) { return null; }
        if (tb.Column.SysCellNote is not { IsDisposed: false } noteColumn) { return null; }

        var cellValue = row.CellGetString(noteColumn);
        if (string.IsNullOrEmpty(cellValue)) { return null; }

        var prefix = $"\r{column.KeyName}|";
        var idx = cellValue.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
        if (idx < 0) { return null; }

        var start = idx + prefix.Length;
        var end = cellValue.IndexOf('\r', start);
        var entry = end < 0 ? cellValue[start..] : cellValue[start..end];

        var parts = entry.Split('|');
        if (parts.Length < 2) { return null; }

        return (parts[0], parts[1]);
    }

    public static void RemoveNote(ColumnItem column, RowItem row) {
        if (column.Table is not { IsDisposed: false } tb) { return; }
        if (tb.Column.SysCellNote is not { IsDisposed: false } noteColumn) { return; }

        var cellValue = row.CellGetString(noteColumn);
        if (string.IsNullOrEmpty(cellValue)) { return; }

        var prefix = $"\r{column.KeyName}|";
        var idx = cellValue.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
        if (idx < 0) { return; }

        var end = cellValue.IndexOf('\r', idx + prefix.Length);
        var newValue = end < 0 ? cellValue[..idx] : cellValue[..idx] + cellValue[end..];
        row.CellSet(noteColumn, newValue, "Notiz entfernt");
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

        var cellValue = row.CellGetString(noteColumn);
        var prefix = $"\r{column.KeyName}|";
        var idx = cellValue?.IndexOf(prefix, StringComparison.OrdinalIgnoreCase) ?? -1;

        var newEntry = $"{column.KeyName}|{cleanSymbol}|{cleanText}";
        var newValue = idx < 0
            ? (string.IsNullOrEmpty(cellValue) ? newEntry : cellValue + "\r" + newEntry)
            : cellValue[..idx] + "\r" + newEntry + cellValue[(cellValue.IndexOf('\r', idx + prefix.Length) < 0 ? cellValue.Length : cellValue.IndexOf('\r', idx + prefix.Length))..];

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

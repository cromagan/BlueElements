// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueTable.Tests;

/// <summary>
/// Erwartungsmodell der Test-Tabelle; die Änderungsphasen pflegen es fort und die Inhaltsprüfung vergleicht danach.
/// </summary>
public sealed class ExpectedContent {

    #region Fields

    private readonly bool withChunk;

    private readonly List<ExpectedRow> rows = [];

    #endregion

    #region Constructors

    /// <summary>
    /// Baut die Erwartungen der Befüllung: wert1..wertN, Menge i*10, Notiz mit Sonderzeichen, Chunk cb0..cb2.
    /// </summary>
    public ExpectedContent(int rowCount, bool withChunkColumn) {
        withChunk = withChunkColumn;

        for (var i = 1; i <= rowCount; i++) {
            rows.Add(new ExpectedRow("wert" + i.ToString1(), (i * 10).ToString1(), TableFactory.NotizValue(i, rowCount), TableFactory.ChunkValue(i)));
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Trägt eine neue Zeile in die Erwartungen ein.
    /// </summary>
    public void AddRow(string name, string menge, string notiz, string chunk) => rows.Add(new ExpectedRow(name, menge, notiz, chunk));

    /// <summary>
    /// Entfernt eine Zeile aus den Erwartungen.
    /// </summary>
    public void RemoveRow(string name) {
        for (var i = rows.Count - 1; i >= 0; i--) {
            if (string.Equals(rows[i].Name, name, StringComparison.Ordinal)) { rows.RemoveAt(i); }
        }
    }

    /// <summary>
    /// Ändert einen erwarteten Zellwert (menge oder notiz).
    /// </summary>
    public void ChangeValue(string name, string columnKey, string value) {
        foreach (var row in rows) {
            if (!string.Equals(row.Name, name, StringComparison.Ordinal)) { continue; }

            switch (columnKey) {
                case "menge":
                    row.Menge = value;
                    return;
                case "notiz":
                    row.Notiz = value;
                    return;
                default:
                    throw new InvalidOperationException("Unbekannte Spalte in den Erwartungen: " + columnKey);
            }
        }

        throw new InvalidOperationException("Zeile in den Erwartungen nicht gefunden: " + name);
    }

    /// <summary>
    /// Prüft die geladene Tabelle gegen die Erwartungen; die Liste enthält jede Abweichung mit Zeile, Spalte und Wert.
    /// </summary>
    public List<string> Check(Table tbl) {
        List<string> problems = [];

        var c1 = tbl.Column["name"];
        var c2 = tbl.Column["menge"];
        var c3 = tbl.Column["notiz"];
        var cc = tbl.Column["chunk"];

        if (c1 is null || c2 is null || c3 is null) {
            problems.Add("Basis-Spalten fehlen nach dem Laden.");
            return problems;
        }

        if (withChunk && cc is null) {
            problems.Add("Chunk-Spalte fehlt nach dem Laden.");
            return problems;
        }

        if (tbl.Row.Count != rows.Count) {
            problems.Add("Zeilenanzahl " + tbl.Row.Count.ToString1() + ", erwartet " + rows.Count.ToString1());
        }

        foreach (var expectedRow in rows) {
            var row = TableFactory.FindRow(tbl, c1, expectedRow.Name, withChunk ? expectedRow.Chunk : null);

            if (row is not { IsDisposed: false }) {
                problems.Add("Zeile fehlt: " + expectedRow.Name);
                continue;
            }

            TableFactory.CompareCell(row, c2, expectedRow.Menge, "menge", expectedRow.Name, problems);
            TableFactory.CompareCell(row, c3, expectedRow.Notiz, "notiz", expectedRow.Name, problems);

            if (withChunk && cc is not null) { TableFactory.CompareCell(row, cc, expectedRow.Chunk, "chunk", expectedRow.Name, problems); }
        }

        return problems;
    }

    #endregion
}

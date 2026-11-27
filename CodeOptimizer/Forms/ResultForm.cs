// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Windows.Forms;
using BlueTable.Classes;
using Form = BlueControls.Forms.Form;

namespace CodeOptimizer.Forms;

/// <summary>
/// Zeigt den Bericht eines Optimizer-Laufs als Tabelle: je Zeile Modul, Art und Meldung.
/// </summary>
public sealed partial class ResultForm : Form {

    #region Fields

    private readonly Table _table;

    #endregion

    #region Constructors

    /// <summary>
    /// Baut die Ergebnisansicht aus dem Textbericht der Module auf.
    /// </summary>
    public ResultForm(string report) {
        InitializeComponent();
        _table = BuildTable(report);
        tvwResult.Table = _table;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Gibt die In-Memory-Tabelle beim Schließen frei.
    /// </summary>
    protected override void OnFormClosing(FormClosingEventArgs e) {
        base.OnFormClosing(e);

        tvwResult.Table = null;
        _table.Dispose();
    }

    /// <summary>
    /// Erzeugt eine In-Memory-Tabelle mit einer Zeile je Berichtszeile.
    /// Kopfzeilen ('=== Modul ===') gruppieren, eingerückte Zeilen sind Funde.
    /// </summary>
    private static Table BuildTable(string report) {
        var tb = Table.Get();
        tb.Caption = "Ergebnis";
        tb.Column.GenerateAndAdd("Modul", "Modul", null, string.Empty);
        tb.Column.GenerateAndAdd("Art", "Art", null, string.Empty);
        tb.Column.GenerateAndAdd("Meldung", "Meldung", null, string.Empty);

        // Row.GenerateAndAdd braucht eine definierte Erstspalte; RepairAfterParse füllt den Zeiger Column.First.
        if (tb.Column["Modul"] is { IsDisposed: false } modulColumn) {
            modulColumn.IsFirst = true;
        }
        tb.RepairAfterParse();

        // Ansichten aufbauen: Ansicht 0 erhält alle Spalten, die Standard-Ansicht wird gefüllt —
        // eine leere Standard-Ansicht zeigt die TableView als „Ansicht nicht definiert“.
        var arrangements = tb.RepairedArrangements();
        arrangements[1].ShowAllColumns();
        tb.ColumnArrangements = arrangements.AsReadOnly();

        var module = "Allgemein";

        foreach (var line in report.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries)) {
            var trimmed = line.Trim();

            if (trimmed.StartsWith("===", StringComparison.Ordinal) && trimmed.EndsWith("===", StringComparison.Ordinal) && trimmed.Length > 6) {
                module = trimmed.Trim('=').Trim();
                continue;
            }

            var art = line.StartsWith("  ", StringComparison.Ordinal) ? "Fund" : "Info";

            if (trimmed.StartsWith("Fehler", StringComparison.Ordinal) || trimmed.StartsWith("at ", StringComparison.Ordinal)) { art = "Fehler"; }

            var row = tb.Row.GenerateAndAdd(module, string.Empty);

            if (row is null) { continue; }

            row.CellSet("Art", art, string.Empty);
            row.CellSet("Meldung", trimmed, string.Empty);
        }

        return tb;
    }

    #endregion
}

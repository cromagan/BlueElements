// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueTable.ClassesStatic;

/// <summary>
/// Berechnet Ähnlichkeits-Scores (0–100) aller Zeilen einer Tabelle bezogen
/// auf eine Referenzzeile. Mehrzeilige Zellen werden über ihre Einträge,
/// Zahlen über den Abstand, Datumsangaben über den Tagesabstand und
/// einzeilige Texte über die Levenshtein-Distanz verglichen. Zahlen zählen
/// 5-fach, mehrzeilige Einträge 3-fach, Datumsangaben 2-fach und Texte
/// einfach. Die Referenzzeile ist identisch mit sich selbst und erhält
/// dadurch 100.
/// </summary>
public static class RowSimilarity {

    #region Methods

    /// <summary>
    /// Similarität zweier Zelleninhalte (0,0–1,0) gemäß dem Format der Spalte.
    /// Identische Werte erhalten 1,0, bei Groß-/Kleinschreibung
    /// identische Werte 0,9.
    /// </summary>
    public static double CellSimilarity(ColumnItem column, string valueReference, string value) {
        if (valueReference == value) { return 1.0; }
        if (valueReference.Length == 0 || value.Length == 0) { return 0.0; }
        if (string.Equals(valueReference, value, StringComparison.OrdinalIgnoreCase)) { return 0.9; }

        if (column.MultiLine) { return EntriesSimilarity(valueReference, value); }

        switch (column.SortType) {
            case SortierTyp.ZahlenwertInt:
            case SortierTyp.ZahlenwertFloat:
                return NumberSimilarity(DoubleParse(valueReference), DoubleParse(value));

            case SortierTyp.Datum_Uhrzeit:
                return DateSimilarity(DateTimeParse(valueReference), DateTimeParse(value));

            default:
                return TextSimilarity(valueReference, value);
        }
    }

    public static Dictionary<RowItem, int> Scores(Table table, RowItem referenceRow) {
        var result = new Dictionary<RowItem, int>();
        if (table.IsDisposed || referenceRow is not { IsDisposed: false }) { return result; }

        List<ColumnItem> columns = [];
        foreach (var thisColumn in table.Column) {
            if (thisColumn is { IsDisposed: false } && !thisColumn.IsSystemColumn()) { columns.Add(thisColumn); }
        }

        if (columns.Count == 0) {
            foreach (var thisRow in table.Row) {
                if (thisRow is { IsDisposed: false }) { result.Add(thisRow, thisRow == referenceRow ? 100 : 0); }
            }
            return result;
        }

        var referenceValues = new Dictionary<ColumnItem, string>();
        var weightSum = 0.0;
        foreach (var thisColumn in columns) {
            referenceValues.Add(thisColumn, referenceRow.CellGetString(thisColumn));
            weightSum += Weight(thisColumn);
        }

        foreach (var thisRow in table.Row) {
            if (thisRow is not { IsDisposed: false }) { continue; }

            var sum = 0.0;
            foreach (var thisColumn in columns) {
                sum += CellSimilarity(thisColumn, referenceValues[thisColumn], thisRow.CellGetString(thisColumn)) * Weight(thisColumn);
            }

            result.Add(thisRow, (int)Math.Round(100.0 * sum / weightSum));
        }

        return result;
    }

    /// <summary>
    /// Ähnlichkeit der Einträge zweier mehrzeiliger Zellen: gemittelte
    /// beste Übereinstimmung der Einträge in beide Richtungen.
    /// </summary>
    private static double AverageBestMatch(List<string> source, List<string> target) {
        var sum = 0.0;
        foreach (var thisSource in source) {
            var best = 0.0;
            foreach (var thisTarget in target) {
                var sim = TextSimilarity(thisSource, thisTarget);
                if (sim > best) { best = sim; }
            }
            sum += best;
        }
        return sum / source.Count;
    }

    /// <summary>
    /// Ähnlichkeit zweier Datumswerte: ein Jahr Abstand entspricht 0.
    /// </summary>
    private static double DateSimilarity(DateTime dateReference, DateTime date) {
        var days = Math.Abs((dateReference - date).TotalDays);
        return Math.Max(0.0, 1.0 - Math.Min(days / 365.0, 1.0));
    }

    private static List<string> Entries(string cellText) {
        List<string> result = [];
        foreach (var thisEntry in cellText.Split('\r')) {
            var t = thisEntry.Trim().ToUpperInvariant();
            if (t.Length > 0) { result.Add(t); }
        }
        return result;
    }

    private static double EntriesSimilarity(string valueReference, string value) {
        var entriesReference = Entries(valueReference);
        var entries = Entries(value);

        if (entriesReference.Count == 0 && entries.Count == 0) { return 1.0; }
        if (entriesReference.Count == 0 || entries.Count == 0) { return 0.0; }

        return (AverageBestMatch(entriesReference, entries) + AverageBestMatch(entries, entriesReference)) / 2.0;
    }

    /// <summary>
    /// Ähnlichkeit zweier Zahlen: der relative Abstand, identische Werte erhalten 1,0.
    /// </summary>
    private static double NumberSimilarity(double valueReference, double value) {
        var range = Math.Max(Math.Max(Math.Abs(valueReference), Math.Abs(value)), 1.0);
        return Math.Max(0.0, 1.0 - Math.Abs(valueReference - value) / range);
    }

    /// <summary>
    /// Ähnlichkeit zweier einzeiliger Texte: 1 - Levenshtein-Distanz / maximale Länge.
    /// </summary>
    private static double TextSimilarity(string valueReference, string value) {
        var upperReference = valueReference.ToUpperInvariant();
        var upperValue = value.ToUpperInvariant();
        var maxLength = Math.Max(upperReference.Length, upperValue.Length);
        if (maxLength == 0) { return 1.0; }
        return 1.0 - (double)LevenshteinDistance(upperReference, upperValue) / maxLength;
    }

    /// <summary>
    /// Gewicht der Spalte im Gesamtscore: Zahlen 5, mehrzeilige Einträge 3,
    /// Datum 2, Text 1.
    /// </summary>
    private static double Weight(ColumnItem column) {
        if (column.MultiLine) { return 3.0; }

        switch (column.SortType) {
            case SortierTyp.ZahlenwertInt:
            case SortierTyp.ZahlenwertFloat:
                return 5.0;

            case SortierTyp.Datum_Uhrzeit:
                return 2.0;

            default:
                return 1.0;
        }
    }

    #endregion
}
// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueTable.ClassesStatic;

/// <summary>
/// Berechnet Ähnlichkeits-Scores (0–100) aller Zeilen einer Tabelle bezogen
/// auf eine Referenzzeile. Mehrzeilige Zellen werden über ihre Einträge,
/// Zahlen über den Abstand, Datumsangaben über den Tagesabstand und
/// einzeilige Texte über die gemeinsamen Zeichen verglichen. Zahlen zählen
/// 5-fach, mehrzeilige Einträge 3-fach, Datumsangaben 2-fach und Texte
/// einfach, jeweils multipliziert mit dem Gewicht der Spalte
/// (ColumnItem.SimilarityWeight). Spalten mit Gewicht 0 werden ignoriert.
/// Die Referenzzeile ist identisch mit sich selbst und erhält
/// dadurch 100.
/// </summary>
public static class RowSimilarity {

    #region Methods

    /// <summary>
    /// Similarität zweier Zelleninhalte (0,0–1,0) gemäß dem Format der Spalte,
    /// Groß-/Kleinschreibung ignoriert. Unterscheiden sich die Werte nur in
    /// der Groß-/Kleinschreibung, wird das Ergebnis mit 0,9 multipliziert.
    /// </summary>
    public static double CellSimilarity(ColumnItem column, string valueReference, string value) {
        if (valueReference == value) { return 1.0; }
        if (valueReference.Length == 0 || value.Length == 0) { return 0.0; }

        double sim;
        if (column.MultiLine) {
            sim = EntriesSimilarity(valueReference, value);
        } else {
            switch (column.SortType) {
                case SortierTyp.ZahlenwertInt:
                case SortierTyp.ZahlenwertFloat:
                    sim = NumberSimilarity(DoubleParse(valueReference), DoubleParse(value));
                    break;

                case SortierTyp.Datum_Uhrzeit:
                    sim = DateSimilarity(DateTimeParse(valueReference), DateTimeParse(value));
                    break;

                default:
                    sim = TextSimilarity(valueReference, value);
                    break;
            }
        }

        return string.Equals(valueReference, value, StringComparison.OrdinalIgnoreCase) ? sim * 0.9 : sim;
    }

    /// <summary>
    /// Gesamtscore (0–100, ungerundet) je Zeile sowie Zell-Score
    /// (0–100, ungerundet) je Zelle der berücksichtigten Spalten. Spalten,
    /// die der Vergleich ignoriert (System-Spalten, erste Spalte),
    /// erhalten keinen Zell-Score.
    /// </summary>
    public static (Dictionary<RowItem, double> Scores, Dictionary<RowItem, Dictionary<ColumnItem, double>> CellScores) Scores(Table table, RowItem referenceRow) {
        var scores = new Dictionary<RowItem, double>();
        var cellScores = new Dictionary<RowItem, Dictionary<ColumnItem, double>>();
        if (table.IsDisposed || referenceRow is not { IsDisposed: false }) { return (scores, cellScores); }

        List<ColumnItem> columns = [];
        foreach (var thisColumn in table.Column) {
            if (thisColumn is { IsDisposed: false } &&
                !thisColumn.IsSystemColumn() &&
                !thisColumn.IsFirst &&
                thisColumn.SimilarityWeight > 0) { columns.Add(thisColumn); }
        }

        if (columns.Count == 0) {
            foreach (var thisRow in table.Row) {
                if (thisRow is { IsDisposed: false }) { scores.Add(thisRow, thisRow == referenceRow ? 100 : 0); }
            }
            return (scores, cellScores);
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
            Dictionary<ColumnItem, double> cells = new(columns.Count);
            foreach (var thisColumn in columns) {
                var sim = CellSimilarity(thisColumn, referenceValues[thisColumn], thisRow.CellGetString(thisColumn));
                sum += sim * Weight(thisColumn);
                cells.Add(thisColumn, sim * 100.0);
            }

            scores.Add(thisRow, 100.0 * sum / weightSum);
            cellScores.Add(thisRow, cells);
        }

        return (scores, cellScores);
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
            var t = thisEntry.Trim();
            if (t.Length > 0) { result.Add(t); }
        }
        return result;
    }

    /// <summary>
    /// Ähnlichkeit der Einträge zweier mehrzeiliger Zellen per 1:1-Zuordnung;
    /// fehlende und überzählige Einträge mindern das Ergebnis beidseitig gleich.
    /// </summary>
    private static double EntriesSimilarity(string valueReference, string value) {
        var entriesReference = Entries(valueReference);
        var entries = Entries(value);

        if (entriesReference.Count == 0 && entries.Count == 0) { return 1.0; }
        if (entriesReference.Count == 0 || entries.Count == 0) { return 0.0; }

        var matchedReference = new bool[entriesReference.Count];
        var matched = new bool[entries.Count];
        var sum = 0.0;

        for (var round = 0; round < Math.Min(entriesReference.Count, entries.Count); round++) {
            var bestSim = 0.0;
            var bestReferenceIndex = -1;
            var bestIndex = -1;
            for (var r = 0; r < entriesReference.Count; r++) {
                if (matchedReference[r]) { continue; }
                for (var i = 0; i < entries.Count; i++) {
                    if (matched[i]) { continue; }
                    var sim = TextSimilarity(entriesReference[r], entries[i]);
                    if (sim > bestSim) {
                        bestSim = sim;
                        bestReferenceIndex = r;
                        bestIndex = i;
                    }
                }
            }

            if (bestReferenceIndex < 0) { break; }

            matchedReference[bestReferenceIndex] = true;
            matched[bestIndex] = true;
            sum += bestSim;
        }

        return 2.0 * sum / (entriesReference.Count + entries.Count);
    }

    /// <summary>
    /// Ähnlichkeit zweier Zahlen: der relative Abstand, identische Werte erhalten 1,0.
    /// </summary>
    private static double NumberSimilarity(double valueReference, double value) {
        var range = Math.Max(Math.Max(Math.Abs(valueReference), Math.Abs(value)), 1.0);
        return Math.Max(0.0, 1.0 - Math.Abs(valueReference - value) / range);
    }

    /// <summary>
    /// Ähnlichkeit zweier Texte (0,0–1,0), Groß-/Kleinschreibung wird ignoriert:
    /// das Minimum aus dem Anteil der gemeinsamen Zeichen (Dice-Koeffizient)
    /// und dem Anteil der positionsgleichen Zeichen. Gleiche Buchstaben in
    /// anderer Reihenfolge werden dadurch schlechter bewertet.
    /// </summary>
    private static double TextSimilarity(string valueReference, string value) {
        var upperReference = valueReference.ToUpperInvariant();
        var upperValue = value.ToUpperInvariant();

        if (upperReference.Length == 0 && upperValue.Length == 0) { return 1.0; }
        if (upperReference.Length == 0 || upperValue.Length == 0) { return 0.0; }

        if (upperReference == upperValue) { return 1.0; }

        var charsReference = upperReference.ToCharArray();
        var charsValue = upperValue.ToCharArray();
        Array.Sort(charsReference);
        Array.Sort(charsValue);

        var common = 0;
        var i = 0;
        var j = 0;
        while (i < charsReference.Length && j < charsValue.Length) {
            if (charsReference[i] == charsValue[j]) {
                common++;
                i++;
                j++;
            } else if (charsReference[i] < charsValue[j]) {
                i++;
            } else {
                j++;
            }
        }

        var samePosition = 0;
        for (var p = 0; p < Math.Min(upperReference.Length, upperValue.Length); p++) {
            if (upperReference[p] == upperValue[p]) { samePosition++; }
        }

        var dice = 2.0 * common / (charsReference.Length + charsValue.Length);
        var position = (double)samePosition / Math.Max(upperReference.Length, upperValue.Length);

        return Math.Min(dice, position);
    }

    /// <summary>
    /// Gewicht der Spalte im Gesamtscore: das Spalten-Gewicht
    /// (ColumnItem.SimilarityWeight) multipliziert mit dem Format-Faktor
    /// (Zahlen 5, mehrzeilige Einträge 3, Datum 2, Text 1).
    /// </summary>
    private static double Weight(ColumnItem column) {
        var w = column.SimilarityWeight;

        if (column.MultiLine) { return 3.0 * w; }

        switch (column.SortType) {
            case SortierTyp.ZahlenwertInt:
            case SortierTyp.ZahlenwertFloat:
                return 5.0 * w;

            case SortierTyp.Datum_Uhrzeit:
                return 2.0 * w;

            default:
                return w;
        }
    }

    #endregion
}
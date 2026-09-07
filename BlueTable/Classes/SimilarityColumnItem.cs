// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Runtime.CompilerServices;

namespace BlueTable.Classes;

/// <summary>
/// Virtuelle Spalte der Ähnlichkeits-Suche: zeigt pro Zeile den Score
/// (0–100) des letzten „Ähnliche Zeilen“-Vergleichs der Tabelle. Hat kein
/// eigenes ColumnItem; die Scores sind Laufzeitzustand und verfallen mit
/// den Zeilen-Objekten. Die Spalte ist nur vorhanden, solange ein
/// Vergleich Scores geliefert hat.
/// </summary>
public sealed class SimilarityColumnItem : ColumnViewItem {

    #region Fields

    private static readonly ConditionalWeakTable<RowItem, StrongBox<int>> _scores = [];
    private static bool _hasScores;
    private static Table? _scoredTable;

    #endregion

    #region Constructors

    public SimilarityColumnItem() : base((Table?)null) { }

    #endregion

    #region Properties

    public static string ClassId => "VIR_SIMILARITY";

    public override AlignmentHorizontal Align => AlignmentHorizontal.Rechts;

    public override string Caption => "Score";

    public override int FixedWidth => 32;

    public override string? Renderer => "ColorRange";
    public override string RendererSettings => "{ClassId=\"ColorRange\", MinValue=0, MaxValue=100, MinColor=#FF0000, MaxColor=#00FF00}";
    public override SortierTyp SortType => SortierTyp.ZahlenwertInt;
    public override string? StorageKey => ClassId;

    #endregion

    #region Methods

    /// <summary>
    /// True, wenn für die Tabelle aktuell Scores vorliegen.
    /// </summary>
    public static bool HasScores(Table? table) => _hasScores && table is { IsDisposed: false } && _scoredTable == table;

    /// <summary>
    /// Entfernt alle Scores; die Zellen der Spalte werden leer.
    /// </summary>
    public static void Reset() {
        _scores.Clear();
        _scoredTable = null;
        _hasScores = false;
    }

    /// <summary>
    /// Speichert die Scores der Ähnlichkeits-Suche einer Tabelle.
    /// </summary>
    public static void SetScores(Table table, Dictionary<RowItem, int> scores) {
        _scores.Clear();
        _scoredTable = table;
        _hasScores = true;

        foreach (var thisPair in scores) {
            _scores.Add(thisPair.Key, new StrongBox<int>(thisPair.Value));
        }
    }

    /// <summary>
    /// Liefert den Score der Zeile als darzustellenden Text, ohne Treffer leer.
    /// </summary>
    public override string CellGetString(RowItem? row, bool isPinned) {
        if (row is not { IsDisposed: false } || _scoredTable != row.Table) { return string.Empty; }
        return _scores.TryGetValue(row, out var score) ? score.Value.ToString1() : string.Empty;
    }

    #endregion
}
// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Classes.FileSystemCaching;
using BlueControls.Classes;
using BlueControls.Controls;
using BlueTable.Classes;
using System.Windows.Forms;
using static BlueBasics.ClassesStatic.Develop;
using static BlueBasics.ClassesStatic.IO;

namespace BlueControls.BlueTableDialogs;

public sealed partial class ChunkInsight : FormWithStatusBar {

    #region Constructors

    public ChunkInsight() : base() =>
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();

    #endregion

    #region Methods

    [StandaloneInfo("Chunk-Insight", ImageCode.Puzzle, "Admin", "Rohe Chunk-Daten ohne Semantic-Parsing als Tabelle anzeigen", 900)]
    public static System.Windows.Forms.Form Start() => new ChunkInsight();

    protected override void OnFormClosing(FormClosingEventArgs e) {
        base.OnFormClosing(e);
        tblChunk.Table?.Dispose();
    }

    protected override void OnShown(System.EventArgs e) {
        base.OnShown(e);
        GenerateTable(tblChunk);
    }

    private static void FillTable(Table tb, byte[] data) {
        var nr = 1;
        var pointer = 0;

        try {
            while (pointer < data.Length) {
                var startPos = pointer;
                var (newPointer, type, value, colName, rowKey) = Table.Parse(data, pointer);

                if (newPointer <= pointer) { break; } // Schutz vor Endlosschleife bei Defekt

                pointer = newPointer;

                var r = tb.Row.GenerateAndAdd(nr.ToString1(), "Chunk-Insight");
                if (r is not { IsDisposed: false }) { continue; }

                r.CellSet("Position", startPos.ToString1(), string.Empty);
                r.CellSet("Routine", $"{(Routinen)data[startPos]} ({data[startPos]})", string.Empty);
                r.CellSet("DataType", $"{type} ({(int)type})", string.Empty);
                r.CellSet("ColumnKey", colName, string.Empty);
                r.CellSet("RowKey", rowKey, string.Empty);
                r.CellSet("Wert", value, string.Empty);

                nr++;

                if (type == TableDataType.EOF) { break; }
            }
        } catch (Exception ex) {
            DebugPrint($"Fehler beim Lesen des Chunks bei Position {pointer}", ex);
        }
    }

    private static void GenerateTable(TableViewWithFilters tableView) {
        var tb = Table.Get();

        var cNr = tb.Column.GenerateAndAdd("Nr", "Nr.", ColumnFormatHolder_LongOnlyPositive.Instance);
        if (cNr is { IsDisposed: false }) { cNr.IsFirst = true; }

        tb.Column.GenerateAndAdd("Position", "Position", ColumnFormatHolder_LongOnlyPositive.Instance);
        tb.Column.GenerateAndAdd("Routine", "Routine", ColumnFormatHolder_TextOneLine.Instance);
        tb.Column.GenerateAndAdd("DataType", "DataType", ColumnFormatHolder_TextOneLine.Instance);
        tb.Column.GenerateAndAdd("ColumnKey", "Spalten-<br>Schlüssel", ColumnFormatHolder_TextOneLine.Instance);
        tb.Column.GenerateAndAdd("RowKey", "Zeilen-<br>Schlüssel", ColumnFormatHolder_TextOneLine.Instance);
        tb.Column.GenerateAndAdd("Wert", "Wert", ColumnFormatHolder_TextOneLine.Instance);

        tb.Column.DisableAllEditing();

        foreach (var thisColumn in tb.Column) {
            if (!thisColumn.IsSystemColumn()) { thisColumn.MultiLine = true; }
        }

        tb.RepairAfterParse();

        var tcvc = ColumnViewCollection.ParseAll(tb);
        tcvc[1].ShowColumns("Nr", "Position", "Routine", "DataType", "ColumnKey", "RowKey", "Wert");

        tb.ColumnArrangements = tcvc.AsReadOnly();

        tb.RepairAfterParse();
        tableView.Table = tb;

        tableView.Arrangement = string.Empty;
        tableView.SortDefinitionTemporary = new RowSortDefinition(tb, cNr, false);
    }

    private void btnOeffnen_Click(object sender, System.EventArgs e) {
        CachedFileSystem.SaveAll(false);
        Table.SaveAll();

        OpenTab.ShowDialog();
        if (string.IsNullOrEmpty(OpenTab.FileName)) { return; }

        LoadChunk(OpenTab.FileName);
    }

    private void LoadChunk(string filename) {
        if (!FileExists(filename)) { return; }

        var chunk = CachedFileSystem.Get<Chunk>(filename);
        if (chunk is null || chunk.LoadFailed) {
            Notification.Show("Chunk konnte nicht geladen werden:<br>" + filename, ImageCode.Warnung);
            return;
        }

        if (!chunk.EnsureContentLoaded()) {
            Notification.Show("Chunk-Inhalt konnte nicht geladen werden:<br>" + filename, ImageCode.Warnung);
            return;
        }

        capInfo.Text = QuickImage.Get(ImageCode.Puzzle, 16).HTMLCode + " " +
                       filename.FileNameWithSuffix() +
                       "  |  Key: <b>" + chunk.KeyName + "</b>" +
                       "  |  Bytes: <b>" + chunk.Content.Length.ToString1() + "</b>";

        Text = "Chunk-Insight - " + filename.FileNameWithSuffix();

        if (tblChunk.Table is not { IsDisposed: false } tb) { return; }

        tb.Row.Clear("Neuer Chunk geladen");
        FillTable(tb, chunk.Content);
    }

    #endregion
}
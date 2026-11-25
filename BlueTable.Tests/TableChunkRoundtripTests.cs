// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueTable.Tests;

/// <summary>
/// Roundtrip-Test für TableChunk (.tblh): Lite-Hauptdatei plus write-once Row-Chunks (.tblc) je Chunk-Ordner;
/// die _master-Verwaltung bleibt außerhalb des Vergleichs.
/// </summary>
[TestClass]
public class TableChunkRoundtripTests : RoundtripTestBase {

    #region Properties

    protected override string LayoutName => "TableChunk_tblh";

    protected override string Suffix => "tblh";

    protected override bool WithChunkColumn => true;

    protected override string[] AreaFileSuffixes => ["tblc"];

    protected override string[] IgnoredFileSuffixes => [];

    protected override string[] IgnoredFolderMarkers => [TableChunk.Chunk_Master + "\\"];

    protected override bool StripFragmentHead => false;

    #endregion
}

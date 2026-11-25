// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueTable.Tests;

/// <summary>
/// Roundtrip-Test für TableFileNew (.btb) ohne Chunk-Spalte: Ohne Chunk-Pfad liegt der gesamte Inhalt
/// als Kopf-Fragment in Snapshot (.snp) und Journal; der Snapshot-Inhalt wird verglichen, Journale (.frg) bleiben außen vor.
/// </summary>
[TestClass]
public class TableFileNewOhneChunkTests : RoundtripTestBase {

    #region Properties

    protected override string LayoutName => "TableFileNew_btb_ohne_Chunk";

    protected override string Suffix => "btb";

    protected override bool WithChunkColumn => false;

    // Das Suffix .snp ist in SaveableFile eine private Konstante und wird hier layoutkonform geführt.
    protected override string[] AreaFileSuffixes => ["snp"];

    protected override string[] IgnoredFileSuffixes => [SaveableFile.SuffixOfFragments];

    protected override string[] IgnoredFolderMarkers => [];

    protected override bool StripFragmentHead => true;

    #endregion
}

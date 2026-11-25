// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueTable.Tests;

/// <summary>
/// Roundtrip-Test für TableFileNew (.btb) mit Chunk-Spalte: Kopf-Datei, Snapshots (.snp) und
/// write-once Daten-Chunks (.chk) werden inhaltlich verglichen; die Sitzungs-Journale (.frg) bleiben außen vor.
/// </summary>
[TestClass]
public class TableFileNewMitChunkTests : RoundtripTestBase {

    #region Properties

    protected override string LayoutName => "TableFileNew_btb_mit_Chunk";

    protected override string Suffix => "btb";

    protected override bool WithChunkColumn => true;

    // Die Suffixe .snp und .chk sind in SaveableFile private Konstanten und werden hier layoutkonform geführt.
    protected override string[] AreaFileSuffixes => ["snp", "chk"];

    protected override string[] IgnoredFileSuffixes => [SaveableFile.SuffixOfFragments];

    protected override string[] IgnoredFolderMarkers => [];

    protected override bool StripFragmentHead => true;

    #endregion
}

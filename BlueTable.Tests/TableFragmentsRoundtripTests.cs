// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueTable.Tests;

/// <summary>
/// Roundtrip-Test für TableFragments (.mbdb): Hauptdatei mit Fragment-Journalen; die Journale tragen
/// Sitzungs-Stempel und bleiben außerhalb des Byte-Vergleichs, die Hauptdatei wird byte-identisch geprüft.
/// </summary>
[TestClass]
public class TableFragmentsRoundtripTests : RoundtripTestBase {

    #region Properties

    protected override string LayoutName => "TableFragments_mbdb";

    protected override string Suffix => "mbdb";

    protected override bool WithChunkColumn => false;

    protected override string[] AreaFileSuffixes => [];

    protected override string[] IgnoredFileSuffixes => [TableFragments.SuffixOfFragments];

    protected override string[] IgnoredFolderMarkers => [];

    protected override bool StripFragmentHead => false;

    #endregion
}

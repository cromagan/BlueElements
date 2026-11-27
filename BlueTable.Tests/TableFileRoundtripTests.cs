// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueTable.Tests;

/// <summary>
/// Roundtrip-Test für TableFile (.bdb): klassisches Ein-Datei-Format, nur die Hauptdatei wird byte-identisch geprüft.
/// </summary>
[TestClass]
public class TableFileRoundtripTests : RoundtripTestBase {

    #region Properties

    protected override string LayoutName => "TableFile_bdb";

    protected override string Suffix => "bdb";

    protected override bool WithChunkColumn => false;

    protected override string[] AreaFileSuffixes => [];

    protected override string[] IgnoredFileSuffixes => [];

    protected override string[] IgnoredFolderMarkers => [];

    protected override bool StripFragmentHead => false;

    /// <summary>
    /// Legacy-Format: Der Laden-Repair entfernt verwaiste Ansichtseinträge gelöschter Spalten einmalig.
    /// </summary>
    protected override bool FirstRoundtripMayRepair => true;

    #endregion
}

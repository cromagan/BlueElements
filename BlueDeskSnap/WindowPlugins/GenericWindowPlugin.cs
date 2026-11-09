// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueDeskSnap.Plugins;

/// <summary>
/// Standard-Plugin für alle Fenster ohne spezialisiertes Plugin - sichert nur Geometrie und Fensterzustand.
/// </summary>
public sealed class GenericWindowPlugin : WindowPlugin {

    #region Properties

    public override string KeyName => "Allgemein";

    public override int Priority => int.MaxValue;

    #endregion

    #region Methods

    public override List<string> CaptureDocuments(IntPtr handle) => [];

    public override bool IsResponsibleFor(string className, string processName) => true;

    #endregion
}

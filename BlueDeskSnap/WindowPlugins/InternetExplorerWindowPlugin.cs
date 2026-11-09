// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueDeskSnap.Plugins;

/// <summary>
/// Plugin für Internet-Explorer-Fenster: ermittelt die offene Adresse über die Shell-Automatisierung.
/// </summary>
public sealed class InternetExplorerWindowPlugin : WindowPlugin {

    #region Properties

    public override string KeyName => "Internet Explorer";

    public override int Priority => 11;

    #endregion

    #region Methods

    public override List<string> CaptureDocuments(IntPtr handle) =>
        ShellWindowReader.LocationsOf(handle).Where(url => !url.StartsWith("file://", StringComparison.OrdinalIgnoreCase)).ToList();

    public override bool IsResponsibleFor(string className, string processName) => className == "IEFrame";

    #endregion
}

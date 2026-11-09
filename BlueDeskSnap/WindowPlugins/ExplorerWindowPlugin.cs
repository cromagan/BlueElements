// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueDeskSnap.Plugins;

/// <summary>
/// Plugin für Windows-Explorer-Fenster: ermittelt den offenen Ordner und öffnet ihn beim Restore wieder.
/// </summary>
public sealed class ExplorerWindowPlugin : WindowPlugin {

    #region Properties

    public override string KeyName => "Explorer";

    public override int Priority => 10;

    #endregion

    #region Methods

    public override List<string> CaptureDocuments(IntPtr handle) {
        List<string> result = [];

        foreach (var url in ShellWindowReader.LocationsOf(handle)) {
            if (url.StartsWith("file://", StringComparison.OrdinalIgnoreCase) && Uri.TryCreate(url, UriKind.Absolute, out var uri)) {
                result.Add(uri.LocalPath);
            } else {
                result.Add(url);
            }
        }

        return result;
    }

    public override bool IsResponsibleFor(string className, string processName) => className is "CabinetWClass" or "ExploreWClass";

    #endregion
}

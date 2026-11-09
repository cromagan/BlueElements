// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Windows.Automation;

namespace BlueDeskSnap.Plugins;

/// <summary>
/// Plugin für Chromium-Browser (Chrome, Edge, Brave, Vivaldi): liest die Adresse des aktiven Tabs über UI Automation.
/// </summary>
public sealed class BrowserWindowPlugin : WindowPlugin {

    #region Properties

    public override string KeyName => "Browser";

    public override int Priority => 20;

    #endregion

    #region Methods

    public override List<string> CaptureDocuments(IntPtr handle) {
        List<string> result = [];

        if (UrlOfAddressBar(handle) is { Length: > 0 } url) { result.Add(url); }

        return result;
    }

    public override bool IsResponsibleFor(string className, string processName) => className == "Chrome_WidgetWin_1";

    /// <summary>
    /// Sucht das erste Eingabefeld, dessen Wert wie eine Adresse aussieht - die Adressleiste heißt je nach Sprache anders.
    /// </summary>
    private static string? UrlOfAddressBar(IntPtr handle) {
        try {
            var root = AutomationElement.FromHandle(handle);
            var edits = root.FindAll(TreeScope.Descendants, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit));

            foreach (AutomationElement edit in edits) {
                if (!edit.TryGetCurrentPattern(ValuePattern.Pattern, out var pattern) || pattern is not ValuePattern valuePattern) { continue; }

                var url = valuePattern.Current.Value;
                if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                    url.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                    url.StartsWith("file://", StringComparison.OrdinalIgnoreCase) ||
                    url.StartsWith("about:", StringComparison.OrdinalIgnoreCase)) {
                    return url;
                }
            }
        } catch {
            // Fenster nicht (mehr) verfügbar - kein Dokument
        }

        return null;
    }

    #endregion
}

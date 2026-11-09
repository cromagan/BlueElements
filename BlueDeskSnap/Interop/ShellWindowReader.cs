// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueDeskSnap.Interop;

/// <summary>
/// Liest über die Shell-Automatisierung (Shell.Application), welche Ordner/Adressen in Explorer- und IE-Fenstern offen sind.
/// </summary>
public static class ShellWindowReader {

    #region Methods

    /// <summary>
    /// Liefert alle LocationURLs der Shell-Fenster, die zum Fenster-Handle gehören (bei Explorer z.B. file:///C:/Ordner).
    /// </summary>
    public static List<string> LocationsOf(IntPtr handle) {
        List<string> result = [];

        var type = Type.GetTypeFromProgID("Shell.Application");
        if (type is null) { return result; }

        dynamic? shell = null;
        dynamic? windows = null;

        try {
            shell = Activator.CreateInstance(type);
            if (shell is null) { return result; }

            windows = shell.Windows();

            foreach (var window in windows) {
                try {
                    if (Convert.ToInt64(window.HWND) != handle.ToInt64()) { continue; }

                    var url = window.LocationURL as string;
                    if (url is { Length: > 0 }) { result.Add(url); }
                } finally {
                    Marshal.FinalReleaseComObject(window);
                }
            }
        } catch {
            // Shell-Automatisierung nicht verfügbar - dann eben ohne Dokumente
        } finally {
            if (windows is not null) { Marshal.FinalReleaseComObject(windows); }
            if (shell is not null) { Marshal.FinalReleaseComObject(shell); }
        }

        return result;
    }

    #endregion
}

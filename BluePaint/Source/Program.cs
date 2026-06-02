// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Windows.Forms;

namespace BluePaint.Source;

internal static class Program {

    #region Methods

    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) {
        try {
            var ex = (Exception)e.ExceptionObject;
            throw Develop.DebugError($"Allgemeiner unbehandelter Fehler unbekannter Herkunft: {ex.Message}");
        } catch { }
        Develop.AbortExe(true);
    }

    /// <summary>
    /// Der Haupteinstiegspunkt für die Anwendung.
    /// </summary>
    [STAThread]
    private static void Main() {
        Application.SetHighDpiMode(HighDpiMode.DpiUnaware);
        Develop.StartService();

        var currentDomain = AppDomain.CurrentDomain;
        currentDomain.UnhandledException += CurrentDomain_UnhandledException;

        //CultureInfo culture = new("de-DE");
        //CultureInfo.DefaultThreadCurrentCulture = culture;
        //CultureInfo.DefaultThreadCurrentUICulture = culture;
        //System.Windows.Forms.Application.EnableVisualStyles();
        //System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

        Application.Run(new MainWindow(true));
    }

    #endregion
}
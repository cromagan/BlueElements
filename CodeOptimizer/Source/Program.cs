// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Windows.Forms;

namespace CodeOptimizer;

internal static class Program {

    #region Methods

    /// <summary>
    /// Der Haupteinstiegspunkt der Anwendung.
    /// </summary>
    [STAThread]
    private static void Main() {
        // DPI-Awareness MUSS vor allen anderen Application-Aufrufen stehen
        Application.SetHighDpiMode(HighDpiMode.DpiUnaware);
        Application.EnableVisualStyles();

        Develop.StartService();
        Application.Run(FormManager.Starter(typeof(OptimizerForm), typeof(OptimizerForm)));
    }

    #endregion
}

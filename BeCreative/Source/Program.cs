// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.ClassesStatic;
using BlueBasics.Enums;
using BlueControls.Classes;
using System.Diagnostics;
using System.Windows.Forms;

namespace BeCreative {

    internal static class Program {

        #region Methods

        [StandaloneInfo("Startmen\u00fc (neuer Prozess)", ImageCode.Stern, "Admin",
    "Zweite, vollst\u00e4ndig isolierte Instanz (eigener OS-Prozess) f\u00fcr Multi-User-Tests mit Tabellen und Formularen.",
    950)]
        public static System.Windows.Forms.Form? StartIsolated() {
            Process.Start(new ProcessStartInfo(System.Windows.Forms.Application.ExecutablePath, "--attach") {
                UseShellExecute = false
            });
            return null;
        }

        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        private static void Main() {
            // DPI-Awareness MUSS vor allen anderen Application-Aufrufen stehen
            Application.SetHighDpiMode(HighDpiMode.DpiUnaware);
            Application.EnableVisualStyles();

            if (!Debugger.IsAttached && Array.IndexOf(Environment.GetCommandLineArgs(), "--attach") >= 0) {
                Debugger.Launch();
            }

            Develop.StartService();
            Generic.UserGroup = Constants.Administrator;
            Application.Run(FormManager.Starter(typeof(BlueControls.Forms.Start), typeof(BlueControls.Forms.Start)));
            //Application.Run(new Start());
            //BlueBasics.Develop.TraceLogging_End();
        }

        #endregion
    }
}
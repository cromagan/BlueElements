using System;
using System.Globalization;
using System.Windows.Forms;

namespace BeCreative {

    internal static class Program {

        #region Methods

        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        private static void Main() {
            Application.EnableVisualStyles();
            var culture = new CultureInfo("de-DE");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new BlueControls.Forms.frmTableView(null, true, true));
            BlueBasics.Develop.TraceLogging_End();
        }

        #endregion
    }
}
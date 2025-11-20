using BlueBasics;
using BlueControls;
using System;
using System.Windows.Forms;

namespace BeCreative {

    internal static class Program {

        #region Methods

        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        private static void Main() {
            // DPI-Awareness MUSS vor allen anderen Application-Aufrufen stehen
            Application.EnableVisualStyles();

            Develop.StartService();
            Generic.UserGroup = Constants.Administrator;
            FormManager.ExecuteAtEnd = null;
            Application.Run(FormManager.Starter(typeof(BlueControls.Forms.Start), typeof(BlueControls.Forms.Start)));
            //Application.Run(new Start());
            //BlueBasics.Develop.TraceLogging_End();
        }

        #endregion
    }
}
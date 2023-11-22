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
            Application.EnableVisualStyles();

            Develop.StartService();
            Generic.UserGroup = Constants.Administrator;
            FormManager.ExecuteAtEnd = Start.Ende;
            Application.Run(FormManager.Starter(typeof(Start), typeof(Start)));
            //Application.Run(new Start());
            //BlueBasics.Develop.TraceLogging_End();
        }

        #endregion
    }
}
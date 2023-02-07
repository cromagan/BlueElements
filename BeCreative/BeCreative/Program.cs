using BlueBasics;
using BlueControls;
using System;
using System.Globalization;
using System.Windows.Forms;

namespace BeCreative;

internal static class Program {

    #region Methods

    /// <summary>
    /// Der Haupteinstiegspunkt für die Anwendung.
    /// </summary>
    [STAThread]
    private static void Main() {
        Application.EnableVisualStyles();

        Develop.StartService();

        FormManager.NewModeSelectionForm = Start.NewForm;
        FormManager.ExecuteAtEnd = Start.Ende;
        FormManager.StartForm = new Start();
        Application.Run(FormManager.Current);
        //Application.Run(new Start());
        //BlueBasics.Develop.TraceLogging_End();
    }

    #endregion
}
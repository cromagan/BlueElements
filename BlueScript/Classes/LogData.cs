// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.Classes;

public class LogData {

    #region Constructors

    public LogData(string subname, int linestart) {
        Subname = subname;
        Line = linestart;
    }

    #endregion

    #region Properties

    public int Line { get; private set; }
    public string Protocol { get; set; } = string.Empty;

    /// <summary>
    ///  In welcher Sub wir uns gerade befinden
    /// </summary>
    public string Subname { get; }

    #endregion

    #region Methods

    public void AddMessage(string errormessage) => Protocol = "[" + Subname + ", Zeile: " + Line + "] " + errormessage;

    public void LineAdd(int c) {
        if (c < 0) {
            Develop.DebugError("Wert unter null nicht erlaubt!");
        }

        Line += c;
    }

    #endregion
}
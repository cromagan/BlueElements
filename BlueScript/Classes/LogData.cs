// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.Classes;

public class LogData {

    #region Fields

    private readonly List<string> _syntaxCheckUnknownVariables = [];

    #endregion

    #region Constructors

    public LogData(string subname, int linestart) {
        Subname = subname;
        Line = linestart;
    }

    #endregion

    #region Properties

    public string ErrorMessage {
        get;
        set {
            field = value;
            Protocol = "[" + Subname + ", Zeile: " + Line + "]\r\n" + field;
        }
    }

    public int Line { get; private set; }
    public string Protocol { get; set; } = string.Empty;

    /// <summary>
    ///  In welcher Sub wir uns gerade befinden
    /// </summary>
    public string Subname { get; }

    #endregion

    #region Methods

    public void LineAdd(int c) {
        if (c < 0) {
            Develop.DebugError("Wert unter null nicht erlaubt!");
        }

        Line += c;
    }

    public void RegisterSyntaxCheckUnknownVariable(string name) {
        if (!_syntaxCheckUnknownVariables.Contains(name, StringComparer.OrdinalIgnoreCase)) {
            _syntaxCheckUnknownVariables.Add(name);
        }
    }

    public List<string> SyntaxCheckUnknownVariables() => [.. _syntaxCheckUnknownVariables.OrderBy(x => x)];

    #endregion
}
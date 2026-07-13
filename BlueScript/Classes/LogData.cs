// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.Classes;

/// <summary>
///  Hält den aktuellen Skript-Kontext (Sub-Name und Zeilennummer), der während
///  des Parsens durchgereicht und über <see cref="LineAdd" /> fortgeschrieben wird.
///  Wird ausschließlich von der Parsing-Engine (<see cref="Script" />) und den
///  Schleifen-/Bedingungs-Befehlen genutzt, um Fehlermeldungen mit Positionen
///  anreichern zu können.
/// </summary>
public class LogData {

    #region Constructors

    public LogData(string subname, int linestart) {
        Subname = subname;
        Line = linestart;
    }

    #endregion

    #region Properties

    public int Line { get; private set; }

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

    #endregion
}

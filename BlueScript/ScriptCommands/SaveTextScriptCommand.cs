// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Text;
using static BlueBasics.ClassesStatic.IO;

namespace BlueScript.ScriptCommands;

/// <summary>
/// Speichert den Text auf die Festplatte
/// </summary>
internal class SaveTextScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal, StringVal];
    public override string Command => "savetext";
    public override List<string> Constants => ["UTF8", "WIN1252"];
    public override ScriptCommandType ScriptCommandLevel => ScriptCommandType.LongTime;
    public override string Syntax => "SaveText(Filename, UTF8/WIN1252, Text);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {

        #region  Dateinamen ermitteln (filn)

        var filn = attvar.ValueStringGet(0);
        if (!filn.IsValidFilepathAndName()) { return new DoItFeedback("Dateinamen-Fehler!", true); }

        var pf = filn.PathParent();
        var opr = CanWriteInDirectory(pf);
        if (opr.IsFailed) { return new DoItFeedback(opr.FailedReason, true); }

        if (FileExists(filn)) { return new DoItFeedback("Datei existiert bereits.", true); }

        #endregion

        //if (!scp.ChangeValues) { return new DoItFeedback(ld, "Bild Speichern im Testmodus deaktiviert."); }

        switch (attvar.ValueStringGet(1).ToUpperInvariant()) {
            case "UTF8":
                if (WriteAllText(filn, attvar.ValueStringGet(2), Encoding.UTF8, false).IsFailed) {
                    return new DoItFeedback("Fehler beim Erzeugen der Datei.", true);
                }

                break;

            case "WIN1252":
                if (WriteAllText(filn, attvar.ValueStringGet(2), Win1252, false).IsFailed) {
                    return new DoItFeedback("Fehler beim Erzeugen der Datei.", true);
                }
                break;

            default:
                return new DoItFeedback("Export-Format unbekannt.", true);
        }

        return DoItFeedback.Null();
    }

    #endregion
}

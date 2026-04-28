// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Text;
using static BlueBasics.ClassesStatic.IO;

namespace BlueScript.Methods;

internal class Method_SaveText : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal, StringVal];
    public override string Command => "savetext";
    public override List<string> Constants => ["UTF8", "WIN1252"];
    public override string Description => "Speichert den Text auf die Festplatte";
    public override MethodType MethodLevel => MethodType.LongTime;
    public override string Syntax => "SaveText(Filename, UTF8/WIN1252, Text);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {

        #region  Dateinamen ermitteln (filn)

        var filn = attvar.ValueStringGet(0);
        if (string.IsNullOrEmpty(filn)) { return new DoItFeedback("Dateinamen-Fehler!", true, ld); }

        if (!filn.IsFormat(FormatHolder_FilepathAndName.Instance)) { return new DoItFeedback("Dateinamen-Fehler!", true, ld); }

        var pf = filn.PathParent();
        var opr = CanWriteInDirectory(pf);
        if (!string.IsNullOrEmpty(opr)) { return new DoItFeedback(opr, true, ld); }

        if (FileExists(filn)) { return new DoItFeedback("Datei existiert bereits.", true, ld); }

        #endregion

        //if (!scp.ChangeValues) { return new DoItFeedback(ld, "Bild Speichern im Testmodus deaktiviert."); }

        switch (attvar.ValueStringGet(1).ToUpperInvariant()) {
            case "UTF8":
                if (!WriteAllText(filn, attvar.ValueStringGet(2), Encoding.UTF8, false)) {
                    return new DoItFeedback("Fehler beim Erzeugen der Datei.", true, ld);
                }

                break;

            case "WIN1252":
                if (!WriteAllText(filn, attvar.ValueStringGet(2), BlueBasics.ClassesStatic.Constants.Win1252, false)) {
                    return new DoItFeedback("Fehler beim Erzeugen der Datei.", true, ld);
                }
                break;

            default:
                return new DoItFeedback("Export-Format unbekannt.", true, ld);
        }

        return DoItFeedback.Null();
    }

    #endregion
}
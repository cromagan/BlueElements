// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Enums;

using System.Text;

namespace BlueScript.Methods;


internal class Method_LoadTextFile : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal];
    public override string Command => "loadtextfile";
    public override List<string> Constants => ["UTF8", "WIN1252"];
    public override string Description => "Lädt die angegebene Textdatei aus dem Dateisystem.";
    public override MethodType MethodLevel => MethodType.LongTime;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableString.ShortName_Variable;
    public override string Syntax => "LoadTextFile(Filename, UTF8/WIN1252)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        var filen = attvar.ValueStringGet(0);

        if (filen.FileType() is not FileFormat.Textdocument and not FileFormat.CSV) {
            return new DoItFeedback("Datei ist kein Textformat: " + filen, true);
        }

        if (!IO.FileExists(filen)) {
            return new DoItFeedback("Datei nicht gefunden: " + filen, true);
        }

        try {
            string importText;
            switch (attvar.ValueStringGet(1).ToUpperInvariant()) {
                case "UTF8":
                    importText = IO.ReadAllText(filen, Encoding.UTF8);
                    break;

                case "WIN1252":
                    importText = IO.ReadAllText(filen, BlueBasics.ClassesStatic.Constants.Win1252);
                    break;

                default:
                    return new DoItFeedback("Import-Format unbekannt.", true);
            }

            return new DoItFeedback(importText);
        } catch {
            return new DoItFeedback("Datei konnte nicht geladen werden: " + filen, true);
        }
    }

    #endregion
}
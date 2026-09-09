// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueScript.ScriptCommands;


/// <summary>
/// Entfernt die angegebenen Texte am Anfang und Ende des Strings. Groß und Kleinschreibung wird ignoriert.
/// </summary>
internal class TrimScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal];
    public override string Command => "trim";
    public override LastArgMinCountTypeScriptCommand LastArgMinCount => LastArgMinCountTypeScriptCommand.MinOnce;
    public override bool MustUseReturnValue => true;
    public override string Returns => StringScriptVariable.ShortName_Plain;
    public override string Syntax => "Trim(StringScriptCommand, TexttoTrim, ...)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        var val = attvar.ValueStringGet(0);

        string txt;

        do {
            txt = val;
            for (var z = 1; z < attvar.Attributes.Count; z++) {
                val = val.Trim(attvar.ValueStringGet(z));
            }
        } while (txt != val);

        return new DoItFeedback(val);
    }

    #endregion
}

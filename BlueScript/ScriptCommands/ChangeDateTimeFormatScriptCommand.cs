// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Globalization;

namespace BlueScript.ScriptCommands;

/// <summary>
/// Wandelt eine Zeitangabe-String in einen andern String um, der mittels des zweiten String definiert ist.
/// Beispiel eines solchen Strings:  dd.MM.yyyy HH:mm:ss.fff
/// Achtung: Groß-Kleinschreibung ist wichtig!
/// </summary>
internal class ChangeDateTimeFormatScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal];
    public override string Command => "changedatetimeformat";
    public override List<string> Constants => [.. DateTimeFormats];
    public override bool MustUseReturnValue => true;
    public override string Returns => StringScriptVariable.ShortName_Plain;
    public override string Syntax => "ChangeDateTimeFormat(DateTimeString, string)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        var d = attvar.ValueDateGet(0);

        if (d is null) {
            return new DoItFeedback("Der Wert '" + attvar.ReadableText(0) + "' wurde nicht als Zeitformat erkannt.", true);
        }

        try {
            return new DoItFeedback(d.Value.ToString(attvar.ReadableText(1), CultureInfo.InvariantCulture));
        } catch {
            return new DoItFeedback("Der Umwandlungs-String '" + attvar.ReadableText(1) + "' ist fehlerhaft.", true);
        }
    }

    #endregion
}

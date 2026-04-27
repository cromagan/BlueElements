// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.Methods;


internal class Method_DateTimeDifferenceInDays : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal];
    public override string Command => "datetimedifferenceindays";
    public override string Description => "Gibt die Differnz in Tagen der beiden Datums als Gleitkommazahl zurück.\rErgebnis = Date1 - Date2";
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableDouble.ShortName_Plain;
    public override string Syntax => "DateTimeDifferenceInDays(DateString1, DateString2)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var d1 = attvar.ValueDateGet(0);

        if (d1 == null) {
            return new DoItFeedback("Der Wert '" + attvar.ReadableText(0) + "' wurde nicht als Zeitformat erkannt.", true, ld);
        }

        var d2 = attvar.ValueDateGet(1);

        return d2 == null
            ? new DoItFeedback("Der Wert '" + attvar.ReadableText(1) + "' wurde nicht als Zeitformat erkannt.", true, ld)
            : new DoItFeedback(d1.Value.Subtract(d2.Value).TotalDays);
    }

    #endregion
}
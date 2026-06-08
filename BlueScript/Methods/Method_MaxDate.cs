// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Globalization;
using static BlueBasics.ClassesStatic.Converter;

namespace BlueScript.Methods;


internal class Method_MaxDate : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal, [VariableListString.ShortName_Plain, VariableString.ShortName_Plain]];
    public override string Command => "maxdate";
    public override string Description => "Gibt den den angegeben Werten den, mit dem höchsten Wert zurück.\r\nLeere Eingangswerte werden ignoriert.\r\nBeispiel für Format-String: dd.MM.yyyy HH:mm:ss.fff";
    public override LastArgMinCountType LastArgMinCount => LastArgMinCountType.MinTwice;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableString.ShortName_Plain;
    public override string Syntax => "MaxDate(FormatString, Value1, Value2, ...)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var d = new DateTime(0);

        var l = new List<string>();

        for (var z = 1; z < attvar.Attributes.Count; z++) {
            if (attvar.Attributes[z] is VariableString vs) { l.Add(vs.ValueString); }
            if (attvar.Attributes[z] is VariableListString vl) { l.AddRange(vl.ValueList); }
        }

        foreach (var thisw in l) {
            if (!string.IsNullOrEmpty(thisw)) {
                var ok = DateTimeTryParse(thisw, out var da);

                if (!ok) {
                    return new DoItFeedback("Wert kann nicht als Datum interpretiert werden: " + thisw, true, ld);
                }

                if (da.Subtract(d).TotalDays > 0) {
                    d = da;
                }
            }
        }

        try {
            return new DoItFeedback(d.ToString(attvar.ReadableText(0), CultureInfo.InvariantCulture));
        } catch {
            return new DoItFeedback("Der Umwandlungs-String '" + attvar.ReadableText(0) + "' ist fehlerhaft.", true, ld);
        }
    }

    #endregion
}
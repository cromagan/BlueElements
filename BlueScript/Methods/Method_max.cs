// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.Methods;


internal class Method_Max : Method {

    #region Properties

    public override List<List<string>> Args => [[VariableDouble.ShortName_Plain, VariableString.ShortName_Plain, VariableListString.ShortName_Plain]];
    public override string Command => "max";

    public override string Description => "Gibt von den angegebenen Werten den mit dem höchsten Wert zurück.\r\n" +
                                            "Ein Text wird - wenn möglich - als Zahl interpretiert.\r\n" +
                                            "Ist das nicht möglich, wird der Text ignoriert.\r\n" +
                                            "Eine angegebene Liste muss mindestens einen Eintrag enthalten.";

    public override LastArgMinCountType LastArgMinCount => LastArgMinCountType.MinOnce;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableDouble.ShortName_Plain;
    public override string Syntax => "Max(Value1, Value2, ...)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        var l = new List<double>();
        foreach (var thisvar in attvar.Attributes) {
            switch (thisvar) {
                case VariableDouble vf:
                    l.Add(vf.ValueNum);
                    break;

                case VariableString vs:
                    if (DoubleTryParse(vs.ValueString, out var r)) {
                        l.Add(r);
                    }
                    break;

                case VariableListString vl:
                    if (vl.ValueList is not { Count: > 0 }) {
                        return new DoItFeedback("Eine angegebene Liste enthält keine Werte.", true);
                    }
                    foreach (var thiss in vl.ValueList) {
                        if (DoubleTryParse(thiss, out var r2)) {
                            l.Add(r2);
                        }
                    }
                    break;
            }
        }

        return l.Count > 0 ? new DoItFeedback(l.Max()) : new DoItFeedback("Keine gültigen Werte angekommen", true);
    }

    #endregion
}
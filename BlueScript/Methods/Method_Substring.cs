// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.Methods;

internal class Method_Substring : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal, FloatVal, FloatVal];
    public override string Command => "substring";
    public override string Description => "Gibt einen Teilstring zurück. Ist der Start oder das Ende keine gültige Position, wird das bestmögliche zurückgegeben und kein Fehler ausgelöst. Subrtring(\"Hallo\", 2,2) gibt ll zurück.";
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableString.ShortName_Plain;
    public override string Syntax => "Substring(String, Start, Anzahl)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        var st = attvar.ValueIntGet(1);
        var en = attvar.ValueIntGet(2);
        if (st < 0) {
            en += st;
            st = 0;
        }

        if (en <= 0) { return new DoItFeedback(string.Empty); }

        var t = attvar.ValueStringGet(0);

        if (st >= t.Length) { return new DoItFeedback(string.Empty); }

        if (st + en > t.Length) { en = t.Length - st; }
        return new DoItFeedback(t.Substring(st, en));
    }

    #endregion
}
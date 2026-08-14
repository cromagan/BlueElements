// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.ScriptCommands;


internal class TrimSuffixScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal];
    public override string Command => "trimsuffix";
    public override string Description => "Entfernt die angegebenen Suffixe und evtl. übrige Leerzeichen. Die Suffixe werden nur entfernt, wenn vor dem Suffix ein Leerzeichen oder eine Zahl ist. Groß- und Kleinschreibung wird ignoriert.";
    public override LastArgMinCountTypeScriptCommand LastArgMinCount => LastArgMinCountTypeScriptCommand.MinOnce;
    public override bool MustUseReturnValue => true;
    public override string Returns => StringScriptVariable.ShortName_Plain;
    public override string Syntax => "TrimSuffixScriptCommand(string, suffix, ...)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        var val = attvar.ValueStringGet(0);

        const string tmp = Char_Numerals + " ";

        for (var z = 1; z < attvar.Attributes.Count; z++) {
            var suf = attvar.ValueStringGet(z);
            if (val.Length <= suf.Length) {
                continue;
            }

            if (val.EndsWith(suf, StringComparison.OrdinalIgnoreCase)) {
                var c = val[val.Length - suf.Length - 1];
                if (tmp.Contains(c)) {
                    return new DoItFeedback(val[..^suf.Length].TrimEnd(' '));
                }
            }
        }

        return new DoItFeedback(val);
    }

    #endregion
}
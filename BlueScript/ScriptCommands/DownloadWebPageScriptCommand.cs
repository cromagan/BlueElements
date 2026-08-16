// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.ScriptCommands;

internal class DownloadWebPageScriptCommand : ScriptCommand {

    #region Fields

    private static readonly VariableCollection Last = [];

    #endregion

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal, StringVal];
    public override string Command => "downloadwebpage";
    public override string Description => "Lädt die angegebene Webseite aus dem Internet.\r\nGibt niemals einen Fehler zurück, eber evtl. string.empty";
    public override bool MustUseReturnValue => true;
    public override string Returns => StringScriptVariable.ShortName_Variable;
    public override ScriptCommandType ScriptCommandLevel => ScriptCommandType.LongTime;
    public override string Syntax => "DownloadWebPageScriptCommand(Url)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        var url = attvar.ValueStringGet(0);
        var varn = "X" + url.ReduceToChars(AllowedCharsVariableName);

        if (Last.GetByKey(varn) is StringScriptVariable vb) {
            return new DoItFeedback(vb.ValueString);
        }

        try {
            CollectGarbage();
            var txt = Download(url);

            Last.Add(new StringScriptVariable(varn, txt, true, string.Empty));
            return new DoItFeedback(txt);
        } catch {
            return new DoItFeedback(string.Empty);
        }
    }

    #endregion
}
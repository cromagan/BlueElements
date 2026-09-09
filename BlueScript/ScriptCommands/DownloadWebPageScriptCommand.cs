// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueScript.ScriptCommands;

/// <summary>
/// Lädt die angegebene Webseite aus dem Internet.
/// Gibt niemals einen Fehler zurück, eber evtl. string.empty
/// </summary>
internal class DownloadWebPageScriptCommand : ScriptCommand {

    #region Fields

    private static readonly VariableCollection Last = [];

    #endregion

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal, StringVal];
    public override string Command => "downloadwebpage";
    public override bool MustUseReturnValue => true;
    public override string Returns => StringScriptVariable.ShortName_Variable;
    public override ScriptCommandType ScriptCommandLevel => ScriptCommandType.LongTime;
    public override string Syntax => "DownloadWebPage(Url)";

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

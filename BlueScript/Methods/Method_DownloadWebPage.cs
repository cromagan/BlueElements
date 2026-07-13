// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.Methods;


internal class Method_DownloadWebPage : Method {

    #region Fields

    private static readonly VariableCollection Last = [];

    #endregion

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal, StringVal];
    public override string Command => "downloadwebpage";
    public override string Description => "Lädt die angegebene Webseite aus dem Internet.\r\nGibt niemals einen Fehler zurück, eber evtl. string.empty";
    public override MethodType MethodLevel => MethodType.LongTime;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableString.ShortName_Variable;
    public override string Syntax => "DownloadWebPage(Url)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        var url = attvar.ValueStringGet(0);
        var varn = "X" + url.ReduceToChars(BlueBasics.ClassesStatic.Constants.AllowedCharsVariableName);

        if (Last.GetByKey(varn) is VariableString vb) {
            return new DoItFeedback(vb.ValueString);
        }

        try {
            Generic.CollectGarbage();
            var txt = Generic.Download(url);

            Last.Add(new VariableString(varn, txt, true, string.Empty));
            return new DoItFeedback(txt);
        } catch {
            return new DoItFeedback(string.Empty);
        }
    }

    #endregion
}
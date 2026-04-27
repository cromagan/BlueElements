// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using Anthropic.SDK;

namespace BlueScript.Methods;

internal class Method_Ai : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal];
    public override string Command => "ai";
    public override string Description => "Initialisiert die KI von Claude";
    public override MethodType MethodLevel => MethodType.LongTime;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableAi.ShortName_Variable;
    public override string Syntax => "Ai(APIKey)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        //https://keyholesoftware.com/2019/02/11/create-your-own-web-bots-in-net-with-cefsharp/

        // Da es keine Möglichkeit gibt, eine Url Variable (außerhalb eines If) zu deklarieren,
        // darf diese Routine nicht fehlschlagen.

        try {
            Generic.CollectGarbage();

            var client = new AnthropicClient(attvar.ValueStringGet(0));

            return new DoItFeedback(new VariableAi(client));
        } catch {
            return new DoItFeedback(new VariableAi(null as AnthropicClient));
        }
    }

    #endregion
}
// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.Methods;

internal class Method_Ai : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal, StringVal];
    public override string Command => "ai";
    public override string Description => "Initialisiert eine KI-Verbindung. Funktioniert mit jedem OpenAI-kompatiblen API-Endpunkt (OpenAI, Mistral, Groq, OpenRouter, DeepSeek, Together AI, Ollama, LM Studio u. a.). Der API-Schlüssel wird als Bearer-Token gesendet.";
    public override MethodType MethodLevel => MethodType.LongTime;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableAi.ShortName_Variable;
    public override string Syntax => "Ai(APIKey, Endpoint, Model)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        //https://keyholesoftware.com/2019/02/11/create-your-own-web-bots-in-net-with-cefsharp/

        // Da es keine Möglichkeit gibt, eine Url Variable (außerhalb eines If) zu deklarieren,
        // darf diese Routine nicht fehlschlagen.

        try {
            Generic.CollectGarbage();

            var apiKey = attvar.ValueStringGet(0);
            var endpoint = attvar.ValueStringGet(1);
            var model = attvar.ValueStringGet(2);

            return new DoItFeedback(new VariableAi(apiKey, endpoint, model));
        } catch {
            return new DoItFeedback(new VariableAi(null, null, null));
        }
    }

    #endregion
}

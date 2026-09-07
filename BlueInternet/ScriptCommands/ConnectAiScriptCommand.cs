// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueScript.ScriptCommands;

/// <summary>
/// Initialisiert eine KI-Verbindung. Funktioniert mit jedem OpenAI-kompatiblen API-Endpunkt (OpenAI, Mistral, Groq, OpenRouter, DeepSeek, Together AI, Ollama, LM Studio u. a.). Der API-Schlüssel wird als Bearer-Token gesendet.
/// </summary>
internal class ConnectAiScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal, StringVal];
    public override string Command => "ai";
    public override bool MustUseReturnValue => true;
    public override string Returns => AiScriptVariable.ShortName_Variable;
    public override ScriptCommandType ScriptCommandLevel => ScriptCommandType.LongTime;
    public override string Syntax => "Ai(APIKey, Endpoint, Model)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        //https://keyholesoftware.com/2019/02/11/create-your-own-web-bots-in-net-with-cefsharp/

        // Da es keine Möglichkeit gibt, eine Url Variable (außerhalb eines If) zu deklarieren,
        // darf diese Routine nicht fehlschlagen.

        try {
            CollectGarbage();

            var apiKey = attvar.ValueStringGet(0);
            var endpoint = attvar.ValueStringGet(1);
            var model = attvar.ValueStringGet(2);

            return new DoItFeedback(new AiScriptVariable(apiKey, endpoint, model));
        } catch {
            return new DoItFeedback(new AiScriptVariable(null, null, null));
        }
    }

    #endregion
}

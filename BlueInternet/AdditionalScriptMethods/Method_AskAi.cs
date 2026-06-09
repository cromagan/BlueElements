// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using Anthropic.SDK.Constants;
using Anthropic.SDK.Messaging;
using static BlueScript.Variables.VariableAi;

//using CefSharp.WinForms;

namespace BlueScript.Methods;

internal class Method_AskAi : Method {

    #region Properties

    public override List<List<string>> Args => [AiVal, StringVal];
    public override string Command => "askai";
    public override string Description => "Gibt einen Text an die KI weiter";
    public override MethodType MethodLevel => MethodType.LongTime;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableString.ShortName_Plain;
    public override string Syntax => "AskAi(Ai, text)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (attvar.Attributes[0] is not VariableAi mai) { return DoItFeedback.InternerFehler(ld); }
        if (mai.ValueClient is not { } client) { return DoItFeedback.InternerFehler(ld); }

        if (scp.SyntaxCheck) { return new DoItFeedback(string.Empty); }

        if (!scp.ProduktivPhase) { return DoItFeedback.TestModusInaktiv(ld); }

        var tries = 0;

        do {
            try {
                var messages = new List<Message> { new(RoleType.User, attvar.ValueStringGet(1)) };

                var parameters = new MessageParameters {
                    Messages = messages,
                    MaxTokens = 1024,
                    Model = AnthropicModels.Claude4Opus,
                    Stream = false,
                    Temperature = 1.0m,
                };
                var firstResult = client.Messages.GetClaudeMessageAsync(parameters).GetAwaiter().GetResult();

                return new DoItFeedback(firstResult.Message.ToString());
            } catch {
                tries++;
                Generic.Pause(10, false);
            }
        } while (tries < 10);
        return new DoItFeedback("Allgemeiner Fehler bei der Übergabe an die KI.", false, ld);
    }

    #endregion
}
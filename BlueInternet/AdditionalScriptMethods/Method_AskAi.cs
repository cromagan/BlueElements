// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using static BlueScript.Variables.VariableAi;

namespace BlueScript.Methods;

internal class Method_AskAi : Method {

    #region Properties

    public override List<List<string>> Args => [AiVal, StringVal];
    public override string Command => "askai";
    public override string Description => "Sendet einen Text an die KI und gibt die Antwort als String zurück. Nutzt den OpenAI-kompatiblen Chat-Completion-Endpunkt (/chat/completions).";
    public override MethodType MethodLevel => MethodType.LongTime;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableString.ShortName_Plain;
    public override string Syntax => "AskAi(Ai, text)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        if (attvar.Attributes[0] is not VariableAi mai) { return DoItFeedback.InternerFehler(); }
        if (mai.IsNullOrEmpty) { return DoItFeedback.InternerFehler(); }

        if (!scp.ProduktivPhase) { return DoItFeedback.TestModusInaktiv(); }

        var tries = 0;
        do {
            try {
                var result = VariableAi.AskAsync(mai.ApiKey, mai.Endpoint, mai.Model, attvar.ValueStringGet(1), null)
                    .GetAwaiter().GetResult();

                if (result is { Length: > 0 }) { return new DoItFeedback(result); }
            } catch {
                // AskAsync gibt bei Fehlern null zurück; unerwartete Exceptions führen zum Retry.
            }
            tries++;
            Generic.Pause(10, false);
        } while (tries < 10);

        return new DoItFeedback("Allgemeiner Fehler bei der Übergabe an die KI.", false);
    }

    #endregion
}
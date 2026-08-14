// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using static BlueScript.ScriptVariables.AiScriptVariable;

namespace BlueScript.ScriptCommands;

internal class AskAiScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [AiVal, StringVal];
    public override string Command => "askai";
    public override string Description => "Sendet einen Text an die KI und gibt die Antwort als StringScriptCommand zurück. Nutzt den OpenAI-kompatiblen Chat-Completion-Endpunkt (/chat/completions).";
    public override ScriptCommandType ScriptCommandLevel => ScriptCommandType.LongTime;
    public override bool MustUseReturnValue => true;
    public override string Returns => StringScriptVariable.ShortName_Plain;
    public override string Syntax => "AskAiScriptCommand(Ai, text)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        if (attvar.Attributes[0] is not AiScriptVariable mai) { return DoItFeedback.InternerFehler(); }
        if (mai.IsNullOrEmpty) { return DoItFeedback.InternerFehler(); }

        if (!scp.ProduktivPhase) { return DoItFeedback.TestModusInaktiv(); }

        var tries = 0;
        do {
            try {
                var result = AskAsync(mai.ApiKey, mai.Endpoint, mai.Model, attvar.ValueStringGet(1), null)
                    .GetAwaiter().GetResult();

                if (result is { Length: > 0 }) { return new DoItFeedback(result); }
            } catch {
                // AskAsync gibt bei Fehlern null zurück; unerwartete Exceptions führen zum Retry.
            }
            tries++;
            Pause(10, false);
        } while (tries < 10);

        return new DoItFeedback("Allgemeiner Fehler bei der Übergabe an die KI.", false);
    }

    #endregion
}
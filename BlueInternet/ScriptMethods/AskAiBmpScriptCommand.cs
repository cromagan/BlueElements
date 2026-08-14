// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using static BlueScript.ScriptVariables.AiScriptVariable;
using static BlueScript.ScriptVariables.BitmapScriptVariable;

namespace BlueScript.ScriptCommands;

internal class AskAiBmpScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [AiVal, StringVal, BmpVar];
    public override string Command => "askaibmp";
    public override string Description => "Sendet einen Text und ein Bild an die KI (Vision) und gibt die Antwort als StringScriptCommand zurück. Nutzt den OpenAI-kompatiblen Chat-Completion-Endpunkt mit image_url-Inhalt. Das Modell muss Vision-fähig sein.";
    public override ScriptCommandType ScriptCommandLevel => ScriptCommandType.LongTime;
    public override bool MustUseReturnValue => true;
    public override string Returns => StringScriptVariable.ShortName_Plain;
    public override string Syntax => "AskAiBmpScriptCommand(Ai, text, image)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        if (attvar.Attributes[0] is not AiScriptVariable mai) { return DoItFeedback.InternerFehler(); }
        if (mai.IsNullOrEmpty) { return DoItFeedback.InternerFehler(); }
        if (attvar.ValueBitmapGet(2) is not { } bmp) { return DoItFeedback.FalscherDatentyp(); }

        if (!scp.ProduktivPhase) { return DoItFeedback.TestModusInaktiv(); }

        var tries = 0;
        do {
            try {
                CollectGarbage();

                var result = AskAsync(mai.ApiKey, mai.Endpoint, mai.Model, attvar.ValueStringGet(1), bmp)
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
// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using static BlueScript.Variables.VariableAi;
using static BlueScript.Variables.VariableBitmap;

namespace BlueScript.Methods;

internal class Method_AskAiBmp : Method {

    #region Properties

    public override List<List<string>> Args => [AiVal, StringVal, BmpVar];
    public override string Command => "askaibmp";
    public override string Description => "Sendet einen Text und ein Bild an die KI (Vision) und gibt die Antwort als String zurück. Nutzt den OpenAI-kompatiblen Chat-Completion-Endpunkt mit image_url-Inhalt. Das Modell muss Vision-fähig sein.";
    public override MethodType MethodLevel => MethodType.LongTime;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableString.ShortName_Plain;
    public override string Syntax => "AskAiBmp(Ai, text, image)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (attvar.Attributes[0] is not VariableAi mai) { return DoItFeedback.InternerFehler(ld); }
        if (mai.IsNullOrEmpty) { return DoItFeedback.InternerFehler(ld); }
        if (attvar.ValueBitmapGet(2) is not { } bmp) { return DoItFeedback.FalscherDatentyp(ld); }

        if (!scp.ProduktivPhase) { return DoItFeedback.TestModusInaktiv(ld); }

        var tries = 0;
        do {
            try {
                Generic.CollectGarbage();

                var result = VariableAi.AskAsync(mai.ApiKey, mai.Endpoint, mai.Model, attvar.ValueStringGet(1), bmp, ld)
                    .GetAwaiter().GetResult();

                if (result is { Length: > 0 }) { return new DoItFeedback(result); }
            } catch {
                // AskAsync gibt bei Fehlern null zurück; unerwartete Exceptions führen zum Retry.
            }
            tries++;
            Generic.Pause(10, false);
        } while (tries < 10);

        return new DoItFeedback("Allgemeiner Fehler bei der Übergabe an die KI.", false, ld);
    }

    #endregion
}
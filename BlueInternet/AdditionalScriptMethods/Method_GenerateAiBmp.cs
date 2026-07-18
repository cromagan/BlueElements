// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using static BlueScript.Variables.VariableAi;

namespace BlueScript.Methods;

internal class Method_GenerateAiBmp : Method {

    #region Properties

    public override List<List<string>> Args => [AiVal, StringVal, StringVal];
    public override string Command => "generateaibmp";
    public override string Description => "Erzeugt aus einem Text-Prompt ein Bild über die KI und gibt es als Bitmap zurück. Nutzt den OpenAI-kompatiblen Image-Generation-Endpunkt (/images/generations). Das dritte Argument ist der Bild-Modellname (z. B. dall-e-3, dall-e-2 oder einen beim Anbieter verfügbaren Bild-Modell).";
    public override MethodType MethodLevel => MethodType.LongTime;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableBitmap.ShortName_Variable;
    public override string Syntax => "GenerateAiBmp(Ai, text, imagemodel)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        if (attvar.Attributes[0] is not VariableAi mai) { return DoItFeedback.InternerFehler(); }
        if (mai.IsNullOrEmpty) { return DoItFeedback.InternerFehler(); }

        if (!scp.ProduktivPhase) { return DoItFeedback.TestModusInaktiv(); }

        var imageModel = attvar.ValueStringGet(2);
        if (string.IsNullOrWhiteSpace(imageModel)) { return new DoItFeedback("Kein Bild-Modell angegeben.", true); }

        var tries = 0;
        do {
            try {
                Generic.CollectGarbage();

                var bmp = VariableAi.GenerateImageAsync(mai.ApiKey, mai.Endpoint, imageModel, attvar.ValueStringGet(1))
                    .GetAwaiter().GetResult();

                if (bmp is not null) { return new DoItFeedback(bmp); }
            } catch {
                // GenerateImageAsync gibt bei Fehlern null zurück; unerwartete Exceptions führen zum Retry.
            }
            tries++;
            Generic.Pause(10, false);
        } while (tries < 10);

        return new DoItFeedback("Allgemeiner Fehler bei der Bildgenerierung durch die KI.", false);
    }

    #endregion
}
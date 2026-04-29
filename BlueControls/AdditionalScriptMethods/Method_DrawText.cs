// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Classes;
using BlueScript.Classes;
using BlueScript.Variables;
using static BlueScript.Variables.VariableBitmap;

namespace BlueControls.AdditionalScriptMethods;

public class Method_DrawText : Method {

    #region Properties

    public override List<List<string>> Args => [BmpVar, StringVal, FloatVal, FloatVal, StringVal, FloatVal];
    public override string Command => "drawtext";
    public override string Description => "Schreibt einen Text auf das angegebene Bild.";
    public override string Syntax => "DrawText(Bild, Text, x, y, Farbe, Schriftgröße);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (attvar.ValueBitmapGet(0) is not { } bmp) { return DoItFeedback.FalscherDatentyp(ld); }
        if (attvar.ValueStringGet(1) is not { Length: > 0 } txt) { return DoItFeedback.FalscherDatentyp(ld); }

        var color = Converter.ColorParse(attvar.ValueStringGet(4));
        var fontSize = attvar.ValueNumGet(5);

        if (fontSize < 1 || fontSize > 1000) {
            return new DoItFeedback("Schriftgröße muss zwischen 1 und 1000 liegen.", true, ld);
        }

        try {
            using var gr = Graphics.FromImage(bmp);
            using var font = new Font("Arial", (float)fontSize, FontStyle.Regular);
            using var brush = new SolidBrush(color);
            gr.DrawString(txt, font, brush, (float)attvar.ValueNumGet(2), (float)attvar.ValueNumGet(3));
        } catch {
            return new DoItFeedback("Text konnte nicht gezeichnet werden.", true, ld);
        }

        return DoItFeedback.Null();
    }

    #endregion
}

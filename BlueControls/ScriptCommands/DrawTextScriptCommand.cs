// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueScript.Classes;
using BlueScript.ScriptVariables;
using static BlueScript.ScriptVariables.BitmapScriptVariable;

namespace BlueScript.ScriptCommands;

/// <summary>
/// Schreibt einen Text auf das angegebene Bild.
/// </summary>
public class DrawTextScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [BmpVar, StringVal, FloatVal, FloatVal, StringVal, FloatVal];
    public override string Command => "drawtext";
    public override string Syntax => "DrawTextScriptCommand(Bild, Text, x, y, Farbe, Schriftgröße);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        if (attvar.ValueBitmapGet(0) is not { } bmp) { return DoItFeedback.FalscherDatentyp(); }
        if (attvar.ValueStringGet(1) is not { Length: > 0 } txt) { return DoItFeedback.FalscherDatentyp(); }

        var color = ColorParse(attvar.ValueStringGet(4));
        var fontSize = attvar.ValueNumGet(5);

        if (fontSize < 1 || fontSize > 1000) {
            return new DoItFeedback("Schriftgröße muss zwischen 1 und 1000 liegen.", true);
        }

        try {
            using var gr = Graphics.FromImage(bmp);
            using var font = new Font("Arial", (float)fontSize, FontStyle.Regular);
            using var brush = new SolidBrush(color);
            gr.DrawString(txt, font, brush, (float)attvar.ValueNumGet(2), (float)attvar.ValueNumGet(3));
        } catch {
            return new DoItFeedback("Text konnte nicht gezeichnet werden.", true);
        }

        return DoItFeedback.Null();
    }

    #endregion
}

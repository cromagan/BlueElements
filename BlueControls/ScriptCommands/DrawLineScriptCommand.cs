// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueScript.Classes;
using BlueScript.ScriptVariables;
using static BlueScript.ScriptVariables.BitmapScriptVariable;

namespace BlueScript.ScriptCommands;

public class DrawLineScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [BmpVar, FloatVal, FloatVal, FloatVal, FloatVal];
    public override string Command => "drawline";
    public override string Description => "Zeichnet eine Linie auf dem angegebenen Bild.";
    public override string Syntax => "DrawLineScriptCommand(Bild, x1, y1, x2, y2);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        if (attvar.ValueBitmapGet(0) is not { } bmp) { return DoItFeedback.FalscherDatentyp(); }

        try {
            using var gr = Graphics.FromImage(bmp);
            gr.DrawLine(Pens.Black, attvar.ValueIntGet(1), attvar.ValueIntGet(2), attvar.ValueIntGet(3), attvar.ValueIntGet(4));
        } catch {
            return new DoItFeedback("Linie konnte nicht gezeichnet werden.", true);
        }

        return DoItFeedback.Null();
    }

    #endregion
}
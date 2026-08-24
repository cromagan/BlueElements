// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.ScriptVariables;

namespace BlueScript.ScriptCommands;

public class ResizeImageScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [BitmapScriptVariable.BmpVar, FloatVal, FloatVal];
    public override string Command => "resizeimage";
    public override string Description => "Verändert die Größe des Bildes";
    public override ScriptCommandType ScriptCommandLevel => ScriptCommandType.LongTime;
    public override bool MustUseReturnValue => true;
    public override string Returns => BitmapScriptVariable.ShortName_Variable;
    public override string Syntax => "ResizeImageScriptCommand(Bild, MaxWidth, MaxHeight);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        if (attvar.ValueBitmapGet(0) is not { } bmp) { return DoItFeedback.FalscherDatentyp(); }

        try {
            var bmp2 = bmp.Resize(attvar.ValueIntGet(1), attvar.ValueIntGet(2),
                SizeModes.Breite_oder_Höhe_Anpassen_MitVergrößern, InterpolationMode.HighQualityBicubic, true);

            return new DoItFeedback(bmp2);
        } catch {
            return new DoItFeedback("Bildgröße konnte nicht verändert werden.", true);
        }
    }

    #endregion
}
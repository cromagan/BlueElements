// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.ScriptVariables;

namespace BlueScript.ScriptCommands;

/// <summary>
/// Verändert die Größe des Bildes
/// </summary>
public class ResizeImageScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [BitmapScriptVariable.BmpVar, FloatVal, FloatVal];
    public override string Command => "resizeimage";
    public override ScriptCommandType ScriptCommandLevel => ScriptCommandType.LongTime;
    public override bool MustUseReturnValue => true;
    public override string Returns => BitmapScriptVariable.ShortName_Variable;
    public override string Syntax => "ResizeImage(Bild, MaxWidth, MaxHeight);";

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

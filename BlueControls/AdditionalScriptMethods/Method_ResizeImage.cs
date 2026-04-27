// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics;
using BlueBasics.Enums;
using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Methods;
using BlueScript.Variables;
using System.Collections.Generic;
using System.Drawing.Drawing2D;

namespace BlueControls.AdditionalScriptMethods;


public class Method_ResizeImage : Method {

    #region Properties

    public override List<List<string>> Args => [VariableBitmap.BmpVar, FloatVal, FloatVal];
    public override string Command => "resizeimage";
    public override List<string> Constants => [];
    public override string Description => "Verändert die Größe des Bildes";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodLevel => MethodType.LongTime;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableBitmap.ShortName_Variable;
    public override string StartSequence => "(";
    public override string Syntax => "ResizeImage(Bild, MaxWidth, MaxHeight);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (attvar.ValueBitmapGet(0) is not { } bmp) { return DoItFeedback.FalscherDatentyp(ld); }

        try {
            var bmp2 = bmp.Resize(attvar.ValueIntGet(1), attvar.ValueIntGet(2),
                SizeModes.Breite_oder_Höhe_Anpassen_MitVergrößern, InterpolationMode.HighQualityBicubic, true);

            return new DoItFeedback(bmp2);
        } catch {
            return new DoItFeedback("Bildgröße konnte nicht verändert werden.", true, ld);
        }
    }

    #endregion
}
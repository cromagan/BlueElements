// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Methods;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueControls.AdditionalScriptMethods;


public class Method_Screenshot : Method {

    #region Properties

    public override string Command => "screenshot";
    public override string Description => "Erstellt einen Screenshot und gibgt diesen zurück.";
    public override MethodType MethodLevel => MethodType.LongTime;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableBitmap.ShortName_Variable;
    public override string Syntax => "Screenshot()";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) => new(ScreenShot.GrabAllScreens());

    #endregion
}
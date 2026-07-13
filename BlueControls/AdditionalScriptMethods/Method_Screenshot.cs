// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Variables;

namespace BlueControls.AdditionalScriptMethods;

public class Method_Screenshot : Method {

    #region Properties

    public override string Command => "screenshot";
    public override string Description => "Erstellt einen Screenshot und gibt diesen zurück.\r\nAlternative: ScreenArea";
    public override MethodType MethodLevel => MethodType.LongTime;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableBitmap.ShortName_Variable;
    public override string Syntax => "Screenshot()";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) => new(ScreenShot.GrabAllScreens());

    #endregion
}
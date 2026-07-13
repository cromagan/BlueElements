// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Variables;

namespace BlueControls.AdditionalScriptMethods;

public class Method_Screenarea : Method {

    #region Properties

    public override string Command => "screenarea";
    public override string Description => "Erstellt einen Screenshot, lässt den benutzer einen Bereich wählen\rund gibt diesen zurück.";
    public override MethodType MethodLevel => MethodType.ManipulatesUser;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableBitmap.ShortName_Variable;
    public override string Syntax => "Screenarea()";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) => new(ScreenShot.GrabArea(null).Screen);

    #endregion
}
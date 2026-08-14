// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.ScriptVariables;

namespace BlueScript.ScriptCommands;

public class ScreenareaScriptCommand : ScriptCommand {

    #region Properties

    public override string Command => "screenarea";
    public override string Description => "Erstellt einen ScreenshotScriptCommand, lässt den benutzer einen Bereich wählen\rund gibt diesen zurück.";
    public override bool MustUseReturnValue => true;
    public override string Returns => BitmapScriptVariable.ShortName_Variable;
    public override ScriptCommandType ScriptCommandLevel => ScriptCommandType.ManipulatesUser;
    public override string Syntax => "ScreenareaScriptCommand()";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) => new(BlueControls.ScreenShot.GrabArea(null).Screen);

    #endregion
}
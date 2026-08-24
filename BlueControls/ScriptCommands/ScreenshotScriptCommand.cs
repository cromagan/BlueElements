// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.ScriptVariables;

namespace BlueScript.ScriptCommands;

public class ScreenshotScriptCommand : ScriptCommand {

    #region Properties

    public override string Command => "screenshot";
    public override string Description => "Erstellt einen ScreenshotScriptCommand und gibt diesen zurück.\r\nAlternative: ScreenArea";
    public override ScriptCommandType ScriptCommandLevel => ScriptCommandType.LongTime;
    public override bool MustUseReturnValue => true;
    public override string Returns => BitmapScriptVariable.ShortName_Variable;
    public override string Syntax => "ScreenshotScriptCommand()";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) => new(BlueControls.ScreenShot.GrabAllScreens());

    #endregion
}
// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.ScriptVariables;

namespace BlueScript.ScriptCommands;

/// <summary>
/// Erstellt einen ScreenshotScriptCommand und gibt diesen zurück.
/// Alternative: ScreenArea
/// </summary>
public class ScreenshotScriptCommand : ScriptCommand {

    #region Properties

    public override string Command => "screenshot";
    public override ScriptCommandType ScriptCommandLevel => ScriptCommandType.LongTime;
    public override bool MustUseReturnValue => true;
    public override string Returns => BitmapScriptVariable.ShortName_Variable;
    public override string Syntax => "ScreenshotScriptCommand()";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) => new(BlueControls.ScreenShot.GrabAllScreens());

    #endregion
}

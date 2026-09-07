// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.ScriptVariables;

namespace BlueScript.ScriptCommands;

/// <summary>
/// Erstellt einen ScreenshotScriptCommand, lässt den benutzer einen Bereich wählen
/// und gibt diesen zurück.
/// </summary>
public class ScreenareaScriptCommand : ScriptCommand {

    #region Properties

    public override string Command => "screenarea";
    public override bool MustUseReturnValue => true;
    public override string Returns => BitmapScriptVariable.ShortName_Variable;
    public override ScriptCommandType ScriptCommandLevel => ScriptCommandType.ManipulatesUser;
    public override string Syntax => "ScreenareaScriptCommand()";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) => new(BlueControls.ScreenShot.GrabArea(null).Screen);

    #endregion
}

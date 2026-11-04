// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.ScriptVariables;

namespace BlueControls.ScriptCommands;

/// <summary>
/// Öffnet ein Fenster und zeigt den übergebenen Text an. Das Skript läuft dabei weiter.
/// </summary>
public class ShowTextScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [[StringScriptVariable.ClassId], [StringScriptVariable.ClassId]];
    public override string Command => "showtext";
    public override ScriptCommandType ScriptCommandLevel => ScriptCommandType.GUI;
    public override string Syntax => "ShowText(Text, Fenstertitel);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        var txt = attvar.ValueStringGet(0);
        var caption = attvar.ValueStringGet(1);

        var form = new TextView(txt, caption);
        FormManager.RegisterForm(form);
        form.Show();

        return DoItFeedback.Null();
    }

    #endregion
}

// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.ScriptVariables;
using Formats = BlueBasics.Classes.Formats;

namespace BlueScript.ScriptCommands;

/// <summary>
/// Zeigt ein Eingabefenster an und wartet, bis der Nutzer einen Text eingibt und bestätigt.
/// Der erste Parameter ist der Anzeigetext, der zweite der Vorgabewert.
/// Alle weiteren Parameter werden als Vorschläge in einer Auswahlliste angeboten.
/// </summary>
public class InputBoxScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal, StringVal];
    public override string Command => "inputbox";
    public override LastArgMinCountTypeScriptCommand LastArgMinCount => LastArgMinCountTypeScriptCommand.Optional;
    public override ScriptCommandType ScriptCommandLevel => ScriptCommandType.GUI;
    public override string Returns => StringScriptVariable.ShortName_Plain;
    public override string Syntax => "InputBoxScriptCommand(Text, Vorgabetext, Vorschlag1, Vorschlag2, ...);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        var txt = attvar.ValueStringGet(0);
        var defaultText = attvar.ValueStringGet(1);

        List<string> suggestions = [];
        for (var z = 2; z < attvar.Attributes.Count; z++) {
            suggestions.Add(attvar.ValueStringGet(z));
        }

        string result;

        if (suggestions.Count > 0) {
            result = InputBoxComboStyle.Show(txt, Formats.TextFormat.Instance, suggestions, true);
        } else {
            result = InputBox.Show(txt, defaultText, Formats.TextFormat.Instance);
        }

        return new DoItFeedback(result);
    }

    #endregion
}

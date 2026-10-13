// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueScript.Classes;
using BlueScript.ScriptVariables;

namespace BlueScript.ScriptCommands;

/// <summary>
/// Zeigt eine QuickNote in der Nähe des Mauscursors mit maximal drei Wörtern an. Als Symbol kann Pencil, Ok, Warning oder Critical benutzt werden.
/// </summary>
public class QuickNoteScriptCommand : TableGenericScriptCommand {

    #region Properties

    public override List<List<string>> Args => [
        [StringScriptVariable.ClassId],
        [StringScriptVariable.ClassId]
    ];

    public override string Command => "quicknote";

    public override string Syntax => "QuickNote(Symbol, Text);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        var symbolText = attvar.ValueStringGet(0);
        var text = attvar.ValueStringGet(1);

        if (!Enum.TryParse<NoteSymbols>(symbolText, true, out var symbol)) {
            return new DoItFeedback($"QuickNote kennt das Symbol '{symbolText}' nicht. Erlaubt sind: Pencil, Ok, Warning, Critical.", true);
        }

        var words = text.Split([' ', '\t', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

        if (words.Length > 3) {
            return new DoItFeedback("QuickNote erlaubt maximal drei Wörter.", true);
        }

        if (text.Contains('{') || text.Contains('}')) {
            return new DoItFeedback("QuickNote erlaubt keine Platzhalter '{}'.", true);
        }

        QuickNote.Show(symbol, text);

        return DoItFeedback.Null();
    }

    #endregion
}
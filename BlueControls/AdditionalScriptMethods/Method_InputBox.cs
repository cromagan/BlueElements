// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Variables;

namespace BlueControls.AdditionalScriptMethods;

public class Method_InputBox : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal, StringVal];
    public override string Command => "inputbox";
    public override string Description => "Zeigt ein Eingabefenster an und wartet, bis der Nutzer einen Text eingibt und bestätigt.\r\nDer erste Parameter ist der Anzeigetext, der zweite der Vorgabewert.\r\nAlle weiteren Parameter werden als Vorschläge in einer Auswahlliste angeboten.";
    public override LastArgMinCountType LastArgMinCount => LastArgMinCountType.Optional;
    public override MethodType MethodLevel => MethodType.GUI;
    public override string Returns => VariableString.ShortName_Plain;
    public override string Syntax => "InputBox(Text, Vorgabetext, Vorschlag1, Vorschlag2, ...);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var txt = attvar.ValueStringGet(0);
        var defaultText = attvar.ValueStringGet(1);

        List<string> suggestions = [];
        for (var z = 2; z < attvar.Attributes.Count; z++) {
            suggestions.Add(attvar.ValueStringGet(z));
        }

        string result;

        if (suggestions.Count > 0) {
            result = InputBoxComboStyle.Show(txt, FormatHolder_Text.Instance, suggestions, true);
        } else {
            result = InputBox.Show(txt, defaultText, FormatHolder_Text.Instance);
        }

        return new DoItFeedback(result);
    }

    #endregion
}
// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.ScriptVariables;

namespace BlueScript.ScriptCommands;

public class MsgBoxScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal, StringVal];
    public override string Command => "msgbox";
    public override string Description => "Zeigt ein Windows-Fenster an und wartet, dass der Nutzer eine Schaltfläche drückt.\r\nEs wird die Nummer (beginnend mit 0) des Knopfes zurückgegeben.\r\nAls Bild kann z.B. 'Information', 'Warnung', 'Kritisch', 'Uhr', etc. benutzt oder leer gelassen werden.";
    public override LastArgMinCountTypeScriptCommand LastArgMinCount => LastArgMinCountTypeScriptCommand.Optional;
    public override ScriptCommandType ScriptCommandLevel => ScriptCommandType.GUI;
    public override string Returns => Double.ShortName_Variable;
    public override string Syntax => "MsgBoxScriptCommand(Text, Bild, Schaltflächenbeschriftung, ...);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        var txt = attvar.ValueStringGet(0);

        var img = attvar.ValueStringGet(1);
        var pic = ImageCode.Information;

        if (Enum.TryParse(img, out ImageCode type)) { pic = type; }

        List<string> buttons = [];
        for (var z = 2; z < attvar.Attributes.Count; z++) {
            buttons.Add(attvar.ValueStringGet(z));
        }

        if (buttons.Count == 0) { buttons.Add("Ok"); }

        var l = MessageBox.Show(txt, pic, true, [.. buttons]);

        return new DoItFeedback(l);
    }

    #endregion
}
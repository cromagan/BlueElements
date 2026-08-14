// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueScript.Classes;
using BlueScript.ScriptVariables;
using System.Windows.Forms;

namespace BlueScript.ScriptCommands;

internal class ClipboardTextScriptCommand : ScriptCommand {

    #region Properties

    public override string Command => "clipboardtext";
    public override string Description => "Gibt den Inhalt des Windows Clipboards als Text zurück. Falls kein Text im Clipboard enthalten ist, wird ein leerer StringScriptCommand zurückgegeben.\r\nMit SetClipoard kann ein Wert in das Clipboard geschrieben werden.";
    public override bool MustUseReturnValue => true;
    public override string Returns => StringScriptVariable.ShortName_Plain;
    public override string Syntax => "ClipboardTextScriptCommand()";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) => Clipboard.ContainsText() ? new DoItFeedback(Clipboard.GetText()) : DoItFeedback.Null();

    #endregion
}
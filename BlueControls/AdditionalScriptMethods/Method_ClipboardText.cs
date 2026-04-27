// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Methods;
using BlueScript.Variables;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BlueControls.AdditionalScriptMethods;

internal class Method_ClipboardText : Method {

    #region Properties

    public override List<List<string>> Args => [];
    public override string Command => "clipboardtext";
    public override List<string> Constants => [];
    public override string Description => "Gibt den Inhalt des Windows Clipboards als Text zurück. Falls kein Text im Clipboard enthalten ist, wird ein leerer String zurückgegeben.\r\nMit SetClipoard kann ein Wert in das Clipboard geschrieben werden.";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodLevel => MethodType.Standard;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableString.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "ClipboardText()";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) => Clipboard.ContainsText() ? new DoItFeedback(Clipboard.GetText()) : DoItFeedback.Null();

    #endregion
}
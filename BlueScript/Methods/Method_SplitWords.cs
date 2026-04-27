// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics;
using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueScript.Methods;


internal class Method_SplitWords : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal];
    public override string Command => "splitwords";
    public override List<string> Constants => [];
    public override string Description => "Gibt eine Liste aller Wörter zurück.\r\nDie Liste ist nach die Zeichen-Länge der Wörter absteigend sortiert.\r\nJedes Wort ist nur einmal in der Liste.";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodLevel => MethodType.Standard;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableListString.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "SplitWords(String)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var txt = attvar.ValueStringGet(0);

        var list = txt.AllWords().SortedDistinctList();

        list.Sort((s1, s2) => s2.Length.CompareTo(s1.Length));

        return new DoItFeedback(list);
    }

    #endregion
}
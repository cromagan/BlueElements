// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.ClassesStatic;
using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Variables;
using System.Collections.Generic;
using static BlueBasics.ClassesStatic.Converter;

namespace BlueScript.Methods;


internal class Method_StringToUTF8 : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal];
    public override string Command => "stringtoutf8";
    public override List<string> Constants => [];
    public override string Description => "Ersetzt einen ASCII-String nach UTF8.";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodLevel => MethodType.Standard;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableString.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "StringToUTF8(String, IgnoreBRbool)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) => new(attvar.ValueStringGet(0).StringtoUtf8());

    #endregion
}
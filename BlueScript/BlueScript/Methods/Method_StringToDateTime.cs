﻿#nullable enable

using BlueScript.Enums;
using BlueScript.Methods;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;
using static BlueBasics.Converter;

namespace BildzeichenListe.AdditionalScriptMethods;

public class Method_StringToDateTime : Method {

    #region Properties

    public override List<List<string>> Args => new() { StringVal };
    public override string Command => "stringtodatetime";
    public override string Description => "Wandelt einen Time-String ein Datum um.";
    public override bool EndlessArgs => false;
    
    public override bool GetCodeBlockAfter => false;
    public override MethodType MethodType => MethodType.Standard;
    public override string Returns => VariableDateTime.ShortName_Variable;

    public override string StartSequence => "(";

    public override string Syntax => "StringToDateTime(String)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(varCol, infos.AttributText, Args, EndlessArgs, infos.Data, scp);
        var ok = DateTimeTryParse(attvar.ReadableText(0), out var d);
        if (!ok) {
            return new DoItFeedback(infos.Data, "Der Wert '" + attvar.ReadableText(0) + "' wurde nicht als Zeitformat erkannt.");
        }
        return new DoItFeedback(d);
    }

    #endregion
}
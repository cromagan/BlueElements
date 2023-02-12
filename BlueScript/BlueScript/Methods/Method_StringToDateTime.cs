#nullable enable

using System.Collections.Generic;
using BlueScript;
using BlueScript.Methods;
using BlueScript.Structures;
using BlueScript.Variables;
using static BlueBasics.Converter;

namespace BildzeichenListe.AdditionalScriptComands;

public class Method_StringToDateTime : Method {

    #region Properties

    public override List<List<string>> Args => new() { new List<string> { VariableString.ShortName_Plain } };

    public override string Description => "Wandelt einen Time-String ein Datum um.";

    public override bool EndlessArgs => false;

    public override string EndSequence => ")";

    public override bool GetCodeBlockAfter => false;
    public override string Returns => VariableDateTime.ShortName_Variable;

    public override string StartSequence => "(";

    public override string Syntax => "StringToDateTime(String)";

    #endregion

    #region Methods

    public override List<string> Comand(Script? s) => new() { "stringtodatetime" };

    public override DoItFeedback DoIt(CanDoFeedback infos, Script s) {
        var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
        var ok = DateTimeTryParse(attvar.Attributes[0].ReadableText, out var d);
        if (!ok) {
            return new DoItFeedback(infos, s, "Der Wert '" + attvar.Attributes[0].ReadableText + "' wurde nicht als Zeitformat erkannt.");
        }
        return new DoItFeedback(infos, s, d);
    }

    #endregion
}
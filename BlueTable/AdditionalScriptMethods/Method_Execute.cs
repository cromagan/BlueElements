// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.ClassesStatic;
using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Methods;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueTable.AdditionalScriptMethods;


internal class Method_Execute : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal];


    public override string Command => "execute";


    public override string Description => "Gibt den Befehl an Windows ab.\r\n" +
                                                  "Versucht das Beste daraus zu machen,\r\n";

    public override MethodType MethodLevel => MethodType.GUI;


    public override string Syntax => "Execute(Command, Attribut);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        IO.ExecuteFile(attvar.ValueStringGet(0), attvar.ValueStringGet(1), false, false);

        return DoItFeedback.Null();
    }



    #endregion
}
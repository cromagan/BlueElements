// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Variables;
using System.Collections.Generic;
using System.Net.NetworkInformation;

namespace BlueScript.Methods;


internal class Method_Ping : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal];
    public override string Command => "ping";
    public override string Description => "Pingt einen Server an und gibt dessen Reaktionszeit in Millsekunden zurück.\r\nTritt ein Fehler auf, für 9999 zurück gegeben.";
    public override MethodType MethodLevel => MethodType.LongTime;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableDouble.ShortName_Plain;
    public override string Syntax => "Ping(ServerAdresse)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        try {
            var p = new Ping();
            var r = p.Send(attvar.ValueStringGet(0));
            if (r.Status == IPStatus.Success) {
                return new DoItFeedback(r.RoundtripTime);
            }
        } catch { }

        return new DoItFeedback(9999);
    }

    #endregion
}
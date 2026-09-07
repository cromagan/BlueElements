// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Net.NetworkInformation;

namespace BlueScript.ScriptCommands;


/// <summary>
/// Pingt einen Server an und gibt dessen Reaktionszeit in Millsekunden zurück.
/// Tritt ein Fehler auf, für 9999 zurück gegeben.
/// </summary>
internal class PingScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [StringVal];
    public override string Command => "ping";
    public override ScriptCommandType ScriptCommandLevel => ScriptCommandType.LongTime;
    public override bool MustUseReturnValue => true;
    public override string Returns => DoubleScriptVariable.ShortName_Plain;
    public override string Syntax => "PingScriptCommand(ServerAdresse)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
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

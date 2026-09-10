// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.ScriptVariables;
using System.Globalization;

namespace BlueScript.ScriptCommands;

/// <summary>
/// Öffnet die Tabelle in den TableViews — als neuer Reiter oder, bei Überschreiben=true, im aktuell angezeigten Reiter.
/// Mit dem Attribut WindowID wird das Ziel-Fenster eingeschränkt; bei leerer Angabe werden alle TableViews bedient.
/// </summary>
internal class OpenTabScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [TableVar, BoolVal, StringVal];
    public override string Command => "opentab";
    public override ScriptCommandType ScriptCommandLevel => ScriptCommandType.GUI;
    public override string Syntax => "OpenTab(Table, AktuellerTabÜberschreiben, WindowID);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        if (attvar.Attributes[0] is not TableScriptVariable vtb || vtb.ValueTable is not { IsDisposed: false } tb) {
            return new DoItFeedback("Tabelle nicht vorhanden", true);
        }

        var aktuellerTabÜberschreiben = attvar.ValueBoolGet(1);
        var fensterId = attvar.ValueStringGet(2);

        foreach (var thisForm in FormManager.Forms) {
            if (thisForm is not TableViewForm thisTbv) { continue; }
            if (fensterId is { Length: > 0 } fid && thisTbv.Handle.ToString(CultureInfo.InvariantCulture) != fid) { continue; }

            if (aktuellerTabÜberschreiben) {
                if (!scp.ProduktivPhase) { return DoItFeedback.TestModusInaktiv(); }
                thisTbv.ReplaceCurrentTab(tb.KeyName);
            } else if (thisTbv.TabExists(tb.KeyName) is null) {
                if (!scp.ProduktivPhase) { return DoItFeedback.TestModusInaktiv(); }
                thisTbv.AddTabPage(tb.KeyName);
            }
        }

        return DoItFeedback.Null();
    }

    #endregion
}
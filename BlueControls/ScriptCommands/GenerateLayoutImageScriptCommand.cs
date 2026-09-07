// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.ScriptVariables;

namespace BlueScript.ScriptCommands;

/// <summary>
/// Generiert ein Layout Bild.
/// Es wird zuvor das Skript 'Export' ausgeführt und dessen Variablen verwendet.
/// </summary>
public class GenerateLayoutImageScriptCommand : TableGenericScriptCommand {

    #region Properties

    public override List<List<string>> Args => [StringVal, FloatVal];
    public override string Command => "generatelayoutimage";
    public override bool MustUseReturnValue => true;
    public override string Returns => BitmapScriptVariable.ShortName_Variable;
    public override ScriptCommandType ScriptCommandLevel => ScriptCommandType.LongTime;
    public override string Syntax => "GenerateLayoutImageScriptCommand(LayoutName, Skalierung);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {

        #region  Meine Zeile ermitteln (r)

        var r = BlockedRow(scp);
        if (r?.Table is not { IsDisposed: false }) { return new DoItFeedback("Zeilenfehler!", true); }

        #endregion

        #region  Layout index ermitteln (ind)

        var ind = attvar.ValueStringGet(0);

        if (string.IsNullOrEmpty(ind)) { return new DoItFeedback("Layout nicht gefunden.", true); }

        #endregion

        #region  scale  ermitteln (sc)

        var sc = attvar.ValueNumGet(1);
        if (sc is < 0.1 or > 10) { return new DoItFeedback("Skalierung nur von 0.1 bis 10 erlaubt.", true); }

        #endregion

        using var l = new CollectionPadItem(ind);

        if (!l.Any()) { return new DoItFeedback("Layout nicht gefunden oder fehlerhaft.", true); }

        l.ResetVariables();
        var scx = l.ReplaceVariables(r);

        if (scx.Failed) {
            scx.ChangeFailedReason("Generierung fehlgeschlagen", scx.NeedsScriptFix);
            return scx;
        }

        var bmp = l.ToBitmap((float)sc);

        return bmp is null ? new DoItFeedback("Generierung fehlgeschlagen", true) : new DoItFeedback(bmp);
    }

    #endregion
}

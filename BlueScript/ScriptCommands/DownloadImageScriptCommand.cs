// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.ScriptCommands;

internal class DownloadImageScriptCommand : ScriptCommand {

    #region Fields

    private static readonly VariableCollection Last = [];

    #endregion

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal, StringVal];
    public override string Command => "downloadimage";
    public override string Description => "Lädt das angegebene Bild aus dem Internet.\r\nDiese Routine wird keinen Fehler auslösen.\r\nFalls etwas schief läuft, enthält die Variable ein Bild des Wertes NULL.";
    public override bool MustUseReturnValue => true;
    public override string Returns => BitmapScriptVariable.ShortName_Variable;
    public override ScriptCommandType ScriptCommandLevel => ScriptCommandType.LongTime;
    public override string Syntax => "DownloadImage(url, username, password)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        // Da es keine Möglichkeit gibt, eine Bild Variable (außerhalb eines If) zu deklarieren,
        // darf diese Routine nicht fehlschlagen.

        var url = attvar.ValueStringGet(0);
        var varn = "X" + url.ReduceToChars(AllowedCharsVariableName);

        if (Last.GetByKey(varn) is BitmapScriptVariable vb) {
            return new DoItFeedback(vb.ValueBitmap);
        }

        try {
            CollectGarbage();
            var img = DownloadImage(url);
            System.Drawing.Bitmap? bmp = null;
            if (img is System.Drawing.Bitmap bmp2) { bmp = bmp2; }

            Last.Add(new BitmapScriptVariable(varn, bmp, true, string.Empty));
            return new DoItFeedback(bmp);
        } catch {
            return new DoItFeedback(null as System.Drawing.Bitmap);
        }
    }

    #endregion
}
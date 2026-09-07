// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueScript.ScriptCommands;

/// <summary>
/// Lädt das angegebene Bild aus dem Internet.
/// Diese Routine wird keinen Fehler auslösen.
/// Falls etwas schief läuft, enthält die Variable ein Bild des Wertes NULL.
/// </summary>
internal class DownloadImageScriptCommand : ScriptCommand {

    #region Fields

    private static readonly VariableCollection Last = [];

    #endregion

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal, StringVal];
    public override string Command => "downloadimage";
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

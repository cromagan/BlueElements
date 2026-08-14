// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Drawing.Imaging;
using static BlueScript.ScriptVariables.BitmapScriptVariable;

namespace BlueScript.ScriptCommands;

internal class BitmapToBase64ScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [BmpVar, StringVal];
    public override string Command => "bitmaptobase64";
    public override List<string> Constants => ["PNG", "JPG", "BMP"];
    public override string Description => "Konvertiert das Bild in das Base64 Format und gibt dessen String zurück.";
    public override bool MustUseReturnValue => true;
    public override string Returns => StringScriptVariable.ShortName_Plain;
    public override ScriptCommandType ScriptCommandLevel => ScriptCommandType.LongTime;
    public override string Syntax => "BitmapToBase64(Bitmap, JPG/PNG/BMP)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        string x;

        switch (attvar.ValueStringGet(1).ToUpperInvariant()) {
            case "JPG":
                x = BitmapToBase64(attvar.ValueBitmapGet(0), ImageFormat.Jpeg);
                break;

            case "PNG":
                x = BitmapToBase64(attvar.ValueBitmapGet(0), ImageFormat.Png);
                break;

            case "BMP":
                x = BitmapToBase64(attvar.ValueBitmapGet(0), ImageFormat.Bmp);
                break;

            default:
                return new DoItFeedback("Es wir als zweites Attribut ein String mit dem Inhalt bmp, jpg oder png erwartet.", true);
        }

        return new DoItFeedback(x);
    }

    #endregion
}
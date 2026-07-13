// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Drawing.Imaging;
using static BlueBasics.ClassesStatic.IO;

using static BlueScript.Variables.VariableBitmap;

namespace BlueScript.Methods;

internal class Method_SaveImage : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal, BmpVar];
    public override string Command => "saveimage";
    public override List<string> Constants => ["PNG", "JPG", "BMP"];
    public override string Description => "Speichert das Bild auf die Festplatte";
    public override MethodType MethodLevel => MethodType.LongTime;
    public override string Syntax => "SaveImage(Filename, PNG/JPG/BMP, Bild);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {

        #region  Bild ermitteln (img)

        var img = attvar.ValueBitmapGet(2);
        if (img is null) { return new DoItFeedback("Bild fehlerhaft.", true); }

        #endregion

        #region  Dateinamen ermitteln (filn)

        var filn = attvar.ValueStringGet(0);
        if (!filn.IsFormat(FormatHolder_FilepathAndName.Instance)) { return new DoItFeedback("Dateinamen-Fehler!", true); }

        var opr = CanWriteInDirectory(filn.FilePath());
        if (opr.IsFailed) { return new DoItFeedback(opr.FailedReason, true); }

        if (FileExists(filn)) { return new DoItFeedback("Datei existiert bereits.", true); }

        #endregion

        //if (!scp.ChangeValues) { return new DoItFeedback(ld, "Bild Speichern im Testmodus deaktiviert."); }

        switch (attvar.ValueStringGet(1).ToUpperInvariant()) {
            case "PNG":
                img.Save(filn, ImageFormat.Png);

                break;

            case "BMP":
                img.Save(filn, ImageFormat.Bmp);

                break;

            case "JPG":
            case "JPEG":
                img.Save(filn, ImageFormat.Jpeg);

                break;

            default:
                return new DoItFeedback("Export-Format unbekannt.", true);
        }

        return DoItFeedback.Null();
    }

    #endregion
}
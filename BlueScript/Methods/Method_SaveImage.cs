// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Classes;
using BlueBasics.ClassesStatic;
using BlueBasics.Interfaces;
using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Variables;
using System.Collections.Generic;
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

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {

        #region  Bild ermitteln (img)

        var img = attvar.ValueBitmapGet(2);
        if (img == null) { return new DoItFeedback("Bild fehlerhaft.", true, ld); }

        #endregion

        #region  Dateinamen ermitteln (filn)

        var filn = attvar.ValueStringGet(0);
        if (string.IsNullOrEmpty(filn)) { return new DoItFeedback("Dateinamen-Fehler!", true, ld); }

        if (!filn.IsFormat(FormatHolder.FilepathAndName)) { return new DoItFeedback("Dateinamen-Fehler!", true, ld); }

        var opr = CanWriteInDirectory(filn.FilePath());
        if (!string.IsNullOrEmpty(opr)) { return new DoItFeedback(opr, true, ld); }

        if (FileExists(filn)) { return new DoItFeedback("Datei existiert bereits.", true, ld); }

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
                return new DoItFeedback("Export-Format unbekannt.", true, ld);
        }

        return DoItFeedback.Null();
    }

    #endregion
}
// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.DrawingHelpers;
using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.ScriptVariables;
using static BlueScript.ScriptVariables.BitmapScriptVariable;

namespace BlueScript.ScriptCommands;

internal class CheckBitmapScriptCommand : ScriptCommand, ICommandBuilder {

    #region Properties

    public override List<List<string>> Args => [BmpVar, FloatVal, FloatVal, StringVal];

    public override string Command => "checkbitmap";

    public override string Description => "Prüft auf den XY-Koordinaten, ob dort ein bestimmtes Bild abgebildet ist. Zum Erstellen des Befehls den Assistenten benutzen.";

    public override bool MustUseReturnValue => true;
    public override string Returns => BoolScriptVariable.ShortName_Plain;
    public override ScriptCommandType ScriptCommandLevel => ScriptCommandType.LongTime;
    public override string Syntax => "CheckBitmapScriptCommand(BMP, X,Y, HasCode)";

    #endregion

    #region Methods

    public string CommandDescription() => "Prüfe, ob auf dem Bildchirm etwas Bestimmtes zu sehen ist.";

    public QuickImage CommandImage() => QuickImage.Get(ImageCode.Bild, 16);

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        if (attvar.ValueBitmapGet(0) is not { } bmp) { return DoItFeedback.FalscherDatentyp(); }

        var x = attvar.ValueIntGet(1);
        var y = attvar.ValueIntGet(2);
        using var bmpa = bmp.Crop(x - 10, y - 5, 20, 10);
        return new DoItFeedback(BitmapToBase64(bmpa, ImageFormat.Bmp).GetMD5Hash() == attvar.ValueStringGet(3));
    }

    public string GetCode(Form? form) {
        var c = BlueControls.ScreenShot.GrabAndClick("Wählen sie den Punkt, der geprüft werden soll.", form, [Rectangle20x10DrawingHelper.Instance]);

        if (c.Screen is null) { return string.Empty; }

        var n = InputBox.Show("Variablenname:", "result", BlueBasics.Classes.Formats.SystemNameFormat.Instance);

        if (string.IsNullOrEmpty(n)) {
            n = "result";
        }

        using var bmpa = c.Screen.Crop(c.Point1.X - 10, c.Point1.Y - 5, 20, 10);
        return $"var sc = ScreenshotScriptCommand();\r\nvar {n} = CheckBitmapScriptCommand(sc, {c.Point1.X}, {c.Point1.Y}, \"{BitmapToBase64(bmpa, ImageFormat.Bmp).GetSHA256HashString()}\");";
    }

    #endregion
}
// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.ScriptCommands;

/// <summary>
/// Lädt das angegebene Bild aus dem Dateisystem.
/// Diese Routine wird keinen Fehler auslösen.
/// Falls etwas schief läuft, enthält die Variable ein Bild des Wertes NULL.
/// </summary>
internal class LoadImageScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [StringVal];
    public override string Command => "loadimage";
    public override ScriptCommandType ScriptCommandLevel => ScriptCommandType.LongTime;
    public override bool MustUseReturnValue => true;
    public override string Returns => BitmapScriptVariable.ShortName_Variable;
    public override string Syntax => "LoadImageScriptCommand(Filename)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        // Da es keine Möglichkeit gibt, eine Bild Variable (außerhalb eines If) zu deklarieren,
        // darf diese Routine nicht fehlschlagen.

        try {
            CollectGarbage();
            if (Image_FromFile(attvar.ValueStringGet(0)) is System.Drawing.Bitmap bmp) { return new DoItFeedback(bmp); }
        } catch { }

        return new DoItFeedback(null as System.Drawing.Bitmap);
    }

    #endregion
}

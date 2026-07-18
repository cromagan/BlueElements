// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Drawing;

namespace BlueScript.Methods;

internal class Method_LoadImage : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal];
    public override string Command => "loadimage";
    public override string Description => "Lädt das angegebene Bild aus dem Dateisystem.\r\nDiese Routine wird keinen Fehler auslösen.\r\nFalls etwas schief läuft, enthält die Variable ein Bild des Wertes NULL.";
    public override MethodType MethodLevel => MethodType.LongTime;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableBitmap.ShortName_Variable;
    public override string Syntax => "LoadImage(Filename)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        // Da es keine Möglichkeit gibt, eine Bild Variable (außerhalb eines If) zu deklarieren,
        // darf diese Routine nicht fehlschlagen.

        try {
            Generic.CollectGarbage();
            if (Image_FromFile(attvar.ValueStringGet(0)) is Bitmap bmp) { return new DoItFeedback(bmp); }
        } catch { }

        return new DoItFeedback(null as Bitmap);
    }

    #endregion
}
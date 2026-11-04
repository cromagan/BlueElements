// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.ScriptVariables;

namespace BlueControls.ScriptCommands;

/// <summary>
/// Öffnet ein Fenster und zeigt das übergebene Bild an. Das Skript läuft dabei weiter.
/// </summary>
public class ShowImageScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [[BitmapScriptVariable.ClassId], [StringScriptVariable.ClassId]];
    public override string Command => "showimage";
    public override ScriptCommandType ScriptCommandLevel => ScriptCommandType.GUI;
    public override string Syntax => "ShowImage(Bild, Fenstertitel);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        var bmp = attvar.ValueBitmapGet(0);
        if (bmp is null) { return new DoItFeedback("Bild fehlerhaft.", true); }

        var form = new PictureView(bmp);

        var caption = attvar.ValueStringGet(1);
        if (!string.IsNullOrEmpty(caption)) { form.Text = caption; }

        FormManager.RegisterForm(form);
        form.Show();

        return DoItFeedback.Null();
    }

    #endregion
}

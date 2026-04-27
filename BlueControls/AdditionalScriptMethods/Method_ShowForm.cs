// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Forms;
using BlueScript.Enums;
using BlueScript.Methods;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueControls.AdditionalScriptMethods;


public class Method_ShowForm : Method {

    #region Properties

    public override List<List<string>> Args => [[VariableItemCollectionPad.ShortName_Variable]];
    public override string Command => "showform";
    public override string Description => "Zeigt ein Windows-Fenster mit dem angegebenen Inhalt an";
    public override MethodType MethodLevel => MethodType.GUI;
    public override string Returns => VariableDouble.ShortName_Variable;
    public override string Syntax => "ShowForm(Inhalt);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (attvar.Attributes[0] is not VariableItemCollectionPad icp) { return DoItFeedback.InternerFehler(ld); }
        if (icp.ValueItemCollection is not { IsDisposed: false } icpv) { return DoItFeedback.InternerFehler(ld); }

        var c = new PadEditor();
        c.Pad.Items = icpv;
        c.Show();

        //var txt = attvar.ValueStringGet(0);

        //var img = attvar.ValueStringGet(1);
        //var pic = ImageCode.Information;

        //if (Enum.TryParse(img, out ImageCode type)) { pic = type; }

        //List<string> buttons = [];
        //for (var z = 2; z < attvar.Attributes.Count; z++) {
        //    buttons.Add(attvar.ValueStringGet(z));
        //}

        //if (buttons.Count == 0) { buttons.Add("Ok"); }

        //var l = MessageBox.Show(txt, pic, true, buttons.ToArray());

        return DoItFeedback.Null();
    }

    #endregion
}
// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
// https://github.com/cromagan/BlueElements
//
// License: GNU Affero General Public License v3.0
// https://github.com/cromagan/BlueElements/blob/master/LICENSE
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

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
    public override List<string> Constants => [];
    public override string Description => "Zeigt ein Windows-Fenster mit dem angegebenen Inhalt an";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodLevel => MethodType.GUI;
    public override bool MustUseReturnValue => false;
    public override string Returns => VariableDouble.ShortName_Variable;
    public override string StartSequence => "(";
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
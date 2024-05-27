// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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

#nullable enable

using BlueBasics;
using BlueScript.Enums;
using BlueScript.EventArgs;
using BlueScript.Interfaces;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace BlueScript.Methods;

// ReSharper disable once UnusedMember.Global
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
internal class Method_Execte : Method, IUseableForButton {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal];

    public List<List<string>> ArgsForButton => Args;

    public List<string> ArgsForButtonDescription => ["Befehl", "Attribute"];

    public ButtonArgs ClickableWhen => ButtonArgs.Egal;
    public override string Command => "execute";

    public override string Description => "Gibt den Befehl an Windows ab.\r\n" +
                                              "Versucht das Beste daraus zu machen,\r\n";

    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodType => MethodType.IO  | MethodType.ManipulatesUser;

    public override bool MustUseReturnValue => false;
    public string NiceTextForUser => "Einen Befehl an Windows übergeben";
    public override string Returns => string.Empty;
    public override string StartSequence => "(";
    public override string Syntax => "Execute(Command, Attribut);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(varCol, infos.AttributText, Args, LastArgMinCount, infos.Data, scp);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos.Data, this, attvar); }

        //const string comment = "Mit dem Befehl 'ExtractTags' erstellt";
        //varCol.RemoveWithComment(comment);

        //var tags = new List<string>();
        //if (attvar.Attributes[0] is VariableString vs) { tags.Add(vs.ValueString); }
        //if (attvar.Attributes[0] is VariableListString vl) { tags.AddRange(vl.ValueList); }

        //foreach (var thisw in tags) {
        //    var x = thisw.SplitBy(attvar.ValueStringGet(1));

        //    if (x.Length == 2) {
        //        var vn = x[0].ToLowerInvariant().ReduceToChars(Constants.AllowedCharsVariableName);
        //        var thisv = x[1].Trim();
        //        if (!string.IsNullOrEmpty(vn) && !string.IsNullOrEmpty(thisv)) {
        //            varCol.Add(new VariableString("extracted_" + vn, thisv, true, comment));
        //        }
        //    }
        //}

        _ = IO.ExecuteFile(attvar.ValueStringGet(0), attvar.ValueStringGet(1), false, false);

        return DoItFeedback.Null();
    }

    public string TranslateButtonArgs(string arg1, string arg2, string arg3, string arg4, string filterarg, string rowarg) => arg1 + ", " + arg2;

    #endregion
}
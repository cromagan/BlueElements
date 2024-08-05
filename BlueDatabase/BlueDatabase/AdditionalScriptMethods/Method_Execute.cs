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
using BlueDatabase.Interfaces;
using BlueDatabase.Enums;
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

    public override List<string> Constants => [];

    public override string Description => "Gibt den Befehl an Windows ab.\r\n" +
                                                  "Versucht das Beste daraus zu machen,\r\n";

    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodType => MethodType.IO | MethodType.ManipulatesUser;

    public override bool MustUseReturnValue => false;
    public string NiceTextForUser => "Einen Befehl an Windows übergeben";
    public override string Returns => string.Empty;
    public override string StartSequence => "(";
    public override string Syntax => "Execute(Command, Attribut);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        _ = IO.ExecuteFile(attvar.ValueStringGet(0), attvar.ValueStringGet(1), false, false);

        return DoItFeedback.Null();
    }

    public string TranslateButtonArgs(List<string> args, string filterarg, string rowarg) => args[0] + ", " + args[1];

    #endregion
}
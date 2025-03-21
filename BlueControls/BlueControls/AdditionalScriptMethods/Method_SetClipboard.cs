// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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

using BlueDatabase.Enums;
using BlueDatabase.Interfaces;
using BlueScript.Enums;
using BlueScript.Methods;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;
using static BlueBasics.Generic;

namespace BlueControls.AdditionalScriptMethods;

// ReSharper disable once UnusedMember.Global
internal class Method_SetClipboard : Method, IUseableForButton {

    #region Properties

    public override List<List<string>> Args => [StringVal];
    public List<List<string>> ArgsForButton => [StringVal];
    public List<string> ArgsForButtonDescription => ["Text"];
    public ButtonArgs ClickableWhen => ButtonArgs.Genau_eine_Zeile;
    public override string Command => "setclipboard";
    public override List<string> Constants => [];
    public override string Description => "Speichert den Text im Clipboard.";

    public override bool GetCodeBlockAfter => false;

    public override int LastArgMinCount => -1;

    public override MethodType MethodType => MethodType.ManipulatesUser;

    public override bool MustUseReturnValue => false;
    public string NiceTextForUser => "Text in die Zwischenablage kopieren";

    public override string Returns => string.Empty;

    public override string StartSequence => "(";

    public override string Syntax => "SetClipboard(Text);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var vs = attvar.ValueStringGet(0);
        _ = CopytoClipboard(vs);

        return DoItFeedback.Null();
    }

    public string TranslateButtonArgs(List<string> args, string filterarg, string rowarg) => args[0];

    #endregion
}
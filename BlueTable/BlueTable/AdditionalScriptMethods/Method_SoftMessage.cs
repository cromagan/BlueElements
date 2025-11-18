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

using BlueBasics;
using BlueBasics.Enums;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using BlueTable.Enums;
using BlueTable.Interfaces;
using System.Collections.Generic;

namespace BlueTable.AdditionalScriptMethods;


public class Method_SoftMessage : Method_TableGeneric, IUseableForButton {

    #region Properties

    public override List<List<string>> Args => [StringVal];

    public List<List<string>> ArgsForButton => Args;

    public List<string> ArgsForButtonDescription => ["Text"];
    public ButtonArgs ClickableWhen => ButtonArgs.Genau_eine_Zeile;

    public override string Command => "softmessage";

    public override List<string> Constants => [];
    public override string Description => "Gibt in der Statusleiste einen Nachricht aus, wenn ein Steuerelement vorhanden ist, dass diese anzeigen kann.";

    public override bool GetCodeBlockAfter => false;

    public override int LastArgMinCount => -1;

    public override MethodType MethodLevel => MethodType.Standard;

    public override bool MustUseReturnValue => false;

    public string NiceTextForUser => "Text in der unteren Statusleiste ausgeben";

    public override string Returns => string.Empty;
    public override string StartSequence => "(";
    public override string Syntax => "SoftMessage(Text);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var txt = "<b>Skript:</b> " + attvar.ValueStringGet(0);

        Develop.Message?.Invoke(ErrorType.Info, MyTable(scp), "Skript", ImageCode.Tabelle, txt, 0);

        return DoItFeedback.Null();
    }

    public string TranslateButtonArgs(List<string> args, string filterarg, string rowarg) => args[0];

    #endregion
}
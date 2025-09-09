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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Anthropic.SDK;
using BlueBasics;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueScript.Methods;

// ReSharper disable once UnusedMember.Global
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
internal class Method_Ai : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal];
    public override string Command => "ai";
    public override List<string> Constants => [];
    public override string Description => "Initialisiert die KI von Claude";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodLevel => MethodType.LongTime;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableAi.ShortName_Variable;
    public override string StartSequence => "(";
    public override string Syntax => "Ai(APIKey)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        //https://keyholesoftware.com/2019/02/11/create-your-own-web-bots-in-net-with-cefsharp/

        // Da es keine Möglichkeit gibt, eine Url Variable (außerhalb eines If) zu deklarieren,
        // darf diese Routine nicht fehlschlagen.

        try {
            Generic.CollectGarbage();

            var client = new AnthropicClient(attvar.ValueStringGet(0));

            return new DoItFeedback(new VariableAi(client));
        } catch {
            return new DoItFeedback(new VariableAi(null as AnthropicClient));
        }
    }

    #endregion
}
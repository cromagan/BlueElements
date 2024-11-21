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

using BlueBasics;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Mistral.SDK;

namespace BlueScript.Methods;

// ReSharper disable once UnusedMember.Global
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
internal class Method_LoadMistralAi : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal];
    public override string Command => "loadmistralai";
    public override List<string> Constants => [];
    public override string Description => "Initialisiert die KI von Mistral";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodType => MethodType.Standard;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableMistralAi.ShortName_Variable;
    public override string StartSequence => "(";
    public override string Syntax => "LoadMistralAi(APIKey)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        //https://keyholesoftware.com/2019/02/11/create-your-own-web-bots-in-net-with-cefsharp/

        // Da es keine Möglichkeit gibt, eine Url Variable (außerhalb eines If) zu deklarieren,
        // darf diese Routine nicht fehlschlagen.

        try {
            Generic.CollectGarbage();

            var client = new MistralClient(attvar.ValueStringGet(0));

            return new DoItFeedback(new VariableMistralAi(client));
        } catch {
            return new DoItFeedback(new VariableMistralAi(null as MistralClient));
        }
    }

    #endregion
}
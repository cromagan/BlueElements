// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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

using System.Collections.Generic;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using static BlueBasics.Extensions;

namespace BlueScript.Methods;

// ReSharper disable once UnusedMember.Global
[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
internal class Method_StringAsciiToHTML : Method {

    #region Properties

    public override List<List<string>> Args => new() { StringVal, BoolVal };
    public override string Description => "Ersetzt einen ASCII-String zu einem HTML-String. Beispiel: aus ä wird &auml;  Dabei kann der Zeilenumbuch explicit ausgenommen werden.";
    public override bool EndlessArgs => false;
    public override string EndSequence => ")";
    public override bool GetCodeBlockAfter => false;
    public override MethodType MethodType => MethodType.Standard;
    public override string Returns => VariableString.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "StringAsciiToHTML(String, IgnoreBRbool)";

    #endregion

    #region Methods

    public override List<string> Comand(VariableCollection? currentvariables) => new() { "stringasciitohtml" };

    public override DoItFeedback DoIt(VariableCollection vs, CanDoFeedback infos, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(vs, infos.AttributText, Args, EndlessArgs, infos.Data, scp);
        return !string.IsNullOrEmpty(attvar.ErrorMessage) ? DoItFeedback.AttributFehler(infos.Data, this, attvar)
            : new DoItFeedback(attvar.ValueStringGet(0).CreateHtmlCodes(!attvar.ValueBoolGet(1)));
    }

    #endregion
}
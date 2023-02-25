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
internal class Method_StringHTMLToAscii : Method {

    #region Properties

    public override List<List<string>> Args => new() { StringVal, BoolVal };
    public override string Description => "Ersetzt einen HTML-String zu normalen ASCII-String. Beispiel: Aus &auml; wird ä. Dabei kann der Zeilenumbuch explicit ausgenommen werden.";
    public override bool EndlessArgs => false;
    public override string EndSequence => ")";
    public override bool GetCodeBlockAfter => false;
    public override MethodType MethodType => MethodType.Standard;
    public override string Returns => VariableString.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "StringHTMLToAscii(String, IgnoreBRbool)";

    #endregion

    #region Methods

    public override List<string> Comand(List<Variable>? currentvariables) => new() { "stringhtmltoascii" };

    public override DoItFeedback DoIt(Script s, CanDoFeedback infos) {
        var attvar = SplitAttributeToVars(s, infos.AttributText, Args, EndlessArgs, infos.Data);
        return string.IsNullOrEmpty(attvar.ErrorMessage) ? new DoItFeedback(infos.Data, ((VariableString)attvar.Attributes[0]).ValueString.HtmlSpecialToNormalChar(((VariableBool)attvar.Attributes[1]).ValueBool))
            : DoItFeedback.AttributFehler(infos.Data, this, attvar);
    }

    #endregion
}
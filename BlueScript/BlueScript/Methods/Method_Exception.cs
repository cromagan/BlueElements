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

namespace BlueScript.Methods;

// ReSharper disable once UnusedMember.Global
[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
internal class Method_Exception : Method {

    #region Properties

    public override List<List<string>> Args => new() { StringVal };
    public override string Description => "Unterbricht das Skript mit einer Fehlermeldung.";
    public override bool EndlessArgs => false;
    public override string EndSequence => ");";
    public override bool GetCodeBlockAfter => false;
    public override MethodType MethodType => MethodType.Standard;
    public override string Returns => string.Empty;
    public override string StartSequence => "(";
    public override string Syntax => "Exception(\"Unbehandelter Programmcode!\");";

    #endregion

    #region Methods

    public override List<string> Comand(VariableCollection? currentvariables) => new() { "Exception" };

    public override DoItFeedback DoIt(Script s, CanDoFeedback infos) {
        if (string.IsNullOrEmpty(infos.AttributText)) { return new DoItFeedback(infos.Data, "Die Ausführung wurde absichtlich abgebrochen."); }
        var attvar = SplitAttributeToVars(s, infos.AttributText, Args, EndlessArgs, infos.Data);
        return attvar.Attributes == null || attvar.Attributes.Count != 1 ? new DoItFeedback(infos.Data, "Die Ausführung wurde absichtlich abgebrochen.")
            : new DoItFeedback(infos.Data, "Abbruch durch Exception-Befehl: " + attvar.ValueString(0));
    }

    #endregion
}
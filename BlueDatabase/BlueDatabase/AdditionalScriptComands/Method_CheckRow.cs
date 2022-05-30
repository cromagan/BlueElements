// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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
using BlueScript;
using BlueScript.Structures;

namespace BlueDatabase.AdditionalScriptComands;

public class Method_CheckRow : MethodDatabase {

    #region Properties

    public override List<List<string>> Args => new() { new() { VariableRowItem.ShortName_Variable } };
    public override string Description => "Prüft die angegebene Zeile mit der Startroutine 'script'. Wenn die Zeile Null ist, wird kein Fehler ausgegeben.";

    public override bool EndlessArgs => false;

    public override string EndSequence => ");";

    public override bool GetCodeBlockAfter => false;

    public override string Returns => string.Empty;
    public override string StartSequence => "(";

    public override string Syntax => "CheckRow(Row);";

    #endregion

    #region Methods

    public override List<string> Comand(Script? s) => new() { "checkrow" };

    public override DoItFeedback DoIt(CanDoFeedback infos, Script s) {
        var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(this, attvar); }

        var row = Method_Row.ObjectToRow(attvar.Attributes[0]);
        row?.DoAutomatic("script");
        return DoItFeedback.Null();
    }

    #endregion
}
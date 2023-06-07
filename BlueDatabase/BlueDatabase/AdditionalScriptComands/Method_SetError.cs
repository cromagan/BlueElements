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
using BlueScript;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueDatabase.AdditionalScriptComands;

public class Method_SetError : Method_Database {

    #region Properties

    public override List<List<string>> Args => new() { StringVal, new List<string> { VariableString.ShortName_Variable, VariableListString.ShortName_Variable, VariableFloat.ShortName_Variable, VariableBool.ShortName_Variable } };
    public override string Description => "Bei Zeilenprüfungen wird ein Fehler abgesetzt. Dessen Inhalt bestimmt die Nachricht. Die Spalten, die als fehlerhaft markiert werden sollen, müssen nachträglich als Variablennamen angegeben werden.";

    public override bool EndlessArgs => true;

    public override string EndSequence => ");";

    public override bool GetCodeBlockAfter => false;

    public override MethodType MethodType => MethodType.Database | MethodType.MyDatabaseRow;

    public override string Returns => string.Empty;
    public override string StartSequence => "(";

    public override string Syntax => "SetError(Nachricht, Column1, Colum2, ...);";

    #endregion

    #region Methods

    public override List<string> Comand(VariableCollection? currentvariables) => new() { "seterror" };

    public override DoItFeedback DoIt(Script s, CanDoFeedback infos) {
        var attvar = SplitAttributeToVars(s, infos.AttributText, Args, EndlessArgs, infos.Data);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos.Data, this, attvar); }

        var see = s.Variables.GetSystem("SetErrorEnabled");
        if (see is not VariableBool seet) { return new DoItFeedback(infos.Data, "SetErrorEnabled Variable nicht gefunden"); }
        if (!seet.ValueBool) { return new DoItFeedback(infos.Data, "'SetError' nur bei FehlerCheck Routinen erlaubt."); }

        var r = MyRow(s.Variables);
        if (r == null || r.IsDisposed) { return new DoItFeedback(infos.Data, "Interner Fehler, Zeile nicht gefunden"); }

        r.LastCheckedRowFeedback ??= new List<string>();

        for (var z = 1; z < attvar.Attributes.Count; z++) {
            var column = Column(s.Variables, attvar, z);
            if (column == null || column.IsDisposed) { return new DoItFeedback(infos.Data, "Spalte nicht gefunden: " + attvar.Attributes[z].Name); }
            r.LastCheckedRowFeedback.Add(column.Name.ToUpper() + "|" + attvar.ValueString(0));
        }

        return DoItFeedback.Null();
    }

    #endregion
}
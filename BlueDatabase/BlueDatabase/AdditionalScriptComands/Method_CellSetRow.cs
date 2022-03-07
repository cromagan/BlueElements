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

using System.Collections.Generic;
using BlueScript;
using BlueScript.Structuren;
using Skript.Enums;

namespace BlueDatabase.AdditionalScriptComands {

    public class Method_CellSetRow : MethodDatabase {

        #region Properties

        public override List<enVariableDataType> Args => new() { enVariableDataType.String, enVariableDataType.String, enVariableDataType.Object };

        public override string Description => "Setzt den Wert. Gibt TRUE zurück, wenn der Wert erfolgreich gesetzt wurde.";

        public override bool EndlessArgs => false;

        public override string EndSequence => ")";

        public override bool GetCodeBlockAfter => false;

        public override enVariableDataType Returns => enVariableDataType.Bool;

        public override string StartSequence => "(";

        public override string Syntax => "CellSetRow(Value, Column, Row)";

        #endregion

        #region Methods

        public override List<string> Comand(Script? s) => new() { "cellsetrow" };

        public override DoItFeedback DoIt(CanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(this, attvar); }

            var row = Method_Row.ObjectToRow(attvar.Attributes[2]);
            if (row is null) { return new DoItFeedback("Fehler in der Zeile"); }

            var columnToSet = row.Database.Column.Exists(attvar.Attributes[1].ValueString);
            if (columnToSet == null) { return new DoItFeedback("Spalte nicht gefunden: " + attvar.Attributes[1].ValueString); }

            row.CellSet(columnToSet, attvar.Attributes[0].ValueString);

            return row.CellGetString(columnToSet) == attvar.Attributes[0].ValueString ? DoItFeedback.Wahr() : DoItFeedback.Falsch();
        }

        #endregion
    }
}
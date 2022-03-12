﻿// Authors:
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
using System.Linq;
using BlueDatabase.Interfaces;
using BlueScript;
using BlueScript.Structures;
using BlueScript.Enums;
using BlueScript.Variables;
using static BlueBasics.Extensions;

namespace BlueDatabase.AdditionalScriptComands {

    internal class Method_MatchColumnFormat : MethodDatabase {

        #region Properties

        public override List<VariableDataType> Args => new() { VariableDataType.String_or_List, VariableDataType.Variable_String_Numeral_List_or_Object };
        public override string Description => "Prüft, ob der Inhalt der Variable mit dem Format der angegebenen Spalte übereinstimmt. Leere Inhalte sind dabei TRUE.";
        public override bool EndlessArgs => false;
        public override string EndSequence => ")";
        public override bool GetCodeBlockAfter => false;
        public override VariableDataType Returns => VariableDataType.Bool;
        public override string StartSequence => "(";
        public override string Syntax => "MatchColumnFormat(Value, Column)";

        #endregion

        #region Methods

        public override List<string> Comand(Script? s) => new() { "matchcolumnformat" };

        public override DoItFeedback DoIt(CanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(this, attvar); }

            var column = Column(s, attvar.Attributes[1].Name);
            if (column == null) { return new DoItFeedback("Spalte in Datenbank nicht gefunden"); }

            var tocheck = new List<string>();
            if (attvar.Attributes[0] is VariableListString vl) { tocheck.AddRange(vl.ValueList); }
            if (attvar.Attributes[0] is VariableString vs) { tocheck.Add(vs.ValueString); }

            tocheck = tocheck.SortedDistinctList();

            if (tocheck.Any(thisstring => !thisstring.IsFormat(column))) {
                return DoItFeedback.Falsch();
            }

            return DoItFeedback.Wahr();
        }

        #endregion
    }
}
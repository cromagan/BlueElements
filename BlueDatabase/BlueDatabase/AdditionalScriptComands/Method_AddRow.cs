// Authors:
// Christian Peter
//
// Copyright (c) 2021 Christian Peter
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
using BlueDatabase;
using Skript.Enums;
using System.Collections.Generic;
using static BlueBasics.Extensions;

namespace BlueScript {

    public class Method_AddRow : Method {

        #region Properties

        public override List<enVariableDataType> Args => new() { enVariableDataType.String, enVariableDataType.String, enVariableDataType.Bool };

        public override string Description => "Lädt eine andere Datenbank (Database) und erstellt eine neue Zeile, wenn diese nicht existiert. Bei Always wird immer eine neue Zeile erstellt. Gibt zurück, ob eine neue Zeile erstellt wurde.";

        public override bool EndlessArgs => false;

        public override string EndSequence => ")";

        public override bool GetCodeBlockAfter => false;

        public override enVariableDataType Returns => enVariableDataType.Bool;

        public override string StartSequence => "(";

        public override string Syntax => "AddRow(database, keyvalue, always);";

        #endregion

        #region Methods

        public override List<string> Comand(Script s) => new() { "addrow" };

        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return strDoItFeedback.AttributFehler(this, attvar); }

            var f = s.Variablen.GetSystem("filename");
            if (f == null) { return new strDoItFeedback("System-Variable 'Filename' nicht gefunden."); }

            var newf = f.ValueString.FilePath() + attvar.Attributes[0].ValueString + ".mdb";

            var db = Database.GetByFilename(newf, true);
            if (db == null) { return new strDoItFeedback("Datenbank nicht gefunden: " + newf); }

            if (db.ReadOnly) { return strDoItFeedback.Falsch(); }

            var r = db.Row[attvar.Attributes[1].ValueString];

            if (r != null && !attvar.Attributes[2].ValueBool) { return strDoItFeedback.Falsch(); }

            r = db.Row.Add(attvar.Attributes[1].ValueString);

            return r == null ? strDoItFeedback.Falsch() : strDoItFeedback.Wahr();
        }

        #endregion
    }
}
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

using Skript.Enums;
using System.Collections.Generic;
using BlueScript.Structuren;
using static BlueBasics.Extensions;

namespace BlueScript {

    public class Method_ContentsFilter : MethodDatabase {

        #region Properties

        public override List<enVariableDataType> Args => new() { enVariableDataType.String, enVariableDataType.Object };

        public override string Description => "Lädt eine andere Datenbank (die mit den Filtern definiert wurde)\rund gibt aus der angegebenen Spalte alle Einträge (sortiert und einzigartig) als Liste zurück.\rDabei wird der Filter benutzt.\rEin Filter kann mit dem Befehl 'Filter' erstellt werden.";

        public override bool EndlessArgs => true;

        public override string EndSequence => ")";

        public override bool GetCodeBlockAfter => false;

        public override enVariableDataType Returns => enVariableDataType.List;

        public override string StartSequence => "(";

        public override string Syntax => "ContentsFilter(Colum, Filter, ...)";

        #endregion

        #region Methods

        public override List<string> Comand(Script s) => new() { "contentsfilter" };

        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return strDoItFeedback.AttributFehler(this, attvar); }

            var allFi = Method_Filter.ObjectToFilter(attvar.Attributes, 1);

            if (allFi is null) { return new strDoItFeedback("Fehler im Filter"); }

            var returncolumn = allFi[0].Database.Column.Exists(attvar.Attributes[0].ValueString);
            if (returncolumn == null) { return new strDoItFeedback("Spalte nicht gefunden: " + attvar.Attributes[0].ValueString); }
            var x = returncolumn.Contents(allFi, null);
            return new strDoItFeedback(x.JoinWithCr() + "\r", enVariableDataType.List);
        }

        #endregion
    }
}
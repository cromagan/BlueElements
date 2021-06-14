#region BlueElements - a collection of useful tools, database and controls

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

#endregion BlueElements - a collection of useful tools, database and controls

using Skript.Enums;
using System.Collections.Generic;
using static BlueBasics.Extensions;

namespace BlueScript {

    internal class Method_StringAsciiToHTML : Method {
        public override string Syntax => "Method_StringAsciiToHTML(String, IgnoreBRbool)";
        public override string Description => "Ersetzt einen ASCII-String zu einem HTML-String. Beispiel: aus ä wird &auml;  Dabei kann der Zeilenumbuch explicit ausgenommen werden.";

        public override List<string> Comand(Script s) => new() { "stringasciitohtml" };

        public override string StartSequence => "(";
        public override string EndSequence => ")";
        public override bool GetCodeBlockAfter => false;
        public override enVariableDataType Returns => enVariableDataType.String;
        public override List<enVariableDataType> Args => new() { enVariableDataType.String, enVariableDataType.Bool };
        public override bool EndlessArgs => false;

        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            return !string.IsNullOrEmpty(attvar.ErrorMessage) ? strDoItFeedback.AttributFehler(this, attvar)
                                                              : new strDoItFeedback(attvar.Attributes[0].ValueString.CreateHtmlCodes(!attvar.Attributes[1].ValueBool), enVariableDataType.String);
        }
    }
}
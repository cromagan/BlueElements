#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2020 Christian Peter
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
#endregion

using BlueBasics;
using Skript.Enums;
using System.Collections.Generic;
using System.Linq;
using static BlueBasics.Extensions;

namespace BlueScript {
    internal class Method_Contains : Method {


        //public Method_Contains(Script parent) : base(parent) { }


        public override string Syntax => "Contains(ListVariable/StringVariable, CaseSensitive, Value1, Value2, ...)";

        public override string Description => "Bei Listen: Prüft, ob einer der Werte in der Liste steht. Bei String: Prüft ob eine der Zeichenketten vorkommt.";

        public override List<string> Comand(Script s) { return new() { "contains" }; }
        public override string StartSequence => "(";
        public override string EndSequence => ")";
        public override bool GetCodeBlockAfter => false;
        public override enVariableDataType Returns => enVariableDataType.Bool;
        public override List<enVariableDataType> Args => new() { enVariableDataType.VariableListOrString, enVariableDataType.Bool, enVariableDataType.String };
        public override bool EndlessArgs => true;

        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return strDoItFeedback.AttributFehler(this, attvar); }


            if (attvar.Attributes[0].Type == Skript.Enums.enVariableDataType.List) {
                var x = attvar.Attributes[0].ValueListString;

                for (var z = 2; z < attvar.Attributes.Count; z++) {
                    if (attvar.Attributes[z].Type != Skript.Enums.enVariableDataType.String) { return strDoItFeedback.FalscherDatentyp(); }

                    if (x.Contains(attvar.Attributes[z].ValueString, attvar.Attributes[1].ValueBool)) {
                        return strDoItFeedback.Wahr();
                    }
                }
                return strDoItFeedback.Falsch();
            }

            if (attvar.Attributes[0].Type == Skript.Enums.enVariableDataType.String) {


                for (var z = 2; z < attvar.Attributes.Count; z++) {
                    if (attvar.Attributes[z].Type != Skript.Enums.enVariableDataType.String) { return strDoItFeedback.FalscherDatentyp(); }


                    if (attvar.Attributes[1].ValueBool) {
                        if (attvar.Attributes[0].ValueString.Contains(attvar.Attributes[z].ValueString)) {
                            return strDoItFeedback.Wahr();
                        }
                    } else {
                        if (attvar.Attributes[0].ValueString.ToLower().Contains(attvar.Attributes[z].ValueString.ToLower())) {
                            return strDoItFeedback.Wahr();
                        }
                    }
                }
                return strDoItFeedback.Falsch();
            }

            return strDoItFeedback.FalscherDatentyp();
        }
    }
}

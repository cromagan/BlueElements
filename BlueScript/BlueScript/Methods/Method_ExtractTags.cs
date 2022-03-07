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

using BlueBasics;
using Skript.Enums;
using System.Collections.Generic;
using BlueScript.Structuren;

namespace BlueScript {

    internal class Method_ExtractTags : Method {

        #region Properties

        public override List<enVariableDataType> Args => new() { enVariableDataType.String_or_List, enVariableDataType.String };

        public override string Description => "Extrahiert aus dem gegebenen String oder Liste die Schlagwörter und erstellt neue String-Variablen.\r\n" +
            "Das zweite Attribut dient als Erkennungszeichen, welche das Ende eine Schlagwortes angibt. Zuvor extrahierte Variablen werden wieder entfernt.\r\n" +
            "Beispiel: ExtractTags(\"Farbe: Blau\", \":\"); erstellt eine neue Variable 'extracted_farbe' mit dem Inhalt 'Blau'";

        public override bool EndlessArgs => true;
        public override string EndSequence => ");";
        public override bool GetCodeBlockAfter => false;
        public override enVariableDataType Returns => enVariableDataType.Null;
        public override string StartSequence => "(";
        public override string Syntax => "ExtractTags(String, Delemiter);";

        #endregion

        #region Methods

        public override List<string> Comand(Script s) => new() { "extracttags" };

        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return strDoItFeedback.AttributFehler(this, attvar); }

            const string coment = "Mit dem Befehl 'ExtractTags' erstellt";
            s.Variablen.RemoveWithComent(coment);

            var tags = new List<string>();
            if (attvar.Attributes[0].Type == enVariableDataType.String) {
                tags.Add(attvar.Attributes[0].ValueString);
            } else {
                tags.AddRange(attvar.Attributes[0].ValueListString);
            }

            foreach (var thisw in tags) {
                var x = thisw.SplitBy(attvar.Attributes[1].ValueString);

                if (x.Length == 2) {
                    var vn = x[0].ToLower().ReduceToChars(Constants.AllowedCharsVariableName);
                    var thisv = x[1].Trim();
                    if (!string.IsNullOrEmpty(vn) && !string.IsNullOrEmpty(thisv)) {
                        s.Variablen.Add(new Variable("extracted_" + vn, thisv, enVariableDataType.String, true, false, coment));
                    }
                }
            }

            return strDoItFeedback.Null();
        }

        #endregion
    }
}
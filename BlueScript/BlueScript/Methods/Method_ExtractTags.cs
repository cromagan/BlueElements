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
using BlueBasics;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueScript.Methods;

internal class Method_ExtractTags : Method {

    #region Properties

    public override List<List<string>> Args => new() { new List<string> { VariableString.ShortName_Plain, VariableListString.ShortName_Plain }, new List<string> { VariableString.ShortName_Plain } };

    public override string Description => "Extrahiert aus dem gegebenen String oder Liste die Schlagwörter und erstellt neue String-Variablen.\r\n" +
                                          "Das zweite Attribut dient als Erkennungszeichen, welche das Ende eine Schlagwortes angibt. Zuvor extrahierte Variablen werden wieder entfernt.\r\n" +
                                          "Beispiel: ExtractTags(\"Farbe: Blau\", \":\"); erstellt eine neue Variable 'extracted_farbe' mit dem Inhalt 'Blau'";

    public override bool EndlessArgs => true;
    public override string EndSequence => ");";
    public override bool GetCodeBlockAfter => false;
    public override string Returns => string.Empty;
    public override string StartSequence => "(";
    public override string Syntax => "ExtractTags(String, Delemiter);";

    #endregion

    #region Methods

    public override List<string> Comand(List<Variable> currentvariables) => new() { "extracttags" };

    public override DoItFeedback DoIt(Script s, CanDoFeedback infos) {
        var attvar = SplitAttributeToVars(s, infos.AttributText, Args, EndlessArgs);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(s, infos, this, attvar); }

        const string Comment = "Mit dem Befehl 'ExtractTags' erstellt";
        s.Variables.RemoveWithComment(Comment);

        var tags = new List<string>();
        if (attvar.Attributes[0] is VariableString vs) { tags.Add(vs.ValueString); }
        if (attvar.Attributes[0] is VariableListString vl) { tags.AddRange(vl.ValueList); }

        foreach (var thisw in tags) {
            var x = thisw.SplitBy(((VariableString)attvar.Attributes[1]).ValueString);

            if (x.Length == 2) {
                var vn = x[0].ToLower().ReduceToChars(Constants.AllowedCharsVariableName);
                var thisv = x[1].Trim();
                if (!string.IsNullOrEmpty(vn) && !string.IsNullOrEmpty(thisv)) {
                    s.Variables.Add(new VariableString("extracted_" + vn, thisv, true, false, Comment));
                }
            }
        }

        return DoItFeedback.Null(s, infos);
    }

    #endregion
}
// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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

using BlueBasics;
using BlueBasics.Interfaces;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueScript.Methods;


internal class Method_DeleteFile : Method {

    #region Properties

    public override List<List<string>> Args => [[VariableString.ShortName_Plain, VariableListString.ShortName_Plain]];
    public override string Command => "deletefile";
    public override List<string> Constants => [];
    public override string Description => "Löscht die Datei aus dem Dateisystem. Gibt TRUE zurück, wenn die Datei nicht (mehr) existiert.";

    public override bool GetCodeBlockAfter => false;

    public override int LastArgMinCount => -1;

    public override MethodType MethodLevel => MethodType.LongTime;

    public override bool MustUseReturnValue => false;

    public override string Returns => VariableBool.ShortName_Variable;

    public override string StartSequence => "(";

    public override string Syntax => "DeleteFile(Filename)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var files = new List<string>();

        foreach (var thisAtt in attvar.Attributes) {
            if (thisAtt is VariableString vs1) { files.Add(vs1.ValueString); }
            if (thisAtt is VariableListString vl1) { files.AddRange(vl1.ValueList); }
        }
        files = files.SortedDistinctList();

        if (!scp.ProduktivPhase) { return DoItFeedback.TestModusInaktiv(ld); }

        foreach (var filn in files) {
            if (!filn.IsFormat(FormatHolder.FilepathAndName)) {
                return new DoItFeedback("Dateinamen-Fehler!", true, ld);
            }

            if (IO.FileExists(filn)) {
                try {
                    if (!IO.DeleteFile(filn, false)) { return new DoItFeedback("Fehler beim Löschen: " + filn, true, ld); }
                } catch {
                    return new DoItFeedback("Fehler beim Löschen: " + filn, true, ld);
                }
            }
        }

        return DoItFeedback.Wahr();
    }

    #endregion
}
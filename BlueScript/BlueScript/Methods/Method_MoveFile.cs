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
using BlueScript.Structures;
using BlueScript.Variables;
using static BlueBasics.IO;

namespace BlueScript.Methods;

internal class Method_MoveFile : Method {

    #region Properties

    public override List<List<string>> Args => new() { new List<string> { VariableString.ShortName_Plain }, new List<string> { VariableString.ShortName_Plain } };
    public override string Description => "Verschiebt eine Datei.";
    public override bool EndlessArgs => false;
    public override string EndSequence => ");";
    public override bool GetCodeBlockAfter => false;

    public override string Returns => string.Empty;
    public override string StartSequence => "(";
    public override string Syntax => "MoveFile(SourceCompleteName, DestinatonCompleteName)";

    #endregion

    #region Methods

    public override List<string> Comand(List<Variable> currentvariables) => new() { "movefile" };

    public override DoItFeedback DoIt(Script s, CanDoFeedback infos) {
        var attvar = SplitAttributeToVars(s, infos.AttributText, Args, EndlessArgs);

        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos, this, attvar); }

        var sop = ((VariableString)attvar.Attributes[0]).ValueString;

        var dep = ((VariableString)attvar.Attributes[1]).ValueString;

        //if (!DirectoryExists(sop.FilePath())) { return new DoItFeedback(infos, s, "Verzeichnis existiert nicht"); }
        if (!DirectoryExists(dep.FilePath())) { return new DoItFeedback(infos, "Ziel-Verzeichnis existiert nicht"); }
        if (!FileExists(sop)) { return new DoItFeedback(infos, "Quelldatei existiert nicht."); }

        if (FileExists(dep)) {
            return new DoItFeedback(infos, "Zieldatei existiert bereits.");
        }

        if (!s.ChangeValues) { return new DoItFeedback(infos, "Verschieben im Testmodus deaktiviert."); }

        if (!MoveFile(sop, dep, false)) {
            return new DoItFeedback(infos, "Verschieben fehlgeschlagen.");
        }

        return DoItFeedback.Null(infos);
    }

    #endregion
}
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

using BlueBasics;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;
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

    public override List<string> Comand(Script? s) => new() { "movefile" };

    public override DoItFeedback DoIt(CanDoFeedback infos, Script s, int line) {
        var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs, line);

        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(this, attvar, line); }

        var sop = ((VariableString)attvar.Attributes[0]).ValueString;

        var dep = ((VariableString)attvar.Attributes[1]).ValueString;

        //if (!DirectoryExists(sop.FilePath())) { return new DoItFeedback("Verzeichnis existiert nicht"); }
        if (!DirectoryExists(dep.FilePath())) { return new DoItFeedback("Ziel-Verzeichnis existiert nicht", line); }
        if (!FileExists(sop)) { return new DoItFeedback("Quelldatei existiert nicht.", line); }

        if (FileExists(dep)) {
            return new DoItFeedback("Zieldatei existiert bereits.", line);
        }

        if (!s.ChangeValues) { return new DoItFeedback("Verschieben im Testmodus deaktiviert.", line); }

        if (!MoveFile(sop, dep, false)) {
            return new DoItFeedback("Verschieben fehlgeschlagen.", line);
        }

        return DoItFeedback.Null(line);
    }

    #endregion
}
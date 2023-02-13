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
using System.IO;
using BlueScript.Structures;
using BlueScript.Variables;
using static BlueBasics.IO;

namespace BlueScript.Methods;

internal class Method_GetFiles : Method {

    #region Properties

    public override List<List<string>> Args => new() { new List<string> { VariableString.ShortName_Plain }, new List<string> { VariableString.ShortName_Plain } };
    public override string Description => "Gibt alle Dateien im angegebenen Verzeichnis zurück. Komplett, mit Pfad und Suffix. Pfad muss mit \\ enden. Suffix im Format *.png";
    public override bool EndlessArgs => false;
    public override string EndSequence => ")";
    public override bool GetCodeBlockAfter => false;

    public override string Returns => VariableListString.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "GetFiles(Path, Suffix)";

    #endregion

    #region Methods

    public override List<string> Comand(Script? s) => new() { "getfiles" };

    public override DoItFeedback DoIt(Script s, CanDoFeedback infos) {
        var attvar = SplitAttributeToVars(s, infos.AttributText, Args, EndlessArgs);

        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(s, infos, this, attvar); }

        var pf = ((VariableString)attvar.Attributes[0]).ValueString;

        if (!DirectoryExists(pf)) {
            return new DoItFeedback(s, infos, "Verzeichnis existiert nicht");
        }

        return new DoItFeedback(s, infos, Directory.GetFiles(pf, ((VariableString)attvar.Attributes[1]).ValueString));
    }

    #endregion
}
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

internal class Method_FreeFileName : Method {

    #region Properties

    public override List<List<string>> Args => new() { new List<string> { VariableString.ShortName_Plain }, new List<string> { VariableString.ShortName_Plain } };
    public override string Description => "Gibt einen zufälligen Dateinamen (ohne Pfad / Suffix) zurück, der im anggebenen Verzeichnis nicht existiert.";
    public override bool EndlessArgs => false;
    public override string EndSequence => ")";
    public override bool GetCodeBlockAfter => false;

    public override string Returns => VariableString.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "FreeFileName(Path, Suffix)";

    #endregion

    #region Methods

    public override List<string> Comand(Script? s) => new() { "freefilename" };

    public override DoItFeedback DoIt(CanDoFeedback infos, Script s) {
        var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);

        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(this, attvar); }

        var pf = ((VariableString)attvar.Attributes[0]).ValueString;

        if (!DirectoryExists(pf)) {
            return new DoItFeedback("Verzeichnis existiert nicht");
        }

        var zeichen = Constants.Char_AZ.ToLower() + Constants.Char_Numerals + Constants.Char_AZ.ToUpper();
        // Ja, lower und upper macht keinen sinn, sieht aber verrückter aus

        do {
            var p = string.Empty;
            while (p.Length < 20) {
                var pos = Constants.GlobalRND.Next(zeichen.Length);
                p += zeichen.Substring(pos, 1);
            }

            if (!FileExists(pf + p + ((VariableString)attvar.Attributes[1]).ValueString)) {
                return new DoItFeedback(p, string.Empty);
            }
        } while (true);
    }

    #endregion
}
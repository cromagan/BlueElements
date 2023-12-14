// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BlueBasics;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using static BlueBasics.IO;

namespace BlueScript.Methods;

// ReSharper disable once UnusedMember.Global
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
internal class Method_FreeDirectoryName : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal];
    public override string Command => "freedirectoryname";
    public override string Description => "Gibt einen zufälligen Ordnernamen (ohne Pfad) zurück, der im anggebenen Verzeichnis nicht existiert.";
    public override bool EndlessArgs => false;
    public override bool GetCodeBlockAfter => false;
    public override MethodType MethodType => MethodType.IO | MethodType.NeedLongTime;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableString.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "FreeDirectoryName(Path)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(varCol, infos.AttributText, Args, EndlessArgs, infos.Data, scp);

        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos.Data, this, attvar); }

        var pf = attvar.ValueStringGet(0);

        if (!DirectoryExists(pf)) {
            return new DoItFeedback(infos.Data, "Verzeichnis existiert nicht");
        }

        var zeichen = Constants.Char_AZ.ToLower() + Constants.Char_Numerals + Constants.Char_AZ.ToUpper();
        // Ja, lower und upper macht keinen sinn, sieht aber verrückter aus

        do {
            var p = string.Empty;
            while (p.Length < 20) {
                var pos = Constants.GlobalRnd.Next(zeichen.Length);
                p += zeichen.Substring(pos, 1);
            }

            if (!DirectoryExists(pf + p)) {
                return new DoItFeedback(p);
            }
        } while (true);
    }

    #endregion
}
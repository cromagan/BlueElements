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
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using static BlueBasics.IO;

namespace BlueScript.Methods;

// ReSharper disable once UnusedMember.Global
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
internal class Method_MoveDirectory : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal];
    public override string Command => "movedirectory";
    public override string Description => "Verschiebt einen Ordner.";
    public override bool EndlessArgs => false;
    public override bool GetCodeBlockAfter => false;
    public override MethodType MethodType => MethodType.IO | MethodType.NeedLongTime;
    public override bool MustUseReturnValue => false;
    public override string Returns => string.Empty;
    public override string StartSequence => "(";
    public override string Syntax => "MoveDirectory(SourceCompleteName, DestinatonCompleteName)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(varCol, infos.AttributText, Args, EndlessArgs, infos.Data, scp);

        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos.Data, this, attvar); }

        var sop = attvar.ValueStringGet(0);
        if (!DirectoryExists(sop)) { return new DoItFeedback(infos.Data, "Quell-Verzeichnis existiert nicht."); }
        var dep = attvar.ValueStringGet(1);

        if (DirectoryExists(dep)) { return new DoItFeedback(infos.Data, "Ziel-Verzeichnis existiert bereits."); }

        if (!scp.ChangeValues) { return new DoItFeedback(infos.Data, "Verschieben im Testmodus deaktiviert."); }

        if (!MoveDirectory(sop, dep, false)) {
            return new DoItFeedback(infos.Data, "Verschieben fehlgeschlagen.");
        }

        return DoItFeedback.Null();
    }

    #endregion
}
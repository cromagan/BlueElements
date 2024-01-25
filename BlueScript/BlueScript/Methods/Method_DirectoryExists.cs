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

#nullable enable

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BlueBasics;
using BlueBasics.Interfaces;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueScript.Methods;

// ReSharper disable once UnusedMember.Global
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
internal class Method_DirectoryExists : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal];
    public override string Command => "directoryexists";
    public override string Description => "Prüft, ob ein Verzeichnis existiert";
     public override int LastArgMinCount => -1;
    public override bool GetCodeBlockAfter => false;
    public override MethodType MethodType => MethodType.IO | MethodType.NeedLongTime;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableBool.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "DirectoryExists(FilePath)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(varCol, infos.AttributText, Args, LastArgMinCount, infos.Data, scp);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos.Data, this, attvar); }

        var pf = attvar.ValueStringGet(0);

        if (!pf.IsFormat(FormatHolder.Filepath)) {
            return new DoItFeedback(infos.Data, "Dateipfad ungültig: " + pf);
        }
        //if(pf.IsFormat(FormatHolder.FilepathAndName))

        return new DoItFeedback(IO.DirectoryExists(pf));
    }

    #endregion
}
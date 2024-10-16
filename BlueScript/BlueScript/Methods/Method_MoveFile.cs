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
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using static BlueBasics.IO;

namespace BlueScript.Methods;

// ReSharper disable once UnusedMember.Global
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
internal class Method_MoveFile : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal];
    public override string Command => "movefile";
    public override List<string> Constants => [];
    public override string Description => "Verschiebt eine Datei.";

    public override bool GetCodeBlockAfter => false;

    public override int LastArgMinCount => -1;

    public override MethodType MethodType => MethodType.Standard;

    public override bool MustUseReturnValue => false;

    public override string Returns => string.Empty;

    public override string StartSequence => "(";

    public override string Syntax => "MoveFile(SourceCompleteName, DestinatonCompleteName)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var sop = attvar.ValueStringGet(0);
        var dep = attvar.ValueStringGet(1);

        //if (!DirectoryExists(sop.FilePath())) { return new DoItFeedback(infos.LogData, s, "Verzeichnis existiert nicht"); }
        if (!DirectoryExists(dep.FilePath())) { return new DoItFeedback(ld, "Ziel-Verzeichnis existiert nicht"); }
        if (!FileExists(sop)) { return new DoItFeedback(ld, "Quelldatei existiert nicht."); }

        if (FileExists(dep)) {
            return new DoItFeedback(ld, "Zieldatei existiert bereits.");
        }

        if (!scp.ProduktivPhase) { return new DoItFeedback(ld, "Verschieben im Testmodus deaktiviert."); }

        if (!MoveFile(sop, dep, false)) {
            return new DoItFeedback(ld, "Verschieben fehlgeschlagen.");
        }

        return DoItFeedback.Null();
    }

    #endregion
}
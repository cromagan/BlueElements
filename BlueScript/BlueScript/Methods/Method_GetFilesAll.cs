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

using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;
using System.IO;
using static BlueBasics.IO;

namespace BlueScript.Methods;

// ReSharper disable once UnusedMember.Global
internal class Method_GetFilesAll : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal];
    public override string Command => "getfilesall";
    public override List<string> Constants => [];
    public override string Description => "Gibt alle Dateien im angegebenen Verzeichnis inkl. Unterverzeichnisse zurück. Komplett, mit Pfad und Suffix. Pfad muss mit \\ enden. Suffix im Format *.png";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodType => MethodType.Standard;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableListString.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "GetFilesAll(Path, Suffix)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, CanDoFeedback ld){
        var pf = attvar.ValueStringGet(0);

        if (!DirectoryExists(pf)) {
            return new DoItFeedback("Verzeichnis existiert nicht", true, ld);
        }

        try {
            return new DoItFeedback(Directory.GetFiles(pf, attvar.ValueStringGet(1), SearchOption.AllDirectories), ld.EndPosition());
        } catch {
            return DoItFeedback.InternerFehler(ld);
        }
    }

    #endregion
}
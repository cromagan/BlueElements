// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
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

using BlueBasics.Classes;
using BlueBasics.ClassesStatic;
using BlueBasics.Interfaces;
using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueScript.Methods;


internal sealed class Method_DeleteDirectory : Method {

    #region Properties

    public static List<List<string>> Args => [StringVal];
    public static string Command => "deletedirectory";
    public static List<string> Constants => [];
    public static string Description => "Löscht die Verzeichnis und dessn Inhalt aus dem Dateisystem. Gibt TRUE zurück, wenn das Verzeichnis nicht (mehr) existiert.";



    public static int LastArgMinCount => -1;

    public static MethodType MethodLevel => MethodType.LongTime;



    public static string Returns => VariableBool.ShortName_Variable;

    public static string StartSequence => "(";

    public static string Syntax => "DeleteDirectory(Dir)";

    #endregion

    #region Methods

    public static DoItFeedback DoItSplitted(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var filn = attvar.ValueStringGet(0);

        if (!filn.IsFormat(FormatHolder.Filepath)) { return new DoItFeedback("Dateinamen-Fehler!", true, ld); }

        if (!IO.DirectoryExists(filn)) {
            return DoItFeedback.Wahr();
        }

        if (!scp.ProduktivPhase) { return DoItFeedback.TestModusInaktiv(ld); }

        try {
            return new DoItFeedback(IO.DeleteDir(filn, false));
        } catch {
            return new DoItFeedback("Fehler beim Löschen: " + filn, true, ld);
        }
    }

    #endregion
}
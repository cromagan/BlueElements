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

using BlueBasics.ClassesStatic;
using BlueScript.Classes;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueScript.Methods;

internal sealed class Method_Exists : Method {

    #region Properties

    public static List<List<string>> Args => [[Variable.Any_Variable]];
    public static string Command => "exists";
    public static List<string> Constants => [];
    public static string Description => "Gibt TRUE zurück, wenn die Variable existiert.";

    public static int LastArgMinCount => -1;

    public static bool MustUseReturnValue => true;
    public static string Returns => VariableBool.ShortName_Plain;

    public static string StartSequence => "(";
    public static string Syntax => "Exists(Variable)";

    #endregion

    #region Methods

    public static DoItFeedback DoItVirtual(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(Command, varCol, infos.AttributText, Args, LastArgMinCount, infos.LogData, scp);
        return attvar.Failed ? DoItFeedback.Falsch() : DoItFeedback.Wahr();
    }

    public static DoItFeedback DoItSplitted(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        // Dummy überschreibung.
        // Wird niemals aufgerufen, weil die andere DoIt Rourine überschrieben wurde.

        Develop.DebugPrint_NichtImplementiert(true);
        return DoItFeedback.Falsch();
    }

    #endregion
}
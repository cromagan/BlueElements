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

using BlueBasics;
using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueTable.AdditionalScriptMethods;

public sealed class Method_SetError : Method_TableGeneric {

    #region Fields

    #endregion

    #region Properties

    public static List<List<string>> Args => [StringVal, [Variable.Any_Variable]];
    public static string Command => "seterror";
    public static List<string> Constants => [];

    public static string Description => "Kann nur im Skript \"Formular vorbereiten\" benutzt werden.\r\n" +
                                          "Die hier angegebenen Variablen müssen einer Spalte der Tabelle entsprechen.\r\n" +
                                          "Diese werden dann als 'fehlerhaft' in der Tabellen-Zeile markiert, mit der hier\r\n" +
                                          "angegebenen Nachricht.";


    public static int LastArgMinCount => 1;
    public static MethodType MethodLevel => MethodType.Special;


    public static string Returns => string.Empty;
    public static string StartSequence => "(";

    public static string Syntax => "SetError(Nachricht, Column1, Colum2, ...);";

    #endregion

    #region Methods

    public static DoItFeedback DoItSplitted(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (varCol.GetByKey("ErrorColumns") is not VariableListString vls) { return DoItFeedback.InternerFehler(ld); }
        var l = vls.ValueList;

        for (var z = 1; z < attvar.Attributes.Count; z++) {
            var column = Column(scp, attvar, z);
            if (column is not { IsDisposed: false }) { return new DoItFeedback("Spalte nicht gefunden: " + attvar.Name(z), true, ld); }
            l.Add(column.KeyName.ToUpperInvariant() + "|" + attvar.ValueStringGet(0));
        }

        vls.ValueList = l.SortedDistinctList();

        return DoItFeedback.Null();
    }

    #endregion
}
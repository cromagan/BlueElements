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
using BlueBasics.Interfaces;
using BlueDatabase.Enums;
using BlueScript;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueDatabase.AdditionalScriptMethods;

// ReSharper disable once ClassNeverInstantiated.Global
public class Method_FilterInMyDB : Method_Database {

    #region Properties

    public override List<List<string>> Args => [[Variable.Any_Variable], StringVal, StringVal];
    public override string Command => "filterinmydb";

    public override List<string> Constants => ["IS", "ISNOT", "INSTR", "STARTSWITH", "BETWEEN"];

    public override string Description => "Erstellt einen Filter, der für andere Befehle (z.B. LookupFilter) verwendet werden kann.\r\n" +
                                                "Aktuell werden nur die FilterTypen 'is', 'isnot', 'startswith', 'instr' und 'between'  unterstützt.\r\n" +
                                            "Bei diesem Filter wird die Groß/Kleinschreibung ignoriert.\r\n" +
                                            "Bei Between müssen die Werte so Angegeben werden: 50|100";

    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => 1;
    public override MethodType MethodType => MethodType.Database;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableFilterItem.ShortName_Variable;
    public override string StartSequence => "(";
    public override string Syntax => "FilterInMyDB(Spalte, Filtertyp, Wert)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var column = Column(scp, attvar, 0);
        if (column?.Database is not { IsDisposed: false }) { return new DoItFeedback(ld, "Spalte nicht gefunden: " + attvar.Name(0)); }

        #region Typ ermitteln

        var filtertype = Method_Filter.StringToFilterType(attvar.ValueStringGet(1));

        if (filtertype == FilterType.AlwaysFalse) {
            return new DoItFeedback(ld, "Filtertype unbekannt: " + attvar.ValueStringGet(1));
        }

        #endregion

        var fii = new FilterItem(column, filtertype, attvar.ValueStringGet(2));

        if (!fii.IsOk()) {
            return new DoItFeedback(ld, "Filter konnte nicht erstellt werden: '" + fii.ErrorReason() + "'");
        }

        column.AddSystemInfo("Filter in Script", column.Database, scp.ScriptName);

        return new DoItFeedback(new VariableFilterItem(fii));
    }

    #endregion
}
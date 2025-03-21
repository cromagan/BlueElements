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

namespace BlueDatabase.AdditionalScriptMethods;

// ReSharper disable once UnusedMember.Global
public class Method_ContentsFilter : Method_Database {

    #region Properties

    public override List<List<string>> Args => [StringVal, FilterVar];
    public override string Command => "contentsfilter";
    public override List<string> Constants => [];
    public override string Description => "Lädt eine andere Datenbank (die mit den Filtern definiert wurde)\rund gibt aus der angegebenen Spalte alle Einträge (sortiert und einzigartig) als Liste zurück.\rDabei wird der Filter benutzt.\rEin Filter kann mit dem Befehl 'Filter' erstellt werden.";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => 1;
    public override MethodType MethodType => MethodType.MyDatabaseRow;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableListString.ShortName_Plain;

    public override string StartSequence => "(";

    public override string Syntax => "ContentsFilter(Colum, Filter, ...)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var (allFi, errorreason) = Method_Filter.ObjectToFilter(attvar.Attributes, 1, MyDatabase(scp), scp.ScriptName, true);
        if (allFi == null || !string.IsNullOrEmpty(errorreason)) { return new DoItFeedback(ld, $"Filter-Fehler: {errorreason}"); }

        if (allFi.Database is not { IsDisposed: false } db) {
            allFi.Dispose();
            return new DoItFeedback(ld, "Datenbankfehler!");
        }

        var r = allFi.Rows;
        allFi.Dispose();

        var returncolumn = db.Column[attvar.ReadableText(0)];
        if (returncolumn == null) { return new DoItFeedback(ld, "Spalte nicht gefunden: " + attvar.ReadableText(0)); }

        returncolumn.AddSystemInfo("Value Used in Script", db, scp.ScriptName);

        var x = returncolumn.Contents(r);
        return new DoItFeedback(x);
    }

    #endregion
}
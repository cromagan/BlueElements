// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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
using BlueScript.Methods;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;
using static BlueDatabase.AdditionalScriptMethods.Method_Database;

namespace BlueDatabase.AdditionalScriptMethods;

public class Method_ContentsFilter : Method {

    #region Properties

    public override List<List<string>> Args => new() { StringVal, FilterVar };
    public override string Command => "contentsfilter";
    public override string Description => "Lädt eine andere Datenbank (die mit den Filtern definiert wurde)\rund gibt aus der angegebenen Spalte alle Einträge (sortiert und einzigartig) als Liste zurück.\rDabei wird der Filter benutzt.\rEin Filter kann mit dem Befehl 'Filter' erstellt werden.";
    public override bool EndlessArgs => true;
    public override bool GetCodeBlockAfter => false;
    public override MethodType MethodType => MethodType.MyDatabaseRow | MethodType.NeedLongTime;
    public override string Returns => VariableListString.ShortName_Plain;

    public override string StartSequence => "(";

    public override string Syntax => "ContentsFilter(Colum, Filter, ...)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(varCol, infos.AttributText, Args, EndlessArgs, infos.Data, scp);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos.Data, this, attvar); }

        var allFi = Method_Filter.ObjectToFilter(attvar.Attributes, 1);

        if (allFi is null) { return new DoItFeedback(infos.Data, "Fehler im Filter"); }

        if (allFi.Database is not DatabaseAbstract db) { return new DoItFeedback(infos.Data, "Datenbankfehler!"); }

        var returncolumn = db.Column.Exists(attvar.ReadableText(0));
        if (returncolumn == null) { return new DoItFeedback(infos.Data, "Spalte nicht gefunden: " + attvar.ReadableText(0)); }
        var x = returncolumn.Contents(allFi.Rows);
        return new DoItFeedback(x);
    }

    #endregion
}
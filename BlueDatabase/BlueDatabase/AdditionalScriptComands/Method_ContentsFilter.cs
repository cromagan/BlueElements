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

using System.Collections.Generic;
using BlueScript;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueDatabase.AdditionalScriptComands;

public class Method_ContentsFilter : Method_Database {

    #region Properties

    public override List<List<string>> Args => new() { new List<string> { VariableString.ShortName_Plain }, new List<string> { VariableFilterItem.ShortName_Variable } };
    public override string Description => "Lädt eine andere Datenbank (die mit den Filtern definiert wurde)\rund gibt aus der angegebenen Spalte alle Einträge (sortiert und einzigartig) als Liste zurück.\rDabei wird der Filter benutzt.\rEin Filter kann mit dem Befehl 'Filter' erstellt werden.";

    public override bool EndlessArgs => true;

    public override string EndSequence => ")";

    public override bool GetCodeBlockAfter => false;

    public override string Returns => VariableListString.ShortName_Plain;

    public override string StartSequence => "(";

    public override string Syntax => "ContentsFilter(Colum, Filter, ...)";

    #endregion

    #region Methods

    public override List<string> Comand(Script? s) => new() { "contentsfilter" };

    public override DoItFeedback DoIt(Script s, CanDoFeedback infos) {
        var attvar = SplitAttributeToVars(s, infos.AttributText, Args, EndlessArgs);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(s, infos, this, attvar); }

        var allFi = Method_Filter.ObjectToFilter(attvar.Attributes, 1);

        if (allFi is null) { return new DoItFeedback(s, infos, "Fehler im Filter"); }

        var returncolumn = allFi[0].Database.Column.Exists(attvar.Attributes[0].ReadableText);
        if (returncolumn == null) { return new DoItFeedback(s, infos, "Spalte nicht gefunden: " + attvar.Attributes[0].ReadableText); }
        var x = returncolumn.Contents(allFi, null);
        return new DoItFeedback(s, infos, x);
    }

    #endregion
}
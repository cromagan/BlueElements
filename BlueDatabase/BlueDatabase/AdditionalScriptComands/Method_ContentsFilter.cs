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
using BlueScript.Enums;
using BlueScript.Methods;
using BlueScript.Structures;
using BlueScript.Variables;
using static BlueDatabase.AdditionalScriptComands.Method_Database;

namespace BlueDatabase.AdditionalScriptComands;

public class Method_ContentsFilter : Method {

    #region Properties

    public override List<List<string>> Args => new() { StringVal, FilterVar };
    public override string Description => "Lädt eine andere Datenbank (die mit den Filtern definiert wurde)\rund gibt aus der angegebenen Spalte alle Einträge (sortiert und einzigartig) als Liste zurück.\rDabei wird der Filter benutzt.\rEin Filter kann mit dem Befehl 'Filter' erstellt werden.";
    public override bool EndlessArgs => true;
    public override string EndSequence => ")";
    public override bool GetCodeBlockAfter => false;
    public override MethodType MethodType => MethodType.MyDatabaseRow | MethodType.NeedLongTime;
    public override string Returns => VariableListString.ShortName_Plain;

    public override string StartSequence => "(";

    public override string Syntax => "ContentsFilter(Colum, Filter, ...)";

    #endregion

    #region Methods

    public override List<string> Comand(VariableCollection? currentvariables) => new() { "contentsfilter" };

    public override DoItFeedback DoIt(Script s, CanDoFeedback infos) {
        var attvar = SplitAttributeToVars(s, infos.AttributText, Args, EndlessArgs, infos.Data);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos.Data, this, attvar); }

        var allFi = Method_Filter.ObjectToFilter(attvar.Attributes, 1);

        if (allFi is null) { return new DoItFeedback(infos.Data, "Fehler im Filter"); }

        var returncolumn = allFi[0].Database.Column.Exists(attvar.ReadableText(0));
        if (returncolumn == null) { return new DoItFeedback(infos.Data, "Spalte nicht gefunden: " + attvar.ReadableText(0)); }
        var x = returncolumn.Contents(allFi, null);
        return new DoItFeedback(x);
    }

    #endregion
}
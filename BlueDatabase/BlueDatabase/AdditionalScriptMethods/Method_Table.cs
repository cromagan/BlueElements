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

using BlueBasics;
using BlueBasics.Interfaces;
using BlueDatabase;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueScript.Methods;

// ReSharper disable once UnusedMember.Global
internal class Method_Table : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal];
    public override string Command => "database";
    public override List<string> Constants => [];
    public override string Description => "Versucht die Tabelle in den Speicher zu holen.";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodType => MethodType.Standard;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableTable.ShortName_Variable;
    public override string StartSequence => "(";
    public override string Syntax => "Databse(Filename/Tablename)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var filn = attvar.ValueStringGet(0);

        //if (!filn.IsFormat(FormatHolder.FilepathAndName)) { return new DoItFeedback("Dateinamen-Fehler!", true, ld); }

        //if (!IO.FileExists(filn)) { return new DoItFeedback(false); }

        if (Database.Get(filn, false, null) is { IsDisposed: false } db) {
            return new DoItFeedback(new VariableTable(db));
        }

        return new DoItFeedback($"Tabelle '{filn}' nicht gefunden", true, ld);
    }

    #endregion
}
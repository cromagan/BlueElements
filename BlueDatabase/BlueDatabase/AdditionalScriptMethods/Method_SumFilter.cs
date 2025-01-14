﻿// Authors:
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

using System.Collections.Generic;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueDatabase.AdditionalScriptMethods;

// ReSharper disable once UnusedMember.Global
public class Method_SumFilter : Method_Database {

    #region Properties

    public override List<List<string>> Args => [StringVal, FilterVar];
    public override string Command => "sumfilter";
    public override List<string> Constants => [];
    public override string Description => "Lädt eine andere Datenbank (die mit den Filtern definiert wurde)\rund gibt aus der angegebenen Spalte alle Einträge summiert zurück.\rDabei wird der Filter benutzt.\rEin Filter kann mit dem Befehl 'Filter' erstellt werden.";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => 1;
    public override MethodType MethodType => MethodType.MyDatabaseRow;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableFloat.ShortName_Plain;

    public override string StartSequence => "(";

    public override string Syntax => "SumFilter(Colum, Filter, ...)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        using var allFi = Method_Filter.ObjectToFilter(attvar.Attributes, 1, MyDatabase(scp), scp.ScriptName);

        if (allFi is null) { return new DoItFeedback(ld, "Fehler im Filter"); }

        if (allFi.Database is not { IsDisposed: false } db) { return new DoItFeedback(ld, "Datenbankfehler!"); }

        var returncolumn = db.Column[attvar.ReadableText(0)];
        if (returncolumn == null) { return new DoItFeedback(ld, "Spalte nicht gefunden: " + attvar.ReadableText(0)); }

        returncolumn.AddSystemInfo("Value Used in Script", db, scp.ScriptName);

        var x = returncolumn.Summe(allFi.Rows);

        if(x is not double xd) {
            return new DoItFeedback(ld, "Summe konnte nicht berechnet werden.");
        }

        return new DoItFeedback(xd);
    }

    #endregion
}
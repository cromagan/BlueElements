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

using BlueBasics;
using BlueScript.Enums;
using BlueScript.Methods;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueDatabase.AdditionalScriptMethods;

// ReSharper disable once UnusedType.Global
public class Method_SetError : Method_Database {

    #region Fields

    public static readonly Method Method = new Method_SetError();

    #endregion

    #region Properties

    public override List<List<string>> Args => [StringVal, [Variable.Any_Variable]];
    public override string Command => "seterror";
    public override List<string> Constants => [];

    public override string Description => "Kann nur im Skript \"Formular vorbereiten\" benutzt werden.\r\n" +
                                          "Die hier angegebenen Variablen müssen einer Spalte der Datenbank entsprechen.\r\n" +
                                          "Diese werden dann als 'fehlerhaft' in der Datenbank-Zeile markiert, mit der hier\r\n" +
                                          "angegebenen Nachricht.";

    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => 1;
    public override MethodType MethodType => MethodType.Special;

    public override bool MustUseReturnValue => false;
    public override string Returns => string.Empty;
    public override string StartSequence => "(";

    public override string Syntax => "SetError(Nachricht, Column1, Colum2, ...);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        //if (!SetErrorAllowed(varCol)) { return new DoItFeedback(ld, "'SetError' nur bei FehlerCheck Routinen erlaubt."); }

        var r = MyRow(scp);
        if (r is not { IsDisposed: false }) { return DoItFeedback.InternerFehler(ld); }

        if (varCol.Get("ErrorColumns") is not VariableListString vls) { return DoItFeedback.InternerFehler(ld); }
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
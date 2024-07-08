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

using BlueScript;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueDatabase.AdditionalScriptMethods;

// ReSharper disable once UnusedType.Global
public class Method_SetError : Method_Database {

    #region Properties

    public override List<List<string>> Args => [StringVal, [Variable.Any_Variable]];
    public override string Command => "seterror";

    public override string Description => "Kann nur im Skript \"Formular vorbereiten\" benutzt werden.\r\n" +
                                          "Die hier angegebenen Variablen müssen einer Spalte der Datenbank entsprechen.\r\n" +
                                          "Diese werden dann als 'fehlerhaft' in der Datenbank-Zeile markiert, mit der hier\r\n" +
                                          "angegebenen Nachricht.";

    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => 1;
    public override MethodType MethodType => MethodType.Database | MethodType.MyDatabaseRow;

    public override bool MustUseReturnValue => false;
    public override string Returns => string.Empty;
    public override string StartSequence => "(";

    public override string Syntax => "SetError(Nachricht, Column1, Colum2, ...);";

    #endregion

    #region Methods

   public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (!SetErrorAllowed(varCol)) { return new DoItFeedback(ld, "'SetError' nur bei FehlerCheck Routinen erlaubt."); }

        var r = MyRow(scp);
        if (r == null || r.IsDisposed) { return new DoItFeedback(ld, "Interner Fehler, Zeile nicht gefunden"); }

        r.LastCheckedRowFeedback ??= [];

        for (var z = 1; z < attvar.Attributes.Count; z++) {
            var column = Column(scp, attvar, z);
            if (column == null || column.IsDisposed) { return new DoItFeedback(ld, "Spalte nicht gefunden: " + attvar.Name(z)); }
            r.LastCheckedRowFeedback.Add(column.KeyName.ToUpperInvariant() + "|" + attvar.ValueStringGet(0));
        }

        return DoItFeedback.Null();
    }

    #endregion
}
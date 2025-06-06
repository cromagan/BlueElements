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

namespace BlueScript.Methods;

// ReSharper disable once UnusedType.Global
public class Method_SetFailed : Method {


    #region Properties

    public override List<List<string>> Args => [StringVal];
    public override string Command => "SetFailed";
    public override List<string> Constants => [];

    public override string Description => "Markiert die Zeile als gescheitert, ohne sie als Fehlerhaft zu setzen.\r\n" +
                                            "Dient dazu, temporäre Fehler, wie Netzwerkabbruche zu kompensieren.\r\n" +
                                                "Beim nächsten Programmstart ist deser Fehlerspeicher wieder gelöscht.";

    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodType => MethodType.MyDatabaseRow | MethodType.Database;

    public override bool MustUseReturnValue => false;
    public override string Returns => string.Empty;
    public override string StartSequence => "(";

    public override string Syntax => "SetFailed(Nachricht);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {


        //SetFailed(varCol, attvar.ValueStringGet(0));

        //return DoItFeedback.Null();
        //if (string.IsNullOrEmpty(reason)) { reason = "Allgemeiner Fehler."; }
        //var b = varCol.Get("successful");

        //if (b is VariableBool vb) {

        //    vb.ReadOnly = false;
        //    vb.ValueBool = false;
        //    vb.ReadOnly = true;
        //}

        //var s = varCol.Get("notsuccessfulreason");

        //if (s is VariableString vs) {
        //    vs.ReadOnly = false;
        //    vs.ValueString = reason;
        //    vs.ReadOnly = true;
        //}

        //Develop.MonitorMessage?.Invoke(ErrorType.Info, this, "Allgemein", "Kreuz", $"Nicht Erfolgreich gesetzt: {reason}", 0);

        var r = attvar.ValueStringGet(0);

        if (string.IsNullOrEmpty(r)) { return new DoItFeedback("Keine Fehlermeldung angegeben.", true, ld); }

        return new DoItFeedback(r, false, ld);



    }




    #endregion
}
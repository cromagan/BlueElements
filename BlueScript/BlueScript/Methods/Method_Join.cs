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
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using static BlueBasics.Extensions;

namespace BlueScript.Methods;

// ReSharper disable once UnusedMember.Global
[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
internal class Method_Join : Method {

    #region Properties

    public override List<List<string>> Args => new() { new List<string> { VariableListString.ShortName_Plain }, StringVal };
    public override string Description => "Wandelt eine Liste in einen Text um.\r\nEs verbindet den Text dabei mitteles dem angegebenen Verbindungszeichen.\r\nSind leere Einträge am Ende der Liste, werden die Trennzeichen am Ende nicht abgeschnitten.\r\nDas letzte Trennzeichen wird allerdings immer abgeschnitten!\r\n\r\nBeispiel: Eine Liste mit den Werten 'a' und 'b' wird beim Join mit Semikolon das zurück geben: 'a;b'\r\nAber: Wird eine Liste mit ChangeType in String umgewandelt, wäre ein zusätzliches Trennzeichen am Ende.";
    public override bool EndlessArgs => false;
    public override string EndSequence => ")";
    public override bool GetCodeBlockAfter => false;
    public override MethodType MethodType => MethodType.Standard;
    public override string Returns => VariableString.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "Join(VariableListe, Verbindungszeichen)";

    #endregion

    #region Methods

    public override List<string> Comand(VariableCollection? currentvariables) => new() { "join" };

    public override DoItFeedback DoIt(Script s, CanDoFeedback infos) {
        var attvar = SplitAttributeToVars(s, infos.AttributText, Args, EndlessArgs, infos.Data);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos.Data, this, attvar); }

        //if (string.IsNullOrEmpty(attvar.ValueString(0))) { return DoItFeedback.Null(infos, s, line); }

        var tmp = attvar.ValueListString(0);
        //tmp = tmp.Substring(0, tmp.Length - 1); // Listen mit Einträgen haben zur Erkennung immer noch einen zusätzlichen Zeilenumbruch

        return new DoItFeedback(tmp.JoinWith(attvar.ValueString(1)));
    }

    #endregion
}
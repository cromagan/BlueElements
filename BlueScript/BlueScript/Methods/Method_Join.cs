// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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

using System.Collections.Generic;
using BlueScript.Structures;
using BlueScript.Variables;
using static BlueBasics.Extensions;

namespace BlueScript.Methods {

    internal class Method_Join : Method {

        #region Properties

        public override List<List<string>> Args => new() { new() { VariableListString.ShortName_Variable }, new() { VariableString.ShortName_Plain } };

        public override string Description => "Wandelt eine Liste in einen Text um.\r\nEs verbindet den Text dabei mitteles dem angegebenen Verbindungszeichen.\r\nSind leere Einträge am Ende der Liste, werden die Trennzeichen am Ende nicht abgeschnitten.\r\nDas letzte Trennzeichen wird allerdings immer abgeschnitten!\r\n\r\nBeispiel: Eine Liste mit den Werten 'a' und 'b' wird beim Join mit Semikolon das zurück geben: 'a;b'\r\nAber: Wird eine Liste mit ChangeType in String umgewandelt, wäre ein zusätzliches Trennzeichen am Ende.";
        public override bool EndlessArgs => false;
        public override string EndSequence => ")";
        public override bool GetCodeBlockAfter => false;
        public override string Returns => VariableString.ShortName_Plain;
        public override string StartSequence => "(";
        public override string Syntax => "Join(VariableListe, Verbindungszeichen)";

        #endregion

        #region Methods

        public override List<string> Comand(Script? s) => new() { "join" };

        public override DoItFeedback DoIt(CanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(this, attvar); }

            //if (string.IsNullOrEmpty(((VariableString)attvar.Attributes[0]).ValueString)) { return new DoItFeedback(string.Empty, string.Empty); }

            var tmp = ((VariableListString)attvar.Attributes[0]).ValueList;
            //tmp = tmp.Substring(0, tmp.Length - 1); // Listen mit Einträgen haben zur Erkennung immer noch einen zusätzlichen Zeilenumbruch

            return new DoItFeedback(tmp.JoinWith(((VariableString)attvar.Attributes[1]).ValueString), string.Empty);
        }

        #endregion
    }
}
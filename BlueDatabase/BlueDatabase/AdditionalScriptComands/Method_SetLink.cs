// Authors:
// Christian Peter
//
// Copyright (c) 2021 Christian Peter
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

using BlueDatabase;
using Skript.Enums;
using System.Collections.Generic;

namespace BlueScript {

    public class Method_SetLink : MethodDatabase {

        #region Properties

        public override List<enVariableDataType> Args => new() { enVariableDataType.Variable_List_Or_String, enVariableDataType.String, enVariableDataType.Object };

        public override string Description => "Setzt in der verlinkten Datenbank den Link zur Zelle manuell.\r\n" +
                                              "Ein Filter kann mit dem Befehl 'Filter' erstellt werden.\r\n" +
                                              "Gibt zurück, ob der Link erstellt werden konnte.\r\n" +
                                              "WICHTIG: Der Wert in ColumnInThisDatabase wird verworfen und durch den neuen Inhalt der verlinkten Zelle ersetzt!";

        public override bool EndlessArgs => true;

        public override string EndSequence => ")";

        public override bool GetCodeBlockAfter => false;

        public override enVariableDataType Returns => enVariableDataType.Bool;

        public override string StartSequence => "(";

        //public Method_Lookup(Script parent) : base(parent) { }
        public override string Syntax => "SetLink(ColumnInThisDatabase, ColumnInLinkedDatabase, Filter, ...)";

        #endregion

        #region Methods

        public override List<string> Comand(Script s) => new() { "setlink" };

        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return strDoItFeedback.AttributFehler(this, attvar); }

            var db = MyDatabase(s);
            if (db == null) { return new strDoItFeedback("Interner Fehler, eigene Datenbank nicht gefunden"); }

            #region Filter prüfen: allFi

            var allFi = new List<FilterItem>();

            for (var z = 2; z < attvar.Attributes.Count; z++) {
                if (!attvar.Attributes[z].ObjectType("rowfilter")) { return new strDoItFeedback("Kein Filter übergeben."); }

                var fi = new BlueDatabase.FilterItem(attvar.Attributes[z].ObjectData());

                if (!fi.IsOk()) { return new strDoItFeedback("Filter fehlerhaft"); }

                if (z > 2) {
                    if (fi.Database != allFi[0].Database) { return new strDoItFeedback("Filter über verschiedene Datenbanken wird nicht unterstützt."); }
                }
                allFi.Add(fi);
            }

            if (allFi.Count < 1) { return new strDoItFeedback("Fehler im Filter"); }

            #endregion

            #region Spalte, die geändert werden soll, finden: ChangeColumn

            var ChangeColumn = db.Column.Exists(attvar.Attributes[0].Name);
            if (ChangeColumn.Format != BlueBasics.Enums.enDataFormat.LinkedCell) { return new strDoItFeedback("Spalte hat das falsche Format: " + attvar.Attributes[1].ValueString); }
            if (ChangeColumn.LinkedCell_RowKey >= -1) { return new strDoItFeedback("Spalte wird automatisiert befüllt: " + attvar.Attributes[1].ValueString); }

            #endregion

            if (allFi[0].Database != ChangeColumn.LinkedDatabase()) { return new strDoItFeedback("Filter zeigen auf eine andere Datenbank als die, die in der Spalte als Verlinkung angegeben ist."); }

            #region Spalte mit den Link-Daten finden: Linkvar

            var Linkvar = s.Variablen.GetSystem(attvar.Attributes[0].Name + "_link");
            Linkvar.Readonly = false;

            #endregion

            #region Ziel Spalte in Ziel-Datenbank finden: LinkTaregtColumn -> Schlägt evtl fehl wenn mit Variablen gearbeitet wird.

            var LinkTaregtColumn = allFi[0].Database.Column.Exists(attvar.Attributes[1].ValueString);
            if (LinkTaregtColumn == null) {
                Linkvar.ValueString = string.Empty;
                attvar.Attributes[0].ValueString = string.Empty;
                Linkvar.Readonly = true;
                return strDoItFeedback.Falsch();
            }

            #endregion

            #region Verknüpfung schlägt fehl (Zeile nicht gefunden)

            var r = RowCollection.MatchesTo(allFi);
            if (r == null || r.Count != 1) {
                Linkvar.ValueString = string.Empty;
                attvar.Attributes[0].ValueString = string.Empty;
                Linkvar.Readonly = true;
                return strDoItFeedback.Falsch();
            }

            #endregion

            var tmpv = RowItem.CellToVariable(LinkTaregtColumn, r[0]);
            if (tmpv == null || tmpv.Count != 1) { return new strDoItFeedback("Wert konnte nicht erzeugt werden: " + attvar.Attributes[4].ValueString); }
            tmpv[0].Type = enVariableDataType.List;

            attvar.Attributes[0].ValueString = tmpv[0].ValueString;

            if (attvar.Attributes[0].Type == enVariableDataType.List && !string.IsNullOrEmpty(attvar.Attributes[0].ValueString) && !attvar.Attributes[0].ValueString.EndsWith("\r")) {
                attvar.Attributes[0].ValueString += "\r";
            }

            Linkvar.ValueString = CellCollection.KeyOfCell(LinkTaregtColumn, r[0]);
            Linkvar.Readonly = true;
            return strDoItFeedback.Wahr();
        }

        #endregion
    }
}
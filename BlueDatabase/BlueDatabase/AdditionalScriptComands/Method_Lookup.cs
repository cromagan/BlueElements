#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2020 Christian Peter
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
#endregion

using System.Collections.Generic;
using static BlueBasics.Extensions;
using BlueBasics;
using BlueDatabase;
using Skript.Enums;

namespace BlueScript {
    public class Method_Lookup : BlueScript.Method {


        //public Method_Lookup(Script parent) : base(parent) { }

        public override string Syntax { get => "Lookup(Database, KeyValue, Column, NothingFoundMessage, FoundToMuchMessage);"; }

        public override string Description { get => "Lädt eine andere Datenbank (Database), sucht eine Zeile (KeyValue) und gibt den Inhalt einer Spalte (Column) als Liste zurück. Wird der Wert nicht gefunden, wird NothingFoundMessage zurück gegeben. Ist der Wert mehrfach vorhanden, wird FoundToMuchMessage zurückgegeben."; }
        public override List<string> Comand(Script s) { return new List<string>() { "lookup" }; }
        public override string StartSequence { get => "("; }
        public override string EndSequence { get => ");"; }
        public override bool GetCodeBlockAfter { get => false; }
        public override enVariableDataType Returns { get => enVariableDataType.List; }

        public override List<enVariableDataType> Args { get => new List<enVariableDataType>() { enVariableDataType.String, enVariableDataType.String, enVariableDataType.String, enVariableDataType.String, enVariableDataType.String }; }
        public override bool EndlessArgs { get => false; }

        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args);
            if (attvar == null) { return strDoItFeedback.AttributFehler(); }


            var f = s.Variablen.GetSystem("filename");
            if (f == null) { return new strDoItFeedback("System-Variable 'Filename' nicht gefunden."); }

            foreach ( var thisv in attvar) {
                if (thisv.Type  != Skript.Enums.enVariableDataType.String) { return strDoItFeedback.FalscherDatentyp(); }
            }

            var newf = f.ValueString.FilePath() + attvar[0].ValueString + ".mdb";

            var db2 = BlueBasics.MultiUserFile.clsMultiUserFile.GetByFilename(newf, true);
            BlueDatabase.Database db;

            if (db2 == null) {
                if (!FileOperations.FileExists(newf)) { return new strDoItFeedback("Datenbank nicht gefunden: " + newf); }
                db = new BlueDatabase.Database(newf, false, false);
            }
            else {
                db = (BlueDatabase.Database)db2;
            }

            var c = db.Column.Exists(attvar[2].ValueString);

            if (c == null) {  return new strDoItFeedback("Spalte nicht gefunden: " + attvar[2].ValueString); } 

            var r = RowCollection.MatchesTo(new FilterItem(db.Column[0], BlueDatabase.Enums.enFilterType.Istgleich_GroßKleinEgal, attvar[1].ValueString));

            if (r == null || r.Count == 0) {
                if (attvar.Count > 3) { return new strDoItFeedback(attvar[3].ValueString, string.Empty); }
                return new strDoItFeedback(string.Empty, string.Empty);
            }

            if (r.Count > 1) {
                if (attvar.Count > 4) { return new strDoItFeedback(attvar[4].ValueString, string.Empty); }
                return new strDoItFeedback(string.Empty, string.Empty);
            }


            var v = RowItem.CellToVariable(c, r[0]);
            if (v==null) { return new strDoItFeedback("Wert konnte nicht erzeugt werden: " + attvar[2].ValueString); }

            return new strDoItFeedback(v.ValueForReplace, string.Empty); 
        }
    }
}

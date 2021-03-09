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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BlueBasics.modAllgemein;
using static BlueBasics.Extensions;
using BlueBasics;
using static BlueBasics.modConverter;
using BlueDatabase;

namespace BlueScript {
    public class Method_Lookup : BlueScript.Method {


        public Method_Lookup(Script parent) : base(parent) { }

        public override string Syntax { get => "Lookup(Database, KeyValue, Column, NothingFoundMessage, FoundToMuchMessage);"; }


        public override List<string> Comand { get => new List<string>() { "lookup" }; }
        public override string StartSequence { get => "("; }
        public override string EndSequence { get => ");"; }
        public override bool GetCodeBlockAfter { get => false; }
        public override string Returns { get => string.Empty; }

        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {

            if (string.IsNullOrEmpty(infos.AttributText)) { return new strDoItFeedback("Kein Text angekommen."); }

            var bs = SplitAttribute(infos.AttributText, s, 0);

            if (bs == null || bs.Count < 3) { return new strDoItFeedback("'Lookup' erwartet mindestens drei Attribute: " + infos.AttributText); }



            var f = s.Variablen.GetSystem("filename");
            if (f == null) { return new strDoItFeedback("System-Variable 'Filename' nicht gefunden."); }



            var newf = f.ValueString.FilePath() + bs[0] + ".mdb";

            var db2 = BlueBasics.MultiUserFile.clsMultiUserFile.GetByFilename(newf, true);
            BlueDatabase.Database db;

            if (db2 == null) {
                if (!FileOperations.FileExists(newf)) { return new strDoItFeedback("Datenbank nicht gefunden: " + newf); }
                db = new BlueDatabase.Database(newf, false, false);
            }
            else {
                db = (BlueDatabase.Database)db2;
            }

            var c = db.Column.Exists(bs[2]);

            if (c == null) {  return new strDoItFeedback("Spalte nicht gefunden: " + bs[2]); } 

            var r = RowCollection.MatchesTo(new FilterItem(db.Column[0], BlueDatabase.Enums.enFilterType.Istgleich_GroßKleinEgal, bs[1].Trim("\"")));

            if (r == null || r.Count == 0) {
                if (bs.Count > 3) { return new strDoItFeedback(bs[3], string.Empty); }
                return new strDoItFeedback(string.Empty, string.Empty);
            }

            if (r.Count > 1) {
                if (bs.Count > 4) { return new strDoItFeedback(bs[4], string.Empty); }
                return new strDoItFeedback(string.Empty, string.Empty);
            }


            var v = RowItem.CellToVariable(c, r[0]);

            if (v==null) { return new strDoItFeedback("Wert konnte nicht erzeugt werden: " + bs[2]); }



            return new strDoItFeedback(v.ValueForReplace, string.Empty); 
        }
    }
}

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

#nullable enable

using BlueSQLDatabase;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueScript {

    public class VariableSQLDatabase : Variable {

        #region Fields

        private Database? _db;

        #endregion

        #region Constructors

        public VariableSQLDatabase(string name, Database? value, bool ronly, bool system, string coment) : base(name, ronly, system, coment) => _db = value;

        public VariableSQLDatabase(string name) : this(name, null, true, false, string.Empty) { }

        public VariableSQLDatabase(Database? value) : this(DummyName(), value, true, false, string.Empty) { }

        #endregion

        #region Properties

        public static string ShortName_Variable => "*dbs";
        public override int CheckOrder => 99;

        public Database? Database {
            get => _db;
            set {
                if (Readonly) { return; }
                _db = value;
            }
        }

        public override bool GetFromStringPossible => false;
        public override bool IsNullOrEmpty => _db == null || _db.Disposed;
        public override string ShortName => "dbs";
        public override bool ToStringPossible => false;

        #endregion

        #region Methods

        public override DoItFeedback GetValueFrom(Variable VariableSQL) {
            if (VariableSQL is not VariableSQLDatabase v) { return DoItFeedback.VerschiedeneTypen(this, VariableSQL); }
            if (Readonly) { return DoItFeedback.Schreibgschützt(); }
            Database = v.Database;
            return DoItFeedback.Null();
        }

        protected override bool TryParse(string txt, out Variable? succesVar, Script s) {
            succesVar = null;
            return false;
        }

        #endregion
    }
}
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

using BlueBasics;
using BlueBasics.Enums;
using BlueDatabase;
using BlueScript.Variables;

namespace BlueScript;

public class VariableDatabase : Variable {

    #region Fields

    private string _lastText = string.Empty;
    private Database? _database;

    #endregion

    #region Constructors

    public VariableDatabase(string name, Database? value, bool ronly, string comment) : base(name, ronly, comment) {
        _database = value;
        GetText();
    }

    public VariableDatabase() : this(string.Empty, null, true, string.Empty) { }

    public VariableDatabase(Database? value) : this(DummyName(), value, true, string.Empty) { }

    public VariableDatabase(string name) : this(name, null, true, string.Empty) { }

    #endregion

    #region Properties

    public static string ClassId => "bdb";
    public static string ShortName_Variable => "*bdb";
    public override int CheckOrder => 99;
    public override bool GetFromStringPossible => true;
    public override bool IsNullOrEmpty => _database == null;

    /// <summary>
    /// Gibt den Text "Database: Caption" zurück.
    /// </summary>
    public override string ReadableText => _lastText;

    public Database? Database {
        get => _database;
        private set {
            if (ReadOnly) { return; }
            _database = value;

            GetText();
        }
    }

    public override string SearchValue => ReadableText;

    public override bool ToStringPossible => true;

    public override string ValueForReplace => _database is not { IsDisposed: false } db ? "{DB:?}" : "{DB:" + db.TableName + "}";

    #endregion

    #region Methods

    public override void DisposeContent() => _database = null;

    public override string GetValueFrom(Variable variable) {
        if (variable is not VariableDatabase v) { return VerschiedeneTypen(variable); }
        if (ReadOnly) { return Schreibgschützt(); }
        Database = v.Database;
        return string.Empty;
    }

    protected override void SetValue(object? x) {
        if (x is null) {
            _database = null;
        } else if (x is Database db) {
            _database = db;
        } else {
            Develop.DebugPrint(ErrorType.Error, "Variablenfehler!");
        }
        GetText();
    }

    protected override bool TryParseValue(string txt, out object? result) {
        result = null;

        if (txt.Length > 6 && txt.StartsWith("{DB:") && txt.EndsWith("}")) {
            var t = txt.Substring(4, txt.Length - 5);

            if (t == "?") { return true; }

            if (Database.Get(t, false, null) is not { IsDisposed: false } db) { return false; }

            result = db;
            return true;
        }

        return false;
    }

    private void GetText() => _lastText = _database == null ? "Database: [NULL]" : "Database: " + _database.TableName;

    #endregion
}
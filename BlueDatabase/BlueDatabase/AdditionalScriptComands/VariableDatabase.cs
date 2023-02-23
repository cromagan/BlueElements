﻿// Authors:
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

using BlueDatabase;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueScript;

public class VariableDatabase : Variable {

    #region Fields

    private DatabaseAbstract? _db;

    #endregion

    #region Constructors

    public VariableDatabase(string name, DatabaseAbstract? value, bool ronly, bool system, string comment) : base(name, ronly, system, comment) => _db = value;

    public VariableDatabase(string name) : this(name, null, true, false, string.Empty) { }

    public VariableDatabase(DatabaseAbstract? value) : this(DummyName(), value, true, false, string.Empty) { }

    #endregion

    #region Properties

    public static string ClassId => "dbs";
    public static string ShortName_Variable => "*dbs";
    public override int CheckOrder => 99;

    public DatabaseAbstract? Database {
        get => _db;
        set {
            if (ReadOnly) { return; }
            _db = value;
        }
    }

    public override bool GetFromStringPossible => false;
    public override bool IsNullOrEmpty => _db == null || _db.IsDisposed;
    public override string MyClassId => ClassId;
    public override bool ToStringPossible => false;

    #endregion

    #region Methods

    public override object Clone() {
        var v = new VariableDatabase(Name);
        v.Parse(ToString());
        return v;
    }

    public override DoItFeedback GetValueFrom(Variable variable, LogData ld) {
        if (variable is not VariableDatabase v) { return DoItFeedback.VerschiedeneTypen(ld, this, variable); }
        if (ReadOnly) { return DoItFeedback.Schreibgschützt(ld); }
        Database = v.Database;
        return DoItFeedback.Null();
    }

    protected override Variable? NewWithThisValue(object x, Script s) => null;

    protected override void SetValue(object? x) { }

    protected override object? TryParse(string txt, Script? s) => null;

    #endregion
}
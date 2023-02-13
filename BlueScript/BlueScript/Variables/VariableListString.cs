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
using System.Linq;
using BlueBasics;
using BlueBasics.Enums;
using BlueScript.Methods;
using BlueScript.Structures;

namespace BlueScript.Variables;

public class VariableListString : Variable {

    #region Fields

    private List<string> _list;

    #endregion

    #region Constructors

    public VariableListString(string name, List<string>? value, bool ronly, bool system, string comment) : base(name,
        ronly, system, comment) {
        _list = new List<string>();
        if (value != null) {
            _list.AddRange(value);
        }
    }

    public VariableListString(string name) : this(name, null, true, false, string.Empty) { }

    /// <summary>
    /// Wichtig für: GetEnumerableOfType Variable("NAME");
    /// </summary>
    public VariableListString(List<string>? value) : this(DummyName(), value, true, false, string.Empty) { }

    public VariableListString(IEnumerable<string> value) : this(value.ToList()) { }

    #endregion

    #region Properties

    public static string ClassId => "lst";
    public static string ShortName_Plain => "lst";
    public static string ShortName_Variable => "*lst";
    public override int CheckOrder => 3;
    public override bool GetFromStringPossible => true;
    public override bool IsNullOrEmpty => _list == null || _list.Count == 0;
    public override string MyClassId => ClassId;

    /// <summary>
    /// Die Liste als Text formatiert. z.B. {"A", "B", "C"}
    /// Kritische Zeichen innerhalb eines Eintrags wurden unschädlich gemacht.
    /// </summary>
    public override string ReadableText {
        get {
            if (_list.Count == 0) { return "{ }"; }

            var s = string.Empty;

            foreach (var thiss in _list) {
                s = s + "\"" + thiss.RemoveCriticalVariableChars() + "\", ";
            }

            return "{" + s.TrimEnd(", ") + "}";
        }
    }

    public override bool ToStringPossible => true;

    public override string ValueForReplace => ReadableText;

    public List<string> ValueList {
        get => _list;
        set {
            if (ReadOnly) { return; }

            _list = new List<string>();
            if (value != null) { _list.AddRange(value); }
        }
    }

    #endregion

    #region Methods

    public override object Clone() {
        var v = new VariableListString(Name);
        v.Parse(ToString());
        return v;
    }

    public override DoItFeedback GetValueFrom(Variable variable) {
        if (variable is not VariableListString v) {
            return DoItFeedback.VerschiedeneTypen(null, this, variable);
        }

        if (ReadOnly) {
            return DoItFeedback.Schreibgschützt(null);
        }

        ValueList = v.ValueList;
        return DoItFeedback.Null(null);
    }

    protected override Variable NewWithThisValue(object x, Script s) {
        var v = new VariableListString(string.Empty);
        v.SetValue(x);
        return v;
    }

    protected override void SetValue(object? x) {
        if (x is List<string> val) {
            _list = val;
        } else {
            Develop.DebugPrint(FehlerArt.Fehler, "Variablenfehler!");
        }
    }

    protected override object? TryParse(string txt, Script? s) {
        if (txt.Length > 1 && txt.StartsWith("{") && txt.EndsWith("}")) {
            var t = txt.DeKlammere(false, true, false, true);

            if (string.IsNullOrEmpty(t)) {
                return new List<string>();
            } // Leere Liste

            var l = Method.SplitAttributeToVars(s, t, new List<List<string>> { new() { VariableString.ShortName_Plain } }, true);
            if (!string.IsNullOrEmpty(l.ErrorMessage)) {
                return null;
            }

            return l.Attributes.AllStringValues();
        }

        return null;
    }

    #endregion
}
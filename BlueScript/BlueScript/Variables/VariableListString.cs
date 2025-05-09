﻿// Authors:
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
using BlueScript.Methods;
using BlueScript.Structures;
using System.Collections.Generic;
using System.Linq;
using static BlueBasics.Constants;

namespace BlueScript.Variables;

public class VariableListString : Variable {

    #region Fields

    private List<string> _list;

    #endregion

    #region Constructors

    public VariableListString(string name, IReadOnlyCollection<string>? value, bool ronly, string comment) : base(name,
        ronly, comment) {
        _list = [];
        if (value != null) {
            _list.AddRange(value);
        }
    }

    public VariableListString() : this(string.Empty, null, true, string.Empty) { }

    public VariableListString(string name) : this(name, null, true, string.Empty) { }

    public VariableListString(IReadOnlyCollection<string>? value) : this(DummyName(), value, true, string.Empty) { }

    public VariableListString(IEnumerable<string> value) : this(value.ToList()) { }

    #endregion

    #region Properties

    public static string ClassId => "lst";
    public static string ShortName_Plain => "lst";
    public static string ShortName_Variable => "*lst";
    public override int CheckOrder => 3;
    public override bool GetFromStringPossible => true;
    public override bool IsNullOrEmpty => _list.Count == 0;

    /// <summary>
    /// Die Liste als Text formatiert. z.B. ["A", "B", "C"]
    /// Kritische Zeichen innerhalb eines Eintrags wurden unschädlich gemacht.
    /// </summary>
    public override string ReadableText {
        get {
            if (_list.Count == 0) { return "[ ]"; }

            var s = string.Empty;

            foreach (var thiss in _list) {
                s = s + "\"" + thiss.RemoveCriticalVariableChars() + "\", ";
            }

            return "[" + s.TrimEnd(", ") + "]";
        }
    }

    public override string SearchValue => ReadableText;
    public override bool ToStringPossible => true;
    public override string ValueForReplace => ReadableText;

    public List<string> ValueList {
        get => _list;
        set {
            if (ReadOnly) { return; }

            _list = value;
            //if (value != null) { _list.AddRange(value); }
        }
    }

    #endregion

    #region Methods

    public override void DisposeContent() { }

    public override DoItFeedback GetValueFrom(Variable variable, LogData ld) {
        if (variable is not VariableListString v) {
            return DoItFeedback.VerschiedeneTypen(ld, this, variable);
        }

        if (ReadOnly) {
            return DoItFeedback.Schreibgschützt(ld);
        }

        ValueList = v.ValueList;
        return DoItFeedback.Null();
    }

    protected override void SetValue(object? x) {
        if (x is List<string> val) {
            _list = val;
        } else {
            Develop.DebugPrint(ErrorType.Error, "Variablenfehler!");
        }
    }

    protected override (bool cando, object? result) TryParse(string txt, VariableCollection? vs, ScriptProperties? scp) {
        if (txt.Length > 1 && txt.StartsWith("[") && txt.EndsWith("]")) {
            var t = txt.Trim(KlammernEckig);

            if (string.IsNullOrEmpty(t)) { return (true, new List<string>()); } // Leere Liste

            var l = Method.SplitAttributeToVars(vs, t, [[VariableString.ShortName_Plain]], 1, null, scp);
            if (l.Failed) { return (false, null); }

            return (true, l.Attributes.AllStringValues());
        }

        return (false, null);
    }

    #endregion
}
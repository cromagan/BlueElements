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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BlueBasics;
using BlueBasics.Enums;

namespace BlueScript.Variables;

public class VariableCollection : IEnumerable<Variable>, ICloneable {

    #region Fields

    private List<Variable> _internal = new();

    #endregion

    #region Constructors

    public VariableCollection() { }

    public VariableCollection(List<Variable> v) : this(v, true) { }

    public VariableCollection(List<Variable> v, bool creadonly) {
        foreach (var thisV in v) {
            Add(thisV);
        }
        ReadOnly = creadonly;
    }

    public VariableCollection(List<VariableString>? v) {
        if (v != null) {
            foreach (var thisV in v) {
                Add(thisV);
            }
        }
        ReadOnly = true;
    }

    #endregion

    #region Properties

    public int Count => _internal.Count;

    public bool ReadOnly { get; private set; }

    #endregion

    #region Indexers

    public Variable? this[int index] {
        get {
            if (index < 0 || index >= _internal.Count) { return null; }
            return _internal[index];
        }
    }

    #endregion

    #region Methods

    public bool Add(Variable? variable) {
        if (ReadOnly) { return false; }
        if (variable == null) { return false; }

        if (_internal.Get(variable.Name) != null) { return false; }

        _internal.Add(variable);
        return true;
    }

    public void AddComment(string additionalComment) {
        if (ReadOnly) { return; }

        foreach (var thisvar in _internal) {
            if (!string.IsNullOrEmpty(thisvar.Comment)) {
                thisvar.Comment += "\r";
            }

            thisvar.Comment += additionalComment;
        }
    }

    /// <summary>
    /// True, wenn ALLE Variablen erfolgreich hinzugefügt wurden.
    /// </summary>
    /// <param name="vars"></param>
    /// <returns></returns>
    public bool AddRange(VariableCollection vars) {
        if (ReadOnly) { return false; }

        var f = true;
        foreach (var thisv in vars._internal) {
            if (!Add(thisv)) { f = false; }
        }

        return f;
    }

    public List<string> AllNames() => _internal != null ? _internal.Select(thisvar => thisvar.Name).ToList() : new List<string>();

    public List<string> AllStringableNames() {
        var l = new List<string>();
        if (_internal != null) {
            foreach (var thisvar in _internal) {
                if (thisvar.ToStringPossible) { l.Add(thisvar.Name); }
            }
        }
        return l;
    }

    /// <summary>
    /// Gibt von allen Variablen, die ein String sind, den Inhalt ohne " am Anfang/Ende zurück.
    /// </summary>
    /// <param name="vars"></param>
    /// <returns></returns>
    public List<string> AllStringValues() {
        var l = new List<string>();
        if (_internal != null) {
            foreach (var thisvar in _internal) {
                if (thisvar is VariableString vs) { l.Add(vs.ValueString); }
            }
        }
        return l;
    }

    public object Clone() => new VariableCollection(ToList(), ReadOnly);

    public Variable? Get(string name) {
        if (_internal == null || _internal.Count == 0) { return null; }

        return _internal.FirstOrDefault(thisv =>
            !thisv.SystemVariable && string.Equals(thisv.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Falls es die Variable gibt, wird dessen Wert ausgegeben. Ansonsten string.Empty
    /// </summary>
    /// <param name="vars"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public bool GetBool(string name) {
        var v = _internal.Get(name);

        if (v is not VariableBool vf) {
            Develop.DebugPrint("Falscher Datentyp");
            return false;
        }

        return vf.ValueBool;
    }

    /// <summary>
    /// Falls es die Variable gibt, wird dessen Wert ausgegeben. Ansonsten 0
    /// </summary>
    /// <param name="vars"></param>
    /// <param name="name"></param>
    public double GetDouble(string name) {
        var v = _internal.Get(name);

        if (v is not VariableFloat vf) {
            Develop.DebugPrint("Falscher Datentyp");
            return 0f;
        }

        return vf.ValueNum;
    }

    public IEnumerator<Variable> GetEnumerator() => _internal.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => IEnumerable_GetEnumerator();

    /// <summary>
    /// Falls es die Variable gibt, wird dessen Wert ausgegeben. Ansonsten 0
    /// </summary>
    /// <param name="vars"></param>
    /// <param name="name"></param>
    public int GetInt(string name) {
        var v = _internal.Get(name);

        if (v is not VariableFloat vf) {
            Develop.DebugPrint("Falscher Datentyp");
            return 0;
        }

        return (int)vf.ValueNum;
    }

    /// <summary>
    /// Falls es die Variable gibt, wird dessen Wert ausgegeben. Ansonsten eine leere Liste
    /// </summary>
    /// <param name="vars"></param>
    /// <param name="name"></param>
    public List<string> GetList(string name) {
        var v = _internal.Get(name);
        if (v == null) { return new List<string>(); }

        if (v is not VariableListString vf) {
            Develop.DebugPrint("Falscher Datentyp");
            return new List<string>();
        }

        return vf.ValueList;
    }

    /// <summary>
    /// Falls es die Variable gibt, wird dessen Wert ausgegeben. Ansonsten string.Empty
    /// </summary>
    /// <param name="vars"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public string GetString(string name) {
        var v = _internal.Get(name);

        if (v is not VariableString vf) {
            //Develop.DebugPrint("Falscher Datentyp");
            return string.Empty;
        }

        return vf.ValueString;
    }

    public Variable? GetSystem(string name) => _internal.FirstOrDefault(thisv =>
        thisv.SystemVariable && thisv.Name.ToUpper() == "*" + name.ToUpper());

    public bool Remove(Variable v) {
        if (ReadOnly) { return false; }
        return _internal.Remove(v);
    }

    public void RemoveWithComment(string comment) {
        if (ReadOnly) { return; }
        var z = 0;
        do {
            if (_internal[z].Comment.Contains(comment)) {
                _internal.RemoveAt(z);
            } else {
                z++;
            }
        } while (z < _internal.Count);
    }

    public string ReplaceInText(string originalText) {
        foreach (var thisvar in _internal) {
            originalText = thisvar.ReplaceInText(originalText);
        }
        return originalText;
    }

    /// <summary>
    /// Erstellt bei Bedarf eine neue Variable und setzt den Wert und auch ReadOnly
    /// </summary>
    /// <param name="vars"></param>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public bool Set(string name, string value) {
        if (ReadOnly) { return false; }

        var v = _internal.Get(name);
        if (v == null) {
            v = new VariableString(name, string.Empty, false, false, string.Empty);
            _internal.Add(v);
        }

        if (v is not VariableString vf) {
            Develop.DebugPrint(FehlerArt.Warnung, "Variablentyp falsch");
            return false;
        }

        vf.ReadOnly = false; // sonst werden keine Daten geschrieben
        vf.ValueString = value;
        vf.ReadOnly = true;
        return true;
    }

    //public  void ScriptFinished(this VariableCollection _internal) {
    //    if (_internal == null || _internal.Count == 0) { return; }
    //    foreach (var thisv in _internal) {
    //        thisv.ScriptFinished();
    //    }
    //}
    /// <summary>
    /// Erstellt bei Bedarf eine neue Variable und setzt den Wert und auch ReadOnly
    /// </summary>
    /// <param name="vars"></param>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public bool Set(string name, double value) {
        if (ReadOnly) { return false; }

        var v = _internal.Get(name);
        if (v == null) {
            v = new VariableFloat(name);
            _internal.Add(v);
        }

        if (v is not VariableFloat vf) {
            Develop.DebugPrint(FehlerArt.Warnung, "Variablentyp falsch");
            return false;
        }

        vf.ReadOnly = false; // sonst werden keine Daten geschrieben
        vf.ValueNum = value;
        vf.ReadOnly = true;
        return true;
    }

    /// <summary>
    /// Erstellt bei Bedarf eine neue Variable und setzt den Wert und auch ReadOnly
    /// </summary>
    /// <param name="vars"></param>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public bool Set(string name, List<string> value) {
        if (ReadOnly) { return false; }

        var v = _internal.Get(name);
        if (v == null) {
            v = new VariableListString(name);
            _internal.Add(v);
        }

        if (v is not VariableListString vf) {
            Develop.DebugPrint(FehlerArt.Warnung, "Variablentyp falsch");
            return false;
        }

        vf.ReadOnly = false; // sonst werden keine Daten geschrieben
        vf.ValueList = value;
        vf.ReadOnly = true;
        return true;
    }

    /// <summary>
    /// Erstellt bei Bedarf eine neue Variable und setzt den Wert und auch ReadOnly
    /// </summary>
    /// <param name="vars"></param>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public bool Set(string name, bool value) {
        if (ReadOnly) { return false; }

        var v = _internal.Get(name);
        if (v == null) {
            v = new VariableBool(name);
            _internal.Add(v);
        }

        if (v is not VariableBool vf) {
            Develop.DebugPrint(FehlerArt.Warnung, "Variablentyp falsch");
            return false;
        }

        vf.ReadOnly = false; // sonst werden keine Daten geschrieben
        vf.ValueBool = value;
        vf.ReadOnly = true;
        return true;
    }

    public List<Variable> ToList() {
        var l = new List<Variable>();
        l.AddRange(_internal);
        return l;
    }

    public List<VariableString> ToListVariableString() {
        var l = new List<VariableString>();

        foreach (var thiss in _internal) {
            if (thiss is VariableString vf) {
                l.Add(vf);
            }
        }

        return l;
    }

    internal void Clear() {
        if (ReadOnly) { return; }

        _internal.Clear();
    }

    private IEnumerator IEnumerable_GetEnumerator() => _internal.GetEnumerator();

    #endregion
}
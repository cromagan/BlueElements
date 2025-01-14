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

using System;
using System.Collections;
using System.Collections.Generic;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;

namespace BlueScript.Variables;

public class VariableCollection : IEnumerable<Variable>, IEditable {

    #region Fields

    private readonly List<Variable> _internal = [];

    #endregion

    #region Constructors

    public VariableCollection() { }

    /// <summary>
    /// Erstellt eine neue Liste aus Variablen, die ReadOnly ist
    /// </summary>
    /// <param name="v"></param>
    public VariableCollection(List<Variable> v) : this(v, true) { }

    public VariableCollection(List<Variable> v, bool readOnly) {
        foreach (var thisV in v) {
            Add(thisV);
        }
        ReadOnly = readOnly;
    }

    /// <summary>
    /// Erstellt eine neue Liste aus Variablen, die ReadOnly ist
    /// </summary>
    /// <param name="v"></param>
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

    public string CaptionForEditor => "Variablen";
    public int Count => _internal.Count;

    public Type? Editor { get; set; }
    public bool ReadOnly { get; }

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

    /// <summary>
    /// Erstellt eine neue VariableCollection, die alle Variablen aus ExistingVars enthält.
    /// Überschreibt die Werte in den existingVars mit den Werten aus newValues. Die Werte aus NewValues können dabei einen besonderen Prefix haben.
    /// Beispiel: ExistingVars die Variable Test
    /// NewValues behinhaltet XX_Test.
    /// mit dem Prefix XX_ wird der Wert aus XX_Test nach Test geschrieben.
    /// </summary>
    /// <param name="existingVars"></param>
    /// <param name="newValues"></param>
    /// <param name="newValsPrefix"></param>
    /// <returns></returns>
    public static VariableCollection Combine(VariableCollection existingVars, VariableCollection newValues, string newValsPrefix) {
        var vaa = new List<VariableString>();
        vaa.AddRange(existingVars.ToListVariableString());

        foreach (var thisvar in vaa) {
            var v = newValues.Get(newValsPrefix + thisvar.KeyName);

            if (v is VariableString vs) {
                thisvar.ReadOnly = false; // weil kein OnPropertyChanged vorhanden ist
                thisvar.ValueString = vs.ValueString;
                thisvar.ReadOnly = true; // weil kein OnPropertyChanged vorhanden ist
            }
        }

        return new VariableCollection(vaa);
    }

    //public static VariableCollection Combine(VariableCollection existingVars, Variable thisvar) {
    //    var vaa = new List<VariableString>();
    //    vaa.AddRange(existingVars.ToListVariableString());

    //    foreach (var thisvar in vaa) {
    //        var v = newVars.Get(newVarsPrefix + thisvar.KeyName);

    //        if (v is VariableString vs) {
    //            thisvar.ReadOnly = false; // weil kein OnPropertyChanged vorhanden ist
    //            thisvar.ValueString = vs.ValueString;
    //            thisvar.ReadOnly = true; // weil kein OnPropertyChanged vorhanden ist
    //        } else {
    //            vaa.Add((VariableString)thisvar.Clone());
    //        }
    //    }
    //    return new VariableCollection(vaa);
    //}

    public bool Add(Variable? variable) {
        if (ReadOnly) { return false; }
        if (variable == null) { return false; }

        if (_internal.Get(variable.KeyName) != null) { return false; }

        _internal.Add(variable);
        return true;
    }

    /// <summary>
    /// True, wenn ALLE Variablen erfolgreich hinzugefügt wurden.
    /// </summary>
    /// <param name="vars"></param>
    /// <returns></returns>
    public bool AddRange(IEnumerable<Variable> vars) {
        if (ReadOnly) { return false; }

        var f = true;
        foreach (var thisv in vars) {
            if (!Add(thisv)) { f = false; }
        }

        return f;
    }

    public List<string> AllStringableNames() {
        var l = new List<string>();

        foreach (var thisvar in _internal) {
            if (thisvar.ToStringPossible) { l.Add(thisvar.KeyName); }
        }

        return l;
    }

    /// <summary>
    /// Gibt von allen Variablen, die ein String sind, den Inhalt ohne " am Anfang/Ende zurück.
    /// </summary>
    /// <returns></returns>
    public List<string> AllStringValues() {
        var l = new List<string>();

        foreach (var thisvar in _internal) {
            if (thisvar is VariableString vs) { l.Add(vs.ValueString); }
        }

        return l;
    }

    //public object Clone() => new VariableCollection(ToList(), ReadOnly);

    /// <summary>
    /// Falls es die Variable gibt, wird dessen Wert ausgegeben. Ansonsten string.Empty
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public bool? GetBoolean(string name) {
        var v = _internal.Get(name);

        if (v is not VariableBool vb) {
            //Develop.DebugPrint("Falscher Datentyp");
            return null;
        }

        return vb.ValueBool;
    }

    public IEnumerator<Variable> GetEnumerator() => _internal.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => IEnumerable_GetEnumerator();

    /// <summary>
    /// Falls es die Variable gibt, wird dessen Wert ausgegeben. Ansonsten eine leere Liste
    /// </summary>
    /// <param name="name"></param>
    public List<string> GetList(string name) {
        var v = _internal.Get(name);
        if (v == null) { return []; }

        if (v is not VariableListString vf) {
            Develop.DebugPrint("Falscher Datentyp");
            return [];
        }

        return vf.ValueList;
    }

    /// <summary>
    /// Falls es die Variable gibt, wird dessen Wert ausgegeben. Ansonsten string.Empty
    /// </summary>
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
    /// <param name="name"></param>
    /// <param name="value"></param>
    public bool Set(string name, string value) {
        if (ReadOnly) { return false; }

        var v = _internal.Get(name);
        if (v == null) {
            v = new VariableString(name, string.Empty, false, string.Empty);
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
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
using BlueBasics.Enums;
using BlueBasics;
using BlueScript.Enums;
using BlueScript.Methods;
using BlueScript.Structures;

namespace BlueScript.Variables;

public class VariableUnknown : Variable {

    #region Fields

    private string _value = string.Empty;

    #endregion

    #region Constructors

    public VariableUnknown(string name, bool ronly, bool system, string comment) : base(name, ronly, system, comment) { }

    /// <summary>
    /// Wichtig für: GetEnumerableOfType<Variable>("NAME");
    /// </summary>
    /// <param name="name"></param>
    public VariableUnknown(string name) : this(name, true, false, string.Empty) { }

    public VariableUnknown(string name, string value) : this(name, true, false, string.Empty) { }

    #endregion

    #region Properties

    public static string ClassId => "ukn";

    public static string ShortName_Plain => "ukn";
    public static string ShortName_Variable => "*ukn";
    public override int CheckOrder => 100;
    public override bool GetFromStringPossible => true;
    public override bool IsNullOrEmpty => false;
    public override string MyClassId => ClassId;

    /// <summary>
    /// Gleichgesetzt mit ValueString
    /// </summary>
    public override string ReadableText => "[unknown]";

    public override bool ToStringPossible => false;

    /// <summary>
    /// Der Wert ohne " am Anfang/Ende. Gleichgesetzt mit ReadableText
    /// </summary>
    public string Value {
        get => _value;
        set {
            if (ReadOnly) {
                Develop.DebugPrint(FehlerArt.Warnung, "Read Only Variable!"); // Wichtig für DatabaseVariables
                return;
            }
            _value = value.RestoreCriticalVariableChars(); // Variablen enthalten immer den richtigen Wert und es werden nur beim Ersetzen im Script die kritischen Zeichen entfernt
        }
    }

    public override string ValueForReplace => ReadableText;

    #endregion

    #region Methods

    public override object Clone() {
        var v = new VariableUnknown(Name);
        v.Parse(ToString());
        return v;
    }

    public override DoItFeedback GetValueFrom(Variable variable, LogData ld) {
        if (variable is not VariableUnknown) { return DoItFeedback.VerschiedeneTypen(ld, this, variable); }
        if (ReadOnly) { return DoItFeedback.Schreibgschützt(ld); }
        return DoItFeedback.Null();
    }

    protected override Variable NewWithThisValue(object x) {
        var v = new VariableUnknown(string.Empty);
        v.SetValue(x);
        return v;
    }

    protected override void SetValue(object? x) {
        if (x is string val) {
            _value = val.RestoreCriticalVariableChars();
        } else {
            Develop.DebugPrint(FehlerArt.Fehler, "Variablenfehler!");
        }
    }

    protected override object? TryParse(string txt, VariableCollection? vs, ScriptProperties? scp) => txt;

    #endregion
}
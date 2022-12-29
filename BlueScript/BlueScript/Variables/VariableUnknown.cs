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

using BlueScript.Structures;

namespace BlueScript.Variables;

public class VariableUnknown : Variable {

    #region Constructors

    public VariableUnknown(string name, bool ronly, bool system, string coment) : base(name, ronly, system, coment) { }

    /// <summary>
    /// Wichtig für: GetEnumerableOfType<Variable>("NAME");
    /// </summary>
    /// <param name="name"></param>
    public VariableUnknown(string name) : this(name, true, false, string.Empty) { }

    public VariableUnknown(string name, string value) : this(name, true, false, string.Empty) { }

    #endregion

    #region Properties

    public static string ShortName_Variable => "*ukn";
    public override int CheckOrder => 100;

    public override bool GetFromStringPossible => true;
    public override bool IsNullOrEmpty => false;

    /// <summary>
    /// Gleichgesetzt mit ValueString
    /// </summary>
    public override string ReadableText => "[unknown]";

    public override string ShortName => "ukn";
    public override bool ToStringPossible => false;
    public override string ValueForReplace => ReadableText;

    #endregion

    //public override string ValueForReplace { get => "\"" + _valueString.RemoveCriticalVariableChars() + "\""; }

    #region Methods

    public override DoItFeedback GetValueFrom(Variable variable) {
        if (variable is not VariableUnknown) { return DoItFeedback.VerschiedeneTypen(this, variable); }
        if (Readonly) { return DoItFeedback.Schreibgschützt(); }
        return DoItFeedback.Null();
    }

    protected override bool TryParse(string txt, out Variable? succesVar, Script s) {
        succesVar = new VariableUnknown(txt);
        return true;
    }

    #endregion
}
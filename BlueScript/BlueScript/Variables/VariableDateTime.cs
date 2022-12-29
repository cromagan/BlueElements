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

using BlueBasics;
using BlueScript.Structures;
using System;

namespace BlueScript.Variables;

public class VariableDateTime : Variable {

    #region Fields

    private DateTime _valueDateTime;

    #endregion

    #region Constructors

    public VariableDateTime(string name, DateTime value, bool ronly, bool system, string coment) : base(name, ronly, system, coment) => _valueDateTime = value;

    /// <summary>
    /// Wichtig für: GetEnumerableOfType<Variable>("NAME");
    /// </summary>
    /// <param name="name"></param>
    public VariableDateTime(string name) : this(name, DateTime.Now, true, false, string.Empty) { }

    public VariableDateTime(DateTime value) : this(DummyName(), value, true, false, string.Empty) { }

    #endregion

    #region Properties

    public static string ShortName_Variable => "*dat";
    public override int CheckOrder => 5;
    public override bool GetFromStringPossible => false;
    public override bool IsNullOrEmpty => false;

    public override string ReadableText => _valueDateTime.ToString(Constants.Format_Date4);

    public override string ShortName => "dat";
    public override bool ToStringPossible => false;

    /// <summary>
    /// Der Wert ohne " am Anfang/Ende. Gleichgesetzt mit ReadableText
    /// </summary>
    public DateTime ValueDate {
        get => _valueDateTime;
        set {
            if (Readonly) { return; }
            _valueDateTime = value;
        }
    }

    #endregion

    #region Methods

    public override DoItFeedback GetValueFrom(Variable variable) {
        if (variable is not VariableDateTime v) { return DoItFeedback.VerschiedeneTypen(this, variable); }
        if (Readonly) { return DoItFeedback.Schreibgschützt(); }
        ValueDate = v.ValueDate;
        return DoItFeedback.Null();
    }

    protected override bool TryParse(string txt, out Variable? succesVar, Script s) {
        succesVar = null;
        return false;
    }

    #endregion
}
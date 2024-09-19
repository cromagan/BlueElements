// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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

using BlueControls.ItemCollectionPad;
using BlueScript.Structures;
using static BlueBasics.Interfaces.ParseableExtension;

namespace BlueScript.Variables;

public class VariableItemCollectionPad : Variable {

    #region Fields

    private ItemCollectionPad? _pad;

    #endregion

    #region Constructors

    public VariableItemCollectionPad(string name, ItemCollectionPad? value, bool ronly, string comment) : base(name, ronly, comment) => _pad = value;

    /// <summary>
    /// Wichtig für: GetEnumerableOfType<Variable>("NAME");
    /// </summary>
    /// <param name="name"></param>
    public VariableItemCollectionPad(string name) : this(name, null, true, string.Empty) { }

    public VariableItemCollectionPad(ItemCollectionPad? value) : this(DummyName(), value, true, string.Empty) { }

    #endregion

    #region Properties

    public static string ClassId => "icp";
    public static string ShortName_Variable => "*icp";
    public override int CheckOrder => 99;
    public override bool GetFromStringPossible => false;
    public override bool IsNullOrEmpty => _pad == null;
    public override string MyClassId => ClassId;
    public override string SearchValue => ReadableText;
    public override bool ToStringPossible => false;

    public ItemCollectionPad? ValuePad {
        get => _pad;
        set {
            if (ReadOnly) { return; }
            _pad = value;
        }
    }

    #endregion

    #region Methods

    public override object Clone() {
        var v = new VariableItemCollectionPad(KeyName);
        v.Parse(ToString());
        return v;
    }

    public override void DisposeContent() => _pad?.Dispose();

    public override DoItFeedback GetValueFrom(Variable variable, LogData ld) {
        if (variable is not VariableItemCollectionPad v) { return DoItFeedback.VerschiedeneTypen(ld, this, variable); }
        if (ReadOnly) { return DoItFeedback.Schreibgschützt(ld); }
        ValuePad = v.ValuePad;
        return DoItFeedback.Null();
    }

    protected override Variable? NewWithThisValue(object? x) => null;

    protected override void SetValue(object? x) { }

    protected override (bool cando, object? result) TryParse(string txt, VariableCollection? vs, ScriptProperties? scp) => (false, null);

    #endregion
}
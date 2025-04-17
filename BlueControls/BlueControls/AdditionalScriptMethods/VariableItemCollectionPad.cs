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

using BlueControls.ItemCollectionPad;
using BlueScript.Structures;
using static BlueBasics.Interfaces.ParseableExtension;

namespace BlueScript.Variables;

public class VariableItemCollectionPad : Variable {

    #region Fields

    private ItemCollectionPadItem? _itemCol;

    #endregion

    #region Constructors

    public VariableItemCollectionPad(string name, ItemCollectionPadItem? value, bool ronly, string comment) : base(name, ronly, comment) => _itemCol = value;

    public VariableItemCollectionPad(string name) : this(name, null, true, string.Empty) { }

    public VariableItemCollectionPad() : this(string.Empty, null, true, string.Empty) { }

    public VariableItemCollectionPad(ItemCollectionPadItem? value) : this(DummyName(), value, true, string.Empty) { }

    #endregion

    #region Properties

    // ReSharper disable once UnusedMember.Global
    public static string ClassId => "icp";

    public static string ShortName_Variable => "*icp";
    public override int CheckOrder => 99;
    public override bool GetFromStringPossible => false;
    public override bool IsNullOrEmpty => _itemCol == null;

    public override string SearchValue => ReadableText;
    public override bool ToStringPossible => false;

    public ItemCollectionPadItem? ValueItemCollection {
        get => _itemCol;
        set {
            if (ReadOnly) { return; }
            _itemCol = value;
        }
    }

    #endregion

    #region Methods

    public override object Clone() {
        var v = new VariableItemCollectionPad(KeyName);
        v.Parse(ToString());
        return v;
    }

    public override void DisposeContent() => _itemCol = null;

    public override DoItFeedback GetValueFrom(Variable variable, LogData ld) {
        if (variable is not VariableItemCollectionPad v) { return DoItFeedback.VerschiedeneTypen(ld, this, variable); }
        if (ReadOnly) { return DoItFeedback.Schreibgschützt(ld); }
        ValueItemCollection = v.ValueItemCollection;
        return DoItFeedback.Null();
    }

    protected override Variable? NewWithThisValue(object? x) => null;

    protected override void SetValue(object? x) { }

    protected override (bool cando, object? result) TryParse(string txt, VariableCollection? vs, ScriptProperties? scp) => (false, null);

    #endregion
}
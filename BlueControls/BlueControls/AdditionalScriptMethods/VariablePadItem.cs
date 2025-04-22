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

using BlueControls.ItemCollectionPad.Abstract;
using BlueScript.Structures;
using static BlueBasics.Interfaces.ParseableExtension;

namespace BlueScript.Variables;

public class VariablePadItem : Variable {

    #region Fields

    private AbstractPadItem? _item;

    #endregion

    #region Constructors

    public VariablePadItem(string name, AbstractPadItem? value, bool ronly, string comment) : base(name, ronly, comment) => _item = value;

    public VariablePadItem(string name) : this(name, null, true, string.Empty) { }

    public VariablePadItem() : this(string.Empty, null, true, string.Empty) { }

    public VariablePadItem(AbstractPadItem? value) : this(DummyName(), value, true, string.Empty) { }

    #endregion

    #region Properties

    // ReSharper disable once UnusedMember.Global
    public static string ClassId => "ici";

    public static string ShortName_Variable => "*ici";
    public override int CheckOrder => 99;
    public override bool GetFromStringPossible => false;
    public override bool IsNullOrEmpty => _item == null;

    public override string SearchValue => ReadableText;
    public override bool ToStringPossible => false;

    public AbstractPadItem? ValuePadItem {
        get => _item;
        set {
            if (ReadOnly) { return; }
            _item = value;
        }
    }

    #endregion

    #region Methods

    public override void DisposeContent() => _item = null;

    public override DoItFeedback GetValueFrom(Variable variable, LogData ld) {
        if (variable is not VariablePadItem v) { return DoItFeedback.VerschiedeneTypen(ld, this, variable); }
        if (ReadOnly) { return DoItFeedback.Schreibgschützt(ld); }
        ValuePadItem = v.ValuePadItem;
        return DoItFeedback.Null();
    }

    protected override void SetValue(object? x) { }

    protected override (bool cando, object? result) TryParse(string txt, VariableCollection? vs, ScriptProperties? scp) => (false, null);

    #endregion
}
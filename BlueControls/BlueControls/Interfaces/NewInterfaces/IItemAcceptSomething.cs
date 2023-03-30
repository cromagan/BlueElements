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

using BlueBasics.Interfaces;
using BlueControls.ItemCollection;
using BlueDatabase;
using BlueDatabase.Interfaces;

namespace BlueControls.Interfaces;

/// <summary>
/// Wird verwendet, wenn das Steuerelement etwas empfangen kann
/// </summary>
public interface IItemAcceptSomething : IHasColorId, IHasKeyName, IChangedFeedback, IHasVersion {

    #region Properties

    public int InputColorId { get; set; }
    public string Page { get; }
    public ItemCollectionPad? Parent { get; }

    #endregion
}

public class ItemAcceptSomething {

    #region Fields

    private int _inputColorId = -1;

    #endregion

    #region Methods

    public int InputColorIdGet() => _inputColorId;

    public void InputColorIdSet(int value, IItemAcceptSomething item) {
        if (_inputColorId == value) { return; }

        _inputColorId = value;
        item.OnChanged();
    }

    #endregion
}
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
using BlueControls.Interfaces;
using BlueControls.ItemCollection;
using BlueDatabase;
using BlueDatabase.Interfaces;

namespace BlueControls.Interfaces;

/// <summary>
/// Wird verwendet, wenn das Steuerelement einen Zeilen empfangen kann
/// </summary>
public interface IItemAcceptRow : IDisposableExtended, IItemToControl, IItemAcceptSomething {

    #region Properties

    public string Datenquelle_wählen { get; set; }
    public IItemSendRow? GetRowFrom { get; set; }

    #endregion
}

public static class IItemAcceptRowExtension {

    #region Methods

    public static void ChangeGetRowFrom(this IItemAcceptRow item, ref IItemSendRow? current, IItemSendRow? newvalue) {
        if (current == newvalue) { return; }

        if (current is IItemSendRow old) { old.RemoveChild(item); }
        current = newvalue;
        if (current is IItemSendRow ne) { ne.AddChild(item); }

        item.RaiseVersion();
        item.OnChanged();
    }

    public static DatabaseAbstract? InputDatabase(this FakeControlAcceptRowPadItem item) => item?.GetRowFrom?.OutputDatabase;

    #endregion
}
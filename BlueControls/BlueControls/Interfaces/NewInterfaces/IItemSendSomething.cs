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
using BlueControls.Enums;
using BlueControls.ItemCollection;
using BlueDatabase;
using BlueDatabase.Interfaces;
using System.Collections.ObjectModel;
using System.Windows.Forms.Design;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace BlueControls.Interfaces;

/// <summary>
/// Wird verwendet, einen einzelnen Filter berechnen kann
/// </summary>
public interface IItemSendSomething : IDisposableExtended, IItemToControl, IHasKeyName {

    #region Properties

    public ObservableCollection<string> ChildIds { get; }

    /// <summary>
    /// Laufende Nummer, bestimmt die Einfärbung
    /// </summary>
    public int Id { get; set; }

    public DatabaseAbstract? OutputDatabase { get; set; }

    public ItemCollectionPad? Parent { get; }

    #endregion

    #region Methods

    public void RemoveAllConnections();

    #endregion
}

public static class IItemSendSomethingExtensions {

    #region Methods

    public static void RepairConnections(IItemSendSomething item) {
        item.RemoveAllConnections();
        var item1 = item.Parent[item.KeyName];
        foreach (var thisChild in item.ChildIds) {
            var item2 = item.Parent[thisChild];

            item1.Parent.Connections.Add(new ItemConnection(item1, ConnectionType.Bottom, false, item2, ConnectionType.Top, true, false));
        }
    }

    #endregion
}
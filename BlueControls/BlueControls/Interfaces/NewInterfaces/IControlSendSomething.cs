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

using System.Collections.Generic;
using BlueBasics.Interfaces;
using BlueDatabase;

namespace BlueControls.Interfaces;

public interface IControlSendSomething : IDisposableExtendedWithEvent {

    #region Properties

    public List<IControlAcceptSomething> Childs { get; }

    /// <summary>
    /// Sollte von DoOuputSettings befüllt werden.
    /// </summary>
    public FilterCollection FilterOutput { get; }

    public string Name { get; set; }

    #endregion
}

public static class IControlSendSomethingExtension {

    #region Methods

    public static void DoOutputSettings(this IControlSendSomething dest, IItemSendSomething source) {
        dest.Name = source.DefaultItemToControlName();
        dest.FilterOutput.Database = source.DatabaseOutput;
    }

    #endregion
}
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
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.Controls;
using BlueControls.Forms;
using BlueControls.ItemCollection;
using BlueDatabase;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using static BlueControls.Interfaces.HasVersionExtensions;

namespace BlueControls.Interfaces;

public interface IControlSendFilter : IControlSendSomething {

    #region Properties

    public FilterItem? Filter { get; }

    #endregion
}

public static class IControlSendFilterExtension {

    #region Methods

    public static void DoOutputSettings(this IControlSendFilter dest, ConnectedFormulaView parent, IItemSendFilter source) {
        dest.Name = source.DefaultItemToControlName();
    }

    #endregion
}

public class ControlSendFilter : ControlSendSomething {
}
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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using BlueBasics;
using BlueControls.Interfaces;
using BlueDatabase;

namespace BlueControls.Controls;

internal class FilterChangeControl : GenericControl, IControlAcceptFilter, IControlSendFilter {

    #region Fields

    private readonly List<IControlAcceptFilter> _childs = new();

    private readonly List<IControlSendFilter> _getFilterFrom = new();

    #endregion

    #region Properties

    public FilterItem? Filter { get; }

    public ReadOnlyCollection<IControlSendFilter> GetFilterFrom => new(_getFilterFrom);

    public DatabaseAbstract? InputDatabase { get; set; }
    public DatabaseAbstract? OutputDatabase { get; set; }

    #endregion

    #region Methods

    public void AddGetFilterFrom(IControlSendFilter item) {
        _getFilterFrom.AddIfNotExists(item);
        FilterFromParentsChanged();
        item.ChildAdd(this);
    }

    public void ChildAdd(IControlAcceptFilter c) {
        if (IsDisposed) { return; }
        _childs.AddIfNotExists(c);
        this.DoChilds(_childs);
    }

    public void FilterFromParentsChanged() { }

    #endregion
}
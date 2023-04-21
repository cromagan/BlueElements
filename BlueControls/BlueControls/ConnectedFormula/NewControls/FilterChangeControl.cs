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
using BlueControls.Interfaces;
using BlueDatabase;

namespace BlueControls.Controls;

internal class FilterChangeControl : GenericControl, IControlAcceptFilter, IControlSendFilter {

    #region Fields

    private readonly List<IControlAcceptSomething> _childs = new();

    private readonly List<IControlSendFilter> _parentSender = new();

    #endregion

    #region Properties

    public FilterItem Filter { get; }

    public DatabaseAbstract? OutputDatabase { get; }

    public ReadOnlyCollection<IControlSendFilter> ParentSender => new(_parentSender);

    #endregion

    #region Methods

    public void AddParentSender(IControlSendFilter item) {
        //Invalidate_FilteredRows();
        _parentSender.Add(item);
    }

    public void ChildAdd(IControlAcceptSomething c) {
        if (IsDisposed) { return; }
        _childs.Add(c);
        this.DoChilds(_childs);
    }

    #endregion
}
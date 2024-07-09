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

using BlueDatabase;
using BlueDatabase.Enums;

namespace BlueControls.Controls;

internal class RowEntryControl : GenericControlReciverSender {

    #region Constructors

    public RowEntryControl() : base(false, false) { }

    #endregion

    #region Methods

    protected override void HandleChangesNow() {
        base.HandleChangesNow();
        if (IsDisposed) { return; }
        if (FilterInputChangedHandled && RowsInputChangedHandled) { return; }



        DoInputFilter(FilterOutput.Database, true);
        DoRows();

        if (RowSingleOrNull() is RowItem r) {
            FilterOutput.ChangeTo(new FilterItem(r));
        } else {
            FilterOutput.ChangeTo(new FilterItem(FilterOutput.Database, FilterType.AlwaysFalse, string.Empty));
        }
    }

    public override void Invalidate_RowsInput() {
        base.Invalidate_RowsInput();
        HandleChangesNow();
    }

    #endregion
}
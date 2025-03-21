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

using BlueBasics;
using BlueBasics.EventArgs;
using BlueControls.Designer_Support;
using BlueDatabase;
using System;
using System.ComponentModel;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
public sealed partial class Monitor : GenericControlReciver //UserControl
    {
    #region Fields

    private RowItem? _lastRow;

    private int _n = 99999;

    #endregion

    #region Constructors

    public Monitor() : base(false, false, false) {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();
        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
        SetNotFocusable();
    }

    #endregion

    #region Properties

    public RowItem? LastRow {
        get => _lastRow;

        set {
            if (value?.Database == null || value.IsDisposed) { value = null; }

            if (_lastRow == value) { return; }

            if (_lastRow != null) {
                _lastRow.DropMessage -= _lastRow_DropMessage;
            }
            _n = 99999;
            _lastRow = value;
            capInfo.Text = string.Empty;
            lstDone.ItemClear();

            if (_lastRow != null) {
                capInfo.Text = "Überwache: " + _lastRow.CellFirstString();
                _lastRow.DropMessage += _lastRow_DropMessage;
            }
        }
    }

    #endregion

    #region Methods

    protected override void HandleChangesNow() {
        base.HandleChangesNow();
        if (IsDisposed) { return; }
        if (RowsInputChangedHandled && FilterInputChangedHandled) { return; }

        DoInputFilter(null, false);
        DoRows();

        LastRow = RowSingleOrNull();
    }

    private void _lastRow_DropMessage(object sender, MessageEventArgs e) {
        if (Disposing || IsDisposed) { return; }

        if (InvokeRequired) {
            try {
                _ = Invoke(new Action(() => _lastRow_DropMessage(sender, e)));
                return;
            } catch {
                return;
            }
        }

        _n--;
        if (_n < 0) { _n = 99999; }

        if (!string.IsNullOrWhiteSpace(capInfo.Text)) {
            lstDone.ItemAdd(ItemOf(e, _n.ToStringInt7()));
        }

        lstDone.Refresh();
        //capInfo.Text = e.Message;
    }

    #endregion
}
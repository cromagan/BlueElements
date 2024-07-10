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

using BlueBasics.Enums;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueScript.Variables;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;
using BlueControls.ItemCollectionList;
using static BlueBasics.Extensions;
using static BlueBasics.IO;
using BlueDatabase;
using BlueBasics.EventArgs;
using BlueBasics;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
public sealed partial class Monitor : GenericControlReciver, IBackgroundNone //UserControl
    {


    #region Constructors

    public Monitor() : base(false, false) {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();
        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
        SetNotFocusable();
    }

    #endregion



    #region Properties


    int n = 99999;

    RowItem? _lastRow = null;

    public RowItem? LastRow {
        get => _lastRow;

        set {

            if (value?.Database == null || value.IsDisposed) { value = null; }

            if(_lastRow == value ) { return; }


            if (_lastRow != null) {
                _lastRow.DropMessage -= _lastRow_DropMessage;
            }
            n = 99999;
            _lastRow = value;
            capInfo.Text = string.Empty;
            lstDone.ItemClear();

            if (_lastRow != null) {
                capInfo.Text = "Überwache: " + _lastRow.CellFirstString();
                _lastRow.DropMessage += _lastRow_DropMessage;
            }


        }


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

        n--;
        if(n<0) { n = 99999; }

        if (!string.IsNullOrWhiteSpace(capInfo.Text)) {
            lstDone.ItemAdd(ItemOf(e, n.ToStringInt7()));  
        }

        //capInfo.Text = e.Message;

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









    #endregion
}
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




    RowItem? _lastRow = null;

    public RowItem? LastRow {
        get => _lastRow;

        set {

            if (_lastRow?.Database == null || _lastRow.Database.IsDisposed) { value = null; }

            if(_lastRow == value ) { return; }


            if (_lastRow != null) {
                _lastRow.DropMessage -= _lastRow_DropMessage;
            }
            _lastRow = value;
            capInfo.Text = string.Empty;
            lstDone.ItemClear();

            if (_lastRow != null) {
                _lastRow.DropMessage += _lastRow_DropMessage;
            }


        }


    }

    private void _lastRow_DropMessage(object sender, MessageEventArgs e) {
     
        if(!string.IsNullOrWhiteSpace(capInfo.Text)) {
            lstDone.ItemAdd(ItemOf(e));
        }

        capInfo.Text = e.Message;

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
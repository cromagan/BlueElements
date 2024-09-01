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

using BlueBasics;
using BlueControls.BlueDatabaseDialogs;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using BlueDatabase;
using BlueDatabase.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using static BlueControls.ConnectedFormula.ConnectedFormula;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
public partial class ConnectedCreativePad : GenericControlReciver {

    #region Fields

    private RowItem? _lastRow;

    #endregion

    #region Constructors

    public ConnectedCreativePad(ItemCollectionPad.ItemCollectionPad? itemCollectionPad) : base(false, false) {
        // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
        InitializeComponent();
        // Initialisierungen nach dem Aufruf InitializeComponent() hinzufügen
        SetNotFocusable();
        pad.Item = itemCollectionPad;
        pad.Unselect();
        pad.ShowInPrintMode = true;
        pad.EditAllowed = false;
        MouseHighlight = false;
    }

    public ConnectedCreativePad() : this(null) { }

    #endregion

    #region Properties

    public RowItem? LastRow {
        get => _lastRow;

        set {
            if (value?.Database == null || value.IsDisposed) { value = null; }

            if (_lastRow == value) { return; }
            if (pad.Item == null) { return; }

            pad.Item.ResetVariables();

            _lastRow = value;

            pad.Item.ReplaceVariables(_lastRow);

            pad.ZoomFit();
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Verwendete Ressourcen bereinigen.
    /// </summary>
    /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
    protected override void Dispose(bool disposing) {
        if (disposing) {
            pad.Item?.Clear();
            pad.Dispose();
            components?.Dispose();
        }
        base.Dispose(disposing);
    }

    protected override void HandleChangesNow() {
        base.HandleChangesNow();
        if (IsDisposed) { return; }
        if (RowsInputChangedHandled && FilterInputChangedHandled) { return; }

        DoInputFilter(null, false);
        DoRows();

        LastRow = RowSingleOrNull();

        //base.HandleChangesNow();

        //if (IsDisposed) { return; }
        //if (RowsInputChangedHandled && FilterInputChangedHandled) { return; }

        //DoInputFilter(FilterOutput.Database, false);
        //DoRows();

        //if (RowSingleOrNull() is { IsDisposed: false } r) {
        //    FilterOutput.ChangeTo(new FilterItem(r));

        //    pad.Visible = r.Database is { IsDisposed: false } db && !string.IsNullOrEmpty(db.ScriptNeedFix);

        //    if (pad.Visible) { pad.BringToFront(); }
        //} else {
        //    FilterOutput.ChangeTo(new FilterItem(FilterOutput.Database, FilterType.AlwaysFalse, string.Empty));
        //}
    }

    #endregion

    // // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
    // ~ConnectedFormulaView()
    // {
    //     // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
    //     Dispose(disposing: false);
    // }
}
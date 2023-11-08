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

using System;
using System.ComponentModel;
using System.Drawing;
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Designer_Support;
using BlueControls.EventArgs;
using BlueControls.Interfaces;
using BlueDatabase;
using BlueDatabase.Enums;
using BlueDatabase.EventArgs;
using BlueDatabase.Interfaces;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
public partial class FlexiControlVariable : FlexiControl, IDisabledReason, IHasDatabase, IControlAcceptRow {

    #region Fields

    private IControlSendRow? _getRowFrom;
    private string _rowKey = string.Empty;
    private RowItem? _tmpRow;

    #endregion

    #region Constructors

    public FlexiControlVariable() : this(null, ÜberschriftAnordnung.Über_dem_Feld, EditTypeFormula.None) { }

    public FlexiControlVariable(DatabaseAbstract? database, ÜberschriftAnordnung captionPosition, EditTypeFormula editType) : base() {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();
        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
        Size = new Size(300, 300);
        ShowInfoWhenDisabled = true;
        CaptionPosition = captionPosition;
        EditType = editType;
        SetData(database, string.Empty);
        CheckEnabledState();
    }

    #endregion

    #region Events

    public event EventHandler<ContextMenuInitEventArgs>? ContextMenuInit;

    public event EventHandler<ContextMenuItemClickedEventArgs>? ContextMenuItemClicked;

    #endregion

    #region Properties

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public DatabaseAbstract? Database { get; private set; }

    public IControlSendRow? GetRowFrom {
        get => _getRowFrom;
        set {
            if (_getRowFrom == value) { return; }
            if (_getRowFrom != null) {
                Develop.DebugPrint(FehlerArt.Fehler, "Änderung nicht erlaubt");
            }

            _getRowFrom = value;
            if (_getRowFrom != null) { _getRowFrom.ChildAdd(this); }
        }
    }

    public RowItem? LastInputRow { get; }

    #endregion

    #region Methods

    public void OnContextMenuInit(ContextMenuInitEventArgs e) => ContextMenuInit?.Invoke(this, e);

    public void OnContextMenuItemClicked(ContextMenuItemClickedEventArgs e) => ContextMenuItemClicked?.Invoke(this, e);

    public void SetData(DatabaseAbstract? database, string rowkey) {
        if (rowkey == _rowKey && database == Database) { return; }

        if (Database != null) {
            Database.Row.RowRemoving -= Row_RowRemoving;
            Database.Row.RowChecked -= Database_RowChecked;
            Database.Loaded -= _Database_Loaded;
            Database.Disposing -= _Database_Disposing;
        }

        Database = database;
        _rowKey = rowkey ?? string.Empty;
        _tmpRow = null;

        if (Database != null) {
            Database.Row.RowRemoving += Row_RowRemoving;
            Database.Row.RowChecked += Database_RowChecked;
            Database.Loaded += _Database_Loaded;
            Database.Disposing += _Database_Disposing;
        }
        SetValueFromCell();
        CheckEnabledState();

        var row = GetTmpVariables();
        row?.CheckRowDataIfNeeded();

        if (row?.LastCheckedEventArgs is RowCheckedEventArgs e) {
            Database_RowChecked(this, e);
        }
    }

    internal void CheckEnabledState() => DisabledReason = "Nur Anzeige";

    private void _Database_Disposing(object sender, System.EventArgs e) => SetData(null, null);

    private void _Database_Loaded(object sender, System.EventArgs e) {
        if (Disposing || IsDisposed) { return; }

        if (InvokeRequired) {
            try {
                _ = Invoke(new Action(() => _Database_Loaded(sender, e)));
                return;
            } catch {
                // Kann dank Multitasking disposed sein
                Develop.CheckStackForOverflow();
                _Database_Loaded(sender, e); // am Anfang der Routine wird auf disposed geprüft
                return;
            }
        }

        SetValueFromCell();
    }

    private void Database_RowChecked(object sender, RowCheckedEventArgs e) {
        _ = GetTmpVariables();

        Develop.DebugPrint_NichtImplementiert();

        //if (e.Row != row) { return; }
        //if (e.ColumnsWithErrors == null) {
        //    InfoText = string.Empty;
        //    return;
        //}

        //var newT = string.Empty;
        //foreach (var thisString in e.ColumnsWithErrors) {
        //    var x = thisString.SplitAndCutBy("|");
        //    if (column != null && string.Equals(x[0], column.KeyName, StringComparison.OrdinalIgnoreCase)) {
        //        if (!string.IsNullOrEmpty(InfoText)) { InfoText += "<br><hr><br>"; }
        //        newT += x[1];
        //    }
        //}
        //InfoText = newT;
    }

    private RowItem? GetTmpVariables() {
        try {
            if (_tmpRow != null) { return _tmpRow; }

            if (Database != null && !Database.IsDisposed) {
                _tmpRow = Database.Row.SearchByKey(_rowKey);
            } else {
                _tmpRow = null;
            }

            return _tmpRow;
        } catch {
            // Multitasking sei dank kann _database trotzem null sein...
            Develop.CheckStackForOverflow();
            return GetTmpVariables();
        }
    }

    private void Row_RowRemoving(object sender, RowReasonEventArgs e) {
        if (e.Row.KeyName == _rowKey) {
            _rowKey = string.Empty;
            _tmpRow = null;
        }
    }

    private void SetValueFromCell() {
        if (IsDisposed) { return; }

        _ = GetTmpVariables();
        Develop.DebugPrint_NichtImplementiert();

        //if (column == null || row == null) {
        //    ValueSet(string.Empty, true, true);
        //    InfoText = string.Empty;
        //    return;
        //}

        //switch (column.Format) {
        //    case DataFormat.Verknüpfung_zu_anderer_Datenbank:
        //        _ = GetRealColumn(column, row);
        //        ValueSet(row.CellGetString(column), true, true);
        //        break;

        //    default:
        //        ValueSet(row.CellGetString(column), true, true);
        //        break;
        //}
    }

    #endregion
}
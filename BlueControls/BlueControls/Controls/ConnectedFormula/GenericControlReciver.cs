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
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionPad;
using BlueControls.ItemCollectionPad.Abstract;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using BlueDatabase;
using BlueDatabase.EventArgs;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace BlueControls.Controls;

public class GenericControlReciver : GenericControl, IBackgroundNone {

    #region Fields

    public readonly List<GenericControlReciverSender> Parents = [];
    private Database? _databaseInput;

    private FilterCollection? _filterInput;

    private bool _rowsInputChangedHandling;

    #endregion

    #region Constructors

    public GenericControlReciver() : base(false, false, false) { }

    public GenericControlReciver(bool doubleBuffer, bool useBackgroundBitmap, bool mouseHighlight) : base(doubleBuffer, useBackgroundBitmap, mouseHighlight) { }

    #endregion

    #region Properties

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Database? DatabaseInput {
        get => _databaseInput;

        private set {
            // Wichtig! Darf nur von HandelChangesNow befültg werden!
            // Grund: Es wird hier nichts invalidiert!

            if (value is { IsDisposed: true }) { value = null; }

            if (value == _databaseInput) { return; }

            if (_databaseInput != null) {
                _databaseInput.Cell.CellValueChanged -= DatabaseInput_CellValueChanged;
                _databaseInput.Column.ColumnPropertyChanged -= DatabaseInput_ColumnPropertyChanged;
                _databaseInput.Row.RowChecked -= DatabaseInput_RowChecked;
                _databaseInput.Loaded -= DatabaseInput_Loaded;
                _databaseInput.Disposed -= DatabaseInput_Disposed;
            }

            _databaseInput = value;

            if (_databaseInput is { IsDisposed: false }) {
                _databaseInput.Cell.CellValueChanged += DatabaseInput_CellValueChanged;
                _databaseInput.Column.ColumnPropertyChanged += DatabaseInput_ColumnPropertyChanged;
                _databaseInput.Row.RowChecked += DatabaseInput_RowChecked;
                _databaseInput.Loaded += DatabaseInput_Loaded;
                _databaseInput.Disposed += DatabaseInput_Disposed;
            }
        }
    }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public AbstractPadItem? GeneratedFrom { get; set; }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public virtual string Mode { get; set; } = string.Empty;

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool RowsInputManualSeted { get; private set; }

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    protected FilterCollection? FilterInput {
        get => _filterInput;
        private set {
            if (_filterInput == value) { return; }
            UnRegisterFilterInputAndDispose();
            _filterInput = value;
            RegisterEvents();
        }
    }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    protected bool FilterInputChangedHandled { get; private set; }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    protected List<RowItem>? RowsInput { get; private set; }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    protected bool RowsInputChangedHandled { get; private set; }

    private bool _filterInputChangedHandling { get; set; }

    #endregion

    #region Methods

    /// <summary>
    /// Nachdem das Control erzeugt wurde, werden hiermit die Einstellungen vom ReciverControlPadItem übernommen.
    /// </summary>
    /// <param name="parentFormula"></param>
    /// <param name="source"></param>
    /// <param name="mode"></param>
    public void DoDefaultSettings(ConnectedFormulaView? parentFormula, ReciverControlPadItem source, string mode) {
        Name = source.DefaultItemToControlName(parentFormula?.Page?.KeyName);
        Mode = mode;
        if (source is AbstractPadItem ali) { GeneratedFrom = ali; }

        if (this is IHasSettings ihs) {
            ihs.SettingsManualFilename = ("%homepath%\\" + Develop.AppName() + "\\" + source.KeyName + ".ini").CheckFile();
        }

        if (parentFormula == null) { return; }

        if (source.Parent is ItemCollectionPadItem { IsDisposed: false } icpi) {
            foreach (var thisKey in source.Parents) {
                var it = icpi[thisKey];

                if (it is IItemToControl itc) {
                    var parentCon = parentFormula.SearchOrGenerate(itc, false, mode);
                    if (parentCon is GenericControlReciverSender parentConSender) {
                        parentConSender.ChildIsBorn(this);
                    }
                } else if (it is RowEntryPadItem) {
                    parentFormula.ChildIsBorn(this);
                }
            }
        }
    }

    /// <summary>
    /// Verwirft den aktuellen InputFilter.
    /// </summary>
    public virtual void Invalidate_FilterInput() {
        if (IsDisposed) { return; }

        if (FilterInputChangedHandled) {
            if (_filterInputChangedHandling) {
                Develop.DebugPrint(ErrorType.Error, "Filter wird gerade berechnet");
            }

            FilterInputChangedHandled = false;
            Invalidate_RowsInput();
        }

        Invalidate();
    }

    public RowItem? RowSingleOrNull() {
        if (IsDisposed) { return null; }
        if (DesignMode) { return null; }

        if (!RowsInputManualSeted) {
            if (!FilterInputChangedHandled) { Develop.DebugPrint(ErrorType.Error, "FilterInput nicht gehandelt"); }
            if (!RowsInputChangedHandled) { Develop.DebugPrint(ErrorType.Error, "RowInput nicht gehandelt"); }
        }

        if (RowsInput is not { Count: 1 } r) { return null; }
        _ = r[0].CheckRow();
        return r[0];
    }

    public void SetToRow(RowItem? row) {
        if (IsDisposed) { return; }
        if (Parents.Count > 0) {
            Develop.DebugPrint(ErrorType.Error, "Element wird von Parents gesteuert!");
        }

        var doAtabaseAfter = _databaseInput == null;

        if (!RowsInputManualSeted) {
            FilterInputChangedHandled = true;
            RowsInputChangedHandled = true;
            RowsInputManualSeted = true;
        }

        if (row != null && DatabaseInput is { IsDisposed: false } dbi) {
            if (row.Database != dbi) {
                row = null;
            }
        }

        if (row == RowSingleOrNull()) { return; }
        RowsInput = [];

        if (row?.Database is { IsDisposed: false }) {
            RowsInput.Add(row);
            _ = row.CheckRow();

            if (doAtabaseAfter) { RegisterEvents(); }
        }

        Invalidate_RowsInput();
    }

    protected virtual void DatabaseInput_CellValueChanged(object sender, CellEventArgs e) { }

    protected virtual void DatabaseInput_ColumnPropertyChanged(object sender, ColumnEventArgs e) { }

    protected void DatabaseInput_Disposed(object sender, System.EventArgs e) { }

    protected virtual void DatabaseInput_Loaded(object sender, System.EventArgs e) { }

    protected virtual void DatabaseInput_RowChecked(object sender, RowCheckedEventArgs e) { }

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);

        if (disposing) {
            UnRegisterFilterInputAndDispose();
            Invalidate_RowsInput();
            Invalidate_FilterInput();

            foreach (var thisParent in Parents) {
                _ = thisParent.Childs.Remove(this);
            }

            Parent?.Controls.Remove(this);
            Parents.Clear();
        }
    }

    /// <summary>
    /// Verwirft den aktuellen InputFilter und erstellt einen neuen von allen Parents
    /// </summary>
    /// <param name="mustbeDatabase"></param>
    /// <param name="doEmptyFilterToo"></param>
    protected void DoInputFilter(Database? mustbeDatabase, bool doEmptyFilterToo) {
        if (IsDisposed || FilterInputChangedHandled) { return; }

        _filterInputChangedHandling = true;

        FilterInputChangedHandled = true;

        FilterInput = GetInputFilter(mustbeDatabase, doEmptyFilterToo);

        if (FilterInput is { Database: null }) {
            FilterInput = new FilterCollection(mustbeDatabase, "Fehlerhafter Filter") {
                new FilterItem(mustbeDatabase, string.Empty)
            };
            //Develop.DebugPrint(FehlerArt.Fehler, "Datenbank Fehler");
        }
        Invalidate_RowsInput();

        _filterInputChangedHandling = false;
    }

    protected void DoRows() {
        if (RowsInputChangedHandled) { return; }

        RowsInputChangedHandled = true;

        if (RowsInputManualSeted) { return; }

        _rowsInputChangedHandling = true;

        if (!FilterInputChangedHandled) { Develop.DebugPrint(ErrorType.Error, "Filter unbehandelt!"); }

        if (FilterInput == null) {
            RowsInput = [];
            _rowsInputChangedHandling = false;
            return;
        }

        RowsInput = [.. FilterInput.Rows];

        if (RowSingleOrNull() is { IsDisposed: false } r) {
            _ = r.CheckRow();
        }

        _rowsInputChangedHandling = false;
    }

    protected override void DrawControl(Graphics gr, States state) {
        if (IsDisposed) { return; }
        HandleChangesNow();
    }

    protected string FilterHash() {
        if (FilterInput is not { IsDisposed: false, Count: not 0 } fc) { return ("NoFilter|" + Mode).GetHashString(); }

        if (!fc.IsOk()) { return string.Empty; }

        if (fc.HasAlwaysFalse()) { return ("FALSE|" + Mode).GetHashString(); }
        using var fn = fc.Normalized();
        var n = ("F" + fn.ParseableItems().FinishParseable() + Mode).GetHashString();
        return n;
    }

    protected virtual void HandleChangesNow() {
        if (IsDisposed) {
            DatabaseInput = null;
        } else if (RowsInput is { Count: > 0 }) {
            DatabaseInput = RowsInput[0].Database;
        } else if (FilterInput is { IsDisposed: false } fc) {
            DatabaseInput = fc.Database;
        } else {
            DatabaseInput = null;
        }
    }

    protected void Invalidate_RowsInput() {
        if (IsDisposed) { return; }

        if (RowsInputChangedHandled) {
            if (_rowsInputChangedHandling) {
                Develop.DebugPrint(ErrorType.Error, "Rows werden gerade berechnet");
            }

            if (!RowsInputManualSeted) { RowsInput = null; }
            RowsInputChangedHandled = false;
            Invalidate();
        }
    }

    protected override void OnCreateControl() {
        base.OnCreateControl();
        RegisterEvents();
    }

    protected override void OnVisibleChanged(System.EventArgs e) {
        base.OnVisibleChanged(e);
        if (!Visible) { return; }

        Invalidate_FilterInput();
    }

    private void FilterInput_DispodingEvent(object sender, System.EventArgs e) => UnRegisterFilterInputAndDispose();

    private void FilterInput_RowsChanged(object sender, System.EventArgs e) => Invalidate_RowsInput();

    private FilterCollection? GetInputFilter(Database? mustbeDatabase, bool doEmptyFilterToo) {
        if (Parents.Count == 0) {
            return doEmptyFilterToo && mustbeDatabase != null ? new FilterCollection(mustbeDatabase, "Empty Input Filter") : null;
        }

        if (Parents.Count == 1) {
            var fc2 = Parents[0].FilterOutput;
            if (fc2.Count == 0) { return null; }

            if (mustbeDatabase != null && fc2.Database != mustbeDatabase) {
                return new FilterCollection(new FilterItem(mustbeDatabase, "Datenbanken inkonsistent 1"), "Datenbanken inkonsistent");
            }

            return fc2;
        }

        FilterCollection? fc = null;

        foreach (var thiss in Parents) {
            if (thiss is { IsDisposed: false, FilterOutput: { IsDisposed: false } fi }) {
                if (mustbeDatabase != null && fi.Database != mustbeDatabase) {
                    fc?.Dispose();
                    return new FilterCollection(new FilterItem(mustbeDatabase, "Datenbanken inkonsistent 2"), "Datenbanken inkonsistent");
                }

                fc ??= new FilterCollection(fi.Database, "filterofsender");

                foreach (var thifi in fi) {
                    fc.AddIfNotExists(thifi);
                }
            }
        }

        return fc;
    }

    private void RegisterEvents() {
        if (FilterInput is not { IsDisposed: false }) { return; }
        FilterInput.RowsChanged += FilterInput_RowsChanged;
        FilterInput.DisposingEvent += FilterInput_DispodingEvent;
    }

    private void UnRegisterFilterInputAndDispose() {
        if (FilterInput is not { IsDisposed: false }) { return; }
        FilterInput.RowsChanged -= FilterInput_RowsChanged;
        FilterInput.DisposingEvent -= FilterInput_DispodingEvent;

        if (Parents.Count > 1 && FilterInput != null && FilterInputChangedHandled) {
            FilterInput.Database = null;
            FilterInput.Dispose();
        }
    }

    #endregion
}
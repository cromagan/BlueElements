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
using BlueTable;
using BlueTable.Enums;
using BlueTable.EventArgs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace BlueControls.Controls;

public class GenericControlReciver : GenericControl, IBackgroundNone {

    #region Fields

    public readonly List<GenericControlReciverSender> Parents = [];
    protected const string _outputf = "FilterOutput";
    private readonly object _filterInputLock = new object();

    // Ein privates Objekt zum Sperren für die Thread-Sicherheit
    private readonly object _lockObject = new object();

    private string? _cachedFilterHash = null;

    private FilterCollection? _filterInput;

    private Table? _tableInput;

    #endregion

    #region Constructors

    public GenericControlReciver() : base(false, false, false) { }

    public GenericControlReciver(bool doubleBuffer, bool useBackgroundBitmap, bool mouseHighlight) : base(doubleBuffer, useBackgroundBitmap, mouseHighlight) { }

    #endregion

    #region Properties

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
    public Table? TableInput {
        get => _tableInput;

        private set {
            // Wichtig! Darf nur von HandelChangesNow befüllt werden!
            // Grund: Es wird hier nichts invalidiert!

            if (value is { IsDisposed: true }) { value = null; }

            if (value == _tableInput) { return; }

            // Thread-Sicherheit: Sperren Sie den Zugriff auf _tableInput
            lock (_lockObject) {
                if (_tableInput is { }) {
                    _tableInput.Cell.CellValueChanged -= TableInput_CellValueChanged;
                    _tableInput.Column.ColumnPropertyChanged -= TableInput_ColumnPropertyChanged;
                    _tableInput.Row.RowChecked -= TableInput_RowChecked;
                    _tableInput.Loaded -= TableInput_Loaded;
                    _tableInput.Disposed -= TableInput_Disposed;
                }

                _tableInput = value;

                if (_tableInput is { IsDisposed: false }) {
                    _tableInput.Cell.CellValueChanged += TableInput_CellValueChanged;
                    _tableInput.Column.ColumnPropertyChanged += TableInput_ColumnPropertyChanged;
                    _tableInput.Row.RowChecked += TableInput_RowChecked;
                    _tableInput.Loaded += TableInput_Loaded;
                    _tableInput.Disposed += TableInput_Disposed;
                }
            }
        }
    }

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
            _cachedFilterHash = null; // Cache invalidieren
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
    protected bool RowsInputChangedHandled { get; set; }

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
        Name = source.DefaultItemToControlName(parentFormula?.Page?.UniqueId);
        Mode = mode;
        if (source is AbstractPadItem ali) { GeneratedFrom = ali; }

        if (this is IHasSettings ihs) {
            ihs.SettingsManualFilename = ("%homepath%\\" + Develop.AppName() + "\\" + source.KeyName + ".ini").NormalizeFile();
        }

        if (parentFormula == null) { return; }

        if (source.Parent is ItemCollectionPadItem { IsDisposed: false } icpi) {
            foreach (var thisKey in source.Parents) {
                var it = icpi[thisKey];

                if (it is IItemToControl itc) {
                    var parentCon = parentFormula.SearchOrGenerate(itc, mode);
                    if (parentCon is GenericControlReciverSender parentConSender) {
                        parentConSender.ChildIsBorn(this);
                    }
                } else if (it is RowEntryPadItem) {
                    parentFormula.ChildIsBorn(this);
                }
            }
        } else {
            Develop.DebugPrint(ErrorType.Warning, "Source hat keinen Parent!");
        }
    }

    /// <summary>
    /// Verwirft den aktuellen InputFilter.
    /// </summary>
    public virtual void Invalidate_FilterInput() {
        if (IsDisposed) { return; }

        lock (_filterInputLock) {
            // Wenn bereits eine Berechnung läuft, Flag setzen und zurückkehren
            if (_filterInputChangedHandling) {
                // Merken, dass nach der aktuellen Berechnung erneut invalidiert werden muss
                FilterInputChangedHandled = false;
                return;
            }

            if (FilterInputChangedHandled) {
                FilterInputChangedHandled = false;
                _cachedFilterHash = null;
            } else {
                // Bereits invalidiert, nichts zu tun
                return;
            }
        }

        // Außerhalb des Locks invalidieren
        _cachedFilterHash = null;
        Invalidate_RowsInput();
        Invalidate();
    }

    public RowItem? RowSingleOrNull() {
        if (IsDisposed || DesignMode) { return null; }

        if (!FilterInputChangedHandled) { Develop.DebugPrint(ErrorType.Error, "FilterInput nicht gehandelt"); }

        if (FilterInput?.Rows is not { Count: 1 } r) { return null; }
        _ = r[0].CheckRow();
        return r[0];
    }

    public void SetToRow(RowItem? row) {
        if (IsDisposed) { return; }

        // Wenn Parents existieren, diese entfernen
        if (Parents.Count > 0) {
            foreach (var parent in Parents) {
                _ = parent.Childs.Remove(this);
            }
            Parents.Clear();
        }

        // Row validieren
        if (row != null && TableInput is { IsDisposed: false } tb) {
            if (row.Table != tb) { row = null; }
        }

        if (row?.Table is { } tb2) {
            var fc = new FilterCollection(tb2, "SetToRow Filter");
            FilterInputChangedHandled = true;

            // FilterInput direkt setzen
            lock (_filterInputLock) {
                fc.Add(new FilterItem(tb2, FilterType.RowKey, row.KeyName));

                FilterInput = fc;
                _ = row?.CheckRow();
            }
        } else {
            FilterInput = null;
        }

        Invalidate_RowsInput();
        Invalidate();
    }

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
    /// <param name="mustbeTable"></param>
    /// <param name="doEmptyFilterToo"></param>
    protected void DoInputFilter(Table? mustbeTable, bool doEmptyFilterToo) {
        if (IsDisposed) { return; }

        lock (_filterInputLock) {
            if (FilterInputChangedHandled) { return; }

            if (_filterInputChangedHandling) {
                // Bereits in Berechnung - nach Abschluss erneut prüfen
                FilterInputChangedHandled = false;
                return;
            }

            _filterInputChangedHandling = true;
            FilterInputChangedHandled = true;
        }

        try {
            FilterInput = GetInputFilter(mustbeTable, doEmptyFilterToo);

            if (FilterInput is { Table: null }) {
                FilterInput = new FilterCollection(mustbeTable, "Fehlerhafter Filter") {
                new FilterItem(mustbeTable, string.Empty)
            };
            }

            Invalidate_RowsInput();
        } catch (Exception ex) {
            Develop.DebugPrint(ErrorType.Error, "Unerwartet bei DoInputFilter", ex);
        } finally {
            lock (_filterInputLock) {
                _filterInputChangedHandling = false;

                // Wenn während der Berechnung erneut invalidiert wurde
                if (!FilterInputChangedHandled) {
                    // Erneut aufrufen (außerhalb des Locks)
                    System.Threading.Tasks.Task.Run(() => Invalidate_FilterInput());
                }
            }
        }
    }

    protected override void DrawControl(Graphics gr, States state) {
        if (IsDisposed) { return; }
        HandleChangesNow();
    }

    protected string FilterHash() {
        if (_cachedFilterHash != null) { return _cachedFilterHash; }

        if (FilterInput is not { IsDisposed: false, Count: not 0 } fc) {
            _cachedFilterHash = ("NoFilter|" + Mode).GetHashString();
            return _cachedFilterHash;
        }

        if (!fc.IsOk()) {
            _cachedFilterHash = string.Empty;
            return _cachedFilterHash;
        }

        if (fc.HasAlwaysFalse()) {
            _cachedFilterHash = ("FALSE|" + Mode).GetHashString();
            return _cachedFilterHash;
        }

        using var fn = fc.Normalized();
        _cachedFilterHash = ("F" + fn.ParseableItems().FinishParseable() + Mode).GetHashString();
        return _cachedFilterHash;
    }

    protected virtual void HandleChangesNow() {
        if (IsDisposed) {
            TableInput = null;
        } else if (FilterInput is { IsDisposed: false } fc) {
            TableInput = fc.Table;
        } else {
            TableInput = null;
        }
    }

    protected void Invalidate_RowsInput() {
        if (IsDisposed || !RowsInputChangedHandled) { return; }
        RowsInputChangedHandled = false;
        Invalidate();
    }

    protected override void OnCreateControl() {
        base.OnCreateControl();
        RegisterEvents();
    }

    protected override void OnVisibleChanged(System.EventArgs e) {
        base.OnVisibleChanged(e);
        if (!Visible) { return; }

        Invalidate_RowsInput();
    }

    protected virtual void TableInput_CellValueChanged(object sender, CellEventArgs e) { }

    protected virtual void TableInput_ColumnPropertyChanged(object sender, ColumnEventArgs e) { }

    protected void TableInput_Disposed(object sender, System.EventArgs e) {
        TableInput = null;
    }

    protected virtual void TableInput_Loaded(object sender, System.EventArgs e) { }

    protected virtual void TableInput_RowChecked(object sender, RowCheckedEventArgs e) { }

    private void FilterInput_DisposingEvent(object sender, System.EventArgs e) => UnRegisterFilterInputAndDispose();

    private void FilterInput_RowsChanged(object sender, System.EventArgs e) => Invalidate_RowsInput();

    private FilterCollection? GetInputFilter(Table? mustbeTable, bool doEmptyFilterToo) {
        if (Parents.Count == 0) {
            return doEmptyFilterToo && mustbeTable != null ? new FilterCollection(mustbeTable, "Empty Input Filter") : null;
        }

        if (Parents.Count == 1) {
            var parent = Parents[0];
            if (parent.IsDisposed || parent.FilterOutput.IsDisposed) {
                return null;
            }

            var fc2 = parent.FilterOutput;
            if (fc2.Count == 0) { return null; }

            if (mustbeTable != null && fc2.Table != mustbeTable) {
                return new FilterCollection(new FilterItem(mustbeTable, "Tabellen inkonsistent 1"), "Tabellen inkonsistent");
            }

            return fc2;
        }

        FilterCollection? fc = null;

        foreach (var thiss in Parents) {
            if (thiss is { IsDisposed: false, FilterOutput: { IsDisposed: false } fi }) {
                if (mustbeTable != null && fi.Table != mustbeTable) {
                    fc?.Dispose();
                    return new FilterCollection(new FilterItem(mustbeTable, "Tabellen inkonsistent 2"), "Tabellen inkonsistent");
                }

                fc ??= new FilterCollection(fi.Table, "filterofsender");

                foreach (var thifi in fi) {
                    fc.AddIfNotExists(thifi);
                }
            }
        }

        return fc;
    }

    private void RegisterEvents() {
        if (_filterInput is not { IsDisposed: false }) { return; }
        _filterInput.RowsChanged += FilterInput_RowsChanged;
        _filterInput.DisposingEvent += FilterInput_DisposingEvent;
    }

    private void UnRegisterFilterInputAndDispose() {
        if (_filterInput is not { IsDisposed: false }) { return; }

        // Events zuerst entfernen
        _filterInput.RowsChanged -= FilterInput_RowsChanged;
        _filterInput.DisposingEvent -= FilterInput_DisposingEvent;

        var filterToDispose = _filterInput;

        // Explizite Referenz löschen
        _filterInput = null;

        // Dispose durchführen, wenn wir dafür verantwortlich sind
        if (!filterToDispose.Coment.StartsWith(_outputf, StringComparison.OrdinalIgnoreCase)) {
            filterToDispose.Table = null;
            filterToDispose.Dispose();
        }
    }

    #endregion
}
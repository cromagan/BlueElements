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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace BlueControls.Controls;

public class GenericControlReciver : GenericControl, IBackgroundNone {

    #region Fields

    public readonly List<GenericControlReciverSender> Parents = [];
    private readonly object _filterInputLock = new object();

    // Ein privates Objekt zum Sperren für die Thread-Sicherheit
    private readonly object _lockObject = new object();

    private readonly object _rowsInputLock = new object();
    private string? _cachedFilterHash = null;
    private Database? _databaseInput;

    private FilterCollection? _filterInput;

    private bool _rowsInputChangedHandling;

    private int _waitTimeoutMs = 5000;

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
            // Wichtig! Darf nur von HandelChangesNow befüllt werden!
            // Grund: Es wird hier nichts invalidiert!

            if (value is { IsDisposed: true }) { value = null; }

            if (value == _databaseInput) { return; }

            // Thread-Sicherheit: Sperren Sie den Zugriff auf _databaseInput
            lock (_lockObject) {
                if (_databaseInput is { }) {
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
        } else {
            Develop.DebugPrint(ErrorType.Warning, "Source hat keinen Parent!");
        }
    }

    /// <summary>
    /// Verwirft den aktuellen InputFilter.
    /// </summary>
    public virtual void Invalidate_FilterInput() {
        if (IsDisposed) { return; }

        var filterNeedsInvalidation = false;

        lock (_filterInputLock) {
            if (FilterInputChangedHandled) {
                if (_filterInputChangedHandling) {
                    Develop.DebugPrint(ErrorType.Error, "Filter wird gerade berechnet");
                    return;
                }

                FilterInputChangedHandled = false;
                filterNeedsInvalidation = true;
            }
        }

        if (filterNeedsInvalidation) {
            _cachedFilterHash = null; // Cache invalidieren
            Invalidate_RowsInput();
        }

        Invalidate();
    }

    public RowItem? RowSingleOrNull() {
        if (IsDisposed || DesignMode) { return null; }

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
            return;
        }

        // Prüfen, ob wir nach der Zuweisung Events registrieren müssen
        var needRegisterEvents = _databaseInput == null;

        // Sicherstellen, dass die manuellen Flags gesetzt sind
        if (!RowsInputManualSeted) {
            lock (_rowsInputLock) {
                FilterInputChangedHandled = true;
                RowsInputChangedHandled = true;
                RowsInputManualSeted = true;
            }
        }

        // Prüfen, ob die Row zur aktuellen Tabelle gehört
        if (row != null && _databaseInput is { IsDisposed: false } dbi) {
            if (row.Database != dbi) {
                row = null;
            }
        }

        // Keine Änderung notwendig, wenn die Row bereits die aktuelle ist
        if (row == RowSingleOrNull()) { return; }

        // Neue Rows-Liste erstellen (thread-sicher)
        lock (_rowsInputLock) {
            RowsInput = [];

            if (row?.Database is { IsDisposed: false }) {
                RowsInput.Add(row);
            }
        }

        // Zusätzliche Verarbeitung außerhalb des Locks
        if (row?.Database is { IsDisposed: false }) {
            _ = row.CheckRow();

            if (needRegisterEvents) { RegisterEvents(); }
        }

        Invalidate_RowsInput();
    }

    protected virtual void DatabaseInput_CellValueChanged(object sender, CellEventArgs e) { }

    protected virtual void DatabaseInput_ColumnPropertyChanged(object sender, ColumnEventArgs e) { }

    protected void DatabaseInput_Disposed(object sender, System.EventArgs e) {
        DatabaseInput = null;
    }

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

        try {
            FilterInputChangedHandled = true;

            FilterInput = GetInputFilter(mustbeDatabase, doEmptyFilterToo);

            if (FilterInput is { Database: null }) {
                FilterInput = new FilterCollection(mustbeDatabase, "Fehlerhafter Filter") {
                new FilterItem(mustbeDatabase, string.Empty)
            };
                //Develop.DebugPrint(ErrorType.Error, "Tabelle Fehler");
            }
            Invalidate_RowsInput();
        } catch (Exception ex) {
            Develop.DebugPrint(ErrorType.Error, "Unerwartet bei DoInputFilter", ex);
        } finally {
            _filterInputChangedHandling = false;
        }
    }

    protected void DoRows() {
        if (RowsInputChangedHandled || IsDisposed) { return; }

        var needsProcessing = false;

        lock (_rowsInputLock) {
            // Doppelte Prüfung im Lock (Verhindert Race Conditions)
            if (RowsInputChangedHandled || _rowsInputChangedHandling) { return; }

            RowsInputChangedHandled = true;

            if (RowsInputManualSeted) { return; }

            _rowsInputChangedHandling = true;
            needsProcessing = true;
        }

        try {
            if (needsProcessing) {
                if (!FilterInputChangedHandled) {
                    Develop.DebugPrint(ErrorType.Error, "Filter unbehandelt!");
                }

                if (FilterInput == null) {
                    lock (_rowsInputLock) {
                        RowsInput = [];
                    }
                    return;
                }

                // Erstelle eine Kopie der Rows außerhalb des Locks
                List<RowItem> rowsToProcess = [.. FilterInput.Rows];
                // Verarbeitung außerhalb des Locks
                if (RowSingleOrNull() is { IsDisposed: false } r) {
                    _ = r.CheckRow();
                }

                // Ergebnis im Lock speichern
                lock (_rowsInputLock) {
                    RowsInput = rowsToProcess;
                }
            }
        } catch (Exception ex) {
            Develop.DebugPrint(ErrorType.Error, "Unerwartet bei DoRows", ex);
        } finally {
            lock (_rowsInputLock) {
                _rowsInputChangedHandling = false;
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
            DatabaseInput = null;
        } else if (RowsInput is { Count: > 0 } ri && ri[0] is { IsDisposed: false } row && row.Database is { IsDisposed: false } db) {
            DatabaseInput = db;
        } else if (FilterInput is { IsDisposed: false } fc) {
            DatabaseInput = fc.Database;
        } else {
            DatabaseInput = null;
        }
    }

    protected void Invalidate_RowsInput() {
        if (IsDisposed || !RowsInputChangedHandled) { return; }

        var lockTaken = false;
        try {
            // Versuche den Lock zu erhalten mit Timeout
            lockTaken = System.Threading.Monitor.TryEnter(_rowsInputLock, _waitTimeoutMs);

            if (!lockTaken) {
                Develop.DebugPrint(ErrorType.Error, "Timeout beim Warten auf Rows-Berechnung");
                return; // Abbruch bei Timeout
            }

            // Prüfen, ob Berechnung läuft (im sicheren Kontext)
            if (_rowsInputChangedHandling) {
                Develop.DebugPrint(ErrorType.Error, "Rows werden gerade berechnet");
                return; // Wichtig: Return wenn Berechnung läuft
            }

            if (!RowsInputManualSeted) { RowsInput = null; }
            RowsInputChangedHandled = false;
        } catch (Exception ex) {
            Develop.DebugPrint(ErrorType.Error, "Unerwartet bei Invalidate_RowsInput", ex);
        } finally {
            // Lock immer freigeben, wenn er erhalten wurde
            if (lockTaken) {
                System.Threading.Monitor.Exit(_rowsInputLock);
            }
        }

        // Invalidate außerhalb des Lock-Blocks aufrufen
        Invalidate();
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

    private void FilterInput_DisposingEvent(object sender, System.EventArgs e) => UnRegisterFilterInputAndDispose();

    private void FilterInput_RowsChanged(object sender, System.EventArgs e) => Invalidate_RowsInput();

    private FilterCollection? GetInputFilter(Database? mustbeDatabase, bool doEmptyFilterToo) {
        if (Parents.Count == 0) {
            return doEmptyFilterToo && mustbeDatabase != null ? new FilterCollection(mustbeDatabase, "Empty Input Filter") : null;
        }

        if (Parents.Count == 1) {
            var parent = Parents[0];
            if (parent.IsDisposed || parent.FilterOutput.IsDisposed) {
                return null;
            }

            var fc2 = parent.FilterOutput;
            if (fc2.Count == 0) { return null; }

            if (mustbeDatabase != null && fc2.Database != mustbeDatabase) {
                return new FilterCollection(new FilterItem(mustbeDatabase, "Tabellen inkonsistent 1"), "Tabellen inkonsistent");
            }

            return fc2;
        }

        FilterCollection? fc = null;

        foreach (var thiss in Parents) {
            if (thiss is { IsDisposed: false, FilterOutput: { IsDisposed: false } fi }) {
                if (mustbeDatabase != null && fi.Database != mustbeDatabase) {
                    fc?.Dispose();
                    return new FilterCollection(new FilterItem(mustbeDatabase, "Tabellen inkonsistent 2"), "Tabellen inkonsistent");
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
        FilterInput.DisposingEvent += FilterInput_DisposingEvent;
    }

    private void UnRegisterFilterInputAndDispose() {
        if (FilterInput is not { IsDisposed: false }) { return; }

        // Events zuerst entfernen
        FilterInput.RowsChanged -= FilterInput_RowsChanged;
        FilterInput.DisposingEvent -= FilterInput_DisposingEvent;

        var filterToDispose = FilterInput;

        // Explizite Referenz löschen
        _filterInput = null;

        // Dispose durchführen, wenn wir dafür verantwortlich sind
        if (Parents.Count > 1) {
            // Nur bei mehreren Parents sind wir sicher für den Filter verantwortlich
            filterToDispose.Database = null;
            filterToDispose.Dispose();
        }
    }

    #endregion
}
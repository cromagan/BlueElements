// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes.ItemCollectionPad;
using BlueControls.Classes.ItemCollectionPad.Abstract;
using BlueControls.Classes.ItemCollectionPad.FunktionsItems_Formular;
using BlueControls.Classes.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using BlueTable.EventArgs;

namespace BlueControls.Controls.ConnectedFormula;

public class GenericControlReciver : GenericControl, IBackgroundNone {

    #region Fields

    public readonly List<GenericControlReciverSender> Parents = [];
    protected const string _outputf = "FilterOutput";

    [ThreadStatic]
    private static int _suppressUpdates;

    private readonly object _filterInputLock = new();

    // Ein privates Objekt zum Sperren für die Thread-Sicherheit
    private readonly object _lockObject = new();

    private string? _cachedFilterHash;

    private FilterCollection? _filterInput;
    private bool _filterInputChangedHandling;

    #endregion

    #region Constructors

    public GenericControlReciver() : base(false, false, false) { }

    public GenericControlReciver(bool doubleBuffer, bool useBackgroundBitmap, bool mouseHighlight) : base(doubleBuffer, useBackgroundBitmap, mouseHighlight) { }

    #endregion

    #region Properties

    public static bool IsUpdating => _suppressUpdates > 0;

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public AbstractPadItem? GeneratedFrom { get; set; }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public virtual string Mode { get; set; } = string.Empty;

    public override string QuickInfo {
        get => base.QuickInfo;
        set {
            base.QuickInfo = value;

            foreach (var thisC in Controls) {
                if (thisC is GenericControl gc) { gc.QuickInfo = value; }
            }
        }
    }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Table? TableInput {
        get;

        private set {
            // Wichtig! Darf nur von HandelChangesNow befüllt werden!
            // Grund: Es wird hier nichts invalidiert!

            if (value is { IsDisposed: true }) { value = null; }

            if (value == field) { return; }

            // Thread-Sicherheit: Sperren Sie den Zugriff auf _tableInput
            lock (_lockObject) {
                if (field is { }) {
                    field.Cell.CellValueChanged -= TableInput_CellValueChanged;
                    field.Column.ColumnPropertyChanged -= TableInput_ColumnPropertyChanged;
                    field.Row.RowChecked -= TableInput_RowChecked;
                    field.Loaded -= TableInput_Loaded;
                    field.Disposed -= TableInput_Disposed;
                }

                field = value;

                if (field is { IsDisposed: false }) {
                    field.Cell.CellValueChanged += TableInput_CellValueChanged;
                    field.Column.ColumnPropertyChanged += TableInput_ColumnPropertyChanged;
                    field.Row.RowChecked += TableInput_RowChecked;
                    field.Loaded += TableInput_Loaded;
                    field.Disposed += TableInput_Disposed;
                }
            }
        }
    }

    /// <summary>
    /// Aggregierter Filter aus allen Parent-Controls (FilterOutput der Parents).
    /// Wird durch DoInputFilter() aus den FilterOutputs aller Parents zusammengeführt.
    /// Ist null wenn keine Parents vorhanden sind oder alle FilterOutputs leer sind.
    /// </summary>
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

    #endregion

    #region Methods

    public static void BeginUpdate() => _suppressUpdates++;

    public static void EndUpdate() {
        if (_suppressUpdates > 0) { _suppressUpdates--; }
    }

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
            ihs.SettingsManualFilename = $"%appdocumentpath%\\{source.KeyName}.ini".NormalizeFile();
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
            Develop.DebugPrint("Source hat keinen Parent!");
        }
    }

    /// <summary>
    /// Markiert den FilterInput als veraltet, sodass er beim nächsten
    /// HandleChangesNow()-Aufruf aus den FilterOutputs der Parents neu berechnet wird.
    /// </summary>
    public virtual void Invalidate_FilterInput() {
        if (IsDisposed) { return; }

        if (Parents.Count != 0) {
            // Parents vorhanden - FilterInput muss neu aus FilterOutputs berechnet werden
            lock (_filterInputLock) {
                _cachedFilterHash = null;
                if (!FilterInputChangedHandled) { return; }
                FilterInputChangedHandled = false;
            }
        }
        Invalidate_RowsInput();
        if (!IsUpdating) { Invalidate(); }
    }

    public RowItem? RowSingleOrNull() {
        if (IsDisposed || DesignMode) { return null; }

        if (!FilterInputChangedHandled) { Develop.DebugError("FilterInput nicht gehandelt"); }

        if (FilterInput?.Rows is not { Count: 1 } r) { return null; }
        r[0].CheckRow();
        return r[0];
    }

    public virtual void SetToRow(RowItem? row) {
        if (IsDisposed) { return; }

        // Wenn Parents existieren, diese entfernen
        if (Parents.Count > 0) {
            foreach (var parent in Parents) {
                parent.Childs.Remove(this);
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

                if (tb2.Column.ChunkValueColumn is { } cvc) {
                    fc.Add(new FilterItem(cvc, FilterType.Istgleich, row.ChunkValue));
                }

                FilterInput = fc;
                row.CheckRow();
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
                thisParent.Childs.Remove(this);
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
        } catch {
            throw Develop.DebugError("Unerwartet bei DoInputFilter");
        } finally {
            lock (_filterInputLock) {
                _filterInputChangedHandling = false;

                // Wenn während der Berechnung erneut invalidiert wurde
                if (!FilterInputChangedHandled) {
                    // Erneut aufrufen (außerhalb des Locks)
                    System.Threading.Tasks.Task.Run(Invalidate_FilterInput);
                }
            }
        }
    }

    protected override void DrawControl(Graphics gr, States state) {
        if (IsDisposed) { return; }
        base.DrawControl(gr, state);
        HandleChangesNow();
    }

    protected string FilterHash() {
        if (_cachedFilterHash != null) { return _cachedFilterHash; }

        if (FilterInput is not { IsDisposed: false, Count: not 0 } fc) {
            _cachedFilterHash = ("NoFilter|" + Mode).GetMD5Hash();
            return _cachedFilterHash;
        }

        if (!fc.IsOk()) {
            _cachedFilterHash = string.Empty;
            return _cachedFilterHash;
        }

        if (fc.HasAlwaysFalse()) {
            _cachedFilterHash = ("FALSE|" + Mode).GetMD5Hash();
            return _cachedFilterHash;
        }

        using var fn = fc.Normalized();
        _cachedFilterHash = ("F" + fn.ParseableItems().FinishParseable() + Mode).GetSHA256HashString();
        return _cachedFilterHash;
    }

    /// <summary>
    /// Befüllt TableInput. Der Wert wird aus FilterInput generiert.
    /// </summary>
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
        if (!IsUpdating) { Invalidate(); }
    }

    protected void NextControl(NavigationDirection direction) {
        if (IsDisposed || Parent == null) { return; }

        var siblings = new List<GenericControlReciver>();
        foreach (System.Windows.Forms.Control c in Parent.Controls) {
            if (c is GenericControlReciver gcr && gcr != this && gcr.Visible && gcr.Enabled && !gcr.IsDisposed) {
                siblings.Add(gcr);
            }
        }

        if (siblings.Count == 0) { return; }

        siblings.Sort((a, b) => {
            var cmp = a.Top.CompareTo(b.Top);
            return cmp != 0 ? cmp : a.Left.CompareTo(b.Left);
        });

        var index = -1;
        for (var i = 0; i < siblings.Count; i++) {
            if (siblings[i] == this) {
                index = i;
                break;
            }
        }

        if (index < 0) {
            for (var i = 0; i < siblings.Count; i++) {
                if (IsToTheRightOrBelow(this, siblings[i])) {
                    index = i;
                    break;
                }
            }
            if (index < 0) { index = 0; }
        }

        if (direction == NavigationDirection.Previous) {
            index = index > 0 ? index - 1 : siblings.Count - 1;
        } else {
            index = index < siblings.Count - 1 ? index + 1 : 0;
        }

        var target = siblings[index];
        target.Focus();
    }

    protected override void OnCreateControl() {
        base.OnCreateControl();
        RegisterEvents();
    }

    protected override void OnVisibleChanged(System.EventArgs e) {
        base.OnVisibleChanged(e);
        if (!Visible) { return; }

        // Wenn das Control wieder sichtbar wird (z.B. nach Tab-Wechsel),
        // müssen alle Eingaben komplett neu verarbeitet werden.
        // Grund: Während der Unsichtbarkeit kann die Benachrichtigungskette
        // unterbrochen gewesen sein — ein unsichtbarer GenericControlReciverSender
        // aktualisiert sein FilterOutput nicht (HandleChangesNow wird nie aufgerufen),
        // sodass dessen Kinder nie Invalidate_FilterInput() erhalten.
        Invalidate_FilterInput();
    }

    protected virtual void TableInput_CellValueChanged(object? sender, CellEventArgs e) { }

    protected virtual void TableInput_ColumnPropertyChanged(object? sender, ColumnEventArgs e) { }

    protected void TableInput_Disposed(object? sender, System.EventArgs e) => TableInput = null;

    protected virtual void TableInput_Loaded(object? sender, System.EventArgs e) { }

    protected virtual void TableInput_RowChecked(object? sender, RowPrepareFormulaEventArgs e) { }

    private static bool IsToTheRightOrBelow(System.Windows.Forms.Control current, System.Windows.Forms.Control candidate) {
        if (candidate.Top > current.Top) { return true; }
        if (candidate.Top == current.Top && candidate.Left > current.Left) { return true; }
        return false;
    }

    private void FilterInput_DisposingEvent(object? sender, System.EventArgs e) => UnRegisterFilterInputAndDispose();

    private void FilterInput_RowsChanged(object? sender, System.EventArgs e) => Invalidate_RowsInput();

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
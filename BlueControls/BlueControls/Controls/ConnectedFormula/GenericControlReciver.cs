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
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueDatabase;
using BlueDatabase.EventArgs;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace BlueControls.Controls;

public class GenericControlReciver : GenericControl, IBackgroundNone {

    #region Fields

    private Database? _databaseInput;

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    private FilterCollection? _filterInput;

    #endregion

    #region Constructors

    public GenericControlReciver(bool doubleBuffer, bool useBackgroundBitmap) : base(doubleBuffer, useBackgroundBitmap) { }

    #endregion

    #region Properties

    public Database? DatabaseInput {
        get => _databaseInput;

        private set {
            // Wichtig! Darf nur von HandelChangesNow befültg werden!
            // Grund: Es wird hier nichts invalidiert!

            if (value != null && value.IsDisposed) { value = null; }

            if (value == _databaseInput) { return; }

            if (_databaseInput != null) {
                _databaseInput.Cell.CellValueChanged -= DatabaseInput_CellValueChanged;
                _databaseInput.Column.ColumnPropertyChanged -= DatabaseInput_ColumnPropertyChanged;
                _databaseInput.Row.RowChecked -= DatabaseInput_RowChecked;
                _databaseInput.Loaded -= DatabaseInput_Loaded;
                _databaseInput.Disposed -= DatabaseInput_Disposed;
            }

            _databaseInput = value;

            if (_databaseInput != null && !_databaseInput.IsDisposed) {
                _databaseInput.Cell.CellValueChanged += DatabaseInput_CellValueChanged;
                _databaseInput.Column.ColumnPropertyChanged += DatabaseInput_ColumnPropertyChanged;
                _databaseInput.Row.RowChecked += DatabaseInput_RowChecked;
                _databaseInput.Loaded += DatabaseInput_Loaded;
                _databaseInput.Disposed += DatabaseInput_Disposed;
            }
        }
    }

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    protected FilterCollection? FilterInput {
        get => _filterInput;
        set {
            if (_filterInput == value) { return; }
            UnRegisterFilterInputAndDispose();
            _filterInput = value;
            RegisterEvents();
        }
    }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    protected bool FilterInputChangedHandled { get; set; }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public List<GenericControlReciverSender> Parents { get; } = [];

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    protected List<RowItem>? RowsInput { get; set; }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    protected bool RowsInputChangedHandled { get; private set; }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    private bool RowsInputManualSeted { get; set; } = false;

    #endregion

    #region Methods

    public void ConnectChildParents(GenericControlReciverSender parent) {
        if (RowsInputManualSeted) {
            Develop.DebugPrint(FehlerArt.Fehler, "Manuelle Filterung kann keine Parents empfangen.");
        }

        if (parent.IsDisposed) { return; }
        if (IsDisposed) { return; }

        var isnew = !Parents.Contains(parent);
        var newFilters = parent.FilterOutput.Count > 0;
        var doDatabaseAfter = DatabaseInput == null;

        if (isnew) { Parents.AddIfNotExists(parent); }

        parent.Childs.AddIfNotExists(this);

        if (newFilters && isnew) { Invalidate_FilterInput(); }

        if (doDatabaseAfter) { RegisterEvents(); }
    }

    public void DisconnectChildParents(List<GenericControlReciverSender> parents) {
        var p = new List<GenericControlReciverSender>();
        p.AddRange(parents);

        foreach (var parent in p) {
            DisconnectChildParents(parent);
        }
    }

    public void DisconnectChildParents(GenericControlReciverSender parent) {
        Parents.Remove(parent);

        if (parent.Childs.Contains(this)) {
            parent.Childs.Remove(this);
        }
    }

    /// <summary>
    /// Nachdem das Control erzeugt wurde, werden hiermit die Einstellungen vom IItemAcceptFilter übernommen.
    /// </summary>
    /// <param name="dest"></param>
    /// <param name="parent"></param>
    /// <param name="source"></param>
    public void DoDefaultSettings(ConnectedFormulaView parent, IItemAcceptFilter source) {
        Name = source.DefaultItemToControlName();

        foreach (var thisKey in source.Parents) {
            var it = source.Parent?[thisKey];

            if (it is IItemToControl itc) {
                var ff = parent.SearchOrGenerate(itc);

                if (ff is GenericControlReciverSender ffx) {
                    ConnectChildParents(ffx);
                }
            }
        }
    }

    /// <summary>
    /// Verwirft den aktuellen InputFilter und erstellt einen neuen von allen Parents
    /// </summary>
    /// <param name="item"></param>
    /// <param name="mustbeDatabase"></param>
    /// <param name="doEmptyFilterToo"></param>
    public void DoInputFilter(Database? mustbeDatabase, bool doEmptyFilterToo) {
        if (IsDisposed || FilterInputChangedHandled) { return; }

        FilterInputChangedHandled = true;

        FilterInput = GetInputFilter(mustbeDatabase, doEmptyFilterToo);

        if (FilterInput != null && FilterInput.Database == null) {
            FilterInput = new FilterCollection(mustbeDatabase, "Fehlerhafter Filter");
            FilterInput.Add(new FilterItem(mustbeDatabase, string.Empty));
            //Develop.DebugPrint(FehlerArt.Fehler, "Datenbank Fehler");
        }
    }

    public string FilterHash() {
        if (FilterInput is not FilterCollection fc) { return "NoFilter"; }

        if (fc.Count == 0) { return "NoFilter"; }

        if (!fc.IsOk()) { return string.Empty; }

        if (fc.HasAlwaysFalse()) { return "FALSE"; }
        var fn = (FilterCollection)fc.Clone("Normalize");
        fn.Normalize();

        var n = "F" + Generic.GetHashString(fn.ToString());
        fn.Dispose();

        return n;
    }

    public void FilterInput_DispodingEvent(object sender, System.EventArgs e) {
        UnRegisterFilterInputAndDispose();
    }

    public FilterCollection? GetInputFilter(Database? mustbeDatabase, bool doEmptyFilterToo) {
        if (Parents.Count == 0) {
            if (doEmptyFilterToo && mustbeDatabase != null) {
                return new FilterCollection(mustbeDatabase, "Empty Input Filter");
            }
            return null;
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
            if (!thiss.IsDisposed && thiss.FilterOutput is FilterCollection fi) {
                if (mustbeDatabase != null && fi.Database != mustbeDatabase) {
                    fc?.Dispose();
                    return new FilterCollection(new FilterItem(mustbeDatabase, "Datenbanken inkonsistent 2"), "Datenbanken inkonsistent");
                }

                fc ??= new FilterCollection(fi.Database, "filterofsender");

                foreach (var thifi in fi) {
                    if (thifi.Clone() is FilterItem fic) {
                        fc.AddIfNotExists(fic);
                    }
                }
            }
        }

        return fc;
    }

    public virtual void HandleChangesNow() {
        if (IsDisposed) {
            DatabaseInput = null;
        } else if (RowsInput != null && RowsInput.Count > 0) {
            DatabaseInput = RowsInput[0].Database;
        } else if (FilterInput is FilterCollection fc) {
            DatabaseInput = fc.Database;
        } else {
            DatabaseInput = null;
        }
    }

    /// <summary>
    /// Verwirft den aktuellen InputFilter.
    /// </summary>
    public virtual void Invalidate_FilterInput() {
        if (IsDisposed) { return; }
        FilterInputChangedHandled = false;
        Invalidate_RowsInput();
        Invalidate();
    }

    public virtual void Invalidate_RowsInput() {
        if (IsDisposed) { return; }
        if (!RowsInputManualSeted) { RowsInput = null; }
        RowsInputChangedHandled = false;
        Invalidate();
    }

    public RowItem? RowSingleOrNull() {
        if (IsDisposed) { return null; }
        if (RowsInput == null || RowsInput.Count != 1) { return null; }
        return RowsInput[0];
    }

    public void SetToRow(RowItem? row) {
        if (IsDisposed) { return; }
        if (Parents.Count > 0) {
            Develop.DebugPrint(FehlerArt.Fehler, "Element wird von Parents gesteuert!");
        }

        var doAtabaseAfter = _databaseInput == null;

        if (row == RowSingleOrNull()) { return; }

        RowsInputManualSeted = true;

        RowsInput = [];

        if (row?.Database is Database db && !db.IsDisposed) {
            RowsInput.Add(row);
            row.CheckRowDataIfNeeded();

            if (doAtabaseAfter) { RegisterEvents(); }
        }

        Invalidate_RowsInput();
    }

    protected virtual void DatabaseInput_CellValueChanged(object sender, CellChangedEventArgs e) { }

    protected virtual void DatabaseInput_ColumnPropertyChanged(object sender, ColumnEventArgs e) { }

    protected virtual void DatabaseInput_Disposed(object sender, System.EventArgs e) { }

    protected virtual void DatabaseInput_Loaded(object sender, System.EventArgs e) { }

    protected virtual void DatabaseInput_RowChecked(object sender, RowCheckedEventArgs e) { }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            UnRegisterFilterInputAndDispose();
            Invalidate_RowsInput();
            Invalidate_FilterInput();
            DisconnectChildParents([.. Parents]);
        }

        base.Dispose(disposing);
    }

    protected void DoRows() {
        if (RowsInputChangedHandled) { return; }

        RowsInputChangedHandled = true;

        if (RowsInputManualSeted) { return; }

        if (!FilterInputChangedHandled) { Develop.DebugPrint(FehlerArt.Fehler, "Filter unbehandelt!"); }

        if (FilterInput == null) {
            RowsInput = new List<RowItem>();
            return;
        }

        RowsInput = [.. FilterInput.Rows];

        if (RowSingleOrNull() is RowItem r) {
            r.CheckRowDataIfNeeded();
        }
    }

    protected override void DrawControl(Graphics gr, States state) {
        if (IsDisposed) { return; }
        HandleChangesNow();
    }

    protected override void OnCreateControl() {
        base.OnCreateControl();
        RegisterEvents();
    }

    private void FilterInput_RowsChanged(object sender, System.EventArgs e) {
        Invalidate_RowsInput();
    }

    private void RegisterEvents() {
        if (FilterInput == null || FilterInput.IsDisposed) { return; }
        FilterInput.RowsChanged += FilterInput_RowsChanged;
        FilterInput.DisposingEvent += FilterInput_DispodingEvent;
    }

    private void UnRegisterFilterInputAndDispose() {
        if (FilterInput == null || FilterInput.IsDisposed) { return; }
        FilterInput.RowsChanged -= FilterInput_RowsChanged;
        FilterInput.DisposingEvent -= FilterInput_DispodingEvent;

        if (Parents.Count > 1 && FilterInput != null && FilterInputChangedHandled) {
            FilterInput.Database = null;
            FilterInput.Dispose();
        }
    }

    #endregion
}
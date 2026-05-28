// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueTable.EventArgs;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Threading;
using static BlueBasics.ClassesStatic.Constants;
using static BlueBasics.ClassesStatic.Converter;

namespace BlueTable.Classes;

public sealed class ColumnViewItem : IParseable, IReadableText, IDisposableExtended, INotifyPropertyChanged, IHasTable {

    #region Fields

    private volatile int _isDisposedFlag;

    #endregion

    #region Constructors

    public ColumnViewItem(ColumnItem column) : this(column.Table) {
        Column = column;
        ViewType = ViewType.Column;
        Renderer = string.Empty;
    }

    public ColumnViewItem(Table? table, string toParse) : this(table) => this.Parse(toParse);

    private ColumnViewItem(Table? parent) : base() {
        Table = parent;
        ViewType = ViewType.None;
        Column = null;
        IsExpanded = true;
        Renderer = string.Empty;
        RendererSettings = string.Empty;
    }

    #endregion

    #region Events

    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion

    #region Properties

    public bool AutoFilterSymbolPossible => Column?.AutoFilterSymbolPossible() ?? false;

    public Color BackColor_ColumnCell {
        get => Column != null && field.IsMagentaOrTransparent() ? Column.BackColor : field;
        set {
            if (field.ToArgb() == value.ToArgb()) { return; }
            field = value;
            OnPropertyChanged();
        }
    } = Color.Transparent;

    public Color BackColor_ColumnHead {
        get => Column != null && field.IsMagentaOrTransparent()
                ? Column.BackColor.MixColor(Color.LightGray, 0.6)
                : field;

        set {
            if (field.ToArgb() == value.ToArgb()) { return; }
            field = value;
            OnPropertyChanged();
        }
    } = Color.Transparent;

    public ColumnBackgroundStyle BackgroundStyle => Column?.BackgroundStyle ?? ColumnBackgroundStyle.None;

    public string Caption => IsDummyColumn ? "Neue Spalte" : Column?.Caption ?? "[Spalte]";

    public string CaptionGroup1 => Column?.CaptionGroup1 ?? string.Empty;

    public string CaptionGroup2 => Column?.CaptionGroup2 ?? string.Empty;

    public string CaptionGroup3 => Column?.CaptionGroup3 ?? string.Empty;

    public ColumnItem? Column {
        get;
        private set {
            if (field == value) { return; }

            UnRegisterEvents();
            field = value;
            RegisterEvents();
            OnPropertyChanged();
        }
    }

    public Color FontColor_Caption {
        get => Column != null && field.IsMagentaOrTransparent() ? Column.ForeColor : field;
        set {
            if (field.ToArgb() == value.ToArgb()) { return; }
            field = value;
            OnPropertyChanged();
        }
    } = Color.Transparent;

    public bool Horizontal {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsDisposed => _isDisposedFlag == 1;

    public bool IsDummyColumn => ViewType == ViewType.DummyColumn;

    public bool IsExpanded {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    public ColumnLineStyle LineLeft => Column?.LineStyleLeft ?? ColumnLineStyle.Dünn;

    public ColumnLineStyle LineRight => Column?.LineStyleRight ?? ColumnLineStyle.Ohne;

    public bool Permanent {
        get => ViewType == ViewType.PermanentColumn;
        set => ViewType = value ? ViewType.PermanentColumn : ViewType.Column;
    }

    public string Renderer {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    public string RendererSettings {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    [DefaultValue(Win11)]
    public string SheetStyle {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    } = Win11;

    public Table? Table { get; }

    public int? TmpIfFilterRemoved { get; set; }

    public ViewType ViewType {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    } = ViewType.None;

    #endregion

    #region Methods

    public static ColumnViewItem CreateDummy() => new((Table?)null) {
        ViewType = ViewType.DummyColumn,
        IsExpanded = true
    };

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void InvalidateLayout() => OnPropertyChanged(nameof(Column));

    public List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        if (IsDummyColumn) { return []; }
        List<string> result = [];
        result.ParseableAdd("Type", ViewType);
        result.ParseableAdd("ColumnName", Column);

        if (Column is not { IsDisposed: false } c || c.DefaultRenderer != Renderer || c.RendererSettings != RendererSettings) {
            result.ParseableAdd("Renderer", Renderer);
            result.ParseableAdd("RendererSettings", RendererSettings);
        }

        if (Column is not { IsDisposed: false } c2 || c2.BackColor.ToArgb != BackColor_ColumnHead.ToArgb || !BackColor_ColumnCell.IsMagentaOrTransparent()) {
            result.ParseableAdd("BackColorColumnHead", BackColor_ColumnHead);
            result.ParseableAdd("BackColorColumnCell", BackColor_ColumnCell);
        }

        if (Column is not { IsDisposed: false } c3 || c3.ForeColor.ToArgb != FontColor_Caption.ToArgb || !FontColor_Caption.IsMagentaOrTransparent()) {
            result.ParseableAdd("FontColorCaption", FontColor_Caption);
        }

        result.ParseableAdd("FontHorizontal", Horizontal);

        return result;
    }

    public void ParseFinished(string parsed) { }

    public bool ParseThis(string key, string value) {
        if (Table is not { IsDisposed: false } tb) {
            Develop.DebugError("Tabelle unbekannt");
            return false;
        }

        switch (key) {
            case "column":
            case "columnname":
                Column = tb.Column[value];
                return true;

            case "columnkey":
                return true;

            case "type":
                ViewType = (ViewType)IntParse(value);
                if (ViewType == ViewType.DummyColumn) { return true; }
                if (Column != null && ViewType == ViewType.None) { ViewType = ViewType.Column; }
                return true;

            case "renderer":
                Renderer = value;
                return true;

            case "renderersettings":
                RendererSettings = value.FromNonCritical();
                return true;

            case "backcolorcolumnhead":
                BackColor_ColumnHead = ColorParse(value);
                return true;

            case "backcolorcolumncell":
                BackColor_ColumnCell = ColorParse(value);
                return true;

            case "fontcolorcaption":
                FontColor_Caption = ColorParse(value);
                return true;

            case "fonthorizontal":
                Horizontal = value.FromPlusMinus();
                return true;
        }

        return false;
    }

    public string ReadableText() => IsDummyColumn ? "Neue Spalte" : Column?.ReadableText() ?? "?";

    public QuickImage? SymbolForReadableText() => Column?.SymbolForReadableText();

    public override string ToString() => ParseableItems().FinishParseable();

    private void _column_PropertyChanged(object? sender, PropertyChangedEventArgs e) => OnPropertyChanged(nameof(Column));

    private void Cell_CellValueChanged(object? sender, CellEventArgs e) {
        if (e.Column == Column) { InvalidateLayout(); }
    }

    private void Dispose(bool disposing) {
        if (Interlocked.CompareExchange(ref _isDisposedFlag, 1, 0) != 0) { return; }

        if (disposing) {
            PropertyChanged = null;
            Column = null;
        }
    }

    private void OnPropertyChanged([CallerMemberName] string propertyName = "unknown") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private void RegisterEvents() {
        if (Column != null) {
            Column.PropertyChanged += _column_PropertyChanged;

            if (Column.Table is { IsDisposed: false } tb) {
                tb.Cell.CellValueChanged += Cell_CellValueChanged;
            }
        }
    }

    private void UnRegisterEvents() {
        if (Column != null) {
            Column.PropertyChanged -= _column_PropertyChanged;
            if (Column.Table is { IsDisposed: false } tb) {
                tb.Cell.CellValueChanged -= Cell_CellValueChanged;
            }
        }
    }

    #endregion
}
// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueTable.EventArgs;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Threading;

namespace BlueTable.Classes;

public class ColumnViewItem : IParseable, IReadableText, IDisposableExtended, INotifyPropertyChanged, IHasTable, IErrorCheckable {

    #region Fields

    private volatile int _isDisposedFlag;

    #endregion

    #region Constructors

    public ColumnViewItem(ColumnItem column) : this(column.Table) {
        Column = column;
        ViewType = ViewType.Column;
        //Renderer = string.Empty;
    }

    public ColumnViewItem(Table? table, string toParse) : this(table) => this.Parse(toParse);

    protected ColumnViewItem(Table? parent) : base() {
        Table = parent;
        ViewType = ViewType.None;
        Column = null;
        IsExpanded = true;
        //Renderer = string.Empty;
        //RendererSettings = string.Empty;
    }

    #endregion

    #region Events

    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion

    #region Properties

    /// <summary>
    /// Horizontale Ausrichtung des Zellinhalts. Echte Spalten liefern die
    /// Ausrichtung ihres <see cref="ColumnItem" />; virtuelle Spalten
    /// standardmäßig links. Renderer wie <c>Button</c> ignorieren die
    /// Ausrichtung (sie zentrieren selbst).
    /// </summary>
    public virtual AlignmentHorizontal Align => Column?.Align ?? AlignmentHorizontal.Links;

    public bool AutoFilterSymbolPossible => Column?.AutoFilterSymbolPossible() ?? false;

    public Color BackColor_ColumnCell => Column?.BackColor ?? Color.White;

    public virtual string Caption => Column?.Caption ?? "[Spalte]";

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

    public string? ColumnName => Column?.KeyName ?? StorageKey;

    /// <summary>
    /// Feste Canvas-Breite der Spalte. Echte Spalten (0) berechnen ihre Breite
    /// aus dem Inhalt; virtuelle Spalten liefern hier ihre konstante Breite.
    /// </summary>
    public virtual int FixedWidth => 0;

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
    } = true;

    public ColumnLineStyle LineLeft => Column?.LineStyleLeft ?? ColumnLineStyle.Dünn;

    public ColumnLineStyle LineRight => Column?.LineStyleRight ?? ColumnLineStyle.Ohne;

    public bool Permanent {
        get => ViewType == ViewType.PermanentColumn;
        set => ViewType = value ? ViewType.PermanentColumn : ViewType.Column;
    }

    /// <summary>
    /// Name des Renderers (ClassId, z. B. "Button" oder "TextOneLine").
    /// Echte Spalten liefern den Namen ihres
    /// <see cref="ColumnItem.DefaultRenderer" />; virtuelle Spalten
    /// überschreiben dies mit ihrem festen Renderer-Namen. Wird für die
    /// Serialisierung standardisiert gespeichert.
    /// </summary>
    public virtual string? Renderer => Column?.DefaultRenderer;

    /// <summary>
    /// Serialisierte Renderer-Einstellungen (z. B.
    /// <c>{ClassId="Button",ShowPic=+}</c>). Echte Spalten liefern die
    /// Einstellungen ihres <see cref="ColumnItem.RendererSettings" />;
    /// virtuelle Spalten überschreiben dies mit ihren festen Einstellungen.
    /// </summary>
    public virtual string RendererSettings => Column?.RendererSettings ?? string.Empty;

    /// <summary>
    /// Schlüssel, unter dem diese Spalte in der ColumnViewCollection
    /// serialisiert wird. Echte Spalten (und on-demand virtuelle Spalten)
    /// liefern null und werden nicht über einen eigenen Schlüssel gespeichert.
    /// Persistente virtuelle Spalten liefern ihren VIR_-Schlüssel
    /// (z. B. "VIR_PIN"); ihre bloße Anwesenheit in der Collection bedeutet
    /// "sichtbar".
    /// </summary>
    public virtual string? StorageKey => null;

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

    /// <summary>
    /// Erzeugt eine Spalte aus ihrer serialisierten Form. Erkennt virtuelle
    /// Spalten am <c>ColumnName</c>-Tag (VIR_-Schlüssel) und legt den
    /// korrekten Typ über <see cref="CreateVirtual" /> an. Alle anderen
    /// Einträge werden als echte Spalte (<see cref="ColumnItem" />) geladen.
    /// </summary>
    public static ColumnViewItem CreateFromParsed(Table? table, string toParse) {
        if (toParse.GetAllTags() is { } tags) {
            foreach (var t in tags) {
                if (t.Key == "columnname" && CreateVirtual(t.Value) is { } virtualItem) {
                    virtualItem.Parse(toParse);
                    return virtualItem;
                }
            }
        }

        return new ColumnViewItem(table, toParse);
    }

    /// <summary>
    /// Erzeugt eine persistente virtuelle Spalte anhand ihres
    /// <see cref="StorageKey" /> (z. B. "VIR_PIN"). Liefert null, wenn der
    /// Schlüssel keiner virtuellen Spalte entspricht. Die Konstruktoren der
    /// virtuellen Spalten sind intern; diese Fabrik ist der öffentliche
    /// Einstiegspunkt für andere Assemblies.
    /// </summary>
    public static ColumnViewItem? CreateVirtual(string storageKey) => storageKey.ToUpperInvariant() switch {
        "VIR_PIN" => new PinColumnItem(),
        "VIR_NUMBER" => new NumberColumnItem(),
        "VIR_ADD" => new AddColumnItem(),
        _ => null
    };

    /// <summary>
    /// Liefert den darzustellenden Wert dieser Spalte für die angegebene
    /// Zeile. Echte Spalten delegieren an <see cref="RowItem.CellGetString(ColumnItem)" />;
    /// virtuelle Spalten überschreiben dies, um ihren Wert zu berechnen
    /// (z. B. Pin-Bildcode, Zeilennummer). Beim Zeichnen ist dies der
    /// einzige Unterschied zwischen echten und virtuellen Spalten.
    /// </summary>
    public virtual string CellGetString(RowItem? row, bool isPinned) {
        if (row is null || Column is not { IsDisposed: false }) { return string.Empty; }
        return row.CellGetString(Column);
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public string ErrorReason() {
        if (IsDisposed) { return "Objekt verworfen"; }

        if (Column is not { IsDisposed: false } && string.IsNullOrEmpty(StorageKey)) { return "Keine Spalte zugeordnet."; }
        return string.Empty;
    }

    public void InvalidateLayout() => OnPropertyChanged(nameof(Column));

    public List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [];
        result.ParseableAdd("Type", ViewType);
        result.ParseableAdd("ColumnName", ColumnName);
        result.ParseableAdd("FontHorizontal", Horizontal);

        return result;
    }

    public void ParseFinished(string parsed) {
        if (Column is not null && ViewType == ViewType.None) { ViewType = ViewType.Column; }
    }

    public bool ParseThis(string key, string value) {
        switch (key) {
            case "column":
            case "columnname":
                // Virtuelle Spalten haben kein echtes ColumnItem — der
                // ColumnName ist ihr VIR_-Schlüssel, keine Spalten-Suche.
                if (StorageKey is not null) { return true; }

                if (Table is not { IsDisposed: false } tb) {
                    Develop.DebugError("Tabelle unbekannt");
                    return false;
                }

                Column = tb.Column[value];
                return true;

            case "columnkey":
                return true;

            case "type":
                ViewType = (ViewType)IntParse(value);
                return true;

            case "renderer":
                return true;

            case "renderersettings":
                return true;

            case "backcolorcolumnhead":
                return true;

            case "backcolorcolumncell":
                return true;

            case "fontcolorcaption":
                return true;

            case "fonthorizontal":
                Horizontal = value.FromPlusMinus();
                return true;
        }

        return false;
    }

    public string ReadableText() => Column?.ReadableText() ?? Caption;

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
        if (Column is not null) {
            Column.PropertyChanged += _column_PropertyChanged;

            if (Column.Table is { IsDisposed: false } tb) {
                tb.CellValueChanged += Cell_CellValueChanged;
            }
        }
    }

    private void UnRegisterEvents() {
        if (Column is not null) {
            Column.PropertyChanged -= _column_PropertyChanged;
            if (Column.Table is { IsDisposed: false } tb) {
                tb.CellValueChanged -= Cell_CellValueChanged;
            }
        }
    }

    #endregion
}
// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes.ItemCollectionList;
using BlueControls.Classes.ItemCollectionPad.Abstract;
using BlueControls.Controls;
using BlueTable.Interfaces;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Classes.ItemCollectionPad;

public class RowFormulaPadItem : FixedRectangleBitmapPadItem, IHasTable, IStyleable {

    #region Fields

    private string _lastQuickInfo = string.Empty;
    private string _rowKey;
    private Table? _table;
    private string _tableName = string.Empty;
    private bool _tableLoaded;
    private string _tmpQuickInfo = string.Empty;

    #endregion

    #region Constructors

    public RowFormulaPadItem() : this(string.Empty, null, string.Empty, string.Empty) { }

    public RowFormulaPadItem(Table table, string rowkey, string layoutId) : this(string.Empty, table, rowkey, layoutId) { }

    public RowFormulaPadItem(string keyName, Table? table, string rowkey, string layoutFileName) : base(keyName) {
        BeginInit();

        try {
            Table = table;
            _rowKey = rowkey;
            Layout_Dateiname = layoutFileName;
        } finally { EndInit(); }
    }

    #endregion

    #region Properties

    public static string ClassId => "ROW";

    public override string Description => string.Empty;

    /// <summary>
    /// Namen so lassen, wegen Kontextmenu
    /// </summary>
    public string Layout_Dateiname {
        get;
        set {
            if (value == field) { return; }
            field = value;
            RemovePic();
            OnPropertyChanged();
        }
    } = string.Empty;

    public override string QuickInfo {
        get {
            var r = Row;
            if (r is not { IsDisposed: false }) { return string.Empty; }
            var q = r.GetQuickInfo();
            if (_lastQuickInfo == q) { return _tmpQuickInfo; }
            _lastQuickInfo = q;
            _tmpQuickInfo = _lastQuickInfo.Replace(r.CellFirstString(), "<b>[<imagecode=Stern|16>" + r.CellFirstString() + "]</b>");
            return _tmpQuickInfo;
        }

        set {
            // Werte zurücksetzen
            _lastQuickInfo = string.Empty;
            _tmpQuickInfo = string.Empty;
        }
    }

    public RowItem? Row => Table?.Row.GetByKey(_rowKey);

    public string SheetStyle => Parent is IStyleable ist ? ist.SheetStyle : string.Empty;

    public Table? Table {
        get {
            if (_tableLoaded) { return _table; }

            _table?.Disposed -= _table_Disposed;

            if (string.IsNullOrEmpty(_tableName)) {
                _table = null;
            } else {
                _table = Table.Get(_tableName);
            }

            _table?.Disposed += _table_Disposed;
            _tableLoaded = true;

            return _table;
        }
        private set {
            if (IsDisposed || (value?.IsDisposed ?? true)) { value = null; }
            if (value == _table && _tableLoaded) { return; }

            _table?.Disposed -= _table_Disposed;
            _table = value;

            _tableName = value?.KeyName ?? string.Empty;
            _tableLoaded = true;

            _table?.Disposed += _table_Disposed;
            RemovePic();
            OnPropertyChanged();
        }
    }

    protected override int SaveOrder => 999;

    #endregion

    #region Methods

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<GenericControl> result = [];

        if (Row?.Table is { IsDisposed: false } tb) {
            var layouts = new List<AbstractListItem>();
            foreach (var thisLayouts in tb.GetAllLayoutsFileNames()) {
                var p = new ItemCollectionPadItem(thisLayouts);
                layouts.Add(ItemOf(p.Caption, p.KeyName, ImageCode.Stern));
            }
            result.Add(new FlexiControlForProperty<string>(() => Layout_Dateiname, layouts));
        }
        result.AddRange(base.GetProperties(widthOfControl));
        return result;
    }

    public override List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [.. base.ParseableItems()];
        result.ParseableAdd("LayoutFileName", Layout_Dateiname);
        result.ParseableAdd("Table", _tableName);
        if (!string.IsNullOrEmpty(_rowKey)) { result.ParseableAdd("RowKey", _rowKey); }
        if (Row is { IsDisposed: false } r) { result.ParseableAdd("FirstValue", r.CellFirstString()); }
        return result;
    }

    public override JsonObject ParseableJson() {
        var json = base.ParseableJson();
        json.Set("layoutfilename", Layout_Dateiname);
        if (!string.IsNullOrEmpty(_tableName)) { json.Set("table", _tableName); }
        if (!string.IsNullOrEmpty(_rowKey)) { json.Set("rowkey", _rowKey); }
        // FirstValue bewusst nicht serialisieren: abgeleiteter Zustand, keine Konfiguration.
        // ParseThis zeigt beim Laden zudem eine MessageBox, was beim Beta-Laden stört.
        return json;
    }

    public override void ParseJson(JsonObject json) {
        BeginInit();
        try {
            Layout_Dateiname = json.GetString("layoutfilename", Layout_Dateiname);
            var name = json.GetString("table", string.Empty);
            if (name is { Length: > 0 }) {
                _tableName = name;
                _tableLoaded = false;
            }
            _rowKey = json.GetString("rowkey", _rowKey);
            base.ParseJson(json);
        } finally {
            EndInit();
        }
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "layoutfilename":
            case "layoutid":
                Layout_Dateiname = value.FromNonCritical();
                return true;

            case "database":
            case "table":
                _tableName = value.FromNonCritical();
                _tableLoaded = false;
                return true;

            case "rowid": // TODO: alt
            case "rowkey":
                _rowKey = value;
                return true;

            case "firstvalue":
                var n = value.FromNonCritical();
                if (Row is { IsDisposed: false }) {
                    if (!string.Equals(Row.CellFirstString(), n, StringComparison.OrdinalIgnoreCase)) {
                        MessageBox.Show("<b><u>Eintrag hat sich geändert:</b></u><br><b>Von: </b> " + n + "<br><b>Nach: </b>" + Row.CellFirstString(), ImageCode.Information, "OK");
                    }
                    return true; // Alles beim Alten
                }

                if (Table?.Row[n] is { IsDisposed: false } rowtmp) {
                    _rowKey = rowtmp.KeyName;
                    MessageBox.Show("<b><u>Eintrag neu gefunden:</b></u><br>" + n, ImageCode.Warnung, "OK");
                } else {
                    MessageBox.Show("<b><u>Eintrag nicht hinzugefügt</b></u><br>" + n, ImageCode.Warnung, "OK");
                }
                return true; // Alles beim Alten
        }
        return base.ParseThis(key, value);
    }

    public override string ReadableText() => "Zeile";

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Zeile, 16);

    protected override void GeneratePic() {
        if (IsDisposed || string.IsNullOrEmpty(Layout_Dateiname) || Table is not { IsDisposed: false } tb) {
            GeneratedBitmap = QuickImage.Get(ImageCode.Warnung, 128);
            return;
        }

        var icp = new ItemCollectionPadItem(Layout_Dateiname);

        if (!icp.Any()) {
            GeneratedBitmap = QuickImage.Get(ImageCode.Warnung, 128);
            return;
        }

        icp.ResetVariables();
        icp.ReplaceVariables(tb, _rowKey);
        GeneratedBitmap = icp.ToBitmap(1);
    }

    private void _table_Disposed(object? sender, System.EventArgs e) {
        if (_table is not null) {
            _table.Disposed -= _table_Disposed;
            _table = null;
        }
        _tableLoaded = true;
        RemovePic();
        OnPropertyChanged();
    }

    #endregion
}
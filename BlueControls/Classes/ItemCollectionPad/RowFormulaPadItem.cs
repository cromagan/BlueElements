// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
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

using BlueBasics;
using BlueBasics.Classes;
using BlueBasics.Enums;
using BlueControls.Classes.ItemCollectionList;
using BlueControls.Classes.ItemCollectionPad.Abstract;
using BlueControls.Controls;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueTable.Classes;
using BlueTable.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Classes.ItemCollectionPad;

public class RowFormulaPadItem : FixedRectangleBitmapPadItem, IHasTable, IStyleable {

    #region Fields

    private string _lastQuickInfo = string.Empty;
    private string _layoutFileName;
    private string _rowKey;
    private string _tmpQuickInfo = string.Empty;

    #endregion

    #region Constructors

    public RowFormulaPadItem() : this(string.Empty, null, string.Empty, string.Empty) { }

    public RowFormulaPadItem(Table table, string rowkey, string layoutId) : this(string.Empty, table, rowkey, layoutId) { }

    public RowFormulaPadItem(string keyName, Table? table, string rowkey, string layoutFileName) : base(keyName) {
        Table = table;
        _rowKey = rowkey;
        _layoutFileName = layoutFileName;
    }

    #endregion

    #region Properties

    public static string ClassId => "ROW";

    public override string Description => string.Empty;

    /// <summary>
    /// Namen so lassen, wegen Kontextmenu
    /// </summary>
    public string Layout_Dateiname {
        get => _layoutFileName;
        set {
            if (value == _layoutFileName) { return; }
            _layoutFileName = value;
            RemovePic();
        }
    }

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
        get;
        private set {
            if (IsDisposed || (value?.IsDisposed ?? true)) { value = null; }
            if (value == field) { return; }

            if (field != null) {
                field.DisposingEvent -= _table_Disposing;
            }
            field = value;

            if (field != null) {
                field.DisposingEvent += _table_Disposing;
            }
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
        result.ParseableAdd("LayoutFileName", _layoutFileName);
        result.ParseableAdd("Table", Table);
        if (!string.IsNullOrEmpty(_rowKey)) { result.ParseableAdd("RowKey", _rowKey); }
        if (Row is { IsDisposed: false } r) { result.ParseableAdd("FirstValue", r.CellFirstString()); }
        return result;
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "layoutfilename":
            case "layoutid":
                _layoutFileName = value.FromNonCritical();
                return true;

            case "database":
            case "table":
                Table = Table.Get(value.FromNonCritical(), TableView.Table_NeedPassword);
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
        if (IsDisposed || string.IsNullOrEmpty(_layoutFileName) || Table is not { IsDisposed: false } tb) {
            GeneratedBitmap = QuickImage.Get(ImageCode.Warnung, 128);
            return;
        }

        var icp = new ItemCollectionPadItem(_layoutFileName);

        if (!icp.Any()) {
            GeneratedBitmap = QuickImage.Get(ImageCode.Warnung, 128);
            return;
        }

        icp.ResetVariables();
        icp.ReplaceVariables(tb, _rowKey);
        GeneratedBitmap = icp.ToBitmap(1);
    }

    private void _table_Disposing(object sender, System.EventArgs e) {
        Table = null;
        RemovePic();
    }

    #endregion
}
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

using System;
using System.Collections.Generic;
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionList;
using BlueControls.ItemCollectionPad.Abstract;
using BlueDatabase;
using BlueDatabase.Interfaces;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.ItemCollectionPad;

public class RowFormulaPadItem : FixedRectangleBitmapPadItem, IHasDatabase, IStyleable {

    #region Fields

    private Database? _database;
    private string _lastQuickInfo = string.Empty;
    private string _layoutFileName;
    private string _rowKey;
    private string _tmpQuickInfo = string.Empty;

    #endregion

    #region Constructors

    public RowFormulaPadItem() : this(string.Empty, null, string.Empty, string.Empty) { }

    public RowFormulaPadItem(Database database, string rowkey, string layoutId) : this(string.Empty, database, rowkey, layoutId) { }

    public RowFormulaPadItem(string keyName, Database? database, string rowkey, string layoutFileName) : base(keyName) {
        Database = database;
        _rowKey = rowkey;
        _layoutFileName = layoutFileName;
    }

    #endregion

    #region Events

    public event EventHandler? StyleChanged;

    #endregion

    #region Properties

    public static string ClassId => "ROW";

    public Database? Database {
        get => _database;
        private set {
            if (IsDisposed || (value?.IsDisposed ?? true)) { value = null; }
            if (value == _database) { return; }

            if (_database != null) {
                _database.DisposingEvent -= _database_Disposing;
            }
            _database = value;

            if (_database != null) {
                _database.DisposingEvent += _database_Disposing;
            }
        }
    }

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
            if (_lastQuickInfo == r.QuickInfo) { return _tmpQuickInfo; }
            _lastQuickInfo = r.QuickInfo;
            _tmpQuickInfo = _lastQuickInfo.Replace(r.CellFirstString(), "<b>[<imagecode=Stern|16>" + r.CellFirstString() + "]</b>");
            return _tmpQuickInfo;
        }
        // ReSharper disable once ValueParameterNotUsed
        set {
            // Werte zurücksetzen
            _lastQuickInfo = string.Empty;
            _tmpQuickInfo = string.Empty;
        }
    }

    public RowItem? Row => Database?.Row.SearchByKey(_rowKey);

    public string SheetStyle {
        get {
            if (Parent is IStyleable ist) { return ist.SheetStyle; }
            return string.Empty;
        }
    }

    protected override int SaveOrder => 999;

    #endregion

    #region Methods

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<GenericControl> result = [];

        if (Row?.Database is { IsDisposed: false } db) {
            var layouts = new List<AbstractListItem>();
            foreach (var thisLayouts in db.GetAllLayoutsFileNames()) {
                ItemCollectionPadItem p = new(thisLayouts);
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
        result.ParseableAdd("Database", Database);
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
                Database = Database.GetById(new ConnectionInfo(value.FromNonCritical(), null, string.Empty), false, null, true);
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

                if (Database?.Row[n] is { IsDisposed: false } rowtmp) {
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
        if (IsDisposed || string.IsNullOrEmpty(_layoutFileName) || Database is not { IsDisposed: false } db) {
            GeneratedBitmap = QuickImage.Get(ImageCode.Warnung, 128);
            return;
        }

        var icp = new ItemCollectionPadItem(_layoutFileName);
        icp.ResetVariables();
        icp.ReplaceVariables(db, _rowKey);
        GeneratedBitmap = icp.ToBitmap(1);
    }

    private void _database_Disposing(object sender, System.EventArgs e) {
        Database = null;
        RemovePic();
    }

    #endregion
}
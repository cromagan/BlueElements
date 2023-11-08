// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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
using System.Drawing;
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Forms;
using BlueControls.ItemCollectionPad.Abstract;
using BlueDatabase;
using BlueDatabase.Interfaces;

namespace BlueControls.ItemCollectionPad;

public class RowFormulaPadItem : FixedRectangleBitmapPadItem, IHasDatabase {

    #region Fields

    private string _lastQuickInfo = string.Empty;
    private string _layoutFileName = string.Empty;
    private string _rowKey;
    private string _tmpQuickInfo = string.Empty;

    #endregion

    #region Constructors

    public RowFormulaPadItem() : this(string.Empty, null, string.Empty, string.Empty) { }

    public RowFormulaPadItem(string internalname) : this(internalname, null, string.Empty, string.Empty) { }

    public RowFormulaPadItem(DatabaseAbstract database, string rowkey) : this(database, rowkey, string.Empty) { }

    public RowFormulaPadItem(DatabaseAbstract database, string rowkey, string layoutId) : this(string.Empty, database, rowkey, layoutId) { }

    public RowFormulaPadItem(string internalname, DatabaseAbstract? database, string rowkey, string layoutFileName) : base(internalname) {
        Database = database;
        if (Database != null) { Database.Disposing += _Database_Disposing; }
        _rowKey = rowkey ?? string.Empty;
        _layoutFileName = layoutFileName;
    }

    #endregion

    #region Properties

    public static string ClassId => "ROW";
    public DatabaseAbstract? Database { get; private set; }
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
            if (r == null || r.IsDisposed) { return string.Empty; }
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

    protected override int SaveOrder => 999;

    #endregion

    #region Methods

    public override List<GenericControl> GetStyleOptions(int widthOfControl) {
        List<GenericControl> l = new();

        if (Row?.Database is DatabaseAbstract dbl) {
            ItemCollectionList.ItemCollectionList layouts = new(true);
            foreach (var thisLayouts in dbl.GetAllLayouts()) {
                ItemCollectionPad p = new(thisLayouts, string.Empty);
                _ = layouts.Add(p.Caption, p.KeyName, ImageCode.Stern);
            }
            l.Add(new FlexiControlForProperty<string>(() => Layout_Dateiname, layouts));
        }
        l.AddRange(base.GetStyleOptions(widthOfControl));
        return l;
    }

    public override bool ParseThis(string tag, string value) {
        if (base.ParseThis(tag, value)) { return true; }
        switch (tag) {
            case "layoutfilename":
            case "layoutid":
                _layoutFileName = value.FromNonCritical();
                return true;

            case "database":
                Database = DatabaseAbstract.GetById(new ConnectionInfo(value.FromNonCritical(), null, string.Empty), false, null, true);
                Database.Disposing += _Database_Disposing;
                return true;

            case "rowid": // TODO: alt
            case "rowkey":
                _rowKey = value;
                return true;

            case "firstvalue":
                var n = value.FromNonCritical();
                if (Row != null && !Row.IsDisposed) {
                    if (!string.Equals(Row.CellFirstString(), n, StringComparison.OrdinalIgnoreCase)) {
                        MessageBox.Show("<b><u>Eintrag hat sich geändert:</b></u><br><b>Von: </b> " + n + "<br><b>Nach: </b>" + Row.CellFirstString(), ImageCode.Information, "OK");
                    }
                    return true; // Alles beim Alten
                }
                var rowtmp = Database.Row[n];
                if (rowtmp == null) {
                    MessageBox.Show("<b><u>Eintrag nicht hinzugefügt</b></u><br>" + n, ImageCode.Warnung, "OK");
                } else {
                    _rowKey = rowtmp.KeyName;
                    MessageBox.Show("<b><u>Eintrag neu gefunden:</b></u><br>" + n, ImageCode.Warnung, "OK");
                }
                return true; // Alles beim Alten
        }
        return false;
    }

    public override void ProcessStyleChange() => RemovePic();

    public override string ToString() {
        if (IsDisposed) { return string.Empty; }
        List<string> result = new();
        result.ParseableAdd("LayoutFileName", _layoutFileName);
        result.ParseableAdd("Database", Database);
        if (!string.IsNullOrEmpty(_rowKey)) { result.ParseableAdd("RowKey", _rowKey); }
        if (Row is RowItem r) { result.ParseableAdd("FirstValue", r.CellFirstString()); }
        return result.Parseable(base.ToString());
    }

    protected override void GeneratePic() {
        if (string.IsNullOrEmpty(_layoutFileName) || Database is not DatabaseAbstract db || db.IsDisposed) {
            GeneratedBitmap = QuickImage.Get(ImageCode.Warnung, 128);
            return;
        }

        var lf = db.GetLayout(_layoutFileName);

        CreativePad pad = new(new ItemCollectionPad(lf));
        pad.Item.ResetVariables();
        pad.Item.ParseVariable(db, _rowKey);
        var re = pad.Item.MaxBounds(string.Empty);

        var generatedBitmap = new Bitmap((int)re.Width, (int)re.Height);

        var mb = pad.Item.MaxBounds(string.Empty);
        var zoomv = ItemCollectionPad.ZoomFitValue(mb, generatedBitmap.Size);
        var centerpos = ItemCollectionPad.CenterPos(mb, generatedBitmap.Size, zoomv);
        var slidervalues = ItemCollectionPad.SliderValues(mb, zoomv, centerpos);
        pad.ShowInPrintMode = true;
        pad.Unselect();
        if (Parent.SheetStyle != null) { pad.Item.SheetStyle = Parent.SheetStyle; }
        pad.Item.DrawCreativePadToBitmap(generatedBitmap, States.Standard, zoomv, slidervalues.X, slidervalues.Y, string.Empty);
        //if (sizeChangeAllowed) { p_RU.SetTo(p_LO.X + GeneratedBitmap.Width, p_LO.Y + GeneratedBitmap.Height); }
        //SizeChanged();
        GeneratedBitmap = generatedBitmap;
    }

    //protected override AbstractPadItem? TryCreate(string id, string name) {
    //    if (id.Equals(ClassId, StringComparison.OrdinalIgnoreCase)) {
    //        return new RowFormulaPadItem(name);
    //    }
    //    return null;
    //}

    private void _Database_Disposing(object sender, System.EventArgs e) {
        Database.Disposing -= _Database_Disposing;
        Database = null;
        RemovePic();
    }

    #endregion
}
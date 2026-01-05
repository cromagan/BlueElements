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
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.CellRenderer;
using BlueControls.Controls;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionList;
using BlueTable.Enums;
using BlueTable.EventArgs;
using BlueTable.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using static BlueBasics.Converter;

namespace BlueTable;

public sealed class ColumnViewItem : IParseable, IReadableText, IDisposableExtended, IStyleable, INotifyPropertyChanged, IHasTable {

    #region Fields

    private Color _backColor_ColumnCell = Color.Transparent;

    private Color _backColor_ColumnHead = Color.Transparent;

    private int? _canvasContentWidth;

    private ColumnItem? _column;

    /// <summary>
    /// Koordinaten OHNE ShiftX, aber skaliert auf Controlebene
    /// Es muss mit Control-Koordinaten gearbeitet werden, da verschiedene Zoom-Stufen anderen Spaltenbreiten haben können
    /// </summary>
    private int _controlColumnLeft;

    /// <summary>
    /// Control heißt, dass die Kooridanten sich auf die Controllebene beziehen und nicht auf den Canvas
    /// Es muss mit Control-Koordinaten gearbeitet werden, da verschiedene Zoom-Stufen anderen Spaltenbreiten haben können
    /// </summary>
    private int _controlColumnWidth;

    private Color _fontColor_Caption = Color.Transparent;

    private bool _horizontal;

    private Renderer_Abstract? _renderer;

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
        //AutoFilterLocation = Rectangle.Empty;
        //ReduceLocation = Rectangle.Empty;
        Invalidate_CanvasContentWidth();
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
        get => Column != null && _backColor_ColumnHead.IsMagentaOrTransparent() ? Column.BackColor : _backColor_ColumnCell;
        set {
            if (_backColor_ColumnCell.ToArgb() == value.ToArgb()) { return; }
            _backColor_ColumnCell = value;
            OnPropertyChanged();
        }
    }

    public Color BackColor_ColumnHead {
        get => Column != null && _backColor_ColumnHead.IsMagentaOrTransparent()
                ? Column.BackColor.MixColor(Color.LightGray, 0.6)
                : _backColor_ColumnHead;

        set {
            if (_backColor_ColumnHead.ToArgb() == value.ToArgb()) { return; }
            _backColor_ColumnHead = value;
            OnPropertyChanged();
        }
    }

    public ColumnBackgroundStyle BackgroundStyle => Column?.BackgroundStyle ?? ColumnBackgroundStyle.None;

    public int CanvasContentWidth {
        get {
            if (_canvasContentWidth is { } v) { return v; }

            _canvasContentWidth = CalculateCanvasContentWith(_column, GetRenderer(SheetStyle));
            return (int)_canvasContentWidth;
        }
    }

    public string Caption => Column?.Caption ?? "[Spalte]";
    public string CaptionGroup1 => Column?.CaptionGroup1 ?? string.Empty;

    public string CaptionGroup2 => Column?.CaptionGroup2 ?? string.Empty;

    public string CaptionGroup3 => Column?.CaptionGroup3 ?? string.Empty;

    public ColumnItem? Column {
        get => _column;
        private set {
            if (_column == value) { return; }

            UnRegisterEvents();
            _column = value;
            Invalidate_CanvasContentWidth();
            RegisterEvents();
            OnPropertyChanged();
        }
    }

    public int? ControlColumnWidth { get; private set; }

    public Color FontColor_Caption {
        get => Column != null && _fontColor_Caption.IsMagentaOrTransparent() ? Column.ForeColor : _fontColor_Caption;
        set {
            if (_fontColor_Caption.ToArgb() == value.ToArgb()) { return; }
            _fontColor_Caption = value;
            OnPropertyChanged();
        }
    }

    public bool Horizontal {
        get => Column != null && _horizontal;
        set {
            if (_horizontal = value) { return; }
            _horizontal = value;
            OnPropertyChanged();
        }
    }

    public bool IsDisposed { get; private set; }

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

    public string Renderer { get; set; }

    public string RendererSettings { get; set; }

    public string SheetStyle { get; set; } = Constants.Win11;

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

    public static int CalculateCanvasContentWith(ColumnItem? column, Renderer_Abstract renderer) {
        if (column is not { IsDisposed: false }) { return 16; }
        if (column.Table is not { IsDisposed: false } tb) { return 16; }
        if (column.FixedColumnWidth > 0) { return column.FixedColumnWidth; }

        var newContentWidth = 16; // Wert muss gesetzt werden, dass er am Ende auch gespeichert wird

        try {
            //  Parallel.ForEach führt ab und zu zu DeadLocks
            foreach (var thisRowItem in tb.Row) {
                var wx = renderer.ContentSize(thisRowItem.CellGetString(column), column.DoOpticalTranslation).Width;
                newContentWidth = Math.Max(newContentWidth, wx);
            }
        } catch {
            Develop.AbortAppIfStackOverflow();
            return CalculateCanvasContentWith(column, renderer);
        }

        return newContentWidth;
    }

    public bool CollapsableEnabled() => CanvasContentWidth > 40 || !IsExpanded;

    public int ControlColumnLeft(float offsetX) {
        if (Permanent) {
            return _controlColumnLeft;
        }

        return _controlColumnLeft + (int)offsetX;
    }

    public int ControlColumnRight(float offsetX) => ControlColumnLeft(offsetX) + ControlColumnWidth ?? 0;

    public void Dispose() =>
                // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
                Dispose(disposing: true);

    public void Dispose(bool disposing) {
        if (!IsDisposed) {
            if (disposing) {
                UnRegisterEvents();
                _column = null;
            }
            IsDisposed = true;
        }
    }

    //GC.SuppressFinalize(this);
    public Renderer_Abstract GetRenderer(string style) {
        if (_renderer != null) { return _renderer; }

        _renderer = TableView.RendererOf(this, style);
        return _renderer;
    }

    public void Invalidate_CanvasContentWidth() {
        _canvasContentWidth = null;
        OnPropertyChanged(nameof(CanvasContentWidth));
    }

    public List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [];
        result.ParseableAdd("Type", ViewType);
        result.ParseableAdd("ColumnName", _column);

        if (_column is not { IsDisposed: false } c || c.DefaultRenderer != Renderer || c.RendererSettings != RendererSettings) {
            result.ParseableAdd("Renderer", Renderer);
            result.ParseableAdd("RendererSettings", RendererSettings);
        }

        if (_column is not { IsDisposed: false } c2 || c2.BackColor.ToArgb != _backColor_ColumnHead.ToArgb || !_backColor_ColumnCell.IsMagentaOrTransparent()) {
            result.ParseableAdd("BackColorColumnHead", _backColor_ColumnHead);
            result.ParseableAdd("BackColorColumnCell", _backColor_ColumnCell);
        }

        if (_column is not { IsDisposed: false } c3 || c3.ForeColor.ToArgb != _fontColor_Caption.ToArgb || !_fontColor_Caption.IsMagentaOrTransparent()) {
            result.ParseableAdd("FontColorCaption", _fontColor_Caption);
        }

        result.ParseableAdd("FontHorizontal", _horizontal);

        return result;
    }

    public void ParseFinished(string parsed) {
    }

    public bool ParseThis(string key, string value) {
        if (Table is not { IsDisposed: false } tb) {
            Develop.DebugPrint(ErrorType.Error, "Tabelle unbekannt");
            return false;
        }

        switch (key) {
            case "column":
            case "columnname":// ColumnName wichtg, wegen CopyLayout
                Column = tb.Column[value];
                return true;

            case "columnkey":
                return true;

            case "type":
                ViewType = (ViewType)IntParse(value);
                if (_column != null && ViewType == ViewType.None) { ViewType = ViewType.Column; }
                return true;

            //case "edittype":
            //    //    _editType = (EditTypeFormula)IntParse(value);
            //    return true;

            case "renderer":
                Renderer = value;
                return true;

            case "renderersettings":
                RendererSettings = value.FromNonCritical();
                return true;

            case "backcolorcolumnhead":
                _backColor_ColumnHead = ColorParse(value);
                return true;

            case "backcolorcolumncell":
                _backColor_ColumnCell = ColorParse(value);
                return true;

            case "fontcolorcaption":
                _fontColor_Caption = ColorParse(value);
                return true;

            case "fonthorizontal":
                _horizontal = value.FromPlusMinus();
                return true;
        }

        return false;
    }

    public string ReadableText() => _column?.ReadableText() ?? "?";

    public QuickImage? SymbolForReadableText() => _column?.SymbolForReadableText();

    //    return new Rectangle(r.Right - size, r.Top, size, size);
    //}
    public override string ToString() => ParseableItems().FinishParseable();

    internal void ComputeLocation(ColumnViewCollection parent, int x, int tableviewWith, float zoom) {
        if (Column == null) { return; }

        _controlColumnLeft = x;

        ControlColumnWidth = ComputeControlColumnWidth(parent, tableviewWith, zoom);
    }

    //    //if (!string.IsNullOrEmpty(CaptionGroup3)) {
    //    //    moveDown += pcch * 3;
    //    //} else if (!string.IsNullOrEmpty(CaptionGroup2)) {
    //    //    moveDown += pcch * 2;
    //    //} else if (!string.IsNullOrEmpty(CaptionGroup1)) {
    //    //    moveDown += pcch;
    //    //}
    private void _column_PropertyChanged(object sender, PropertyChangedEventArgs e) => OnPropertyChanged(nameof(Column));

    private void Cell_CellValueChanged(object sender, CellEventArgs e) {
        if (e.Column == _column) { Invalidate_CanvasContentWidth(); }
    }

    /// <summary>
    /// Control heißt, dass die Kooridanten sich auf die Controllebene beziehen und nicht auf den Canvas
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="tableviewWith"></param>
    /// <param name="zoom"></param>
    /// <returns></returns>
    private int ComputeControlColumnWidth(ColumnViewCollection parent, int tableviewWith, float zoom) {
        // Hier wird die ORIGINAL-Spalte gezeichnet, nicht die FremdZelle!!!!

        var p16 = 16.CanvasToControl(zoom);
        var pa = 8.CanvasToControl(zoom);

        if (parent == null) { return p16; }

        if (_column is not { IsDisposed: false }) {
            _controlColumnWidth = p16;
            return _controlColumnWidth;
        }

        if (parent.Count == 1) {
            _controlColumnWidth = tableviewWith;
            return _controlColumnWidth;
        }

        var minw = (p16 * (_column.Caption.CountString("\r") + 1)) + pa;

        if (!IsExpanded) {
            _controlColumnWidth = minw;
        } else {
            _controlColumnWidth = ViewType == ViewType.PermanentColumn
                ? Math.Min(CanvasContentWidth.CanvasToControl(zoom) + pa, (int)(tableviewWith * 0.3))
                : Math.Min(CanvasContentWidth.CanvasToControl(zoom) + pa, (int)(tableviewWith * 0.6));
        }

        _controlColumnWidth = Math.Max(_controlColumnWidth, FilterBarListItem.AutoFilterSize.CanvasToControl(zoom)); // Mindestens so groß wie der Autofilter;
        _controlColumnWidth = Math.Max(_controlColumnWidth, minw);
        return _controlColumnWidth;
    }

    private void OnPropertyChanged([CallerMemberName] string propertyName = "unknown") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private void RegisterEvents() {
        if (_column != null) {
            _column.PropertyChanged += _column_PropertyChanged;

            if (_column.Table is { IsDisposed: false } tb) {
                tb.Cell.CellValueChanged += Cell_CellValueChanged;
            }
        }
    }

    private void UnRegisterEvents() {
        if (_column != null) {
            _column.PropertyChanged -= _column_PropertyChanged;
            if (_column.Table is { IsDisposed: false } tb) {
                tb.Cell.CellValueChanged -= Cell_CellValueChanged;
            }
        }
    }

    #endregion
}
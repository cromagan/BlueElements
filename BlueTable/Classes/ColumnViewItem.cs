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
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using BlueBasics;
using BlueBasics.Classes;
using BlueBasics.ClassesStatic;
using BlueBasics.Interfaces;
using BlueTable.Enums;
using BlueTable.EventArgs;
using BlueTable.Interfaces;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using static BlueBasics.ClassesStatic.Constants;
using static BlueBasics.ClassesStatic.Converter;

namespace BlueTable.Classes;

public sealed class ColumnViewItem : IParseable, IReadableText, IDisposableExtended, INotifyPropertyChanged, IHasTable {

    #region Fields

    private Color _backColor_ColumnCell = Color.Transparent;

    private Color _backColor_ColumnHead = Color.Transparent;

    private ColumnItem? _column;

    private Color _fontColor_Caption = Color.Transparent;

    private bool _horizontal;

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

    public bool AutoFilterSymbolPossible => Column?.AutoFilterSymbolPossible() ?? false;

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
            RegisterEvents();
            OnPropertyChanged();
        }
    }

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

    [DefaultValue(Win11)]
    public string SheetStyle { get; set; } = Win11;

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

    public void InvalidateLayout() => OnPropertyChanged(nameof(Column));

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
                if (_column != null && ViewType == ViewType.None) { ViewType = ViewType.Column; }
                return true;

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

    public override string ToString() => ParseableItems().FinishParseable();

    private void _column_PropertyChanged(object? sender, PropertyChangedEventArgs e) => OnPropertyChanged(nameof(Column));

    private void Cell_CellValueChanged(object? sender, CellEventArgs e) {
        if (e.Column == _column) { InvalidateLayout(); }
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
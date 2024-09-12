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
using BlueDatabase.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using BlueControls.CellRenderer;
using BlueControls.Controls;
using BlueControls.Enums;
using static BlueBasics.Constants;
using static BlueBasics.Converter;
using System.Runtime.InteropServices;

namespace BlueDatabase;

public sealed class ColumnViewItem : IParseable, IReadableText, IDisposable {

    #region Fields

    public Renderer_Abstract? _renderer;
    public int? Contentwidth;

    private ColumnItem? _column = null;

    private bool _reduced = false;

    private ViewType _viewType = BlueDatabase.Enums.ViewType.None;

    private bool disposedValue;

    #endregion

    #region Constructors

    public ColumnViewItem(ColumnItem column, ViewType type, ColumnViewCollection parent) : this(parent) {
        Column = column;
        ViewType = type;
        Renderer = string.Empty;
    }

    public ColumnViewItem(ColumnViewCollection parent, string toParse) : this(parent) => this.Parse(toParse);

    private ColumnViewItem(ColumnViewCollection parent) : base() {
        Parent = parent;
        ViewType = ViewType.None;
        Column = null;
        X_WithSlider = null;
        X = null;
        AutoFilterLocation = Rectangle.Empty;
        ReduceLocation = Rectangle.Empty;
        _drawWidth = null;
        Reduced = false;
        Renderer = string.Empty;
        RendererSettings = string.Empty;
    }

    #endregion

    #region Properties

    public Rectangle AutoFilterLocation { get; set; }

    public ColumnItem? Column {
        get => _column;
        private set {
            if (_column == value) { return; }

            UnRegisterEvents();
            _column = value;
            RegisterEvents();
        }
    }

    public bool Reduced {
        get => _reduced;
        set {
            if (_reduced != value) {
                _reduced = value;
                _drawWidth = null;
            }
        }
    }

    public Rectangle ReduceLocation { get; set; }
    public string Renderer { get; set; }

    public string RendererSettings { get; set; }

    public ViewType ViewType {
        get => _viewType;
        set {
            if (_viewType != value) {
                _viewType = value;
                _drawWidth = null;
            }
        }
    }

    /// <summary>
    /// Koordinate der Spalte ohne Slider
    /// </summary>
    public int? X { get; set; }

    /// <summary>
    /// Koordinate der Spalte mit einbrechneten Slider
    /// </summary>
    public int? X_WithSlider { get; set; }

    private int? _drawWidth { get; set; }
    private ColumnViewCollection Parent { get; set; }

    #endregion

    #region Methods

    public void Dispose() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public int DrawWidth(Rectangle displayRectangleWoSlider, float scale) {
        // Hier wird die ORIGINAL-Spalte gezeichnet, nicht die FremdZelle!!!!

        if (_drawWidth is { } v) { return v; }

        if (_column == null || _column.IsDisposed) {
            _drawWidth = Table.GetPix(16, scale);
            return (int)_drawWidth;
        }

        if (Parent.Count == 1) {
            _drawWidth = displayRectangleWoSlider.Width;
            return displayRectangleWoSlider.Width;
        }

        if (Reduced) {
            _drawWidth = Table.GetPix(16, scale);
        } else {
            _drawWidth = _viewType == ViewType.PermanentColumn
                ? Math.Min(Table.GetPix(CalculateColumnContentWidth(), scale), (int)(displayRectangleWoSlider.Width * 0.3))
                : Math.Min(Table.GetPix(CalculateColumnContentWidth(), scale), (int)(displayRectangleWoSlider.Width * 0.6));
        }

        _drawWidth = Math.Max((int)_drawWidth, AutoFilterSize); // Mindestens so groß wie der Autofilter;
        return (int)_drawWidth;
    }

    public Renderer_Abstract? GetRenderer() {
        if (_renderer != null) { return _renderer; }

        _renderer = Renderer_Abstract.RendererOf(this);
        return _renderer;
    }

    public void Invalidate_DrawWidth() => _drawWidth = null;

    public ColumnViewItem? NextVisible() => Parent.NextVisible(this);

    public void ParseFinished(string parsed) { }

    public bool ParseThis(string key, string value) {
        if (Parent.Database is not { IsDisposed: false } db) {
            Develop.DebugPrint(FehlerArt.Fehler, "Datenbank unbekannt");
            return false;
        }

        switch (key) {
            case "column":
            case "columnname":// ColumnName wichtg, wegen CopyLayout
                Column = db.Column[value];
                return true;

            case "columnkey":
                return true;

            //case "permanent": // Todo: Alten Code Entfernen, Permanent wird nicht mehr verstringt 06.09.2019
            //    ViewType = ViewType.PermanentColumn;
            //    return true;

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
        }

        return false;
    }

    public ColumnViewItem? PreviewsVisible() => Parent.PreviousVisible(this);

    public string ReadableText() => _column?.ReadableText() ?? "?";

    public QuickImage? SymbolForReadableText() => _column?.SymbolForReadableText();

    public string ToParseableString() {
        var result = new List<string>();
        result.ParseableAdd("Type", ViewType);
        result.ParseableAdd("ColumnName", _column);

        if (_column is not { } c || c.DefaultRenderer != Renderer || c.RendererSettings != RendererSettings) {
            result.ParseableAdd("Renderer", Renderer);
            result.ParseableAdd("RendererSettings", RendererSettings);
        }

        return result.Parseable();
    }

    public override string ToString() => ToParseableString();

    private void _column_PropertyChanged(object sender, System.EventArgs e) { _drawWidth = null; }

    private int CalculateColumnContentWidth() {
        if (_column is not { IsDisposed: false }) { return 16; }
        if (_column.Database is not { IsDisposed: false } db) { return 16; }
        if (_column.FixedColumnWidth > 0) { return _column.FixedColumnWidth; }
        if (Contentwidth is { } v) { return v; }

        _column.RefreshColumnsData();

        var newContentWidth = 16; // Wert muss gesetzt werden, dass er am Ende auch gespeichert wird

        var renderer = GetRenderer();

        if (renderer == null) { return 16; }

        try {
            //  Parallel.ForEach führt ab und zu zu DeadLocks
            foreach (var thisRowItem in db.Row) {
                var wx = renderer.ContentSize(thisRowItem.CellGetString(_column), Design.Table_Cell, States.Standard, _column.DoOpticalTranslation).Width;
                newContentWidth = Math.Max(newContentWidth, wx);
            }
        } catch {
            Develop.CheckStackForOverflow();
            return CalculateColumnContentWidth();
        }

        Contentwidth = newContentWidth;
        return newContentWidth;
    }

    private void Cell_CellValueChanged(object sender, EventArgs.CellChangedEventArgs e) {
        if (e.Column == _column) { _drawWidth = null; }
    }

    private void Dispose(bool disposing) {
        if (!disposedValue) {
            if (disposing) {
                // TODO: Verwalteten Zustand (verwaltete Objekte) bereinigen
                UnRegisterEvents();
                Parent = null;
                _column = null;
            }

            // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer überschreiben
            // TODO: Große Felder auf NULL setzen
            disposedValue = true;
        }
    }

    private void RegisterEvents() {
        if (_column != null) {
            _column.PropertyChanged += _column_PropertyChanged;

            if (_column.Database is Database db && !db.IsDisposed) {
                db.Cell.CellValueChanged += Cell_CellValueChanged;
            }
        }
    }

    private void UnRegisterEvents() {
        if (_column != null) {
            _column.PropertyChanged -= _column_PropertyChanged;
            if (_column.Database is Database db && !db.IsDisposed) {
                db.Cell.CellValueChanged += Cell_CellValueChanged;
            }
        }
    }

    #endregion

    // // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
    // ~ColumnViewItem()
    // {
    //     // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
    //     Dispose(disposing: false);
    // }
}
// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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
using BlueDatabase.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using static BlueBasics.Converter;


namespace BlueDatabase;

public sealed class RowColorRule : IParseable, IHasDatabase, IDisposableExtended, INotifyPropertyChanged, IReadableText, IEditable, IErrorCheckable {

    #region Fields

    private FilterCollection? _filter;
    private int _prio;
    private Color _rowColor;
    private string _caption = string.Empty;

    #endregion

    #region Constructors

    public RowColorRule() {
        _filter = new FilterCollection("RowColorRule");
        _prio = 0;
        _rowColor = Color.White;
        _caption = "Neue Regel";
    }

    public RowColorRule(FilterCollection filter, int prio, Color rowColor, string caption) {
        _filter = filter;
        _prio = prio;
        _rowColor = rowColor;
        _caption = caption;
        
        if (_filter != null) {
            _filter.PropertyChanged += Filter_PropertyChanged;
            _filter.DisposingEvent += Filter_DisposingEvent;
        }
    }

    public RowColorRule(string parseableString) {
        _filter = new FilterCollection("RowColorRule");
        _prio = 0;
        _rowColor = Color.White;
        _caption = string.Empty;
        
        this.Parse(parseableString);
    }

    #endregion

    #region Destructors

    ~RowColorRule() { Dispose(disposing: false); }

    #endregion

    #region Events

    public event EventHandler? DisposingEvent;
    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion

    #region Properties

    public string CaptionForEditor => "Zeilen-Farb-Regel";

    public string Caption {
        get => _caption;
        set {
            if (_caption == value) { return; }
            _caption = value;
            OnPropertyChanged();
        }
    }

    public Database? Database => _filter?.Database;

    public Type? Editor { get; set; }

    public FilterCollection? Filter {
        get => _filter;
        private set {
            if (_filter == value) { return; }
            
            if (_filter != null) {
                _filter.PropertyChanged -= Filter_PropertyChanged;
                _filter.DisposingEvent -= Filter_DisposingEvent;
            }
            
            _filter = value;
            
            if (_filter != null) {
                _filter.PropertyChanged += Filter_PropertyChanged;
                _filter.DisposingEvent += Filter_DisposingEvent;
            }
            
            OnPropertyChanged();
            OnPropertyChanged(nameof(Database));
        }
    }

    public bool IsDisposed { get; private set; }

    public int Prio {
        get => _prio;
        set {
            if (_prio == value) { return; }
            _prio = value;
            OnPropertyChanged();
        }
    }

    public Color RowColor {
        get => _rowColor;
        set {
            if (_rowColor == value) { return; }
            _rowColor = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Methods

    public void Dispose() {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public string ErrorReason() {
        if (IsDisposed) { return "Objekt wurde verworfen"; }
        
        if (_filter == null) { return "Kein Filter vorhanden"; }
        
        if (!_filter.IsOk()) { return "Filter fehlerhaft: " + _filter.ErrorReason(); }
        
        if (string.IsNullOrEmpty(_caption)) { return "Caption fehlt"; }
        
        return string.Empty;
    }

    public string IsNowEditable() => IsDisposed ? "Objekt wurde verworfen" : string.Empty;

    public List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        
        List<string> result = [];
        result.ParseableAdd("Caption", _caption);
        result.ParseableAdd("Prio", _prio);
        result.ParseableAdd("RowColor", _rowColor.ToArgb());
        result.ParseableAdd("Filter", (IStringable?)_filter);
        return result;
    }

    public void ParseFinished(string parsed) { }

    public bool ParseThis(string key, string value) {
        switch (key.ToLowerInvariant()) {
            case "caption":
                _caption = value.FromNonCritical();
                return true;
                
            case "prio":
                _prio = IntParse(value);
                return true;
                
            case "rowcolor":
                _rowColor = Color.FromArgb(IntParse(value));
                return true;
                
            case "filter":
                var newFilter = new FilterCollection(value.FromNonCritical(), "RowColorRule");
                Filter = newFilter;
                return true;
        }
        
        return false;
    }

    public string ReadableText() {
        if (IsDisposed) { return "Verworfene Regel"; }
        return $"{_caption} (Prio: {_prio}, Farbe: {_rowColor.Name})";
    }

    public QuickImage? SymbolForReadableText() => QuickImage.Get(ImageCode.Pinsel, 16);

    public override string ToString() => ParseableItems().FinishParseable();

    private void Dispose(bool disposing) {
        if (!IsDisposed) {
            if (disposing) {
                OnDisposingEvent();
                
                if (_filter != null) {
                    _filter.PropertyChanged -= Filter_PropertyChanged;
                    _filter.DisposingEvent -= Filter_DisposingEvent;
                    _filter.Dispose();
                    _filter = null;
                }
            }
            
            IsDisposed = true;
        }
    }

    private void Filter_DisposingEvent(object? sender, System.EventArgs e) {
        if (_filter != null) {
            _filter.PropertyChanged -= Filter_PropertyChanged;
            _filter.DisposingEvent -= Filter_DisposingEvent;
            _filter = null;
            OnPropertyChanged(nameof(Filter));
            OnPropertyChanged(nameof(Database));
        }
    }

    private void Filter_PropertyChanged(object? sender, PropertyChangedEventArgs e) => OnPropertyChanged(nameof(Filter));

    private void OnDisposingEvent() => DisposingEvent?.Invoke(this, System.EventArgs.Empty);

    private void OnPropertyChanged([CallerMemberName] string propertyName = "unknown") {
        if (IsDisposed) { return; }
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
}
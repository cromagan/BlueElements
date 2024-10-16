﻿// Authors:
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueDatabase;
using BlueDatabase.Enums;

namespace BlueControls.CellRenderer;

public abstract class Renderer_Abstract : ParsebleItem, IReadableText, ISimpleEditor, IStyleableOne {

    #region Fields

    public readonly bool ReadOnly;
    internal static readonly Renderer_Abstract Bool = new Renderer_ImageAndText("+|Häkchen\r\no|Kreis2\r\n-|Kreuz");
    internal static readonly Renderer_Abstract Default = new Renderer_ImageAndText();
    private static readonly ConcurrentDictionary<string, string> Replaced = [];
    private static readonly ConcurrentDictionary<string, Size> Sizes = [];
    private string _lastCode = "?";
    private RowItem? _sheetStyle;
    private float _sheetStyleScale;
    private PadStyles _style = PadStyles.Style_Standard;

    #endregion

    #region Constructors

    public Renderer_Abstract() : this(false) { }

    protected Renderer_Abstract(bool readOnly) => ReadOnly = readOnly;

    #endregion

    #region Events

    public event EventHandler? DoUpdateSideOptionMenu;

    #endregion

    #region Properties

    public abstract string Description { get; }
    public BlueFont? Font { get; set; }
    public string QuickInfo => Description;

    public RowItem? SheetStyle {
        get => _sheetStyle;
        set {
            if (_sheetStyle == value) { return; }
            _sheetStyle = value;
            OnPropertyChanged();
        }
    }

    [DefaultValue(1.0)]
    public float SheetStyleScale {
        get => _sheetStyleScale;
        set {
            if (value < 0.1f) { value = 0.1f; }
            if (Math.Abs(_sheetStyleScale - value) < Constants.DefaultTolerance) { return; }
            _sheetStyleScale = value;
            OnPropertyChanged();
        }
    }

    public PadStyles Stil {
        get => _style;
        set {
            if (_style == value) { return; }
            _style = value;
            this.InvalidateFont();
        }
    }

    #endregion

    #region Methods

    public static Renderer_Abstract RendererOf(ColumnItem? column) {
        if (column == null || string.IsNullOrEmpty(column.DefaultRenderer)) { return Default; }

        var renderer = NewByTypeName<Renderer_Abstract>(column.DefaultRenderer);
        if (renderer == null) { return Default; }

        renderer.Parse(column.RendererSettings);

        return renderer;
    }

    public Size ContentSize(string content, Design design, States state, TranslationType translate) {
        if (string.IsNullOrEmpty(content)) { return Size.Empty; }

        var key = TextSizeKey(_lastCode, content);

        if (Sizes.TryGetValue(key, out var excontentsize)) { return excontentsize; }

        var contentsize = CalculateContentSize(content, design, state, translate);

        _ = Sizes.TryAdd(key, contentsize);

        return contentsize;
    }

    public abstract void Draw(Graphics gr, string content, Rectangle drawarea, Design design, States state, TranslationType translate, Alignment align, float scale);

    public abstract List<GenericControl> GetProperties(int widthOfControl);

    public override void OnPropertyChanged() {
        _lastCode = ParseableItems().FinishParseable();

        base.OnPropertyChanged();
    }

    public override void ParseFinished(string parsed) {
        base.ParseFinished(parsed);
        _lastCode = ParseableItems().FinishParseable();
    }

    public abstract string ReadableText();

    public abstract QuickImage? SymbolForReadableText();

    public string ValueReadable(string content, ShortenStyle style, TranslationType translate) {
        if (string.IsNullOrEmpty(content)) { return string.Empty; }

        var key = (int)style + "|" + (int)translate + "|" + TextSizeKey(_lastCode, content);

        if (Replaced.TryGetValue(key, out var excontentsize)) { return excontentsize; }

        var replaced = CalculateValueReadable(content, style, translate);

        _ = Replaced.TryAdd(key, replaced);

        return replaced;
    }

    internal static Renderer_Abstract RendererOf(ColumnViewItem columnViewItem) {
        if (!string.IsNullOrEmpty(columnViewItem.Renderer)) {
            var renderer = NewByTypeName<Renderer_Abstract>(columnViewItem.Renderer);
            if (renderer == null) { return RendererOf(columnViewItem.Column); }

            renderer.Parse(columnViewItem.RendererSettings);

            return renderer;
        }

        return RendererOf(columnViewItem.Column);
    }

    protected abstract Size CalculateContentSize(string content, Design design, States state, TranslationType doOpticalTranslation);

    protected abstract string CalculateValueReadable(string content, ShortenStyle style, TranslationType doOpticalTranslation);

    protected void OnDoUpdateSideOptionMenu() => DoUpdateSideOptionMenu?.Invoke(this, System.EventArgs.Empty);

    /// <summary>
    /// Ändert die anderen Zeilen dieser Spalte, so dass der verknüpfte Text bei dieser und den anderen Spalten gleich ist, ab.
    /// </summary>
    /// <param name="column"></param>
    /// <param name="renderer"></param>
    /// <param name="content"></param>
    private string TextSizeKey(string renderer, string content) => renderer + "|" + content;

    #endregion
}
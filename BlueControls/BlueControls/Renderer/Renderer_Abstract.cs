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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueDatabase;
using BlueDatabase.Enums;

namespace BlueControls.CellRenderer;

public abstract class Renderer_Abstract : ParsebleItem, IReadableTextWithKey, ISimpleEditor, IPropertyChangedFeedback {

    protected void OnDoUpdateSideOptionMenu() => DoUpdateSideOptionMenu?.Invoke(this, System.EventArgs.Empty);

    #region Fields

    internal static readonly Renderer_Abstract Default = new Renderer_Default("ImageAndText");
    private static readonly ConcurrentDictionary<string, Size> Sizes = [];
    private string _lastCode = "?";

    public event EventHandler? DoUpdateSideOptionMenu;

    #endregion

    #region Constructors

    protected Renderer_Abstract(string keyName) : base(keyName) { KeyName = keyName; }

    #endregion

    #region Properties

    public abstract string Description { get; }

    public string QuickInfo => Description;

    #endregion

    #region Methods

    public static Renderer_Abstract RendererOf(ColumnItem? column) {
        if (column == null || string.IsNullOrEmpty(column.DefaultRenderer)) { return new Renderer_Default("Null Renderer"); }

        var renderer = ParsebleItem.NewByTypeName<Renderer_Abstract>(column.DefaultRenderer, "Renderer of " + column.KeyName);
        if (renderer == null) { return new Renderer_Default("Unknown Renderer"); }

        renderer.Parse(column.RendererSettings);

        return renderer;
    }

    public abstract void Draw(Graphics gr, string content, Rectangle drawarea, Design design, States state, BildTextVerhalten behaviorOfImageAndText, TranslationType doOpticalTranslation, ReadOnlyCollection<string> opticalReplace, string constantHeightOfImageCode, float scale, Alignment align);

    public abstract List<GenericControl> GetProperties(int widthOfControl);

    public Size GetSizeOfCellContent(string content, Design design, States state, BildTextVerhalten behaviorOfImageAndText, TranslationType doOpticalTranslation, ReadOnlyCollection<string> opticalReplace, string constantHeightOfImageCode) {
        if (string.IsNullOrEmpty(content)) { return Size.Empty; }

        var key = TextSizeKey(_lastCode, content);

        if (key == null) { return Size.Empty; }

        if (Sizes.TryGetValue(key, out var excontentsize)) { return excontentsize; }

        var contentsize = CalculateContentSize(content, design, state, behaviorOfImageAndText, doOpticalTranslation, opticalReplace, constantHeightOfImageCode);

        SetSizeOfCellContent(content, contentsize);
        return contentsize;
    }

    public override void OnPropertyChanged() {
        _lastCode = ToParseableString();
        base.OnPropertyChanged();
    }

    public override void ParseFinished(string parsed) {
        base.ParseFinished(parsed);
        _lastCode = ToParseableString();
    }

    public abstract string ReadableText();

    public void SetSizeOfCellContent(string content, Size contentSize) {
        var key = TextSizeKey(_lastCode, content);

        if (key == null) { return; }

        if (Sizes.ContainsKey(key)) {
            Sizes[key] = contentSize;
            return;
        }

        _ = Sizes.TryAdd(key, contentSize);
    }

    public abstract QuickImage? SymbolForReadableText();

    public abstract string ValueReadable(string content, ShortenStyle style, BildTextVerhalten behaviorOfImageAndText, bool removeLineBreaks, TranslationType doOpticalTranslation, ReadOnlyCollection<string> opticalReplace);

    internal static Renderer_Abstract? RendererOf(ColumnViewItem columnViewItem) {
        if (!string.IsNullOrEmpty(columnViewItem.Renderer)) {
            var renderer = ParsebleItem.NewByTypeName<Renderer_Abstract>(columnViewItem.Renderer, "Renderer");
            if (renderer == null) { return RendererOf(columnViewItem.Column); }

            renderer.Parse(columnViewItem.RendererSettings);

            return renderer;
        }

        return RendererOf(columnViewItem.Column);
    }

    protected abstract Size CalculateContentSize(string content, Design design, States state, BildTextVerhalten behaviorOfImageAndText, TranslationType doOpticalTranslation, ReadOnlyCollection<string> opticalReplace, string constantHeightOfImageCode);

    /// <summary>
    /// Ändert die anderen Zeilen dieser Spalte, so dass der verknüpfte Text bei dieser und den anderen Spalten gleich ist, ab.
    /// </summary>
    /// <param name="column"></param>
    /// <param name="renderer"></param>
    /// <param name="content"></param>
    private string? TextSizeKey(string renderer, string content) {
        return renderer + "|" + content;
    }

    #endregion
}
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

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using BlueBasics;
using BlueBasics.Interfaces;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueDatabase;
using BlueDatabase.Enums;

namespace BlueControls.CellRenderer;

public abstract class AbstractRenderer : ParsebleItem, IReadableTextWithKey, ISimpleEditor {

    #region Fields

    private static readonly ConcurrentDictionary<string, Size> Sizes = [];

    #endregion

    #region Constructors

    protected AbstractRenderer(string keyName) : base(keyName) { KeyName = keyName; }

    #endregion

    #region Properties

    public abstract string Description { get; }
    public string QuickInfo => Description;

    #endregion

    #region Methods

    public static AbstractRenderer RendererOf(ColumnItem? column) {
        if (column == null || string.IsNullOrEmpty(column.DefaultRenderer)) { return new DefaultRenderer("Null Renderer"); }

        var renderer = ParsebleItem.NewByTypeName<AbstractRenderer>(column.DefaultRenderer, "Renderer of " + column.KeyName);
        if (renderer == null) { return new DefaultRenderer("Unknown Renderer"); }

        renderer.Parse(column.RendererSettings);

        return renderer;
    }

    public abstract void Draw(Graphics gr, string content, Rectangle drawarea, Design design, States state, ColumnItem? column, float scale);

    public abstract List<GenericControl> GetProperties(int widthOfControl);

    public Size GetSizeOfCellContent(ColumnItem column, string content, Design design, States state, BildTextVerhalten behaviorOfImageAndText, string prefix, string suffix, TranslationType doOpticalTranslation, ReadOnlyCollection<string> opticalReplace, float scale, string constantHeightOfImageCode) {
        if (string.IsNullOrEmpty(content)) { return Size.Empty; }

        var key = TextSizeKey(column, KeyName, content);

        if (key == null) { return Size.Empty; }

        if (Sizes.TryGetValue(key, out var excontentsize)) { return excontentsize; }

        var contentsize = CalculateContentSize(column, content, design, state, behaviorOfImageAndText, prefix, suffix,
            doOpticalTranslation, opticalReplace, scale, constantHeightOfImageCode);

        SetSizeOfCellContent(column, key, content, contentsize);
        return contentsize;
    }

    public abstract string ReadableText();

    public void SetSizeOfCellContent(ColumnItem column, string renderer, string content, Size contentSize) {
        var key = TextSizeKey(column, renderer, content);

        if (key == null) { return; }

        if (Sizes.ContainsKey(key)) {
            Sizes[key] = contentSize;
            return;
        }

        _ = Sizes.TryAdd(key, contentSize);
    }

    public abstract QuickImage? SymbolForReadableText();

    public abstract string ValueReadable(string content, ShortenStyle style, BildTextVerhalten behaviorOfImageAndText,
                                        bool removeLineBreaks, string prefix, string suffix, TranslationType doOpticalTranslation,
        ReadOnlyCollection<string> opticalReplace);

    internal static AbstractRenderer? RendererOf(ColumnViewItem columnViewItem) {
        if (!string.IsNullOrEmpty(columnViewItem.Renderer)) {
            var renderer = ParsebleItem.NewByTypeName<AbstractRenderer>(columnViewItem.Renderer, "Renderer");
            if (renderer == null) { return RendererOf(columnViewItem.Column); }

            renderer.Parse(columnViewItem.RendererSettings);

            return renderer;
        }

        return RendererOf(columnViewItem.Column);
    }

    protected abstract Size CalculateContentSize(ColumnItem column, string originalText, Design design, States state, BildTextVerhalten behaviorOfImageAndText, string prefix, string suffix, TranslationType doOpticalTranslation, ReadOnlyCollection<string> opticalReplace, float scale, string constantHeightOfImageCode);

    /// <summary>
    /// Ändert die anderen Zeilen dieser Spalte, so dass der verknüpfte Text bei dieser und den anderen Spalten gleich ist, ab.
    /// </summary>
    /// <param name="column"></param>
    /// <param name="renderer"></param>
    /// <param name="content"></param>
    private string? TextSizeKey(ColumnItem? column, string renderer, string content) {
        if (column?.Database is not { IsDisposed: false } db || column.IsDisposed) { return null; }
        return renderer + "|" + db.TableName + "|" + column.KeyName + "|" + content;
    }

    #endregion
}
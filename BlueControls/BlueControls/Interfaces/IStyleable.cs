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

using BlueDatabase;

namespace BlueControls.Interfaces;

/// <summary>
/// Wird vermendet, wenn das Element sein Aussehen verändern kann - mittels StyleDB
/// </summary>
public interface IStyleable {

    #region Properties

    public RowItem? SheetStyle { get; }

    public float SheetStyleScale { get; }

    #endregion
}

public static class StyleableExtension {

    #region Methods

    public static BlueFont GetFont(this IStyleable o, int stufe) {
        return Skin.GetBlueFont(Enums.PadStyles.Style_Standard, o.SheetStyle, stufe) ?? BlueFont.DefaultFont;
    }

    #endregion

    //#region Fields

    //public const float GridSize = 8; // PixelToMm(4f, ItemCollectionPadItem.Dpi);

    //public const float MinHeigthCapAndBox = 48;
    //public const float MinHeigthCaption = 16;
    //public const float MinHeigthTextBox = 24;

    //#endregion

    //#region Methods

    //public static bool CanChangeHeightTo(this IAutosizable item, float heightinPixel) => item.AutoSizeableHeight && heightinPixel > MinHeigthCapAndBox;

    //public static bool CanScaleHeightTo(this IAutosizable item, float scale) => CanChangeHeightTo(item, item.UsedArea.Height * scale);

    //#endregion
}
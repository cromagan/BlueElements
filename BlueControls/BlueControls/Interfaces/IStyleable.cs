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
using BlueControls.ItemCollectionPad;
using BlueDatabase;
using System;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace BlueControls.Interfaces;


/// <summary>
/// Wird vermendet, wenn das Element sein Aussehen verändern kann - mittels StyleDB
/// </summary>
public interface IStyleable {

    public IStyleable? Parent { get; set; }

    public void OnParentChanged();

    public void OnParentStyleChanged();

    (RowItem? sheetStyle, float? sheetStyleScale) MyStyle();
}

public static class StyleableExtension {

    public static (RowItem sheetStyle, float sheetStyleScale) GetParentStyle(this IStyleable ob) {

        if (ob.Parent is IStyleable obn) { return obn.GetParentStyle(); }

        var t = ob.MyStyle();

        if (t.sheetStyle is { } r1 && t.sheetStyleScale is { } s) { return (r1, s); }

        if (Skin.StyleDb?.Row.First() is { } r2) { return (r2, 1f); }

        Develop.DebugPrint(BlueBasics.Enums.FehlerArt.Fehler, "StyleDB-Fehler");
        throw new Exception();
    }

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
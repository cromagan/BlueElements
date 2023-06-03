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

using BlueDatabase;
using System.Collections.Generic;
using System.Drawing;
using static BlueBasics.Converter;

namespace BlueControls.Interfaces;

public interface IAutosizable {

    #region Properties

    bool AutoSizeableHeight { get; }
    public RectangleF UsedArea { get; }

    #endregion

    #region Methods

    public void SetCoordinates(RectangleF r, bool overrideFixedSize);

    #endregion
}

public static class IAutosizableExtension {

    #region Fields

    public static float MinHeigthToLines = MmToPixel(4f, 300);

    #endregion

    #region Methods

    public static bool CanChangeHeightTo(this IAutosizable item, float heightinPixel) {
        if (!item.AutoSizeableHeight) { return false; }

        return heightinPixel > MinHeigthToLines;
    }

    public static bool CanScaleHeightTo(this IAutosizable item, float scale) => CanChangeHeightTo(item, item.UsedArea.Height * scale);

    #endregion
}
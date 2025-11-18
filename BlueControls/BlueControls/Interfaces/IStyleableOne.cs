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
using BlueControls.Enums;
using System;

namespace BlueControls.Interfaces;

/// <summary>
/// Wird vermendet, wenn das Element sein Aussehen verändern kann - mittels StyleDB
/// Zusätzlich wenn es davon EINEN bestimmten Stil benutzt. Z.B. die Überschrift davon
/// </summary>
public interface IStyleableOne : IStyleable {

    #region Properties

    BlueFont? Font { get; set; }

    PadStyles Style { get; }

    #endregion
}

public static class StyleableOneExtension {

    #region Methods

    public static BlueFont GetFont(this IStyleableOne o, float additionalScale) => Math.Abs(1 - additionalScale) < Constants.DefaultTolerance ? GetFont(o) : GetFont(o).Scale(additionalScale);

    public static BlueFont GetFont(this IStyleableOne o) {
        o.Font ??= Skin.GetBlueFont(o.SheetStyle, o.Style);
        return o.Font;
    }

    public static void InvalidateFont(this IStyleableOne o) {
        if (o.Style != PadStyles.Undefiniert) {
            o.Font = null;
        }
    }

    #endregion
}
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

using BlueBasics.Interfaces;
using System;

namespace BlueControls.Interfaces;

public interface IStyleableParent : IParent, IStyleable {

    #region Events

    event EventHandler StyleChanged;

    #endregion

    #region Methods

    public void OnStyleChanged();

    #endregion
}

public static class StyleableParentExtension {
    //public static (string sheetStyle, float sheetStyleScale) GetParentStyle(this IStylableParent ob) {
    //    if (ob.Parent is IStylableParent obn) { return obn.GetParentStyle(); }

    //    var t = ob.MyStyle();

    //    if (t.sheetStyle is { } r1 && t.sheetStyleScale is { } s) { return (r1, s); }

    //    if (Skin.StyleDb?.Row.First() is { } r2) { return (r2, 1f); }

    //    Develop.DebugPrint(BlueBasics.Enums.FehlerArt.Fehler, "StyleDB-Fehler");
    //    throw new Exception();
    //}

    //public static void InvalidateFont(this IStylableChild o) {
    //    o.Font = null;
    //}

    //public static BlueFont? GetFont(this IStylableChild ob) {
    //    if (ob.Parent is IStylableChild obn) { return obn.GetFont(); }

    //    var t = ob.MyStyle();

    //    if (t.sheetStyle is { } r1 && t.sheetStyleScale is { } s) { return (r1, s); }

    //    if (Skin.StyleDb?.Row.First() is { } r2) { return (r2, 1f); }

    //    Develop.DebugPrint(BlueBasics.Enums.FehlerArt.Fehler, "StyleDB-Fehler");
    //    throw new Exception();

    //}
}
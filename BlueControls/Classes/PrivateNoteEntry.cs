// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
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

using BlueBasics.Classes;
using BlueBasics.ClassesStatic;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.Classes.ItemCollectionList;
using BlueControls.Controls;
using BlueControls.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.Json;

namespace BlueControls.Classes;

public sealed class PrivateNoteEntry : ISimpleEditor, IReadableText, IHasKeyName {

    #region Fields

    private static readonly Pen PenNoteCircle = new(Color.FromArgb(200, 100, 130, 200)) { Width = 2f };
    private static readonly Pen PenNoteCritical = new(Color.FromArgb(200, 220, 50, 50)) { Width = 2f };
    private static readonly Pen PenNoteNone = new(Color.FromArgb(200, 150, 150, 150)) { Width = 2f };
    private static readonly Pen PenNoteOk = new(Color.FromArgb(200, 50, 180, 80)) { Width = 2f };
    private static readonly Pen PenNoteWarning = new(Color.FromArgb(200, 230, 180, 30)) { Width = 2f };

    #endregion

    #region Constructors

    public PrivateNoteEntry(string keyName) => KeyName = keyName;

    #endregion

    #region Events

    public event EventHandler? DoUpdateSideOptionMenu;

    #endregion

    #region Properties

    public string Description => "Private Notiz bearbeiten";

    public bool KeyIsCaseSensitive => true;
    public string KeyName { get; private set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public string Symbol { get; set; } = "Stift";
    public float X { get; set; }

    public float Y { get; set; }

    #endregion

    #region Methods

    public static PrivateNoteEntry? Parse(JsonElement element) {
        if (element.ValueKind != JsonValueKind.Object) { return null; }

        var props = new Dictionary<string, JsonElement>();
        foreach (var prop in element.EnumerateObject()) {
            props[prop.Name] = prop.Value;
        }

        var keyName = JsonHelper.GetJsonProperty(props, "keyName", string.Empty);

        float xVal = 0f;
        float yVal = 0f;
        if (props.TryGetValue("x", out var xEl) && xEl.ValueKind == System.Text.Json.JsonValueKind.Number) {
            xVal = xEl.GetSingle();
        }
        if (props.TryGetValue("y", out var yEl) && yEl.ValueKind == System.Text.Json.JsonValueKind.Number) {
            yVal = yEl.GetSingle();
        }

        return new PrivateNoteEntry(keyName) {
            Symbol = JsonHelper.GetJsonProperty(props, "image", string.Empty),
            Note = JsonHelper.GetJsonProperty(props, "note", string.Empty),
            X = xVal,
            Y = yVal
        };
    }

    public static Pen PenForSymbol(string symbol) {
        return symbol switch {
            "Kritisch" => PenNoteCritical,
            "Warnung" => PenNoteWarning,
            "Häkchen" => PenNoteOk,
            "Kreis" => PenNoteCircle,
            _ => PenNoteNone
        };
    }

    public List<GenericControl> GetProperties(int widthOfControl) {
        var levels = new List<AbstractListItem> {
            new TextListItem("Kreis", "Kreis2", QuickImage.Get(ImageCode.Kreis, 16), false, true, string.Empty, string.Empty),
            new TextListItem("Neutral", "Stift", QuickImage.Get(ImageCode.Stift, 16), false, true, string.Empty, string.Empty),
            new TextListItem("Ok", "Häkchen", QuickImage.Get(ImageCode.HäkchenDoppelt, 16), false, true, string.Empty, string.Empty),
            new TextListItem("Warnung", "Warnung", QuickImage.Get(ImageCode.Warnung, 16), false, true, string.Empty, string.Empty),
            new TextListItem("Kritisch", "Kritisch", QuickImage.Get(ImageCode.Kritisch, 16), false, true, string.Empty, string.Empty)
        };

        return [
            //new FlexiControl("Symbol:", widthOfControl, true),
            new FlexiControlForProperty<string>(() => Symbol, levels),
            //new FlexiControl("Notiz:", widthOfControl, true),
            new FlexiControlForProperty<string>(() => Note, 10)
        ];
    }

    public Pen Pen() => PenForSymbol(Symbol);

    public string ReadableText() => Note;

    public QuickImage? SymbolForReadableText() => SymbolForReadableText(16);

    public QuickImage? SymbolForReadableText(int size) => Symbol switch {
        "Häkchen" => QuickImage.Get(ImageCode.Häkchen, size),
        "Warnung" => QuickImage.Get(ImageCode.Warnung, size),
        "Kritisch" => QuickImage.Get(ImageCode.Kritisch, size),
        "Stift" => QuickImage.Get(ImageCode.Stift, size),
        "Kreis" => QuickImage.Get(ImageCode.Kreis2, size),
        _ => null
    };

    #endregion
}
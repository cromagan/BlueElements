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
using BlueControls.Enums;
using BlueDatabase.Enums;
using System.Drawing;
using BlueControls.CellRenderer;
using BlueBasics.Enums;

namespace BlueControls.ItemCollectionList;

public class CellLikeListItem : AbstractListItem {
    // Implements IReadableText
    //http://www.kurztutorial.info/programme/punkt-mm/rechner.html
    // Dim Ausgleich As Double = MmToPixel(1 / 72 * 25.4, 300)
    //   Dim FixZoom As Single = 3.07F

    #region Fields

    private readonly Alignment _align;
    private readonly Renderer_Abstract _cellRenderer;
    private readonly SortierTyp _sortType;
    private readonly TranslationType _translate;

    #endregion

    #region Constructors

    public CellLikeListItem(string keyNameAndReadableText, Renderer_Abstract cellRenderer, bool enabled, TranslationType translate, Alignment align, SortierTyp sortType) : base(keyNameAndReadableText, enabled) {
        _cellRenderer = cellRenderer;
        _translate = translate;
        _align = align;
        _sortType = sortType;
    }

    #endregion

    #region Properties

    public override string QuickInfo => KeyName.CreateHtmlCodes(true);

    #endregion

    #region Methods

    public override bool FilterMatch(string filterText) {
        if (base.FilterMatch(filterText)) { return true; }
        if (_cellRenderer == null) { return false; }
        var txt = _cellRenderer.ValueReadable(KeyName, ShortenStyle.Both, _translate);
        return txt.ToUpperInvariant().Contains(filterText.ToUpperInvariant());
    }

    public override int HeightForListBox(ListBoxAppearance style, int columnWidth, Design itemdesign, Renderer_Abstract renderer) => SizeUntouchedForListBox(itemdesign).Height;

    protected override Size ComputeSizeUntouchedForListBox(Design itemdesign) {
        if (_cellRenderer == null) { return new Size(16, 0); }

        return _cellRenderer.ContentSize(KeyName, itemdesign, States.Standard, _translate);
    }

    protected override void DrawExplicit(Graphics gr, Rectangle positionModified, Design itemdesign, States state, bool drawBorderAndBack, bool translate) {
        if (drawBorderAndBack) {
            Skin.Draw_Back(gr, itemdesign, state, positionModified, null, false);
        }
        _cellRenderer?.Draw(gr, KeyName, positionModified, itemdesign, state, _translate, (Alignment)_align, 1f);
        if (drawBorderAndBack) {
            Skin.Draw_Border(gr, itemdesign, state, positionModified);
        }
    }

    protected override string GetCompareKey() {
        // Die Hauptklasse fragt nach diesem Compare-Key
        if (_cellRenderer == null) {
            // Wenn _styleLikeThis null ist, geben wir einen leeren String zurück
            return string.Empty;
        }
        // Erzeugen eines lesbaren Werts basierend auf dem internen Wert und dem Stil
        var txt = _cellRenderer.ValueReadable(KeyName, ShortenStyle.HTML, _translate);
        // Erzeugen des Compare-Keys basierend auf dem lesbaren Wert und dem Sortiertyp des Stils
        var compareKey = txt.CompareKey(_sortType);
        // Rückgabe des Compare-Keys mit dem internen Wert
        return $"{compareKey}|{KeyName}";
    }

    #endregion
}
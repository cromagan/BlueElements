// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueControls.ListItems;

public class CellDesignListItem : ListItem {

    #region Fields

    private readonly Alignment _align;
    private readonly Renderer.Renderer _cellRenderer;
    private readonly SortierTyp _sortType;
    private readonly TranslationType _translate;

    #endregion

    #region Constructors

    public CellDesignListItem(string keyNameAndReadableText, Renderer.Renderer cellRenderer, bool enabled, TranslationType translate, Alignment align, SortierTyp sortType) : base(keyNameAndReadableText, enabled) {
        _cellRenderer = cellRenderer;
        _translate = translate;
        _align = align;
        _sortType = sortType;
        QuickInfo = cellRenderer.ValueReadable(keyNameAndReadableText, ShortenStyle.Replaced, translate);
    }

    #endregion

    #region Methods

    public override bool FilterMatch(string filterText) {
        if (base.FilterMatch(filterText)) { return true; }
        var txt = _cellRenderer.ValueReadable(KeyName, ShortenStyle.Both, _translate);
        return txt.Contains(filterText, StringComparison.OrdinalIgnoreCase);
    }

    public override int HeightInControl(ListBoxAppearance style, int columnWidth, Design itemdesign) => UntrimmedCanvasSize(itemdesign).Height;

    protected override Size ComputeUntrimmedCanvasSize(Design itemdesign) =>
        _cellRenderer.ContentSize(KeyName, _translate);

    protected override void DrawExplicit(Graphics gr, Rectangle visibleAreaControl, RectangleF positionControl, Design itemdesign, States state, bool drawBorderAndBack, bool translate, float offsetX, float offsetY, float zoom) {
        if (drawBorderAndBack) {
            Skin.Draw_Back(gr, itemdesign, state, positionControl.ToRect(), null, false);
        }
        _cellRenderer.Draw(gr, KeyName, null, positionControl.ToRect(), _translate, _align, zoom, itemdesign, state);
        if (drawBorderAndBack) {
            Skin.Draw_Border(gr, itemdesign, state, positionControl.ToRect());
        }
    }

    protected override string GetCompareKey() {
        // Bei Zahlenwerten den Rohwert nutzen, die Anzeige-Formatierung (z. B. 1.000) zerstört das Parsen.
        var txt = _sortType is SortierTyp.ZahlenwertInt or SortierTyp.ZahlenwertFloat
            ? KeyName
            : _cellRenderer.ValueReadable(KeyName, ShortenStyle.HTML, _translate);
        return $"{txt.CompareKey(_sortType)}|{KeyName}";
    }

    #endregion
}
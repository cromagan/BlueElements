// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.Chars;

public class ListItemChar : Char {

    #region Fields

    private const int PaddingH = 6;
    private const int PaddingV = 2;

    #endregion

    #region Constructors

    public ListItemChar() { }

    public ListItemChar(ListItem listItem) => Item = listItem;

    #endregion

    #region Properties

    public States ChipState { get; set; } = States.Standard;

    public ListItem Item { get; init; } = new TextListItem(string.Empty, string.Empty, null, false, true, string.Empty, string.Empty);
    public override Alignment RowAlignment => Alignment.VerticalCenter;

    #endregion

    #region Methods

    public override void Draw(Graphics gr, Point controlPos, Size controlSize, float zoom) {
        var chipRect = new Rectangle(controlPos.X, controlPos.Y, controlSize.Width, controlSize.Height);

        Skin.Draw_Back(gr, Design.Button, ChipState, chipRect, null, false);
        Skin.Draw_Border(gr, Design.Button, ChipState, chipRect);

        var itemState = ChipState;
        if (!Item.Enabled) { itemState = States.Standard_Disabled; }

        Item.CanvasPosition = new Rectangle(chipRect.X + PaddingH, chipRect.Y + PaddingV, chipRect.Width - 2 * PaddingH, chipRect.Height - 2 * PaddingV);
        Item.Draw(gr, chipRect, 0, 0, Design.Button, Design.Button, itemState, false, string.Empty, false, Design.Undefined, zoom);
    }

    public override string HtmlText() => "???";

    public override bool IsLineBreak() => false;

    public override bool IsPossibleLineBreak() => true;

    public override bool IsSpace() => false;

    public override bool IsWordSeparator() => true;

    public override string PlainText() => Item.KeyName;

    protected override SizeF CalculateSizeCanvas() {
        var untrimmedSize = Item.UntrimmedCanvasSize(Design.Button);
        return new SizeF(untrimmedSize.Width + 2 * PaddingH, untrimmedSize.Height + 2 * PaddingV);
    }

    #endregion
}
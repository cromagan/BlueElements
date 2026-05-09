// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes;
using BlueControls.Classes.ItemCollectionList;

namespace BlueControls.Extended_Text;

public class ExtCharListItem : ExtChar {

    #region Fields

    private const int PaddingH = 6;
    private const int PaddingV = 2;

    #endregion

    #region Constructors

    public ExtCharListItem() { }

    public ExtCharListItem(AbstractListItem listItem) {
        ListItem = listItem;
    }

    #endregion

    #region Properties

    public States ChipState { get; set; } = States.Standard;

    public AbstractListItem ListItem { get; private set; } = new TextListItem(string.Empty, string.Empty, null, false, true, string.Empty, string.Empty);
    public override Alignment RowAlignment => Alignment.VerticalCenter;

    #endregion

    #region Methods

    public override void Draw(Graphics gr, Point controlPos, Size controlSize, float zoom) {
        var chipRect = new Rectangle(controlPos.X, controlPos.Y, controlSize.Width, controlSize.Height);

        Skin.Draw_Back(gr, Design.Button, ChipState, chipRect, null, false);
        Skin.Draw_Border(gr, Design.Button, ChipState, chipRect);

        var itemState = ChipState;
        if (!ListItem.Enabled) { itemState = States.Standard_Disabled; }

        ListItem.CanvasPosition = new Rectangle(chipRect.X + PaddingH, chipRect.Y + PaddingV, chipRect.Width - 2 * PaddingH, chipRect.Height - 2 * PaddingV);
        ListItem.Draw(gr, chipRect, 0, 0, Design.Button, Design.Button, itemState, false, string.Empty, false, Design.Undefined, zoom);
    }

    public override string HtmlText() => "???";

    public override bool IsLineBreak() => false;

    public override bool IsPossibleLineBreak() => true;

    public override bool IsSpace() => false;

    public override bool IsWordSeparator() => true;

    public override string PlainText() => ListItem.KeyName;

    protected override SizeF CalculateSizeCanvas() {
        var untrimmedSize = ListItem.UntrimmedCanvasSize(Design.Button);
        return new SizeF(untrimmedSize.Width + 2 * PaddingH, untrimmedSize.Height + 2 * PaddingV);
    }

    #endregion
}
// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Extended_Text;

namespace BlueControls.Classes.ItemCollectionList;

public class ReadableListItem : AbstractListItem {

    #region Fields

    private const int ErrorIndent = 20;
    private const int MaxErrorLines = 3;
    private const int SymbolSize = 16;
    private QuickImage? _symbol;
    private string _text = string.Empty;

    #endregion

    #region Constructors

    public ReadableListItem(IReadableTextWithKey item, bool enabled, string userDefCompareKey) : this(item, item.KeyName, enabled, item.QuickInfo, userDefCompareKey) { }

    public ReadableListItem(IReadableText item, string keyName, bool enabled, string quickinfo, string userDefCompareKey) : base(keyName, enabled) {
        UserDefCompareKey = userDefCompareKey;
        _text = item.ReadableText();
        _symbol = item.SymbolForReadableText();
        QuickInfo = string.IsNullOrEmpty(quickinfo) ? _text.CreateHtmlCodes() : quickinfo;
        Item = item;
    }

    #endregion

    #region Properties

    public IReadableText? Item {
        get;
        private set {
            if (field == value) { return; }

            if (field is INotifyPropertyChanged npc) {
                npc.PropertyChanged -= Item_PropertyChanged;
            }

            if (field is IDisposableExtendedWithEvent disposable) {
                disposable.DisposingEvent -= Item_DisposingEvent;
            }

            field = value;
            Update();

            if (field is INotifyPropertyChanged npc2) {
                npc2.PropertyChanged += Item_PropertyChanged;
            }

            if (field is IDisposableExtendedWithEvent disposable2) {
                disposable2.DisposingEvent += Item_DisposingEvent;
            }
        }
    }

    #endregion

    #region Methods

    public override bool FilterMatch(string filterText) => base.FilterMatch(filterText) || _text.Contains(filterText, StringComparison.OrdinalIgnoreCase);

    public override int HeightInControl(ListBoxAppearance style, int columnWidth, Design itemdesign) {
        var firstH = Skin.GetBlueFont(itemdesign, States.Standard).FormatedText_NeededSize(_text, _symbol, SymbolSize).Height;
        return firstH + ErrorHeightFor(columnWidth, itemdesign);
    }

    protected override Size ComputeUntrimmedCanvasSize(Design itemdesign) {
        var firstSize = Skin.GetBlueFont(itemdesign, States.Standard).FormatedText_NeededSize(_text, _symbol, SymbolSize);

        if (!HasError()) { return firstSize; }

        var errLineH = Skin.GetBlueFont(itemdesign, States.Standard_Disabled).FormatedText_NeededSize("X", null, SymbolSize).Height;
        return new Size(firstSize.Width, firstSize.Height + errLineH * MaxErrorLines);
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            Item = null;
        }
        base.Dispose(disposing);
    }

    protected override void DrawExplicit(Graphics gr, Rectangle visibleAreaControl, RectangleF positionControl, Design itemdesign, States state, bool drawBorderAndBack, bool translate, float offsetX, float offsetY, float zoom) {
        // Clip auf den eigenen Bereich, damit der Fehlertext nie über andere Items gezeichnet wird.
        // Wird die Canvas-Größe (z. B. bei erst später erscheinendem Fehler) nicht rechtzeitig
        // neu berechnet, wäre die ErrorRect-Höhe 0 und ExtChar.IsVisible würde ungeclippt zeichnen.
        var clipState = gr.Save();
        try {
            gr.SetClip(positionControl);

            if (drawBorderAndBack) {
                Skin.Draw_Back(gr, itemdesign, state, positionControl.ToRect(), null, false);
            }

            var lineHeight = Skin.GetBlueFont(itemdesign, States.Standard).FormatedText_NeededSize("X", null, SymbolSize).Height;
            var firstLineRect = new RectangleF(positionControl.X, positionControl.Y, positionControl.Width, lineHeight);

            Skin.Draw_FormatedText(gr, _text, _symbol, Alignment.VerticalCenter_Left, firstLineRect.ToRect(), itemdesign, state, null, false, translate);

            if (HasError()) {
                var errIndent = ErrorIndent.CanvasToControl(zoom);
                var errorRect = new RectangleF(
                    positionControl.X + errIndent,
                    positionControl.Y + lineHeight,
                    Math.Max(0, positionControl.Width - errIndent),
                    Math.Max(0, positionControl.Height - lineHeight));

                var disabledFont = Skin.GetBlueFont(itemdesign, States.Standard_Disabled);
                var errHtml = ErrorHtml();

                using var errTxt = new ExtText {
                    BaseFont = disabledFont,
                    HtmlText = errHtml,
                    TextDimensions = new Size((int)errorRect.Width, -1),
                    AreaControl = errorRect.ToRect(),
                    Ausrichtung = Alignment.Top_Left,
                };
                errTxt.Draw(gr, zoom, (int)errorRect.X, (int)errorRect.Y);
            }

            if (drawBorderAndBack) {
                Skin.Draw_Border(gr, itemdesign, state, positionControl.ToRect());
            }
        } finally {
            gr.Restore(clipState);
        }
    }

    protected override string GetCompareKey() => KeyName.CompareKey(SortierTyp.Sprachneutral_String);

    private string ErrorHtml() {
        var kritisch = QuickImage.Get(ImageCode.Kritisch, SymbolSize);
        return (kritisch?.HTMLCode ?? string.Empty) + ErrorText().Replace("\r\n", "<br>").Replace("\n", "<br>");
    }

    private int ErrorHeightFor(int availableWidth, Design itemdesign) {
        if (!HasError() || availableWidth <= ErrorIndent) { return 0; }

        var disabledFont = Skin.GetBlueFont(itemdesign, States.Standard_Disabled);
        var lineH = disabledFont.FormatedText_NeededSize("X", null, SymbolSize).Height;

        using var errTxt = new ExtText {
            BaseFont = disabledFont,
            HtmlText = ErrorHtml(),
            TextDimensions = new Size(availableWidth - ErrorIndent, -1)
        };

        return Math.Min(errTxt.HeightControl, lineH * MaxErrorLines);
    }

    private string ErrorText() => Item is IErrorCheckable ec ? ec.ErrorReason() ?? string.Empty : string.Empty;

    private bool HasError() => !string.IsNullOrEmpty(ErrorText());

    private void Item_DisposingEvent(object? sender, System.EventArgs e) => Item = null;

    private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e) => Update();

    private void Update() {
        if (Item is IDisposableExtended di && di.IsDisposed) {
            Item = null;
            return; // Item seter geht nochmal rein
        }

        if (Item is null) {
            _text = "Verworfen";
            _symbol = QuickImage.Get(ImageCode.Kritisch, SymbolSize);
            QuickInfo = string.Empty;
        } else {
            _text = Item.ReadableText();
            _symbol = Item.SymbolForReadableText();
            if (Item is IHasKeyName hkn) { KeyName = hkn.KeyName; }
            if (Item is IReadableTextWithKey rtk) { QuickInfo = rtk.QuickInfo; }
        }
        Invalidate_UntrimmedCanvasSize();
    }

    #endregion
}
// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes.ItemCollectionPad.Abstract;
using BlueControls.Controls;
using static BlueBasics.ClassesStatic.Converter;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Classes.ItemCollectionPad;

public class SymbolPadItem : RectanglePadItem, IStyleableOne {

    #region Fields

    private PadStyles _style = PadStyles.Standard;

    #endregion

    #region Constructors

    public SymbolPadItem() : base(string.Empty) {
        Symbol = Contour.Arrow;
        Hintergrundfarbe = Color.White;
        Randfarbe = Color.Black;
        Randdicke = 1;
    }

    #endregion

    #region Properties

    public static string ClassId => "Symbol";

    public override string Description => string.Empty;

    public BlueFont? Font { get; set; }

    public Color Hintergrundfarbe { get; set; }

    public float Randdicke { get; set; }

    public Color Randfarbe { get; set; }

    public string SheetStyle => Parent is IStyleable ist ? ist.SheetStyle : string.Empty;

    public PadStyles Style {
        get => _style;
        set {
            if (_style == value) { return; }
            _style = value;
            this.InvalidateFont();
            OnPropertyChanged();
        }
    }

    public Contour Symbol { get; set; }

    protected override int SaveOrder => 999;

    #endregion

    #region Methods

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<GenericControl> result =
        [
            new FlexiControlForProperty<Contour>(() => Symbol, ItemsOf(typeof(Contour))),
            new FlexiControlForProperty<float>(() => Randdicke),
            new FlexiControlForProperty<Color>(() => Randfarbe),
            new FlexiControlForProperty<Color>(() => Hintergrundfarbe),
            .. base.GetProperties(widthOfControl),
        ];
        return result;
    }

    public override List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [.. base.ParseableItems()];
        result.ParseableAdd("Contour", Symbol);
        result.ParseableAdd("Backcolor", Hintergrundfarbe.ToArgb());
        result.ParseableAdd("BorderColor", Randfarbe.ToArgb());
        result.ParseableAdd("BorderWidth", Randdicke);
        result.ParseableAdd("Style", _style);
        return result;
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "symbol":
            case "contour":
                Symbol = (Contour)IntParse(value);
                return true;

            case "backcolor":
                Hintergrundfarbe = ColorParse(value);
                return true;

            case "bordercolor":
                Randfarbe = ColorParse(value);
                return true;

            case "borderwidth":
                FloatTryParse(value.FromNonCritical(), out var tRanddicke);
                Randdicke = tRanddicke;
                return true;

            case "style":
                _style = (PadStyles)IntParse(value);
                return true;

            case "fill": // alt: 28.11.2019
            case "whiteback": // alt: 28.11.2019
                return true;
        }
        return base.ParseThis(key, value);
    }

    public override string ReadableText() => "Symbol";

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Stern, 16);

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);
        UnRegisterEvents();
    }

    protected override void DrawExplicit(Graphics gr, Rectangle visibleAreaControl, RectangleF positionControl, float zoom, float offsetX, float offsetY, bool forPrinting) {
        if (Symbol is Contour.None or Contour.Undefined) { return; }

        var trp = positionControl.PointOf(Alignment.Horizontal_Vertical_Center);
        var w = (int)positionControl.Width;
        var h = (int)positionControl.Height;
        var p = GraphicsPaths.GetContour(Symbol, w, h);

        if (p != null && Parent != null) {
            gr.TranslateTransform(trp.X, trp.Y);
            gr.RotateTransform(-Drehwinkel);
            gr.TranslateTransform(-w / 2f, -h / 2f);
            gr.FillPath(new SolidBrush(Hintergrundfarbe), p);
            gr.DrawPath(new Pen(Randfarbe, Randdicke.CanvasToControl(zoom)), p);
            gr.ResetTransform();
        }
    }

    protected override void OnParentChanged() {
        base.OnParentChanged();
        this.InvalidateFont();
        if (Parent is ItemCollectionPadItem icpi) {
            icpi.StyleChanged += Icpi_StyleChanged;
        }
    }

    protected override void OnParentChanging() {
        base.OnParentChanging();
        UnRegisterEvents();
    }

    private void Icpi_StyleChanged(object? sender, System.EventArgs e) => this.InvalidateFont();

    private void UnRegisterEvents() {
        if (Parent is ItemCollectionPadItem icpi) {
            icpi.StyleChanged -= Icpi_StyleChanged;
        }
    }

    #endregion
}
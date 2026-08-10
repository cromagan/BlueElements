// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes.ItemCollectionList;
using BlueControls.Classes.ItemCollectionPad.Abstract;
using BlueControls.Controls;
using BlueControls.EventArgs;
using BlueControls.Extended_Text;
using BlueScript.Variables;
using System.Globalization;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Classes.ItemCollectionPad;

public class TextPadItem : RectanglePadItem, ICanHaveVariables, IStyleableOne, ISupportsTextScale {

    #region Fields

    /// <summary>
    /// Kopie von _text_original - aber mit evtl. ersetzten Variablen
    /// </summary>
    private string _textReplaced;

    /// <summary>
    /// Dieses Element ist nur temporär und ist der tatsächlich angezeigte Text - mit Bildern, verschieden Größen, etc.
    /// Wird immer von _text_replaced abgeleitet.
    /// </summary>
    private ExtText? _txt;

    #endregion

    #region Constructors

    public TextPadItem() : this(string.Empty, string.Empty) { }

    public TextPadItem(string keyName, string visibleText) : base(keyName) {
        // Suppress-Modus whrend der Konstruktion: Property-Setter (Text,
        // Ausrichtung) lsen keine Change-Events aus.
        // Siehe ParseableItem.ISupportInitialize.
        BeginInit();
        try {
            _textReplaced = visibleText;
            Text = visibleText;
            Ausrichtung = Alignment.Top_Left;
        } finally {
            EndInit();
        }
        InvalidateText();
    }

    #endregion

    #region Properties

    public static string ClassId => "TEXT";

    public Alignment Ausrichtung {
        get;
        set {
            if (IsDisposed) { return; }
            if (field == value) { return; }
            field = value;
            InvalidateText();
            OnPropertyChanged();
        }
    } = Alignment.Top_Left;

    public override string Description => string.Empty;

    public BlueFont? Font { get; set; }

    public string SheetStyle => Parent is IStyleable ist ? ist.SheetStyle : string.Empty;

    public PadStyles Style {
        get;
        set {
            if (field == value) { return; }
            field = value;
            InvalidateText();
            OnPropertyChanged();
        }
    } = PadStyles.Standard;

    [Description("Text der angezeigt werden soll.<br>Alternativ kann ein (oder mehrere) Variablenname im Format ~Name~ angegeben werden.")]
    public string Text {
        get;
        set {
            if (IsDisposed) { return; }
            if (value == field) { return; }
            field = value;
            _textReplaced = value;
            InvalidateText();
            //CalculateSlavePoints();
            OnPropertyChanged();
        }
    } = string.Empty;

    public float TextScale {
        get;
        set {
            value = Math.Clamp(value, 0.01f, 20);
            if (Math.Abs(value - field) < Constants.DefaultTolerance) { return; }
            field = value;
            InvalidateText();
            OnPropertyChanged();
        }
    } = 3.07f;

    protected override int SaveOrder => 999;

    #endregion

    #region Methods

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<AbstractListItem> aursicht =
        [
            ItemOf("Linksbündig ausrichten", ((int)Alignment.Top_Left).ToString1(), ImageCode.Linksbündig),
            ItemOf("Zentrieren", ((int)Alignment.Top_HorizontalCenter).ToString1(), ImageCode.Zentrieren),
            ItemOf("Rechtsbündig ausrichten", ((int)Alignment.Top_Right).ToString1(), ImageCode.Rechtsbündig)
        ];

        var textFlex = new FlexiControlForProperty<string>(() => Text, 5);

        if (Parent is ItemCollectionPadItem icpi) {
            var vars = icpi.GetExportVariables();
            var applicableVars = vars.Where(v => v.ToStringPossible).ToList();

            textFlex.QuickInfo = icpi.GetExportVariablesInfo(textFlex.QuickInfo, applicableVars.Count);

            if (applicableVars.Count > 0) {
                textFlex.EditType = EditTypeFormula.Textfeld_mit_Suggestions;
                textFlex.SuggestionPosition = SuggestionPosition.ContextMenuOnly;
                textFlex.ListItems = [.. applicableVars.Select(v => ItemOf($"~{v.KeyName}~"))];
            }
        }

        List<GenericControl> result =
        [
            textFlex,
            new FlexiControlForProperty<Alignment>(() => Ausrichtung, aursicht),
            new FlexiControlForProperty<float>(() => TextScale),
            new FlexiControlForProperty<PadStyles>(() => Style, Skin.GetRahmenArt(SheetStyle, true))
        ];
        result.AddRange(base.GetProperties(widthOfControl));
        return result;
    }

    public override List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [.. base.ParseableItems()];
        result.ParseableAdd("ReadableText", Text.EscapeUnicode());
        result.ParseableAdd("Alignment", Ausrichtung);
        result.ParseableAdd("AdditionalScale", TextScale);
        result.ParseableAdd("Style", Style);
        return result;
    }

    public override JsonObject ParseableJson() {
        var json = base.ParseableJson();
        json.Set("text", Text);
        json.Set("alignment", (int)Ausrichtung);
        json.Set("additionalscale", TextScale);
        json.Set("style", (int)Style);
        return json;
    }

    public override void ParseFinished(string parsed) {
        base.ParseFinished(parsed);
        InvalidateText();
    }

    public override void ParseFinishedJson(JsonObject parsed) {
        base.ParseFinishedJson(parsed);
        InvalidateText();
    }

    public override void ParseJson(JsonObject json) {
        BeginInit();
        try {
            Text = json.GetString("text", Text);
            Ausrichtung = json.GetEnum("alignment", Ausrichtung);
            TextScale = json.GetFloat("additionalscale", TextScale);
            Style = json.GetEnum("style", Style);
            base.ParseJson(json);
        } finally {
            EndInit();
        }
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "readabletext":
                Text = value.FromNonCritical().UnEscapeUnicode();
                return true;

            case "alignment":
                Ausrichtung = (Alignment)byte.Parse(value, CultureInfo.InvariantCulture);
                return true;

            case "style":
                Style = (PadStyles)IntParse(value);
                return true;

            case "additionalscale":
                TextScale = FloatParse(value.FromNonCritical());
                return true;
        }
        return base.ParseThis(key, value);
    }

    public override void PointMoved(object? sender, MoveEventArgs e) {
        base.PointMoved(sender, e);
        InvalidateText();
    }

    public override string ReadableText() => "Text";

    /// <summary>
    /// Löst die angegebene Variable in _text_replaced auf, falls diese (noch) vorhanden ist.
    /// </summary>
    /// <param name="variable"></param>
    /// <returns></returns>
    public bool ReplaceVariable(Variable variable) {
        if (IsDisposed) { return false; }
        var nt = variable.ReplaceInText(_textReplaced);

        if (nt == _textReplaced) { return false; }
        _textReplaced = nt;
        InvalidateText();
        OnPropertyChanged("Variables");
        return true;
    }

    public bool ResetVariables() {
        if (IsDisposed) { return false; }
        if (Text == _textReplaced) { return false; }
        _textReplaced = Text;
        InvalidateText();
        OnPropertyChanged("Variables");
        return true;
    }

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Textfeld2, 16);

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);
        UnRegisterEvents();
    }

    //public override void CalculateSlavePoints() {
    //    base.CalculateSlavePoints();
    //    InvalidateText();
    //}
    protected override void DrawExplicit(Graphics gr, Rectangle visibleAreaControl, RectangleF positionControl, float zoom, float offsetX, float offsetY, bool forPrinting) {
        if (Style != PadStyles.Undefined) {
            gr.SetClip(positionControl);
            var trp = positionControl.PointOf(Alignment.Horizontal_Vertical_Center);
            gr.TranslateTransform(trp.X, trp.Y);
            gr.RotateTransform(-Drehwinkel);

            if (_txt is null) { MakeNewETxt(); }

            if (_txt is not null && Parent is not null) {
                var offsetX2 = (int)(positionControl.Left - trp.X);
                var offsetY2 = (int)(positionControl.Top - trp.Y);

                _txt.AreaControl = Rectangle.Empty; // new Rectangle(drawingCoordinates.Left, drawingCoordinates.Top, drawingCoordinates.Width, drawingCoordinates.Height);
                if (!string.IsNullOrEmpty(_textReplaced) || !forPrinting) {
                    _txt.Draw(gr, zoom * TextScale, offsetX2, offsetY2);
                }
            }
            gr.TranslateTransform(-trp.X, -trp.Y);
            gr.ResetTransform();
            gr.ResetClip();
        }
    }

    protected override void OnParentChanged() {
        base.OnParentChanged();
        InvalidateText();
        if (Parent is ItemCollectionPadItem icpi) {
            icpi.StyleChanged += Icpi_StyleChanged;
        }
    }

    protected override void OnParentChanging() {
        base.OnParentChanging();
        UnRegisterEvents();
    }

    private void Icpi_StyleChanged(object? sender, System.EventArgs e) => InvalidateText();

    private void InvalidateText() {
        this.InvalidateFont();
        _txt = null;
    }

    private void MakeNewETxt() {
        _txt = null;
        if (Style != PadStyles.Undefined) {
            if (Parent is null) {
                Develop.DebugError("Parent is Nothing, wurde das Objekt zu einer Collection hinzugefügt?");
                return;
            }

            _txt = new ExtText(SheetStyle, Style) {
                HtmlText = !string.IsNullOrEmpty(_textReplaced) ? _textReplaced : "{Text}",
                //// da die Font 1:1 berechnet wird, aber bei der Ausgabe evtl. skaliert,
                //// muss etxt vorgegaukelt werden, daß der Drawberehich xxx% größer ist
                //etxt.DrawingArea = new Rectangle((int)CanvasUsedArea().Left, (int)CanvasUsedArea().Top, (int)(CanvasUsedArea().Width / AdditionalScale / SheetStyleScale), -1);
                //etxt.LineBreakWidth = etxt.DrawingArea.Width;
                TextDimensions = new Size((int)(CanvasUsedArea.Width / TextScale), -1),
                Ausrichtung = Ausrichtung
            };
        }
    }

    private void UnRegisterEvents() {
        if (Parent is ItemCollectionPadItem icpi) {
            icpi.StyleChanged -= Icpi_StyleChanged;
        }
    }

    #endregion
}
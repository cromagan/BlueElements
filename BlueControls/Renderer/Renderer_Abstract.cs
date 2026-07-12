// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes;
using BlueControls.Controls;
using System.Runtime.CompilerServices;

namespace BlueControls.Renderer;

public abstract class Renderer_Abstract : ParseableItem, IReadableText, ISimpleEditor {

    #region Fields

    public static readonly AssemblyAwareCache<Renderer_Abstract> AllRenderers = new();

    internal static readonly Renderer_Abstract Bool = new Renderer_ImageAndText("+|Häkchen\r\no|Kreis2\r\n-|Kreuz");
    internal static readonly Renderer_Abstract Default = new Renderer_ImageAndText();
    private static readonly ConcurrentCache<string, string> Replaced = new(1000);
    private static readonly ConcurrentCache<string, Size> Sizes = new(1000);
    private static readonly ConcurrentCache<string, Size> SizesAtWidth = new(2000);
    private BlueFont? _font;
    private string _lastCode = "?";

    #endregion

    #region Constructors

    protected Renderer_Abstract() : this(false) { }

    protected Renderer_Abstract(bool readOnly) => ReadOnly = readOnly;

    #endregion

    #region Events

    public event EventHandler? DoUpdateSideOptionMenu;

    #endregion

    #region Properties

    public abstract string Description { get; }
    public string QuickInfo => Description;
    public bool ReadOnly { get; }

    public string SheetStyle {
        get;
        set {
            if (field == value) { return; }
            if (ReadOnly) { Develop.DebugPrint_ReadOnly(); return; }
            field = value;
            _font = null;
            OnPropertyChanged();
        }
    } = Constants.Win11;

    #endregion

    #region Methods

    public Size ContentSize(string content, TranslationType translate) {
        //if (string.IsNullOrEmpty(content)) { return CanvasSize.Empty; }

        var key = TextSizeKey(_lastCode, content);

        return Sizes.GetOrAdd(key, _ => CalculateContentSize(content, translate));
    }

    /// <summary>
    /// Berechnet die benötigte Inhaltsgröße bei einer vorgegebenen Canvas-Breite.
    /// Einzeilige Renderer ignorieren die Breite. Mehrzeilige Renderer (z. B.
    /// <see cref="Renderer_RichText"/>) berechnen die korrekte Höhe basierend auf
    /// dem Zeilenumbruch bei der angegebenen Breite. Wichtig für ScaleToFit: Die
    /// Zeilenhöhe muss zur tatsächlichen Spaltenbreite passen, nicht zu einem
    /// hartkodierten Standardwert.
    /// </summary>
    public Size ContentSizeAtWidth(string content, TranslationType translate, int canvasWidth) {
        var key = TextSizeKey(_lastCode, content) + "|w" + canvasWidth;

        return SizesAtWidth.GetOrAdd(key, _ => CalculateContentSizeAtWidth(content, translate, canvasWidth));
    }

    public abstract void Draw(Graphics gr, string content, RowItem? affectingRow, Rectangle drawingAreaControl, TranslationType translate, Alignment align, float zoom, Design design, States state);

    public abstract List<GenericControl> GetProperties(int widthOfControl);

    public override List<string> ParseableItems() {
        List<string> result = [.. base.ParseableItems()];

        return result;
    }

    public override void ParseFinished(string parsed) {
        base.ParseFinished(parsed);
        _lastCode = ParseableItems().FinishParseable();
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "style":
                // Ignoriert — SheetStyle wird vom übergeordneten Element (z.B. TableView) gesetzt
                return true;
        }
        return true;   // Immer true. So kann gefahrlos hin und her geschaltet werden und evtl. Werte aus anderen Renderen benutzt werden.
    }

    public abstract string ReadableText();

    public abstract QuickImage? SymbolForReadableText();

    public string ValueReadable(string content, ShortenStyle style, TranslationType translate) {
        if (string.IsNullOrEmpty(content)) { return string.Empty; }

        var key = (int)style + "|" + (int)translate + "|" + TextSizeKey(_lastCode, content);

        return Replaced.GetOrAdd(key, _ => CalculateValueReadable(content, style, translate));
    }

    protected abstract Size CalculateContentSize(string content, TranslationType doOpticalTranslation);

    /// <summary>
    /// Berechnet die Inhaltsgröße bei einer konkreten Breite. Die Standard-
    /// Implementierung ignoriert die Breite und delegiert an
    /// <see cref="CalculateContentSize"/>. Mehrzeilige Renderer überschreiben dies,
    /// um die Höhe abhängig vom Zeilenumbruch zu berechnen.
    /// </summary>
    protected virtual Size CalculateContentSizeAtWidth(string content, TranslationType doOpticalTranslation, int canvasWidth) {
        return CalculateContentSize(content, doOpticalTranslation);
    }

    protected abstract string CalculateValueReadable(string content, ShortenStyle style, TranslationType doOpticalTranslation);

    protected BlueFont GetFont() {
        _font ??= Skin.GetBlueFont(SheetStyle, PadStyles.Standard);
        return _font;
    }

    protected BlueFont GetFont(float additionalScale) => Math.Abs(1 - additionalScale) < Constants.DefaultTolerance ? GetFont() : GetFont().Scale(additionalScale);

    protected BlueFont GetFont(float additionalScale, Design design, States state) {
        var baseFont = GetFont(additionalScale);
        if (state == States.Standard) { return baseFont; }
        var skinFont = Skin.GetBlueFont(design, state);
        return BlueFont.Get(baseFont.FontName, baseFont.Size, baseFont.Bold, baseFont.Italic, baseFont.Underline, baseFont.StrikeOut, skinFont.ColorMain, baseFont.ColorOutline, baseFont.ColorBack);
    }

    protected void OnDoUpdateSideOptionMenu() => DoUpdateSideOptionMenu?.Invoke(this, System.EventArgs.Empty);

    protected override void OnPropertyChanged([CallerMemberName] string propertyName = "unknown") {
        _lastCode = ParseableItems().FinishParseable();
        base.OnPropertyChanged(propertyName);
    }

    /// <summary>
    /// Ändert die anderen Zeilen dieser Spalte, so dass der verknüpfte Text bei dieser und den anderen Spalten gleich ist, ab.
    /// </summary>
    /// <param name="renderer"></param>
    /// <param name="content"></param>
    private static string TextSizeKey(string renderer, string content) => renderer + "|" + content;

    #endregion
}
// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Classes;
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
    private string _lastCode = "?";
    private string _sheetStyle = Constants.Win11;
    private BlueFont? _font;

    #endregion

    #region Constructors

    protected Renderer_Abstract() : this(false) { }

    protected Renderer_Abstract(bool readOnly) => ReadOnly = readOnly;

    #endregion

    #region Events

    public event EventHandler? DoUpdateSideOptionMenu;

    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion

    #region Properties

    public abstract string Description { get; }
    public string QuickInfo => Description;
    public bool ReadOnly { get; }

    public string SheetStyle {
        get => _sheetStyle;
        set {
            if (_sheetStyle == value) { return; }
            if (ReadOnly) { Develop.DebugPrint_ReadOnly(); return; }
            _sheetStyle = value;
            _font = null;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Methods

    public Size ContentSize(string content, TranslationType translate) {
        //if (string.IsNullOrEmpty(content)) { return CanvasSize.Empty; }

        var key = TextSizeKey(_lastCode, content);

        return Sizes.GetOrAdd(key, _ => CalculateContentSize(content, translate));
    }

    public abstract void Draw(Graphics gr, string content, RowItem? affectingRow, Rectangle drawingAreaControl, TranslationType translate, Alignment align, float zoom, Design design, States state);

    public abstract List<GenericControl> GetProperties(int widthOfControl);

    public override List<string> ParseableItems() {
        List<string> result = [.. base.ParseableItems()];
        result.ParseableAdd("Style", _sheetStyle);

        return result;
    }

    public override void ParseFinished(string parsed) {
        base.ParseFinished(parsed);
        _lastCode = ParseableItems().FinishParseable();
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "style":
                _sheetStyle = value.FromNonCritical();
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

    protected abstract string CalculateValueReadable(string content, ShortenStyle style, TranslationType doOpticalTranslation);

    protected BlueFont GetFont() {
        _font ??= Skin.GetBlueFont(_sheetStyle, PadStyles.Standard);
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

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "unknown") {
        _lastCode = ParseableItems().FinishParseable();
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Ändert die anderen Zeilen dieser Spalte, so dass der verknüpfte Text bei dieser und den anderen Spalten gleich ist, ab.
    /// </summary>
    /// <param name="renderer"></param>
    /// <param name="content"></param>
    private static string TextSizeKey(string renderer, string content) => renderer + "|" + content;

    #endregion
}

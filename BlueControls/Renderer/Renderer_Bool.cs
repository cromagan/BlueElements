// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes;
using BlueControls.Classes.ItemCollectionList;
using BlueControls.Controls;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Renderer;

public class Renderer_Bool : Renderer_Abstract {

    #region Properties

    public static string ClassId => "Bool";

    public override string Description => "Stellt boolesche Werte (+, -, o/O) als Symbol und/oder Text dar.";

    public string Symbol_False {
        get;
        set {
            if (field == value) { return; }
            if (ReadOnly) { Develop.DebugPrint_ReadOnly(); return; }
            field = value;
            OnPropertyChanged();
        }
    } = "Kreuz";

    public string Symbol_neutral {
        get;
        set {
            if (field == value) { return; }
            if (ReadOnly) { Develop.DebugPrint_ReadOnly(); return; }
            field = value;
            OnPropertyChanged();
        }
    } = "Kreis2";

    public string Symbol_True {
        get;
        set {
            if (field == value) { return; }
            if (ReadOnly) { Develop.DebugPrint_ReadOnly(); return; }
            field = value;
            OnPropertyChanged();
        }
    } = "Häkchen";

    public string Text_False {
        get;
        set {
            if (field == value) { return; }
            if (ReadOnly) { Develop.DebugPrint_ReadOnly(); return; }
            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    public string Text_neutral {
        get;
        set {
            if (field == value) { return; }
            if (ReadOnly) { Develop.DebugPrint_ReadOnly(); return; }
            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    public string Text_True {
        get;
        set {
            if (field == value) { return; }
            if (ReadOnly) { Develop.DebugPrint_ReadOnly(); return; }
            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    #endregion

    #region Methods

    public static List<AbstractListItem> SymboleFalse() => [
        ItemOf("Kreuz", ImageCode.Kreuz),
        ItemOf("MinusZeichen", ImageCode.MinusZeichen),
        ItemOf("MinusZeichen2", ImageCode.MinusZeichen2),
        ItemOf("SmileyUnhappy", ImageCode.SmileyUnhappy),
        ItemOf("Papierkorb", ImageCode.Papierkorb),
        ItemOf("Stop", ImageCode.Stop),
        ItemOf("CheckBox", ImageCode.CheckBox),
        ItemOf("OptionBox", ImageCode.OptionBox)
    ];

    public static List<AbstractListItem> SymboleNeutral() => [
        ItemOf("Kreis", ImageCode.Kreis),
        ItemOf("Kreis2", ImageCode.Kreis2),
        ItemOf("Fragezeichen", ImageCode.Fragezeichen),
        ItemOf("Frage", ImageCode.Frage),
        ItemOf("Pause", ImageCode.Pause)
    ];

    public static List<AbstractListItem> SymboleTrue() => [
        ItemOf("Häkchen", ImageCode.Häkchen),
        ItemOf("HäkchenDoppelt", ImageCode.HäkchenDoppelt),
        ItemOf("PlusZeichen", ImageCode.PlusZeichen),
        ItemOf("PlusZeichen2", ImageCode.PlusZeichen2),
        ItemOf("Smiley", ImageCode.Smiley),
        ItemOf("Stern", ImageCode.Stern),
        ItemOf("CheckBox_Checked", ImageCode.CheckBox_Checked),
        ItemOf("OptionBox_Checked", ImageCode.OptionBox_Checked)
    ];

    public static List<AbstractListItem> TexteFalse() => [
        ItemOf("Nein"),
        ItemOf("Falsch"),
        ItemOf("Nein danke"),
        ItemOf("Nicht"),
        ItemOf("-")
    ];

    public static List<AbstractListItem> TexteNeutral() => [
        ItemOf("—"),
        ItemOf("o"),
        ItemOf("Unbekannt"),
        ItemOf("Vielleicht")
    ];

    public static List<AbstractListItem> TexteTrue() => [
        ItemOf("Ja"),
        ItemOf("Wahr"),
        ItemOf("Richtig"),
        ItemOf("OK")
    ];

    public override void Draw(Graphics gr, string content, RowItem? affectingRow, Rectangle drawingAreaControl, TranslationType translate, Alignment align, float zoom, Design design, States state) {
        if (string.IsNullOrEmpty(content)) { return; }

        var pix = 16.CanvasToControl(zoom);
        var (symbol, text) = Resolve(content);

        var image = GetSymbol(symbol, pix);
        Skin.Draw_FormatedText(gr, text, image, align, drawingAreaControl, GetFont(zoom, design, state), false);
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<GenericControl> result = [
            new FlexiControlForProperty<string>(() => Symbol_True, SymboleTrue(), true),
            new FlexiControlForProperty<string>(() => Symbol_False, SymboleFalse(), true),
            new FlexiControlForProperty<string>(() => Symbol_neutral, SymboleNeutral(), true),
            new FlexiControlForProperty<string>(() => Text_True, TexteTrue(), true),
            new FlexiControlForProperty<string>(() => Text_False, TexteFalse(), true),
            new FlexiControlForProperty<string>(() => Text_neutral, TexteNeutral(), true)
        ];
        return result;
    }

    public override List<string> ParseableItems() {
        List<string> result = [.. base.ParseableItems()];
        result.ParseableAdd("SymbolTrue", Symbol_True);
        result.ParseableAdd("TextTrue", Text_True);
        result.ParseableAdd("SymbolNeutral", Symbol_neutral);
        result.ParseableAdd("TextNeutral", Text_neutral);
        result.ParseableAdd("SymbolFalse", Symbol_False);
        result.ParseableAdd("TextFalse", Text_False);
        return result;
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "symboltrue":
                Symbol_True = value.FromNonCritical();
                return true;

            case "symbolfalse":
                Symbol_False = value.FromNonCritical();
                return true;

            case "symbolneutral":
                Symbol_neutral = value.FromNonCritical();
                return true;

            case "texttrue":
                Text_True = value.FromNonCritical();
                return true;

            case "textfalse":
                Text_False = value.FromNonCritical();
                return true;

            case "textneutral":
                Text_neutral = value.FromNonCritical();
                return true;
        }
        return base.ParseThis(key, value);
    }

    public override string ReadableText() => "Bool / Ja-Nein";

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Häkchen);

    protected override Size CalculateContentSize(string content, TranslationType doOpticalTranslation) {
        var replacedText = ValueReadable(content, ShortenStyle.Replaced, doOpticalTranslation);
        var (symbol, _) = Resolve(content);
        var image = GetSymbol(symbol, 16);
        return GetFont().FormatedText_NeededSize(replacedText, image, 16);
    }

    protected override string CalculateValueReadable(string content, ShortenStyle style, TranslationType doOpticalTranslation) {
        var (_, text) = Resolve(content);
        return text;
    }

    private static QuickImage? GetSymbol(string symbolName, int size) {
        if (string.IsNullOrEmpty(symbolName)) { return null; }
        if (Enum.TryParse(symbolName, true, out ImageCode imgCode)) {
            return QuickImage.Get(imgCode, size);
        }
        var qi = QuickImage.Get(symbolName);
        return qi.IsError ? null : qi;
    }

    private (string Symbol, string Text) Resolve(string content) {
        switch (content) {
            case "+":
                return (Symbol_True, Text_True);

            case "-":
                return (Symbol_False, Text_False);

            case "o":
            case "O":
                return (Symbol_neutral, Text_neutral);

            default:
                return (string.Empty, content);
        }
    }

    #endregion
}
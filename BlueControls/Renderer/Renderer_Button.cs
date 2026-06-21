// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes;
using BlueControls.Controls;

namespace BlueControls.Renderer;

public class Renderer_Button : Renderer_Abstract {

    #region Fields

    private bool _bild_anzeigen;

    private bool _checkstatus_anzeigen;

    private int _padding = Skin.PaddingSmal;

    private bool _text_anzeigen;

    #endregion

    #region Properties

    public static string ClassId => "Button";

    public bool Bild_anzeigen {
        get => _bild_anzeigen;
        set {
            if (_bild_anzeigen == value) { return; }
            if (ReadOnly) { Develop.DebugPrint_ReadOnly(); return; }
            _bild_anzeigen = value;
            OnPropertyChanged();
        }
    }

    public bool CheckStatus_anzeigen {
        get => _checkstatus_anzeigen;
        set {
            if (_checkstatus_anzeigen == value) { return; }
            if (ReadOnly) { Develop.DebugPrint_ReadOnly(); return; }
            _checkstatus_anzeigen = value;
            OnPropertyChanged();
        }
    }

    public override string Description => "Stellt den Inhalt als Schaltfläche dar.\r\nFormat: checked(+/-);BildCode;Text";

    /// <summary>
    /// Innenabstand des Buttons zur Zelle. 0 = zellfüllend.
    /// </summary>
    public int Padding {
        get => _padding;
        set {
            if (_padding == value) { return; }
            if (ReadOnly) { Develop.DebugPrint_ReadOnly(); return; }
            if (value < 0) { value = 0; }
            _padding = value;
            OnPropertyChanged();
        }
    }

    public bool Text_anzeigen {
        get => _text_anzeigen;
        set {
            if (_text_anzeigen == value) { return; }
            if (ReadOnly) { Develop.DebugPrint_ReadOnly(); return; }
            _text_anzeigen = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Methods

    public override void Draw(Graphics gr, string content, RowItem? affectingRow, Rectangle drawingAreaControl, TranslationType translate, Alignment align, float zoom, Design design, States state) {
        if (string.IsNullOrEmpty(content)) { return; }

        var s = States.Standard;

        if (_checkstatus_anzeigen) {
            var t = (content + ";").SplitBy(";");
            if (_bild_anzeigen && t[1].FromPlusMinus()) {
                s |= States.Checked;
            } else if (!_bild_anzeigen && t[0].FromPlusMinus()) {
                s |= States.Checked;
            }
        }

        var replacedText = ValueReadable(content, ShortenStyle.Replaced, translate);
        var q = QImage(content);

        drawingAreaControl.Inflate(-_padding, -_padding);

        Button.DrawButton(null, gr, Design.Button_CheckBox, s, q, Alignment.Horizontal_Vertical_Center, false, null, replacedText, drawingAreaControl, true);
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<GenericControl> result =
        [   new FlexiControlForProperty<bool>(() => Bild_anzeigen),
            new FlexiControlForProperty<bool>(() => CheckStatus_anzeigen),
                new FlexiControlForProperty<bool>(() => Text_anzeigen),
            new FlexiControlForProperty<int>(() => Padding)
        ];
        return result;
    }

    public override List<string> ParseableItems() {
        List<string> result = [.. base.ParseableItems()];

        result.ParseableAdd("ShowPic", _bild_anzeigen);
        result.ParseableAdd("ShowText", _text_anzeigen);
        result.ParseableAdd("ShowCheckState", _checkstatus_anzeigen);
        result.ParseableAdd("Padding", _padding);
        return result;
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "showpic":
                _bild_anzeigen = value.FromPlusMinus();
                return true;

            case "showtext":
                _text_anzeigen = value.FromPlusMinus();
                return true;

            case "showcheckstate":
                _checkstatus_anzeigen = value.FromPlusMinus();
                return true;

            case "padding":
                _padding = Math.Max(0, Converter.IntParse(value));
                return true;
        }
        return base.ParseThis(key, value);
    }

    public override string ReadableText() => "Als Schaltfläche anzeigen";

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Schaltfläche);

    protected override Size CalculateContentSize(string content, TranslationType doOpticalTranslation) {
        var replacedText = ValueReadable(content, ShortenStyle.Replaced, doOpticalTranslation);

        return GetFont().FormatedText_NeededSize(replacedText, QImage(content), 32);
    }

    /// <summary>
    /// Gibt eine einzelne Zeile richtig ersetzt mit Prä- und Suffix zurück.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="style"></param>
    /// <param name="doOpticalTranslation"></param>
    /// <returns></returns>
    protected override string CalculateValueReadable(string content, ShortenStyle style, TranslationType doOpticalTranslation) {
        if (!_text_anzeigen) { return string.Empty; }

        content = content.Replace("\r\n", "; ");
        content = content.Replace("\r", "; ");

        var t = (content + ";;").SplitBy(";");

        string r;

        if (_bild_anzeigen && _checkstatus_anzeigen) {
            r = t[2];
        } else if (!_bild_anzeigen && !_checkstatus_anzeigen) {
            r = t[0];
        } else {
            r = t[1];
        }

        return LanguageTool.PrepaireText(r, style, string.Empty, string.Empty, doOpticalTranslation, null);
    }

    private QuickImage? QImage(string content) {
        var t = (content + ";;;").SplitBy(";");

        return _bild_anzeigen ? QuickImage.Get(t[0]) : null;
    }

    #endregion
}
// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;
using Padding = System.Windows.Forms.Padding;

namespace BlueControls.Renderer;

public class ButtonRenderer : Renderer {

    #region Fields

    private Padding _padding = new(Skin.PaddingSmal);

    #endregion

    #region Properties

    public static string ClassId => "Button";

    public override string Description => "Stellt den Zelleninhalt als Schaltfläche dar.\r\nIst ShowCellValue aktiv, wird der Zelltext unverändert angezeigt. Sonst wird der Inhalt als Ja/Nein-Wert (+, Wahr, True) interpretiert.";

    /// <summary>
    /// Zeigt den Zellinhalt unverändert als Text ohne Bild und ungecheckt an.
    /// </summary>
    public bool ShowCellValue {
        get;
        set {
            if (field == value) { return; }
            if (ReadOnly) { Develop.DebugPrint_ReadOnly(); return; }
            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Bildcode, der angezeigt wird, wenn der Zelleninhalt als TRUE interpretiert werden kann.
    /// </summary>
    public string PictureTrue {
        get;
        set {
            if (field == value) { return; }
            if (ReadOnly) { Develop.DebugPrint_ReadOnly(); return; }
            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    /// <summary>
    /// Text, der angezeigt wird, wenn der Zelleninhalt als TRUE interpretiert werden kann.
    /// </summary>
    public string TextTrue {
        get;
        set {
            if (field == value) { return; }
            if (ReadOnly) { Develop.DebugPrint_ReadOnly(); return; }
            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    /// <summary>
    /// Bildcode, der angezeigt wird, wenn der Zelleninhalt nicht als TRUE interpretiert werden kann.
    /// </summary>
    public string PictureFalse {
        get;
        set {
            if (field == value) { return; }
            if (ReadOnly) { Develop.DebugPrint_ReadOnly(); return; }
            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    /// <summary>
    /// Text, der angezeigt wird, wenn der Zelleninhalt nicht als TRUE interpretiert werden kann.
    /// </summary>
    public string TextFalse {
        get;
        set {
            if (field == value) { return; }
            if (ReadOnly) { Develop.DebugPrint_ReadOnly(); return; }
            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    /// <summary>
    /// Zeigt bei leerer Zelle die Schaltfläche im FALSE-Zustand, statt nichts zu zeichnen.
    /// </summary>
    public bool NoValuesShowFalse {
        get;
        set {
            if (field == value) { return; }
            if (ReadOnly) { Develop.DebugPrint_ReadOnly(); return; }
            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Innenabstand des Buttons zur Zelle. Negative Werte vergrößern die Zeichenfläche
    /// über die Zelle hinaus, z. B. um das Zeilen-Padding {4, 2} zu negieren: {-4, -2}.
    /// </summary>
    public Padding Padding {
        get => _padding;
        set {
            if (_padding == value) { return; }
            if (ReadOnly) { Develop.DebugPrint_ReadOnly(); return; }
            _padding = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Methods

    public override void Draw(Graphics gr, string content, RowItem? affectingRow, Rectangle drawingAreaControl, TranslationType translate, Alignment align, float zoom, Design design, States state) {
        if (string.IsNullOrEmpty(content)) {
            if (ShowCellValue || !NoValuesShowFalse) { return; }

            // Leere Zelle als FALSE-Wert behandeln
            content = "-";
        }

        var s = !ShowCellValue && IsTrueValue(content) ? States.Checked : States.Standard;

        var replacedText = ValueReadable(content, ShortenStyle.Replaced, translate);
        var q = QImage(content);

        // Positive Paddings ignorieren — nur negative Werte wirken und
        // vergrößern die Zeichenfläche über die Zelle hinaus.
        var padLeft = Math.Min(0, _padding.Left).CanvasToControl(zoom);
        var padTop = Math.Min(0, _padding.Top).CanvasToControl(zoom);
        var padRight = Math.Min(0, _padding.Right).CanvasToControl(zoom);
        var padBottom = Math.Min(0, _padding.Bottom).CanvasToControl(zoom);

        drawingAreaControl = new Rectangle(
            drawingAreaControl.X + padLeft,
            drawingAreaControl.Y + padTop,
            drawingAreaControl.Width - padLeft - padRight,
            drawingAreaControl.Height - padTop - padBottom);

        Button.DrawButton(null, gr, Design.Button_CheckBox, s, q, Alignment.Horizontal_Vertical_Center, false, null, replacedText, drawingAreaControl, true);
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<GenericControl> result = [new FlexiControlForProperty<bool>(() => ShowCellValue)];

        if (!ShowCellValue) {
            result.Add(new FlexiControlForProperty<string>(() => PictureTrue));
            result.Add(new FlexiControlForProperty<string>(() => TextTrue));
            result.Add(new FlexiControlForProperty<string>(() => PictureFalse));
            result.Add(new FlexiControlForProperty<string>(() => TextFalse));
            result.Add(new FlexiControlForProperty<bool>(() => NoValuesShowFalse));
        }

        result.Add(new FlexiControlForProperty<Padding>(() => Padding));
        return result;
    }

    public override List<string> ParseableItems() {
        List<string> result = [.. base.ParseableItems()];

        result.ParseableAdd("ShowCellValue", ShowCellValue);
        result.ParseableAdd("PictureTrue", PictureTrue);
        result.ParseableAdd("TextTrue", TextTrue);
        result.ParseableAdd("PictureFalse", PictureFalse);
        result.ParseableAdd("TextFalse", TextFalse);
        result.ParseableAdd("NoValuesShowFalse", NoValuesShowFalse);
        result.ParseableAdd("Padding", _padding);
        return result;
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "showcellvalue":
                ShowCellValue = value.FromPlusMinus();
                return true;

            case "picturetrue":
                PictureTrue = value.FromNonCritical();
                return true;

            case "texttrue":
                TextTrue = value.FromNonCritical();
                return true;

            case "picturefalse":
                PictureFalse = value.FromNonCritical();
                return true;

            case "textfalse":
                TextFalse = value.FromNonCritical();
                return true;

            case "novaluesshowfalse":
                NoValuesShowFalse = value.FromPlusMinus();
                return true;

            case "padding":
                _padding = value.PaddingParse();
                return true;
        }
        return base.ParseThis(key, value);
    }

    public override string ReadableText() => "Als Schaltfläche anzeigen";

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Schaltfläche);

    protected override Size CalculateContentSize(string content, TranslationType doOpticalTranslation) {
        if (!ShowCellValue && NoValuesShowFalse && string.IsNullOrEmpty(content)) { content = "-"; }

        var replacedText = ValueReadable(content, ShortenStyle.Replaced, doOpticalTranslation);

        // Mindesthöhe: 16 Pixel Button-Inhalt plus umgekehrtes vertikales Padding
        // (nur negative Werte wirken, Positive werden ignoriert).
        var minSize = Math.Max(1, 16 - Math.Min(0, _padding.Top) - Math.Min(0, _padding.Bottom));
        return GetFont().FormatedText_NeededSize(replacedText, QImage(content), minSize);
    }

    protected override string CalculateValueReadable(string content, ShortenStyle style, TranslationType doOpticalTranslation) {
        if (ShowCellValue) {
            var v = content.Replace("\r\n", "; ").Replace("\r", "; ");
            return LanguageTool.PrepaireText(v, style, string.Empty, string.Empty, doOpticalTranslation, null);
        }

        var r = IsTrueValue(content) ? TextTrue : TextFalse;
        return LanguageTool.PrepaireText(r, style, string.Empty, string.Empty, doOpticalTranslation, null);
    }

    /// <summary>
    /// Liefert true, wenn der Zelleninhalt als TRUE interpretiert werden kann.
    /// </summary>
    private static bool IsTrueValue(string content) => content.ToLowerInvariant() is "+" or "wahr" or "true";

    private QuickImage? QImage(string content) {
        if (ShowCellValue) { return null; }

        var code = IsTrueValue(content) ? PictureTrue : PictureFalse;

        return code is { Length: > 0 } ? QuickImage.Get(code) : null;
    }

    #endregion
}
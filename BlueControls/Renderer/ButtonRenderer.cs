// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueControls.Controls;
using Padding = System.Windows.Forms.Padding;

namespace BlueControls.Renderer;

/// <summary>
/// Stellt den Zelleninhalt als Schaltfläche dar.
/// Ist ShowCellValue aktiv, wird der Zelltext unverändert angezeigt. Sonst wird der Inhalt als Ja/Nein-Wert (+, Wahr, True) interpretiert.
/// </summary>
public class ButtonRenderer : Renderer {

    #region Fields

    private Padding _padding = new(Skin.PaddingSmal);

    #endregion

    #region Properties

    public static string ClassId => "Button";

    /// <summary>
    /// Wenn gewählt, wird der Inhalt der Zelle unverändert als Text angezeigt.
    /// Wenn nicht gewählt, wird der Inhalt als Ja/Nein-Angabe dargestellt.
    /// </summary>
    public bool ShowCellValue {
        get;
        set {
            if (field == value) { return; }
            if (ReadOnly) { Develop.DebugPrint_ReadOnly(); return; }
            field = value;
            OnPropertyChanged();
            OnDoUpdateSideOptionMenu();
        }
    }

    /// <summary>
    /// Bild auf dem Knopf, wenn der Wert „Ja" ist.
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
    /// Aufschrift des Knopfes, wenn der Wert „Ja" ist.
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
    /// Bild auf dem Knopf, wenn der Wert „Nein" ist.
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
    /// Aufschrift des Knopfes, wenn der Wert „Nein" ist.
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
    /// Wenn gewählt, wird bei einer leeren Zelle der Knopf im Zustand „Nein" angezeigt.
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
    /// Abstand zwischen dem Knopf und dem Zellenrand in Pixel.<br />
    /// Möglich sind 1 Wert (alle Seiten), 2 Werte (links/rechts, oben/unten) oder 4 Werte (links, oben, rechts, unten), z. B. {-4, -2}.<br />
    /// Negative Werte vergrößern den Knopf, gezeichnet wird dabei jedoch nie über den Zellrand hinaus.<br />
    /// Positive Werte haben keine Wirkung.
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

        // Positive Paddings ignorieren — nur negative Werte wirken und vergrößern die Knopffläche.
        var padLeft = Math.Min(0, _padding.Left).CanvasToControl(zoom);
        var padTop = Math.Min(0, _padding.Top).CanvasToControl(zoom);
        var padRight = Math.Min(0, _padding.Right).CanvasToControl(zoom);
        var padBottom = Math.Min(0, _padding.Bottom).CanvasToControl(zoom);

        var buttonArea = new Rectangle(
            drawingAreaControl.X + padLeft,
            drawingAreaControl.Y + padTop,
            drawingAreaControl.Width - padLeft - padRight,
            drawingAreaControl.Height - padTop - padBottom);

        // Auch bei negativen Paddings niemals über den Zellrand zeichnen.
        var clipState = gr.Save();
        gr.SetClip(drawingAreaControl, CombineMode.Intersect);
        Button.DrawButton(null, gr, Design.Button_CheckBox, s, q, Alignment.Horizontal_Vertical_Center, false, null, replacedText, buttonArea, true);
        gr.Restore(clipState);
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

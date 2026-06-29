// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes.ItemCollectionList;
using BlueControls.Controls;
using System.Reflection;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Classes;

public static class Skin {

    #region Fields

    public const int Padding = 9;
    public const int PaddingMedium = 6;
    public const int PaddingSmal = 3;
    public const float Scale = 1.0f;
    internal static Pen PenLinieDick = Pens.Red;
    internal static Pen PenLinieDünn = Pens.Red;
    internal static Pen PenLinieKräftig = Pens.Red;
    private static readonly ConcurrentCache<string, BlueFont> _fontCache = new(500);
    private static readonly Dictionary<Design, Dictionary<States, SkinDesign>> Design = [];
    private static readonly ImageCodeEffect[] St = new ImageCodeEffect[1];
    private static Dictionary<string, Dictionary<int, string>> _styleData = [];
    private static bool _stylesLoaded;

    #endregion

    #region Properties

    public static bool HasStyles {
        get {
            InitStyles();
            return _styleData.Count > 0;
        }
    }

    public static bool Inited { get; private set; }

    #endregion

    #region Methods

    public static ImageCodeEffect AdditionalState(States state) => state.HasFlag(States.Standard_Disabled) ? St[0] : ImageCodeEffect.None;

    public static List<string>? AllStyles() {
        InitStyles();
        return [.. _styleData.Keys];
    }

    public static Color Color_Back(Design vDesign, States vState) => DesignOf(vDesign, vState).BackColor1;

    public static SkinDesign DesignOf(Design design, States state) {
        try {
            return Design[design][state];
        } catch {
            var d = new SkinDesign() {
                BackColor1 = Color.White,
                BorderColor1 = Color.Red,
                Font = BlueFont.DefaultFont,
                BackgroundStyle = BackgroundStyle.Solid,
                BorderStyle = BorderStyle.Solid1Px,
                Contour = Contour.Rectangle
            };
            return d;
        }
    }

    public static void Draw_Back(Graphics gr, Design design, States state, Rectangle r, System.Windows.Forms.Control? control, bool needTransparenz) => Draw_Back(gr, DesignOf(design, state), r, control, needTransparenz);

    public static void Draw_Back(Graphics gr, SkinDesign design, Rectangle r, System.Windows.Forms.Control? control, bool needTransparenz) {
        try {
            if (MustDrawTransparence(design, needTransparenz)) { Draw_Back_Transparent(gr, r, control); }

            if (design.BackgroundStyle == BackgroundStyle.None || design.Contour == Contour.None) { return; }

            r = Rectangle.FromLTRB(r.Left - design.X1, r.Top - design.Y1, r.Right + design.X2, r.Bottom + design.Y2);

            if (r.Width < 1 || r.Height < 1) { return; }

            gr.TranslateTransform(r.X, r.Y);
            try {
                var lr = new Rectangle(0, 0, r.Width, r.Height);
                switch (design.BackgroundStyle) {
                    case BackgroundStyle.Solid:
                        BackgroundFill.Solid(gr, design.Contour, lr, design.BackColor1);
                        break;

                    case BackgroundStyle.GradientVertical:
                        BackgroundFill.GradientVertical(gr, design.Contour, lr, design.BackColor1, design.BackColor2);
                        break;

                    case BackgroundStyle.GradientVertical3:
                        BackgroundFill.GradientVertical3(gr, design.Contour, lr, design.BackColor1, design.BackColor2, design.BackColor3, design.GradientMidpoint);
                        break;

                    case BackgroundStyle.GradientHorizontal:
                        BackgroundFill.GradientHorizontal(gr, design.Contour, lr, design.BackColor1, design.BackColor2);
                        break;

                    case BackgroundStyle.GradientHorizontal3:
                        BackgroundFill.GradientHorizontal3(gr, design.Contour, lr, design.BackColor1, design.BackColor2, design.BackColor3, design.GradientMidpoint);
                        break;

                    case BackgroundStyle.GradientDiagonal:
                        BackgroundFill.GradientDiagonal(gr, design.Contour, lr, design.BackColor1, design.BackColor2, design.BackColor3, design.GradientMidpoint);
                        break;

                    case BackgroundStyle.Glossy:
                        BackgroundFill.Glossy(gr, design.Contour, lr, design.BackColor1, design.BackColor2);
                        break;

                    case BackgroundStyle.GlossyPressed:
                        BackgroundFill.GlossyPressed(gr, design.Contour, lr, design.BackColor1, design.BackColor2);
                        break;

                    case BackgroundStyle.GradientVerticalHighlight:
                        BackgroundFill.GradientVerticalHighlight(gr, design.Contour, lr, design.BackColor1, design.BackColor2, design.GradientMidpoint);
                        break;

                    case BackgroundStyle.None:
                    case BackgroundStyle.Undefined:
                        break;

                    default:
                        Develop.DebugPrint(design.BackgroundStyle);
                        break;
                }
            } finally {
                gr.TranslateTransform(-r.X, -r.Y);
            }
        } catch (Exception ex) {
            Develop.DebugPrint("Fehler beim Zeichnen des Skins:" + design, ex);
        }
    }

    public static void Draw_Back_Transparent(Graphics gr, Rectangle r, System.Windows.Forms.Control? control) {
        if (control?.Parent is null) { return; }

        switch (control.Parent) {
            case IBackgroundNone:
                Draw_Back_Transparent(gr, r, control.Parent);
                break;

            case GenericControl trb:
                if (trb.BitmapOfControl() is not { } bmp) {
                    gr.FillRectangle(BlueFont.GetBrush(control.Parent.BackColor), r);
                    return;
                }
                gr.DrawImage(bmp, r, r with { X = control.Left + r.Left, Y = control.Top + r.Top }, GraphicsUnit.Pixel);
                break;

            case Form frm:
                gr.FillRectangle(BlueFont.GetBrush(frm.BackColor), r);
                break;

            case System.Windows.Forms.SplitContainer:
                Draw_Back_Transparent(gr, r, control.Parent);
                break;

            case System.Windows.Forms.SplitterPanel:
                Draw_Back_Transparent(gr, r, control.Parent);
                break;

            case System.Windows.Forms.TableLayoutPanel:
                Draw_Back_Transparent(gr, r, control.Parent);
                break;

            case System.Windows.Forms.TabPage:
                gr.FillRectangle(BlueFont.GetBrush(control.Parent.BackColor), r);
                break;

            case System.Windows.Forms.Panel:
                Draw_Back_Transparent(gr, r, control.Parent);
                break;

            default:
                gr.FillRectangle(BlueFont.GetBrush(control.Parent.BackColor), r);
                break;
        }
    }

    public static void Draw_Border(Graphics gr, Design vDesign, States vState, Rectangle r) => Draw_Border(gr, DesignOf(vDesign, vState), r);

    public static void Draw_Border(Graphics gr, SkinDesign design, Rectangle r) {
        if (design.Contour == Contour.None || design.BorderStyle == BorderStyle.None) { return; }

        if (design.Contour == Contour.Undefined) {
            design.Contour = Contour.Rectangle;
            r.Width--;
            r.Height--;
        } else {
            r = Rectangle.FromLTRB(
                r.Left - design.X1,
                r.Top - design.Y1,
                r.Right + design.X2 - 1,
                r.Bottom + design.Y2 - 1
            );
        }

        if (r.Width < 1 || r.Height < 1) { return; }

        try {
            gr.TranslateTransform(r.X, r.Y);
            try {
                var lr = new Rectangle(0, 0, r.Width, r.Height);
                switch (design.BorderStyle) {
                    case BorderStyle.Solid1Px:
                        BorderDraw.Solid1Px(gr, design.Contour, lr, design.BorderColor1);
                        break;

                    case BorderStyle.Solid1PxDualColor:
                        BorderDraw.Solid1PxDualColor(gr, design.Contour, lr, design.BorderColor1, design.BorderColor2);
                        break;

                    case BorderStyle.Solid1PxFocusDot:
                        BorderDraw.Solid1PxFocusDot(gr, design.Contour, lr, design.BorderColor1, design.BorderColor2);
                        break;

                    case BorderStyle.FocusDot:
                        BorderDraw.FocusDot(gr, design.Contour, lr, design.BorderColor2);
                        break;

                    case BorderStyle.Solid3Px:
                        BorderDraw.Solid3Px(gr, design.Contour, lr, design.BorderColor1);
                        break;

                    case BorderStyle.Solid21Px:
                        BorderDraw.Solid21Px(gr, design.Contour, lr, design.BorderColor1);
                        break;

                    default:
                        Develop.DebugPrint(design.BorderStyle);
                        break;
                }
            } finally {
                gr.TranslateTransform(-r.X, -r.Y);
            }
        } catch {
            // PathX kann durch die ganzen Expand mal zu klein werden
        }
    }

    public static void Draw_FormatedText(Graphics gr, string txt, QuickImage? qi,
        Alignment align,
        Rectangle fitInRect, Design design, States state,
        System.Windows.Forms.Control? child, bool deleteBack, bool translate) {
        var sd = DesignOf(design, state);
        var img = qi is not null ? QuickImage.Get(qi, AdditionalState(sd.Status)) : null;
        Draw_FormatedText(gr, txt, img, align, fitInRect, child, deleteBack, sd.Font, translate);
    }

    public static void Draw_FormatedText(Graphics gr, string txt, QuickImage? qi, Alignment align, Rectangle fitInRect, BlueFont? bFont, bool translate) =>
        Draw_FormatedText(gr, txt, qi, align, fitInRect, null, false, bFont, translate);

    public static void Draw_FormatedText(Graphics gr, string txt, QuickImage? qi, Alignment align,
        Rectangle fitInRect, SkinDesign design,
        System.Windows.Forms.Control? child, bool deleteBack, bool translate) {
        if (string.IsNullOrEmpty(txt) && qi is null) { return; }
        var img = qi is not null ? QuickImage.Get(qi, AdditionalState(design.Status)) : null;
        Draw_FormatedText(gr, txt, img, align, fitInRect, child, deleteBack, design.Font, translate);
    }

    public static void Draw_FormatedText(Graphics gr, string txt, QuickImage? qi, Alignment align, Rectangle fitInRect, System.Windows.Forms.Control? child, bool deleteBack, BlueFont? bFont, bool translate) {
        if (string.IsNullOrEmpty(txt) && qi is null) { return; }

        try {
            var pSize = SizeF.Empty;
            if (qi is not null) { pSize = ((Bitmap)qi).Size; }

            if (LanguageTool.Translation is not null) { txt = LanguageTool.DoTranslate(txt, translate); }

            var tSize = SizeF.Empty;
            if (bFont is not null) {
                if (fitInRect.Width > 0) { txt = BlueFont.TrimByWidth(bFont, txt, fitInRect.Width - pSize.Width); }
                tSize = bFont.MeasureString(txt);
            }

            var xp = 0f;
            var yp1 = 0f;
            var yp2 = 0f;
            var totalW = pSize.Width + tSize.Width;
            if (align.HasFlag(Alignment.Right)) { xp = fitInRect.Width - totalW; }
            if (align.HasFlag(Alignment.HorizontalCenter)) { xp = (fitInRect.Width - totalW) / 2f; }
            if (align.HasFlag(Alignment.VerticalCenter)) {
                yp1 = (fitInRect.Height - pSize.Height) / 2f;
                yp2 = (fitInRect.Height - tSize.Height) / 2f;
            }
            if (align.HasFlag(Alignment.Bottom)) {
                yp1 = fitInRect.Height - pSize.Height;
                yp2 = fitInRect.Height - tSize.Height;
            }

            if (deleteBack) {
                if (child is not null) {
                    if (!string.IsNullOrEmpty(txt)) { Draw_Back_Transparent(gr, new Rectangle((int)(fitInRect.X + pSize.Width + xp - 1), (int)(fitInRect.Y + yp2 - 1), (int)(tSize.Width + 2), (int)(tSize.Height + 2)), child); }
                    if (qi is not null) { Draw_Back_Transparent(gr, new Rectangle((int)(fitInRect.X + xp), (int)(fitInRect.Y + yp1), (int)pSize.Width, (int)pSize.Height), child); }
                } else {
                    var c = BackgroundFill.DeleteBackBrush;
                    if (!string.IsNullOrEmpty(txt)) { gr.FillRectangle(c, new Rectangle((int)(fitInRect.X + pSize.Width + xp - 1), (int)(fitInRect.Y + yp2 - 1), (int)(tSize.Width + 2), (int)(tSize.Height + 2))); }
                    if (qi is not null) { gr.FillRectangle(c, new Rectangle((int)(fitInRect.X + xp), (int)(fitInRect.Y + yp1), (int)pSize.Width, (int)pSize.Height)); }
                }
            }

            if (qi is not null) { gr.DrawImageUnscaled(qi, (int)(fitInRect.X + xp), (int)(fitInRect.Y + yp1)); }
            if (!string.IsNullOrEmpty(txt)) { bFont?.DrawString(gr, txt, fitInRect.X + pSize.Width + xp, fitInRect.Y + yp2); }
        } catch {
            // Bitmap oder Graphics wird reentrant verwendet (WinForms Reentrancy)
        }
    }

    public static BlueFont GetBlueFont(Design design, States state) => DesignOf(design, state).Font;

    public static BlueFont GetBlueFont(string style, PadStyles format) {
        if (format == PadStyles.Undefined || string.IsNullOrEmpty(style)) { return BlueFont.DefaultFont; }

        var cacheKey = style + "|" + (int)format;
        return _fontCache.GetOrAdd(cacheKey, _ => {
            InitStyles();
            if (_styleData.TryGetValue(style, out var formats) && formats.TryGetValue((int)format, out var fontString)) {
                return BlueFont.Get(fontString);
            }
            return BlueFont.DefaultFont;
        });
    }

    public static List<AbstractListItem> GetFonts(string sheetStyle) {
        List<AbstractListItem> rahms =
        [
            ItemOf("Haupt-Überschrift", ((int)PadStyles.Title).ToString1(),
                GetBlueFont(sheetStyle, PadStyles.Title).SymbolForReadableText()),
            ItemOf("Untertitel für Haupt-Überschrift", ((int)PadStyles.Subtitle).ToString1(),
                GetBlueFont(sheetStyle, PadStyles.Subtitle).SymbolForReadableText()),
            ItemOf("Überschrift für Kapitel", ((int)PadStyles.Chapter).ToString1(),
                GetBlueFont(sheetStyle, PadStyles.Chapter).SymbolForReadableText()),
            ItemOf("Standard", ((int)PadStyles.Standard).ToString1(),
                GetBlueFont(sheetStyle, PadStyles.Standard).SymbolForReadableText()),
            ItemOf("Standard Fett", ((int)PadStyles.Emphasized).ToString1(),
                GetBlueFont(sheetStyle, PadStyles.Emphasized).SymbolForReadableText()),
            ItemOf("Standard Alternativ-Design", ((int)PadStyles.Alternative).ToString1(),
                GetBlueFont(sheetStyle, PadStyles.Alternative).SymbolForReadableText()),
            ItemOf("Kleiner Zusatz", ((int)PadStyles.Footnote).ToString1(),
                GetBlueFont(sheetStyle, PadStyles.Footnote).SymbolForReadableText())
        ];
        return rahms;
    }

    public static List<AbstractListItem> GetRahmenArt(string sheetStyle, bool mitOhne) {
        var rahms = new List<AbstractListItem>();
        if (mitOhne) {
            rahms.Add(ItemOf("Ohne Rahmen", ((int)PadStyles.Undefined).ToString1(), ImageCode.Kreuz));
        }
        rahms.Add(ItemOf("Haupt-Überschrift", ((int)PadStyles.Title).ToString1(), GetBlueFont(sheetStyle, PadStyles.Title).SymbolOfLine()));
        rahms.Add(ItemOf("Untertitel für Haupt-Überschrift", ((int)PadStyles.Subtitle).ToString1(), GetBlueFont(sheetStyle, PadStyles.Subtitle).SymbolOfLine()));
        rahms.Add(ItemOf("Überschrift für Kapitel", ((int)PadStyles.Chapter).ToString1(), GetBlueFont(sheetStyle, PadStyles.Chapter).SymbolOfLine()));
        rahms.Add(ItemOf("Standard", ((int)PadStyles.Standard).ToString1(), GetBlueFont(sheetStyle, PadStyles.Standard).SymbolOfLine()));
        rahms.Add(ItemOf("Standard Fett", ((int)PadStyles.Emphasized).ToString1(), GetBlueFont(sheetStyle, PadStyles.Emphasized).SymbolOfLine()));
        rahms.Add(ItemOf("Standard Alternativ-Design", ((int)PadStyles.Alternative).ToString1(), GetBlueFont(sheetStyle, PadStyles.Alternative).SymbolOfLine()));
        rahms.Add(ItemOf("Kleiner Zusatz", ((int)PadStyles.Footnote).ToString1(), GetBlueFont(sheetStyle, PadStyles.Footnote).SymbolOfLine()));
        return rahms;
    }

    public static Color IdColor(List<int>? id) => id is not { Count: not 0 } ? IdColor(-1) : IdColor(id[0]);

    public static Color IdColor(int id) {
        if (id < 0) { return Color.White; }

        switch (id % 10) {
            case 0:
                return Color.Red;

            case 1:
                return Color.Blue;

            case 2:
                return Color.Green;

            case 3:
                return Color.Yellow;

            case 4:
                return Color.Purple;

            case 5:
                return Color.Cyan;

            case 6:
                return Color.Orange;

            case 7:
                return Color.LightBlue;

            case 8:
                return Color.PaleVioletRed;

            case 9:
                return Color.LightGreen;

            default:
                return Color.Gray;
        }
    }

    public static void InitStyles() {
        if (_stylesLoaded) { return; }
        _stylesLoaded = true;

        try {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                try {
                    foreach (var resourceName in assembly.GetManifestResourceNames()) {
                        if (!resourceName.EndsWith(".json", StringComparison.OrdinalIgnoreCase)) { continue; }

                        var segments = resourceName.Split('.');
                        var inStylesFolder = false;
                        for (var i = 0; i < segments.Length - 2; i++) {
                            if (segments[i].Equals("Styles", StringComparison.OrdinalIgnoreCase)) {
                                inStylesFolder = true;
                                break;
                            }
                        }
                        if (!inStylesFolder) { continue; }

                        var styleName = segments[segments.Length - 2];
                        if (styleName.StartsWith("Skin", StringComparison.OrdinalIgnoreCase)) { continue; }

                        using var stream = assembly.GetManifestResourceStream(resourceName);
                        if (stream is null) { continue; }

                        using var doc = JsonDocument.Parse(stream);
                        var formats = new Dictionary<int, string>();
                        foreach (var prop in doc.RootElement.EnumerateObject()) {
                            if (Enum.TryParse<PadStyles>(prop.Name, ignoreCase: true, out var styleEnum)) {
                                formats[(int)styleEnum] = prop.Value.GetString() ?? string.Empty;
                            } else if (int.TryParse(prop.Name, out var styleKey)) {
                                formats[styleKey] = prop.Value.GetString() ?? string.Empty;
                            }
                        }

                        _styleData[styleName] = formats;
                    }
                } catch (Exception ex) { Develop.DebugPrint("Fehler beim Laden der Style-Ressourcen aus " + assembly.GetName().Name, ex); }
            }
        } catch (Exception ex) { Develop.DebugPrint("Fehler beim Initialisieren der Styles", ex); }
    }

    public static void LoadSkin() {
        try {
            BackgroundFill.ClearAll();
            BorderDraw.ClearAll();
            GraphicsPaths.ClearAll();
            _fontCache.Clear();
            LoadSkin("Win11");
        } catch (Exception ex) { Develop.DebugPrint("Fehler beim Laden des Win11-Skins", ex); }
        Inited = true;

        St[0] = ImageCodeEffect.WindowsXPDisabled;

        PenLinieDünn = BlueFont.GetPen(Color_Border(Enums.Design.Table_Lines_Thin, States.Standard), 1);
        PenLinieKräftig = BlueFont.GetPen(Color_Border(Enums.Design.Table_Lines_Thick, States.Standard), 1);
        PenLinieDick = BlueFont.GetPen(Color_Border(Enums.Design.Table_Lines_Thick, States.Standard), 3);
    }

    public static bool MustDrawTransparence(SkinDesign design, bool needTransparenz) {
        if (!needTransparenz || !design.Need) { return false; }

        if (design.Contour != Contour.None) {
            if (design.BackgroundStyle != BackgroundStyle.None) {
                if (design.Contour == Contour.Rectangle && design is { X1: >= 0, X2: >= 0 } and { Y1: >= 0, Y2: >= 0 }) {
                    return false;
                }
                if (design.Contour == Contour.RoundedRect && design is { X1: >= 1, X2: >= 1 } and { Y1: >= 1, Y2: >= 1 }) {
                    return false;
                }
            }
        }

        return true;
    }

    internal static Color Color_Border(Design design, States state) => DesignOf(design, state).BorderColor1;

    private static void LoadSkin(string skinName) {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream($"BlueControls.Ressources.Skins.Skin{skinName}.json");
        if (stream is null) { return; }

        using var doc = JsonDocument.Parse(stream);
        var root = doc.RootElement;

        foreach (var designProp in root.EnumerateObject()) {
            if (!Enum.TryParse<Design>(designProp.Name, out var design)) { continue; }

            foreach (var stateProp in designProp.Value.EnumerateObject()) {
                if (!Enum.TryParse<States>(stateProp.Name, out var state)) { continue; }

                var stateData = stateProp.Value;
                var kontur = stateData.GetEnum<Contour>("Contour");
                var font = stateData.GetString("Font");
                var x1 = stateData.GetInt("X1");
                var y1 = stateData.GetInt("Y1");
                var x2 = stateData.GetInt("X2");
                var y2 = stateData.GetInt("Y2");
                var hint = stateData.GetEnum<BackgroundStyle>("Background");
                var bc1 = stateData.GetString("BC1");
                var bc2 = stateData.GetString("BC2");
                var bc3 = stateData.GetString("BC3");
                var vm = stateData.GetFloat("VM", 0.7f);
                var rahm = stateData.GetEnum<BorderStyle>("Border");
                var boc1 = stateData.GetString("BOC1");
                var boc2 = stateData.GetString("BOC2");
                var pic = stateData.GetString("PIC");

                Design.Add(design, state, font, kontur, x1, y1, x2, y2, hint, bc1, bc2, rahm, boc1, boc2, pic, bc3, vm);
            }
        }
    }

    #endregion
}
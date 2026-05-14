// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes.ItemCollectionList;
using BlueControls.Controls;
using System.Reflection;
using System.Text.Json;
using static BlueBasics.ClassesStatic;
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
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, BlueFont> _fontCache = new();
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
                        DrawBack_Solid(gr, design.Contour, lr, design.BackColor1);
                        break;

                    case BackgroundStyle.GradientVertical:
                        DrawBack_GradientVertical(gr, design.Contour, lr, design.BackColor1, design.BackColor2);
                        break;

                    case BackgroundStyle.GradientVertical3:
                        DrawBack_GradientVertical3(gr, design.Contour, lr, design.BackColor1, design.BackColor2, design.BackColor3, design.GradientMidpoint);
                        break;

                    case BackgroundStyle.GradientHorizontal:
                        DrawBack_GradientHorizontal(gr, design.Contour, lr, design.BackColor1, design.BackColor2);
                        break;

                    case BackgroundStyle.GradientHorizontal3:
                        DrawBack_GradientHorizontal3(gr, design.Contour, lr, design.BackColor1, design.BackColor2, design.BackColor3, design.GradientMidpoint);
                        break;

                    case BackgroundStyle.GradientDiagonal:
                        DrawBack_GradientDiagonal(gr, design.Contour, lr, design.BackColor1, design.BackColor2, design.BackColor3, design.GradientMidpoint);
                        break;

                    case BackgroundStyle.Glossy:
                        DrawBack_Glossy(gr, design.Contour, lr, design.BackColor1, design.BackColor2);
                        break;

                    case BackgroundStyle.GlossyPressed:
                        DrawBack_GlossyPressed(gr, design.Contour, lr, design.BackColor1, design.BackColor2);
                        break;

                    case BackgroundStyle.GradientVerticalHighlight:
                        DrawBack_GradientVerticalHighlight(gr, design.Contour, lr, design.BackColor1, design.BackColor2, design.GradientMidpoint);
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
        if (control?.Parent == null) { return; }

        switch (control.Parent) {
            case IBackgroundNone:
                Draw_Back_Transparent(gr, r, control.Parent);
                break;

            case GenericControl trb:
                if (trb.BitmapOfControl() == null) {
                    gr.FillRectangle(BlueFont.GetBrush(control.Parent.BackColor), r);
                    return;
                }
                gr.DrawImage(trb.BitmapOfControl(), r, r with { X = control.Left + r.Left, Y = control.Top + r.Top }, GraphicsUnit.Pixel);
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
                        DrawBorder_Solid1Px(gr, design.Contour, lr, design.BorderColor1);
                        break;

                    case BorderStyle.Solid1PxDualColor:
                        DrawBorder_Solid1PxDualColor(gr, design.Contour, lr, design.BorderColor1, design.BorderColor2);
                        break;

                    case BorderStyle.Solid1PxFocusDot:
                        DrawBorder_Solid1PxFocusDot(gr, design.Contour, lr, design.BorderColor1, design.BorderColor2);
                        break;

                    case BorderStyle.FocusDot:
                        DrawBorder_FocusDot(gr, design.Contour, lr, design.BorderColor2);
                        break;

                    case BorderStyle.Solid3Px:
                        DrawBorder_Solid3Px(gr, design.Contour, lr, design.BorderColor1);
                        break;

                    case BorderStyle.Solid21Px:
                        DrawBorder_Solid21Px(gr, design.Contour, lr, design.BorderColor1);
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
        var img = qi != null ? QuickImage.Get(qi, AdditionalState(sd.Status)) : null;
        Draw_FormatedText(gr, txt, img, align, fitInRect, child, deleteBack, sd.Font, translate);
    }

    public static void Draw_FormatedText(Graphics gr, string txt, QuickImage? qi, Alignment align, Rectangle fitInRect, BlueFont? bFont, bool translate) =>
        Draw_FormatedText(gr, txt, qi, align, fitInRect, null, false, bFont, translate);

    public static void Draw_FormatedText(Graphics gr, string txt, QuickImage? qi, Alignment align,
        Rectangle fitInRect, SkinDesign design,
        System.Windows.Forms.Control? child, bool deleteBack, bool translate) {
        if (string.IsNullOrEmpty(txt) && qi == null) { return; }
        var img = qi != null ? QuickImage.Get(qi, AdditionalState(design.Status)) : null;
        Draw_FormatedText(gr, txt, img, align, fitInRect, child, deleteBack, design.Font, translate);
    }

    public static void Draw_FormatedText(Graphics gr, string txt, QuickImage? qi, Alignment align, Rectangle fitInRect, System.Windows.Forms.Control? child, bool deleteBack, BlueFont? bFont, bool translate) {
        if (string.IsNullOrEmpty(txt) && qi == null) { return; }

        var pSize = SizeF.Empty;
        if (qi != null) { lock (qi) { pSize = ((Bitmap)qi).Size; } }

        if (LanguageTool.Translation != null) { txt = LanguageTool.DoTranslate(txt, translate); }

        var tSize = SizeF.Empty;
        if (bFont != null) {
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
            if (child != null) {
                if (!string.IsNullOrEmpty(txt)) { Draw_Back_Transparent(gr, new Rectangle((int)(fitInRect.X + pSize.Width + xp - 1), (int)(fitInRect.Y + yp2 - 1), (int)(tSize.Width + 2), (int)(tSize.Height + 2)), child); }
                if (qi != null) { Draw_Back_Transparent(gr, new Rectangle((int)(fitInRect.X + xp), (int)(fitInRect.Y + yp1), (int)pSize.Width, (int)pSize.Height), child); }
            } else {
                var c = SkinCache.DeleteBackBrush;
                if (!string.IsNullOrEmpty(txt)) { gr.FillRectangle(c, new Rectangle((int)(fitInRect.X + pSize.Width + xp - 1), (int)(fitInRect.Y + yp2 - 1), (int)(tSize.Width + 2), (int)(tSize.Height + 2))); }
                if (qi != null) { gr.FillRectangle(c, new Rectangle((int)(fitInRect.X + xp), (int)(fitInRect.Y + yp1), (int)pSize.Width, (int)pSize.Height)); }
            }
        }
        try {
            if (qi != null) { gr.DrawImageUnscaled(qi, (int)(fitInRect.X + xp), (int)(fitInRect.Y + yp1)); }
            if (!string.IsNullOrEmpty(txt)) { bFont?.DrawString(gr, txt, fitInRect.X + pSize.Width + xp, fitInRect.Y + yp2); }
        } catch {
            // es kommt selten vor, dass das Graphics-Objekt an anderer Stelle verwendet wird.
        }
    }

    public static BlueFont GetBlueFont(Design design, States state) => DesignOf(design, state).Font;

    public static BlueFont GetBlueFont(string style, PadStyles format) {
        if (format == PadStyles.Undefined || string.IsNullOrEmpty(style)) { return BlueFont.DefaultFont; }

        var cacheKey = style + "|" + (int)format;
        if (_fontCache.TryGetValue(cacheKey, out var cachedFont)) {
            return cachedFont;
        }

        InitStyles();

        BlueFont result;
        if (_styleData.TryGetValue(style, out var formats) && formats.TryGetValue((int)format, out var fontString)) {
            result = BlueFont.Get(fontString);
        } else {
            result = BlueFont.DefaultFont;
        }

        _fontCache.TryAdd(cacheKey, result);
        return result;
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
                        if (stream == null) { continue; }

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
                } catch { }
            }
        } catch { }
    }

    public static void LoadSkin() {
        try {
            SkinCache.ClearAll();
            LoadSkin("Win11");
        } catch { }
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

    private static void DrawBack_Glossy(Graphics gr, Contour contour, Rectangle lr, Color backColor1, Color backColor2) {
        var c3 = Color.FromArgb(180, backColor2);
        var brush = SkinCache.GetGradient(BackgroundStyle.Glossy, backColor1, backColor2, c3, lr.Width, lr.Height, 0);
        if (contour == Contour.Rectangle) {
            gr.FillRectangle(brush, lr);
        } else {
            var path = SkinCache.GetContour(contour, lr.Width, lr.Height);
            if (path != null) { gr.FillPath(brush, path); }
        }
    }

    private static void DrawBack_GlossyPressed(Graphics gr, Contour contour, Rectangle lr, Color backColor1, Color backColor2) {
        var c3 = Color.FromArgb(180, backColor1);
        var brush = SkinCache.GetGradient(BackgroundStyle.GlossyPressed, backColor2, backColor1, c3, lr.Width, lr.Height, 0);
        if (contour == Contour.Rectangle) {
            gr.FillRectangle(brush, lr);
        } else {
            var path = SkinCache.GetContour(contour, lr.Width, lr.Height);
            if (path != null) { gr.FillPath(brush, path); }
        }
    }

    private static void DrawBack_GradientDiagonal(Graphics gr, Contour contour, Rectangle lr, Color backColor1, Color backColor2, Color backColor3, float gradientMidpoint) {
        var brush = SkinCache.GetGradient(BackgroundStyle.GradientDiagonal, backColor1, backColor2, backColor3, lr.Width, lr.Height, gradientMidpoint);
        if (contour == Contour.Rectangle) {
            gr.FillRectangle(brush, lr);
        } else {
            var path = SkinCache.GetContour(contour, lr.Width, lr.Height);
            if (path != null) { gr.FillPath(brush, path); }
        }
    }

    private static void DrawBack_GradientHorizontal(Graphics gr, Contour contour, Rectangle lr, Color backColor1, Color backColor2) {
        var brush = SkinCache.GetGradient(BackgroundStyle.GradientHorizontal, backColor1, backColor2, Color.Empty, lr.Width, lr.Height, 0);
        if (contour == Contour.Rectangle) {
            gr.FillRectangle(brush, lr);
        } else {
            var path = SkinCache.GetContour(contour, lr.Width, lr.Height);
            if (path != null) { gr.FillPath(brush, path); }
        }
    }

    private static void DrawBack_GradientHorizontal3(Graphics gr, Contour contour, Rectangle lr, Color backColor1, Color backColor2, Color backColor3, float gradientMidpoint) {
        var brush = SkinCache.GetGradient(BackgroundStyle.GradientHorizontal3, backColor1, backColor2, backColor3, lr.Width, lr.Height, gradientMidpoint);
        if (contour == Contour.Rectangle) {
            gr.FillRectangle(brush, lr);
        } else {
            var path = SkinCache.GetContour(contour, lr.Width, lr.Height);
            if (path != null) { gr.FillPath(brush, path); }
        }
    }

    private static void DrawBack_GradientVertical(Graphics gr, Contour contour, Rectangle lr, Color backColor1, Color backColor2) {
        var brush = SkinCache.GetGradient(BackgroundStyle.GradientVertical, backColor1, backColor2, Color.Empty, lr.Width, lr.Height, 0);
        if (contour == Contour.Rectangle) {
            gr.FillRectangle(brush, lr);
        } else {
            var path = SkinCache.GetContour(contour, lr.Width, lr.Height);
            if (path != null) { gr.FillPath(brush, path); }
        }
    }

    private static void DrawBack_GradientVertical3(Graphics gr, Contour contour, Rectangle lr, Color backColor1, Color backColor2, Color backColor3, float gradientMidpoint) {
        var brush = SkinCache.GetGradient(BackgroundStyle.GradientVertical3, backColor1, backColor2, backColor3, lr.Width, lr.Height, gradientMidpoint);
        if (contour == Contour.Rectangle) {
            gr.FillRectangle(brush, lr);
        } else {
            var path = SkinCache.GetContour(contour, lr.Width, lr.Height);
            if (path != null) { gr.FillPath(brush, path); }
        }
    }

    private static void DrawBack_GradientVerticalHighlight(Graphics gr, Contour contour, Rectangle lr, Color backColor1, Color backColor2, float gradientMidpoint) {
        var brush = SkinCache.GetGradient(BackgroundStyle.GradientVerticalHighlight, backColor1, Color.White, backColor2, lr.Width, lr.Height, gradientMidpoint);
        if (contour == Contour.Rectangle) {
            gr.FillRectangle(brush, lr);
        } else {
            var path = SkinCache.GetContour(contour, lr.Width, lr.Height);
            if (path != null) { gr.FillPath(brush, path); }
        }
    }

    private static void DrawBack_Solid(Graphics gr, Contour contour, Rectangle lr, Color backColor1) {
        var brush = BlueFont.GetBrush(backColor1);
        if (contour == Contour.Rectangle) {
            gr.FillRectangle(brush, lr);
        } else {
            var path = SkinCache.GetContour(contour, lr.Width, lr.Height);
            if (path != null) { gr.FillPath(brush, path); }
        }
    }

    private static void DrawBorder_FocusDot(Graphics gr, Contour contour, Rectangle lr, Color borderColor2) {
        var innerW = lr.Width - 6;
        var innerH = lr.Height - 6;
        if (innerW > 0 && innerH > 0) {
            var pen = SkinCache.GetDottedPen(borderColor2);
            if (contour == Contour.Rectangle) {
                gr.DrawRectangle(pen, new Rectangle(3, 3, innerW, innerH));
            } else {
                var innerPath = SkinCache.GetContour(contour, innerW, innerH);
                if (innerPath != null) {
                    gr.TranslateTransform(3, 3);
                    gr.DrawPath(pen, innerPath);
                    gr.TranslateTransform(-3, -3);
                }
            }
        }
    }

    private static void DrawBorder_Solid1Px(Graphics gr, Contour contour, Rectangle lr, Color borderColor1) {
        var pen = BlueFont.GetPen(borderColor1, 1);
        if (contour == Contour.Rectangle) {
            gr.DrawRectangle(pen, lr);
        } else {
            var path = SkinCache.GetContour(contour, lr.Width, lr.Height);
            if (path != null) { gr.DrawPath(pen, path); }
        }
    }

    private static void DrawBorder_Solid1PxDualColor(Graphics gr, Contour contour, Rectangle lr, Color borderColor1, Color borderColor2) {
        if (contour == Contour.Rectangle) {
            gr.DrawRectangle(BlueFont.GetPen(borderColor1, 1), lr);
        } else {
            var path = SkinCache.GetContour(contour, lr.Width, lr.Height);
            if (path != null) { gr.DrawPath(BlueFont.GetPen(borderColor1, 1), path); }
        }
        var lgb = SkinCache.GetBorderGradientBrush(borderColor1, borderColor2, lr.Height);
        gr.FillRectangle(lgb, 0, 0, lr.Width + 1, 2);
        gr.FillRectangle(lgb, 0, lr.Height - 1, lr.Width + 1, 2);
        gr.FillRectangle(lgb, 0, 0, 2, lr.Height + 1);
        gr.FillRectangle(lgb, lr.Width - 1, 0, 2, lr.Height + 1);
    }

    private static void DrawBorder_Solid1PxFocusDot(Graphics gr, Contour contour, Rectangle lr, Color borderColor1, Color borderColor2) {
        if (contour == Contour.Rectangle) {
            gr.DrawRectangle(BlueFont.GetPen(borderColor1, 1), lr);
        } else {
            var path = SkinCache.GetContour(contour, lr.Width, lr.Height);
            if (path != null) { gr.DrawPath(BlueFont.GetPen(borderColor1, 1), path); }
        }
        var innerW = lr.Width - 6;
        var innerH = lr.Height - 6;
        if (innerW > 0 && innerH > 0) {
            if (contour == Contour.Rectangle) {
                gr.DrawRectangle(SkinCache.GetDottedPen(borderColor2), new Rectangle(3, 3, innerW, innerH));
            } else {
                var innerPath = SkinCache.GetContour(contour, innerW, innerH);
                if (innerPath != null) {
                    gr.TranslateTransform(3, 3);
                    gr.DrawPath(SkinCache.GetDottedPen(borderColor2), innerPath);
                    gr.TranslateTransform(-3, -3);
                }
            }
        }
    }

    private static void DrawBorder_Solid21Px(Graphics gr, Contour contour, Rectangle lr, Color borderColor1) {
        var pen = BlueFont.GetPen(borderColor1, 21);
        if (contour == Contour.Rectangle) {
            gr.DrawRectangle(pen, lr);
        } else {
            var path = SkinCache.GetContour(contour, lr.Width, lr.Height);
            if (path != null) { gr.DrawPath(pen, path); }
        }
    }

    private static void DrawBorder_Solid3Px(Graphics gr, Contour contour, Rectangle lr, Color borderColor1) {
        var pen = BlueFont.GetPen(borderColor1, 3);
        if (contour == Contour.Rectangle) {
            gr.DrawRectangle(pen, lr);
        } else {
            var path = SkinCache.GetContour(contour, lr.Width, lr.Height);
            if (path != null) { gr.DrawPath(pen, path); }
        }
    }

    private static void LoadSkin(string skinName) {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream($"BlueControls.Ressources.Skins.Skin{skinName}.json");
        if (stream == null) { return; }

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
                var rahm = stateData.GetEnum<Enums.BorderStyle>("Border");
                var boc1 = stateData.GetString("BOC1");
                var boc2 = stateData.GetString("BOC2");
                var pic = stateData.GetString("PIC");

                Design.Add(design, state, font, kontur, x1, y1, x2, y2, hint, bc1, bc2, rahm, boc1, boc2, pic, bc3, vm);
            }
        }
    }

    #endregion
}
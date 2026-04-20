// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
// https://github.com/cromagan/BlueElements
//
// License: GNU Affero General Public License v3.0
// https://github.com/cromagan/BlueElements/blob/master/LICENSE
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using BlueBasics;
using BlueBasics.Classes;
using BlueBasics.ClassesStatic;
using BlueBasics.Enums;
using BlueControls.Classes.ItemCollectionList;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueTable.Classes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Text.Json;
using System.Windows.Forms;
using static BlueBasics.ClassesStatic.Converter;
using static BlueBasics.ClassesStatic.Polygons;
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

    public static Color RandomColor =>
        Color.FromArgb((byte)Constants.GlobalRnd.Next(0, 255),
            (byte)Constants.GlobalRnd.Next(0, 255),
            (byte)Constants.GlobalRnd.Next(0, 255));
    //TODO: Unused

    #endregion

    #region Methods

    public static ImageCodeEffect AdditionalState(States state) => state.HasFlag(States.Standard_Disabled) ? St[0] : ImageCodeEffect.None;

    public static List<string>? AllStyles() {
        InitStyles();
        return [.. _styleData.Keys];
    }

    public static void ChangeDesign(Design ds, States status, Contour enKontur, int x1, int y1, int x2, int y2, BackgroundStyle hint, string bc1, string bc2, Enums.BorderStyle rahm, string boc1, string boc2, string f, string pic, string bc3 = "", float vm = 0.7f) {
        //TODO: Unused
        Design.Remove(ds, status);
        Design.Add(ds, status, f, enKontur, x1, y1, x2, y2, hint, bc1, bc2, rahm, boc1, boc2, pic, bc3, vm);
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
                BorderStyle = Enums.BorderStyle.Solid1Px,
                Contour = Enums.Contour.Rectangle
            };
            return d;
        }
    }

    public static void Draw_Back(Graphics gr, Design design, States state, Rectangle r, Control? control, bool needTransparenz) => Draw_Back(gr, DesignOf(design, state), r, control, needTransparenz);

    public static void Draw_Back(Graphics gr, SkinDesign design, Rectangle r, Control? control, bool needTransparenz) {
        try {
            if (design.Need) {
                if (!needTransparenz) { design.Need = false; }
                if (design.Contour != Enums.Contour.None) {
                    if (design.BackgroundStyle != BackgroundStyle.None) {
                        if (design.Contour == Enums.Contour.Rectangle && design is { X1: >= 0, X2: >= 0 } and { Y1: >= 0, Y2: >= 0 }) { design.Need = false; }
                        if (design.Contour == Enums.Contour.RoundedRect && design is { X1: >= 1, X2: >= 1 } and { Y1: >= 1, Y2: >= 1 }) { design.Need = false; }
                    }
                }
            }
            if (design.Need) { Draw_Back_Transparent(gr, r, control); }
            if (design.BackgroundStyle == BackgroundStyle.None || design.Contour == Enums.Contour.None) { return; }
            r.X -= design.X1;
            r.Y -= design.Y1;
            r.Width += design.X1 + design.X2;
            r.Height += design.Y1 + design.Y2;
            if (r.Width < 1 || r.Height < 1) { return; }// Durchaus möglich, Creative-Pad, usereingabe
            switch (design.BackgroundStyle) {
                case BackgroundStyle.None:
                    break;

                case BackgroundStyle.Solid:
                    gr.FillPath(new SolidBrush(design.BackColor1), Contour(design.Contour, r));
                    break;

                case BackgroundStyle.GradientVertical:
                    var lgb = new LinearGradientBrush(r, design.BackColor1, design.BackColor2, LinearGradientMode.Vertical);
                    gr.FillPath(lgb, Contour(design.Contour, r));
                    break;

                case BackgroundStyle.GradientVertical3: {
                        var lgb3 = new LinearGradientBrush(r, design.BackColor1, design.BackColor2, LinearGradientMode.Vertical);
                        var cb = new ColorBlend {
                            Colors = [design.BackColor1, design.BackColor2, design.BackColor3],
                            Positions = [0.0F, design.GradientMidpoint, 1.0F]
                        };
                        lgb3.InterpolationColors = cb;
                        lgb3.GammaCorrection = true;
                        gr.FillPath(lgb3, Contour(design.Contour, r));
                    }
                    break;

                case BackgroundStyle.GradientHorizontal: {
                        var lgbH = new LinearGradientBrush(r, design.BackColor1, design.BackColor2, LinearGradientMode.Horizontal);
                        gr.FillPath(lgbH, Contour(design.Contour, r));
                    }
                    break;

                case BackgroundStyle.GradientHorizontal3: {
                        var lgbH3 = new LinearGradientBrush(r, design.BackColor1, design.BackColor2, LinearGradientMode.Horizontal);
                        var cbH = new ColorBlend {
                            Colors = [design.BackColor1, design.BackColor2, design.BackColor3],
                            Positions = [0.0F, design.GradientMidpoint, 1.0F]
                        };
                        lgbH3.InterpolationColors = cbH;
                        lgbH3.GammaCorrection = true;
                        gr.FillPath(lgbH3, Contour(design.Contour, r));
                    }
                    break;

                case BackgroundStyle.GradientDiagonal: {
                        var lgbD = new LinearGradientBrush(new Point(r.Left, r.Top), new Point(r.Right, r.Bottom), design.BackColor1, design.BackColor2);
                        var cbD = new ColorBlend {
                            Colors = [design.BackColor1, design.BackColor2, design.BackColor3],
                            Positions = [0.0F, design.GradientMidpoint, 1.0F]
                        };
                        lgbD.InterpolationColors = cbD;
                        lgbD.GammaCorrection = true;
                        gr.FillPath(lgbD, Contour(design.Contour, r));
                    }
                    break;

                case BackgroundStyle.Glossy: {
                        var lgbG = new LinearGradientBrush(r, design.BackColor1, design.BackColor2, LinearGradientMode.Vertical);
                        var cbG = new ColorBlend {
                            Colors = [design.BackColor1, design.BackColor2, Color.FromArgb(180, design.BackColor2)],
                            Positions = [0.0F, 0.4F, 1.0F]
                        };
                        lgbG.InterpolationColors = cbG;
                        lgbG.GammaCorrection = true;
                        gr.FillPath(lgbG, Contour(design.Contour, r));
                    }
                    break;

                case BackgroundStyle.GlossyPressed: {
                        var lgbGP = new LinearGradientBrush(r, design.BackColor2, design.BackColor1, LinearGradientMode.Vertical);
                        var cbGP = new ColorBlend {
                            Colors = [design.BackColor2, design.BackColor1, Color.FromArgb(180, design.BackColor1)],
                            Positions = [0.0F, 0.6F, 1.0F]
                        };
                        lgbGP.InterpolationColors = cbGP;
                        lgbGP.GammaCorrection = true;
                        gr.FillPath(lgbGP, Contour(design.Contour, r));
                    }
                    break;

                case BackgroundStyle.GradientVerticalHighlight: {
                        var lgbVH = new LinearGradientBrush(r, design.BackColor1, design.BackColor2, LinearGradientMode.Vertical);
                        var cbVH = new ColorBlend {
                            Colors = [design.BackColor1, Color.White, design.BackColor2],
                            Positions = [0.0F, design.GradientMidpoint, 1.0F]
                        };
                        lgbVH.InterpolationColors = cbVH;
                        lgbVH.GammaCorrection = true;
                        gr.FillPath(lgbVH, Contour(design.Contour, r));
                    }
                    break;

                case BackgroundStyle.Undefined:
                    break;

                default:
                    Develop.DebugPrint(design.BackgroundStyle);
                    break;
            }
        } catch (Exception ex) {
            Develop.DebugPrint("Fehler beim Zeichnen des Skins:" + design, ex);
        }
    }

    public static void Draw_Back_Transparent(Graphics gr, Rectangle r, Control? control) {
        if (control?.Parent == null) { return; }
        switch (control.Parent) {
            case IBackgroundNone:
                Draw_Back_Transparent(gr, r, control.Parent);
                break;

            case GenericControl trb:
                if (trb.BitmapOfControl() == null) {
                    gr.FillRectangle(new SolidBrush(control.Parent.BackColor), r);
                    return;
                }
                gr.DrawImage(trb.BitmapOfControl(), r, r with { X = control.Left + r.Left, Y = control.Top + r.Top }, GraphicsUnit.Pixel);
                break;

            case Form frm:
                gr.FillRectangle(new SolidBrush(frm.BackColor), r);
                break;

            case SplitContainer:
                Draw_Back_Transparent(gr, r, control.Parent);
                break;

            case SplitterPanel:
                Draw_Back_Transparent(gr, r, control.Parent);
                break;

            case TableLayoutPanel:
                Draw_Back_Transparent(gr, r, control.Parent);
                break;

            case TabPage: // TabPage leitet sich von Panel ab!
                gr.FillRectangle(new SolidBrush(control.Parent.BackColor), r);
                break;

            case Panel:
                Draw_Back_Transparent(gr, r, control.Parent);
                break;

            default:
                gr.FillRectangle(new SolidBrush(control.Parent.BackColor), r);
                break;
        }
    }

    public static void Draw_Border(Graphics gr, Design vDesign, States vState, Rectangle r) => Draw_Border(gr, DesignOf(vDesign, vState), r);

    public static void Draw_Border(Graphics gr, SkinDesign design, Rectangle r) {
        if (design.Contour == Enums.Contour.None || design.BorderStyle == Enums.BorderStyle.None) { return; }

        if (design.Contour == Enums.Contour.Undefined) {
            design.Contour = Enums.Contour.Rectangle;
            r.Width--;
            r.Height--;
        } else {
            r.X -= design.X1;
            r.Y -= design.Y1;
            r.Width += design.X1 + design.X2 - 1;
            r.Height += design.Y1 + design.Y2 - 1;
        }
        if (r.Width < 1 || r.Height < 1) { return; }

        // PathX kann durch die ganzen Expand mal zu klein werden, dann wird nothing zurückgegeben
        try {
            Pen penX;
            GraphicsPath? pathX;
            switch (design.BorderStyle) {
                case Enums.BorderStyle.Solid1Px:
                    pathX = Contour(design.Contour, r);
                    penX = new Pen(design.BorderColor1);
                    if (pathX != null) { gr.DrawPath(penX, pathX); }
                    break;

                case Enums.BorderStyle.Solid1PxDualColor: {
                        pathX = Contour(design.Contour, r);
                        penX = new Pen(design.BorderColor1);
                        gr.DrawPath(penX, pathX);
                        var lgbB = new LinearGradientBrush(new Point(r.Left, r.Top), new Point(r.Left, r.Bottom), design.BorderColor1, design.BorderColor2) {
                            GammaCorrection = true
                        };
                        gr.FillRectangle(lgbB, r.Left, r.Top, r.Width + 1, 2);
                        gr.FillRectangle(lgbB, r.Left, r.Bottom - 1, r.Width + 1, 2);
                        gr.FillRectangle(lgbB, r.Left, r.Top, 2, r.Height + 1);
                        gr.FillRectangle(lgbB, r.Right - 1, r.Top, 2, r.Height + 1);
                    }
                    break;

                case Enums.BorderStyle.Solid1PxFocusDot:
                    pathX = Contour(design.Contour, r);
                    penX = new Pen(design.BorderColor1);
                    gr.DrawPath(penX, pathX);
                    r.Inflate(-3, -3);
                    pathX = Contour(design.Contour, r);
                    penX = new Pen(design.BorderColor2) {
                        DashStyle = DashStyle.Dot
                    };
                    if (pathX != null) { gr.DrawPath(penX, pathX); }
                    break;

                case Enums.BorderStyle.FocusDot:
                    penX = new Pen(design.BorderColor2) {
                        DashStyle = DashStyle.Dot
                    };
                    r.Inflate(-3, -3);
                    pathX = Contour(design.Contour, r);
                    if (pathX != null) { gr.DrawPath(penX, pathX); }
                    break;

                case Enums.BorderStyle.Solid3Px:
                    pathX = Contour(design.Contour, r);
                    penX = new Pen(design.BorderColor1, 3);
                    if (pathX != null) { gr.DrawPath(penX, pathX); }
                    break;

                case Enums.BorderStyle.Solid21Px:
                    pathX = Contour(design.Contour, r);
                    penX = new Pen(design.BorderColor1, 21);
                    if (pathX != null) { gr.DrawPath(penX, pathX); }
                    break;

                default:
                    pathX = Contour(design.Contour, r);
                    penX = new Pen(Color.Red);
                    if (pathX != null) { gr.DrawPath(penX, pathX); }
                    Develop.DebugPrint(design.BorderStyle);
                    break;
            }
        } catch {
            //Develop.DebugPrint("Fehler beim Zeichen des Randes " + design, ex);
        }
    }

    /// <summary>
    /// Bild wird in dieser Routine nicht mehr gändert, aber in der nachfolgenden
    /// </summary>
    /// <param name="gr"></param>
    /// <param name="txt"></param>
    /// <param name="qi"></param>
    /// <param name="align"></param>
    /// <param name="fitInRect"></param>
    /// <param name="design"></param>
    /// <param name="state"></param>
    /// <param name="child"></param>
    /// <param name="deleteBack"></param>
    /// <param name="translate"></param>
    public static void Draw_FormatedText(Graphics gr, string txt, QuickImage? qi,
        Alignment align,
        Rectangle fitInRect, Design design, States state,
        Control? child, bool deleteBack, bool translate) => Draw_FormatedText(gr, txt, qi, align, fitInRect, DesignOf(design, state), child, deleteBack, translate);

    public static void Draw_FormatedText(Graphics gr, string txt, QuickImage? qi, Alignment align, Rectangle fitInRect, BlueFont? bFont, bool translate) => Draw_FormatedText(gr, txt, qi, align, fitInRect, null, false, bFont, translate);

    //private static void Draw_Border_DuoColor(Graphics GR, RowItem Row, Rectangle r, bool NurOben) {
    //    var c1 = Color.FromArgb(Value(Row, col_Color_Border_2, 0));
    //    var c2 = Color.FromArgb(Value(Row, col_Color_Border_3, 0));
    //    var lgb = new LinearGradientBrush(new Point(r.Left, r.Top), new Point(r.Left, r.Height), c1, c2) {
    //        GammaCorrection = true
    //    };
    //    var x = GR.SmoothingMode;
    //    GR.SmoothingMode = SmoothingMode.Default; //Returns the smoothing mode to default for a crisp structure
    //    GR.FillRectangle(lgb, r.Left, r.Top, r.Width + 1, 2); // Oben
    //    if (!NurOben) {
    //        GR.FillRectangle(lgb, r.Left, r.Bottom - 1, r.Width + 1, 2); // unten
    //        GR.FillRectangle(lgb, r.Left, r.Top, 2, r.Height + 1); // links
    //        GR.FillRectangle(lgb, r.Right - 1, r.Top, 2, r.Height + 1); // rechts
    //    }
    //    GR.SmoothingMode = x;
    //}

    /// <summary>
    /// Status des Bildes (Disabled) wird geändert
    /// </summary>
    /// <param name="gr"></param>
    /// <param name="txt"></param>
    /// <param name="qi"></param>
    /// <param name="align"></param>
    /// <param name="fitInRect"></param>
    /// <param name="design"></param>
    /// <param name="child"></param>
    /// <param name="deleteBack"></param>
    /// <param name="translate"></param>
    public static void Draw_FormatedText(Graphics gr, string txt, QuickImage? qi, Alignment align,
        Rectangle fitInRect, SkinDesign design,
        Control? child, bool deleteBack, bool translate) {
        if (string.IsNullOrEmpty(txt) && qi == null) { return; }
        QuickImage? tmpImage = null;
        if (qi != null) { tmpImage = QuickImage.Get(qi, AdditionalState(design.Status)); }
        Draw_FormatedText(gr, txt, tmpImage, align, fitInRect, child, deleteBack, design.Font, translate);
    }

    /// <summary>
    /// Zeichnet den Text und das Bild ohne weitere Modifikation
    /// </summary>
    /// <param name="gr"></param>
    /// <param name="txt"></param>
    /// <param name="qi"></param>
    /// <param name="align"></param>
    /// <param name="fitInRect"></param>
    /// <param name="child"></param>
    /// <param name="deleteBack"></param>
    /// <param name="bFont"></param>
    /// <param name="translate"></param>
    public static void Draw_FormatedText(Graphics gr, string txt, QuickImage? qi, Alignment align, Rectangle fitInRect, Control? child, bool deleteBack, BlueFont? bFont, bool translate) {
        var pSize = SizeF.Empty;
        var tSize = SizeF.Empty;
        float xp = 0;
        float yp1 = 0;
        float yp2 = 0;
        if (qi != null) {
            lock (qi) {
                pSize = ((Bitmap)qi).Size;
            }
        }
        if (LanguageTool.Translation != null) { txt = LanguageTool.DoTranslate(txt, translate); }
        if (bFont != null) {
            if (fitInRect.Width > 0) { txt = BlueFont.TrimByWidth(bFont, txt, fitInRect.Width - pSize.Width); }
            tSize = bFont.MeasureString(txt);
        }
        if (align.HasFlag(Alignment.Right)) { xp = fitInRect.Width - pSize.Width - tSize.Width; }
        if (align.HasFlag(Alignment.HorizontalCenter)) { xp = (float)((fitInRect.Width - pSize.Width - tSize.Width) / 2.0); }
        if (align.HasFlag(Alignment.VerticalCenter)) {
            yp1 = (float)((fitInRect.Height - pSize.Height) / 2.0);
            yp2 = (float)((fitInRect.Height - tSize.Height) / 2.0);
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
                var c = new SolidBrush(Color.FromArgb(220, 255, 255, 255));
                if (!string.IsNullOrEmpty(txt)) { gr.FillRectangle(c, new Rectangle((int)(fitInRect.X + pSize.Width + xp - 1), (int)(fitInRect.Y + yp2 - 1), (int)(tSize.Width + 2), (int)(tSize.Height + 2))); }
                if (qi != null) { gr.FillRectangle(c, new Rectangle((int)(fitInRect.X + xp), (int)(fitInRect.Y + yp1), (int)pSize.Width, (int)pSize.Height)); }
            }
        }
        try {
            if (qi != null) { gr.DrawImage(qi, (int)(fitInRect.X + xp), (int)(fitInRect.Y + yp1)); }
            if (!string.IsNullOrEmpty(txt)) { bFont?.DrawString(gr, txt, fitInRect.X + pSize.Width + xp, fitInRect.Y + yp2); }
        } catch {
            // es kommt selten vor, dass das Graphics-Objekt an anderer Stelle verwendet wird.
            //Develop.DebugPrint(ex);
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
        //rahms.Sort();
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
        //rahms.Sort();
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
                        bool inStylesFolder = false;
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

    // Der Abstand von z.B. in Textboxen: Text Linke Koordinate
    public static void LoadSkin() {
        try {
            LoadSkin("Win11");
        } catch { }
        Inited = true;

        St[0] = ImageCodeEffect.WindowsXPDisabled;

        PenLinieDünn = new Pen(Color_Border(Enums.Design.Table_Lines_Thin, States.Standard));
        PenLinieKräftig = new Pen(Color_Border(Enums.Design.Table_Lines_Thick, States.Standard));
        PenLinieDick = new Pen(Color_Border(Enums.Design.Table_Lines_Thick, States.Standard), 3);
    }

    internal static Color Color_Border(Design design, States state) => DesignOf(design, state).BorderColor1;

    private static GraphicsPath? Contour(Contour kon, Rectangle r) {
        switch (kon) {
            case Enums.Contour.Rectangle:
                return Poly_Rechteck(r);

            case Enums.Contour.RoundedRectThin:
                return Poly_RoundRec(r, 2);

            case Enums.Contour.RoundedRect:
                return Poly_RoundRec(r, 4);

            case Enums.Contour.None:
                return null;

            default:
                return Poly_Rechteck(r);
        }
    }

    private static T GetEnumProperty<T>(Dictionary<string, JsonElement> props, string key) where T : struct, Enum {
        if (props.TryGetValue(key, out var elem) && elem.ValueKind == JsonValueKind.String) {
            var value = elem.GetString();
            if (!string.IsNullOrEmpty(value) && Enum.TryParse<T>(value, out var result)) {
                return result;
            }
        }
        return default;
    }

    private static string GetJsonProperty(Dictionary<string, JsonElement> props, string key, string defaultValue) {
        if (props.TryGetValue(key, out var elem) && elem.ValueKind == JsonValueKind.String) {
            return elem.GetString() ?? defaultValue;
        }
        return defaultValue;
    }

    private static int GetJsonProperty(Dictionary<string, JsonElement> props, string key, int defaultValue) {
        if (props.TryGetValue(key, out var elem) && elem.ValueKind == JsonValueKind.Number) {
            return elem.GetInt32();
        }
        return defaultValue;
    }

    private static void LoadSkin(string skinName) {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream($"BlueControls.Ressources.Skins.Skin{skinName}.json");
        if (stream == null) { return; }

        var skinData = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, Dictionary<string, JsonElement>>>>(stream);
        if (skinData == null) { return; }

        foreach (var designKvp in skinData) {
            if (!Enum.TryParse<Design>(designKvp.Key, out var design)) { continue; }

            foreach (var stateKvp in designKvp.Value) {
                if (!Enum.TryParse<States>(stateKvp.Key, out var state)) { continue; }

                var props = stateKvp.Value;
                var kontur = GetEnumProperty<Contour>(props, "Contour");
                var font = GetJsonProperty(props, "Font", string.Empty);
                var x1 = GetJsonProperty(props, "X1", 0);
                var y1 = GetJsonProperty(props, "Y1", 0);
                var x2 = GetJsonProperty(props, "X2", 0);
                var y2 = GetJsonProperty(props, "Y2", 0);
                var hint = GetEnumProperty<BackgroundStyle>(props, "Background");
                var bc1 = GetJsonProperty(props, "BC1", string.Empty);
                var bc2 = GetJsonProperty(props, "BC2", string.Empty);
                var bc3 = GetJsonProperty(props, "BC3", string.Empty);
                var vm = GetJsonProperty(props, "VM", "0.7");
                var vmFloat = FloatParse(vm.FromNonCritical());
                var rahm = GetEnumProperty<Enums.BorderStyle>(props, "Border");
                var boc1 = GetJsonProperty(props, "BOC1", string.Empty);
                var boc2 = GetJsonProperty(props, "BOC2", string.Empty);
                var pic = GetJsonProperty(props, "PIC", string.Empty);

                Design.Add(design, state, font, kontur, x1, y1, x2, y2, hint, bc1, bc2, rahm, boc1, boc2, pic, bc3, vmFloat);
            }
        }
    }

    #endregion
}
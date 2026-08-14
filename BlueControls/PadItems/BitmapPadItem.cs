// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;
using BlueControls.PadItems.Abstract;
using BlueScript.ScriptVariables;
using static BlueBasics.ClassesStatic.IO;

namespace BlueControls.PadItems;

public sealed class BitmapPadItem : SizeableRectanglePadItem, ICanHaveVariables, IStyleableOne {

    #region Fields

    private System.Drawing.Bitmap? _bitmap;

    /// <summary>
    /// Base64-kodiertes PNG des aktuellen Bildes. Wird beim Setzen der
    /// <see cref="BitmapValue" />-Property sofort erzeugt und beim Spiegeln
    /// aktualisiert. Beim Parsen wird der Original-String unverändert
    /// übernommen, damit ein PNG-Roundtrip (decode → encode) keine
    /// anderen Bytes erzeugt — der GDI+-Encoder produziert nämlich nicht
    /// zwingend identische IDAT-Bytes. Ist das Feld leer, gibt es kein Bild.
    /// Das eigentliche <see cref="_bitmap" /> wird erst bei Bedarf im
    /// <see cref="BitmapValue" />-Getter dekodiert (Lazy Loading).
    /// </summary>
    private string _rawImageBase64 = string.Empty;

    #endregion

    #region Constructors

    public BitmapPadItem() : this(string.Empty, null, Size.Empty) { }

    public BitmapPadItem(string keyName, System.Drawing.Bitmap? bmp, Size size) : base(keyName) {
        // Suppress-Modus whrend der Konstruktion: Property-Setter lsen keine
        // Change-Events aus. Siehe ParseableItem.ISupportInitialize.
        BeginInit();
        try {
            BitmapValue = bmp;
            SetCoordinates(new RectangleF(0, 0, size.Width, size.Height));
            Hintergrund_Weiß_Füllen = true;
            Bild_Modus = SizeModes.EmptySpace;
            Style = PadStyles.Undefined; // Kein Rahmen
        } finally {
            EndInit();
        }
    }

    #endregion

    #region Properties

    public static string ClassId => "IMAGE";

    public SizeModes Bild_Modus {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    } = SizeModes.EmptySpace;

    public System.Drawing.Bitmap? BitmapValue {
        get {
            if (_bitmap is null && _rawImageBase64 is { Length: > 0 } raw) {
                _bitmap = Base64ToBitmap(raw);
            }
            return _bitmap;
        }
        set {
            if (_bitmap == value) { return; }
            _rawImageBase64 = value is null ? string.Empty : BitmapToBase64(value, ImageFormat.Png);
            _bitmap = value;
            OnPropertyChanged();
        }
    }

    public override string Description => string.Empty;

    public BlueFont? Font { get; set; }

    public bool Hintergrund_Weiß_Füllen {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    public bool PixelGenau {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    [Description("Hier kann ein Variablenname als Platzhalter eingegeben werden. Beispiel: ~Bild~")]
    public string Platzhalter_Für_Layout {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    public string SheetStyle => Parent is IStyleable ist ? ist.SheetStyle : string.Empty;

    public PadStyles Style {
        get;
        set {
            if (field == value) { return; }
            field = value;
            this.InvalidateFont();
            OnPropertyChanged();
        }
    }

    protected override int SaveOrder => 999;

    #endregion

    #region Methods

    public void Bildschirmbereich_wählen() {
        if (BitmapValue is not null) {
            if (MessageBox.Show("Vorhandenes Bild überschreiben?", ImageCode.Warnung, "Ja", "Nein") != 0) { return; }
        }
        BitmapValue = ScreenShot.GrabArea(null).Area;
    }

    public void Datei_laden() {
        if (BitmapValue is not null) {
            if (MessageBox.Show("Vorhandenes Bild überschreiben?", ImageCode.Warnung, "Ja", "Nein") != 0) { return; }
        }
        var e = new System.Windows.Forms.OpenFileDialog() {
            CheckFileExists = true,
            Multiselect = false,
            Title = "Bild wählen:",
            Filter = "PNG Portable Network Graphics|*.png|JPG Jpeg Interchange|*.jpg|Bmp Windows Bitmap|*.bmp"
        };
        e.ShowDialog();

        if (!FileExists(e.FileName)) { return; }
        BitmapValue = (System.Drawing.Bitmap?)Image_FromFile(e.FileName);
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<ListItem> comms =
        [
            ItemOf("Abschneiden", ((int)SizeModes.BildAbschneiden).ToString1(),
                QuickImage.Get("BildmodusAbschneiden|32")),
            ItemOf("Verzerren", ((int)SizeModes.Verzerren).ToString1(), QuickImage.Get("BildmodusVerzerren|32")),
            ItemOf("Einpassen", ((int)SizeModes.EmptySpace).ToString1(), QuickImage.Get("BildmodusEinpassen|32"))
        ];

        var platzhalterFlex = new FlexiControlForProperty<string>(() => Platzhalter_Für_Layout, 2);

        if (Parent is CollectionPadItem icpi) {
            var vars = icpi.GetExportVariables();
            var bitmapVars = vars.Where(v => v is BitmapScriptVariable or StringScriptVariable).ToList();

            platzhalterFlex.QuickInfo = icpi.GetExportVariablesInfo(platzhalterFlex.QuickInfo, bitmapVars.Count);

            if (bitmapVars.Count > 0) {
                platzhalterFlex.EditType = EditTypeFormula.Textfeld_mit_Suggestions;
                platzhalterFlex.SuggestionPosition = SuggestionPosition.ContextMenuOnly;
                platzhalterFlex.ListItems = [.. bitmapVars.Select(v => ItemOf($"~{v.KeyName}~"))];
            }
        }

        List<GenericControl> result =
        [
            new FlexiControlForDelegate(Bildschirmbereich_wählen, "Bildschirmbereich wählen", ImageCode.Bild),

            new FlexiControlForDelegate(Datei_laden, "Bild laden", ImageCode.Ordner),

            new FlexiControl(),

            platzhalterFlex,

            new FlexiControl(),
            new FlexiControlForProperty<SizeModes>(() => Bild_Modus, comms),
            new FlexiControl(),
            new FlexiControlForProperty<PadStyles>(() => Style, Skin.GetRahmenArt(SheetStyle, true)),
            new FlexiControlForProperty<bool>(() => Hintergrund_Weiß_Füllen),
            new FlexiControlForProperty<bool>(() => PixelGenau),
            new FlexiControl(),
            ..base.GetProperties(widthOfControl)
        ];

        return result;
    }

    public override List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [.. base.ParseableItems()];

        result.ParseableAdd("Modus", Bild_Modus);
        result.ParseableAdd("Placeholder", Platzhalter_Für_Layout);
        result.ParseableAdd("PixelPerfect", PixelGenau);
        result.ParseableAdd("WhiteBack", Hintergrund_Weiß_Füllen);
        result.ParseableAdd("Image", _rawImageBase64);
        result.ParseableAdd("Style", Style);

        return result;
    }

    public override JsonObject ParseableJson() {
        var json = base.ParseableJson();
        json.Set("modus", (int)Bild_Modus);
        json.Set("placeholder", Platzhalter_Für_Layout);
        json.Set("pixelperfect", PixelGenau);
        json.Set("whiteback", Hintergrund_Weiß_Füllen);
        json.Set("image", _rawImageBase64);
        json.Set("style", (int)Style);
        return json;
    }

    public override void ParseJson(JsonObject json) {
        BeginInit();
        try {
            Bild_Modus = json.GetEnum("modus", Bild_Modus);
            Platzhalter_Für_Layout = json.GetString("placeholder", Platzhalter_Für_Layout);
            PixelGenau = json.GetBool("pixelperfect", PixelGenau);
            Hintergrund_Weiß_Füllen = json.GetBool("whiteback", Hintergrund_Weiß_Füllen);
            _rawImageBase64 = json.GetString("image");
            _bitmap = null;
            Style = json.GetEnum("style", Style);
            base.ParseJson(json);
        } finally {
            EndInit();
        }
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "modus":
                Bild_Modus = (SizeModes)IntParse(value);
                return true;

            case "whiteback":
                Hintergrund_Weiß_Füllen = value.FromPlusMinus();
                return true;

            case "pixelperfect":
                PixelGenau = value.FromPlusMinus();
                return true;

            case "padding":
                //_padding = IntParse(value);
                return true;

            case "image":
                _rawImageBase64 = value.FromNonCritical();
                _bitmap = null;
                return true;

            case "placeholder":
                Platzhalter_Für_Layout = value.FromNonCritical();
                return true;

            case "style":
                Style = (PadStyles)IntParse(value);
                return true;
        }
        return base.ParseThis(key, value);
    }

    public override string ReadableText() => "Bild";

    public bool ReplaceVariable(ScriptVariable variable) {
        if (IsDisposed) { return false; }
        if (string.IsNullOrEmpty(Platzhalter_Für_Layout)) { return false; }
        if (!("~" + variable.KeyName + "~").Equals(Platzhalter_Für_Layout, StringComparison.OrdinalIgnoreCase)) { return false; }

        switch (variable) {
            case BlueScript.ScriptVariables.BitmapScriptVariable vbmp:
                if (vbmp.ValueBitmap is { } bmp1) {
                    BitmapValue = bmp1;
                    return true;
                }
                return false;

            case StringScriptVariable filn:
                if (FileExists(filn.ValueString) && Image_FromFile(filn.ValueString) is System.Drawing.Bitmap bmp2) {
                    BitmapValue = bmp2;
                    return true;
                }
                return false;

            default:
                return false;
        }
    }

    public bool ResetVariables() {
        if (IsDisposed) { return false; }
        if (!string.IsNullOrEmpty(Platzhalter_Für_Layout) && BitmapValue is not null) {
            BitmapValue = null;
            return true;
        }
        return false;
    }

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Bild, 16);

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);
        UnRegisterEvents();
        if (!IsDisposed) {
            if (disposing) {
                // TODO: Verwalteten Zustand (verwaltete Objekte) bereinigen
            }

            // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer überschreiben
            // TODO: Große Felder auf NULL setzen
            _rawImageBase64 = string.Empty;
            if (_bitmap is not null) {
                _bitmap.Dispose();
                _bitmap = null;
            }

            //IsDisposed = true;
        }
    }

    protected override void DrawExplicit(Graphics gr, Rectangle visibleAreaControl, RectangleF positionControl, float zoom, float offsetX, float offsetY, bool forPrinting) {
        //positionControl.Inflate(-_padding, -_padding);
        //RectangleF r1 = new(positionControl.Left , positionControl.Top , positionControl.Width , positionControl.Height );
        var r2 = new RectangleF();
        var r3 = new RectangleF();
        var tmpPixelPerfekt = PixelGenau;

        if (BitmapValue is not null) {
            r3 = new RectangleF(0, 0, BitmapValue.Width, BitmapValue.Height);
            switch (Bild_Modus) {
                case SizeModes.Verzerren: {
                        r2 = positionControl;
                        tmpPixelPerfekt = tmpPixelPerfekt && r2.Width > r3.Width;
                        break;
                    }

                case SizeModes.BildAbschneiden: {
                        var scale2 = Math.Max(positionControl.Width / BitmapValue.Width, positionControl.Height / BitmapValue.Height);
                        var tmpw = positionControl.Width / scale2;
                        var tmph = positionControl.Height / scale2;
                        r3 = new RectangleF((BitmapValue.Width - tmpw) / 2, (BitmapValue.Height - tmph) / 2, tmpw, tmph);
                        r2 = positionControl;
                        tmpPixelPerfekt = tmpPixelPerfekt && scale2 > 1f;
                        break;
                    }
                default: // Is = enSizeModes.WeißerRand
                {
                        var scale2 = Math.Min(positionControl.Width / BitmapValue.Width, positionControl.Height / BitmapValue.Height);
                        r2 = new RectangleF((positionControl.Width - BitmapValue.Width.CanvasToControl(scale2)) / 2 + positionControl.Left, (positionControl.Height - BitmapValue.Height.CanvasToControl(scale2)) / 2 + positionControl.Top, BitmapValue.Width.CanvasToControl(scale2), BitmapValue.Height.CanvasToControl(scale2));
                        tmpPixelPerfekt = tmpPixelPerfekt && scale2 > 1f;
                        break;
                    }
            }
        }
        var trp = positionControl.PointOf(Alignment.Horizontal_Vertical_Center);
        gr.TranslateTransform(trp.X, trp.Y);
        gr.RotateTransform(-Drehwinkel);
        var r1 = positionControl with { X = positionControl.Left - trp.X, Y = positionControl.Top - trp.Y };
        r2 = r2 with { X = r2.Left - trp.X, Y = r2.Top - trp.Y };
        if (Hintergrund_Weiß_Füllen) {
            gr.FillRectangle(Brushes.White, r1);
        }
        try {
            if (BitmapValue is not null) {
                if (tmpPixelPerfekt) {
                    gr.InterpolationMode = InterpolationMode.NearestNeighbor;
                    gr.PixelOffsetMode = PixelOffsetMode.Half;
                } else if (forPrinting) {
                    gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                } else {
                    gr.InterpolationMode = InterpolationMode.Low;
                    gr.PixelOffsetMode = PixelOffsetMode.HighSpeed;
                }
                gr.DrawImage(BitmapValue, r2, r3, GraphicsUnit.Pixel);
            }
        } catch {
            CollectGarbage();
        }
        if (Style != PadStyles.Undefined) {
            gr.DrawRectangle(this.GetFont().Pen(zoom), r1);
        }

        gr.TranslateTransform(-trp.X, -trp.Y);
        gr.ResetTransform();
        if (!forPrinting) {
            if (!string.IsNullOrEmpty(Platzhalter_Für_Layout)) {
                BlueFont.Get("Arial", 8, false, false, false, false, Color.Black, Color.Transparent, Color.Transparent).DrawString(gr, Platzhalter_Für_Layout, positionControl.Left, positionControl.Top);
            }
        }
    }

    protected override void OnParentChanged() {
        base.OnParentChanged();
        this.InvalidateFont();
        if (Parent is CollectionPadItem icpi) {
            icpi.StyleChanged += Icpi_StyleChanged;
        }
    }

    protected override void OnParentChanging() {
        base.OnParentChanging();
        UnRegisterEvents();
    }

    private void Icpi_StyleChanged(object? sender, System.EventArgs e) => this.InvalidateFont();

    private void UnRegisterEvents() {
        if (Parent is CollectionPadItem icpi) {
            icpi.StyleChanged -= Icpi_StyleChanged;
        }
    }

    #endregion
}
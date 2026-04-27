// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes.ItemCollectionPad.Abstract;
using BlueControls.Controls;
using BlueControls.Renderer;
using BlueTable.Interfaces;

namespace BlueControls.Classes.ItemCollectionPad.FunktionsItems_ColumnArrangement_Editor;

/// <summary>
/// Zum Darstellen einer Spalte. Im ViewEditpt benutzt
/// </summary>
public class ColumnPadItem : FixedRectangleBitmapPadItem, IHasTable {

    #region Fields

    public static readonly BlueFont? ColumnFont = Skin.GetBlueFont(Constants.Win11, PadStyles.Emphasized);

    #endregion

    #region Constructors

    public ColumnPadItem(ColumnViewItem cvi, Renderer_Abstract renderer) : base(cvi.Column?.Table?.KeyName + "|" + cvi.Column?.KeyName) {
        CVI = cvi;
        Renderer = renderer;

        if (CVI.Column is { IsDisposed: false } col) {
            col.PropertyChanged += Column_PropertyChanged;
            CVI.PropertyChanged += Column_PropertyChanged;

            CanvasSize = new SizeF(Math.Min(ColumnViewItemRenderingExtensions.CalculateCanvasContentWith(col, renderer), 300), 300);
        }
    }

    #endregion

    #region Properties

    public static string ClassId => "FI-Column";

    public ColumnViewItem? CVI { get; }

    public override string Description => string.Empty;
    public Renderer_Abstract Renderer { get; }

    public Table? Table => CVI?.Column?.Table;

    /// <summary>
    /// Wird von Flexoptions aufgerufen
    /// </summary>
    protected override int SaveOrder => 999;

    #endregion

    #region Methods

    //        if (value == Permanent) { return; }
    public override List<GenericControl> GetProperties(int widthOfControl) {
        if (CVI is not { IsDisposed: false } cvi) { return []; }
        if (cvi.Column is not { IsDisposed: false } col) { return []; }
        if (col.Table is not { IsDisposed: false } tb) { return []; }

        List<GenericControl> result =
        [
            new FlexiControlForDelegate(tb),
            new FlexiControlForDelegate(col),
            new FlexiControl(),
            new FlexiControlForProperty<bool>(() => cvi.Permanent),
            new FlexiControlForProperty<bool>(() => cvi.Horizontal),
            new FlexiControlForProperty<Color>(() => cvi.BackColor_ColumnHead),
            new FlexiControlForProperty<Color>(() => cvi.BackColor_ColumnCell),
            new FlexiControlForProperty<Color>(() => cvi.FontColor_Caption),
            new FlexiControl(),
            //new FlexiControl(),
            //new FlexiControlForProperty<string>(() => Column.CaptionGroup1),
            //new FlexiControlForProperty<string>(() => Column.CaptionGroup2),
            //new FlexiControlForProperty<string>(() => Column.CaptionGroup3),
            new FlexiControl(),
            new FlexiControlForProperty<string>(() => col.QuickInfo, 5),
            new FlexiControlForProperty<string>(() => col.AdminInfo, 5)
        ];

        return result;
    }

    public override string ReadableText() => "Spalte";

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Spalte, 16);

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);
        if (disposing) {
            if (CVI?.Column is { IsDisposed: false } col) {
                col.PropertyChanged -= Column_PropertyChanged;
                CVI.PropertyChanged -= Column_PropertyChanged;
            }
        }
    }

    protected override void GeneratePic() {
        if (CVI?.Column is not { IsDisposed: false } col) {
            GeneratedBitmap = QuickImage.Get(ImageCode.Warnung, 128);
            return;
        }

        //var wi = 300;// Math.Max(Column.CanvasWidthx ?? 0, 24);

        var bmp = new Bitmap((int)CanvasSize.Width, (int)CanvasSize.Height);
        var gr = Graphics.FromImage(bmp);

        gr.Clear(CVI.BackColor_ColumnHead);
        //gr.DrawString(Column.Caption, CellFont, )
        //Table.Draw_FormatedText(gr,)

        for (var z = 0; z < 3; z++) {
            var n = col.CaptionGroup(z);
            if (!string.IsNullOrEmpty(n)) {
                Skin.Draw_FormatedText(gr, n, null, Alignment.Horizontal_Vertical_Center, new Rectangle(0, z * 16, bmp.Width, 61), null, false, ColumnFont, true);
            }
        }

        gr.TranslateTransform(bmp.Width / 2f, 50);
        gr.RotateTransform(-90);
        Skin.Draw_FormatedText(gr, col.Caption, null, Alignment.VerticalCenter_Left, new Rectangle(-150, -150, 300, 300), null, false, ColumnFont, true);

        gr.TranslateTransform(-bmp.Width / 2f, -50);
        gr.ResetTransform();

        gr.DrawLine(Pens.Black, 0, 210, bmp.Width, 210);

        var r = col.Table?.Row.First();
        if (r is { IsDisposed: false }) {
            Renderer?.Draw(gr, r.CellGetString(col), null, new Rectangle(0, 210, bmp.Width, 90), col.DoOpticalTranslation, (Alignment)col.Align, 1f);
        }

        GeneratedBitmap = bmp;
    }

    private void Column_PropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (IsDisposed) { return; }
        RemovePic();
        OnPropertyChanged(e.PropertyName);
    }

    #endregion
}
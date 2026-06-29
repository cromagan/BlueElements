// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes;
using BlueControls.Classes.ItemCollectionList;
using BlueControls.Editoren;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Forms;

public partial class FontEditor : EditorEasy {

    #region Fields

    private static List<AbstractListItem>? _fnList;

    private static List<AbstractListItem>? _fsList;

    #endregion

    #region Constructors

    public FontEditor() : base() => InitializeComponent();

    #endregion

    #region Properties

    public override Type? EditorFor => typeof(BlueFont);

    #endregion

    #region Methods

    public override void Clear() {
        lstName.UncheckAll();
        listSize.UncheckAll();
        chkFett.Checked = false;
        chkKursiv.Checked = false;
        chkUnterstrichen.Checked = false;
        chkDurchgestrichen.Checked = false;
        chkOutline.Checked = false;
        btnFontColor.ImageCode = string.Empty;
        btnOutlineColor.ImageCode = string.Empty;
        btnBackColor.ImageCode = string.Empty;
        preview.Image = null;
    }

    public override object? CreateNewItem() {
        if (lstName.Checked.Count == 0 || listSize.Checked.Count == 0) { return null; }

        return BlueFont.Get(lstName.Checked[0],
                             FloatParse(listSize.Checked[0]),
                             chkFett.Checked,
                             chkKursiv.Checked,
                             chkUnterstrichen.Checked,
                             chkDurchgestrichen.Checked,
                             QuickImage.Get(btnFontColor.ImageCode).ChangeGreenTo ?? Color.Transparent,
                             QuickImage.Get(btnOutlineColor.ImageCode).ChangeGreenTo ?? Color.Transparent,
                             QuickImage.Get(btnBackColor.ImageCode).ChangeGreenTo ?? Color.Transparent);
    }

    protected override void InitializeComponentDefaultValues() {
        if (_fnList is null) {
            _fnList = [];
            foreach (var f in FontFamily.Families) {
                if (string.IsNullOrEmpty(f.Name)) { continue; }

                if (f.IsStyleAvailable(FontStyle.Regular)) {
                    //var fo = new Font(f.Name, 100);
                    try {
                        //fo.MeasureString("T");
                        _fnList.Add(ItemOf(string.Empty, f.Name, BlueFont.Get(f, 12).NameInStyle(), true));
                    } catch { }
                }
            }

            _fsList = [
                ItemOf("8", SortierTyp.ZahlenwertFloat),
                ItemOf("9", SortierTyp.ZahlenwertFloat),
                ItemOf("10", SortierTyp.ZahlenwertFloat),
                ItemOf("11", SortierTyp.ZahlenwertFloat),
                ItemOf("12", SortierTyp.ZahlenwertFloat),
                ItemOf("14", SortierTyp.ZahlenwertFloat),
                ItemOf("16", SortierTyp.ZahlenwertFloat),
                ItemOf("18", SortierTyp.ZahlenwertFloat),
                ItemOf("20", SortierTyp.ZahlenwertFloat),
                ItemOf("22", SortierTyp.ZahlenwertFloat),
                ItemOf("24", SortierTyp.ZahlenwertFloat),
                ItemOf("26", SortierTyp.ZahlenwertFloat),
                ItemOf("28", SortierTyp.ZahlenwertFloat),
                ItemOf("36", SortierTyp.ZahlenwertFloat),
                ItemOf("48", SortierTyp.ZahlenwertFloat),
                ItemOf("72", SortierTyp.ZahlenwertFloat)
            ];
        }

        lstName.ItemAddRange(_fnList);
        listSize.ItemAddRange(_fsList);
    }

    protected override void OnVisibleChanged(System.EventArgs e) {
        base.OnVisibleChanged(e);
        UpdateSampleText();
    }

    protected override void SetEnabledState(bool enabled) {
        base.SetEnabledState(enabled);
        lstName.Enabled = enabled;
        listSize.Enabled = enabled;
        chkFett.Enabled = enabled;
        chkKursiv.Enabled = enabled;
        chkUnterstrichen.Enabled = enabled;
        chkDurchgestrichen.Enabled = enabled;
        chkOutline.Enabled = enabled;
        btnFontColor.Enabled = enabled;
        btnOutlineColor.Enabled = enabled;
        btnBackColor.Enabled = enabled;
    }

    protected override bool SetValuesToFormula(object? toEdit) {
        if (toEdit is not BlueFont { } bf) { return false; }

        if (lstName[bf.FontName] is null) { lstName.ItemAdd(ItemOf(bf.FontName, bf.FontName, QuickImage.Get(ImageCode.Warnung, 20))); }
        if (listSize[bf.Size.ToString1_2()] is null) { listSize.ItemAdd(ItemOf(bf.Size.ToString1_2())); }

        lstName.UncheckAll();
        lstName.Check(bf.FontName);
        listSize.UncheckAll();
        listSize.Check(bf.Size.ToString1_2());
        chkFett.Checked = bf.Bold;
        chkKursiv.Checked = bf.Italic;
        chkUnterstrichen.Checked = bf.Underline;
        chkDurchgestrichen.Checked = bf.StrikeOut;
        chkOutline.Checked = bf.ColorOutline.A > 0;
        btnFontColor.ImageCode = QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, bf.ColorMain).KeyName;
        btnOutlineColor.ImageCode = QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, bf.ColorOutline).KeyName;
        btnBackColor.ImageCode = QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, bf.ColorBack).KeyName;

        return true;
    }

    private void btnBackColor_Click(object sender, System.EventArgs e) {
        ColorDia.Color = QuickImage.Get(btnOutlineColor.ImageCode).ChangeGreenTo ?? Color.Transparent;
        ColorDia.ShowDialog();
        btnBackColor.ImageCode = QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, ColorDia.Color).KeyName;
        UpdateSampleText();
    }

    private void cFarbe_Click(object sender, System.EventArgs e) {
        ColorDia.Color = QuickImage.Get(btnFontColor.ImageCode).ChangeGreenTo ?? Color.Transparent;
        ColorDia.ShowDialog();
        btnFontColor.ImageCode = QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, ColorDia.Color).KeyName;
        UpdateSampleText();
    }

    private void cRandF_Click(object sender, System.EventArgs e) {
        ColorDia.Color = QuickImage.Get(btnOutlineColor.ImageCode).ChangeGreenTo ?? Color.Transparent;
        ColorDia.ShowDialog();
        btnOutlineColor.ImageCode = QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, ColorDia.Color).KeyName;
        UpdateSampleText();
    }

    private void FName_Item_CheckedChanged(object sender, System.EventArgs e) => UpdateSampleText();

    private void style_CheckedChanged(object sender, System.EventArgs e) => UpdateSampleText();

    private void UpdateSampleText() {
        if (((IIsEditor)this).OutputItem is not BlueFont { } bf) { return; }

        preview.Image = bf.SampleText()?.CloneFromBitmap();
    }

    #endregion
}
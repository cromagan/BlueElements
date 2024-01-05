// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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

#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionList;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using BlueDatabase;
using BlueDatabase.Enums;
using static BlueBasics.Converter;

namespace BlueControls.ItemCollectionPad.FunktionsItems_Formular;

/// <summary>
/// Erzeut ein FlexiControllForCell
/// Standard-Bearbeitungs-Feld
/// </summary>
public class EditFieldPadItem : FakeControlPadItem, IReadableText, IItemToControl, IItemAcceptSomething, IHasVersion, IAutosizable {

    #region Fields

    private readonly ItemAcceptSomething _itemAccepts;

    private EditTypeFormula _bearbeitung = EditTypeFormula.Textfeld;

    private ColumnItem? _column;

    private string _columnName = string.Empty;

    private ÜberschriftAnordnung _überschriftanordung = ÜberschriftAnordnung.Über_dem_Feld;

    #endregion

    #region Constructors

    public EditFieldPadItem(string internalname) : base(internalname) => _itemAccepts = new();

    #endregion

    #region Properties

    public static string ClassId => "FI-EditField";

    public bool AutoSizeableHeight {
        get {
            if (_bearbeitung == EditTypeFormula.nur_als_Text_anzeigen) {
                return (int)UsedArea.Height > IAutosizableExtension.MinHeigthCaption;
            }

            if (_überschriftanordung is ÜberschriftAnordnung.Links_neben_Dem_Feld or ÜberschriftAnordnung.ohne) {
                return (int)UsedArea.Height > IAutosizableExtension.MinHeigthTextBox;
            }

            return (int)UsedArea.Height > IAutosizableExtension.MinHeigthCapAndBox;
        }
    }

    public ÜberschriftAnordnung CaptionPosition {
        get => _überschriftanordung;
        set {
            if (IsDisposed) { return; }
            if (_überschriftanordung == value) { return; }
            this.RaiseVersion();
            _überschriftanordung = value;
            OnChanged();
        }
    }

    public ColumnItem? Column {
        get {
            _column ??= DatabaseInput?.Column.Exists(_columnName);

            return _column;
        }
        set {
            if (IsDisposed) { return; }
            var tmpn = value?.KeyName ?? string.Empty;
            if (_columnName == tmpn) { return; }
            _columnName = tmpn;
            _column = null;
            OnChanged();
        }
    }

    public Database? DatabaseInput => _itemAccepts.DatabaseInput(this);
    public Database? DatabaseInputMustBe => null;
    public override string Description => "Standard Bearbeitungs-Steuerelement für Zellen.";

    public EditTypeFormula EditType {
        get => _bearbeitung;
        set {
            if (IsDisposed) { return; }
            if (_bearbeitung == value) { return; }
            this.RaiseVersion();
            _bearbeitung = value;
            OnChanged();
        }
    }

    public List<int> InputColorId => _itemAccepts.InputColorIdGet(this);

    public string Interner_Name {
        get {
            if (Column == null || Column.IsDisposed) { return "?"; }
            return Column.KeyName;
        }
    }

    public override bool MustBeInDrawingArea => true;

    public bool MustBeOneRow => true;

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ReadOnlyCollection<string> Parents {
        get => _itemAccepts.GetFilterFromKeysGet();
        set => _itemAccepts.GetFilterFromKeysSet(value, this);
    }

    public string Spalten_AdminInfo {
        get {
            if (Column != null && !Column.IsDisposed) { return Column.AdminInfo; }
            return string.Empty;
        }
        set {
            if (Column != null && !Column.IsDisposed) { Column.AdminInfo = value; }
        }
    }

    public string Spalten_QuickInfo {
        get {
            if (Column != null && !Column.IsDisposed) { return Column.Quickinfo; }
            return string.Empty;
        }
        set {
            if (Column != null && !Column.IsDisposed) { Column.Quickinfo = value; }
        }
    }

    public bool WaitForDatabase => false;

    #endregion

    #region Methods

    //public Bitmap? Bitmap { get; set; }
    //public enSizeModes Bild_Modus { get; set; }
    public static List<AbstractListItem> GetAllowedEditTypes(ColumnItem? column) {
        var l = new List<AbstractListItem>();
        if (column == null || column.IsDisposed) { return l; }
        var t = typeof(EditTypeFormula);

        foreach (int z1 in Enum.GetValues(t)) {
            if (column.UserEditDialogTypeInFormula((EditTypeFormula)z1)) {
                l.Add(new TextListItem(Enum.GetName(t, z1).Replace("_", " "), z1.ToString(), null, false, true, string.Empty));
            }
        }
        return l;
    }

    public void CalculateInputColorIds() => _itemAccepts.CalculateInputColorIds(this);

    public override System.Windows.Forms.Control CreateControl(ConnectedFormulaView parent) {
        //var ff = parent.SearchOrGenerate(rfw2);

        var con = new FlexiControlForCell();
        con.ColumnName = Column?.KeyName ?? string.Empty;
        con.EditType = EditType;
        con.CaptionPosition = CaptionPosition;

        con.DoInputSettings(parent, this);
        //con.DoOutputSettings(this);
        return con;
    }

    public override string ErrorReason() {
        var b = base.ErrorReason();
        if (!string.IsNullOrEmpty(b)) { return b; }

        b = _itemAccepts.ErrorReason(this);
        if (!string.IsNullOrEmpty(b)) { return b; }

        //b = _itemSends.ErrorReason(this);
        //if (!string.IsNullOrEmpty(b)) { return b; }

        if (Column == null || Column.IsDisposed) {
            return "Spalte fehlt";
        }

        return string.Empty;
    }

    public override List<GenericControl> GetStyleOptions(int widthOfControl) {
        List<GenericControl> l = [.. _itemAccepts.GetStyleOptions(this, widthOfControl)];

        if (DatabaseInput is not Database db || db.IsDisposed) { return l; }

        l.Add(new FlexiControlForDelegate(Spalte_wählen, "Spalte wählen", ImageCode.Pfeil_Rechts));

        if (Column == null || Column.IsDisposed) { return l; }
        l.Add(new FlexiControlForProperty<string>(() => Interner_Name));
        l.Add(new FlexiControlForDelegate(Spalte_bearbeiten, "Spalte bearbeiten", ImageCode.Spalte));

        var u = new ItemCollectionList.ItemCollectionList(false);
        u.AddRange(typeof(ÜberschriftAnordnung));
        l.Add(new FlexiControlForProperty<ÜberschriftAnordnung>(() => CaptionPosition, u));
        var b = new ItemCollectionList.ItemCollectionList(false);
        b.AddRange(GetAllowedEditTypes(Column));
        l.Add(new FlexiControlForProperty<EditTypeFormula>(() => EditType, b));
        l.Add(new FlexiControl());
        l.Add(new FlexiControlForProperty<string>(() => Spalten_QuickInfo, 5));
        l.Add(new FlexiControlForProperty<string>(() => Spalten_AdminInfo, 5));

        l.Add(new FlexiControl());
        l.AddRange(base.GetStyleOptions(widthOfControl));
        return l;
    }

    public override void ParseFinished(string parsed) {
        base.ParseFinished(parsed);
        //_itemSends.ParseFinished(this);
        _itemAccepts.ParseFinished(this);
    }

    public override bool ParseThis(string tag, string value) {
        if (base.ParseThis(tag, value)) { return true; }
        if (_itemAccepts.ParseThis(tag, value)) { return true; }

        switch (tag) {
            case "column":
                //Column = GetRowFrom?.Database?.Column.SearchByKey(LongParse(value));
                return true;

            case "columnname":
                _columnName = value;
                return true;

            case "edittype":
                _bearbeitung = (EditTypeFormula)IntParse(value);
                return true;

            case "caption":
                _überschriftanordung = (ÜberschriftAnordnung)IntParse(value);
                return true;
        }
        return false;
    }

    public override string ReadableText() {
        const string txt = "Wert aus: ";

        if (this.IsOk() && Column != null) {
            return txt + Column.Caption;
        }

        return txt + ErrorReason();
    }

    public void Spalte_bearbeiten() {
        if (IsDisposed) { return; }
        if (Column == null || Column.IsDisposed) { return; }
        TableView.OpenColumnEditor(Column, null, null);

        OnChanged();
    }

    [Description("Wählt die Spalte, die angezeigt werden soll.\r\nDiese bestimmt maßgeblich die Eigenschaften")]
    public void Spalte_wählen() {
        if (IsDisposed) { return; }

        if (DatabaseInput is not Database db || db.IsDisposed) {
            MessageBox.Show("Quelle fehlerhaft!");
            return;
        }

        var lst = new ItemCollectionList.ItemCollectionList(true);
        lst.AddRange(db.Column, false);
        //lst.Sort();

        var sho = InputBoxListBoxStyle.Show("Spalte wählen:", lst, AddType.None, true);

        if (sho == null || sho.Count != 1) { return; }

        var col = db.Column.Exists(sho[0]);

        if (col == Column) { return; }
        Column = col;
        this.RaiseVersion();
        UpdateSideOptionMenu();
        OnChanged();
    }

    public override QuickImage? SymbolForReadableText() {
        if (this.IsOk() && Column != null) {
            return Column.SymbolForReadableText();

            //return QuickImage.Get(ImageCode.Zeile, 16, Color.Transparent, Skin.IdColor(InputColorId));
        }

        return QuickImage.Get(ImageCode.Warnung, 16);
    }

    public override string ToString() {
        if (IsDisposed) { return string.Empty; }
        List<string> result = [.. _itemAccepts.ParsableTags()];

        result.ParseableAdd("ColumnName", _columnName);
        result.ParseableAdd("EditType", _bearbeitung);
        result.ParseableAdd("Caption", _überschriftanordung);
        return result.Parseable(base.ToString());
    }

    internal override void AddedToCollection() {
        base.AddedToCollection();
        //_itemSends.DoCreativePadAddedToCollection(this);
        _itemAccepts.DoCreativePadAddedToCollection(this);
        //RepairConnections();
    }

    protected override void DrawExplicit(Graphics gr, RectangleF positionModified, float zoom, float shiftX, float shiftY, bool forPrinting) {
        if (!forPrinting) {
            DrawColorScheme(gr, positionModified, zoom, InputColorId, false, false, false);
        }

        //if (Column  ==null || Column .IsDisposed) {
        //    Skin.Draw_FormatedText(gr, "Spalte fehlt", QuickImage.Get(ImageCode.Warnung, (int)(16 * zoom)), Alignment.Horizontal_Vertical_Center, positionModified.ToRect(), CaptionFnt.Scale(zoom), true);
        //} else {
        DrawFakeControl(gr, positionModified, zoom, CaptionPosition, Column?.ReadableText() + ":", EditType);
        //}

        if (!forPrinting) {
            DrawColorScheme(gr, positionModified, zoom, InputColorId, true, true, true);
        }

        base.DrawExplicit(gr, positionModified, zoom, shiftX, shiftY, forPrinting);

        DrawArrorInput(gr, positionModified, zoom, shiftX, shiftY, forPrinting, InputColorId);
    }

    #endregion

    //public bool ReplaceVariable(Variable variable) {
    //    if (string.IsNullOrEmpty(Platzhalter_Für_Layout)) { return false; }
    //    if ("~" + variable.Name.ToLower() + "~" != Platzhalter_Für_Layout.ToLower()) { return false; }
    //    if (variable is not VariableBitmap vbmp) { return false; }
    //    var ot = vbmp.ValueBitmap;
    //    if (ot is Bitmap bmp) {
    //        Bitmap = bmp;
    //        OnChanged();
    //        return true;
    //    }
    //protected override AbstractPadItem? TryCreate(string id, string name) {
    //    if (id.Equals(ClassId, StringComparison.OrdinalIgnoreCase)) {
    //        return new EditFieldPadItem(name);
    //    }
    //    return null;
    //}

    // // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
    // ~BitmapPadItem()
    // {
    //     // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
    //     Dispose(disposing: false);
    // }
    //public override void DoStyleCommands(object sender, List<string> Tags, ref bool CloseMenu)
    //{
    //    base.DoStyleCommands(sender, Tags, ref CloseMenu);
    //    if (Tags.TagGet("Bildschirmbereich wählen").FromPlusMinus())
    //    {
    //        CloseMenu = false;
    //        if (Bitmap != null)
    //        {
    //            if (MessageBox.Show("Vorhandenes Bild überschreiben?", ImageCode.Warnung, "Ja", "Nein") != 0) { return; }
    //        }
    //        Bitmap = ScreenShot.GrabArea(null, 2000, 2000).Pic;
    //        return;
    //    }
    //    if (Tags.TagGet("Datei laden").FromPlusMinus())
    //    {
    //        CloseMenu = false;
    //        if (Bitmap != null)
    //        {
    //            if (MessageBox.Show("Vorhandenes Bild überschreiben?", ImageCode.Warnung, "Ja", "Nein") != 0) { return; }
    //        }
    //        var e = new System.Windows.Forms.OpenFileDialog();
    //        e.CheckFileExists = true;
    //        e.Multiselect = false;
    //        e.Title = "Bild wählen:";
    //        e.Filter = "PNG Portable Network Graphics|*.png|JPG Jpeg Interchange|*.jpg|Bmp Windows Bitmap|*.bmp";
    //        e.ShowDialog();
    //        if (!FileExists(e.FileName))
    //        {
    //            return;
    //        }
    //        Bitmap = (Bitmap)Generic.Image_FromFile(e.FileName);
    //        return;
    //    }
    //    if (Tags.TagGet("Skalieren").FromPlusMinus())
    //    {
    //        CloseMenu = false;
    //        var t = InputBox.Show("Skalierfaktor oder Formel eingeben:", "1", DataFormat.Text);
    //        var sc = modErgebnis.Ergebnis(t);
    //        if (sc == null || sc == 1)
    //        {
    //            Notification.Show("Keine Änderung vorgenommen.");
    //            return;
    //        }
    //        var x = p_RU.X - p_LO.X;
    //        var y = p_RU.Y - p_LO.Y;
    //        p_RU.X = (float)((float)p_LO.X + (float)x * sc);
    //        p_RU.Y = (float)((float)p_LO.Y + (float)y * sc);
    //        KeepInternalLogic();
    //        return;
    //    }
    //    Hintergrund_weiß_füllen = Tags.TagGet("Hintergrund weiß füllen").FromPlusMinus();
    //    Bild_Modus = (enSizeModes)IntParse(Tags.TagGet("Bild-Modus"));
    //    Stil = (PadStyles)IntParse(Tags.TagGet("Umrandung"));
    //    Platzhalter_für_Layout = Tags.TagGet("Platzhalter für Layout").FromNonCritical();
    //}
}
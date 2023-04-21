// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueControls.ItemCollection.ItemCollectionList;
using BlueDatabase;
using BlueDatabase.Enums;
using static BlueBasics.Converter;
using MessageBox = BlueControls.Forms.MessageBox;

namespace BlueControls.ItemCollection;

/// <summary>
/// Erzeut ein FlexiControllForCell
/// Standard-Bearbeitungs-Feld
/// </summary>
public class EditFieldPadItem : FakeControlPadItem, IReadableText, IItemToControl, IItemAcceptRow, IHasVersion {

    #region Fields

    private readonly ItemAcceptRow _itemAccepts;

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

    public ÜberschriftAnordnung CaptionPosition {
        get => _überschriftanordung;
        set {
            if (_überschriftanordung == value) { return; }
            this.RaiseVersion();
            _überschriftanordung = value;
            OnChanged();
        }
    }

    public ColumnItem? Column {
        get {
            _column ??= GetRowFrom?.OutputDatabase?.Column.Exists(_columnName);

            return _column;
        }
        set {
            var tmpn = value?.Name ?? string.Empty;
            if (_columnName == tmpn) { return; }
            _columnName = tmpn;
            _column = null;
            OnChanged();
        }
    }

    public EditTypeFormula EditType {
        get => _bearbeitung;
        set {
            if (_bearbeitung == value) { return; }
            this.RaiseVersion();
            _bearbeitung = value;
            OnChanged();
        }
    }

    public IItemSendRow? GetRowFrom {
        get => _itemAccepts.GetRowFromGet(this);
        set => _itemAccepts.GetRowFromSet(value, this);
    }

    public override int InputColorId {
        get => _itemAccepts.InputColorIdGet();
        set => _itemAccepts.InputColorIdSet(this, value);
    }

    public string Interner_Name {
        get {
            if (Column == null) { return "?"; }
            return Column.Name;
        }
    }

    public string Spalten_AdminInfo {
        get {
            if (Column != null) { return Column.AdminInfo; }
            return string.Empty;
        }
        set {
            if (Column != null) { Column.AdminInfo = value; }
        }
    }

    public string Spalten_QuickInfo {
        get {
            if (Column != null) { return Column.Quickinfo; }
            return string.Empty;
        }
        set {
            if (Column != null) { Column.Quickinfo = value; }
        }
    }

    #endregion

    #region Methods

    //public Bitmap? Bitmap { get; set; }
    //public enSizeModes Bild_Modus { get; set; }
    public static List<BasicListItem> GetAllowedEditTypes(ColumnItem? column) {
        var l = new List<BasicListItem>();
        if (column == null) { return l; }
        var t = typeof(EditTypeFormula);

        foreach (int z1 in Enum.GetValues(t)) {
            if (column.UserEditDialogTypeInFormula((EditTypeFormula)z1)) {
                l.Add(new TextListItem(Enum.GetName(t, z1).Replace("_", " "), z1.ToString(), null, false, true, string.Empty));
            }
        }
        return l;
    }

    public override Control CreateControl(ConnectedFormulaView parent) {
        //var ff = parent.SearchOrGenerate(rfw2);

        var con = new FlexiControlForCell();
        con.SetData(Column?.Database, -1);
        con.ColumnName = Column?.Name ?? string.Empty;
        con.EditType = EditType;
        con.CaptionPosition = CaptionPosition;

        con.DoInputSettings(parent, this);
        //con.DoOutputSettings(this);
        return con;

        //var cy = new FlexiControl();
        //if (Column == null) {
        //    cy.Caption = "?Kein Bezug?:";
        //} else {
        //    cy.Caption = Column.ReadableText() + ":";
        //}

        //cy.EditType = EditType;
        //cy.CaptionPosition = CaptionPosition;
        //cy.DisabledReason = "Keine Verknüpfung vorhanden.";
        //cy.Tag = KeyName;
        ////cy.Name = DefaultItemToControlName();
        //return cy;
    }

    public override List<GenericControl> GetStyleOptions() {
        List<GenericControl> l = new();

        l.AddRange(_itemAccepts.GetStyleOptions(this));

        l.Add(new FlexiControlForDelegate(Spalte_wählen, "Spalte wählen", ImageCode.Pfeil_Rechts));
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
        l.AddRange(base.GetStyleOptions());
        return l;
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

    public string ReadableText() {
        if (Column != null) {
            return "Wert aus: " + Column.ReadableText();

            //if (Genau_eine_Zeile) {
            //    return "(eine) Zeile aus: " + Database.Caption;
            //} else {
            //    return "Zeilen aus: " + Database.Caption;
            //}
        }

        return "Wert einer Spalte";
    }

    public void Spalte_bearbeiten() {
        if (Column == null) { return; }
        TableView.OpenColumnEditor(Column, null, null);

        OnChanged();
    }

    //public bool Hintergrund_Weiß_Füllen { get; set; }
    [Description("Wählt die Spalte, die angezeigt werden soll.\r\nDiese bestimmt maßgeblich die Eigenschaften")]
    public void Spalte_wählen() {
        if (GetRowFrom == null) {
            MessageBox.Show("Zuerst Datenquelle wählen.");
            return;
        }

        if (GetRowFrom.OutputDatabase == null) {
            MessageBox.Show("Quelle fehlerhaft!");
            return;
        }

        var lst = new ItemCollectionList.ItemCollectionList(true);
        lst.AddRange(GetRowFrom.OutputDatabase.Column, false);
        //lst.Sort();

        var sho = InputBoxListBoxStyle.Show("Spalte wählen:", lst, AddType.None, true);

        if (sho == null || sho.Count != 1) { return; }

        var col = GetRowFrom.OutputDatabase.Column.Exists(sho[0]);

        if (col == Column) { return; }
        Column = col;
        this.RaiseVersion();
        OnChanged();
    }

    public QuickImage? SymbolForReadableText() {
        if (GetRowFrom == null) { return null; }

        return QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, Skin.IDColor(GetRowFrom.OutputColorId));
    }

    public override string ToString() {
        var result = new List<string>();

        result.AddRange(_itemAccepts.ParsableTags());

        result.ParseableAdd("ColumnName", _columnName);
        result.ParseableAdd("EditType", _bearbeitung);
        result.ParseableAdd("Caption", _überschriftanordung);
        return result.Parseable(base.ToString());
    }

    protected override void DrawExplicit(Graphics gr, RectangleF positionModified, float zoom, float shiftX, float shiftY, bool forPrinting) {
        if (!forPrinting) {
            DrawColorScheme(gr, positionModified, zoom, InputColorId);
        }

        if (GetRowFrom == null) {
            Skin.Draw_FormatedText(gr, "Datenquelle fehlt", QuickImage.Get(ImageCode.Warnung, (int)(16 * zoom)), Alignment.Horizontal_Vertical_Center, positionModified.ToRect(), CaptionFnt.Scale(zoom), true);
        } else if (Column == null) {
            Skin.Draw_FormatedText(gr, "Spalte fehlt", QuickImage.Get(ImageCode.Warnung, (int)(16 * zoom)), Alignment.Horizontal_Vertical_Center, positionModified.ToRect(), CaptionFnt.Scale(zoom), true);
        } else {
            DrawFakeControl(gr, positionModified, zoom, CaptionPosition, Column.ReadableText() + ":");
        }

        base.DrawExplicit(gr, positionModified, zoom, shiftX, shiftY, forPrinting);

        RowEntryPadItem.DrawInputArrow(gr, positionModified, zoom, shiftX, shiftY, forPrinting, "Zeile", InputColorId);
    }

    protected override void OnParentChanged() {
        base.OnParentChanged();
        //_itemSends.DoCreativePadParentChanged(this);
        _itemAccepts.DoCreativePadParentChanged(this);
        //RepairConnections();
    }

    protected override void ParseFinished() {
        base.ParseFinished();
        //_itemSends.ParseFinished(this);
        _itemAccepts.ParseFinished(this);
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
    //protected override BasicPadItem? TryCreate(string id, string name) {
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
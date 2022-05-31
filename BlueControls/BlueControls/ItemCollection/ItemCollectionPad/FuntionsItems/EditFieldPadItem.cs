// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using MessageBox = BlueControls.Forms.MessageBox;
using static BlueBasics.Converter;
using BlueDatabase;
using BlueDatabase.Enums;

using BlueBasics.Interfaces;
using BlueControls.Interfaces;

namespace BlueControls.ItemCollection;

public class EditFieldPadItem : CustomizableShowPadItem, IReadableText, IAcceptAndSends, IContentHolder, IItemToControl {

    #region Fields

    public ColumnItem? Column = null;
    private EditTypeFormula _bearbeitung = EditTypeFormula.Textfeld;

    private ÜberschriftAnordnung _überschiftanordung = ÜberschriftAnordnung.Über_dem_Feld;

    #endregion

    #region Constructors

    public EditFieldPadItem(string internalname) : base(internalname) { }

    #endregion

    #region Properties

    public ÜberschriftAnordnung CaptionPosition {
        get => _überschiftanordung;
        set {
            if (_überschiftanordung == value) { return; }
            _überschiftanordung = value;
            OnChanged();
        }
    }

    public EditTypeFormula EditType {
        get => _bearbeitung;
        set {
            if (_bearbeitung == value) { return; }
            _bearbeitung = value;
            OnChanged();
        }
    }

    public string Interner_Name {
        get {
            if (Column == null) { return "?"; }
            return Column.Name;
        }
    }

    public string Spalte_bearbeiten {
        get => string.Empty;
        set {
            if (Column == null) { return; }
            Forms.TableView.OpenColumnEditor(Column, null, null);

            OnChanged();
        }
    }

    //public bool Hintergrund_Weiß_Füllen { get; set; }
    [Description("Wählt die Spalte, die angezeigt werden soll.\r\nDiese bestimmt maßgeblich die Eigenschaften")]
    public string Spalte_wählen {
        get => string.Empty;
        set {
            if (GetRowFrom == null) {
                MessageBox.Show("Zuerst Datenquelle wählen.");
                return;
            }

            if (GetRowFrom.Database == null) {
                MessageBox.Show("Quelle fehlerhaft!");
                return;
            }

            var lst = new ItemCollectionList.ItemCollectionList();
            lst.AddRange(GetRowFrom.Database.Column, false);

            var sho = Forms.InputBoxListBoxStyle.Show("Spalte wählen:", lst, AddType.None, true);

            if (sho == null || sho.Count != 1) { return; }

            var k = IntParse(sho[0]);

            var col = GetRowFrom.Database.Column.SearchByKey(k);

            if (col == Column) { return; }
            Column = col;

            OnChanged();
        }
    }

    //public Bitmap? Bitmap { get; set; }
    public string Spalten_AdminInfo {
        get {
            if (Column != null) { return Column.AdminInfo; }
            return string.Empty;
        }
        set {
            if (Column != null) { Column.AdminInfo = value; }
        }
    }

    //public enSizeModes Bild_Modus { get; set; }
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

    public override System.Windows.Forms.Control CreateControl(ConnectedFormulaView parent) {
        if (GetRowFrom is ICalculateOneRowItemLevel rfw2) {
            var ff = parent.SearchOrGenerate((BasicPadItem)rfw2);

            var cx = new FlexiControlForCell();
            cx.Database = Column.Database;
            cx.ColumnKey = Column.Key;
            cx.EditType = EditType;
            cx.CaptionPosition = CaptionPosition;
            cx.Tag = Internal;
            if (ff is ICalculateRowsControlLevel cc) { cc.Childs.Add(cx); }
            return cx;
        }

        var cy = new FlexiControl();
        cy.Caption = Column.ReadableText() + ":";
        cy.EditType = EditType;
        cy.CaptionPosition = CaptionPosition;
        cy.DisabledReason = "Keine Verknüpfung vorhanden.";
        cy.Tag = Internal;
        return cy;
    }

    public override List<GenericControl> GetStyleOptions() {
        List<GenericControl> l = new();

        l.AddRange(base.GetStyleOptions());
        l.Add(new FlexiControlForProperty<string>(() => Spalte_wählen, ImageCode.Pfeil_Rechts));
        l.Add(new FlexiControlForProperty<string>(() => Interner_Name));
        l.Add(new FlexiControlForProperty<string>(() => Spalte_bearbeiten, ImageCode.Spalte));

        var u = new ItemCollection.ItemCollectionList.ItemCollectionList();
        u.AddRange(typeof(ÜberschriftAnordnung));
        l.Add(new FlexiControlForProperty<ÜberschriftAnordnung>(() => CaptionPosition, u));
        var b = new ItemCollection.ItemCollectionList.ItemCollectionList();
        b.AddRange(Formula.GetAllowedEditTypes(Column));
        l.Add(new FlexiControlForProperty<EditTypeFormula>(() => EditType, b));
        l.Add(new FlexiControl());
        l.Add(new FlexiControlForProperty<string>(() => Spalten_QuickInfo, 5));
        l.Add(new FlexiControlForProperty<string>(() => Spalten_AdminInfo, 5));
        l.Add(new FlexiControl());

        //if(_getValueFrom != null && _getValueFrom.Database is Database db) {
        //      foreach(var thisC in db) {
        //          s
        //      }

        //  }

        //{
        //    new FlexiControlForProperty(()=> this.Bildschirmbereich_wählen", ImageCode.Bild),
        //    new FlexiControlForProperty(()=> this.Datei_laden", ImageCode.Ordner),
        //    new FlexiControl(),
        //    new FlexiControlForProperty(()=> this.Platzhalter_für_Layout", 2),
        //    new FlexiControl()
        //};
        //ItemCollectionList.ItemCollectionList comms = new()
        //{
        //    { "Abschneiden", ((int)enSizeModes.BildAbschneiden).ToString(), QuickImage.Get("BildmodusAbschneiden|32") },
        //    { "Verzerren", ((int)enSizeModes.Verzerren).ToString(), QuickImage.Get("BildmodusVerzerren|32") },
        //    { "Einpassen", ((int)enSizeModes.EmptySpace).ToString(), QuickImage.Get("BildmodusEinpassen|32") }
        //};
        //l.Add(new FlexiControlForProperty(()=> this.Bild-Modus", comms));
        //l.Add(new FlexiControl());
        //AddLineStyleOption(l);
        //l.Add(new FlexiControlForProperty(()=> this.Hintergrund_weiß_füllen"));
        //l.AddRange(base.GetStyleOptions());
        return l;
    }

    public bool IsRecursiveWith(IAcceptAndSends obj) {
        if (obj == this) { return true; }

        if (GetRowFrom is IAcceptAndSends i) { return i.IsRecursiveWith(obj); }
        return false;
    }

    public override bool ParseThis(string tag, string value) {
        if (base.ParseThis(tag, value)) { return true; }

        switch (tag) {
            case "column":
                Column = GetRowFrom.Database.Column.SearchByKey(IntParse(value));
                return true;

            case "edittype":
                _bearbeitung = (EditTypeFormula)IntParse(value);
                return true;

            case "caption":
                _überschiftanordung = (ÜberschriftAnordnung)IntParse(value);
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

    public QuickImage? SymbolForReadableText() {
        if (GetRowFrom == null) { return null; }

        return QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, Skin.IDColor(GetRowFrom.Id));
    }

    public override string ToString() {
        var t = base.ToString();
        t = t.Substring(0, t.Length - 1) + ", ";

        if (Column != null) {
            t = t + "Column=" + Column.Key + ", ";
        }

        t = t + "EditType=" + ((int)_bearbeitung).ToString() + ", ";
        t = t + "Caption=" + ((int)_überschiftanordung).ToString() + ", ";

        return t.Trim(", ") + "}";
    }

    protected override string ClassId() => "FI-EditField";

    protected override void DrawExplicit(Graphics gr, RectangleF positionModified, float zoom, float shiftX, float shiftY, bool forPrinting) {
        var id = -1;
        if (GetRowFrom != null) { id = GetRowFrom.Id; }

        if (!forPrinting) {
            DrawColorScheme(gr, positionModified, zoom, id);
        }

        if (GetRowFrom == null) {
            Skin.Draw_FormatedText(gr, "Datenquelle fehlt", QuickImage.Get(ImageCode.Warnung, (int)(16 * zoom)), Alignment.Horizontal_Vertical_Center, positionModified.ToRect(), CaptionFnt.Scale(zoom), true);
        } else if (Column == null) {
            Skin.Draw_FormatedText(gr, "Spalte fehlt", QuickImage.Get(ImageCode.Warnung, (int)(16 * zoom)), Alignment.Horizontal_Vertical_Center, positionModified.ToRect(), CaptionFnt.Scale(zoom), true);
        } else {
            base.DrawFakeControl(positionModified, zoom, CaptionPosition, gr, Column.ReadableText());
        }

        base.DrawExplicit(gr, positionModified, zoom, shiftX, shiftY, forPrinting);
    }

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
    protected override BasicPadItem? TryCreate(string id, string name) {
        if (id.Equals(ClassId(), StringComparison.OrdinalIgnoreCase)) {
            return new EditFieldPadItem(name);
        }
        return null;
    }

    private void RepairConnections() {
        ConnectsTo.Clear();

        if (GetRowFrom != null) {
            ConnectsTo.Add(new ItemConnection(ConnectionType.Top, true, (BasicPadItem)GetRowFrom, ConnectionType.Bottom, false));
        }
    }

    #endregion

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
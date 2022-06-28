﻿// Authors:
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
using BlueDatabase;
using System;
using System.Collections.Generic;
using System.Drawing;
using BlueBasics.Interfaces;
using System.ComponentModel;
using BlueDatabase.Enums;
using static BlueBasics.Converter;
using BlueControls.Interfaces;
using System.Windows.Forms;
using BlueControls.ConnectedFormula;

namespace BlueControls.ItemCollection;

public class RowWithFilterPaditem : RectanglePadItem, IReadableText, IAcceptAndSends, ICalculateOneRowItemLevel, IItemToControl {

    #region Fields

    public static BlueFont? CellFont = Skin.GetBlueFont(Design.Table_Cell, States.Standard);
    public static BlueFont? ChapterFont = Skin.GetBlueFont(Design.Table_Cell_Chapter, States.Standard);
    public static BlueFont? ColumnFont = Skin.GetBlueFont(Design.Table_Column, States.Standard);

    public readonly Database FilterDefiniton;

    public Table? FilterTable = null;
    private string _anzeige = string.Empty;
    private EditTypeFormula _bearbeitung = EditTypeFormula.Textfeld_mit_Auswahlknopf;
    private ÜberschriftAnordnung _überschiftanordung = ÜberschriftAnordnung.Über_dem_Feld;
    private string _überschrift = string.Empty;

    #endregion

    #region Constructors

    public RowWithFilterPaditem(Database? db, int id) : this(string.Empty, db, id) { }

    public RowWithFilterPaditem(string intern, Database? db, int id) : base(intern) {
        Database = db;

        Id = id;

        FilterTable = new Table();
        FilterTable.DropMessages = false;
        FilterTable.ShowWaitScreen = true;
        FilterTable.Size = new Size(968, 400);

        FilterDefiniton = GenerateFilterDatabase();
        FilterTable.Database = FilterDefiniton;

        FilterTable.Arrangement = 1;

        FilterDefiniton.Cell.CellValueChanged += Cell_CellValueChanged;
        FilterTable.ContextMenuInit += FilterTable_ContextMenuInit;
        FilterTable.ContextMenuItemClicked += Filtertable_ContextMenuItemClicked;
    }

    public RowWithFilterPaditem(string intern) : this(intern, null, 0) { }

    #endregion

    #region Properties

    [Description("Nach welchem Format die Zeilen angezeigt werden sollen. Es können Variablen im Format ~Variable~ benutzt werden. Achtung, KEINE Skript-Variaben, nur Spaltennamen.")]
    public string Anzeige {
        get => _anzeige;
        set {
            if (_anzeige == value) { return; }
            _anzeige = value;
            OnChanged();
        }
    }

    public ÜberschriftAnordnung CaptionPosition {
        get => _überschiftanordung;
        set {
            if (_überschiftanordung == value) { return; }
            _überschiftanordung = value;
            OnChanged();
        }
    }

    public Database? Database { get; set; }

    public string Datenbankkopf {
        get => string.Empty;
        set {
            if (Database == null) { return; }
            Forms.TableView.OpenDatabaseHeadEditor(Database);
        }
    }

    //public EditTypeFormula EditType {
    //    get => _bearbeitung;
    //    set {
    //        if (value != EditTypeFormula.Textfeld_mit_Auswahlknopf) { value = EditTypeFormula.Textfeld_mit_Auswahlknopf; }

    //        if (_bearbeitung == value) { return; }
    //        _bearbeitung = value;
    //        OnChanged();
    //    }
    //}

    public string Filter_hinzufügen {
        get => string.Empty;
        set {
            if (Database == null) { return; }

            var c = new ItemCollectionList.ItemCollectionList();
            foreach (var thiscol in Database.Column) {
                if (thiscol.Format.Autofilter_möglich() && !thiscol.Format.NeedTargetDatabase()) {
                    c.Add(thiscol);
                }
            }

            var t = Forms.InputBoxListBoxStyle.Show("Filter für welche Spalte?", c, AddType.None, true);

            if (t == null || t.Count != 1) { return; }

            var r = FilterDefiniton.Row.Add(t[0]);
            r.CellSet("FilterArt", "=");
        }
    }

    /// <summary>
    /// Laufende Nummer, bestimmt die Einfärbung
    /// </summary>
    public int Id { get; set; }

    public string Überschrift {
        get => _überschrift;
        set {
            if (_überschrift == value) { return; }
            _überschrift = value;
            OnChanged();
        }
    }

    //[Description("Mit dieser Verbindungs-ID können formularübergeifend Filter an\r\nan andere Filterelemnte übergeben werden bzw.\r\nempfangen werden.\r\nZiegt KEIN Pfeil auf dieses Element, übernimmt es den Wert.\r\nAndernfalls empfängt es den Wert.")]
    //public string VerbindungsID {
    //    get => _VerbindungsID;
    //    set {
    //        if (_VerbindungsID == value) { return; }
    //        _VerbindungsID = value;
    //        OnChanged();
    //    }
    //}

    protected override int SaveOrder => 1;

    #endregion

    #region Methods

    public Control CreateControl(ConnectedFormulaView parent) {
        var c = new FlexiControlRowSelector(Database, this.Parent, FilterDefiniton, _überschrift, _anzeige);
        c.EditType = _bearbeitung;
        c.CaptionPosition = CaptionPosition;
        c.Tag = Internal;
        return c;
    }

    public override List<GenericControl> GetStyleOptions() {
        List<GenericControl> l = new();
        if (Database == null) { return l; }
        l.Add(new FlexiControlForProperty<string>(() => Überschrift));
        l.Add(new FlexiControlForProperty<string>(() => Anzeige));
        //l.Add(new FlexiControlForProperty<String>(() => Variable));

        var u = new ItemCollection.ItemCollectionList.ItemCollectionList();
        u.AddRange(typeof(ÜberschriftAnordnung));
        l.Add(new FlexiControlForProperty<ÜberschriftAnordnung>(() => CaptionPosition, u));
        //var b = new ItemCollection.ItemCollectionList.ItemCollectionList();
        //b.AddRange(typeof(EditTypeFormula));
        //l.Add(new FlexiControlForProperty<EditTypeFormula>(() => EditType, b));
        l.Add(new FlexiControl());

        //l.Add(new FlexiControlForProperty<string>(() => Database.Caption));
        //l.Add(new FlexiControlForProperty(Database, "Caption"));
        l.Add(new FlexiControlForProperty<string>(() => Datenbankkopf, ImageCode.Datenbank));
        //l.Add(new FlexiControlForProperty(()=> this.Datenbankkopf"));
        //l.Add(new FlexiControl());
        //l.Add(new FlexiControlForProperty<string>(() => VerbindungsID));
        //l.Add(new FlexiControl());

        FilterDatabaseUpdate();
        l.Add(new FlexiControlForProperty<string>(() => Filter_hinzufügen, ImageCode.PlusZeichen));
        l.Add(FilterTable);

        //l.Add(new FlexiControl());
        //l.Add(new FlexiControlForProperty<string>(() => Column.Ueberschrift1"));
        //l.Add(new FlexiControlForProperty<string>(() => Column.Ueberschrift2"));
        //l.Add(new FlexiControlForProperty<string>(() => Column.Ueberschrift3"));
        //l.Add(new FlexiControl());
        //l.Add(new FlexiControlForProperty(Database.t, "Quickinfo"));
        //l.Add(new FlexiControlForProperty<string>(() => Column.AdminInfo"));

        //if (AdditionalStyleOptions != null) {
        //    l.Add(new FlexiControl());
        //    l.AddRange(AdditionalStyleOptions);
        //}

        return l;
    }

    public bool IsRecursiveWith(IAcceptAndSends obj) {
        if (obj == this) { return true; }

        foreach (var thisR in FilterDefiniton.Row) {
            var it = Parent[thisR.CellGetString("suchtxt")];
            if (it is IAcceptAndSends i) {
                if (i.IsRecursiveWith(obj)) { return true; }
            }
        }

        return false;
    }

    public override bool ParseThis(string tag, string value) {
        if (base.ParseThis(tag, value)) { return true; }
        switch (tag) {
            case "database":
                Database = Database.GetByFilename(value.FromNonCritical(), false, false);
                return true;

            //case "connectionid":
            //    VerbindungsID = value.FromNonCritical();
            //    return true;

            case "id":
                Id = IntParse(value);
                return true;

            case "filterdb":
                FilterDefiniton.Row.Clear();
                FilterDatabaseUpdate();
                FilterDefiniton.Import(value.FromNonCritical(), true, false, ";", false, false, false, string.Empty);
                return true;

            case "edittype":
                _bearbeitung = (EditTypeFormula)IntParse(value);
                return true;

            case "caption":
                _überschiftanordung = (ÜberschriftAnordnung)IntParse(value);
                return true;

            case "captiontext":
                _überschrift = value.FromNonCritical();
                return true;

            case "showformat":
                _anzeige = value.FromNonCritical();
                return true;
                //case "variable":
                //    _variable = value.FromNonCritical();
                //    return true;
        }
        return false;
    }

    public string ReadableText() {
        if (Database != null) {
            return "eine Zeile aus: " + Database.Caption;
        }

        return "Zeile einer Datenbank";
    }

    public QuickImage? SymbolForReadableText() {
        return QuickImage.Get(ImageCode.Kreis, 10, Color.Transparent, Skin.IDColor(Id));
    }

    public override string ToString() {
        var t = base.ToString();
        t = t.Substring(0, t.Length - 1) + ", ";

        t = t + "CaptionText=" + _überschrift.ToNonCritical() + ", ";
        t = t + "ShowFormat=" + _anzeige.ToNonCritical() + ", ";
        //t = t + "Variable=" + _variable.ToNonCritical() + ", ";

        t = t + "EditType=" + ((int)_bearbeitung).ToString() + ", ";
        t = t + "Caption=" + ((int)_überschiftanordung).ToString() + ", ";

        t = t + "ID=" + Id.ToString() + ", ";

        if (Database != null) {
            t = t + "Database=" + Database.Filename.ToNonCritical() + ", ";
        }

        //t = t + "ConnectionID=" + VerbindungsID.ToNonCritical() + ", ";

        if (FilterDefiniton != null) {
            t = t + "FilterDB=" + FilterDefiniton.Export_CSV(FirstRow.ColumnInternalName, (List<ColumnItem>)null, null).ToNonCritical() + ", ";
        }

        return t.Trim(", ") + "}";
    }

    protected override string ClassId() => "FI-RowWithFilter";

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);

        if (disposing) {
            FilterDefiniton.Cell.CellValueChanged -= Cell_CellValueChanged;
            FilterTable.ContextMenuInit -= FilterTable_ContextMenuInit;
            FilterTable.ContextMenuItemClicked -= Filtertable_ContextMenuItemClicked;
        }
    }

    protected override void DrawExplicit(Graphics gr, RectangleF modifiedPosition, float zoom, float shiftX, float shiftY, bool forPrinting) {
        DrawColorScheme(gr, modifiedPosition, zoom, Id);

        if (Database != null) {
            var txt = "eine Zeile aus\r\n";

            txt = txt + Database.Caption;

            //if (!string.IsNullOrEmpty(_VerbindungsID)) {
            //    txt = txt + "\r\nKlon-ID: " + _VerbindungsID;
            //}

            Skin.Draw_FormatedText(gr, txt, QuickImage.Get(ImageCode.Zeile, (int)(zoom * 16)), Alignment.Horizontal_Vertical_Center, modifiedPosition.ToRect(), ColumnPadItem.ColumnFont.Scale(zoom), false);
        }

        //gr.FillRectangle(new SolidBrush(Color.FromArgb(128, 255, 255, 255)), modifiedPosition);
        //drawingCoordinates.Inflate(-Padding, -Padding);
        //RectangleF r1 = new(drawingCoordinates.Left + Padding, drawingCoordinates.Top + Padding,
        //    drawingCoordinates.Width - (Padding * 2), drawingCoordinates.Height - (Padding * 2));
        //RectangleF r2 = new();
        //RectangleF r3 = new();
        //if (Bitmap != null) {
        //    r3 = new RectangleF(0, 0, Bitmap.Width, Bitmap.Height);
        //    switch (Bild_Modus) {
        //        case enSizeModes.Verzerren: {
        //                r2 = r1;
        //                break;
        //            }

        //        case enSizeModes.BildAbschneiden: {
        //                var scale = Math.Max((drawingCoordinates.Width - (Padding * 2)) / Bitmap.Width, (drawingCoordinates.Height - (Padding * 2)) / Bitmap.Height);
        //                var tmpw = (drawingCoordinates.Width - (Padding * 2)) / scale;
        //                var tmph = (drawingCoordinates.Height - (Padding * 2)) / scale;
        //                r3 = new RectangleF((Bitmap.Width - tmpw) / 2, (Bitmap.Height - tmph) / 2, tmpw, tmph);
        //                r2 = r1;
        //                break;
        //            }
        //        default: // Is = enSizeModes.WeißerRand
        //            {
        //                var scale = Math.Min((drawingCoordinates.Width - (Padding * 2)) / Bitmap.Width, (drawingCoordinates.Height - (Padding * 2)) / Bitmap.Height);
        //                r2 = new RectangleF(((drawingCoordinates.Width - (Bitmap.Width * scale)) / 2) + drawingCoordinates.Left, ((drawingCoordinates.Height - (Bitmap.Height * scale)) / 2) + drawingCoordinates.Top, Bitmap.Width * scale, Bitmap.Height * scale);
        //                break;
        //            }
        //    }
        //}
        //var trp = drawingCoordinates.PointOf(enAlignment.Horizontal_Vertical_Center);
        //gr.TranslateTransform(trp.X, trp.Y);
        //gr.RotateTransform(-Drehwinkel);
        //r1 = new RectangleF(r1.Left - trp.X, r1.Top - trp.Y, r1.Width, r1.Height);
        //r2 = new RectangleF(r2.Left - trp.X, r2.Top - trp.Y, r2.Width, r2.Height);
        //if (Hintergrund_Weiß_Füllen) {
        //    gr.FillRectangle(Brushes.White, r1);
        //}
        //try {
        //    if (Bitmap != null) {
        //        if (forPrinting) {
        //            gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
        //            gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
        //        } else {
        //            gr.InterpolationMode = InterpolationMode.Low;
        //            gr.PixelOffsetMode = PixelOffsetMode.HighSpeed;
        //        }
        //        gr.DrawImage(Bitmap, r2, r3, GraphicsUnit.Pixel);
        //    }
        //} catch {
        //    Generic.CollectGarbage();
        //}
        //if (Stil != PadStyles.Undefiniert) {
        //    if (Parent.SheetStyleScale > 0 && Parent.SheetStyle != null) {
        //        gr.DrawRectangle(Skin.GetBlueFont(Stil, Parent.SheetStyle).Pen(zoom * Parent.SheetStyleScale), r1);
        //    }
        //}
        //foreach (var thisQi in Overlays) {
        //    gr.DrawImage(thisQi, r2.Left + 8, r2.Top + 8);
        //}
        //gr.TranslateTransform(-trp.X, -trp.Y);
        //gr.ResetTransform();
        //if (!forPrinting) {
        //    if (!string.IsNullOrEmpty(Platzhalter_Für_Layout)) {
        //        Font f = new("Arial", 8);
        //        BlueFont.DrawString(gr, Platzhalter_Für_Layout, f, Brushes.Black, drawingCoordinates.Left, drawingCoordinates.Top);
        //    }
        //}
        base.DrawExplicit(gr, modifiedPosition, zoom, shiftX, shiftY, forPrinting);
    }

    protected override BasicPadItem? TryCreate(string id, string name) {
        if (id.Equals(ClassId(), StringComparison.OrdinalIgnoreCase)) {
            return new RowWithFilterPaditem(name);
        }
        return null;
    }

    private void Cell_CellValueChanged(object sender, BlueDatabase.EventArgs.CellEventArgs e) {
        RepairConnections();
    }

    private void FilterDatabaseUpdate() {
        if (FilterDefiniton == null) { return; }

        var sc = "if (!IsDropDownItem(suchtxt, suchtxt)) {suchtxt=\"\";}";

        #region Hauptspalte

        var hs = FilterDefiniton.Column["spalte"];
        hs.OpticalReplace.Clear();
        if (Database != null) {
            foreach (var thisc in Database.Column) {
                hs.OpticalReplace.Add(thisc.Key.ToString() + "|" + thisc.ReadableText());
            }
        }

        #endregion

        #region Spalte Suchtext & SuchSym-Script

        var b = FilterDefiniton.Column["suchtxt"];
        b.DropDownItems.Clear();
        b.OpticalReplace.Clear();

        if (Parent != null) {
            foreach (var thisPadItem in Parent) {
                if (thisPadItem is IContentHolder efpi) {
                    var rek = false;
                    if (efpi is IAcceptAndSends aas) { rek = aas.IsRecursiveWith(this); }

                    if (!rek) {
                        b.DropDownItems.Add(efpi.Internal);
                        b.OpticalReplace.Add(efpi.Internal + "|" + efpi.ReadableText());
                        var s = string.Empty;
                        var tmp = efpi.SymbolForReadableText();
                        if (tmp != null) { s = tmp.ToString(); }

                        sc = sc + "if (" + b.Name + "==\"" + efpi.Internal + "\") {suchsym=\"" + s + "\";}";
                    }
                }
            }
        }

        #endregion

        #region Zeilen Prüfen

        foreach (var thisrow in FilterDefiniton.Row) {
            thisrow.DoAutomatic("to be sure");
        }

        #endregion

        FilterDefiniton.RulesScript = sc;
    }

    private void FilterTable_ContextMenuInit(object sender, EventArgs.ContextMenuInitEventArgs e) {
        var bt = (Table)sender;
        var cellKey = e.Tags.TagGet("CellKey");
        if (string.IsNullOrEmpty(cellKey)) { return; }
        RowItem? row = null;
        bt.Database?.Cell.DataOfCellKey(cellKey, out var column, out row);
        if (row == null) { return; }

        e.UserMenu.Add(BlueControls.Enums.ContextMenuComands.Löschen);
    }

    private void Filtertable_ContextMenuItemClicked(object sender, EventArgs.ContextMenuItemClickedEventArgs e) {
        var bt = (Table)sender;
        var cellKey = e.Tags.TagGet("CellKey");
        if (string.IsNullOrEmpty(cellKey)) { return; }
        RowItem? row = null;
        bt.Database?.Cell.DataOfCellKey(cellKey, out var column, out row);
        if (row == null) { return; }

        switch (e.ClickedComand.ToLower()) {
            case "löschen":
                row.Database?.Row.Remove(row);
                break;

            default:
                BlueBasics.Develop.DebugPrint_MissingCommand(e.ClickedComand);
                break;
        }
    }

    private Database GenerateFilterDatabase() {
        Database x = new(false);
        //x.Column.Add("count", "count", VarType.Integer);
        //var vis = x.Column.Add("visible", "visible", VarType.Bit);
        var sp = x.Column.Add("Spalte", "Spalte", VarType.Text);
        sp.Align = AlignmentHorizontal.Rechts;

        var fa = x.Column.Add("FilterArt", "Art", VarType.Text);
        fa.MultiLine = false;
        fa.TextBearbeitungErlaubt = false;
        fa.DropdownAllesAbwählenErlaubt = true;
        fa.DropdownBearbeitungErlaubt = true;
        fa.DropDownItems.Add("=");
        fa.DropDownItems.Add("=!empty");
        //fa.DropDownItems.Add("x");
        fa.OpticalReplace.Add("=|ist (GK egal)");
        fa.OpticalReplace.Add("=!empty|wenn nicht leer, ist");
        //fa.OpticalReplace.Add("x|LÖSCHEN");

        var b1 = x.Column.Add("suchsym", " ", VarType.Text);
        b1.BildTextVerhalten = BildTextVerhalten.Nur_Bild;
        b1.ScriptType = ScriptType.String;

        var b = x.Column.Add("suchtxt", "Suchtext", VarType.Text);
        //            //b.Quickinfo = "<b>Entweder</b> ~Spaltenname~<br><b>oder</b> fester Text zum suchen<br>Mischen wird nicht unterstützt.";
        b.MultiLine = false;
        b.TextBearbeitungErlaubt = false;
        b.DropdownAllesAbwählenErlaubt = true;
        b.DropdownBearbeitungErlaubt = true;
        b.ScriptType = ScriptType.String;

        FilterDatabaseUpdate();

        x.RepairAfterParse(null, null);
        x.ColumnArrangements[1].ShowAllColumns();
        //x.ColumnArrangements[1].Hide("visible");
        x.ColumnArrangements[1].HideSystemColumns();
        x.SortDefinition = new RowSortDefinition(x, "Spalte", false);

        return x;
    }

    private void RepairConnections() {
        ConnectsTo.Clear();

        foreach (var thisRow in FilterDefiniton.Row) {
            var GetValueFrom = Parent[thisRow.CellGetString("suchtxt")];

            if (GetValueFrom != null) {
                ConnectsTo.Add(new ItemConnection(ConnectionType.Top, true, GetValueFrom, ConnectionType.Bottom, false));
            }
        }
    }

    #endregion
}
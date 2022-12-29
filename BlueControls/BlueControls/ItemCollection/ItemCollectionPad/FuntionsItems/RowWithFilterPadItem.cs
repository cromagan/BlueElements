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

using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.ConnectedFormula;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueDatabase;
using BlueDatabase.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using static BlueBasics.Converter;

namespace BlueControls.ItemCollection;

public class RowWithFilterPadItem : RectanglePadItemWithVersion, IReadableText, IAcceptAndSends, ICalculateRowsItemLevel, IItemToControl {

    #region Fields

    public readonly Database FilterDefiniton;

    private string _anzeige = string.Empty;

    private EditTypeFormula _bearbeitung = EditTypeFormula.Textfeld_mit_Auswahlknopf;

    private ÜberschriftAnordnung _überschiftanordung = ÜberschriftAnordnung.Über_dem_Feld;

    private string _überschrift = string.Empty;

    #endregion

    #region Constructors

    public RowWithFilterPadItem(Database? db, int id) : this(string.Empty, db, id) { }

    public RowWithFilterPadItem(string intern, Database? db, int id) : base(intern) {
        Database = db;

        Id = id;

        FilterDefiniton = GenerateFilterDatabase();
        FilterDefiniton.Cell.CellValueChanged += Cell_CellValueChanged;
    }

    public RowWithFilterPadItem(string intern) : this(intern, null, 0) { }

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

    public DatabaseAbstract? Database { get; set; }

    public string Datenbankkopf {
        get => string.Empty;
        set {
            if (Database == null) { return; }
            Forms.TableView.OpenDatabaseHeadEditor(Database);
        }
    }

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

            var r = FilterDefiniton.Row.GenerateAndAdd(t[0]);
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

    protected override int SaveOrder => 1;

    #endregion

    #region Methods

    public Control CreateControl(ConnectedFormulaView parent) {
        var con = new FlexiControlRowSelector(Database, Parent, FilterDefiniton, _überschrift, _anzeige);
        con.EditType = _bearbeitung;
        con.CaptionPosition = CaptionPosition;
        con.Name = DefaultItemToControlName();
        return con;
    }

    public Table FilterTable() {
        var FilterTable = new Table();
        FilterTable.DropMessages = false;
        FilterTable.ShowWaitScreen = true;
        FilterTable.Size = new Size(968, 400);

        FilterTable.DatabaseSet(FilterDefiniton, string.Empty);

        FilterTable.Arrangement = 1;
        FilterTable.ContextMenuInit += FilterTable_ContextMenuInit;
        FilterTable.ContextMenuItemClicked += Filtertable_ContextMenuItemClicked;
        FilterTable.Disposed += FilterTable_Disposed;

        return FilterTable;
    }

    public override List<GenericControl> GetStyleOptions() {
        List<GenericControl> l = new();
        if (Database == null) { return l; }
        l.Add(new FlexiControlForProperty<string>(() => Überschrift));
        l.Add(new FlexiControlForProperty<string>(() => Anzeige));

        var u = new ItemCollection.ItemCollectionList.ItemCollectionList();
        u.AddRange(typeof(ÜberschriftAnordnung));
        l.Add(new FlexiControlForProperty<ÜberschriftAnordnung>(() => CaptionPosition, u));
        l.Add(new FlexiControl());

        l.Add(new FlexiControlForProperty<string>(() => Datenbankkopf, ImageCode.Datenbank));

        FilterDatabaseUpdate();
        l.Add(new FlexiControlForProperty<string>(() => Filter_hinzufügen, ImageCode.PlusZeichen));
        l.Add(FilterTable());

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
                Database = DatabaseAbstract.GetByID(new ConnectionInfo(value.FromNonCritical()));
                return true;

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
        }
        return false;
    }

    public string ReadableText() {
        if (Database != null) {
            return "eine Zeile aus: " + Database.Caption;
        }

        return "Zeile einer Datenbank";
    }

    public QuickImage? SymbolForReadableText() => QuickImage.Get(ImageCode.Kreis, 10, Color.Transparent, Skin.IDColor(Id));

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
            t = t + "Database=" + Database.ConnectionData.UniqueID.ToNonCritical() + ", ";
        }

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
        }
    }

    protected override void DrawExplicit(Graphics gr, RectangleF positionModified, float zoom, float shiftX, float shiftY, bool forPrinting) {
        if (!forPrinting) {
            DrawColorScheme(gr, positionModified, zoom, Id);

            if (Database != null) {
                var txt = "eine Zeile aus " + Database.Caption;

                Skin.Draw_FormatedText(gr, txt, QuickImage.Get(ImageCode.Zeile, (int)(zoom * 16)), Alignment.Horizontal_Vertical_Center, positionModified.ToRect(), ColumnFont.Scale(zoom), false);
            } else {
                Skin.Draw_FormatedText(gr, "Bezug fehlt", QuickImage.Get(ImageCode.Zeile, (int)(zoom * 16)), Alignment.Horizontal_Vertical_Center, positionModified.ToRect(), ColumnFont.Scale(zoom), false);
            }
        } else {
            CustomizableShowPadItem.DrawFakeControl(gr, positionModified, zoom, CaptionPosition, _überschrift);
        }

        base.DrawExplicit(gr, positionModified, zoom, shiftX, shiftY, forPrinting);
    }

    protected override BasicPadItem? TryCreate(string id, string name) {
        if (id.Equals(ClassId(), StringComparison.OrdinalIgnoreCase)) {
            return new RowWithFilterPadItem(name);
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

        var or2 = new List<string>();
        if (Database != null) {
            foreach (var thisc in Database.Column) {
                or2.Add(thisc.Key.ToString() + "|" + thisc.ReadableText());
            }
        }

        hs.OpticalReplace = or2;

        #endregion

        #region Spalte Suchtext & SuchSym-Script

        var b = FilterDefiniton.Column["suchtxt"];

        var dd = new List<string>();
        var or = new List<string>();

        if (Parent != null) {
            foreach (var thisPadItem in Parent) {
                if (thisPadItem.IsVisibleOnPage(Page) && thisPadItem is IContentHolder efpi) {
                    var rek = false;
                    if (efpi is IAcceptAndSends aas) { rek = aas.IsRecursiveWith(this); }

                    if (!rek) {
                        dd.Add(efpi.Internal);
                        or.Add(efpi.Internal + "|" + efpi.ReadableText());
                        var s = string.Empty;
                        var tmp = efpi.SymbolForReadableText();
                        if (tmp != null) { s = tmp.ToString(); }

                        sc = sc + "if (" + b.Name + "==\"" + efpi.Internal + "\") {suchsym=\"" + s + "\";}";
                    }
                }
            }
        }
        b.DropDownItems = dd;
        b.OpticalReplace = or;

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
        bt.Database?.Cell.DataOfCellKey(cellKey, out _, out row);
        if (row == null) { return; }

        e.UserMenu.Add(ContextMenuComands.Löschen);
    }

    private void Filtertable_ContextMenuItemClicked(object sender, EventArgs.ContextMenuItemClickedEventArgs e) {
        var bt = (Table)sender;
        var cellKey = e.Tags.TagGet("CellKey");
        if (string.IsNullOrEmpty(cellKey)) { return; }
        RowItem? row = null;
        bt.Database?.Cell.DataOfCellKey(cellKey, out _, out row);
        if (row == null) { return; }

        switch (e.ClickedComand.ToLower()) {
            case "löschen":
                row.Database?.Row.Remove(row);
                break;

            default:
                Develop.DebugPrint_MissingCommand(e.ClickedComand);
                break;
        }
    }

    private void FilterTable_Disposed(object sender, System.EventArgs e) {
        if (sender is Table t) {
            t.ContextMenuInit -= FilterTable_ContextMenuInit;
            t.ContextMenuItemClicked -= Filtertable_ContextMenuItemClicked;
            t.Disposed += FilterTable_Disposed;
        }
    }

    private Database GenerateFilterDatabase() {
        Database x = new(false, "Filterdatabase_" + Internal);
        var sp = x.Column.GenerateAndAdd("Spalte", "Spalte", VarType.Text);
        sp.Align = AlignmentHorizontal.Rechts;

        var fa = x.Column.GenerateAndAdd("FilterArt", "Art", VarType.Text);
        fa.MultiLine = false;
        fa.TextBearbeitungErlaubt = false;
        fa.DropdownAllesAbwählenErlaubt = true;
        fa.DropdownBearbeitungErlaubt = true;
        fa.DropDownItems = new List<string>() { "=", "=!empty" };
        fa.OpticalReplace = new List<string>() { "=|ist (GK egal)", "=!empty|wenn nicht leer, ist" };
        var b1 = x.Column.GenerateAndAdd("suchsym", " ", VarType.Text);
        b1.BehaviorOfImageAndText = BildTextVerhalten.Nur_Bild;
        b1.ScriptType = ScriptType.String;

        var b = x.Column.GenerateAndAdd("suchtxt", "Suchtext", VarType.Text);
        b.MultiLine = false;
        b.TextBearbeitungErlaubt = false;
        b.DropdownAllesAbwählenErlaubt = true;
        b.DropdownBearbeitungErlaubt = true;
        b.ScriptType = ScriptType.String;

        FilterDatabaseUpdate();

        x.RepairAfterParse();

        var car = x.ColumnArrangements.CloneWithClones();
        car[1].ShowAllColumns();
        car[1].HideSystemColumns();
        x.ColumnArrangements = car;

        x.SortDefinition = new RowSortDefinition(x, "Spalte", false);

        return x;
    }

    private void RepairConnections() {
        ConnectsTo.Clear();

        foreach (var thisRow in FilterDefiniton.Row) {
            var GetValueFrom = Parent[thisRow.CellGetString("suchtxt")];

            if (GetValueFrom != null) {
                ConnectsTo.Add(new ItemConnection(ConnectionType.Top, true, GetValueFrom, ConnectionType.Bottom, false, false));
            }
        }
    }

    #endregion
}
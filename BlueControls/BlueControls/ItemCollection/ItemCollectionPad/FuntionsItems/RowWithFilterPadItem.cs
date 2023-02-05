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
using BlueDatabase.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using static BlueBasics.Converter;

namespace BlueControls.ItemCollection;

public class RowWithFilterPadItem : RectanglePadItemWithVersion, IReadableText, IAcceptAndSends, ICalculateRowsItemLevel, IItemToControl {

    #region Fields

    public readonly DatabaseAbstract FilterDefiniton;

    private string _anzeige = string.Empty;

    private EditTypeFormula _bearbeitung = EditTypeFormula.Textfeld_mit_Auswahlknopf;

    private ÜberschriftAnordnung _überschiftanordung = ÜberschriftAnordnung.Über_dem_Feld;

    private string _überschrift = string.Empty;

    #endregion

    #region Constructors

    public RowWithFilterPadItem(DatabaseAbstract? db, int id) : this(string.Empty, db, id) { }

    public RowWithFilterPadItem(string intern, DatabaseAbstract? db, int id) : base(intern) {
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

    public string Datenbank_wählen {
        get => string.Empty;
        set {
            var db = Forms.CommonDialogs.ChooseKnownDatabase();

            if (db == null) { return; }

            if (db == Database) { return; }
            Database = db;

            FilterDatabaseUpdate();

            RepairConnections();
        }
    }

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

            var c = new ItemCollectionList.ItemCollectionList(true);
            foreach (var thiscol in Database.Column) {
                if (thiscol.Format.Autofilter_möglich() && !thiscol.Format.NeedTargetDatabase()) {
                    _ = c.Add(thiscol);
                }
            }

            var t = Forms.InputBoxListBoxStyle.Show("Filter für welche Spalte?", c, AddType.None, true);

            if (t == null || t.Count != 1) { return; }

            var r = FilterDefiniton.Row.GenerateAndAdd(t[0], "Neuer Filter");
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
        var con = new FlexiControlRowSelector(Database, Parent, FilterDefiniton, _überschrift, _anzeige) {
            EditType = _bearbeitung,
            CaptionPosition = CaptionPosition,
            Name = DefaultItemToControlName()
        };
        return con;
    }

    public Table FilterTable() {
        var filterTable = new Table {
            DropMessages = false,
            ShowWaitScreen = true,
            Size = new Size(968, 400)
        };

        filterTable.DatabaseSet(FilterDefiniton, string.Empty);

        filterTable.Arrangement = 1;
        filterTable.ContextMenuInit += FilterTable_ContextMenuInit;
        filterTable.ContextMenuItemClicked += Filtertable_ContextMenuItemClicked;
        filterTable.Disposed += FilterTable_Disposed;

        return filterTable;
    }

    public override List<GenericControl> GetStyleOptions() {
        List<GenericControl> l = new() {
            new FlexiControlForProperty<string>(() => Datenbank_wählen, ImageCode.Datenbank),
            new FlexiControl()
        };
        if (Database == null) { return l; }
        l.Add(new FlexiControlForProperty<string>(() => Überschrift));
        l.Add(new FlexiControlForProperty<string>(() => Anzeige));

        var u = new ItemCollection.ItemCollectionList.ItemCollectionList(false);
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

                var na = value.FromNonCritical();

                if (na.IsFormat(FormatHolder.FilepathAndName)) {
                    na = na.FilePath() + SQLBackAbstract.MakeValidTableName(na.FileNameWithoutSuffix()) + "." + na.FileSuffix();
                }

                Database = DatabaseAbstract.GetById(new ConnectionInfo(na, null), null);
                return true;

            case "id":
                Id = IntParse(value);
                return true;

            case "filterdb":
                _ = FilterDefiniton.Row.Clear("Neue Filter-Datenbank per Parse");
                FilterDatabaseUpdate();
                _ = FilterDefiniton.Import(value.FromNonCritical(), true, false, ";", false, false, false, string.Empty);

                foreach (var thisRow in FilterDefiniton.Row) {
                    var n = thisRow.CellGetString("Spalte");
                    if (n.IsLong()) {
                        var k = LongParse(n);
                        var c = Database?.Column.SearchByKey(k);
                        if (c != null) {
                            thisRow.CellSet("Spalte", c.Name);
                        }
                    }
                }

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
        if (Database != null && !Database.IsDisposed) {
            return "eine Zeile aus: " + Database.Caption;
        }

        return "Zeile einer Datenbank";
    }

    public QuickImage? SymbolForReadableText() => QuickImage.Get(ImageCode.Kreis, 10, Color.Transparent, Skin.IDColor(Id));

    public override string ToString() {
        var result = new List<string>();
        result.ParseableAdd("CaptionText", _überschrift);
        result.ParseableAdd("ShowFormat", _anzeige);
        result.ParseableAdd("EditType", _bearbeitung);
        result.ParseableAdd("Caption", _überschiftanordung);
        result.ParseableAdd("ID", Id);
        result.ParseableAdd("Database", Database);
        result.ParseableAdd("FilterDB", FilterDefiniton.Export_CSV(FirstRow.ColumnInternalName, null as List<ColumnItem>, null));
        return result.Parseable(base.ToString());
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

            if (Database != null && !Database.IsDisposed) {
                var txt = "eine Zeile aus " + Database.Caption;

                Skin.Draw_FormatedText(gr, txt, QuickImage.Get(ImageCode.Zeile, (int)(zoom * 16)), Alignment.Horizontal_Vertical_Center, positionModified.ToRect(), ColumnFont?.Scale(zoom), false);
            } else {
                Skin.Draw_FormatedText(gr, "Bezug fehlt", QuickImage.Get(ImageCode.Zeile, (int)(zoom * 16)), Alignment.Horizontal_Vertical_Center, positionModified.ToRect(), ColumnFont?.Scale(zoom), false);
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

    private void Cell_CellValueChanged(object sender, BlueDatabase.EventArgs.CellEventArgs e) => RepairConnections();

    private void FilterDatabaseUpdate() {
        if (FilterDefiniton == null) { return; }

        var sc = "if (!IsDropDownItem(suchtxt, suchtxt)) {suchtxt=\"\";}";

        #region Hauptspalte

        var hs = FilterDefiniton.Column["spalte"];

        if (hs != null) {
            if (Database != null && !Database.IsDisposed) {
                var or2 = new List<string>();
                foreach (var thisc in Database.Column) {
                    or2.Add(thisc.Name + "|" + thisc.ReadableText());
                }
                hs.OpticalReplace = new(or2);
                hs.DropDownItems = new System.Collections.ObjectModel.ReadOnlyCollection<string>(Database.Column.ToListOfString());
            } else {
                hs.OpticalReplace = new(Array.Empty<string>());
                hs.DropDownItems = new(Array.Empty<string>());
            }
        }

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
        b.DropDownItems = new(dd);
        b.OpticalReplace = new(or);

        #endregion

        #region Events Mappen

        var eves = FilterDefiniton.EventScript.CloneWithClones();
        var l = new EventScript(FilterDefiniton) {
            NeedRow = true,
            ManualExecutable = false,
            Script = sc,
            Name = "Main",
            Events = Events.value_changed | Events.new_row
        };
        eves.Add(l);
        FilterDefiniton.EventScript = new System.Collections.ObjectModel.ReadOnlyCollection<EventScript>(eves);

        #endregion

        #region Zeilen Prüfen

        foreach (var thisrow in FilterDefiniton.Row) {
            _ = thisrow.DoAutomatic(Events.new_row, false, string.Empty);
        }

        #endregion
    }

    private void FilterTable_ContextMenuInit(object sender, EventArgs.ContextMenuInitEventArgs e) {
        var bt = (Table)sender;
        var cellKey = e.Tags.TagGet("CellKey");
        if (string.IsNullOrEmpty(cellKey)) { return; }
        RowItem? row = null;
        bt.Database?.Cell.DataOfCellKey(cellKey, out _, out row);
        if (row == null) { return; }

        _ = e.UserMenu.Add(ContextMenuComands.Löschen);
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
                _ = (row.Database?.Row.Remove(row, "Benutzer: Filter (und somit Zeile) gelöscht"));
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

    private DatabaseAbstract GenerateFilterDatabase() {
        Database x = new(false, "Filterdatabase_" + Internal);

        var sp = x.Column.GenerateAndAdd("Spalte", "Spalte", ColumnFormatHolder.TextOptions);
        sp.Align = AlignmentHorizontal.Rechts;
        sp.DropdownBearbeitungErlaubt = true;
        sp.DropdownAllesAbwählenErlaubt = false;

        //if (Database != null && !Database.IsDisposed) {
        //    sp.DropDownItems = new System.Collections.ObjectModel.ReadOnlyCollection<string>(Database.Column.ToListOfString());
        //}
        var fa = x.Column.GenerateAndAdd("FilterArt", "Art", ColumnFormatHolder.Text);
        fa.MultiLine = false;
        fa.TextBearbeitungErlaubt = false;
        fa.DropdownAllesAbwählenErlaubt = true;
        fa.DropdownBearbeitungErlaubt = true;
        fa.DropDownItems = new(new List<string>() { "=", "=!empty" });
        fa.OpticalReplace = new(new List<string>() { "=|ist (GK egal)", "=!empty|wenn nicht leer, ist" });

        var b1 = x.Column.GenerateAndAdd("suchsym", " ", ColumnFormatHolder.Text);
        b1.BehaviorOfImageAndText = BildTextVerhalten.Nur_Bild;
        b1.ScriptType = ScriptType.String;

        var b = x.Column.GenerateAndAdd("suchtxt", "Suchtext", ColumnFormatHolder.Text);
        b.MultiLine = false;
        b.TextBearbeitungErlaubt = false;
        b.DropdownAllesAbwählenErlaubt = true;
        b.DropdownBearbeitungErlaubt = true;
        b.ScriptType = ScriptType.String;

        FilterDatabaseUpdate();

        x.RepairAfterParse();

        var car = x.ColumnArrangements.CloneWithClones();
        car[1].ShowColumns("Spalte", "FilterArt", "suchsym", "suchtxt");
        x.ColumnArrangements = new(car);

        x.SortDefinition = new RowSortDefinition(x, "Spalte", false);

        return x;
    }

    private void RepairConnections() {
        ConnectsTo.Clear();

        foreach (var thisRow in FilterDefiniton.Row) {
            var getValueFrom = Parent[thisRow.CellGetString("suchtxt")];

            if (getValueFrom != null) {
                ConnectsTo.Add(new ItemConnection(ConnectionType.Top, true, getValueFrom, ConnectionType.Bottom, false, false));
            }
        }
    }

    #endregion
}
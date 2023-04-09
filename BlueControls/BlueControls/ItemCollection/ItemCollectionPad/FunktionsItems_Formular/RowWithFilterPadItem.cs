﻿// Authors:
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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.ConnectedFormula;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueDatabase;
using BlueDatabase.Enums;
using BlueDatabase.EventArgs;
using static BlueBasics.Converter;

namespace BlueControls.ItemCollection;

/// <summary>
/// Altes Element, Filter und Zeilenauswahl in einem
/// </summary>
[Obsolete]
public class RowWithFilterPadItem : FakeControlPadItem, IReadableText, ICalculateRowsItemLevel, IItemToControl, IItemSendRow {

    #region Fields

    private readonly ItemSendRow _itemSends;
    private string _anzeige = string.Empty;

    private EditTypeFormula _bearbeitung = EditTypeFormula.Textfeld_mit_Auswahlknopf;
    private string _überschrift = string.Empty;

    private ÜberschriftAnordnung _überschriftanordung = ÜberschriftAnordnung.Über_dem_Feld;

    #endregion

    #region Constructors

    public RowWithFilterPadItem(string keyname, string toParse) : this(keyname, null as DatabaseAbstract) => Parse(toParse);

    public RowWithFilterPadItem(DatabaseAbstract? db) : this(string.Empty, db) { }

    public RowWithFilterPadItem(string intern, DatabaseAbstract? db) : base(intern) {
        _itemSends = new();

        OutputDatabase = db;

        FilterDefiniton = GenerateFilterDatabase();
        FilterDefiniton.Cell.CellValueChanged += Cell_CellValueChanged;
        FilterDefiniton.Row.RowRemoved += Row_RowRemoved;
        FilterDefiniton.Row.RowAdded += Row_RowAdded;
    }

    public RowWithFilterPadItem(string intern) : this(intern, null as DatabaseAbstract) { }

    #endregion

    #region Properties

    public static string ClassId => "FI-RowWithFilter";

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
        get => _überschriftanordung;
        set {
            if (_überschriftanordung == value) { return; }
            _überschriftanordung = value;
            OnChanged();
        }
    }

    public ReadOnlyCollection<string>? ChildIds {
        get => _itemSends.ChildIdsGet();
        set => _itemSends.ChildIdsSet(value, this);
    }

    public DatabaseAbstract FilterDefiniton { get; }

    public override int InputColorId {
        get;
        set;
    }

    public int OutputColorId {
        get => _itemSends.OutputColorIdGet();
        set => _itemSends.OutputColorIdSet(value, this);
    }

    public DatabaseAbstract? OutputDatabase {
        get => _itemSends.OutputDatabaseGet();
        set => _itemSends.OutputDatabaseSet(value, this);
    }

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

    public void AddChild(IHasKeyName add) => _itemSends.AddChild(add, this);

    public override Control CreateControl(ConnectedFormulaView parent) {
        var con = new FlexiControlRowSelector(OutputDatabase, FilterDefiniton, _überschrift, _anzeige) {
            EditType = _bearbeitung,
            CaptionPosition = CaptionPosition,
            Name = DefaultItemToControlName()
        };

        con.DoInputSettings(this);
        con.DoOutputSettings(this);

        return con;
    }

    public void Filter_hinzufügen() {
        if (OutputDatabase == null || OutputDatabase.IsDisposed) { return; }

        var c = new ItemCollectionList.ItemCollectionList(true);
        foreach (var thiscol in OutputDatabase.Column) {
            if (thiscol.Format.Autofilter_möglich() && !thiscol.Format.NeedTargetDatabase()) {
                _ = c.Add(thiscol);
            }
        }

        var t = InputBoxListBoxStyle.Show("Filter für welche Spalte?", c, AddType.None, true);

        if (t == null || t.Count != 1) { return; }

        var r = FilterDefiniton.Row.GenerateAndAdd(t[0], "Neuer Filter");
        r.CellSet("FilterArt", "=");
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
        List<GenericControl> l = new();

        l.AddRange(_itemSends.GetStyleOptions(this));

        l.Add(new FlexiControlForProperty<string>(() => Überschrift));
        l.Add(new FlexiControlForProperty<string>(() => Anzeige));

        var u = new ItemCollectionList.ItemCollectionList(false);
        u.AddRange(typeof(ÜberschriftAnordnung));
        l.Add(new FlexiControlForProperty<ÜberschriftAnordnung>(() => CaptionPosition, u));
        l.Add(new FlexiControl());

        FilterDatabaseUpdate();
        l.Add(new FlexiControlForDelegate(Filter_hinzufügen, "Filter hinzufügen", ImageCode.PlusZeichen));
        l.Add(FilterTable());

        l.Add(new FlexiControl());
        l.AddRange(base.GetStyleOptions());

        return l;
    }

    public override bool ParseThis(string tag, string value) {
        if (base.ParseThis(tag, value)) { return true; }
        if (_itemSends.ParseThis(tag, value)) { return true; }

        switch (tag) {
            case "id":
                //Id = IntParse(value);
                return true;

            case "filterdb":
                _ = FilterDefiniton.Row.Clear("Neue Filter-Datenbank per Parse");
                FilterDatabaseUpdate();
                _ = FilterDefiniton.Import(value.FromNonCritical(), true, false, ";", false, false, false);
                var exi = FilterDefiniton.Column.Exists("SuchSym");
                if (exi != null) {
                    _ = FilterDefiniton.Column.Remove("SuchSym", "Veraltete Spalte");
                }
                //foreach (var thisRow in FilterDefiniton.Row) {
                //    var n = thisRow.CellGetString("Spalte");
                //    if (n.IsLong()) {
                //        var k = LongParse(n);
                //        var c = Database?.Column.SearchByKey(k);
                //        if (c != null) {
                //            thisRow.CellSet("Spalte", c.Name);
                //        }
                //    }
                //}

                return true;

            case "edittype":
                _bearbeitung = (EditTypeFormula)IntParse(value);
                return true;

            case "caption":
                _überschriftanordung = (ÜberschriftAnordnung)IntParse(value);
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
        if (OutputDatabase != null && !OutputDatabase.IsDisposed) {
            return "eine Zeile aus: " + OutputDatabase.Caption;
        }

        return "Zeile einer Datenbank";
    }

    public void RemoveChild(IHasKeyName remove) => _itemSends.RemoveChild(remove, this);

    public QuickImage? SymbolForReadableText() => QuickImage.Get(ImageCode.Kreis, 10, Color.Transparent, Skin.IDColor(InputColorId));

    public override string ToString() {
        var result = new List<string>();

        //result.AddRange(_itemAccepts.ParsableTags());
        result.AddRange(_itemSends.ParsableTags());

        result.ParseableAdd("CaptionText", _überschrift);
        result.ParseableAdd("ShowFormat", _anzeige);
        result.ParseableAdd("EditType", _bearbeitung);
        result.ParseableAdd("Caption", _überschriftanordung);
        result.ParseableAdd("FilterDB", FilterDefiniton.Export_CSV(FirstRow.ColumnInternalName, null as List<ColumnItem>, null));
        return result.Parseable(base.ToString());
    }

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);

        if (disposing) {
            FilterDefiniton.Cell.CellValueChanged -= Cell_CellValueChanged;
            FilterDefiniton.Row.RowRemoved -= Row_RowRemoved;
            FilterDefiniton.Row.RowAdded -= Row_RowAdded;
        }
    }

    protected override void DrawExplicit(Graphics gr, RectangleF positionModified, float zoom, float shiftX, float shiftY, bool forPrinting) {
        if (!forPrinting) {
            DrawColorScheme(gr, positionModified, zoom, InputColorId);

            RowEntryPadItem.DrawInputArrow(gr, positionModified, zoom, shiftX, shiftY, forPrinting, "Zeile", -1);

            if (OutputDatabase != null && !OutputDatabase.IsDisposed) {
                var txt = "ALT: eine Zeile aus " + OutputDatabase.Caption;

                Skin.Draw_FormatedText(gr, txt, QuickImage.Get(ImageCode.Zeile, (int)(zoom * 16)), Alignment.Horizontal_Vertical_Center, positionModified.ToRect(), ColumnFont?.Scale(zoom), false);
            } else {
                Skin.Draw_FormatedText(gr, "ALT Bezug fehlt", QuickImage.Get(ImageCode.Zeile, (int)(zoom * 16)), Alignment.Horizontal_Vertical_Center, positionModified.ToRect(), ColumnFont?.Scale(zoom), false);
            }
        } else {
            DrawFakeControl(gr, positionModified, zoom, CaptionPosition, _überschrift);
        }

        base.DrawExplicit(gr, positionModified, zoom, shiftX, shiftY, forPrinting);
        RowEntryPadItem.DrawOutputArrow(gr, positionModified, zoom, shiftX, shiftY, forPrinting, "Zeile", OutputColorId);
    }

    protected override void OnParentChanged() {
        base.OnParentChanged();
        _itemSends.DoParentChanged(this);
        //RepairConnections();
    }

    private void Cell_CellValueChanged(object sender, CellEventArgs e) {
        //RepairConnections();
        _itemSends.DoChilds(this);
        OnChanged();
    }

    //protected override BasicPadItem? TryCreate(string id, string name) {
    //    if (id.Equals(ClassId, StringComparison.OrdinalIgnoreCase)) {
    //        return new RowWithFilterPadItem(name);
    //    }
    //    return null;
    //}
    private void FilterDatabaseUpdate() {
        if (FilterDefiniton == null) { return; }

        //var sc = "if (!IsDropDownItem(suchtxt, suchtxt)) {suchtxt=\"\";}";

        #region Hauptspalte

        var hs = FilterDefiniton.Column["spalte"];

        if (hs != null) {
            if (OutputDatabase != null && !OutputDatabase.IsDisposed) {
                var or2 = new List<string>();
                foreach (var thisc in OutputDatabase.Column) {
                    or2.Add(thisc.Name + "|" + thisc.ReadableText());
                }
                hs.OpticalReplace = new(or2);
                hs.DropDownItems = new ReadOnlyCollection<string>(OutputDatabase.Column.ToListOfString());
            } else {
                hs.OpticalReplace = new(Array.Empty<string>());
                hs.DropDownItems = new(Array.Empty<string>());
            }
        }

        #endregion

        #region Spalte Suchtext & SuchSym-Script

        var b = FilterDefiniton.Column["suchtxt"];

        var dd = new List<string> { "#first" };

        var or = new List<string> { "#first|Erste Spalte der Eingangsdatenbank" };

        if (OutputDatabase != null) {
            foreach (var thisColumn in OutputDatabase.Column) {
                if (thisColumn != null) {
                    dd.Add("~" + thisColumn.Name + "~");
                    or.Add("~" + thisColumn.Name + "~|Spalte: " + thisColumn.ReadableText());
                }
            }
        }
        b.DropDownItems = new(dd);
        b.OpticalReplace = new(or);

        #endregion

        //#region Events Mappen

        //var eves = FilterDefiniton.EventScript.CloneWithClones();
        //var l = new EventScript(FilterDefiniton) {
        //    NeedRow = true,
        //    ManualExecutable = false,
        //    Script = string.Empty ,
        //    Name = "Main",
        //    EventTypes = EventTypes.value_changedx | EventTypes.new_row
        //};
        //eves.Add(l);
        //FilterDefiniton.EventScript = new ReadOnlyCollection<EventScript>(eves);

        //#endregion

        //#region Zeilen Prüfen

        //foreach (var thisrow in FilterDefiniton.Row) {
        //    _ = thisrow.ExecuteScript(EventTypes.new_row, string.Empty, false, false, true, 0);
        //}

        //#endregion
    }

    private void FilterTable_ContextMenuInit(object sender, ContextMenuInitEventArgs e) {
        var bt = (Table)sender;
        var cellKey = e.Tags.TagGet("CellKey");
        if (string.IsNullOrEmpty(cellKey)) { return; }
        RowItem? row = null;
        bt.Database?.Cell.DataOfCellKey(cellKey, out _, out row);
        if (row == null) { return; }

        _ = e.UserMenu.Add(ContextMenuComands.Löschen);
    }

    private void Filtertable_ContextMenuItemClicked(object sender, ContextMenuItemClickedEventArgs e) {
        var bt = (Table)sender;
        var cellKey = e.Tags.TagGet("CellKey");
        if (string.IsNullOrEmpty(cellKey)) { return; }
        RowItem? row = null;
        bt.Database?.Cell.DataOfCellKey(cellKey, out _, out row);
        if (row == null) { return; }

        switch (e.ClickedComand.ToLower()) {
            case "löschen":
                _ = row.Database?.Row.Remove(row, "Benutzer: Filter (und somit Zeile) gelöscht");
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
        Database x = new(false, "Filterdatabase_" + KeyName, DatabaseAbstract.Administrator);
        x.DropMessages = false;

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
        fa.DropDownItems = new(new List<string> { "=", "=!empty", "=word" });
        fa.OpticalReplace = new(new List<string> { "=|ist (GK egal)", "=!empty|wenn nicht leer, ist", "=word|ein Eintrag aus" });

        //var b1 = x.Column.GenerateAndAdd("suchsym", " ", ColumnFormatHolder.Text);
        //b1.BehaviorOfImageAndText = BildTextVerhalten.Nur_Bild;
        //b1.ScriptType = ScriptType.String;

        var b = x.Column.GenerateAndAdd("suchtxt", "Suchtext", ColumnFormatHolder.Text);
        b.MultiLine = false;
        b.TextBearbeitungErlaubt = true;
        b.DropdownAllesAbwählenErlaubt = true;
        b.DropdownBearbeitungErlaubt = true;
        b.ScriptType = ScriptType.String;
        b.Quickinfo = "<b><u>Folgende drei Möglichkeiten:</b></u><br>" +
                      "<i>Da das Formular von einer beliebigen Datenbank aufgerufen werden kann,<br>" +
                      "kann der Filter hier nur über Texteingaben bzw. Variablen gesteuert werden.<br>" +
                      "Deswegen sind manuelle Eingaben nötig.<br>" +
                      "<b>Fester Wert:</b><br>" +
                      "Eine Eingabe, die immer fix ist. Ganz normale Eingabe des Wertes.<br>" +
                      "<b>Erste Spalte:</b><br>" +
                      "Wenn #first eingegeben wird, wird aus der eingebenden Datenbank der<br>" +
                      "Wert der ersten Spalte gewählt.<br>" +
                      "<b>Variablenname:</b><br>" +
                      "Wird ein Wert im Format ~Variable~ eingegeben wird der Wert der Variable<br>" +
                      "der eingebenenden Datenbank gewählt.";

        FilterDatabaseUpdate();

        x.RepairAfterParse();

        var car = x.ColumnArrangements.CloneWithClones();
        car[1].ShowColumns("Spalte", "FilterArt", "suchtxt");
        x.ColumnArrangements = new(car);

        x.SortDefinition = new RowSortDefinition(x, "Spalte", false);

        return x;
    }

    private void Row_RowAdded(object sender, RowEventArgs e) {
        //RepairConnections();
        _itemSends.DoChilds(this);
        OnChanged();
    }

    private void Row_RowRemoved(object sender, System.EventArgs e) {
        //RepairConnections();
        _itemSends.DoChilds(this);
        OnChanged();
    }

    #endregion
}
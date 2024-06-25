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

using BlueBasics;
using BlueBasics.Interfaces;
using BlueControls.Controls;
using BlueControls.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using BlueDatabase;
using static BlueBasics.Converter;
using BlueDatabase.Enums;

namespace BlueControls.ItemCollectionPad.FunktionsItems_Formular;

public class RowAdderSingleRow : IParseable, IReadableTextWithKey, IErrorCheckable, IHasKeyName, IReadableText, IEditable, ISimpleEditor {

    #region Fields

    public List<RowAdderSingleCell> Columns = new();

    private bool _filling = false;

    /// <summary>
    /// Die Herkunft-Id, die mit Variablen der erzeugt wird.
    /// Diese Id muss für jede Zeile der eingehenden Datenbank einmalig sein.
    /// Die Struktur muss wie ein Dateipfad aufgebaut sein. z.B. Kochen\\Zutaten\\Vegetarisch\\Mehl
    /// </summary>
    private string _textKey = string.Empty;

    private Database? _tmpEditDB = null;

    #endregion

    #region Constructors

    public RowAdderSingleRow(RowAdderSingle parent, string toParse) : this(parent, 0) => this.Parse(toParse);

    public RowAdderSingleRow(RowAdderSingle parent, int count) : base() {
        KeyName = Generic.GetUniqueKey();
        Parent = parent;

        if (count >= 0) {
            Count = count;
        }
    }

    #endregion

    #region Properties

    public string CaptionForEditor => "Import Element One Row";
    public int Count { get; private set; } = 0;

    /// <summary>
    /// Datenbank, aus der die Zeile generiert wird
    /// </summary>
    public Database? Database {
        get {
            return Parent?.Database;
        }
    }

    /// <summary>
    /// Datenbank, mit der Ursprünglich gefilter wird und eine einzigartige ID enthält
    /// </summary>
    public Database? DatabaseOfUniqeRow {
        get {
            return Parent?.Parent?.DatabaseInput;
        }
    }



    /// <summary>
    /// Datenbank, in der die Werte gespeichert werden
    /// </summary>
    public Database? DatabaseWriteTo {
        get {
            return Parent?.Parent?.DatabaseOutput;
        }
    }



    public string Description => "Ein Element, das beschreibt, wie die Daten zusammengetragen werden.";
    public Type? Editor { get; set; }
    public string KeyName { get; private set; } = string.Empty;
    public RowAdderSingle? Parent { get; private set; } = null;
    public string QuickInfo => ReadableText();

    /// <summary>
    /// Die Herkunft-Id, die mit Variablen der erzeugt wird.
    /// Diese Id muss für jede Zeile der eingehenden Datenbank einmalig sein.
    /// Die Struktur muss wie ein Dateipfad aufgebaut sein. z.B. Kochen\\Zutaten\\Vegetarisch\\Mehl
    /// </summary>
    [Description("Die Herkunft-Id, die mit Variablen der erzeugt wird.\r\nDiese Id muss für jede Zeile der eingehenden Datenbank einmalig sein.\r\nDie Struktur muss wie ein Dateipfad aufgebaut sein. z.B. Kochen\\Zutaten\\Vegetarisch\\Mehl")]
    public string TextKey {
        get => _textKey;
        set {
            //if (IsDisposed) { return; }
            if (_textKey == value) { return; }
            _textKey = value;
            //OnPropertyChanged();
        }
    }

    #endregion

    #region Methods

    public string ErrorReason() {
        if (Database is not Database db || db.IsDisposed) { return "Datenbank fehlt."; }

        if (string.IsNullOrEmpty(_textKey)) { return "TextKey-Id-Generierung fehlt"; }
        //if (!_textKey.Contains("~")) { return "TextKey-ID-Generierung muss mit Variablen definiert werden."; }

        return string.Empty;
    }

    public List<GenericControl> GetProperties(int widthOfControl) {
        var result = new List<GenericControl>();
        result.Add(new FlexiControlForProperty<Database?>(() => Database, ItemSendFilter.AllAvailableTables()));

        if (Database != null && !Database.IsDisposed) {
            result.Add(new FlexiControlForProperty<string>(() => TextKey, 1));

            var t = TextTable();

            if (t != null) { result.Add(t); }
        }

        return result;
    }

    public void ParseFinished(string parsed) { }

    public bool ParseThis(string key, string value) {
        switch (key) {
            case "textkey":
                _textKey = value.FromNonCritical();
                return true;

            case "additionaltext":
                return true;

            case "columns":
                foreach (var pair2 in value.GetAllTags()) {
                    Columns.Add(new RowAdderSingleCell(this, pair2.Value.FromNonCritical()));
                }

                return true;

            case "count":
                Count = IntParse(value);
                return true;
        }
        return false;
    }

    public string ReadableText() {
        var b = ErrorReason();
        if (!string.IsNullOrEmpty(b) || Database == null) { return b; }
        return _textKey;
    }

    public QuickImage? SymbolForReadableText() => null;

    public override string ToString() {
        List<string> result = [];

        result.ParseableAdd("TextKey", _textKey);
        result.ParseableAdd("Columns", "Item", Columns);
        result.ParseableAdd("Count", Count);

        return result.Parseable();
    }

    private void _tmpEditTable_CellValueChanged(object sender, BlueDatabase.EventArgs.CellChangedEventArgs e) {
        if (_filling) { return; }

        if (e.Row.Database is not Database db || db.IsDisposed) { return; }

        Columns.Clear();

        foreach (var thisRow in db.Row) {
            var c = new RowAdderSingleCell(this);

            c.Column = thisRow.CellGetString("SpalteName");
            c.ReplaceableText = thisRow.CellGetString("Text");
            Columns.Add(c);
        }
    }



    private void Fill() {
        if (DatabaseWriteTo is not Database db || db.IsDisposed) { return; }

        if (_tmpEditDB is not Database db2 || db2.IsDisposed) { return; }

        _filling = true;

        foreach (var thisr in db2.Row) {
            thisr.CellSet("Text", string.Empty, string.Empty);
        }

        if (db2.Column["SpalteName"] is not ColumnItem c) { _filling = false; return; }
        if (db2.Row.Count== 0) {


            foreach (var thisc in db.Column) {

                if (thisc.Function.CanBeChangedByRules() && !thisc.IsSystemColumn()) {
                    var r = db2.Row.GenerateAndAdd(thisc.KeyName, null, string.Empty);

                    if (r != null) {
                        r.CellSet("Spalte", thisc.ReadableText(), string.Empty);
                    }
                }

            }
        }

        foreach (var thisc in Columns) {
            var r = db2.Row[new FilterItem(c, FilterType.Istgleich_GroßKleinEgal, thisc.Column)];

            if (r != null) {
                r.CellSet("Text", thisc.ReplaceableText, string.Empty);
            }
        }

        _filling = false;
    }

    private Table? TextTable() {
        if (Database is not Database db || db.IsDisposed) { return null; }

        if (_tmpEditDB == null) {
            _tmpEditDB = new(Database.UniqueKeyValue());
            _tmpEditDB.LogUndo = false;
            _ = _tmpEditDB.Column.GenerateAndAdd("SpalteName", "Spalte-Name", ColumnFormatHolder.Text);

            //var vis = db.Column.GenerateAndAdd("visible", "visible", ColumnFormatHolder.Bit);
            //if (vis == null || vis.IsDisposed) { return; }

            var sp = _tmpEditDB.Column.GenerateAndAdd("Spalte", "Spalte", ColumnFormatHolder.SystemName);
            if (sp == null || sp.IsDisposed) { return null; }
            //sp.Align = AlignmentHorizontal.Rechts;

            var b = _tmpEditDB.Column.GenerateAndAdd("Text", "Text", ColumnFormatHolder.Text);
            if (b == null || b.IsDisposed) { return null; }
            b.QuickInfo = "~Spaltenname~ und/oder fester Text";
            b.MultiLine = false;
            b.TextBearbeitungErlaubt = true;
            b.DropdownAllesAbwählenErlaubt = true;
            b.DropdownBearbeitungErlaubt = true;

            var dd = b.DropDownItems.Clone();
            var or = b.OpticalReplace.Clone();

            foreach (var thisColumn in db.Column) {
                if (thisColumn.Function.CanBeCheckedByRules() && !thisColumn.MultiLine) {
                    dd.Add("~" + thisColumn.KeyName.ToLowerInvariant() + "~");
                    or.Add("~" + thisColumn.KeyName.ToLowerInvariant() + "~|[Spalte: " + thisColumn.ReadableText() + "]");
                }
            }

            if (DatabaseOfUniqeRow is Database db2) {
                foreach (var thisColumn in db2.Column) {
                    if (thisColumn.Function.CanBeCheckedByRules() && !thisColumn.MultiLine) {
                        dd.Add("~UNI_" + thisColumn.KeyName.ToLowerInvariant() + "~");
                        or.Add("~UNI_" + thisColumn.KeyName.ToLowerInvariant() + "~|[Herkunft-Zeile, Spalte: " + thisColumn.ReadableText() + "]");
                    }
                }
            }

            b.DropDownItems = dd.AsReadOnly();
            b.OpticalReplace = or.AsReadOnly();

            _tmpEditDB.RepairAfterParse();
            var car = _tmpEditDB.ColumnArrangements.CloneWithClones();

            car[1].Add(sp, false);
            car[1].Add(b, false);

            _tmpEditDB.ColumnArrangements = car.AsReadOnly();

            _tmpEditDB.SortDefinition = new RowSortDefinition(_tmpEditDB, sp, false);
        }

        var _tmpEditTable = new Table(); // Immer neu erstellen, kann nur in einem Container sein

        _tmpEditTable = new Table();
        _tmpEditTable.CellValueChanged += _tmpEditTable_CellValueChanged;
        _tmpEditTable.DatabaseSet(_tmpEditDB, string.Empty);

        _tmpEditTable.Size = new System.Drawing.Size(500, 500);
        _tmpEditTable.Visible = true;
        _tmpEditTable.Enabled = true;

        _tmpEditDB.RepairAfterParse(); // Dass ja die 0 Ansicht stimmt

        Fill();

        return _tmpEditTable;
    }

    #endregion
}
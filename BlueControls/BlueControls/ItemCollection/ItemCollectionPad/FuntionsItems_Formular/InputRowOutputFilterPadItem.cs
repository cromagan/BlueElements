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
using BlueDatabase.Interfaces;
using static BlueBasics.Converter;
using static BlueControls.Interfaces.IItemSendSomethingExtensions;
using static BlueControls.Interfaces.IItemAcceptRowExtension;

namespace BlueControls.ItemCollection;

/// <summary>
/// Dieses Element kann eine Zeile empfangen, und einen Filter für eine andere Datenbank basteln und diesen abgeben.
/// Unsichtbares Element, wird nicht angezeigt
/// </summary>

public class InputRowOutputFilterPadItem : AcceptSomethingPadItem, IReadableText, IItemToControl, IItemAcceptRow, IItemSendFilter {

    #region Fields

    private string? _getValueFromkey;

    private IItemSendRow? _tmpgetValueFrom;

    #endregion

    #region Constructors

    public InputRowOutputFilterPadItem(string keyname, string toParse) : this(keyname, null, 0) => Parse(toParse);

    public InputRowOutputFilterPadItem(DatabaseAbstract? db, int id) : this(string.Empty, db, id) { }

    public InputRowOutputFilterPadItem(string intern, DatabaseAbstract? db, int id) : base(intern) {
        OutputDatabase = db;

        ColorId = id;
    }

    public InputRowOutputFilterPadItem(string intern) : this(intern, null, 0) { }

    #endregion

    #region Properties

    public static string ClassId => "FI-UserSelectionFilter";

    /// <summary>
    /// Laufende Nummer, bestimmt die Einfärbung
    /// </summary>
    public int ColorId { get; set; }

    public string Datenbank_wählen {
        get => string.Empty;
        set {
            var db = CommonDialogs.ChooseKnownDatabase();

            if (db == null) { return; }

            if (db == OutputDatabase) { return; }
            OutputDatabase = db;

            this.DoChilds();

            //RepairConnections();
        }
    }

    public string Datenbankkopf {
        get => string.Empty;
        set {
            if (OutputDatabase == null || OutputDatabase.IsDisposed) { return; }
            TableView.OpenDatabaseHeadEditor(OutputDatabase);
        }
    }

    [Description("Wählt ein Zeilen-Objekt, aus der die Werte kommen.")]
    public string Datenquelle_wählen {
        get => string.Empty;
        set => FakeControlPadItem.Datenquelle_wählen_Zeile(this);
    }

    public string Filter_hinzufügen {
        get => string.Empty;
        set {
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
    }

    public DatabaseAbstract FilterDefiniton { get; }

    public IItemSendRow? GetRowFrom {
        get {
            if (Parent == null || _getValueFromkey == null) { return null; }

            _tmpgetValueFrom ??= Parent[_getValueFromkey] as IItemSendRow;

            return _tmpgetValueFrom;
        }
        set {
            var f = GetRowFrom;
            this.ChangeGetRowFrom(ref f, value);

            _tmpgetValueFrom = f;
            _getValueFromkey = null;

            _getValueFromkey = f?.KeyName ?? string.Empty;
        }
    }

    public DatabaseAbstract? OutputDatabase { get; set; }

    protected override int SaveOrder => 1;

    #endregion

    #region Methods

    public new Control CreateControl(ConnectedFormulaView parent) {
        //var con = new FlexiControlRowSelector(Database, FilterDefiniton, _überschrift, _anzeige) {
        //    EditType = _bearbeitung,
        //    CaptionPosition = CaptionPosition,
        //    Name = DefaultItemToControlName()
        //};
        //return con;
        Develop.DebugPrint_NichtImplementiert();
        return new Control();
    }

    public override List<GenericControl> GetStyleOptions() {
        var l = base.GetStyleOptions();

        var u = new ItemCollectionList.ItemCollectionList(false);
        u.AddRange(typeof(ÜberschriftAnordnung));

        l.Add(new FlexiControl());

        l.Add(new FlexiControlForProperty<string>(() => Datenbankkopf, ImageCode.Datenbank));

        return l;
    }

    public override bool ParseThis(string tag, string value) {
        if (base.ParseThis(tag, value)) { return true; }
        switch (tag) {
            case "outputdatabase":
            case "database":

                var na = value.FromNonCritical();

                if (na.IsFormat(FormatHolder.FilepathAndName)) {
                    na = na.FilePath() + SqlBackAbstract.MakeValidTableName(na.FileNameWithoutSuffix()) + "." + na.FileSuffix();
                }

                OutputDatabase = DatabaseAbstract.GetById(new ConnectionInfo(na, null), null, string.Empty);
                return true;

            case "id":
                ColorId = IntParse(value);
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

    public QuickImage? SymbolForReadableText() => QuickImage.Get(ImageCode.Kreis, 10, Color.Transparent, Skin.IDColor(ColorId));

    public override string ToString() {
        var result = new List<string>();
        result.ParseableAdd("ID", ColorId);
        result.ParseableAdd("OutputDatabase", OutputDatabase);
        result.ParseableAdd("FilterDB", FilterDefiniton.Export_CSV(FirstRow.ColumnInternalName, null as List<ColumnItem>, null));
        return result.Parseable(base.ToString());
    }

    protected override void DrawExplicit(Graphics gr, RectangleF positionModified, float zoom, float shiftX, float shiftY, bool forPrinting) {
        if (!forPrinting) {
            DrawColorScheme(gr, positionModified, zoom, ColorId);
            RowEntryPadItem.DrawInputArrow(gr, positionModified, zoom, shiftX, shiftY, forPrinting, "Zeile", ColorId);
            RowEntryPadItem.DrawOutputArrow(gr, positionModified, zoom, shiftX, shiftY, forPrinting, "Filter", ColorId);

            if (OutputDatabase != null && !OutputDatabase.IsDisposed) {
                var txt = "eine Zeile aus " + OutputDatabase.Caption;

                Skin.Draw_FormatedText(gr, txt, QuickImage.Get(ImageCode.Zeile, (int)(zoom * 16)), Alignment.Horizontal_Vertical_Center, positionModified.ToRect(), ColumnFont?.Scale(zoom), false);
            } else {
                Skin.Draw_FormatedText(gr, "Bezug fehlt", QuickImage.Get(ImageCode.Zeile, (int)(zoom * 16)), Alignment.Horizontal_Vertical_Center, positionModified.ToRect(), ColumnFont?.Scale(zoom), false);
            }
        }

        base.DrawExplicit(gr, positionModified, zoom, shiftX, shiftY, forPrinting);
    }

    protected override void OnParentChanged() {
        base.OnParentChanged();
        this.DoParentChanged();
        //RepairConnections();
    }

    #endregion
}
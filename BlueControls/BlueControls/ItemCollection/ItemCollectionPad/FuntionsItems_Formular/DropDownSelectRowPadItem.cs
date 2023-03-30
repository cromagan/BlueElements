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
using BlueDatabase.AdditionalScriptComands;
using BlueDatabase.Enums;
using BlueDatabase.EventArgs;
using BlueDatabase.Interfaces;
using static BlueBasics.Converter;

namespace BlueControls.ItemCollection;

/// <summary>
/// Dieses Element kann Filter empfangen, und gibt dem Nutzer die Möglichkeit, aus dem daraus resultierenden Zeilen EINE zu wählen.
/// Per Dropwdown menü
/// </summary>

public class DropDownSelectRowPadItem : FakeControlPadItem, IReadableText, IItemToControl, IItemAcceptFilter, IItemSendRow {

    #region Fields

    private string _anzeige = string.Empty;

    private EditTypeFormula _bearbeitung = EditTypeFormula.Textfeld_mit_Auswahlknopf;

    private ItemAcceptFilter _iaf;

    private ItemSendRow _isr;

    private string _überschrift = string.Empty;

    private ÜberschriftAnordnung _überschriftanordung = ÜberschriftAnordnung.Über_dem_Feld;

    #endregion

    #region Constructors

    public DropDownSelectRowPadItem(string keyname, string toParse) : this(keyname, null, 0) => Parse(toParse);

    public DropDownSelectRowPadItem(DatabaseAbstract? db, int id) : this(string.Empty, db, id) { }

    public DropDownSelectRowPadItem(string intern, DatabaseAbstract? db, int id) : base(intern) {
        OutputDatabase = db;

        ColorId = id;
    }

    public DropDownSelectRowPadItem(string intern) : this(intern, null, 0) { }

    #endregion

    #region Properties

    public static string ClassId => "FI-SelectRowWithDropDownMenu";

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
        get => _isr.ChildIdsGet();
        set => _isr.ChildIdsSet(value, this);
    }

    /// <summary>
    /// Laufende Nummer, bestimmt die Einfärbung
    /// </summary>
    public int ColorId { get; set; }

    public string Datenbank_wählen {
        get => string.Empty;
        set => _isr.Datenbank_wählen(this);
    }

    [Description("Wählt ein Filter-Objekt, aus der die Werte kommen.")]
    public string Datenquelle_hinzufügen {
        get => string.Empty;
        set => _iaf.Datenquelle_hinzufügen(this);
    }

    public ReadOnlyCollection<IItemSendFilter>? GetFilterFrom {
        get => _iaf.GetFilterFromGet();
        set => _iaf.GetFilterFromSet(value, this);
    }

    public override int InputColorId {
        get => _iaf.InputColorIdGet();
        set => _iaf.InputColorIdSet(value, this);
    }

    public DatabaseAbstract? OutputDatabase {
        get => _isr.OutputDatabaseGet();
        set => _isr.OutputDatabaseSet(value, this);
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

    public void AddChild(IHasKeyName add) => _isr.AddChild(add, this);

    public override Control CreateControl(ConnectedFormulaView parent) {
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
        List<GenericControl> l = new();
        //    new FlexiControlForProperty<string>(() => Datenbank_wählen, ImageCode.Datenbank),
        //    new FlexiControl()
        //};
        //if (OutputDatabase == null || OutputDatabase.IsDisposed) { return l; }
        //l.Add(new FlexiControlForProperty<string>(() => Überschrift));
        //l.Add(new FlexiControlForProperty<string>(() => Anzeige));

        //var u = new ItemCollectionList.ItemCollectionList(false);
        //u.AddRange(typeof(ÜberschriftAnordnung));
        //l.Add(new FlexiControlForProperty<ÜberschriftAnordnung>(() => CaptionPosition, u));
        //l.Add(new FlexiControl());

        //l.Add(new FlexiControlForProperty<string>(() => Datenbankkopf, ImageCode.Datenbank));

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

    public void RemoveChild(IHasKeyName remove) => _isr.RemoveChild(remove, this);

    public QuickImage? SymbolForReadableText() => QuickImage.Get(ImageCode.Kreis, 10, Color.Transparent, Skin.IDColor(ColorId));

    public override string ToString() {
        var result = new List<string>();
        result.ParseableAdd("CaptionText", _überschrift);
        result.ParseableAdd("ShowFormat", _anzeige);
        result.ParseableAdd("EditType", _bearbeitung);
        result.ParseableAdd("Caption", _überschriftanordung);
        result.ParseableAdd("ID", ColorId);
        result.ParseableAdd("OutputDatabase", OutputDatabase);
        return result.Parseable(base.ToString());
    }

    protected override void DrawExplicit(Graphics gr, RectangleF positionModified, float zoom, float shiftX, float shiftY, bool forPrinting) {
        if (!forPrinting) {
            DrawColorScheme(gr, positionModified, zoom, ColorId);

            RowEntryPadItem.DrawInputArrow(gr, positionModified, zoom, shiftX, shiftY, forPrinting, "Zeile", ColorId);
            RowEntryPadItem.DrawOutputArrow(gr, positionModified, zoom, shiftX, shiftY, forPrinting, "Zeile", ColorId);

            if (OutputDatabase != null && !OutputDatabase.IsDisposed) {
                var txt = "eine Zeile aus " + OutputDatabase.Caption;

                Skin.Draw_FormatedText(gr, txt, QuickImage.Get(ImageCode.Zeile, (int)(zoom * 16)), Alignment.Horizontal_Vertical_Center, positionModified.ToRect(), ColumnFont?.Scale(zoom), false);
            } else {
                Skin.Draw_FormatedText(gr, "Bezug fehlt", QuickImage.Get(ImageCode.Zeile, (int)(zoom * 16)), Alignment.Horizontal_Vertical_Center, positionModified.ToRect(), ColumnFont?.Scale(zoom), false);
            }
        } else {
            FakeControlPadItem.DrawFakeControl(gr, positionModified, zoom, CaptionPosition, _überschrift);
        }

        base.DrawExplicit(gr, positionModified, zoom, shiftX, shiftY, forPrinting);
    }

    protected override void OnParentChanged() {
        base.OnParentChanged();
        _isr.DoParentChanged(this);
        //RepairConnections();
    }

    #endregion
}
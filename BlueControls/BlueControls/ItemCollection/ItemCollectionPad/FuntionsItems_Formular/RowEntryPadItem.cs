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

namespace BlueControls.ItemCollection;

/// <summary>
/// Diese Element ist in jedem Formular vorhanden und empfängt die Zeile aus einem anderen Element
/// Hat NICHT IAcceptRowItem, da es nur von einer einzigen Intneren Routine befüllt werden darf
/// Unsichtbares Element, wird nicht angezeigt
/// </summary>

public class RowEntryPadItem : RectanglePadItemWithVersion, IReadableText, IItemToControl, IItemSendRow {

    #region Constructors

    public RowEntryPadItem(string keyname, string toParse) : this(keyname, null, 0) => Parse(toParse);

    public RowEntryPadItem(DatabaseAbstract? db, int id) : this(string.Empty, db, id) { }

    public RowEntryPadItem(string intern, DatabaseAbstract? db, int id) : base(intern) {
        OutputDatabase = db;

        Id = id;
    }

    public RowEntryPadItem(string intern) : this(intern, null, 0) { }

    #endregion

    #region Properties

    public static string ClassId => "FI-RowEntryElement";

    public string Datenbank_wählen {
        get => string.Empty;
        set {
            var db = CommonDialogs.ChooseKnownDatabase();

            if (db == null) { return; }

            if (db == OutputDatabase) { return; }
            OutputDatabase = db;

            RepairConnections();
        }
    }

    public string Datenbankkopf {
        get => string.Empty;
        set {
            if (OutputDatabase == null || OutputDatabase.IsDisposed) { return; }
            TableView.OpenDatabaseHeadEditor(OutputDatabase);
        }
    }

    /// <summary>
    /// Laufende Nummer, bestimmt die Einfärbung
    /// </summary>
    public int Id { get; set; }

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
        List<GenericControl> l = new() {
            new FlexiControlForProperty<string>(() => Datenbank_wählen, ImageCode.Datenbank),
            new FlexiControl()
        };
        if (OutputDatabase == null || OutputDatabase.IsDisposed) { return l; }

        l.Add(new FlexiControlForProperty<string>(() => Datenbankkopf, ImageCode.Datenbank));
        return l;
    }

    public override bool ParseThis(string tag, string value) {
        if (base.ParseThis(tag, value)) { return true; }
        switch (tag) {
            case "outputdatabase":

                var na = value.FromNonCritical();

                if (na.IsFormat(FormatHolder.FilepathAndName)) {
                    na = na.FilePath() + SqlBackAbstract.MakeValidTableName(na.FileNameWithoutSuffix()) + "." + na.FileSuffix();
                }

                OutputDatabase = DatabaseAbstract.GetById(new ConnectionInfo(na, null), null);
                return true;

            case "id":
                Id = IntParse(value);
                return true;
        }
        return false;
    }

    public string ReadableText() {
        if (OutputDatabase != null && !OutputDatabase.IsDisposed) {
            return "Eingangs-Zeile der Datenbank: " + OutputDatabase.Caption;
        }

        return "Eingangs-Zeile einer Datenbank";
    }

    public QuickImage? SymbolForReadableText() => QuickImage.Get(ImageCode.Kreis, 10, Color.Transparent, Skin.IDColor(Id));

    public override string ToString() {
        var result = new List<string>();
        result.ParseableAdd("ID", Id);
        result.ParseableAdd("OutputDatabase", OutputDatabase);
        return result.Parseable(base.ToString());
    }

    protected override void DrawExplicit(Graphics gr, RectangleF positionModified, float zoom, float shiftX, float shiftY, bool forPrinting) {
        if (!forPrinting) {
            DrawColorScheme(gr, positionModified, zoom, Id);

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
        RepairConnections();
    }

    private void Cell_CellValueChanged(object sender, CellEventArgs e) {
        RepairConnections();
        OnChanged();
    }

    private void RepairConnections() {
        //Develop.DebugPrint_NichtImplementiert();
        //if (Parent == null) { return; }

        //ConnectsTo.RemoveAll();

        //foreach (var thisRow in FilterDefiniton.Row) {
        //    var getValueFrom = Parent[thisRow.CellGetString("suchtxt")];

        //    if (getValueFrom != null) {
        //        ConnectsTo.Add(new ItemConnection(ConnectionType.Top, true, getValueFrom, ConnectionType.Bottom, false, false));
        //    }
        //}
    }

    #endregion
}
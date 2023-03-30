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
/// Dieses Element ist ein Button. Dieser kann Filter empfangen und mit diesen Filtern eine neue Zeile anlegen
/// Per Tabellenansicht
/// </summary>

public class AddRowPaditem : FakeControlPadItem, IReadableText, IItemToControl, IItemAcceptFilter {

    #region Fields

    private string _anzeige = string.Empty;

    private ItemAcceptFilter _iaf;

    #endregion

    #region Constructors

    public AddRowPaditem(string keyname, string toParse) : this(keyname) => Parse(toParse);

    public AddRowPaditem(string intern) : base(intern) { }

    #endregion

    #region Properties

    public static string ClassId => "FI-AddRowButton";

    [Description("Nach welchem Format die Zeilen angezeigt werden sollen. Es können Variablen im Format ~Variable~ benutzt werden. Achtung, KEINE Skript-Variaben, nur Spaltennamen.")]
    public string Anzeige {
        get => _anzeige;
        set {
            if (_anzeige == value) { return; }
            _anzeige = value;
            OnChanged();
        }
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

    protected override int SaveOrder => 1;

    #endregion

    #region Methods

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

        return l;
    }

    public override bool ParseThis(string tag, string value) {
        if (base.ParseThis(tag, value)) { return true; }
        switch (tag) {
            case "showformat":
                _anzeige = value.FromNonCritical();
                return true;
        }
        return false;
    }

    public string ReadableText() {
        var db = _iaf.InputDatabase();

        if (db != null && !db.IsDisposed) {
            return "Neue Zeile anlegen in: " + db.Caption;
        }

        return "Neue Zeile anlegen einer Datenbank";
    }

    public QuickImage? SymbolForReadableText() => null;

    public override string ToString() {
        var result = new List<string>();
        result.ParseableAdd("ShowFormat", _anzeige);
        return result.Parseable(base.ToString());
    }

    protected override void DrawExplicit(Graphics gr, RectangleF positionModified, float zoom, float shiftX, float shiftY, bool forPrinting) {
        if (!forPrinting) {
            var db = _iaf.InputDatabase();

            DrawColorScheme(gr, positionModified, zoom, -1);

            if (db != null && !db.IsDisposed) {
                var txt = "Neue Zeile anlegen Zeile in " + db.Caption;

                Skin.Draw_FormatedText(gr, txt, QuickImage.Get(ImageCode.Zeile, (int)(zoom * 16)), Alignment.Horizontal_Vertical_Center, positionModified.ToRect(), ColumnFont?.Scale(zoom), false);
            } else {
                Skin.Draw_FormatedText(gr, "Bezug fehlt", QuickImage.Get(ImageCode.Zeile, (int)(zoom * 16)), Alignment.Horizontal_Vertical_Center, positionModified.ToRect(), ColumnFont?.Scale(zoom), false);
            }
        }

        base.DrawExplicit(gr, positionModified, zoom, shiftX, shiftY, forPrinting);
    }

    protected override void OnParentChanged() {
        base.OnParentChanged();
        //RepairConnections(this);
    }

    #endregion
}
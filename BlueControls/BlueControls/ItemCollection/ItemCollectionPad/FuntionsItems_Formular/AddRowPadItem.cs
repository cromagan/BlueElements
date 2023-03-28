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

namespace BlueControls.ItemCollection;

/// <summary>
/// Dieses Element ist ein Button. Dieser kann Filter empfangen und mit diesen Filtern eine neue Zeile anlegen
/// Per Tabellenansicht
/// </summary>

public class AddRowPaditem : CustomizableShowPadItem, IReadableText, IItemToControl, IItemAcceptFilter {

    #region Fields

    private string _anzeige = string.Empty;

    #endregion

    #region Constructors

    public AddRowPaditem(string keyname, string toParse) : this(keyname, null, 0) => Parse(toParse);

    public AddRowPaditem(DatabaseAbstract? db, int id) : this(string.Empty, db, id) { }

    public AddRowPaditem(string intern, DatabaseAbstract? db, int id) : base(intern) {
        InputDatabase = db;

        Id = id;
    }

    public AddRowPaditem(string intern) : this(intern, null, 0) { }

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

    public string Datenbank_wählen {
        get => string.Empty;
        set {
            var db = CommonDialogs.ChooseKnownDatabase();

            if (db == null) { return; }

            if (db == InputDatabase) { return; }
            InputDatabase = db;

            //FilterDatabaseUpdate();

            //RepairConnections(this);
        }
    }

    public string Datenbankkopf {
        get => string.Empty;
        set {
            if (InputDatabase == null || InputDatabase.IsDisposed) { return; }
            TableView.OpenDatabaseHeadEditor(InputDatabase);
        }
    }

    /// <summary>
    /// Laufende Nummer, bestimmt die Einfärbung
    /// </summary>
    public int Id { get; set; }

    public DatabaseAbstract? InputDatabase { get; set; }
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
        if (InputDatabase == null || InputDatabase.IsDisposed) { return l; }

        l.Add(new FlexiControlForProperty<string>(() => Anzeige));

        l.Add(new FlexiControlForProperty<string>(() => Datenbankkopf, ImageCode.Datenbank));

        return l;
    }

    public override bool ParseThis(string tag, string value) {
        if (base.ParseThis(tag, value)) { return true; }
        switch (tag) {
            case "inputdatabase":

                var na = value.FromNonCritical();

                if (na.IsFormat(FormatHolder.FilepathAndName)) {
                    na = na.FilePath() + SqlBackAbstract.MakeValidTableName(na.FileNameWithoutSuffix()) + "." + na.FileSuffix();
                }

                InputDatabase = DatabaseAbstract.GetById(new ConnectionInfo(na, null), null, string.Empty);
                return true;

            case "id":
                Id = IntParse(value);
                return true;

            case "showformat":
                _anzeige = value.FromNonCritical();
                return true;
        }
        return false;
    }

    public string ReadableText() {
        if (InputDatabase != null && !InputDatabase.IsDisposed) {
            return "Neue Zeile anlegen in: " + InputDatabase.Caption;
        }

        return "Neue Zeile anlegen einer Datenbank";
    }

    public QuickImage? SymbolForReadableText() => QuickImage.Get(ImageCode.PlusZeichen, 10, Color.Transparent, Skin.IDColor(Id));

    public override string ToString() {
        var result = new List<string>();
        result.ParseableAdd("ShowFormat", _anzeige);
        result.ParseableAdd("ID", Id);
        result.ParseableAdd("InputDatabase", InputDatabase);
        return result.Parseable(base.ToString());
    }

    protected override void DrawExplicit(Graphics gr, RectangleF positionModified, float zoom, float shiftX, float shiftY, bool forPrinting) {
        if (!forPrinting) {
            DrawColorScheme(gr, positionModified, zoom, Id);

            if (InputDatabase != null && !InputDatabase.IsDisposed) {
                var txt = "Neue Zeile anlegen Zeile in " + InputDatabase.Caption;

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
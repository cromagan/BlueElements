// Authors:
// Christian Peter
//
// Copyright (con) 2022 Christian Peter
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
using BlueControls.Interfaces;
using BlueDatabase;
using BlueDatabase.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using static BlueBasics.Converter;

namespace BlueControls.ItemCollection;

public class DatabaseSourcePadItem : RectanglePadItemWithVersion, IReadableText, ICalculateRowsItemLevel, IItemToControl {

    #region Constructors

    public DatabaseSourcePadItem(Database? db, int id) : this(string.Empty, db, id) { }

    public DatabaseSourcePadItem(string intern, Database? db, int id) : base(intern) {
        Database = db;

        Id = id;
    }

    public DatabaseSourcePadItem(string intern) : this(intern, null, 0) { }

    #endregion

    #region Properties

    public DatabaseAbstract? Database { get; set; }

    public string Datenbankkopf {
        get => string.Empty;
        set {
            if (Database == null) { return; }
            Forms.TableView.OpenDatabaseHeadEditor(Database);
        }
    }

    /// <summary>
    /// Laufende Nummer, bestimmt die Einfärbung
    /// </summary>
    public int Id { get; set; }

    protected override int SaveOrder => 1;

    #endregion

    #region Methods

    public Control? CreateControl(ConnectedFormulaView parent) {
        var con = new FlexiControlRowSelector(Database, Parent, null, "Datenbank", String.Empty);
        con.EditType = EditTypeFormula.None;
        con.CaptionPosition = ÜberschriftAnordnung.ohne;
        con.Name = DefaultItemToControlName();
        return con;
    }

    public override List<GenericControl> GetStyleOptions() {
        List<GenericControl> l = new();
        if (Database == null) { return l; }

        l.Add(new FlexiControlForProperty<string>(() => Datenbankkopf, ImageCode.Datenbank));
        return l;
    }

    public override bool ParseThis(string tag, string value) {
        if (base.ParseThis(tag, value)) { return true; }
        switch (tag) {
            case "database":
                Database = DatabaseAbstract.GetByID(value.FromNonCritical(), false, false, null, value.FromNonCritical().FileNameWithoutSuffix());
                return true;

            case "id":
                Id = IntParse(value);
                return true;
        }
        return false;
    }

    public string ReadableText() {
        if (Database != null) {
            return "Datenbank: " + Database.Caption;
        }

        return "Eine Datenbank";
    }

    public QuickImage? SymbolForReadableText() => QuickImage.Get(ImageCode.Kreis, 10, Color.Transparent, Skin.IDColor(Id));

    public override string ToString() {
        var t = base.ToString();
        t = t.Substring(0, t.Length - 1) + ", ";

        t = t + "ID=" + Id.ToString() + ", ";

        if (Database != null) {
            t = t + "Database=" + Database.ConnectionID.ToNonCritical() + ", ";
        }

        return t.Trim(", ") + "}";
    }

    protected override string ClassId() => "FI-DatabaseSource";

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);

        if (disposing) {
        }
    }

    protected override void DrawExplicit(Graphics gr, RectangleF positionModified, float zoom, float shiftX, float shiftY, bool forPrinting) {
        if (!forPrinting) {
            DrawColorScheme(gr, positionModified, zoom, Id);

            if (Database != null) {
                var txt = "Datenbank " + Database.Caption;

                Skin.Draw_FormatedText(gr, txt, QuickImage.Get(ImageCode.Zeile, (int)(zoom * 16)), Alignment.Horizontal_Vertical_Center, positionModified.ToRect(), ColumnPadItem.ColumnFont.Scale(zoom), false);
            } else {
                Skin.Draw_FormatedText(gr, "Bezug fehlt", QuickImage.Get(ImageCode.Zeile, (int)(zoom * 16)), Alignment.Horizontal_Vertical_Center, positionModified.ToRect(), ColumnPadItem.ColumnFont.Scale(zoom), false);
            }
        }

        base.DrawExplicit(gr, positionModified, zoom, shiftX, shiftY, forPrinting);
    }

    protected override BasicPadItem? TryCreate(string id, string name) {
        if (id.Equals(ClassId(), StringComparison.OrdinalIgnoreCase)) {
            return new DatabaseSourcePadItem(name);
        }
        return null;
    }

    #endregion
}
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

using System;
using System.Collections.Generic;
using System.Drawing;
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Forms;
using BlueControls.ItemCollectionPad.Abstract;
using BlueDatabase;
using BlueDatabase.Enums;

namespace BlueControls.ItemCollectionPad.FunktionsItems_ColumnArrangement_Editor;

/// <summary>
/// Zum Darstellen einer Spalte. Im ViewEditpt benutzt
/// </summary>
public class ColumnPadItem : FixedRectangleBitmapPadItem {

    #region Constructors

    public ColumnPadItem(string internalname) : base(internalname) { }

    public ColumnPadItem(ColumnItem c, bool permanent) : base(c.Database?.TableName + "|" + c.KeyName) {
        Column = c;
        Permanent = permanent;

        if (Column != null && !Column.IsDisposed) {
            Column.Changed += Column_Changed;
        }
    }

    #endregion

    #region Properties

    public static string ClassId => "FI-Column";
    public ColumnItem? Column { get; }

    public string Datenbank {
        get {
            if (Column?.Database is not Database db || db.IsDisposed) { return "?"; }
            return db.TableName;
        }
    }

    public override string Description => string.Empty;
    public string Interner_Name => Column == null ? "?" : Column.KeyName;

    /// <summary>
    /// Wird von Flexoptions aufgerufen
    /// </summary>

    //        _viewType = value ? ViewType.PermanentColumn : ViewType.Column;
    //    }
    //}
    public bool Permanent { get; set; }

    protected override int SaveOrder => 999;

    #endregion

    ///// <summary>
    ///// Für FlexOptions
    ///// </summary>
    //public bool Permanent {
    //    get => _viewType == ViewType.PermanentColumn;
    //    set {
    //        if (!PermanentPossible() && Permanent) { return; }
    //        if (!NonPermanentPossible() && !value) { return; }

    #region Methods

    //        if (value == Permanent) { return; }
    public override List<GenericControl> GetStyleOptions(int widthOfControl) {
        List<GenericControl> l = [];
        if (Column == null || Column.IsDisposed) { return l; }

        l.Add(new FlexiControlForProperty<string>(() => Datenbank));
        l.Add(new FlexiControlForProperty<string>(() => Interner_Name));
        l.Add(new FlexiControl());
        l.Add(new FlexiControlForProperty<bool>(() => Permanent));
        l.Add(new FlexiControl());
        l.Add(new FlexiControlForDelegate(Spalte_bearbeiten, "Spalte bearbeiten", ImageCode.Spalte));
        l.Add(new FlexiControl());
        l.Add(new FlexiControlForProperty<string>(() => Column.Caption));
        l.Add(new FlexiControl());
        l.Add(new FlexiControlForProperty<string>(() => Column.CaptionGroup1));
        l.Add(new FlexiControlForProperty<string>(() => Column.CaptionGroup2));
        l.Add(new FlexiControlForProperty<string>(() => Column.CaptionGroup3));
        l.Add(new FlexiControl());
        l.Add(new FlexiControlForProperty<string>(() => Column.Quickinfo, 5));
        l.Add(new FlexiControlForProperty<string>(() => Column.AdminInfo, 5));

        //if (AdditionalStyleOptions != null) {
        //    l.Add(new FlexiControl());
        //    l.AddRange(AdditionalStyleOptions);
        //}

        //ItemCollectionList.ItemCollectionList layouts = new();
        //foreach (var thisLayouts in Row.Database.Layouts) {
        //    ItemCollectionPad p = new(thisLayouts, string.Empty);
        //    layouts.GenerateAndAdd(p.Caption, p.Id, ImageCode.Stern);
        //}
        //l.GenerateAndAdd(new FlexiControlForProperty(()=> this.Layout-ID", layouts));
        //l.AddRange(base.GetStyleOptions(widthOfControl));
        return l;
    }

    public void Spalte_bearbeiten() {
        if (Column == null || Column.IsDisposed) { return; }
        TableView.OpenColumnEditor(Column, null, null);
    }

    public override string ToString() {
        if (IsDisposed) { return string.Empty; }
        List<string> result = [];

        result.ParseableAdd("Database", Column?.Database);
        result.ParseableAdd("ColumnName", Column);

        return result.Parseable(base.ToString());
    }

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);
        if (disposing) {
            if (Column != null && !Column.IsDisposed) {
                Column.Changed -= Column_Changed;
            }
            //Column = null;
        }
    }

    protected override void GeneratePic() {
        if (Column == null || Column.IsDisposed) {
            GeneratedBitmap = QuickImage.Get(ImageCode.Warnung, 128);
            return;
        }

        var wi = Math.Max(Column.Contentwidth ?? 0, 24);

        var bmp = new Bitmap(Math.Max((int)(wi * 0.7), 30), 300);
        var gr = Graphics.FromImage(bmp);

        gr.Clear(Column.BackColor);
        //gr.DrawString(Column.Caption, CellFont, )
        //Table.Draw_FormatedText(gr,)

        for (var z = 0; z < 3; z++) {
            var n = Column.CaptionGroup(z);
            if (!string.IsNullOrEmpty(n)) {
                Skin.Draw_FormatedText(gr, n, null, Alignment.Horizontal_Vertical_Center, new Rectangle(0, z * 16, bmp.Width, 61), null, false, ColumnFont, true);
            }
        }

        gr.TranslateTransform(bmp.Width / 2, 50);
        gr.RotateTransform(-90);
        Skin.Draw_FormatedText(gr, Column.Caption, null, Alignment.VerticalCenter_Left, new Rectangle(-150, -150, 300, 300), null, false, ColumnFont, true);

        gr.TranslateTransform(-bmp.Width / 2, -50);
        gr.ResetTransform();

        gr.DrawLine(Pens.Black, 0, 210, bmp.Width, 210);

        var r = Column.Database?.Row.First();
        if (r != null && !r.IsDisposed) {
            Table.Draw_FormatedText(gr, r.CellGetString(Column), ShortenStyle.Replaced, Column, new Rectangle(0, 210, bmp.Width, 90), Design.Table_Cell, States.Standard, Column.BehaviorOfImageAndText, 1);
        }

        GeneratedBitmap = bmp;
    }

    //protected override AbstractPadItem? TryCreate(string id, string name) {
    //    if (id.Equals(ClassId, StringComparison.OrdinalIgnoreCase)) {
    //        return new ColumnPadItem(name);
    //    }
    //    return null;
    //}

    private void Column_Changed(object sender, System.EventArgs e) {
        if (IsDisposed) { return; }
        RemovePic();
        OnChanged();
    }

    #endregion
}